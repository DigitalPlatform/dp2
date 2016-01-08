using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using System.Collections;

namespace dp2Circulation
{
    /// <summary>
    /// 为一个分馆应用某类统计报表的对话框
    /// 从已有的报表配置模板中选择，配用
    /// </summary>
    public partial class ReportApplyForm : Form
    {
        /// <summary>
        /// 框架窗口对象
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 用于存储报表配置文件的目录
        /// </summary>
        public string CfgFileDir = "";  // 用于存储报表配置文件的目录

        /// <summary>
        /// 馆代码
        /// </summary>
        public string LibraryCode = ""; // 馆代码

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReportApplyForm()
        {
            InitializeComponent();
        }

        private void ReportApplyForm_Load(object sender, EventArgs e)
        {
            // 确保目录已经存在
            if (string.IsNullOrEmpty(this.CfgFileDir) == false)
                PathUtil.CreateDirIfNeed(this.CfgFileDir);

            SetButtonState();

            FillReportTypeList();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.textBox_reportName.Text) == true)
            {
                this.tabControl_main.SelectedTab = this.tabPage_normal;
                strError = "请指定报表名";
                goto ERROR1;
            }

            string strNumber = this.GetTypeNumber();
            if (string.IsNullOrEmpty(strNumber) == true)
            {
                this.tabControl_main.SelectedTab = this.tabPage_normal;
                strError = "请指定报表类型";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_cfgFileName.Text) == true)
            {
                this.tabControl_main.SelectedTab = this.tabPage_normal;
                strError = "请指定报表配置文件";
                goto ERROR1;
            }

            if (File.Exists(this.textBox_cfgFileName.Text) == false)
            {
                this.tabControl_main.SelectedTab = this.tabPage_normal;
                strError = "报表配置文件 " + this.textBox_cfgFileName.Text + " 尚未创建。请从 dp2003.com 下载此配置文件，或手动创建";
                goto ERROR1;
            }

            if (strNumber == "102"
                && string.IsNullOrEmpty(this.textBox_nameTable_strings.Text) == true)
            {
                this.tabControl_main.SelectedTab = this.tabPage_nameTable;
                strError = "类型为 102 报表必须配置名字列表内容";
                goto ERROR1;
            }


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

        /// <summary>
        /// 报表名
        /// </summary>
        public string ReportName
        {
            get
            {
                return this.textBox_reportName.Text;
            }
            set
            {
                this.textBox_reportName.Text = value;
            }
        }

        /// <summary>
        /// 报表类型。三个数字的字符串
        /// </summary>
        public string ReportType
        {
            get
            {
                // return this.comboBox_reportType.Text;
                return this.GetTypeNumber();
            }
            set
            {
                this.comboBox_reportType.Text = value;
            }
        }

        /// <summary>
        /// 报表配置文件名
        /// </summary>
        public string ReportCfgFileName
        {
            get
            {
                return this.textBox_cfgFileName.Text;
            }
            set
            {
                this.textBox_cfgFileName.Text = value;
            }
        }

        private void button_download_templateCfgFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strNumber = this.GetTypeNumber();

            if (string.IsNullOrEmpty(strNumber) == true)
            {
                strError = "请先指定报表类型，才能下载";
                goto ERROR1;
            }

            string strFileName = strNumber + ".xml";
            string strLocalFilePath = "";

            int nRet = DownloadDataFile(strFileName,
                out strLocalFilePath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // string strLocalFileName = Path.Combine(this.CfgFileDir, strFileName);
            this.textBox_cfgFileName.Text = strLocalFilePath;

            AutoFillReportName();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 下载数据文件
        /// 从 http://dp2003.com/dp2Circulation/report_def 位置下载到 CfgFileDir，文件名保持不变
        /// </summary>
        /// <param name="strFileName">纯文件名</param>
        /// <param name="strLocalFilePath">返回本地文件名全路径</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        int DownloadDataFile(string strFileName,
            out string strLocalFilePath,
            out string strError)
        {
            strError = "";


            string strUrl = "http://dp2003.com/dp2Circulation/report_def/" + strFileName;

            PathUtil.CreateDirIfNeed(this.CfgFileDir);
            strLocalFilePath = Path.Combine(this.CfgFileDir, strFileName);
            string strTempFileName = Path.Combine(this.CfgFileDir, "~temp_download_webfile");

            if (File.Exists(strLocalFilePath) == true)
            {
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
            }

            int nRet = WebFileDownloadDialog.DownloadWebFile(
                this,
                strUrl,
                strLocalFilePath,
                strTempFileName,
                out strError);
            if (nRet == -1)
                return -1;
            strError = "下载" + strFileName + "文件成功 :\r\n" + strUrl + " --> " + strLocalFilePath;
            return 0;
        }

        private void button_editCfgFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strLocalFileName = this.textBox_cfgFileName.Text;
            if (string.IsNullOrEmpty(strLocalFileName) == false)
            {
            }
            else
            {
                string strNumber = this.GetTypeNumber();
                if (string.IsNullOrEmpty(strNumber) == true)
                {
                    strError = "请先指定报表类型，才能创建新的配置文件";
                    goto ERROR1;
                }

                string strFileName = strNumber + ".xml";
                strLocalFileName = Path.Combine(this.CfgFileDir, strFileName);
            }

            ReportDefForm dlg = new ReportDefForm();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.AppInfo = this.MainForm.AppInfo;

            this.MainForm.AppInfo.LinkFormState(dlg, "ReportDefForm_state");
            dlg.CfgFileName = strLocalFileName;
            dlg.UiState = this.MainForm.AppInfo.GetString("reportapply_form", "reportdefform_ui_state", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString("reportapply_form", "reportdefform_ui_state", dlg.UiState);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_cfgFileName.Text = strLocalFileName;
            AutoFillReportName();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void checkedComboBox_createFreq_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_createFreq.Items.Count > 0)
                return;

            this.checkedComboBox_createFreq.Items.Add("day");
            this.checkedComboBox_createFreq.Items.Add("month");
            this.checkedComboBox_createFreq.Items.Add("year");
        }

        /// <summary>
        /// 创建频率。例如 day,month,year
        /// </summary>
        public string Freguency
        {
            get
            {
                return this.checkedComboBox_createFreq.Text;
            }
            set
            {
                this.checkedComboBox_createFreq.Text = value;
            }
        }

        /// <summary>
        /// ReportForm 对象
        /// </summary>
        public ReportForm ReportForm = null;

        private void button_nameTable_importStrings_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.ReportForm == null)
            {
                strError = "this.ReportForm == null";
                goto ERROR1;
            }

            string strNumber = this.GetTypeNumber();

            if (strNumber == "102")
            {
                List<string> results = null;
                // 获得一个分馆内读者记录的所有单位名称
                nRet = this.ReportForm.GetAllReaderDepartments(
                    this.LibraryCode,
                    out results,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_nameTable_strings.Text = StringUtil.MakePathList(results, "\r\n");
            }
            else if (strNumber == "212"
                || strNumber == "213"
                || strNumber == "301"
                || strNumber == "302"
                || strNumber == "493")
            {
                List<string> results = null;
                // 获得所有的分类号 style
                nRet = this.ReportForm.GetClassFromStylesFromFile(
                    out results,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_nameTable_strings.Text = StringUtil.MakePathList(results, "\r\n");
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 名字表
        // 逗号分隔的字符串
        /// <summary>
        /// 名字表。逗号分隔的字符串
        /// </summary>
        public string NameTable
        {
            get
            {
                return this.textBox_nameTable_strings.Text.Replace("\r\n", ",");
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                    this.textBox_nameTable_strings.Text = "";
                else
                    this.textBox_nameTable_strings.Text = value.Replace(",", "\r\n");
            }
        }

        private void comboBox_reportType_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        void SetButtonState()
        {
            if (string.IsNullOrEmpty(this.comboBox_reportType.Text) == true)
                this.button_download_templateCfgFile.Enabled = false;
            else
                this.button_download_templateCfgFile.Enabled = true;

            if (string.IsNullOrEmpty(this.textBox_cfgFileName.Text) == true)
            {
                this.button_editCfgFile.Enabled = false;
            }
            else
            {
                if (File.Exists(this.textBox_cfgFileName.Text) == true)
                    this.button_editCfgFile.Enabled = true;
                else
                    this.button_editCfgFile.Enabled = false;
            }
        }

        string GetTypeNumber()
        {
            string strNumber = "";
            string strText = "";
            StringUtil.ParseTwoPart(this.comboBox_reportType.Text, "\t", out strNumber, out strText);
            return strNumber;
        }

        private void comboBox_reportType_TextChanged(object sender, EventArgs e)
        {
            this.textBox_cfgFileName.Text = "";
            string strNumber = this.GetTypeNumber();
            if (string.IsNullOrEmpty(strNumber) == false)
            {
                string strFileName = strNumber + ".xml";
                this.textBox_cfgFileName.Text = Path.Combine(this.CfgFileDir, strFileName);
            }

            SetButtonState();
            AutoFillReportName();

            // 名字表是否显示
            {
                // string strNumber = this.GetTypeNumber();

                if (strNumber == "212"
                    || strNumber == "213"
                    || strNumber == "301"
                    || strNumber == "302"
                    || strNumber == "493"
                    )
                {
                    this.label_nameTable_strings.Text = "分类号名列表 [每行一个名称]";
                    this.textBox_nameTable_strings.Enabled = true;
                    this.button_nameTable_importStrings.Enabled = true;
                }
                else if (strNumber == "102")
                {
                    this.label_nameTable_strings.Text = "部门名称列表 [每行一个名称]";
                    this.textBox_nameTable_strings.Enabled = true;
                    this.button_nameTable_importStrings.Enabled = true;
                }
                else
                {
                    this.textBox_nameTable_strings.Enabled = false;
                    this.button_nameTable_importStrings.Enabled = false;
                }
            }
        }

        // 自动填充面板上的 报表名和创建频率
        void AutoFillReportName()
        {
#if NO
            // 用配置文件中的 typeName 填充报表名称
            if (string.IsNullOrEmpty(this.textBox_reportName.Text) == false)
                return;
#endif

            string strError = "";

            string strLocalFilePath = this.textBox_cfgFileName.Text;

            if (string.IsNullOrEmpty(strLocalFilePath) == true)
                return;
            if (File.Exists(strLocalFilePath) == false)
                return;

#if NO
            string strTypeName = "";
            // 获得 typeName
            // return:
            //      -1  出错
            //      0   配置文件没有找到
            //      1   成功
            int nRet = ReportDefForm.GetReportTypeName(
                strLocalFilePath,
                out strTypeName,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
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
                goto ERROR1;

            if (string.IsNullOrEmpty(config.TypeName) == false
                && config.TypeName.Length > 3)
                this.textBox_reportName.Text = config.TypeName.Substring(GetNumberPrefix(config.TypeName).Length).Trim();

            if (string.IsNullOrEmpty(this.checkedComboBox_createFreq.Text) == true)
                this.checkedComboBox_createFreq.Text = config.CreateFreq;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得一个字符串前面的数字部分。例如 "101 xxxx" "9101 xxxxxxx"
        static string GetNumberPrefix(string strText)
        {
            StringBuilder result = new StringBuilder();
            foreach(char ch in strText)
            {
                if (char.IsDigit(ch) == false)
                    return result.ToString();
                result.Append(ch);
            }

            return result.ToString();
        }

        private void textBox_cfgFileName_TextChanged(object sender, EventArgs e)
        {
        }

        void FillReportTypeList()
        {
            if (this.comboBox_reportType.Items.Count > 0)
                return;

            string strCfgDir = Path.Combine(this.MainForm.UserDir, "report_def");
            DirectoryInfo di = new DirectoryInfo(strCfgDir);

            List<FileInfo> array = new List<FileInfo>();
            array.AddRange( di.GetFiles("???.xml"));
            array.AddRange( di.GetFiles("????.xml"));
            FileInfo[] fis = array.ToArray();
            Array.Sort(fis, new FileInfoCompare());

            foreach (FileInfo fi in fis)
            {
                ReportConfigStruct config = null;

                string strError = "";
                string strName = "";
                // 从报表配置文件中获得各种配置信息
                // return:
                //      -1  出错
                //      0   没有找到配置文件
                //      1   成功
                int nRet = ReportDefForm.GetReportConfig(fi.FullName,
                    out config,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    strName = strError;
                else
                {
                    string strNumber = "";
                    StringUtil.ParseTwoPart(config.TypeName, " ", out strNumber, out strName);
                }

                this.comboBox_reportType.Items.Add(Path.GetFileNameWithoutExtension(fi.Name) + "\t" + strName);
            }
        }

        class FileInfoCompare : IComparer
        {
            int IComparer.Compare(Object x, Object y)
            {
                return ((new CaseInsensitiveComparer()).Compare(((FileInfo)x).Name, ((FileInfo)y).Name));
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
                controls.Add(this.tabControl_main);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                GuiState.SetUiState(controls, value);
            }
        }
    }
}
