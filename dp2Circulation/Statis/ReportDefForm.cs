using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 通用的报表定义 编辑对话框
    /// 对存储在 XML 文件中的报表定义进行编辑修改
    /// </summary>
    public partial class ReportDefForm : Form
    {
        /// <summary>
        /// ApplicationInfo 对象
        /// </summary>
        public IApplicationInfo AppInfo = null;

        /// <summary>
        /// 配置文件名
        /// </summary>
        public string CfgFileName = "";

        /// <summary>
        /// 配置文件的 XmlDocument 对象
        /// </summary>
        public XmlDocument CfgDom = null;

        /// <summary>
        /// 内容是否被修改过
        /// </summary>
        public bool Changed
        {
            get;
            set;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReportDefForm()
        {
            InitializeComponent();
        }

        private void ReportDefForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.CfgFileName) == false)
            {
                string strError = "";
                int nRet = this.LoadCfgFile(this.CfgFileName, out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
            }

            LoadData();
        }

        private void ReportDefForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ReportDefForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_title_typeName.Text) == true)
            {
                this.tabControl_main.SelectedTab = this.tabPage_title;
                strError = "请指定类型";
                goto ERROR1;
            }

            // TODO: 检查，类型名字前三个字符必须为数字
            // 推荐为 101 - xxxxx 这样的形式
            {
                string strText = this.textBox_title_typeName.Text;

                if (strText.Length < 3)
                {
                    this.tabControl_main.SelectedTab = this.tabPage_title;
                    strError = "类型 '" + strText + "' 不合法。应当为三个数字字符引导的字符串";
                    goto ERROR1;
                }

                string strNumber = strText.Substring(0, 3);
                if (StringUtil.IsPureNumber(strNumber) == false)
                {
                    this.tabControl_main.SelectedTab = this.tabPage_title;
                    strError = "类型 '" + strText + "' 不合法。前三个字符应当为数字";
                    goto ERROR1;
                }
            }

            if (string.IsNullOrEmpty(this.textBox_title_title.Text) == true)
            {
                this.tabControl_main.SelectedTab = this.tabPage_title;
                strError = "请指定标题文字";
                goto ERROR1;
            }

            if (this.listView_columns.Items.Count == 0)
            {
                this.tabControl_main.SelectedTab = this.tabPage_columns;
                strError = "请创建至少一个栏目";
                goto ERROR1;
            }

            // CSS 必须具备？

            this.SetData();
            this.Save();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

#if NO
        // 获得 typeName
        // return:
        //      -1  出错
        //      0   配置文件没有找到
        //      1   成功
        public static int GetReportTypeName(
            string strCfgFileName,
            out string strTypeName,
            //out string strCreateFreq,
            out string strError)
        {
            strTypeName = "";
            //strCreateFreq = "";

            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFileName);
            }
            catch (FileNotFoundException)
            {
                return 0;   // 配置文件没有找到
            }
            catch (Exception ex)
            {
                strError = "报表配置文件 " + strCfgFileName + " 打开错误: " + ex.Message;
                return -1;
            }

            if (dom.DocumentElement != null)
            {
                strTypeName = DomUtil.GetElementText(dom.DocumentElement, "typeName");
                //strCreateFreq = DomUtil.GetElementText(dom.DocumentElement, "createFrequency");
            }

            return 1;
        }
#endif

        /// <summary>
        /// 装载配置文件
        /// </summary>
        /// <param name="strCfgFileName">配置文件名</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错；0: 成功</returns>
        public int LoadCfgFile(string strCfgFileName,
    out string strError)
        {
            strError = "";

            this.CfgFileName = strCfgFileName;
            this.CfgDom = new XmlDocument();
            try
            {
                this.CfgDom.Load(this.CfgFileName);
            }
            catch (FileNotFoundException)
            {
                this.CfgDom.LoadXml("<root />");
            }
            catch (Exception ex)
            {
                this.CfgDom = null;
                strError = "报表配置文件 " + this.CfgFileName + " 打开错误: " + ex.Message;
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// 保存到配置文件
        /// </summary>
        public void Save()
        {
            if (this.CfgDom != null && string.IsNullOrEmpty(this.CfgFileName) == false)
            {
                this.CfgDom.Save(this.CfgFileName);
            }

            this.Changed = false;
        }

        const int COLUMN_NAME = 0;
        const int COLUMN_DATATYPE = 1;
        const int COLUMN_ALIGN = 2;
        const int COLUMN_SUM = 3;
        const int COLUMN_CSSCLASS = 4;
        const int COLUMN_EVAL = 5;

        void LoadData()
        {
            if (this.CfgDom == null || this.CfgDom.DocumentElement == null)
                return;

            this.textBox_title_typeName.Text = DomUtil.GetElementText(this.CfgDom.DocumentElement,
                "typeName");

            // XmlNode node_title = this.CfgDom.DocumentElement.SelectSingleNode("title");
            this.textBox_title_title.Text = DomUtil.GetElementText(this.CfgDom.DocumentElement,
                "title").Replace("\\r", "\r\n");
            this.textBox_title_comment.Text = DomUtil.GetElementText(this.CfgDom.DocumentElement,
                "titleComment").Replace("\\r", "\r\n");

            this.listView_columns.Items.Clear();

            XmlNodeList nodes = this.CfgDom.DocumentElement.SelectNodes("columns/column");
            foreach (XmlNode node in nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                string strDataType = DomUtil.GetAttr(node, "type");
                string strAlign = DomUtil.GetAttr(node, "align");
                string strSum = DomUtil.GetAttr(node, "sum");
                string strClass = DomUtil.GetAttr(node, "class");
                string strEval = DomUtil.GetAttr(node, "eval");

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, COLUMN_NAME, strName);
                ListViewUtil.ChangeItemText(item, COLUMN_DATATYPE, strDataType);
                ListViewUtil.ChangeItemText(item, COLUMN_ALIGN, strAlign);
                ListViewUtil.ChangeItemText(item, COLUMN_SUM, strSum);
                ListViewUtil.ChangeItemText(item, COLUMN_CSSCLASS, strClass);
                ListViewUtil.ChangeItemText(item, COLUMN_EVAL, strEval);

                this.listView_columns.Items.Add(item);
            }

            this.textBox_columns_sortStyle.Text = DomUtil.GetElementText(this.CfgDom.DocumentElement,
                "columnSortStyle");

            this.textBox_css_content.Text = DomUtil.GetElementText(this.CfgDom.DocumentElement,
                "css").Replace("\\r", "\r\n").Replace("\\t", "\t");

            this.checkedComboBox_property_createFreq.Text = DomUtil.GetElementText(this.CfgDom.DocumentElement,
                "createFrequency");

            this.checkBox_property_fresh.Checked = DomUtil.GetBooleanParam(
                this.CfgDom.DocumentElement,
                "property",
                "fresh",
                false);
        }

        void SetData()
        {
            if (this.CfgDom == null || this.CfgDom.DocumentElement == null)
                return;

            DomUtil.SetElementText(this.CfgDom.DocumentElement,
                "typeName",
                this.textBox_title_typeName.Text);

            DomUtil.SetElementText(this.CfgDom.DocumentElement,
                "title",
                this.textBox_title_title.Text.Replace("\r\n", "\\r"));

            DomUtil.SetElementText(this.CfgDom.DocumentElement,
                "titleComment",
                this.textBox_title_comment.Text.Replace("\r\n", "\\r"));

            XmlNode node_columns = this.CfgDom.DocumentElement.SelectSingleNode("columns");
            if (node_columns == null)
            {
                node_columns = this.CfgDom.CreateElement("columns");
                this.CfgDom.DocumentElement.AppendChild(node_columns);
            }
            else
            {
                node_columns.RemoveAll();
            }

            foreach (ListViewItem item in this.listView_columns.Items)
            {
                string strName = ListViewUtil.GetItemText(item, COLUMN_NAME);
                string strDataType = ListViewUtil.GetItemText(item, COLUMN_DATATYPE);
                string strAlign = ListViewUtil.GetItemText(item, COLUMN_ALIGN);
                string strSum = ListViewUtil.GetItemText(item, COLUMN_SUM);
                string strClass = ListViewUtil.GetItemText(item, COLUMN_CSSCLASS);
                string strEval = ListViewUtil.GetItemText(item, COLUMN_EVAL);

                XmlNode node_column = this.CfgDom.CreateElement("column");
                node_columns.AppendChild(node_column);

                DomUtil.SetAttr(node_column, "name", strName);
                DomUtil.SetAttr(node_column, "type", strDataType);
                DomUtil.SetAttr(node_column, "align", strAlign);
                DomUtil.SetAttr(node_column, "sum", strSum);
                DomUtil.SetAttr(node_column, "class", strClass);
                DomUtil.SetAttr(node_column, "eval", strEval);
            }

            DomUtil.SetElementText(this.CfgDom.DocumentElement,
                "columnSortStyle",
                this.textBox_columns_sortStyle.Text);

            DomUtil.SetElementText(this.CfgDom.DocumentElement,
                "css",
                this.textBox_css_content.Text.Replace("\r\n", "\\r").Replace("\t", "\\t"));

            DomUtil.SetElementText(this.CfgDom.DocumentElement,
    "createFrequency",
    this.checkedComboBox_property_createFreq.Text);

            DomUtil.SetBooleanParam(
    this.CfgDom.DocumentElement,
    "property",
    "fresh",
    this.checkBox_property_fresh.Checked);

            this.Changed = true;
        }

        private void listView_columns_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("修改栏目 (&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyColumn_Click);
            if (this.listView_columns.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建栏目 (&C)");
            menuItem.Click += new System.EventHandler(this.menu_newColumn_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("上移 (&U)");
            menuItem.Click += new System.EventHandler(this.menu_moveUpColumn_Click);
            if (ListViewUtil.MoveItemEnabled(this.listView_columns, true) == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("下移 (&U)");
            menuItem.Click += new System.EventHandler(this.menu_moveDownColumn_Click);
            if (ListViewUtil.MoveItemEnabled(this.listView_columns, false) == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("删除 [" + this.listView_columns.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_deleteColumn_Click);
            if (this.listView_columns.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_columns, new Point(e.X, e.Y));		
        }

        void menu_moveUpColumn_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<int> indices = null;
            int nRet = ListViewUtil.MoveItemUpDown(this.listView_columns, 
                true,
                out indices, 
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
        }

        void menu_moveDownColumn_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<int> indices = null;
            int nRet = ListViewUtil.MoveItemUpDown(this.listView_columns,
                false,
                out indices,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
        }

        void menu_modifyColumn_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;

            if (this.listView_columns.SelectedItems.Count == 0)
            {
                strError = "尚未选定要修改的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_columns.SelectedItems[0];

            ReportColumnDialog dlg = new ReportColumnDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ColumnName = ListViewUtil.GetItemText(item, COLUMN_NAME);
            dlg.DataType = ListViewUtil.GetItemText(item, COLUMN_DATATYPE);
            dlg.ColumnAlign = ListViewUtil.GetItemText(item, COLUMN_ALIGN);
            dlg.CssClass = ListViewUtil.GetItemText(item, COLUMN_CSSCLASS);
            dlg.ColumnSum = StringUtil.GetBooleanValue(
                ListViewUtil.GetItemText(item, COLUMN_SUM),
                true);
            dlg.Eval = ListViewUtil.GetItemText(item, COLUMN_EVAL);

            if (this.AppInfo != null)
                this.AppInfo.LinkFormState(dlg, "ReportColumnDialog_state");
            dlg.ShowDialog(this);
            if (this.AppInfo != null)
                this.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewUtil.ChangeItemText(item, COLUMN_NAME, dlg.ColumnName);
            ListViewUtil.ChangeItemText(item, COLUMN_DATATYPE, dlg.DataType);
            ListViewUtil.ChangeItemText(item, COLUMN_ALIGN, dlg.ColumnAlign);
            ListViewUtil.ChangeItemText(item, COLUMN_CSSCLASS, dlg.CssClass);
            ListViewUtil.ChangeItemText(item, COLUMN_SUM, dlg.ColumnSum == true ? "yes" : "no");
            ListViewUtil.ChangeItemText(item, COLUMN_EVAL, dlg.Eval);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_newColumn_Click(object sender, EventArgs e)
        {
            ReportColumnDialog dlg = new ReportColumnDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            if (this.AppInfo != null)
                this.AppInfo.LinkFormState(dlg, "ReportColumnDialog_state");
            dlg.ShowDialog(this);
            if (this.AppInfo != null)
                this.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, COLUMN_NAME, dlg.ColumnName);
            ListViewUtil.ChangeItemText(item, COLUMN_DATATYPE, dlg.DataType);
            ListViewUtil.ChangeItemText(item, COLUMN_ALIGN, dlg.ColumnAlign);
            ListViewUtil.ChangeItemText(item, COLUMN_CSSCLASS, dlg.CssClass);
            ListViewUtil.ChangeItemText(item, COLUMN_SUM, dlg.ColumnSum == true ? "yes" : "no");
            ListViewUtil.ChangeItemText(item, COLUMN_EVAL, dlg.Eval);

            this.listView_columns.Items.Add(item);
            ListViewUtil.SelectLine(item, true);
        }

        void menu_deleteColumn_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;

            if (this.listView_columns.SelectedItems.Count == 0)
            {
                strError = "尚未选定要删除的事项";
                goto ERROR1;
            }

            // TODO: 是否对话框询问?

            ListViewUtil.DeleteSelectedItems(this.listView_columns);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_columns_DoubleClick(object sender, EventArgs e)
        {
            menu_modifyColumn_Click(sender, e);
        }

        private void checkedComboBox_property_createFreq_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_property_createFreq.Items.Count > 0)
                return;

            this.checkedComboBox_property_createFreq.Items.Add("day\t每日");
            this.checkedComboBox_property_createFreq.Items.Add("month\t每月");
            this.checkedComboBox_property_createFreq.Items.Add("year\t每年");

        }



        // 从报表配置文件中获得各种配置信息
        // return:
        //      -1  出错
        //      0   没有找到配置文件
        //      1   成功
        internal static int GetReportConfig(string strCfgFile,
            out ReportConfigStruct config,
            out string strError)
        {
            strError = "";
            config = new ReportConfigStruct();

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFile);
            }
            catch (FileNotFoundException)
            {
                strError = "配置文件 '" + strCfgFile + "' 没有找到";
                return 0;
            }
            catch (Exception ex)
            {
                strError = "报表配置文件 " + strCfgFile + " 打开错误: " + ex.Message;
                return -1;
            }

            XmlNode nodeProperty = dom.DocumentElement.SelectSingleNode("property");
            if (nodeProperty != null)
            {
                bool bValue = false;
                int nRet = DomUtil.GetBooleanParam(nodeProperty,
                    "fresh",
                    false,
                    out bValue,
                    out strError);
                if (nRet == -1)
                    return -1;
                config.Fresh = bValue;
            }

            config.ColumnSortStyle = DomUtil.GetElementText(dom.DocumentElement,
                "columnSortStyle");

            config.TypeName = DomUtil.GetElementText(dom.DocumentElement, "typeName");
            config.CreateFreq = DomUtil.GetElementText(dom.DocumentElement, "createFrequency");

            return 1;
        }

        /// <summary>
        /// 获取或设置控件尺寸状态
        /// </summary>
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.listView_columns);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.listView_columns);
                GuiState.SetUiState(controls, value);
            }
        }
    }


    internal class ReportConfigStruct
    {
        public string ColumnSortStyle = "";
        public bool Fresh = false;

        public string TypeName = "";
        public string CreateFreq = "";
    }
}
