using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 对期进行管理的控件
    /// 显示出各期，每期的独特订购信息(也就是包含了验收信息的订购信息)
    /// </summary>
    internal partial class IssueManageControl : UserControl
    {
        public List<string> DeletingIds = new List<string>();

        public const int TYPE_RECIEVE_ZERO = 0; // 一册也未收到
        public const int TYPE_RECIEVE_NOT_COMPLETE = 1; // 尚未收全 
        public const int TYPE_RECIEVE_COMPLETED = 2;    // 已经收全

        // 获得订购信息
        public event GetOrderInfoEventHandler GetOrderInfo = null;

        // 获得册信息
        // public event GetItemInfoEventHandler GetItemInfo = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        /*
        // 创建/删除实体数据
        public event GenerateEntityEventHandler GenerateEntity = null;
         * */

        TreeNode m_currentTreeNode = null;

        public IssueManageControl()
        {
            InitializeComponent();

            this.TreeView.ImageList = this.imageList_treeIcon;

            this.orderDesignControl1.ArriveMode = true;
            this.orderDesignControl1.SeriesMode = true;
            this.orderDesignControl1.Changed = false;

            this.orderDesignControl1.GetValueTable -= new GetValueTableEventHandler(orderDesignControl1_GetValueTable);
            this.orderDesignControl1.GetValueTable += new GetValueTableEventHandler(orderDesignControl1_GetValueTable);

            EanbleOrderDesignControl(false);
        }

        void orderDesignControl1_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            if (this.GetValueTable != null)
                this.GetValueTable(sender, e);
        }

        void EanbleOrderDesignControl(bool bEnable)
        {
            if (bEnable == true)
            {
                this.orderDesignControl1.Enabled = true;
                this.orderDesignControl1.Visible = true;
                this.label_orderInfo_message.Visible = false;
            }
            else
            {
                this.orderDesignControl1.Clear();
                this.orderDesignControl1.Enabled = false;
                this.orderDesignControl1.Visible = false;
                this.label_orderInfo_message.Visible = true;
            }
        }

        public string OrderInfoMessage
        {
            get
            {
                return this.label_orderInfo_message.Text;
            }
            set
            {
                this.label_orderInfo_message.Text = value;
            }
        }

        // 获取值列表时作为线索的数据库名
        public string BiblioDbName
        {
            get
            {
                return this.orderDesignControl1.BiblioDbName;
            }
            set
            {
                this.orderDesignControl1.BiblioDbName = value;
            }
        }

        public void Clear()
        {
            this.TreeView.Nodes.Clear();
        }

        bool m_bChanged = false;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;
            }
        }

        public List<IssueManageItem> Items
        {
            get
            {
                List<IssueManageItem> results = new List<IssueManageItem>();
                for (int i = 0; i < this.TreeView.Nodes.Count; i++)
                {
                    results.Add((IssueManageItem)this.TreeView.Nodes[i].Tag);
                }

                return results;
            }
        }

        public IssueManageItem AppendNewItem(string strXml,
            out string strError)
        {
            strError = "";

            IssueManageItem item = new IssueManageItem();

            /*
            item.Xml = strXml;
            item._dom = new XmlDocument();
            try
            {
                item._dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return null;
            }
             * */
            int nRet = item.Initial(strXml, out strError);
            if (nRet == -1)
                return null;


            // 创建树节点对象
            TreeNode tree_node = new TreeNode();
            tree_node.ImageIndex = TYPE_RECIEVE_ZERO;
            tree_node.Tag = item;
            // 显示出定位信息、卷期册号
            item.SetNodeCaption(tree_node);

            this.TreeView.Nodes.Add(tree_node);

            return item;
        }

        // 构造能表达一期的年、当年期号、总期号、卷号的字符串，常用于显示，而不用于存储
        public static string BuildVolumeDisplayString(
            string strPublishTime,
            string strIssue,
            string strZong,
            string strVolume)
        {
            string strResult = "";

            string strYear = "";
            // 取出年份
            if (String.IsNullOrEmpty(strPublishTime) == true
                || strPublishTime.Length < 4)
                strYear = "????";
            else
            {
                strYear = strPublishTime.Substring(0,4);
            }

            strResult += strYear + ", no."
                + (String.IsNullOrEmpty(strIssue) == false ? strIssue : "?")
                + "";

            if (String.IsNullOrEmpty(strZong) == false)
            {
                if (strResult != "")
                    strResult += ", ";
                strResult += "总." + strZong;
            }


            if (String.IsNullOrEmpty(strVolume) == false)
            {
                if (strResult != "")
                    strResult += ", ";
                strResult += "v." + strVolume;
            }

            return strResult;
        }

        // 构造能表达一期的年、当年期号、总期号、卷号的字符串，常用于显示，而不用于存储
        // 包装后的版本
        public static string BuildVolumeDisplayString(
            string strPublishTime,
            string strItemVolumeString)
        {
            if (String.IsNullOrEmpty(strPublishTime) == true
                && String.IsNullOrEmpty(strItemVolumeString) == true)
                return "";

            if (strPublishTime.IndexOf("-") != -1)
                return "[合订] " + strItemVolumeString; // 合订册的volumestring已经包含了年份，不需要重新组织

            string strIssue = "";
            string strZong = "";
            string strVolume = "";
            VolumeInfo.ParseItemVolumeString(strItemVolumeString,
                out strIssue,
                out strZong,
                out strVolume);

            return BuildVolumeDisplayString(
                strPublishTime,
                strIssue,
                strZong,
                strVolume);
        }


        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e == null)
            {
                // 没有选中任何节点
                EanbleOrderDesignControl(false);

                // 修改工具条按钮状态
                {
                    this.toolStripButton_delete.Enabled = false;
                    this.toolStripButton_modify.Enabled = false;
                    this.toolStripButton_moveUp.Enabled = false;
                    this.toolStripButton_moveDown.Enabled = false;
                }

                return;
            }


            string strError = "";
            // 装入内容到右边

            // 如果当前TreeNode节点下面还没有订购信息，则从外部获取。最好在获取一次后，有个已经获取的标志

            TreeNode tree_node = e.Node;

            // 修改工具条按钮状态
            {
                this.toolStripButton_delete.Enabled = true;
                this.toolStripButton_modify.Enabled = true;

                int index = this.TreeView.Nodes.IndexOf(tree_node);
                if (index == 0)
                    this.toolStripButton_moveUp.Enabled = false;
                else
                    this.toolStripButton_moveUp.Enabled = true;

                if (index >= this.TreeView.Nodes.Count - 1)
                    this.toolStripButton_moveDown.Enabled = false;
                else
                    this.toolStripButton_moveDown.Enabled = true;
            }



            IssueManageItem item = (IssueManageItem)tree_node.Tag;
            Debug.Assert(item != null, "");

            List<string> XmlRecords = new List<string>();
            XmlNodeList nodes = item.dom.DocumentElement.SelectNodes("orderInfo/*");

            if (nodes.Count > 0)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlRecords.Add(nodes[i].OuterXml);
                }
            }
            else if (this.GetOrderInfo != null)
            {
                GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
                e1.BiblioRecPath = "";
                e1.PublishTime = item.PublishTime;
                this.GetOrderInfo(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = "在获取本种内出版日期为 '" + item.PublishTime + "' 的订购信息的过程中发生错误: " + e1.ErrorInfo;
                    goto ERROR1;
                }

                XmlRecords = e1.OrderXmls;

                if (XmlRecords.Count == 0)
                {
                    this.OrderInfoMessage = "出版日期 '" + item.PublishTime + "' 没有对应的的订购信息";
                    EanbleOrderDesignControl(false);

                    item.OrderedCount = -1;
                    this.m_currentTreeNode = e.Node;
                    return;
                }
            }

            this.OrderInfoMessage = "";
            EanbleOrderDesignControl(true);

            // return:
            //      -1  error
            //      >=0 订购的总份数
            int nRet = LoadOrderDesignItems(XmlRecords,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            item.OrderedCount = nRet;

            this.m_currentTreeNode = e.Node;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void TreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            UpdateTreeNodeInfo();
        }

        // 获得可用的最大订购时间范围
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetMaxOrderRange(out string strStartDate,
            out string strEndDate,
            out string strError)
        {
            strStartDate = "";
            strEndDate = "";
            strError = "";

            if (this.GetOrderInfo == null)
                return 0;

            GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
            e1.BiblioRecPath = "";
            e1.PublishTime = "*";
            this.GetOrderInfo(this, e1);
            if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                strError = e1.ErrorInfo;
                return -1;
            }

            if (e1.OrderXmls.Count == 0)
                return 0;

            for (int i = 0; i < e1.OrderXmls.Count; i++)
            {
                string strXml = e1.OrderXmls[i];

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "订购XML装入DOM时发生错误: " + ex.Message;
                    return -1;
                }

                string strRange = DomUtil.GetElementText(dom.DocumentElement,
                    "range");

                string strIssueCount = DomUtil.GetElementText(dom.DocumentElement,
                    "issueCount");

                int nIssueCount = 0;
                try
                {
                    nIssueCount = Convert.ToInt32(strIssueCount);
                }
                catch
                {
                    continue;
                }

                int nRet = strRange.IndexOf("-");
                if (nRet == -1)
                {
                    strError = "时间范围 '" + strRange + "' 格式错误，缺乏-";
                    return -1;
                }

                string strStart = strRange.Substring(0, nRet).Trim();
                string strEnd = strRange.Substring(nRet + 1).Trim();

                if (strStart.Length != 8)
                {
                    strError = "时间范围 '" + strRange + "' 格式错误，左边部分字符数不为8";
                    return -1;
                }
                if (strEnd.Length != 8)
                {
                    strError = "时间范围 '" + strRange + "' 格式错误，右边部分字符数不为8";
                    return -1;
                }

                if (strStartDate == "")
                    strStartDate = strStart;
                else
                {
                    if (String.Compare(strStartDate, strStart) > 0)
                        strStartDate = strStart;
                }

                if (strEndDate == "")
                    strEndDate = strEnd;
                else
                {
                    if (String.Compare(strEndDate, strEnd) < 0)
                        strEndDate = strEnd;
                }
            }

            if (strStartDate == "")
            {
                Debug.Assert(strEndDate == "", "");
                return 0;
            }

            return 1;
        }

        // 检测一个出版时间是否在已经订购的范围内
        bool InOrderRange(string strPublishTime)
        {
            if (this.GetOrderInfo == null)
                return false;

            GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
            e1.BiblioRecPath = "";
            e1.PublishTime = strPublishTime;
            this.GetOrderInfo(this, e1);
            if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                return false;

            if (e1.OrderXmls.Count == 0)
                return false;

            return true;
        }

        // 获得一年内的期总数
        // return:
        //      -1  出错
        //      0   无法获得
        //      1   获得
        int GetOneYearIssueCount(string strPublishYear,
            out int nValue,
            out string strError)
        {
            strError = "";
            nValue = 0;

            if (this.GetOrderInfo == null)
                return 0;   // 无法获得

            GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
            e1.BiblioRecPath = "";
            e1.PublishTime = strPublishYear;
            this.GetOrderInfo(this, e1);
            if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                strError = "在获取本种内出版日期为 '" + strPublishYear + "' 的订购信息的过程中发生错误: " + e1.ErrorInfo;
                return -1;
            }

            if (e1.OrderXmls.Count == 0)
                return 0;

            for (int i = 0; i < e1.OrderXmls.Count; i++)
            {
                string strXml = e1.OrderXmls[i];

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "XML装入DOM时发生错误: " + ex.Message;
                    return -1;
                }

                string strRange = DomUtil.GetElementText(dom.DocumentElement,
                    "range");

                string strIssueCount = DomUtil.GetElementText(dom.DocumentElement,
                    "issueCount");

                int nIssueCount = 0;
                try
                {
                    nIssueCount = Convert.ToInt32(strIssueCount);
                }
                catch
                {
                    continue;
                }

                float years = Global.Years(strRange);
                if (years != 0)
                {
                    nValue = Convert.ToInt32((float)nIssueCount * (1/years));
                }
            }

            return 1;
        }

        public void UpdateTreeNodeInfo()
        {
            if (this.orderDesignControl1.Changed == false)
                return;

            if (this.m_currentTreeNode == null)
                return;


            // 将即将离开焦点的修改过的右边事项保存
            TreeNode tree_node = this.m_currentTreeNode;
            IssueManageItem item = (IssueManageItem)tree_node.Tag;
            XmlNodeList nodes = item.dom.DocumentElement.SelectNodes("orderInfo/*");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                node.ParentNode.RemoveChild(node);
            }

            string strError = "";
            List<string> XmlRecords = null;
            // 根据右边的OrderDesignControl内容构造XML记录
            int nRet = BuildOrderXmlRecords(
                out XmlRecords,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            XmlNode root = item.dom.DocumentElement.SelectSingleNode("orderInfo");
            if (root == null)
            {
                root = item.dom.CreateElement("orderInfo");
                item.dom.DocumentElement.AppendChild(root);
            }
            for (int i = 0; i < XmlRecords.Count; i++)
            {
                XmlDocumentFragment fragment = item.dom.CreateDocumentFragment();
                try
                {
                    fragment.InnerXml = XmlRecords[i];
                }
                catch (Exception ex)
                {
                    strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                    goto ERROR1;
                }

                root.AppendChild(fragment);
                this.Changed = true;
            }

            item.Changed = true;

            item.SetNodeCaption(tree_node); // 刷新节点显示

            this.m_currentTreeNode = null;

            this.orderDesignControl1.Changed = false;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 将订购记录装载到右边的OrderDesignControl中
        // return:
        //      -1  error
        //      >=0 订购的总份数
        int LoadOrderDesignItems(List<string> XmlRecords,
            out string strError)
        {
            strError = "";

            this.orderDesignControl1.DisableUpdate();

            try
            {

                this.orderDesignControl1.Clear();

                int nOrderedCount = 0;  // 顺便计算出订购的总份数
                for (int i = 0; i < XmlRecords.Count; i++)
                {
                    DigitalPlatform.CommonControl.Item item =
                        this.orderDesignControl1.AppendNewItem(XmlRecords[i],
                        out strError);
                    if (item == null)
                        return -1;

                    nOrderedCount += item.OldCopyValue;
                }

                this.orderDesignControl1.Changed = false;
                return nOrderedCount;

            }
            finally
            {
                this.orderDesignControl1.EnableUpdate();
            }

        }

        // 根据右边的OrderDesignControl内容构造XML记录
        int BuildOrderXmlRecords(
            out List<string> XmlRecords,
            out string strError)
        {
            strError = "";
            XmlRecords = new List<string>();

            for (int i = 0; i < this.orderDesignControl1.Items.Count; i++)
            {
                DigitalPlatform.CommonControl.Item design_item = this.orderDesignControl1.Items[i];

                string strXml = "";
                int nRet = design_item.BuildXml(out strXml, out strError);
                if (nRet == -1)
                    return -1;

                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);

                /*
                DomUtil.SetElementText(_dom.DocumentElement,
                    "parent", Global.GetID(this.BiblioRecPath));
                 * */

                /*
                if (design_item.NewlyAcceptedCount > 0)
                {
                    DomUtil.SetElementText(_dom.DocumentElement,
                        "state", "已验收");
                }*/

                XmlRecords.Add(dom.DocumentElement.OuterXml);   // 不要包含prolog
            }

            return 0;
        }

        private void TreeView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            TreeNode node = this.TreeView.SelectedNode;

            //
            menuItem = new MenuItem("修改(&M)");
            menuItem.DefaultItem = true;
            menuItem.Click += new System.EventHandler(this.button_modify_Click);
            if (node == null)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("新增期(&N)");
            menuItem.Click += new System.EventHandler(this.button_newIssue_Click);
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("增全各期(&A)");
            menuItem.Click += new System.EventHandler(this.button_newAllIssue_Click);
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            /*
            // 
            menuItem = new MenuItem("上移(&U)");
            menuItem.Click += new System.EventHandler(this.button_up_Click);
            if (this.TreeView.SelectedNode == null
                || this.TreeView.SelectedNode.PrevNode == null)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);



            // 
            menuItem = new MenuItem("下移(&D)");
            menuItem.Click += new System.EventHandler(this.button_down_Click);
            if (this.TreeView.SelectedNode == null
                || this.TreeView.SelectedNode.NextNode == null)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);
             * */

            //
            menuItem = new MenuItem("全部删除");
            menuItem.Click += new System.EventHandler(this.button_deleteAll_Click);
            if (this.TreeView.Nodes.Count == 0)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);


            //
            menuItem = new MenuItem("删除(&E)");
            menuItem.Click += new System.EventHandler(this.button_delete_Click);
            if (node == null)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            /*
            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.button_CopyToClipboard_Click);
            if (node == null || node.ImageIndex == 0)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(Project)) == false)
                bHasClipboardObject = false;
            else
                bHasClipboardObject = true;



            menuItem = new MenuItem("粘贴到当前目录 '" + GetCurTreeDir() + "' (&P)");
            menuItem.Click += new System.EventHandler(this.button_PasteFromClipboard_Click);
            if (bHasClipboardObject == false)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("粘贴到原目录 '" + GetClipboardProjectDir() + "' (&O)");
            menuItem.Click += new System.EventHandler(this.button_PasteFromClipboardToOriginDir_Click);

            if (bHasClipboardObject == false)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("导出(&E)");
            menuItem.Click += new System.EventHandler(this.button_CopyToFile_Click);
            if (node == null || node.ImageIndex == 0)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);


            */

            contextMenu.Show(TreeView, new Point(e.X, e.Y));		
        }

        // 删除全部期节点
        void button_deleteAll_Click(object sender, System.EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.TreeView.Nodes.Count == 0)
            {
                strError = "没有任何期节点可供删除";
                goto ERROR1;
            }

            string strText = "确实要删除全部 "+this.TreeView.Nodes.Count.ToString()+" 个期节点 ?";
            DialogResult result = MessageBox.Show(this,
                strText,
                "IssueManageControl",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;


            for (int i = this.TreeView.Nodes.Count - 1; i >= 0; i--)
            {
                TreeNode tree_node = this.TreeView.Nodes[i];

                IssueManageItem item = (IssueManageItem)tree_node.Tag;

                List<string> ids = null;
                // 获得册参考ID列表
                nRet = item.GetItemRefIDs(out ids,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获得期事项下属的refid时出错: " + strError;
                    goto ERROR1;
                }

                this.TreeView.Nodes.Remove(tree_node);

                // 删除期节点后，储存属于本期的所有册事项refid
                if (ids.Count > 0)
                {
                    // Debug.Assert(this.GenerateEntity != null, "");

                    this.DeletingIds.AddRange(ids);
                }
            }

            this.m_currentTreeNode = null;
            this.orderDesignControl1.Clear();

            this.Changed = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }


        // 删除期节点
        // TODO: 已经完全验收的期节点，删除的时候要慎重。比方说册有人借阅？
        void button_delete_Click(object sender, System.EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 当前已选择的node
            if (this.TreeView.SelectedNode == null)
            {
                strError = "尚未选择要删除的期节点";
                goto ERROR1;
            }

            IssueManageItem item = (IssueManageItem)this.TreeView.SelectedNode.Tag;

            List<string> ids = null;
            // 获得册参考ID列表
            nRet = item.GetItemRefIDs(out ids,
                out strError);
            if (nRet == -1)
            {
                strError = "获得期事项下属的refid时出错: " + strError;
                goto ERROR1;
            }

            string strText = "确实要删除期节点 '"+this.TreeView.SelectedNode.Text+"' ";

            if (ids.Count > 0)
                strText += "和下属的 " + ids.Count.ToString() + " 个已记到的册事项";
            
            strText += "?";

            DialogResult result = MessageBox.Show(this,
                strText,
                "IssueManageControl",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            if (this.m_currentTreeNode == this.TreeView.SelectedNode)
            {
                this.m_currentTreeNode = null;
            }

            this.TreeView.Nodes.Remove(this.TreeView.SelectedNode);

            if (this.m_currentTreeNode == null)
            {
                this.orderDesignControl1.Clear();
            }

            this.Changed = true;

#if NOOOOOOOOOOOO
            // TODO: 删除期节点后，要注意删除属于本期的所有册事项(标记删除)
            if (ids.Count > 0)
            {
                Debug.Assert(this.GenerateEntity != null, "");

                List<string> deleted_ids = null;
                nRet = DeleteItemRecords(ids, 
                    out deleted_ids,
                    out strError);
                if (nRet == -1)
                {
                    // TODO: 把期节点下已经成功删除的refid去除，然后再报错
                    goto ERROR1;
                }

                // 注：虽然删除过程发生了错误，但是由于删除操作属于maskdelete，所以操作者可以到“册”页去手动undo maskdelete，如果必要
            }
#endif
            // 删除期节点后，储存属于本期的所有册事项refid
            if (ids.Count > 0)
            {
                // Debug.Assert(this.GenerateEntity != null, "");

                this.DeletingIds.AddRange(ids);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

#if NOOOOOOOOOOOOOOOOOOOOOOOO
        // parameters:
        //      deleted_ids 已经成功删除的id
        int DeleteItemRecords(List<string> ids,
            out List<string> deleted_ids,
            out string strError)
        {
            strError = "";
            deleted_ids = new List<string>();

            Debug.Assert(this.GenerateEntity != null, "");

            GenerateEntityEventArgs data_container = new GenerateEntityEventArgs();
            // data_container.InputItemBarcode = this.InputItemsBarcode;
            data_container.SeriesMode = true;

            for (int i = 0; i < ids.Count; i++)
            {
                GenerateEntityData e = new GenerateEntityData();

                e.Action = "delete";
                e.RefID = ids[i];
                e.Xml = "";

                data_container.DataList.Add(e);
            }

            if (data_container.DataList != null
    && data_container.DataList.Count > 0)
            {
                // 调用外部挂接的事件
                this.GenerateEntity(this, data_container);
                string strErrorText = "";

                if (String.IsNullOrEmpty(data_container.ErrorInfo) == false)
                {
                    strError = data_container.ErrorInfo;
                    return -1;
                }

                for (int i = 0; i < data_container.DataList.Count; i++)
                {
                    GenerateEntityData data = data_container.DataList[i];
                    if (String.IsNullOrEmpty(data.ErrorInfo) == false)
                    {
                        strErrorText += data.ErrorInfo;
                    }
                    else
                        deleted_ids.Add(data.RefID);
                }

                if (String.IsNullOrEmpty(strErrorText) == false)
                {
                    strError = strErrorText;
                    return -1;
                }
            }

            return 0;
        }
#endif

        private void button_up_Click(object sender, System.EventArgs e)
        {
            MoveUpDown(true);
        }

        private void button_down_Click(object sender, System.EventArgs e)
        {
            MoveUpDown(false);
        }

        bool MoveUpDown(bool bUp)
        {
            string strError = "";

            // 当前已选择的node
            if (this.TreeView.SelectedNode == null)
            {
                strError = "尚未选择要移动的树节点";
                goto ERROR1;
            }

            TreeNode tree_node = this.TreeView.SelectedNode;

            int index = this.TreeView.Nodes.IndexOf(tree_node);

            if (bUp == true)
            {
                if (index == 0)
                {
                    strError = "已经到头";
                    goto ERROR1;
                }
            }
            else
            {
                if (index >= this.TreeView.Nodes.Count - 1)
                {
                    strError = "已经到尾";
                    goto ERROR1;
                }
            }

            // 移出
            this.TreeView.Nodes.Remove(tree_node);

            // 插入回去
            if (bUp == true)
            {
                this.TreeView.Nodes.Insert(index - 1, tree_node);
                this.Changed = true;
            }
            else
            {
                this.TreeView.Nodes.Insert(index + 1, tree_node);
                this.Changed = true;
            }

            // 选定发生过移动的节点
            this.TreeView.SelectedNode = tree_node;

            return true;    // 发生了移动
        ERROR1:
            MessageBox.Show(this, strError);
            return false;   // 没有移动
        }

        private void TreeView_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode curSelectedNode = this.TreeView.GetNodeAt(e.X, e.Y);

            if (TreeView.SelectedNode != curSelectedNode)
            {
                UpdateTreeNodeInfo();   // 2009/1/6

                TreeView.SelectedNode = curSelectedNode;

                if (TreeView.SelectedNode == null)
                    TreeView_AfterSelect(null, null);	// 补丁
            }

        }

        // 修改期信息
        void button_modify_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.TreeView.SelectedNode == null)
            {
                strError = "尚未选定要修改的期节点";
                goto ERROR1;
            }

            IssueManageItem item = (IssueManageItem)this.TreeView.SelectedNode.Tag;

            IssueDialog dlg = new IssueDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.PublishTime = item.PublishTime;
            dlg.Issue = item.Issue;
            dlg.Zong = item.Zong;
            dlg.Volume = item.Volume;
            dlg.Comment = item.Comment;

            dlg.StartPosition = FormStartPosition.CenterScreen;

            REDO_INPUT:
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            TreeNode dup_tree_node = null;
            // 对出版时间进行查重
            // parameters:
            //      exclude 检查中要排除的TreeNode对象
            // return:
            //      -1  error
            //      0   没有重
            //      1   重
            int nRet = CheckPublishTimeDup(dlg.PublishTime,
                this.TreeView.SelectedNode,
                out dup_tree_node,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                // 选中所重复的TreeNode节点，便于操作者观察重复的情况
                Debug.Assert(dup_tree_node != null, "");
                if (dup_tree_node != null)
                    this.TreeView.SelectedNode = dup_tree_node;

                MessageBox.Show(this, "修改后的期节点 " + strError + "\r\n请修改。");

                goto REDO_INPUT;
            }


            item.PublishTime = dlg.PublishTime;
            item.Issue = dlg.Issue;
            item.Zong = dlg.Zong;
            item.Volume = dlg.Volume;
            item.Comment = dlg.Comment;

            item.SetNodeCaption(this.TreeView.SelectedNode);

            item.Changed = true;

            this.Changed = true;

            // TODO: 修改出版时间后，要注意修改属于本期的所有册的出版时间字段内容
            // 为了避免和标记删除的册事项发生冲突，真正修改前可要求所有未提交的册修改先保存提交?

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        static string IncreaseNumber(string strNumber)
        {
            int v = 0;
            try
            {
                v = Convert.ToInt32(strNumber);
            }
            catch
            {
                return strNumber;   // 增量失败
            }
            return (v+1).ToString();
        }

        // 预测下一期的出版时间
        // exception:
        //      可能因strPublishTime为不可能的日期而抛出异常
        // parameters:
        //      strPublishTime  当前这一期出版时间
        //      nIssueCount 一年内出多少期
        static string NextPublishTime(string strPublishTime,
            int nIssueCount)
        {
            DateTime now = DateTimeUtil.Long8ToDateTime(strPublishTime);

            // 一年一期
            if (nIssueCount == 1)
            {
                return DateTimeUtil.DateTimeToString8(DateTimeUtil.NextYear(now));
            }

            // 一年两期
            if (nIssueCount == 2)
            {
                // 6个月以后的同日
                for (int i = 0; i < 6; i++)
                {
                    now = DateTimeUtil.NextMonth(now);
                }

                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年三期
            if (nIssueCount == 3)
            {
                // 4个月以后的同日
                for (int i = 0; i < 4; i++)
                {
                    now = DateTimeUtil.NextMonth(now);
                }

                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年4期
            if (nIssueCount == 4)
            {
                // 3个月以后的同日
                for (int i = 0; i < 3; i++)
                {
                    now = DateTimeUtil.NextMonth(now);
                }

                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年5期 和一年6期处理办法一样
            // 一年6期
            if (nIssueCount == 5 || nIssueCount == 6)
            {
                // 
                // 2个月以后的同日
                for (int i = 0; i < 2; i++)
                {
                    now = DateTimeUtil.NextMonth(now);
                }

                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年7/8/9/10/11期 和一年12期处理办法一样
            // 一年12期
            if (nIssueCount >= 7 && nIssueCount <= 12)
            {
                // 1个月以后的同日
                now = DateTimeUtil.NextMonth(now);

                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年24期
            if (nIssueCount == 24)
            {
                // 15天以后
                now += new TimeSpan(15,0,0,0);
                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年36期
            if (nIssueCount == 36)
            {
                // 10天以后
                now += new TimeSpan(10, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年48期
            if (nIssueCount == 48)
            {
                // 7天以后
                now += new TimeSpan(7, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年52期
            if (nIssueCount == 52)
            {
                // 7天以后
                now += new TimeSpan(7, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年365期
            if (nIssueCount == 365)
            {
                // 1天以后
                now += new TimeSpan(1, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(now);
            }

            return "????????";  // 无法处理的情形
        }

        /*
        // 根据出版日期查找第一个匹配的TreeNode节点
        TreeNode FindTreeNode(string strPublishTime)
        {
            for (int i = 0; i < this.TreeView.Nodes.Count; i++)
            {
                TreeNode tree_node = this.TreeView.Nodes[i];

                IssueManageItem item = (IssueManageItem)tree_node.Tag;
                Debug.Assert(item != null, "");

                if (item.PublishTime == strPublishTime)
                    return tree_node;
            }

            return null;
        }
        */

        // 增全各期(后插)。从当前最末尾的一个期开始，增补各个期直到超过订购时间范围
        void button_newAllIssue_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            int nCreateCount = 0;

            // 找到最后一期。如果找不到，则先出现对话框询问第一期
            if (this.TreeView.Nodes.Count == 0)
            {
                string strStartDate = "";
                string strEndDate = "";
                // 获得可用的最大订购时间范围
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetMaxOrderRange(out strStartDate,
                    out strEndDate,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "当前没有订购信息，无法进行增全操作";
                    goto ERROR1;
                }


                // 出现对话框，让输入第一期的参数。出版时间由软件自动探测和推荐
                // 这里要求日常管理订购信息把已经到全的订购记录“封闭”。否则会出现把原来早就验收过的第一期出版时间推荐出来的情况
                // 所谓封闭(订购信息的)操作，可以由过刊装订操作来负责

                IssueDialog dlg = new IssueDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "请指定首期的特征";
                dlg.PublishTime = strStartDate + "?";   // 获得订购范围的起点日期
                dlg.EditComment = "当前订购时间范围为 " + strStartDate + "-" + strEndDate;   // 显示可用的订购时间范围
                dlg.StartPosition = FormStartPosition.CenterScreen;

            REDO_INPUT:
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return; // 放弃整个功能

                // 检查一下这个出版时间是否超过订购时间范围?
                if (InOrderRange(dlg.PublishTime) == false)
                {
                    MessageBox.Show(this, "您指定的首期出版时间 '" + dlg.PublishTime + "' 不在当前订购时间范围内，请重新输入。");
                    goto REDO_INPUT;
                }



                IssueManageItem new_item = new IssueManageItem();
                nRet = new_item.Initial("<root />", out strError);
                if (nRet == -1)
                    goto ERROR1;

                new_item.PublishTime = dlg.PublishTime;
                new_item.Issue = dlg.Issue;
                new_item.Zong = dlg.Zong;
                new_item.Volume = dlg.Volume;
                new_item.Comment = dlg.Comment;

                TreeNode tree_node = new TreeNode();
                tree_node.ImageIndex = TYPE_RECIEVE_ZERO;
                tree_node.Tag = new_item;
                // 显示出定位信息、卷期册号
                new_item.SetNodeCaption(tree_node);

                int index = 0;
                if (this.TreeView.SelectedNode != null)
                    index = this.TreeView.Nodes.IndexOf(this.TreeView.SelectedNode) + 1;

                this.TreeView.Nodes.Insert(index, tree_node);
                nCreateCount++;

                new_item.Changed = true;
                this.Changed = true;

                // 选上新插入的节点
                this.TreeView.SelectedNode = tree_node;

            }
            else
            {
                // 选定最后一个TreeNode
                Debug.Assert(this.TreeView.Nodes.Count != 0, "");
                TreeNode last_tree_node = this.TreeView.Nodes[this.TreeView.Nodes.Count - 1];

                if (this.TreeView.SelectedNode != last_tree_node)
                    this.TreeView.SelectedNode = last_tree_node;
            }

            Debug.Assert(this.TreeView.SelectedNode != null, "");
            TreeNode tail_node = this.TreeView.SelectedNode;
            // int nWarningCount = 0;

            // 进行循环，增补全部节点
            for (int i=0;  ;i++ )
            {
                Debug.Assert(this.TreeView.SelectedNode != null, "");

                IssueManageItem ref_item = (IssueManageItem)tail_node.Tag;

                string strNextPublishTime = "";
                string strNextIssue = "";
                string strNextZong = "";
                string strNextVolume = "";

                {
                    int nIssueCount = 0;
                    // 获得一年内的期总数
                    // return:
                    //      -1  出错
                    //      0   无法获得
                    //      1   获得
                    nRet = GetOneYearIssueCount(ref_item.PublishTime,
                        out nIssueCount,
                        out strError);

                    int nRefIssue = 0;
                    try
                    {
                        nRefIssue = Convert.ToInt32(ref_item.Issue);
                    }
                    catch
                    {
                        nRefIssue = 0;
                    }


                    try
                    {
                        // 预测下一期的出版时间
                        // parameters:
                        //      strPublishTime  当前这一期出版时间
                        //      nIssueCount 一年内出多少期
                        strNextPublishTime = NextPublishTime(ref_item.PublishTime,
                             nIssueCount);
                    }
                    catch (Exception ex)
                    {
                        // 2009/2/8
                        strError = "在获得日期 '" + ref_item.PublishTime + "' 的后一期出版日期时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    if (strNextPublishTime == "????????")
                        break;

                    // 检查一下这个出版时间是否超过订购时间范围?
                    if (InOrderRange(strNextPublishTime) == false)
                        break;  // 避免最后多插入一个


                    // 号码自动增量需要知道一个期是否跨年，可以通过查询采购信息得到一年所订阅的期数
                    if (nRefIssue >= nIssueCount
                        && nIssueCount > 0) // 2010/3/3
                    {
                        // 跨年了
                        strNextIssue = "1";
                    }
                    else
                    {
                        strNextIssue = (nRefIssue + 1).ToString();
                    }

                    strNextZong = IncreaseNumber(ref_item.Zong);
                    if (nRefIssue >= nIssueCount && nIssueCount > 0)
                        strNextVolume = IncreaseNumber(ref_item.Volume);
                    else
                        strNextVolume = ref_item.Volume;

                }

                // 对publishTime要查重，对号码体系要进行检查和提出警告
                TreeNode dup_tree_node = null;
                // 对出版时间进行查重
                // parameters:
                //      exclude 检查中要排除的TreeNode对象
                // return:
                //      -1  error
                //      0   没有重
                //      1   重
                nRet = CheckPublishTimeDup(strNextPublishTime,
                    null,
                    out dup_tree_node,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    //this.TreeView.SelectedNode = dup_tree_node;
                    tail_node = dup_tree_node;

                    MessageBox.Show(this, "出版时间为 '" + strNextPublishTime + "' 的期节点已经存在了。其位置将被调整到末尾");

                    // 将重复节点移动到最后位置
                    this.TreeView.Nodes.Remove(dup_tree_node);
                    this.TreeView.Nodes.Add(dup_tree_node);

                    // this.TreeView.SelectedNode = dup_tree_node; // 若没有这一句会引起死循环
                    tail_node = dup_tree_node;
                    continue;
                }

                IssueManageItem new_item = new IssueManageItem();
                nRet = new_item.Initial("<root />", out strError);
                if (nRet == -1)
                    goto ERROR1;

                new_item.PublishTime = strNextPublishTime;
                new_item.Issue = strNextIssue;
                new_item.Zong = strNextZong;
                new_item.Volume = strNextVolume;

                TreeNode tree_node = new TreeNode();
                tree_node.ImageIndex = TYPE_RECIEVE_ZERO;
                tree_node.Tag = new_item;
                // 显示出定位信息、卷期册号
                new_item.SetNodeCaption(tree_node);

                int index = 0;
                /*
                if (this.TreeView.SelectedNode != null)
                    index = this.TreeView.Nodes.IndexOf(this.TreeView.SelectedNode) + 1;
                 * */
                if (tail_node != null)
                    index = this.TreeView.Nodes.IndexOf(tail_node) + 1;

                this.TreeView.Nodes.Insert(index, tree_node);
                nCreateCount++;

                new_item.Changed = true;
                this.Changed = true;

                /*
                // 选上新插入的节点
                this.TreeView.SelectedNode = tree_node;
                 * */
                tail_node = tree_node;
            }

          
            if (tail_node != null)
            {
                // 选上新插入的节点
                this.TreeView.SelectedNode = tail_node;
            }

            string strMessage = "";
            if (nCreateCount == 0)
                strMessage = "没有增加新的期节点";
            else
                strMessage = "共新增了 " + nCreateCount.ToString() + " 个期节点";

            MessageBox.Show(this, strMessage);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 新增期(后插)
        void button_newIssue_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            IssueManageItem ref_item = null;
            
            if (this.TreeView.SelectedNode != null)
                ref_item = (IssueManageItem)this.TreeView.SelectedNode.Tag;

            IssueDialog dlg = new IssueDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            if (ref_item != null)
            {
                // TODO: 最好能自动增量

                int nIssueCount = 0;
                // 获得一年内的期总数
                // return:
                //      -1  出错
                //      0   无法获得
                //      1   获得
                nRet = GetOneYearIssueCount(ref_item.PublishTime,
                    out nIssueCount,
                    out strError);

                int nRefIssue = 0;
                try
                {
                    nRefIssue = Convert.ToInt32(ref_item.Issue);
                }
                catch
                {
                    nRefIssue = 0;
                }


                string strNextPublishTime = "";

                try
                {
                    // 预测下一期的出版时间
                    // parameters:
                    //      strPublishTime  当前这一期出版时间
                    //      nIssueCount 一年内出多少期
                    strNextPublishTime = NextPublishTime(ref_item.PublishTime,
                         nIssueCount);
                }
                catch (Exception ex)
                {
                    // 2009/2/8
                    strError = "在获得日期 '" + ref_item.PublishTime + "' 的后一期出版日期时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                dlg.PublishTime = strNextPublishTime;

                // 号码自动增量需要知道一个期是否跨年，可以通过查询采购信息得到一年所订阅的期数
                if (nRefIssue >= nIssueCount
                    && nIssueCount > 0) // 2010/3/3
                {
                    // 跨年了
                    dlg.Issue = "1";
                }
                else
                {
                    dlg.Issue = (nRefIssue+1).ToString();
                }

                dlg.Zong = IncreaseNumber(ref_item.Zong);
                if (nRefIssue >= nIssueCount && nIssueCount > 0)
                    dlg.Volume = IncreaseNumber(ref_item.Volume);
                else
                    dlg.Volume = ref_item.Volume;

                if (nIssueCount > 0)
                    dlg.EditComment = "一年出版 " + nIssueCount.ToString() + " 期";
            }

            dlg.StartPosition = FormStartPosition.CenterScreen;

            REDO_INPUT:
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 对publishTime要查重，对号码体系要进行检查和提出警告
            TreeNode dup_tree_node = null;
            // 对出版时间进行查重
            // parameters:
            //      exclude 检查中要排除的TreeNode对象
            // return:
            //      -1  error
            //      0   没有重
            //      1   重
            nRet = CheckPublishTimeDup(dlg.PublishTime,
                null,
                out dup_tree_node,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                // 选中所重复的TreeNode节点，便于操作者观察重复的情况
                Debug.Assert(dup_tree_node != null, "");
                if (dup_tree_node != null)
                    this.TreeView.SelectedNode = dup_tree_node;

                MessageBox.Show(this, "拟新增的期节点 " + strError + "\r\n请修改。");

                goto REDO_INPUT;
            }

            IssueManageItem new_item = new IssueManageItem();
            nRet = new_item.Initial("<root />", out strError);
            if (nRet == -1)
                goto ERROR1;

            new_item.PublishTime = dlg.PublishTime;
            new_item.Issue = dlg.Issue;
            new_item.Zong = dlg.Zong;
            new_item.Volume = dlg.Volume;
            new_item.Comment = dlg.Comment;

            TreeNode tree_node = new TreeNode();
            tree_node.ImageIndex = TYPE_RECIEVE_ZERO;
            tree_node.Tag = new_item;
            // 显示出定位信息、卷期册号
            new_item.SetNodeCaption(tree_node);

            int index = 0;
            if (this.TreeView.SelectedNode != null)
                index = this.TreeView.Nodes.IndexOf(this.TreeView.SelectedNode) + 1;

            this.TreeView.Nodes.Insert(index, tree_node);

            new_item.Changed = true;
            this.Changed = true;

            // 选上新插入的节点
            this.TreeView.SelectedNode = tree_node;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 对出版时间进行查重
        // parameters:
        //      exclude 检查中要排除的TreeNode对象
        // return:
        //      -1  error
        //      0   没有重
        //      1   重
        int CheckPublishTimeDup(string strPublishTime,
            TreeNode exclude,
            out TreeNode dup_tree_node,
            out string strError)
        {
            strError = "";
            dup_tree_node = null;

            for (int i = 0; i < this.TreeView.Nodes.Count; i++)
            {
                TreeNode tree_node = this.TreeView.Nodes[i];

                if (tree_node == exclude)
                    continue;

                IssueManageItem item = (IssueManageItem)tree_node.Tag;

                if (item.PublishTime == strPublishTime)
                {
                    strError = "出版时间 '" + strPublishTime + "' 和位置 " + (i+1).ToString() + " 节点重复了";
                    dup_tree_node = tree_node;
                    return 1;
                }
            }

            return 0;
        }

        private void TreeView_DoubleClick(object sender, EventArgs e)
        {
            button_modify_Click(sender, e);
        }

        private void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Insert)
            {
                button_newIssue_Click(this, null);
                e.Handled = true;
            }
            else if (e.KeyData == Keys.Delete)
            {
                button_delete_Click(this, null);
                e.Handled = true;
            }
        }

        private void toolStripButton_newIssue_Click(object sender, EventArgs e)
        {
            button_newIssue_Click(sender, e);
        }

        private void toolStripButton_delete_Click(object sender, EventArgs e)
        {
            button_delete_Click(sender, e);
        }

        // 修改一个期节点
        private void toolStripButton_modify_Click(object sender, EventArgs e)
        {
            button_modify_Click(sender, e);
        }

        // 新增(一个周期内的)全部期节点
        private void toolStripButton_newAll_Click(object sender, EventArgs e)
        {
            button_newAllIssue_Click(sender, e);
        }

        private void toolStripButton_moveUp_Click(object sender, EventArgs e)
        {
            button_up_Click(sender, e);
        }

        private void toolStripButton_moveDown_Click(object sender, EventArgs e)
        {
            button_down_Click(sender, e);
        }

        public void Sort()
        {
            TreeView.TreeViewNodeSorter = new NodeSorter();
            this.TreeView.Sort();
            // 一旦排序进行后，没有清除TreeViewNodeSorter，则插入新对象的时候会自动排序
        }
    }


    // 第一层次，期对象
    internal class IssueManageItem
    {
        public IssueManageControl Container = null;

        public object Tag = null;   // 用于存放需要连接的任意类型对象

        public string Xml = ""; // 一个期记录的XML

        internal XmlDocument dom = null;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed = false;

        public int OrderedCount = -1;    // 订购的份数。从订购XML中获得的。-1表示未知

        public int Initial(string strXml,
            out string strError)
        {
            strError = "";

            this.Xml = strXml;
            this.dom = new XmlDocument();
            try
            {
                this.dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        public string PublishTime
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "publishTime");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "publishTime", value);
            }
        }

        public string Issue
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "issue");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "issue", value);
            }
        }

        public string Volume
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "volume");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "volume", value);
            }
        }

        // 2010/3/28
        public string Comment
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "comment");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "comment", value);
            }
        }

        public string Zong
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "zong");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "zong", value);
            }
        }

        public string RefID
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementText(this.dom.DocumentElement,
                    "refID");
            }
            set
            {
                if (this.dom == null)
                    throw new Exception("this.dom尚未初始化");

                DomUtil.SetElementText(this.dom.DocumentElement,
                    "refID", value);
            }
        }

        public string OrderInfo
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetElementInnerXml(this.dom.DocumentElement,
                    "orderInfo");
            }
        }

        // 获得册参考ID列表
        public int GetItemRefIDs(out List<string> ids,
            out string strError)
        {
            strError = "";
            ids = new List<string>();

            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*/distribute");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strDistribute = node.InnerText.Trim();
                if (String.IsNullOrEmpty(strDistribute) == true)
                    continue;

                LocationCollection locations = new LocationCollection();
                int nRet = locations.Build(strDistribute,
                    out strError);
                if (nRet == -1)
                    return -1;

                for (int j = 0; j < locations.Count; j++)
                {
                    Location location = locations[j];

                    // 尚未创建过的事项，跳过
                    if (location.RefID == "*"
                        || String.IsNullOrEmpty(location.RefID) == true)
                        continue;

                    ids.Add(location.RefID);
                }
            }

            return 0;
        }

        // 设置树节点的文字和图像Icon
        public void SetNodeCaption(TreeNode tree_node)
        {
            Debug.Assert(this.dom != null, "");

            string strPublishTime = DomUtil.GetElementText(this.dom.DocumentElement,
                "publishTime");
            string strIssue = DomUtil.GetElementText(this.dom.DocumentElement,
                "issue");
            string strVolume = DomUtil.GetElementText(this.dom.DocumentElement,
                "volume");
            string strZong = DomUtil.GetElementText(this.dom.DocumentElement,
                "zong");

            int nOrderdCount = 0;
            int nRecievedCount = 0;
            // 已验收的册数
            // string strOrderInfoXml = "";

            if (this.dom == null)
                goto SKIP_COUNT;

            {

                XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("orderInfo/*/copy");
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];

                    string strCopy = node.InnerText.Trim();
                    if (String.IsNullOrEmpty(strCopy) == true)
                        continue;

                    string strNewCopy = "";
                    string strOldCopy = "";
                    dp2StringUtil.ParseOldNewValue(strCopy,
                        out strOldCopy,
                        out strNewCopy);

                    int nNewCopy = 0;
                    int nOldCopy = 0;

                    try
                    {
                        if (String.IsNullOrEmpty(strNewCopy) == false)
                        {
                            nNewCopy = Convert.ToInt32(strNewCopy);
                        }
                        if (String.IsNullOrEmpty(strOldCopy) == false)
                        {
                            nOldCopy = Convert.ToInt32(strOldCopy);
                        }
                    }
                    catch
                    {
                    }

                    nOrderdCount += nOldCopy;
                    nRecievedCount += nNewCopy;
                }
            }

        SKIP_COUNT:

            if (this.OrderedCount == -1 && nOrderdCount > 0)
                this.OrderedCount = nOrderdCount;

            tree_node.Text = strPublishTime + " no." + strIssue + " 总." + strZong + " v." + strVolume + " (" + nRecievedCount.ToString() + ")";

            if (this.OrderedCount == -1)
            {
                if (nRecievedCount == 0)
                    tree_node.ImageIndex = IssueManageControl.TYPE_RECIEVE_ZERO;
                else
                    tree_node.ImageIndex = IssueManageControl.TYPE_RECIEVE_NOT_COMPLETE;
            }
            else
            {
                if (nRecievedCount >= this.OrderedCount)
                    tree_node.ImageIndex = IssueManageControl.TYPE_RECIEVE_COMPLETED;
                else if (nRecievedCount > 0)
                    tree_node.ImageIndex = IssueManageControl.TYPE_RECIEVE_NOT_COMPLETE;
                else
                    tree_node.ImageIndex = IssueManageControl.TYPE_RECIEVE_ZERO;
            }

            tree_node.SelectedImageIndex = tree_node.ImageIndex;
        }

    }

    /*
    // 第二层次，采购信息对象
    public class OrderItem
    {


    }*/




    // Create a node sorter that implements the IComparer interface.
    internal class NodeSorter : IComparer
    {
        // Compare the length of the strings, or the strings
        // themselves, if they are the same length.
        public int Compare(object x, object y)
        {
            IssueManageItem item_x = (IssueManageItem)((TreeNode)x).Tag;
            IssueManageItem item_y = (IssueManageItem)((TreeNode)y).Tag;

            return string.Compare(item_x.PublishTime, item_y.PublishTime);
        }
    }

}
