using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.Script;


namespace dp2Circulation
{
    /// <summary>
    /// 输出订单对话框。既用于输出订单前指定订单输出属性，也用于平时配置订单输出属性
    /// </summary>
    internal partial class OrderFileDialog : Form
    {
        /// <summary>
        /// 当前是否为“执行模式”？
        /// 如果为执行模式，“确定”按钮将显示文字“开始输出”
        /// </summary>
        public bool RunMode = false; // 

        // OK按钮按下后，反映当前已经配置的输出事项
        /// <summary>
        /// 当前已经配置的输出事项
        /// </summary>
        public List<OutputItem> OutputItems = new List<OutputItem>();

        /// <summary>
        /// 脚本管理器
        /// </summary>
        public ScriptManager ScriptManager = null;

        /// <summary>
        /// 系统配置参数存储对象
        /// </summary>
        public ApplicationInfo AppInfo = null;

        /// <summary>
        /// 数据目录
        /// </summary>
        public string DataDir = "";

        // 获得值列表
        /// <summary>
        /// 事件接口。当前对话框内需要获得值列表时被触发
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        /// <summary>
        /// 配置文件名全路径
        /// </summary>
        public string CfgFileName = "";

        bool m_bChanged = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public OrderFileDialog()
        {
            InitializeComponent();
        }

        private void OrderFileDialog_Load(object sender, EventArgs e)
        {
            string strError = "";

            if (this.RunMode == true)
            {
                this.button_OK.Text = "开始输出";
                this.button_OK.Font = new Font(this.button_OK.Font, FontStyle.Bold);

                this.toolStrip_list.Enabled = false;
                this.toolStrip_list.Visible = false;

                this.button_projectManager.Enabled = false;
                this.button_projectManager.Visible = false;
            }

            if (String.IsNullOrEmpty(this.CfgFileName) == false)
            {
                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                int nRet = FillList(this.CfgFileName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.m_bChanged = false;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void OrderFileDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void OrderFileDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        int FillList(string strCfgFilename,
            out string strError)
        {
            strError = "";

            this.listView_list.Items.Clear();

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFilename);
            }
            catch (FileNotFoundException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                strError = "将配置文件 '" + strCfgFilename + "' 装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            if (dom.DocumentElement == null)
            {
                strError = "缺乏根元素";
                return -1;
            }

            this.textBox_outputFolder.Text = DomUtil.GetAttr(dom.DocumentElement,
                "outputFolder");

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("seller");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strSeller = DomUtil.GetAttr(node, "name");
                string strFormat = DomUtil.GetAttr(node, "outputFormat");

                ListViewItem item = new ListViewItem();
                item.Text = strSeller;
                item.SubItems.Add(strFormat);

                this.listView_list.Items.Add(item);
            }

            return 1;
        }

        int SaveList(string strCfgFilename,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root/>");

            DomUtil.SetAttr(dom.DocumentElement,
                "outputFolder",
                this.textBox_outputFolder.Text);

            for (int i = 0; i < this.listView_list.Items.Count; i++)
            {
                ListViewItem item = this.listView_list.Items[i];

                string strSellerName = ListViewUtil.GetItemText(item, 0);
                string strOutputFormat = ListViewUtil.GetItemText(item, 1);

                XmlNode node = dom.CreateElement("seller");
                dom.DocumentElement.AppendChild(node);

                DomUtil.SetAttr(node, "name", strSellerName);
                DomUtil.SetAttr(node, "outputFormat", strOutputFormat);
            }

            try
            {
                dom.Save(strCfgFilename);
            }
            catch (Exception ex)
            {
                strError = "保存XMLDOM到文件 '" + strCfgFilename + "' 时发生错误: " + ex.Message;
                return -1;
            }

            return 0;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.textBox_outputFolder.Text == "")
            {
                strError = "尚未指定订单输出目录";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(this.CfgFileName) == true)
            {
                strError = "CfgFileName值为空，无法保存到配置文件";
                goto ERROR1;
            }

            if (m_bChanged == true)
            {
                nRet = SaveList(this.CfgFileName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 创建OutputItems，以便窗口关闭后外部使用
            this.OutputItems.Clear();
            for (int i = 0; i < this.listView_list.Items.Count; i++)
            {
                ListViewItem item = this.listView_list.Items[i];

                OutputItem output = new OutputItem();
                output.Seller = item.Text;
                output.OutputFormat = ListViewUtil.GetItemText(item, 1);

                this.OutputItems.Add(output);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            // TODO: 是否警告有修改?

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button_projectManager_Click(object sender, EventArgs e)
        {
            ProjectManageDlg dlg = new ProjectManageDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.ProjectsUrl = "http://dp2003.com/dp2circulation/projects/projects.xml";
            dlg.HostName = "PrintOrderForm";
            dlg.scriptManager = this.ScriptManager;
            dlg.AppInfo = this.AppInfo;
            dlg.DataDir = this.DataDir;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
        }

        private void button_findOutputFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.Description = "请指定订单文件输出的目录:";
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dlg.SelectedPath = this.textBox_outputFolder.Text;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_outputFolder.Text = dlg.SelectedPath;
        }

        private void textBox_outputFolder_TextChanged(object sender, EventArgs e)
        {
            this.m_bChanged = true;

            if (this.textBox_outputFolder.Text == "")
                this.toolStripButton_openOutputFolder.Enabled = false;
            else
                this.toolStripButton_openOutputFolder.Enabled = true;
        }

        private void listView_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_list.SelectedItems.Count == 0)
            {
                this.toolStripButton_modifyItem.Enabled = false;
                this.toolStripButton_deleteItem.Enabled = false;
            }
            else
            {
                this.toolStripButton_modifyItem.Enabled = true;
                this.toolStripButton_deleteItem.Enabled = true;
            }
        }

        List<string> GetUsedSellers(ListViewItem skip)
        {
            List<string> results = new List<string>();
            for(int i=0;i<this.listView_list.Items.Count;i++)
            {
                ListViewItem item = this.listView_list.Items[i];
                if (item == skip)
                    continue;
                results.Add(item.Text);
            }

            return results;
        }

        // 新增一个事项
        private void toolStripButton_newItem_Click(object sender, EventArgs e)
        {
            OrderOutputItemDialog dlg = new OrderOutputItemDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.ScriptManager = this.ScriptManager;
            dlg.AppInfo = this.AppInfo;

            dlg.ExcludeSellers = GetUsedSellers(null);

            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

            dlg.StartPosition = FormStartPosition.CenterScreen;

        REDO_INPUT:
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // TODO: 渠道名查重
            ListViewItem dup = ListViewUtil.FindItem(this.listView_list, dlg.Seller, 0);
            if (dup != null)
            {
                MessageBox.Show(this, "渠道名 '" + dlg.Seller + "' 在当前列表中已经存在。请重新输入...");
                goto REDO_INPUT;
            }

            ListViewItem item = new ListViewItem();
            item.Text = dlg.Seller;
            item.SubItems.Add(dlg.OutputFormat);

            this.listView_list.Items.Add(item);
            ListViewUtil.SelectLine(item, true);
            this.m_bChanged = true;
        }

        void dlg_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            if (this.GetValueTable != null)
                this.GetValueTable(sender, e);
        }

        // 修改一个事项
        private void toolStripButton_modifyItem_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_list.SelectedItems.Count == 0)
            {
                strError = "尚未选定要修改的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_list.SelectedItems[0];

            OrderOutputItemDialog dlg = new OrderOutputItemDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.ScriptManager = this.ScriptManager;
            dlg.AppInfo = this.AppInfo;

            dlg.ExcludeSellers = GetUsedSellers(item);

            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

            dlg.Seller = ListViewUtil.GetItemText(item, 0);
            dlg.OutputFormat = ListViewUtil.GetItemText(item, 1);
            dlg.StartPosition = FormStartPosition.CenterScreen;

        REDO_INPUT:
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // TODO: 渠道名查重
            ListViewItem dup = ListViewUtil.FindItem(this.listView_list, dlg.Seller, 0);
            if (dup != null && dup != item)
            {
                MessageBox.Show(this, "修改后的渠道名 '" + dlg.Seller + "' 在当前列表中已经存在。请重新输入...");
                goto REDO_INPUT;
            }

            ListViewUtil.ChangeItemText(item, 0, dlg.Seller);
            ListViewUtil.ChangeItemText(item, 1, dlg.OutputFormat);
            // ListViewUtil.SelectLine(item, true);
            this.m_bChanged = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 删除一个或多个事项
        private void toolStripButton_deleteItem_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_list.SelectedIndices.Count == 0)
            {
                strError = "尚未选定要删除的事项";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
                "确实要删除选定的 "+this.listView_list.SelectedIndices.Count.ToString()+" 个事项? ",
                "OrderFileDialog",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

#if NO
            for (int i = this.listView_list.SelectedIndices.Count - 1; i >= 0; i--)
            {
                this.listView_list.Items.RemoveAt(this.listView_list.SelectedIndices[i]);
            } 
#endif
            // 2012/3/11
            ListViewUtil.DeleteSelectedItems(this.listView_list);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_list_DoubleClick(object sender, EventArgs e)
        {
            this.toolStripButton_modifyItem_Click(sender, e);
        }

        // 上下文菜单
        private void listView_list_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("修改(&M)");
            menuItem.DefaultItem = true;
            menuItem.Click += new System.EventHandler(this.toolStripButton_modifyItem_Click);
            if (this.listView_list.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新增(&N)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_newItem_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_deleteItem_Click);
            if (this.listView_list.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_list, new Point(e.X, e.Y));		
        }

        /// <summary>
        /// 获得或设置输出目录
        /// </summary>
        public string OutputFolder
        {
            get
            {
                return this.textBox_outputFolder.Text;
            }
            set
            {
                this.textBox_outputFolder.Text = value;
            }
        }

        private void toolStripButton_openOutputFolder_Click(object sender, EventArgs e)
        {
            if (this.textBox_outputFolder.Text == "")
            {
                MessageBox.Show(this, "尚未指定输出目录，无法打开");
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(this.textBox_outputFolder.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }
    }

    /// <summary>
    /// 输出事项
    /// </summary>
    public class OutputItem
    {
        /// <summary>
        /// 渠道名
        /// </summary>
        public string Seller = "";

        /// <summary>
        /// 输出格式
        /// </summary>
        public string OutputFormat = "";
    }
}