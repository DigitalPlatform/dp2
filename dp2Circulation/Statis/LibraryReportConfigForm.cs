using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.GUI;
using System.IO;
using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;
using System.Collections;

namespace dp2Circulation
{
    /// <summary>
    /// 一个分馆的报表配置
    /// </summary>
    public partial class LibraryReportConfigForm : Form
    {
        public MainForm MainForm = null;

        bool _changed = false;

        public bool Changed
        {
            get
            {
                return this._changed;
            }
            set
            {
                this._changed = value;
            }
        }

        public LibraryReportConfigForm()
        {
            InitializeComponent();
        }

#if NO
        /// <summary>
        /// 102 表的部门定义
        /// \r\n 分隔的字符串列表
        /// </summary>
        public string table_102_departments
        {
            get
            {
                return this.textBox_102_departments.Text;
            }
            set
            {
                this.textBox_102_departments.Text = value;
            }
        }
#endif

        private void LibraryReportConfigForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                List<string> librarycodes = this.MainForm.GetAllLibraryCode();
                foreach (string c in librarycodes)
                {
                    if (string.IsNullOrEmpty(c) == true)
                        this.comboBox_general_libraryCode.Items.Add("[全局]");
                    else
                        this.comboBox_general_libraryCode.Items.Add(c);
                }
            }

            this.Changed = false;
        }

        public void Clear()
        {
            this.comboBox_general_libraryCode.Text = "";
            this.listView_reports.Items.Clear();
        }

        // 自动增全报表配置
        // return:
        //      -1  出错
        //      >=0 新增的报表类型个数
        public int AutoAppend(
            out string strError)
        {
            strError = "";

            string strCfgFileDir = Path.Combine(this.MainForm.UserDir, "report_def");   //  Path.Combine(this.MainForm.UserDir, "report_def");

            DirectoryInfo di = new DirectoryInfo(strCfgFileDir);
            FileInfo[] fis = di.GetFiles("???.xml");
            Array.Sort(fis, new FileInfoCompare());

            List<string> types = new List<string>();
            foreach (FileInfo fi in fis)
            {
                string strName = Path.GetFileNameWithoutExtension(fi.Name);

                ListViewItem dup = ListViewUtil.FindItem(this.listView_reports, strName, COLUMN_REPORT_TYPE);
                if (dup != null)
                    continue;

                types.Add(strName);
            }

            int nCount = 0;
            int i = 0;
            foreach (string strType in types)
            {
                string strFileName = strType + ".xml";
                string strLocalFilePath = Path.Combine(this.MainForm.UserDir, "report_def\\" + strFileName);

                ReportConfigStruct config = null;

                // 从报表配置文件中获得各种配置信息
                // return:
                //      -1  出错
                //      0   没有找到配置文件
                //      1   成功
                int nRet = ReportDefForm.GetReportConfig(strLocalFilePath,
                    out config,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    return -1;

                string strReportName = "";
                if (string.IsNullOrEmpty(config.TypeName) == false
    && config.TypeName.Length > 3)
                    strReportName = config.TypeName.Substring(3).Trim();
                else
                    strReportName = strType + "_" + (i + 1).ToString();

                string strNameTable = "";
                if (strType == "102")
                    strNameTable = "";  // TODO: 根据现有数据创建名称表

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, COLUMN_REPORT_NAME, strReportName);
                ListViewUtil.ChangeItemText(item, COLUMN_REPORT_FREQ, config.CreateFreq);
                ListViewUtil.ChangeItemText(item, COLUMN_REPORT_TYPE, strType);
                ListViewUtil.ChangeItemText(item, COLUMN_REPORT_CFGFILE, strLocalFilePath);
                ListViewUtil.ChangeItemText(item, COLUMN_REPORT_NAMETABLE, strNameTable);
                this.listView_reports.Items.Add(item);

                i++;
                nCount++;
            }

            return nCount;
        }

        // 自动创建全部报表配置
        public int AutoCreate(
            string strLibraryCode,
            out string strError)
        {
            strError = "";

            this.Clear();

            // 基本参数
            this.comboBox_general_libraryCode.Text = strLibraryCode;

            // 创建每种类型的报表
#if NO
            string[] types = new string[] { 
                "101",
                "102",
                "111",
                "121",
                "131",
                "201",
                "202",
                "212",
                "301",
                "441",
                "442",
                "443",
            };
#endif

            string strCfgFileDir = Path.Combine(this.MainForm.UserDir, "report_def");   //  Path.Combine(this.MainForm.UserDir, "report_def");

            DirectoryInfo di = new DirectoryInfo(strCfgFileDir);
            FileInfo[] fis = di.GetFiles("???.xml");
            Array.Sort(fis, new FileInfoCompare());

            List<string> types = new List<string>();
            foreach (FileInfo fi in fis)
            {
                types.Add(Path.GetFileNameWithoutExtension(fi.Name));
            }

            int i = 0;
            foreach (string strType in types)
            {
                // 从 dp2003.com 下载配置文件
                string strFileName = strType + ".xml";
                string strLocalFilePath = Path.Combine(this.MainForm.UserDir, "report_def\\" + strFileName);

#if NO
                int nRet = DownloadDataFile(
                    this,
                    strFileName,
                    strCfgFileDir,
                    out strLocalFilePath,
                    out strError);
                if (nRet == -1)
                    return -1;
#endif

                ReportConfigStruct config = null;

                // 从报表配置文件中获得各种配置信息
                // return:
                //      -1  出错
                //      0   没有找到配置文件
                //      1   成功
                int nRet = ReportDefForm.GetReportConfig(strLocalFilePath,
                    out config,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    return -1;

                string strReportName = "";
                if (string.IsNullOrEmpty(config.TypeName) == false
    && config.TypeName.Length > 3)
                    strReportName = config.TypeName.Substring(3).Trim();
                else
                    strReportName = strType + "_" + (i+1).ToString();

                string strNameTable = "";
                if (strType == "102")
                    strNameTable = "";  // TODO: 根据现有数据创建名称表

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, COLUMN_REPORT_NAME, strReportName);
                ListViewUtil.ChangeItemText(item, COLUMN_REPORT_FREQ, config.CreateFreq);
                ListViewUtil.ChangeItemText(item, COLUMN_REPORT_TYPE, strType);
                ListViewUtil.ChangeItemText(item, COLUMN_REPORT_CFGFILE, strLocalFilePath);
                ListViewUtil.ChangeItemText(item, COLUMN_REPORT_NAMETABLE, strNameTable);
                this.listView_reports.Items.Add(item);

                i++;
            }

            return 0;
        }

        class FileInfoCompare : IComparer
        {
            int IComparer.Compare(Object x, Object y)
            {
                return ((new CaseInsensitiveComparer()).Compare(((FileInfo)x).Name, ((FileInfo)y).Name));
            }
        }

        int DownloadDataFile(
            IWin32Window owner,
            string strFileName,
            string strCfgFileDir,
            out string strLocalFilePath,
            out string strError)
        {
            strError = "";

            string strUrl = "http://dp2003.com/dp2Circulation/report_def/" + strFileName;

            PathUtil.CreateDirIfNeed(strCfgFileDir);

            strLocalFilePath = Path.Combine(strCfgFileDir, strFileName);
            string strTempFileName = Path.Combine(strCfgFileDir, "~temp_download_webfile");

            if (File.Exists(strLocalFilePath) == true)
            {
#if NO
                DialogResult result = MessageBox.Show(this,
    "本地配置文件 " + strLocalFilePath + " 已经存在，从 dp2003.com 下载同名配置文件会覆盖它。" + "\r\n\r\n确实要下载并覆盖此文件?",
    "ReportForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                {
                    strError = "放弃下载";
                    return -1;
                }
#endif
                return 0;
            }

            int nRet = WebFileDownloadDialog.DownloadWebFile(
                owner,
                strUrl,
                strLocalFilePath,
                strTempFileName,
                out strError);
            if (nRet == -1)
                return -1;
            strError = "下载" + strFileName + "文件成功 :\r\n" + strUrl + " --> " + strLocalFilePath;
            return 0;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // TODO: 这里可以插入保存动作

            // this.Changed = false;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        // 报表 ListView 的栏目下标
        const int COLUMN_REPORT_NAME = 0;
        const int COLUMN_REPORT_FREQ = 1;
        const int COLUMN_REPORT_TYPE = 2;
        const int COLUMN_REPORT_CFGFILE = 3;
        const int COLUMN_REPORT_NAMETABLE = 4;  // 名字表

        public void LoadData(XmlNode nodeLibrary)
        {
            if (nodeLibrary == null)
                return;

            this.comboBox_general_libraryCode.Text = ReportForm.GetDisplayLibraryCode(DomUtil.GetAttr(nodeLibrary, "code"));
            // this.textBox_102_departments.Text = DomUtil.GetAttr(nodeLibrary, "table_102_departments").Replace(",", "\r\n");

            this.listView_reports.Items.Clear();

            XmlNodeList nodes = nodeLibrary.SelectNodes("reports/report");
            if (nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    string strName = DomUtil.GetAttr(node, "name");
                    string strFreq = DomUtil.GetAttr(node, "frequency");

                    string strType = DomUtil.GetAttr(node, "type");
                    string strCfgFile = DomUtil.GetAttr(node, "cfgFile");
                    string strNameTable = DomUtil.GetAttr(node, "nameTable");

                    ListViewItem item = new ListViewItem();
                    ListViewUtil.ChangeItemText(item, COLUMN_REPORT_NAME, strName);
                    ListViewUtil.ChangeItemText(item, COLUMN_REPORT_FREQ, strFreq);
                    ListViewUtil.ChangeItemText(item, COLUMN_REPORT_TYPE, strType);
                    ListViewUtil.ChangeItemText(item, COLUMN_REPORT_CFGFILE, strCfgFile);
                    ListViewUtil.ChangeItemText(item, COLUMN_REPORT_NAMETABLE, strNameTable);

                    this.listView_reports.Items.Add(item);
                }
            }

            this.comboBox_style_htmlTemplate.Text = DomUtil.GetAttr(nodeLibrary, "htmlTemplate");

            this.Changed = false;
        }

        // 将对话框中的数据保存到 XML 结构中
        public void SetData(XmlNode nodeLibrary)
        {
            if (nodeLibrary == null)
                return;

            DomUtil.SetAttr(nodeLibrary, "code", ReportForm.GetOriginLibraryCode(this.comboBox_general_libraryCode.Text));
            // DomUtil.SetAttr(nodeLibrary, "table_102_departments", this.textBox_102_departments.Text.Replace("\r\n", ","));

            XmlNode node_reports = nodeLibrary.SelectSingleNode("reports");
            if (node_reports == null)
            {
                node_reports = nodeLibrary.OwnerDocument.CreateElement("reports");
                nodeLibrary.AppendChild(node_reports);
            }
            else
                node_reports.RemoveAll();

            foreach (ListViewItem item in this.listView_reports.Items)
            {
                string strName = ListViewUtil.GetItemText(item, COLUMN_REPORT_NAME);
                string strFreq = ListViewUtil.GetItemText(item, COLUMN_REPORT_FREQ);
                string strType = ListViewUtil.GetItemText(item, COLUMN_REPORT_TYPE);
                string strCfgFile = ListViewUtil.GetItemText(item, COLUMN_REPORT_CFGFILE);
                string strNameTable = ListViewUtil.GetItemText(item, COLUMN_REPORT_NAMETABLE);

                XmlNode node_report = nodeLibrary.OwnerDocument.CreateElement("report");
                node_reports.AppendChild(node_report);
                DomUtil.SetAttr(node_report, "name", strName);
                DomUtil.SetAttr(node_report, "frequency", strFreq);
                DomUtil.SetAttr(node_report, "type", strType);
                DomUtil.SetAttr(node_report, "cfgFile", strCfgFile);
                DomUtil.SetAttr(node_report, "nameTable", strNameTable);
            }

            DomUtil.SetAttr(nodeLibrary, "htmlTemplate", this.comboBox_style_htmlTemplate.Text);
        }

        public string LibraryCode
        {
            get
            {
                return ReportForm.GetOriginLibraryCode(this.comboBox_general_libraryCode.Text);
            }
            set
            {
                this.comboBox_general_libraryCode.Text = ReportForm.GetDisplayLibraryCode(value);
            }
        }

        // 是否为 修改模式？ 如果为修改模式，则管代码不让修改
        bool _modifyMode = false;


        public bool ModifyMode
        {
            get
            {
                return this._modifyMode;
            }
            set
            {
                this._modifyMode = value;
                if (value == true)
                    this.comboBox_general_libraryCode.Enabled = false;
                else
                    this.comboBox_general_libraryCode.Enabled = true;
            }
        }

        public ReportForm ReportForm = null;

#if NO
        private void button_102_importDepartments_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.ReportForm == null)
                return;

            List<string> results = null;
                    // 获得一个分馆内读者记录的所有单位名称
            nRet = this.ReportForm.GetAllReaderDepartments(
                this.comboBox_general_libraryCode.Text,
                out results,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_102_departments.Text = StringUtil.MakePathList(results, "\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif

        private void listView_reports_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("修改 (&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyReport_Click);
            if (this.listView_reports.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建 (&C)");
            menuItem.Click += new System.EventHandler(this.menu_newReport_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("删除 [" + this.listView_reports.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_deleteReport_Click);
            if (this.listView_reports.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_reports, new Point(e.X, e.Y));		

        }

        void menu_modifyReport_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_reports.SelectedItems.Count == 0)
            {
                strError = "尚未选定要修改的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_reports.SelectedItems[0];

            ReportApplyForm dlg = new ReportApplyForm();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            dlg.ReportForm = this.ReportForm;
            dlg.LibraryCode = ReportForm.GetOriginLibraryCode(this.comboBox_general_libraryCode.Text);
            dlg.CfgFileDir = Path.Combine(this.MainForm.UserDir, "report_def"); //  Path.Combine(this.MainForm.UserDir, "report_def");
            dlg.ReportName = ListViewUtil.GetItemText(item, COLUMN_REPORT_NAME);
            dlg.Freguency = ListViewUtil.GetItemText(item, COLUMN_REPORT_FREQ);
            dlg.ReportType = ListViewUtil.GetItemText(item, COLUMN_REPORT_TYPE);
            dlg.ReportCfgFileName = ListViewUtil.GetItemText(item, COLUMN_REPORT_CFGFILE);
            dlg.NameTable = ListViewUtil.GetItemText(item, COLUMN_REPORT_NAMETABLE);

            this.MainForm.AppInfo.LinkFormState(dlg, "ReportApplyForm_state");
            dlg.UiState = this.MainForm.AppInfo.GetString("libraryreportconfig_form", "reportapplyform_ui_state", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString("libraryreportconfig_form", "reportapplyform_ui_state", dlg.UiState);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewUtil.ChangeItemText(item, COLUMN_REPORT_NAME, dlg.ReportName);
            ListViewUtil.ChangeItemText(item, COLUMN_REPORT_FREQ, dlg.Freguency);
            ListViewUtil.ChangeItemText(item, COLUMN_REPORT_TYPE, dlg.ReportType);
            ListViewUtil.ChangeItemText(item, COLUMN_REPORT_CFGFILE, dlg.ReportCfgFileName);
            ListViewUtil.ChangeItemText(item, COLUMN_REPORT_NAMETABLE, dlg.NameTable);

            this.Changed = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_newReport_Click(object sender, EventArgs e)
        {
            ReportApplyForm dlg = new ReportApplyForm();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            dlg.ReportForm = this.ReportForm;
            dlg.LibraryCode = ReportForm.GetOriginLibraryCode(this.comboBox_general_libraryCode.Text);
            dlg.CfgFileDir = Path.Combine(this.MainForm.UserDir, "report_def"); //  Path.Combine(this.MainForm.UserDir, "report_def");
        REDO_INPUT:
            this.MainForm.AppInfo.LinkFormState(dlg, "ReportApplyForm_state");
            dlg.UiState = this.MainForm.AppInfo.GetString("libraryreportconfig_form", "reportapplyform_ui_state", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString("libraryreportconfig_form", "reportapplyform_ui_state", dlg.UiState);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            // 对报表文件名进行查重
            ListViewItem dup = ListViewUtil.FindItem(this.listView_reports, dlg.ReportName, COLUMN_REPORT_NAME);
            if (dup != null)
            {
                ListViewUtil.SelectLine(dup, true);
                MessageBox.Show(this, "报表名为 '" + dlg.ReportName + "' 的事项已经存在，不允许重复创建。请修改报表名");
                goto REDO_INPUT;
            }

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, COLUMN_REPORT_NAME, dlg.ReportName);
            ListViewUtil.ChangeItemText(item, COLUMN_REPORT_FREQ, dlg.Freguency);
            ListViewUtil.ChangeItemText(item, COLUMN_REPORT_TYPE, dlg.ReportType);
            ListViewUtil.ChangeItemText(item, COLUMN_REPORT_CFGFILE, dlg.ReportCfgFileName);
            ListViewUtil.ChangeItemText(item, COLUMN_REPORT_NAMETABLE, dlg.NameTable);
            this.listView_reports.Items.Add(item);

            ListViewUtil.SelectLine(item, true);

            this.Changed = true;
        }

        void menu_deleteReport_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_reports.SelectedItems.Count == 0)
            {
                strError = "尚未选定要删除的事项";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
"确实要删除选定的 " + this.listView_reports.SelectedItems.Count.ToString() + " 个事项?",
"LibraryReportCfgForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;
            ListViewUtil.DeleteSelectedItems(this.listView_reports);

            this.Changed = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_reports_DoubleClick(object sender, EventArgs e)
        {
            menu_modifyReport_Click(sender, e);
        }

        private void comboBox_general_libraryCode_TextChanged(object sender, EventArgs e)
        {
            this.Changed = true;
        }

        private void comboBox_style_htmlTemplate_TextChanged(object sender, EventArgs e)
        {
            this.Changed = true;
        }

        private void LibraryReportConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Changed == true
                && this.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前对话框有信息被修改。若关闭窗口或者取消修改，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "LibraryReportConfigForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        /// <summary>
        /// 获取或设置控件尺寸状态
        /// </summary>
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl1);
                controls.Add(this.listView_reports);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl1);
                controls.Add(this.listView_reports);
                GuiState.SetUiState(controls, value);
            }
        }
    }
}
