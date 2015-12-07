using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.Z3950;

namespace dp2Catalog
{
    public partial class BerDebugForm : Form
    {
        public MainForm MainForm = null;

        long CurrentRightStartOffs = -1; // 当前已经显示在右侧的数据startoffs

        public BerDebugForm()
        {
            InitializeComponent();
        }

        private void BerDebugForm_Load(object sender, EventArgs e)
        {
            this.textBox_logFilename.Text = this.MainForm.AppInfo.GetString(
    "logfileviewer",
    "logfilename",
    "");

        }

        private void BerDebugForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetString(
        "logfileviewer",
        "logfilename",
        this.textBox_logFilename.Text);
            }

        }

        private void button_load_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";

            nRet = LoadIndex(this.textBox_logFilename.Text,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

        }

        // 装载记录索引
        int LoadIndex(string strLogFilename,
            out string strError)
        {
            strError = "";

            Stream stream = null;

            try
            {
                stream = File.OpenRead(strLogFilename);
            }
            catch (Exception ex)
            {
                strError = "file '" + strLogFilename + "'open error: " + ex.Message;
                return -1;
            }

            try
            {
                this.treeView_ber.Nodes.Clear();

                for (long i = 0; ; i++)
                {
                    byte[] len_buffer = new byte[8];

                    int nRet = stream.Read(len_buffer, 0, 8);
                    if (nRet == 0)
                        break;
                    if (nRet != 8)
                    {
                        strError = "file format error";
                        return -1;
                    }

                    long length = BitConverter.ToInt64(len_buffer, 0);

                    if (length == -1)
                    {
                        // 出现了一个未收尾的事项
                        // 把文件最后都算作它的内容
                        length = stream.Length - stream.Position;
                    }

                    IndexInfo info = new IndexInfo();

                    info.index = i;

                    TreeNode node = new TreeNode();

                    if (length == 0)
                    {
                        // 特殊处理
                        node.Text = i.ToString();
                        node.ImageIndex = 0;
                        node.Tag = info;
                        this.treeView_ber.Nodes.Add(node);
                        continue;
                    }

                    int direction = stream.ReadByte();

                    long lStartOffs = stream.Position;

                    length--;   // 这是内容长度

                    node.Text = i.ToString();
                    // 方向'
                    string strDirection = "";
                    int nImageIndex = 0;
                    if (direction == 0)
                    {
                        strDirection = "none";
                        nImageIndex = 0;
                    }
                    else if (direction == 1)
                    {
                        strDirection = "client";
                        nImageIndex = 1;
                    }
                    else if (direction == 2)
                    {
                        strDirection = "server";
                        nImageIndex = 2;
                    }
                    else
                    {
                        strDirection = "error direction value: " + ((int)direction).ToString();
                        nImageIndex = 0;
                    }

                    node.Text += " " + strDirection;
                    node.Text += " len:" + length.ToString();
                    node.Text += " offs:" + lStartOffs.ToString();
                    node.ImageIndex = nImageIndex;

                    info.Offs = lStartOffs;
                    info.Length = length;

                    node.Tag = info;
                    this.treeView_ber.Nodes.Add(node);


                    if (length >= 100*1024)
                    {
                        stream.Seek(length, SeekOrigin.Current);
                        info.BerTree = null;
                    }
                    else {
                        byte [] baPackage = new byte[(int)length];
                        stream.Read(baPackage, 0, (int)length);

                        int nParseStart = 0;
                        for (int j=0; ;j++ )
                        {
                            BerTree tree = new BerTree();
                            int nTotlen = 0;
                            tree.m_RootNode.BuildPartTree(baPackage,
                                nParseStart,
                                baPackage.Length,
                                out nTotlen);

                            if (j == 0)
                            {
                                TreeNode newnode = new TreeNode();
                                node.Nodes.Add(newnode);

                                FillNodeAndChildren(newnode, tree.m_RootNode.ChildrenCollection[0]);
                            }
                            else
                            {
                                TreeNode newnode = new TreeNode();
                                node.Nodes.Add(newnode);

                                FillNodeAndChildren(newnode, tree.m_RootNode.ChildrenCollection[0]);
                            }

                            nParseStart += nTotlen;
                            if (nParseStart >= baPackage.Length)
                                break;
                        }
                    }
                }
            }
            finally
            {
                stream.Close();
            }

            return 0;
        }

        /*
        void FillChildren(TreeNode topnode,
            IndexInfo info)
        {
            TreeNode newnode = new TreeNode();
            topnode.Nodes.Add(newnode);

            FillNodeAndChildren(newnode, info.BerTree.m_RootNode.ChildrenCollection[0]);
        }
         * */

        void FillNodeAndChildren(TreeNode topnode,
            BerNode bernode)
        {
            topnode.Text = bernode.GetDebugString();
            topnode.Tag = bernode;

            for (int i = 0; i < bernode.ChildrenCollection.Count; i++)
            {
                BerNode berchildnode = bernode.ChildrenCollection[i];

                TreeNode newtreenode = new TreeNode();
                topnode.Nodes.Add(newtreenode);
                FillNodeAndChildren(newtreenode, berchildnode);
            }
        }


        private void treeView_ber_AfterSelect(object sender, 
            TreeViewEventArgs e)
        {
            string strError = "";

            TreeNode node = this.treeView_ber.SelectedNode;

            if (node == null)
            {
                SetRightBlank();
                return;
            }

            TreeNode topnode = null;

            if (node.Parent != null)
            {
                topnode = node;
                while (topnode.Parent != null)
                {
                    topnode = topnode.Parent;
                }
            }
            else
            {
                topnode = node;
            }


            {
                IndexInfo info = (IndexInfo)topnode.Tag;

                Debug.Assert(info != null, "");

                if (info.Length == -1 || info.Offs == -1)
                {
                    SetRightBlank();
                    return;
                }

                string strLogFilename = this.textBox_logFilename.Text;

                try
                {
                    using (Stream stream = File.OpenRead(strLogFilename))
                    {
                        if (info.Offs != this.CurrentRightStartOffs)
                        {
                            this.binaryEditor_onePackage.SetData(stream, info.Offs, info.Length);
                            this.CurrentRightStartOffs = info.Offs;
                        }
                    }
                }
                catch (Exception ex)
                {
                    strError = "file '" + strLogFilename + "'open error: " + ex.Message;
                    goto ERROR1;
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void SetRightBlank()
        {
            if (this.CurrentRightStartOffs != -1)
            {
                this.binaryEditor_onePackage.SetData(null, 0, 0);
                this.CurrentRightStartOffs = -1;
            }
        }

        private void treeView_ber_MouseUp(object sender,
            MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            /*
            ToolStripMenuItem subMenuItem = null;
            ToolStripSeparator menuSepItem = null;
             * */

            TreeNode node = this.treeView_ber.SelectedNode;

            // 字符串
            menuItem = new ToolStripMenuItem("字符串内容");
            if (node == null || node.Parent == null)
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_stringContent_Click);
            contextMenu.Items.Add(menuItem);


            // 整数
            menuItem = new ToolStripMenuItem("整数内容");
            if (node == null || node.Parent == null)
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_integerContent_Click);
            contextMenu.Items.Add(menuItem);


            // OID
            menuItem = new ToolStripMenuItem("OID内容");
            if (node == null || node.Parent == null)
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_oidContent_Click);
            contextMenu.Items.Add(menuItem);

            // BITSTRING
            menuItem = new ToolStripMenuItem("BITSTRING内容");
            if (node == null || node.Parent == null)
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_bitstringContent_Click);
            contextMenu.Items.Add(menuItem);


            // 解释包
            menuItem = new ToolStripMenuItem("解释包");
            if (TreeNodeLevel(node) != 1)
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_explainContent_Click);
            contextMenu.Items.Add(menuItem);


            

            contextMenu.Show(this, e.Location);

        }

        void menuItem_stringContent_Click(object sender,
            EventArgs e)
        {
            TreeNode node = this.treeView_ber.SelectedNode;

            if (node == null)
            {
                MessageBox.Show(this, "尚未选定节点");
                return;
            }

            BerNode bernode = (BerNode)node.Tag;

            // MessageBox.Show(this, "'" + bernode.GetCharNodeData() + "'");
            MessageBox.Show(this, "'" + Encoding.GetEncoding(936).GetString(bernode.m_baData) + "'");

        }

        void menuItem_integerContent_Click(object sender,
    EventArgs e)
        {
            TreeNode node = this.treeView_ber.SelectedNode;

            if (node == null)
            {
                MessageBox.Show(this, "尚未选定节点");
                return;
            }

            BerNode bernode = (BerNode)node.Tag;

            MessageBox.Show(this, "'" + bernode.GetIntegerNodeData().ToString() + "'");
        }

        void menuItem_oidContent_Click(object sender,
    EventArgs e)
        {
            TreeNode node = this.treeView_ber.SelectedNode;

            if (node == null)
            {
                MessageBox.Show(this, "尚未选定节点");
                return;
            }

            BerNode bernode = (BerNode)node.Tag;

            MessageBox.Show(this, "'" + bernode.GetOIDsNodeData() + "'");
        }

        void menuItem_bitstringContent_Click(object sender,
EventArgs e)
        {
            TreeNode node = this.treeView_ber.SelectedNode;

            if (node == null)
            {
                MessageBox.Show(this, "尚未选定节点");
                return;
            }

            BerNode bernode = (BerNode)node.Tag;

            MessageBox.Show(this, "'" + bernode.GetBitstringNodeData() + "'");
        }

        // 树节点的层次数
        static int TreeNodeLevel(TreeNode node)
        {
            if (node == null)
                return -1;

            int nLevel = 0;
            while (node.Parent != null)
            {
                nLevel++;
                node = node.Parent;
            }

            return nLevel;
        }

        // 解释包
        void menuItem_explainContent_Click(object sender,
EventArgs e)
        {
            TreeNode node = this.treeView_ber.SelectedNode;

            if (node == null)
            {
                MessageBox.Show(this, "尚未选定节点");
                return;
            }

            BerNode bernode = (BerNode)node.Tag;

            if (TreeNodeLevel(node) != 1)
            {
                MessageBox.Show(this, "必须是Ber包根节点");
                return;
            }

            BerNode root = bernode;
            int nRet = 0;
            string strError = "";
            string strDebugInfo = "OK";

            if (root.m_uTag == BerTree.z3950_initRequest)
            {
        // 观察Initial请求包
                nRet = BerTree.GetInfo_InitRequest(
                    root,
                    out strDebugInfo,
                    out strError);
            }
            else if (root.m_uTag == BerTree.z3950_searchRequest)
            {
                nRet = BerTree.GetInfo_SearchRequest(
                    root,
                    out strDebugInfo,
                    out strError);
            }
            else if (root.m_uTag == BerTree.z3950_presentRequest)
            {
                nRet = BerTree.GetInfo_PresentRequest(
                    root,
                    out strDebugInfo,
                    out strError);
            }
            else if (root.m_uTag == BerTree.z3950_initResponse)
            {
                nRet = BerTree.GetInfo_InitResponse(root,
                    out strDebugInfo,
                    out strError);
            }
            else if (root.m_uTag == BerTree.z3950_searchResponse)
            {
                nRet = BerTree.GetInfo_SearchResponse(root,
                    out strDebugInfo,
                    out strError);
            }
            else if (root.m_uTag == BerTree.z3950_presentResponse)
            {
                RecordCollection records = null;
                SEARCH_RESPONSE search_response = new SEARCH_RESPONSE();
                nRet = BerTree.GetInfo_PresentResponse(root,
                                       ref search_response,
                                       out records,
                                       true,
                                       out strError);
            }

            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                MessageBox.Show(this, strDebugInfo);
        }


        private void treeView_ber_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode curSelectedNode = this.treeView_ber.GetNodeAt(e.X, e.Y);

            if (this.treeView_ber.SelectedNode != curSelectedNode)
                this.treeView_ber.SelectedNode = curSelectedNode;

        }

        private void button_findLogFilename_Click(object sender, EventArgs e)
        {
            // 询问原始文件全路径
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.FileName = this.textBox_logFilename.Text;
            //dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "bin files (*.bin)|*.bin|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.textBox_logFilename.Text = dlg.FileName;
        }
    }

    public class IndexInfo
    {
        public long Offs = -1;
        public long Length = -1;
        public long index = -1;

        public BerTree BerTree = null;
    }
}