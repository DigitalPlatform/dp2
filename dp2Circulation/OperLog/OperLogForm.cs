
#define DELAY_UPDATE

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Range;
using DigitalPlatform.Text;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;

using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 日志窗。用于观察操作日志
    /// </summary>
    public partial class OperLogForm : MyForm
    {
        WebExternalHost m_webExternalHost = null;
        /// <summary>
        /// 获得摘要
        /// </summary>
        event GetSummaryEventHandler GetSummary = null;

#if NO
        const string SubFieldChar = "‡";
        const string FieldEndChar = "¶";
#endif

        const string NOTSUPPORT = "<html><body><p>[暂不支持]</p></body></html>";

        CommentViewerForm m_operlogViewer = null;

        // int m_nInFilling = 0;
        // 栏目下标
        const int COLUMN_FILENAME = 0;
        const int COLUMN_INDEX = 1;
        const int COLUMN_LIBRARYCODE = 2;
        const int COLUMN_OPERATION = 3;
        const int COLUMN_OPERATOR = 4;
        const int COLUMN_OPERTIME = 5;
        const int COLUMN_ATTACHMENT = 6;

        bool StoreInTempFile = false;

        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();

#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;
#endif

        // 临时文件名集合
        List<string> m_tempFileNames = new List<string>();

        // const int WM_SETCARETPOS = API.WM_USER + 201;

        /// <summary>
        /// 构造函数
        /// </summary>
        public OperLogForm()
        {
            InitializeComponent();
        }

        private void OperLogForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

#if NO
            MainForm.AppInfo.LoadMdiChildFormStates(this,
    "mdi_form_state");


            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.AcceptButton = this.button_loadFromSingleFile;

            this.textBox_logFileName.Text = MainForm.AppInfo.GetString(
                "operlogform",
                "logfilename",
                "");

            this.textBox_repair_sourceFilename.Text = MainForm.AppInfo.GetString(
                "operlogform",
                "repair_source_filename",
                "");
            this.textBox_repair_targetFilename.Text = MainForm.AppInfo.GetString(
                "operlogform",
                "repair_target_filename",
                "");
            this.textBox_repair_verifyFolderName.Text = MainForm.AppInfo.GetString(
                "operlogform",
                "repair_verify_foldername",
                "");

            this.Channels = new LibraryChannelCollection();
            this.Channels.BeforeLogin -= new BeforeLoginEventHandle(Channels_BeforeLogin);
            this.Channels.BeforeLogin += new BeforeLoginEventHandle(Channels_BeforeLogin);

            /*
            if (this.MainForm.CanDisplayItemProperty() == true)
                DownPannelVisible = false;
             * */
            DownPannelVisible = false;  // 必须隐藏。因为放开后有 javascript 没有连接 host 等问题
        }

        /// <summary>
        /// 下方面板是否可见
        /// </summary>
        public bool DownPannelVisible
        {
            get
            {
                if (this.splitContainer_logRecords.Visible == true)
                    return true;
                return false;
            }
            set
            {
                if (value == true)
                {
                    this.tabPage_logRecords.Controls.Remove(this.listView_records);

                    this.splitContainer_logRecords.Panel1.Controls.Add(this.listView_records);
                    this.splitContainer_logRecords.Visible = true;
                }
                else
                {
                    this.splitContainer_logRecords.Panel1.Controls.Remove(this.listView_records);
                    this.splitContainer_logRecords.Visible = false;

                    this.tabPage_logRecords.Controls.Add(this.listView_records);
                }

                this.listView_records.ForceUpdate();
            }
        }

        void Channels_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            this.MainForm.Channel_BeforeLogin(this, e);
        }

        private void OperLogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }

            }
#endif
        }

        private void OperLogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
            if (this.m_operlogViewer != null)
            {
                this.m_operlogViewer.ExitWebBrowser();  // 虽然 CommentViwerForm 的 Dispose() 里面也作了释放，但为了保险起见，这里也释放一次
                this.m_operlogViewer.Close();
            }

            if (this.m_webExternalHost != null)
                this.m_webExternalHost.Destroy();

            ClearAllTempFiles();

            MainForm.AppInfo.SetString(
                "operlogform",
                "logfilename",
                this.textBox_logFileName.Text);

            MainForm.AppInfo.SetString(
                "operlogform",
                "repair_source_filename",
                this.textBox_repair_sourceFilename.Text);
            MainForm.AppInfo.SetString(
                "operlogform",
                "repair_target_filename",
                this.textBox_repair_targetFilename.Text);
            MainForm.AppInfo.SetString(
                "operlogform",
                "repair_verify_foldername",
                this.textBox_repair_verifyFolderName.Text);
#if NO
            MainForm.AppInfo.SaveMdiChildFormStates(this,
    "mdi_form_state");
#endif

        }

#if NO
        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            this.MainForm.Channel_BeforeLogin(this, e);
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

        private void button_loadFromSingleFile_Click(object sender, EventArgs e)
        {
            if (this.textBox_logFileName.Text == "")
            {
                MessageBox.Show(this, "尚未指定日志文件名");
                return;
            }
            this.LoadRecordsFromSingleLogFile(this.textBox_logFileName.Text);
        }

        // 删除所有临时文件
        void ClearAllTempFiles()
        {
            for (int i = 0; i < m_tempFileNames.Count; i++)
            {
                File.Delete(m_tempFileNames[i]);
            }

            this.m_tempFileNames.Clear();
        }

        // 
        /// <summary>
        /// 清除窗口内全部内容
        /// </summary>
        public void Clear()
        {
            this.listView_records.Items.Clear();
            this.listView_records.Update();

            this.SortColumns.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_records.Columns);

            if (this.m_operlogViewer != null)
                this.m_operlogViewer.Clear();

            this.ClearAllTempFiles();

            Global.ClearHtmlPage(this.webBrowser_xml,
    this.MainForm.DataDir);
            Global.ClearHtmlPage(this.webBrowser_html,
                this.MainForm.DataDir);
        }

        // 
        /// <summary>
        /// 装入一个日志文件中的全部记录
        /// </summary>
        /// <param name="strLogFileName">日志文件名。纯文件名</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int LoadRecordsFromSingleLogFile(string strLogFileName)
        {
            string strError = "";

            this.tabControl_main.SelectedTab = this.tabPage_logRecords;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在初始化浏览器组件 ...");
            stop.BeginLoop();


            this.Update();
            this.MainForm.Update();

#if NO
            this.listView_records.Items.Clear();
            // 2008/11/22 new add
            this.SortColumns.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_records.Columns);

            this.ClearAllTempFiles();

            Global.ClearHtmlPage(this.webBrowser_xml,
                this.MainForm.DataDir);
            Global.ClearHtmlPage(this.webBrowser_html,
                this.MainForm.DataDir);
#endif
            this.Clear();

            stop.SetMessage("正在装入日志文件 " + strLogFileName + " 中的记录...");
#if DELAY_UPDATE
            this.listView_records.BeginUpdate();
#endif
            try
            {
                List<string> lines = new List<string>();
                lines.Add(strLogFileName);

                string strStyle = "";
                if (this.MainForm.AutoCacheOperlogFile == true)
                    strStyle = "autocache";

                int nRet = ProcessFiles(this,
    stop,
    this.estimate,
    Channel,
    lines,
    this.MainForm.OperLogLevel,
    strStyle,
    this.MainForm.OperLogCacheDir,
    null,   // param,
    DoRecord,
    out strError);
                if (nRet == -1)
                    goto ERROR1;

            }
            finally
            {
#if DELAY_UPDATE
                    this.listView_records.EndUpdate();
#endif
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }

            return 1;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.comboBox_quickSetFilenames.Enabled = bEnable;

            this.textBox_logFileName.Enabled = bEnable;

            this.button_loadFromSingleFile.Enabled = bEnable;

            // 
            if (String.IsNullOrEmpty(this.textBox_filenames.Text) == true)
                this.button_loadLogRecords.Enabled = false;
            else
                this.button_loadLogRecords.Enabled = bEnable;

            this.button_loadFilenams.Enabled = bEnable;

            this.textBox_filenames.Enabled = bEnable;

            // this.splitContainer_logRecords.Enabled = bEnable;

            // repair
            this.button_repair_findSourceFilename.Enabled = bEnable;
            this.button_repair_findTargetFilename.Enabled = bEnable;
            this.button_repair_repair.Enabled = bEnable;

            this.textBox_repair_sourceFilename.Enabled = bEnable;
            this.textBox_repair_targetFilename.Enabled = bEnable;

            this.textBox_repair_verifyFolderName.Enabled = bEnable;
            this.button_repair_findVerifyFolderName.Enabled = bEnable;
            this.button_repair_verify.Enabled = bEnable;
        }

        #region HTML 解释日志记录

        // 创建解释日志记录内容的 HTML 字符串
        // return:
        //      -1  出错
        //      0   成功
        //      1   未知的操作类型
        int GetHtmlString(string strXml,
            bool bPageWrap,
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "日志记录XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            if (strOperation == "borrow")
                nRet = GetBorrowString(dom, out strHtml, out strError);
            else if (strOperation == "return")
                nRet = GetReturnString(dom, out strHtml, out strError);
            else if (strOperation == "changeReaderPassword")
                nRet = GetChangeReaderPasswordString(dom, out strHtml, out strError);
            else if (strOperation == "setReaderInfo")
                nRet = GetSetReaderInfoString(dom, out strHtml, out strError);
            else if (strOperation == "reservation")
                nRet = GetReservationString(dom, out strHtml, out strError);
            else if (strOperation == "amerce")
                nRet = GetAmerceString(dom, out strHtml, out strError);
            else if (strOperation == "devolveReaderInfo")
                nRet = GetDevoleveReaderInfoString(dom, out strHtml, out strError);
            else if (strOperation == "repairBorrowInfo")
                nRet = GetRepairBorrowInfoString(dom, out strHtml, out strError);
            else if (strOperation == "writeRes")
                nRet = GetWriteResString(dom, out strHtml, out strError);
            else if (strOperation == "setBiblioInfo")
                nRet = GetSetBiblioInfoString(dom, out strHtml, out strError);
            else if (strOperation == "hire")
                nRet = GetHireString(dom, out strHtml, out strError);
            else if (strOperation == "foregift")
                nRet = GetForegiftString(dom, out strHtml, out strError);
            else if (strOperation == "settlement")
                nRet = GetSettlementString(dom, out strHtml, out strError);
            else if (strOperation == "passgate")
                nRet = GetPassGateString(dom, out strHtml, out strError);
            else if (strOperation == "setEntity"
                || strOperation == "setOrder"
                || strOperation == "setIssue"
                || strOperation == "setComment")
                nRet = GetSetEntityString(dom, out strHtml, out strError);
            else if (strOperation == "setUser")
                nRet = GetSetUserString(dom, out strHtml, out strError);
            else
            {
                strError = "未知的操作类型 '"+strOperation+"'";
                return 1;
            }

            if (bPageWrap == true)
            {
                if (string.IsNullOrEmpty(strHtml) == true)
                    return 0;
                strHtml = "<html>" +
                    GetHeadString() +
                    "<body>" +
                    strHtml +
                    "</body></html>";
            }

            return 0;
        }

        // SetReaderInfo
        int GetSetReaderInfoString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");

            string strRecord = DomUtil.GetElementText(dom.DocumentElement, "record",out node);
            string strRecPath = "";
            string strRecordHtml = "";
            if (node != null)
            {
                strRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetReaderInfoString(
                    this.DisplayReaderBorrowHistory,
                    strRecord,
                    out strRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOldRecord = DomUtil.GetElementText(dom.DocumentElement, "oldRecord", out node);
            string strOldRecPath = "";
            string strOldRecordHtml = "";
            if (node != null)
            {
                strOldRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetReaderInfoString(
                    this.DisplayReaderBorrowHistory,
                    strOldRecord,
                    out strOldRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 设置读者记录") +

                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +

                BuildHtmlEncodedLine("新读者记录", strRecPath, strRecordHtml) +
                BuildHtmlEncodedLine("旧读者记录", strOldRecPath, strOldRecordHtml) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        string GetHeadString(bool bAjax = true)
        {
            string strCssFilePath = PathUtil.MergePath(this.MainForm.DataDir, "operloghtml.css");

            if (bAjax == true)
                return
                    "<head>" +
                    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
                    "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>" +
                    //"<script type='text/javascript' src='%datadir%/jquery.js'></script>" +
                    "<script type='text/javascript' charset='UTF-8' src='%datadir%\\getsummary.js" + "'></script>" +
                    "</head>";
            return
    "<head>" +
    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
    "</head>";

        }

        /// <summary>
        /// 构造册记录锚点
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <returns>锚点字符串</returns>
        public static string BuildItemBarcodeLink(string strItemBarcode)
        {
            if (string.IsNullOrEmpty(strItemBarcode) == true)
                return "";
            return "<a href='javascript:void(0);' onclick=\"try { window.external.OpenForm('ItemInfoForm', this.innerText, true);} catch(e) {}\">" + strItemBarcode + "</a>";
        }

        /// <summary>
        /// 构造读者记录锚点
        /// </summary>
        /// <param name="strReaderBarcode">读者证条码号</param>
        /// <returns>冒点字符串</returns>
        public static string BuildReaderBarcodeLink(string strReaderBarcode)
        {
            if (string.IsNullOrEmpty(strReaderBarcode) == true)
                return "";
            return "<a href='javascript:void(0);' onclick=\"try { window.external.OpenForm('ReaderInfoForm', this.innerText, true);} catch(e) {}\">" + strReaderBarcode + "</a>";
        }

        string BuildPendingBiblioSummary(string strItemBarcode,
            string strItemRecPath)
        {
            if (string.IsNullOrEmpty(strItemBarcode) == true)
                return "";
            string strCommand = "B:" + strItemBarcode + "|" + strItemRecPath;
            if (this.GetSummary == null)
                return strCommand;

            GetSummaryEventArgs e = new GetSummaryEventArgs();
            e.Command = strCommand;
            this.GetSummary(this, e);
            return e.Summary;
        }

        string BuildPendingReaderSummary(string strReaderBarcode)
        {
            if (string.IsNullOrEmpty(strReaderBarcode) == true)
                return "";
            string strCommand = "P:" + strReaderBarcode;
            if (this.GetSummary == null)
                return strCommand;

            GetSummaryEventArgs e = new GetSummaryEventArgs();
            e.Command = strCommand;
            this.GetSummary(this, e);
            return e.Summary;
        }

        // Borrow
        int GetBorrowString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "itemBarcode");

            string strConfirmItemRecPath = DomUtil.GetElementText(dom.DocumentElement, "confirmItemRecPath");
            string strBorrowDate = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "borrowDate"));
            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement, "borrowPeriod");
            string strNo = DomUtil.GetElementText(dom.DocumentElement, "no");

            string strReaderRecord = DomUtil.GetElementText(dom.DocumentElement, "readerRecord", out node);
            string strReaderRecPath = "";
            string strReaderRecordHtml = "";
            if (node != null)
            {
                strReaderRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetReaderInfoString(
                    this.DisplayReaderBorrowHistory,
                    strReaderRecord,
                    out strReaderRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strItemRecord = DomUtil.GetElementText(dom.DocumentElement, "itemRecord", out node);
            string strItemRecPath = "";
            string strItemRecordHtml = "";
            if (node != null)
            {
                strItemRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetItemInfoString(
                    this.DisplayItemBorrowHistory,
                    strItemRecord,
                    strItemRecPath,
                    out strItemRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            //string strItemBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\">" + strItemBarcode + "</a>";
            //string strReaderBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + strReaderBarcode + "</a>";

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- " + (string.IsNullOrEmpty(strNo) == true || strNo == "0" ? "借书" : "续借")) +
                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +
                BuildHtmlEncodedLine("读者证条码号", BuildReaderBarcodeLink(strReaderBarcode)) +
                BuildHtmlPendingLine("(读者摘要)", BuildPendingReaderSummary(strReaderBarcode)) +
                BuildHtmlEncodedLine("册条码号", BuildItemBarcodeLink(strItemBarcode)) +
                BuildHtmlPendingLine("(书目摘要)", BuildPendingBiblioSummary(strItemBarcode, strConfirmItemRecPath)) +
                BuildHtmlLine("册记录路径", strConfirmItemRecPath) +
                BuildHtmlLine("借阅日期", strBorrowDate) +
                BuildHtmlLine("借阅期限", strBorrowPeriod) +
                BuildHtmlLine("续借次数", strNo) +

                BuildHtmlEncodedLine("册记录", strItemRecPath, strItemRecordHtml) +
                BuildHtmlEncodedLine("读者记录", strReaderRecPath, strReaderRecordHtml) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // Return
        int GetReturnString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "itemBarcode");

            string strConfirmItemRecPath = DomUtil.GetElementText(dom.DocumentElement, "confirmItemRecPath");

            string strReaderRecord = DomUtil.GetElementText(dom.DocumentElement, "readerRecord", out node);
            string strReaderRecPath = "";
            string strReaderRecordHtml = "";
            if (node != null)
            {
                strReaderRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetReaderInfoString(
                        this.DisplayReaderBorrowHistory,
                        strReaderRecord,
                        out strReaderRecordHtml,
                        out strError);
                if (nRet == -1)
                    return -1;
            }

            string strItemRecord = DomUtil.GetElementText(dom.DocumentElement, "itemRecord", out node);
            string strItemRecPath = "";
            string strItemRecordHtml = "";
            if (node != null)
            {
                strItemRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetItemInfoString(
                    this.DisplayItemBorrowHistory,
                    strItemRecord,
                    strItemRecPath,
                    out strItemRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOverdues = FormatInnerXml(
    DomUtil.GetElementText(dom.DocumentElement, "overdues"));
            string strLostComment = DomUtil.GetElementText(dom.DocumentElement, "lostComment");

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            //string strItemBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\">" + strItemBarcode + "</a>";
            //string strReaderBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + strReaderBarcode + "</a>";

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 还书(声明丢失)") +
                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +
                BuildHtmlEncodedLine("读者证条码号", BuildReaderBarcodeLink(strReaderBarcode)) +
                BuildHtmlPendingLine("(读者摘要)", BuildPendingReaderSummary(strReaderBarcode)) +
                BuildHtmlEncodedLine("册条码号", BuildItemBarcodeLink(strItemBarcode)) +
                BuildHtmlPendingLine("(书目摘要)", BuildPendingBiblioSummary(strItemBarcode,strConfirmItemRecPath)) +
                BuildHtmlLine("册记录路径", strConfirmItemRecPath) +

                BuildHtmlLine("丢失情况附注", strLostComment) +
                BuildHtmlEncodedLine("待交费信息", strOverdues) +

                BuildHtmlEncodedLine("册记录", strItemRecPath, strItemRecordHtml) +
                BuildHtmlEncodedLine("读者记录", strReaderRecPath, strReaderRecordHtml) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // ChangeReaderPassword
        int GetChangeReaderPasswordString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
            string strNewPassword = DomUtil.GetElementText(dom.DocumentElement, "newPassword");

            string strReaderRecord = DomUtil.GetElementText(dom.DocumentElement, "readerRecord", out node);
            string strReaderRecPath = "";
            string strReaderRecordHtml = "";
            if (node != null)
            {
                strReaderRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetReaderInfoString(
                    this.DisplayReaderBorrowHistory,
                    strReaderRecord,
                    out strReaderRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            // string strReaderBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + strReaderBarcode + "</a>";

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 修改读者密码") +
                BuildHtmlEncodedLine("读者证条码号", BuildReaderBarcodeLink(strReaderBarcode)) +
                BuildHtmlPendingLine("(读者摘要)", BuildPendingReaderSummary(strReaderBarcode)) +

                BuildHtmlLine("新的密码", strNewPassword) +

                BuildHtmlEncodedLine("读者记录", strReaderRecPath, strReaderRecordHtml) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // Reservation
        int GetReservationString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            // int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
            string strItemBarcodeList = DomUtil.GetElementText(dom.DocumentElement, "itemBarcodeList");

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            string strItemBarcodeLink = "";
            List<string> itembarcodes = StringUtil.FromListString(strItemBarcodeList);
            foreach (string strItemBarcode in itembarcodes)
            {
                if (string.IsNullOrEmpty(strItemBarcodeLink) == false)
                    strItemBarcodeLink += "<br/>";
                strItemBarcodeLink += BuildItemBarcodeLink(strItemBarcode) + " <span class='summary pending'>" + BuildPendingBiblioSummary(strItemBarcode, "") + "</span>";
            }

            // string strReaderBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + strReaderBarcode + "</a>";

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 预约") +
                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +
                BuildHtmlEncodedLine("读者证条码号", BuildReaderBarcodeLink(strReaderBarcode)) +
                BuildHtmlPendingLine("(读者摘要)", BuildPendingReaderSummary(strReaderBarcode)) +

                BuildHtmlEncodedLine("册条码号列表", strItemBarcodeLink) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // Amerce
        int GetAmerceString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");

            string strAmerceItems = FormatInnerXml(
                DomUtil.GetElementInnerXml(dom.DocumentElement, "amerceItems"));
            string strExpiredOverdues = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "expiredOverdues"));

            string strReaderRecord = DomUtil.GetElementText(dom.DocumentElement, "readerRecord", out node);
            string strReaderRecPath = "";
            string strReaderRecordHtml = "";
            if (node != null)
            {
                strReaderRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetReaderInfoString(
                    this.DisplayReaderBorrowHistory,
                    strReaderRecord,
                    out strReaderRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOldReaderRecord = DomUtil.GetElementText(dom.DocumentElement, "oldReaderRecord", out node);
            string strOldReaderRecPath = "";
            string strOldReaderRecordHtml = "";
            if (node != null)
            {
                strOldReaderRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetReaderInfoString(
                    this.DisplayReaderBorrowHistory,
                    strOldReaderRecord,
                    out strOldReaderRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            // string strReaderBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + strReaderBarcode + "</a>";

            string strAmerceRecords = "";
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("amerceRecord");
            int i = 0;
            foreach (XmlNode cur_node in nodes)
            {
                string strAmerceRecord = cur_node.InnerText.Trim();
                string strAmerceRecPath = "";
                string strAmerceRecordHtml = "";
                if (cur_node != null)
                {
                    strAmerceRecPath = DomUtil.GetAttr(cur_node, "recPath");
                    nRet = GetAmerceInfoString(strAmerceRecord,
    out strAmerceRecordHtml,
    out strError);
                    if (nRet == -1)
                        return -1;
                }
                strAmerceRecords += BuildHtmlEncodedLine("违约金记录 " + (i+1).ToString() + "/" + nodes.Count.ToString(),
                    strAmerceRecPath, strAmerceRecordHtml);

                i++;
            }

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 违约金操作") +
                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +
                BuildHtmlEncodedLine("读者证条码号", BuildReaderBarcodeLink(strReaderBarcode)) +
                BuildHtmlPendingLine("(读者摘要)", BuildPendingReaderSummary(strReaderBarcode)) +
                BuildHtmlEncodedLine("交费事项", strAmerceItems) +
                BuildHtmlEncodedLine("已经到期的违约金元素", strExpiredOverdues) +

                strAmerceRecords +

                BuildHtmlEncodedLine("操作前的读者记录", strOldReaderRecPath, strOldReaderRecordHtml) +
                BuildHtmlEncodedLine("操作后的读者记录", strReaderRecPath, strReaderRecordHtml) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // SetUser
        int GetSetUserString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            /*
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
             * */

            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");

            string strAccountRecord = DomUtil.GetElementOuterXml(dom.DocumentElement, "account");
            string strAccountRecordHtml = "";
            if (string.IsNullOrEmpty(strAccountRecord) == false)
            {
                nRet = GetAccountInfoString(
                    strAccountRecord,
                    out strAccountRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOldAccountRecord = DomUtil.GetElementOuterXml(dom.DocumentElement, "oldAccount");
            string strOldAccountRecordHtml = "";
            if (string.IsNullOrEmpty(strOldAccountRecord) == false)
            {
                nRet = GetAccountInfoString(
                    strOldAccountRecord,
                    out strOldAccountRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strNewPassword = DomUtil.GetElementText(dom.DocumentElement, "newPassword");

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            strHtml =
                "<table class='operlog'>" +
                BuildHtmlLine("操作类型", strOperation + " -- 用户操作") +
                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +
                (string.IsNullOrEmpty(strNewPassword) == false ? BuildHtmlLine("新密码", strNewPassword) : "") +
                BuildHtmlEncodedLine("操作前的用户记录", strOldAccountRecordHtml) +
                BuildHtmlEncodedLine("操作后的用户记录", strAccountRecordHtml) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // DevolveReaderInfo
        int GetDevoleveReaderInfoString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strSourceReaderBarcode = DomUtil.GetElementInnerText(dom.DocumentElement, "sourceReaderBarcode");
            string strTargetReaderBarcode = DomUtil.GetElementInnerText(dom.DocumentElement, "targetReaderBarcode");
            string strBorrows = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "borrows"));
            string strOverdues = FormatInnerXml(
                DomUtil.GetElementInnerXml(dom.DocumentElement, "overdues"));

            string strSourceReaderRecord = DomUtil.GetElementText(dom.DocumentElement, "sourceReaderRecord", out node);
            string strSourceReaderRecPath = "";
            string strSourceReaderRecordHtml = "";
            if (node != null)
            {
                strSourceReaderRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetReaderInfoString(
                    this.DisplayReaderBorrowHistory,
                    strSourceReaderRecord,
                    out strSourceReaderRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strTargetReaderRecord = DomUtil.GetElementText(dom.DocumentElement, "targetReaderRecord", out node);
            string strTargetReaderRecPath = "";
            string strTargetReaderRecordHtml = "";
            if (node != null)
            {
                strTargetReaderRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetReaderInfoString(
                    this.DisplayReaderBorrowHistory,
                    strTargetReaderRecord,
                    out strTargetReaderRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strChangedEntityRecords = "";
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("changeEntityRecord");
            int i = 0;
            foreach (XmlNode cur_node in nodes)
            {
                string strEntityRecord = cur_node.InnerText.Trim();
                string strEntityRecPath = "";
                string strEntityRecordHtml = "";
                string strAttachmentIndex = "";
                if (cur_node != null)
                {
                    strEntityRecPath = DomUtil.GetAttr(cur_node, "recPath");
                    strAttachmentIndex = DomUtil.GetAttr(cur_node, "attachmentIndex");
                    nRet = GetItemInfoString(
                        this.DisplayItemBorrowHistory,
                        strEntityRecord,
                        strEntityRecPath,
                        out strEntityRecordHtml,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                if (string.IsNullOrEmpty(strEntityRecordHtml) == false)
                {
                    strChangedEntityRecords += BuildHtmlEncodedLine("修改后的册记录 " + (i + 1).ToString() + "/" + nodes.Count.ToString(),
                        strEntityRecPath + strAttachmentIndex, strEntityRecordHtml);
                }
                else
                {
                    strChangedEntityRecords += BuildHtmlLine("修改后的册记录 " + (i + 1).ToString() + "/" + nodes.Count.ToString(),
                        strEntityRecPath + " 附件索引: " + strAttachmentIndex);
                }

                i++;
            }


            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 转移借阅信息") +
                BuildHtmlEncodedLine("源读者证条码号", BuildReaderBarcodeLink(strSourceReaderBarcode)) +
                BuildHtmlPendingLine("(源读者摘要)", BuildPendingReaderSummary(strSourceReaderBarcode)) +
                BuildHtmlEncodedLine("目标读者证条码号", BuildReaderBarcodeLink(strTargetReaderBarcode)) +
                BuildHtmlPendingLine("(目标读者摘要)", BuildPendingReaderSummary(strTargetReaderBarcode)) +

                BuildHtmlEncodedLine("发生转移的借阅信息", strBorrows) +
                BuildHtmlEncodedLine("发生转移的待交费信息", strOverdues) +

                BuildHtmlEncodedLine("源读者记录", strSourceReaderRecPath, strSourceReaderRecordHtml) +
                BuildHtmlEncodedLine("目标读者记录", strTargetReaderRecPath, strTargetReaderRecordHtml) +

                strChangedEntityRecords +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // RepairBorrowInfo
        int GetRepairBorrowInfoString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            //int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "itemBarcode");

            string strConfirmItemRecPath = DomUtil.GetElementText(dom.DocumentElement, "confirmItemRecPath");

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 修复借阅信息") +
                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +
                BuildHtmlEncodedLine("读者证条码号", BuildReaderBarcodeLink(strReaderBarcode)) +
                BuildHtmlPendingLine("(读者摘要)", BuildPendingReaderSummary(strReaderBarcode)) +
                BuildHtmlEncodedLine("册条码号", BuildItemBarcodeLink(strItemBarcode)) +
                BuildHtmlPendingLine("(书目摘要)", BuildPendingBiblioSummary(strItemBarcode, strConfirmItemRecPath)) +
                BuildHtmlLine("册记录路径", strConfirmItemRecPath) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // Hire
        int GetHireString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");

            string strOverdues = FormatInnerXml(
    DomUtil.GetElementText(dom.DocumentElement, "overdues"));

            string strReaderRecord = DomUtil.GetElementText(dom.DocumentElement, "readerRecord", out node);
            string strReaderRecPath = "";
            string strReaderRecordHtml = "";
            if (node != null)
            {
                strReaderRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetReaderInfoString(
                    this.DisplayReaderBorrowHistory,
                    strReaderRecord,
                    out strReaderRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 创建租金交费请求") +
                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +
                BuildHtmlEncodedLine("读者证条码号", BuildReaderBarcodeLink(strReaderBarcode)) +
                BuildHtmlPendingLine("(读者摘要)", BuildPendingReaderSummary(strReaderBarcode)) +

                BuildHtmlEncodedLine("所创建的待交费事项", strOverdues) +

                BuildHtmlEncodedLine("读者记录", strReaderRecPath, strReaderRecordHtml) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // Foregift
        int GetForegiftString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");

            string strOverdues = FormatInnerXml(
    DomUtil.GetElementText(dom.DocumentElement, "overdues"));

            string strReaderRecord = DomUtil.GetElementText(dom.DocumentElement, "readerRecord", out node);
            string strReaderRecPath = "";
            string strReaderRecordHtml = "";
            if (node != null)
            {
                strReaderRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetReaderInfoString(
                    this.DisplayReaderBorrowHistory,
                    strReaderRecord,
                    out strReaderRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 创建押金交费请求") +
                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +
                BuildHtmlEncodedLine("读者证条码号", BuildReaderBarcodeLink(strReaderBarcode)) +
                BuildHtmlPendingLine("(读者摘要)", BuildPendingReaderSummary(strReaderBarcode)) +

                BuildHtmlEncodedLine("所创建的待交费事项", strOverdues) +

                BuildHtmlEncodedLine("读者记录", strReaderRecPath, strReaderRecordHtml) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // Settlement
        int GetSettlementString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strID = DomUtil.GetElementText(dom.DocumentElement, "id");

            string strOldAmerceRecord = DomUtil.GetElementText(dom.DocumentElement, "oldAmerceRecord", out node);
            string strOldAmerceRecPath = "";
            string strOldAmerceRecordHtml = "";
            if (node != null)
            {
                strOldAmerceRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetAmerceInfoString(
                    strOldAmerceRecord,
                    out strOldAmerceRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strAmerceRecord = DomUtil.GetElementText(dom.DocumentElement, "amerceRecord", out node);
            string strAmerceRecPath = "";
            string strAmerceRecordHtml = "";
            if (node != null)
            {
                strAmerceRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetAmerceInfoString(
                    strAmerceRecord,
                    out strAmerceRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 结算") +
                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +

                BuildHtmlLine("ID", strID) +

                BuildHtmlEncodedLine("操作前的违约金记录", strOldAmerceRecPath, strOldAmerceRecordHtml) +
                BuildHtmlEncodedLine("操作后的违约金记录", strAmerceRecPath, strAmerceRecordHtml) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // PassGate
        int GetPassGateString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            //int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
            string strGateName = DomUtil.GetElementText(dom.DocumentElement, "gateName");

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 入馆登记") +
                BuildHtmlEncodedLine("读者证条码号", BuildReaderBarcodeLink(strReaderBarcode)) +
                BuildHtmlPendingLine("(读者摘要)", BuildPendingReaderSummary(strReaderBarcode)) +

                BuildHtmlLine("门名字", strGateName) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // SetEntity
        int GetSetEntityString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strStyle = DomUtil.GetElementText(dom.DocumentElement, "style");

            string strRecord = DomUtil.GetElementText(dom.DocumentElement, "record", out node);
            string strRecPath = "";
            string strRecordHtml = "";
            if (node != null)
            {
                strRecPath = DomUtil.GetAttr(node, "recPath");

                if (strOperation == "setEntity")
                    nRet = GetItemInfoString(
                        this.DisplayItemBorrowHistory,
                        strRecord,
                        strRecPath,
                        out strRecordHtml,
                        out strError);
                else if (strOperation == "setOrder")
                    nRet = GetOrderInfoString(
                        strRecord,
                        out strRecordHtml,
                        out strError);
                else if (strOperation == "setIssue")
                    nRet = GetIssueInfoString(
                        strRecord,
                        out strRecordHtml,
                        out strError);
                else if (strOperation == "setComment")
                    nRet = GetCommentInfoString(
                        strRecord,
                        out strRecordHtml,
                        out strError);

                if (nRet == -1)
                    return -1;
            }

            string strOldRecord = DomUtil.GetElementText(dom.DocumentElement, "oldRecord", out node);
            string strOldRecPath = "";
            string strOldRecordHtml = "";
            if (node != null)
            {
                strOldRecPath = DomUtil.GetAttr(node, "recPath");

                if (strOperation == "setEntity")
                    nRet = GetItemInfoString(
                        this.DisplayItemBorrowHistory,
                        strOldRecord,
                        strOldRecPath,
                        out strOldRecordHtml,
                        out strError);
                else if (strOperation == "setOrder")
                    nRet = GetOrderInfoString(
                        strOldRecord,
                        out strOldRecordHtml,
                        out strError);
                else if (strOperation == "setIssue")
                    nRet = GetIssueInfoString(
                        strOldRecord,
                        out strOldRecordHtml,
                        out strError);
                else if (strOperation == "setComment")
                    nRet = GetCommentInfoString(
                        strOldRecord,
                        out strOldRecordHtml,
                        out strError);
                if (nRet == -1)
                    return -1;

            }

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            string strOperationCaption = "";
            if (strOperation == "setEntity")
                strOperationCaption = "设置册记录";
            else if (strOperation == "setOrder")
                strOperationCaption = "设置订购记录";
            else if (strOperation == "setIssue")
                strOperationCaption = "设置期记录";
            else if (strOperation == "setComment")
                strOperationCaption = "设置评注记录";

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- " + strOperationCaption) +

                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +

                BuildHtmlEncodedLine("操作前的记录", strOldRecPath, strOldRecordHtml) +
                BuildHtmlEncodedLine("操作后的记录", strRecPath, strRecordHtml) +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // WriteRes
        int GetWriteResString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            //int nRet = 0;

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strRequestResPath = DomUtil.GetElementText(dom.DocumentElement, "requestResPath");
            string strResPath = DomUtil.GetElementText(dom.DocumentElement, "resPath");

            string strRanges = DomUtil.GetElementText(dom.DocumentElement, "ranges");
            string strTotalLength = DomUtil.GetElementText(dom.DocumentElement, "totalLength");
            string strMetadata = DomUtil.GetElementText(dom.DocumentElement, "metadata");
            string strStyle = DomUtil.GetElementText(dom.DocumentElement, "style");

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 写入对象资源") +
                BuildHtmlLine("请求参数中的资源路径", strRequestResPath) +
                BuildHtmlLine("实际资源路径", strResPath) +
                BuildHtmlLine("字节范围", strRanges) +
                BuildHtmlLine("总长度", strTotalLength) +
                BuildHtmlLine("元数据", strMetadata) +
                BuildHtmlLine("操作方式", strStyle) +
                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        // SetBiblioInfo
        int GetSetBiblioInfoString(XmlDocument dom,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";
            int nRet = 0;

            XmlNode node = null;
            // 注: 实际上没有<libraryCode>元素
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");

            string strRecord = DomUtil.GetElementText(dom.DocumentElement, "record", out node);
            string strRecPath = "";
            string strRecordHtml = "";
            if (node != null)
            {
                strRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetBiblioInfoString(
                    strRecPath,
                    strRecord,
out strRecordHtml,
out strError);
                if (nRet == -1)
                    return -1;
            }

            string strOldRecord = DomUtil.GetElementText(dom.DocumentElement, "oldRecord", out node);
            string strOldRecPath = "";
            string strOldRecordHtml = "";
            if (node != null)
            {
                strOldRecPath = DomUtil.GetAttr(node, "recPath");
                nRet = GetBiblioInfoString(
                    strOldRecPath,
                    strOldRecord,
    out strOldRecordHtml,
    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strDiffRecordHtml = "";
            if (string.IsNullOrEmpty(strOldRecord) == false && string.IsNullOrEmpty(strOldRecPath) == false
                && string.IsNullOrEmpty(strRecord) == false && string.IsNullOrEmpty(strRecPath) == false)
            {
                nRet = GetDiffBiblioInfoString(
                    strRecPath,
                    strOldRecord,
                    strRecord,
                    out strDiffRecordHtml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            string strDiffLineTitle = "";
            if (strOldRecPath != strRecPath)
                strDiffLineTitle = strOldRecPath + " -- " + strRecPath;
            else
                strDiffLineTitle = strOldRecPath;

            string strDeletedEntityRecords = FormatInnerXml(
                DomUtil.GetElementInnerXml(dom.DocumentElement, "deletedEntityRecords"));
            string strCopyEntityRecords = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "copyEntityRecords"));
            string strMoveEntityRecords = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "moveEntityRecords"));

            string strDeletedOrderRecords = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "deletedOrderRecords"));
            string strCopyOrderRecords = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "copyOrderRecords"));
            string strMoveOrderRecords = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "moveOrderRecords"));

            string strDeletedIssueRecords = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "deletedIssueRecords"));
            string strCopyIssueRecords = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "copyIssueRecords"));
            string strMoveIssueRecords = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "moveIssueRecords"));

            string strDeletedCommentRecords = FormatInnerXml(
DomUtil.GetElementInnerXml(dom.DocumentElement, "deletedCommentRecords"));
            string strCopyCommentRecords = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "copyCommentRecords"));
            string strMoveCommentRecords = FormatInnerXml(
    DomUtil.GetElementInnerXml(dom.DocumentElement, "moveCommentRecords"));

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            strHtml = 
                "<table class='operlog'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlLine("操作类型", strOperation + " -- 设置书目信息") +
                BuildHtmlLine("动作", strAction + " -- " + GetActionName(strOperation, strAction)) +

                (string.IsNullOrEmpty(strDiffRecordHtml) == false ?
                BuildHtmlLine("修改前后书目记录", strDiffLineTitle) +
                BuildHtmlEncodedOneColumnLine("", strDiffRecordHtml) : "") +

                (string.IsNullOrEmpty(strRecordHtml) == false && string.IsNullOrEmpty(strDiffRecordHtml) == true ?
                BuildHtmlEncodedLine("新书目记录", strRecPath, strRecordHtml) : "") +
                (string.IsNullOrEmpty(strOldRecordHtml) == false && string.IsNullOrEmpty(strDiffRecordHtml) == true ?
                BuildHtmlEncodedLine("旧书目记录", strOldRecPath, strOldRecordHtml) : "") +

                BuildHtmlLine("操作者", strOperator) +
                BuildHtmlLine("操作时间", strOperTime) +
                BuildClientAddressLine(dom) +
                "</table>";

            return 0;
        }

        static string GetActionName(string strOperation,
            string strAction)
        {
            if (strAction == "change")
                return "修改";
            if (strAction == "new")
                return "创建";
            if (strAction == "copy")
                return "复制";
            if (strAction == "delete")
                return "删除";
            if (strAction == "move")
                return "移动";
            if (strAction == "onlydeletebiblio")
                return "仅删除书目记录";
            if (strAction == "onlycopybiblio")
                return "仅复制书目记录";
            if (strAction == "onlymovebiblio")
                return "仅移动书目记录";

            if (strOperation == "borrow")
            {
                if (strAction == "borrow")
                    return "借书";
                if (strAction == "renew")
                    return "续借";
            }

            if (strOperation == "return")
            {
                if (strAction == "return")
                    return "还书";
                if (strAction == "lost")
                    return "丢失声明";
            }

            // Reservation
            if (strAction == "merge")
                return "合并";
            if (strAction == "split")
                return "拆分";

            // Amerce
            if (strOperation == "amerce")
            {
                if (strAction == "amerce")
                    return "交费";
                if (strAction == "undo")
                    return "撤回交费";
                if (strAction == "modifyprice")
                    return "变更金额";
                if (strAction == "modifycomment")
                    return "变更注释";
                if (strAction == "expire")
                    return "以停代金到期";
            }

            if (strOperation == "repairBorrowInfo")
            {
                if (strAction == "repairreaderside")
                    return "从读者记录入手修复";
                if (strAction == "repairitemside")
                    return "从册记录入手修复";
            }

            if (strOperation == "hire")
            {
                if (strAction == "hire")
                    return "租金交费";
                if (strAction == "hirelate")
                    return "延迟型租金交费";
            }

            if (strOperation == "foregift")
            {
                if (strAction == "foregift")
                    return "押金交费";
                if (strAction == "return")
                    return "押金退费";
            }

            if (strOperation == "settlement")
            {
                if (strAction == "settlement")
                    return "结算";
                if (strAction == "undosettlement")
                    return "撤回结算";
                if (strAction == "delete")
                    return "删除";
            }

            return strAction;
        }

        /// <summary>
        /// 构造前端地址 HTML 片断
        /// </summary>
        /// <param name="dom">XmlDocument 对象。本方法从中取 clientAddress 元素</param>
        /// <returns>HTML 片断</returns>
        public static string BuildClientAddressLine(XmlDocument dom)
        {
            if (dom == null || dom.DocumentElement == null)
                return "";
            XmlNode node = dom.DocumentElement.SelectSingleNode("clientAddress");
            if (node == null)
                return "";

            string strCaption = "前端地址";
            string strAddress = node.InnerText.Trim();
            string strVia = DomUtil.GetAttr(node, "via");

            string strContent = "";
            if (string.IsNullOrEmpty(strAddress) == false)
                strContent += HttpUtility.HtmlEncode(strAddress);
            if (string.IsNullOrEmpty(strVia) == false)
                strContent += "<span class='via'>" + HttpUtility.HtmlEncode("经由 " + strVia) + "</span>";

            return "<tr>" +
                "<td class='name'>" + HttpUtility.HtmlEncode(strCaption) + "</td>" +
                "<td class='content'>" + strContent + "</td>" +
                "</tr>";
        }

        /// <summary>
        /// 构造 HTML 片断字符串
        /// </summary>
        /// <param name="strCaption">标题</param>
        /// <param name="strValue">内容</param>
        /// <returns>HTML 片断</returns>
        public static string BuildHtmlLine(string strCaption,
            string strValue)
        {
            if (string.IsNullOrEmpty(strValue) == true)
                return "";

            return "<tr>" +
                "<td class='name'>" + HttpUtility.HtmlEncode(strCaption) + "</td>" + 
                "<td class='content'>" + HttpUtility.HtmlEncode(strValue) + "</td>" +
                "</tr>";
        }

        /// <summary>
        /// 构造 HTML 片断字符串
        /// </summary>
        /// <param name="strCaption">标题</param>
        /// <param name="strValue">内容。本方法对这个字符串不进行 HtmlEncode </param>
        /// <param name="strValueClass">额外添加的 class 名字</param>
        /// <returns>HTML 片断</returns>
        public static string BuildClassHtmlEncodedLine(string strCaption,
            string strCaptionClass,
            string strValue,
            string strValueClass)
        {
            if (string.IsNullOrEmpty(strValue) == true)
                return "";

            return "<tr>" +
                "<td class='name " + strCaptionClass + "'>" + HttpUtility.HtmlEncode(strCaption) + "</td>" +
                "<td class='content "+strValueClass+"'>" + strValue + "</td>" +
                "</tr>";
        }

        /// <summary>
        /// 构造 HTML 片断字符串
        /// </summary>
        /// <param name="strCaption">标题</param>
        /// <param name="strValue">内容。本方法对这个字符串不进行 HtmlEncode </param>
        /// <returns>HTML 片断</returns>
        public static string BuildHtmlEncodedLine(string strCaption,
            string strValue)
        {
            if (string.IsNullOrEmpty(strValue) == true)
                return "";

            return "<tr>" +
                "<td class='name'>" + HttpUtility.HtmlEncode(strCaption) + "</td>" +
                "<td class='content'>" + strValue + "</td>" +
                "</tr>";
        }

        /// <summary>
        /// 构造 Ajax摘要获取 HTML 片断字符串
        /// </summary>
        /// <param name="strCaption">标题</param>
        /// <param name="strValue">内容</param>
        /// <returns>HTML 片断</returns>
        public static string BuildHtmlPendingLine(string strCaption,
    string strValue)
        {
            if (string.IsNullOrEmpty(strValue) == true)
                return "";

            return "<tr>" +
                "<td class='name'>" + HttpUtility.HtmlEncode(strCaption) + "</td>" +
                "<td class='content summary pending'>" + strValue + "</td>" +
                "</tr>";
        }

        static string BuildHtmlEncodedLine(
            string strCaption,
            string strFirstLine,
            string strValue)
        {
            if (string.IsNullOrEmpty(strValue) == true
                && string.IsNullOrEmpty(strFirstLine) == true)
                return "";

            return "<tr>" +
                "<td class='name'>" + HttpUtility.HtmlEncode(strCaption) + "</td>" +
                "<td class='content' colspan='2'>" + HttpUtility.HtmlEncode(strFirstLine) + strValue + "</td>" +
                "</tr>";
        }

        // 自动 HhmlEncode 一个字符串。
        // 如果字符串的第一个字符为 '<'，表示不需要 encode。
        // 特殊情况下，正文正好第一个字符为 '<' 但是也需要 encode，就不适合用本函数
        static string AutoHtmlEncode(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return strText;

            if (strText[0] == '<')
                return strText;

            return HttpUtility.HtmlEncode(strText);
        }
        
        // 一列, content
        static string BuildHtmlEncodedOneColumnLine(string strFirstLine,
            string strValue)
        {
            if (string.IsNullOrEmpty(strValue) == true
                && string.IsNullOrEmpty(strFirstLine) == true)
                return "";

            return "<tr>" +
                "<td class='content' colspan='2'>" + HttpUtility.HtmlEncode(strFirstLine) + strValue + "</td>" +
                "</tr>";
        }

        /// <summary>
        /// 获得 RFC 1123 时间字符串的显示格式字符串
        /// </summary>
        /// <param name="strRfc1123TimeString">时间字符串。RFC1123 格式</param>
        /// <returns>显示格式</returns>
        public static string GetRfc1123DisplayString(string strRfc1123TimeString)
        {
            if (string.IsNullOrEmpty(strRfc1123TimeString) == true)
                return "";

            try
            {
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123TimeString, "G") + " (" + strRfc1123TimeString + ")";
            }
            catch (Exception ex)
            {
                return "解析 RFC1123 时间字符串 '" + strRfc1123TimeString + "' 时出错: " + ex.Message;
            }
        }

        static string FormatInnerXml(string strInnerXml)
        {
            if (string.IsNullOrEmpty(strInnerXml) == true)
                return "";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            try
            {
                dom.DocumentElement.InnerXml = strInnerXml;
            }
            catch (Exception ex)
            {
                return "格式化InnerXml '' 出错: " + ex.Message;
            }

            StringBuilder result = new StringBuilder(4096);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("*");
            foreach (XmlNode node in nodes)
            {
                result.Append("<p class='xmlline'>");
                result.Append(HttpUtility.HtmlEncode(node.OuterXml));
                result.Append("</p>");
            }

            return result.ToString();
        }

        // 包装后的版本
        // 获得读者记录 HTML 字符串。不包括外面的<html><body>
        int GetReaderInfoString(
            bool bDisplayBorrowHistory,
            string strReaderXml,
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            if (string.IsNullOrEmpty(strReaderXml) == true)
                return 0;

            XmlDocument reader_dom = new XmlDocument();
            try
            {
                reader_dom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "读者记录XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            return GetReaderInfoString(bDisplayBorrowHistory, reader_dom, out strHtml, out strError);
        }

        // 获得读者记录 HTML 字符串。不包括外面的<html><body>
        int GetReaderInfoString(
            bool bDisplayBorrowHistory,
            XmlDocument reader_dom,
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            string strBarcode = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "barcode");
            string strState = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "state");
            string strReaderType = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "readerType");
            string strCardNumber = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "cardNumber");
            string strComment = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "comment");
            string strCreateDate = GetRfc1123DisplayString(
                DomUtil.GetElementInnerText(reader_dom.DocumentElement, "createDate"));
            string strExpireDate = GetRfc1123DisplayString(
                DomUtil.GetElementInnerText(reader_dom.DocumentElement, "expireDate"));
            string strName = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "name");
            string strDisplayName = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "displayName");
            string strPreference = DomUtil.GetElementInnerXml(reader_dom.DocumentElement, "preference");
            string strGender = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "gender");
            string strNation = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "nation");
            string strDateOfBirth = GetRfc1123DisplayString(
                DomUtil.GetElementInnerText(reader_dom.DocumentElement, "dateOfBirth"));
            string strIdCardNumber = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "idCardNumber");

            string strDepartment = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "department");
            string strPost = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "post");
            string strAddress = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "address");
            string strTel = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "tel");
            string strEmail = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "email");

            string strBorrows = FormatInnerXml(
                DomUtil.GetElementInnerXml(reader_dom.DocumentElement, "borrows"));
            string strOverdues = FormatInnerXml(
                DomUtil.GetElementInnerXml(reader_dom.DocumentElement, "overdues"));
            string strReservations = FormatInnerXml(
                DomUtil.GetElementInnerXml(reader_dom.DocumentElement, "reservations"));

            string strOutofReservations = FormatInnerXml(
                DomUtil.GetElementInnerXml(reader_dom.DocumentElement, "outofReservations"));
            string strOutofReservationsCount = DomUtil.GetElementAttr(reader_dom.DocumentElement,
                "outofReservations", "count");

            string strBorrowHistory = "";
            string strBorrowHistoryCount = "";
            if (bDisplayBorrowHistory == true)
            {
                strBorrowHistory = FormatInnerXml(
                 DomUtil.GetElementInnerXml(reader_dom.DocumentElement, "borrowHistory"));

                strBorrowHistoryCount = DomUtil.GetElementAttr(reader_dom.DocumentElement,
                    "borrowHistory", "count");
            }

            string strForegift = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "foregift");

            string strHireXml = DomUtil.GetElementOuterXml(reader_dom.DocumentElement, "hire").Trim();

            // string strReaderBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + strBarcode + "</a>";

            strHtml =
                "<table class='readerinfo'>" +
                BuildHtmlEncodedLine("读者证条码号", BuildReaderBarcodeLink(strBarcode)) +
                BuildHtmlLine("姓名", strName) +
                BuildHtmlLine("读者类型", strReaderType)
                + BuildHtmlLine("证状态", strState)
                + BuildHtmlLine("证件号", strCardNumber)
                + BuildHtmlLine("注释", strComment)
                + BuildHtmlLine("办证日期", strCreateDate)
                + BuildHtmlLine("失效期", strExpireDate)
                + BuildHtmlLine("显示名", strDisplayName)
                + BuildHtmlLine("偏好配置", strPreference)
                + BuildHtmlLine("性别", strGender)
                + BuildHtmlLine("民族", strNation)
                + BuildHtmlLine("出生日期", strDateOfBirth)
                + BuildHtmlLine("身份证号", strIdCardNumber)
                + BuildHtmlLine("部门", strDepartment)
                + BuildHtmlLine("职务", strPost)
                + BuildHtmlLine("地址", strAddress)
                + BuildHtmlLine("电话号码", strTel)
                + BuildHtmlLine("Email", strEmail)
                + BuildHtmlLine("押金余额", strEmail)
                + BuildHtmlLine("租金信息", strHireXml)

                + BuildHtmlEncodedLine("借阅信息", strBorrows)
                + BuildHtmlEncodedLine("待交费信息", strOverdues)
                + BuildHtmlEncodedLine("预约信息", strReservations)
                + BuildHtmlEncodedLine("预约未取信息", strOutofReservationsCount, strOutofReservations)

                + (bDisplayBorrowHistory == true ? BuildHtmlEncodedLine("借阅历史", strBorrowHistoryCount, strBorrowHistory) : "")

                + "</table>";

            return 0;
        }

        // 获得两条书目记录对照的 HTML 字符串。不包括外面的<html><body>
        int GetDiffBiblioInfoString(
            string strBiblioRecPath,
            string strOldBiblioXml,
            string strNewBiblioXml,
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            if (string.IsNullOrEmpty(strOldBiblioXml) == true
                && string.IsNullOrEmpty(strNewBiblioXml) == true)
                return 0;

            if (string.IsNullOrEmpty(strOldBiblioXml) == true 
                || string.IsNullOrEmpty(strNewBiblioXml) == true)
            {
                if (string.IsNullOrEmpty(strOldBiblioXml) == false)
                    return GetBiblioInfoString(
                        strBiblioRecPath,
                        strOldBiblioXml,
     out strHtml,
     out strError);
                else
                    return GetBiblioInfoString(
                        strBiblioRecPath,
                        strNewBiblioXml,
    out strHtml,
    out strError);
            }

            Debug.Assert(string.IsNullOrEmpty(strOldBiblioXml) == false
                && string.IsNullOrEmpty(strNewBiblioXml) == false, "");

            string strOldMarcSyntax = "";
            string strOldMarc = "";
            string strOldFragmentXml = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            int nRet = MarcUtil.Xml2Marc(strOldBiblioXml,
                MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                null,
                out strOldMarcSyntax,
                out strOldMarc,
                out strOldFragmentXml,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            // strOldFragmentXml = strOldBiblioXml;    // testing

            string strNewMarcSyntax = "";
            string strNewMarc = "";
            string strNewFragmentXml = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            nRet = MarcUtil.Xml2Marc(strNewBiblioXml,
                MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                null,
                out strNewMarcSyntax,
                out strNewMarc,
                out strNewFragmentXml,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            if (string.IsNullOrEmpty(strOldMarc) == true && string.IsNullOrEmpty(strNewMarc) == true)  // 不是MARC格式
            {
                // TODO: 其中一个是 MARC 格式的如何处理?
                strHtml = HttpUtility.HtmlEncode(strOldBiblioXml) + HttpUtility.HtmlEncode(strNewBiblioXml);
            }
            else
            {
                if (string.IsNullOrEmpty(strOldMarc) == false
                    && string.IsNullOrEmpty(strNewMarc) == false)
                {
                    string strOldImageFragment = BiblioSearchForm.GetImageHtmlFragment(
strBiblioRecPath,
strOldMarc);
                    string strNewImageFragment = BiblioSearchForm.GetImageHtmlFragment(
strBiblioRecPath,
    strNewMarc);
                    // 创建展示两个 MARC 记录差异的 HTML 字符串
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = MarcDiff.DiffHtml(
                        strOldMarc,
                        strOldFragmentXml,
                        strOldImageFragment,
                        strNewMarc,
                        strNewFragmentXml,
                        strNewImageFragment,
                        out strHtml,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                else if (string.IsNullOrEmpty(strOldMarc) == false
        && string.IsNullOrEmpty(strNewMarc) == true)
                {
                    string strOldImageFragment = BiblioSearchForm.GetImageHtmlFragment(
strBiblioRecPath,
strOldMarc);
                    if (string.IsNullOrEmpty(strNewFragmentXml) == false)
                    {

                        nRet = MarcDiff.DiffHtml(
                            strOldMarc,
                            strOldFragmentXml,
                            strOldImageFragment,
                            strNewMarc,
                            strNewFragmentXml,
                            "",
                            out strHtml,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    else
                    strHtml = MarcUtil.GetHtmlOfMarc(strOldMarc,
                        strOldFragmentXml,
                        strOldImageFragment,
                        false);
                }
                else if (string.IsNullOrEmpty(strOldMarc) == true
                    && string.IsNullOrEmpty(strNewMarc) == false)
                {
                    string strNewImageFragment = BiblioSearchForm.GetImageHtmlFragment(
strBiblioRecPath,
    strNewMarc); 
                    if (string.IsNullOrEmpty(strOldFragmentXml) == false)
                    {
                        nRet = MarcDiff.DiffHtml(
                            strOldMarc,
                            strOldFragmentXml,
                            "",
                            strNewMarc,
                            strNewFragmentXml,
                            strNewImageFragment,
                            out strHtml,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    else 
                        strHtml = MarcUtil.GetHtmlOfMarc(strNewMarc,
                        strNewFragmentXml,
                        strNewImageFragment,
                        false);
                }
            }

            return 0;
        }

        // 包装后的版本
        // 获得书目记录 HTML 字符串。不包括外面的<html><body>
        int GetBiblioInfoString(
            string strBiblioRecPath,
            string strBiblioXml,
    out string strHtml,
    out string strError)
        {
            strHtml = "";
            strError = "";

            if (string.IsNullOrEmpty(strBiblioXml) == true)
                return 0;

            string strOutMarcSyntax = "";
            string strMarc = "";
            string strFragmentXml = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            int nRet = MarcUtil.Xml2Marc(strBiblioXml,
                MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                null,
                out strOutMarcSyntax,
                out strMarc,
                out strFragmentXml,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            if (string.IsNullOrEmpty(strMarc) == true)  // 不是MARC格式
                strHtml = HttpUtility.HtmlEncode(strBiblioXml);
            else
            {
                // 2015/1/3
                string strImageFragment = BiblioSearchForm.GetImageHtmlFragment(
    strBiblioRecPath,
    strMarc);

                strHtml = MarcUtil.GetHtmlOfMarc(strMarc,
                    strFragmentXml,
                    strImageFragment,
                    false);
            }

            return 0;
        }

        // 包装后的版本
        // 获得册记录 HTML 字符串。不包括外面的<html><body>
        int GetItemInfoString(
            bool bDisplayBorrowHistory,
            string strItemXml,
            string strItemRecPath,
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            if (string.IsNullOrEmpty(strItemXml) == true)
                return 0;

            XmlDocument item_dom = new XmlDocument();
            try
            {
                item_dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "册记录XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            return GetItemInfoString(
                bDisplayBorrowHistory,
                item_dom, 
                strItemRecPath,
                out strHtml,
                out strError);
        }

        // 获得册记录 HTML 字符串。不包括外面的<html><body>
        int GetItemInfoString(
            bool bDisplayBorrowHistory,
            XmlDocument item_dom,
            string strItemRecPath,
out string strHtml,
out string strError)
        {
            strHtml = "";
            strError = "";

            string strParent = DomUtil.GetElementInnerText(item_dom.DocumentElement, "parent");
            string strBarcode = DomUtil.GetElementInnerText(item_dom.DocumentElement, "barcode");
            string strState = DomUtil.GetElementInnerText(item_dom.DocumentElement, "state");
            string strBookType = DomUtil.GetElementInnerText(item_dom.DocumentElement, "bookType");
            string strPublishTime = DomUtil.GetElementInnerText(item_dom.DocumentElement, "publishTime");
            string strLocation = DomUtil.GetElementInnerText(item_dom.DocumentElement, "location");

            string strSeller = DomUtil.GetElementInnerText(item_dom.DocumentElement, "seller");
            string strSource = DomUtil.GetElementInnerText(item_dom.DocumentElement, "source");
            string strPrice = DomUtil.GetElementInnerXml(item_dom.DocumentElement, "price");
            string strAccessNo = DomUtil.GetElementInnerText(item_dom.DocumentElement, "accessNo");
            string strVolume = DomUtil.GetElementInnerText(item_dom.DocumentElement, "volume");

            string strRegisterNo = DomUtil.GetElementInnerText(item_dom.DocumentElement, "registerNo");
            string strComment = DomUtil.GetElementInnerText(item_dom.DocumentElement, "comment");
            string strMergeComment = DomUtil.GetElementInnerText(item_dom.DocumentElement, "mergeComment");
            string strBatchNo = DomUtil.GetElementInnerText(item_dom.DocumentElement, "batchNo");
            string strBorrower = DomUtil.GetElementInnerText(item_dom.DocumentElement, "borrower");

            string strBorrowDate = GetRfc1123DisplayString(
                DomUtil.GetElementInnerText(item_dom.DocumentElement, "borrowDate"));

            string strBorrowPeriod = DomUtil.GetElementInnerText(item_dom.DocumentElement, "borrowPeriod");
            string strIntact = DomUtil.GetElementInnerText(item_dom.DocumentElement, "intact");
            string strRefID = DomUtil.GetElementInnerText(item_dom.DocumentElement, "refID");

            string strBinding = FormatInnerXml(
                DomUtil.GetElementInnerXml(item_dom.DocumentElement, "binding"));
            string strOperations = FormatInnerXml(
                DomUtil.GetElementInnerXml(item_dom.DocumentElement, "operations"));

            string strBorrowHistory = "";
            string strBorrowHistoryCount = "";
            if (bDisplayBorrowHistory == true)
            {
                strBorrowHistory = FormatInnerXml(
                 DomUtil.GetElementInnerXml(item_dom.DocumentElement, "borrowHistory"));
                strBorrowHistoryCount = DomUtil.GetElementAttr(item_dom.DocumentElement,
        "borrowHistory", "count");
            }
            // string strBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\">" + strBarcode + "</a>";

            // string strCssFilePath = PathUtil.MergePath(this.MainForm.DataDir, "operloghtml.css");

            strHtml =
                "<table class='iteminfo'>" +
                BuildHtmlEncodedLine("册条码号", BuildItemBarcodeLink(strBarcode)) +
                BuildHtmlPendingLine("(书目摘要)", BuildPendingBiblioSummary(strBarcode, strItemRecPath)) +
                BuildHtmlLine("册类型", strBookType) +
                BuildHtmlLine("册状态", strState)
                + BuildHtmlLine("出版时间", strPublishTime)
                + BuildHtmlLine("馆藏处", strLocation)
                + BuildHtmlLine("订购渠道", strSeller)
                + BuildHtmlLine("经费来源", strSource)
                + BuildHtmlLine("册价格", strPrice)
                + BuildHtmlLine("索取号", strAccessNo)
                + BuildHtmlLine("卷期册", strVolume)
                + BuildHtmlLine("登录号", strRegisterNo)
                + BuildHtmlLine("注释", strComment)
                + BuildHtmlLine("合并注释", strMergeComment)
                + BuildHtmlLine("批次号", strBatchNo)
                + BuildHtmlLine("借阅者", strBorrower)
                + BuildHtmlLine("借阅日期", strBorrowDate)
                + BuildHtmlLine("借阅期限", strBorrowPeriod)
                + BuildHtmlLine("完好率", strIntact)
                + BuildHtmlLine("参考ID", strRefID)
                + BuildHtmlLine("父记录ID", strParent)
                + BuildHtmlEncodedLine("装订信息", strBinding)
                + BuildHtmlEncodedLine("操作历史", strOperations)
                + (bDisplayBorrowHistory == true ? BuildHtmlEncodedLine("借阅历史", strBorrowHistoryCount, strBorrowHistory) : "")
                + "</table>";

            return 0;
        }

        // 包装后的版本
        // 获得违约金记录 HTML 字符串。不包括外面的<html><body>
        int GetAmerceInfoString(string strAmerceXml,
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            if (string.IsNullOrEmpty(strAmerceXml) == true)
                return 0;

            XmlDocument amerce_dom = new XmlDocument();
            try
            {
                amerce_dom.LoadXml(strAmerceXml);
            }
            catch (Exception ex)
            {
                strError = "违约金记录XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            return GetAmerceInfoString(amerce_dom,
                out strHtml,
                out strError);
        }

        // 获得违约金记录 HTML 字符串。不包括外面的<html><body>
        int GetAmerceInfoString(XmlDocument amerce_dom,
out string strHtml,
out string strError)
        {
            strHtml = "";
            strError = "";

            XmlNode node = null;
            string strLibraryCode = DomUtil.GetElementText(amerce_dom.DocumentElement, "libraryCode", out node);
            if (node != null && string.IsNullOrEmpty(strLibraryCode) == true)
                strLibraryCode = "<空>";
            string strItemBarcode = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "itemBarcode");
            string strReaderBarcode = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "readerBarcode");
            string strState = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "state");
            string strID = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "id");
            string strReason = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "reason");
            string strOverduePeriod = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "overduePeriod");

            string strPrice = DomUtil.GetElementInnerXml(amerce_dom.DocumentElement, "price");
            string strComment = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "comment");

            string strBorrowDate = GetRfc1123DisplayString(
                DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "borrowDate"));
            string strBorrowPeriod = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "borrowPeriod");
            string strBorrowOperator = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "borrowOperator");
            string strReturnDate = GetRfc1123DisplayString(
    DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "returnDate"));
            string strReturnOperator = DomUtil.GetElementInnerText(amerce_dom.DocumentElement, "returnOperator");

            string strOperator = DomUtil.GetElementText(amerce_dom.DocumentElement, "operator");
            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(amerce_dom.DocumentElement, "operTime"));

            string strSettlementOperator = DomUtil.GetElementText(amerce_dom.DocumentElement, "settlementOperator");
            string strSettlementOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(amerce_dom.DocumentElement, "settlementOperTime"));

            string strUndoSettlementOperator = DomUtil.GetElementText(amerce_dom.DocumentElement, "undoSettlementOperator");
            string strUndoSettlementOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(amerce_dom.DocumentElement, "undoSettlementOperTime"));

            /*
            string strItemBarcodeLink = "";
            if (string.IsNullOrEmpty(strItemBarcode) == false)
                strItemBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\">" + strItemBarcode + "</a>";

            string strReaderBarcodeLink = "";
            if (string.IsNullOrEmpty(strReaderBarcode) == false)
                strReaderBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + strReaderBarcode + "</a>";
            */

            strHtml =
                "<table class='amerceinfo'>" +
                BuildHtmlLine("馆代码", strLibraryCode) +
                BuildHtmlEncodedLine("册条码号", BuildItemBarcodeLink(strItemBarcode)) +
                BuildHtmlPendingLine("(书目摘要)", BuildPendingBiblioSummary(strItemBarcode,"")) +
                BuildHtmlEncodedLine("读者证条码号", BuildReaderBarcodeLink(strReaderBarcode)) +
                BuildHtmlPendingLine("(读者摘要)", BuildPendingReaderSummary(strReaderBarcode)) +
                BuildHtmlLine("状态", strState) +
                BuildHtmlLine("ID", strID) +
                BuildHtmlLine("原因", strReason) +
                BuildHtmlLine("超期", strOverduePeriod) +
                BuildHtmlLine("金额", strPrice) +
                BuildHtmlLine("注释", strComment) +

                BuildHtmlLine("起点操作者", strBorrowOperator) +
                BuildHtmlLine("起点日期", strBorrowDate) +
                BuildHtmlLine("期限", strBorrowPeriod) +

                BuildHtmlLine("终点操作者", strReturnOperator) +
                BuildHtmlLine("终点日期", strReturnDate) +

                BuildHtmlLine("收取违约金操作者", strOperator) +
                BuildHtmlLine("收取违约金操作时间", strOperTime) +

                BuildHtmlLine("结算操作者", strSettlementOperator) +
                BuildHtmlLine("结算操作时间", strSettlementOperTime) +

                BuildHtmlLine("撤销结算操作者", strUndoSettlementOperator) +
                BuildHtmlLine("撤销结算操作时间", strUndoSettlementOperTime) +
                BuildClientAddressLine(amerce_dom) +
                "</table>";

            return 0;
        }

        // 包装后的版本
        // 获得订购记录 HTML 字符串。不包括外面的<html><body>
        int GetOrderInfoString(string strOrderXml,
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            if (string.IsNullOrEmpty(strOrderXml) == true)
                return 0;

            XmlDocument order_dom = new XmlDocument();
            try
            {
                order_dom.LoadXml(strOrderXml);
            }
            catch (Exception ex)
            {
                strError = "订购记录XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            return GetOrderInfoString(order_dom,
                out strHtml,
                out strError);
        }

        // 获得订购记录 HTML 字符串。不包括外面的<html><body>
        int GetOrderInfoString(XmlDocument order_dom,
out string strHtml,
out string strError)
        {
            strHtml = "";
            strError = "";

            string strParent = DomUtil.GetElementInnerText(order_dom.DocumentElement, "parent");
            string strIndex = DomUtil.GetElementInnerText(order_dom.DocumentElement, "index");
            string strState = DomUtil.GetElementInnerText(order_dom.DocumentElement, "state");
            string strSeller = DomUtil.GetElementInnerText(order_dom.DocumentElement, "seller");
            string strSource = DomUtil.GetElementInnerText(order_dom.DocumentElement, "source");
            string strRange = DomUtil.GetElementInnerText(order_dom.DocumentElement, "range");

            string strIssueCount = DomUtil.GetElementInnerText(order_dom.DocumentElement, "issueCount");
            string strCopy = DomUtil.GetElementInnerText(order_dom.DocumentElement, "copy");
            string strPrice = DomUtil.GetElementInnerXml(order_dom.DocumentElement, "price");
            string strTotalPrice = DomUtil.GetElementInnerText(order_dom.DocumentElement, "totalPrice");

            string strOrderTime = GetRfc1123DisplayString(
    DomUtil.GetElementInnerText(order_dom.DocumentElement, "orderTime"));

            string strOrderID = DomUtil.GetElementInnerText(order_dom.DocumentElement, "orderID");

            string strDistribute = DomUtil.GetElementInnerText(order_dom.DocumentElement, "distibute");
            string strComment = DomUtil.GetElementInnerText(order_dom.DocumentElement, "comment");
            string strSellerAddress = FormatInnerXml(
                DomUtil.GetElementInnerXml(order_dom.DocumentElement, "sellerAddress"));
            string strBatchNo = DomUtil.GetElementInnerText(order_dom.DocumentElement, "batchNo");

            string strRefID = DomUtil.GetElementInnerText(order_dom.DocumentElement, "refID");

            /*
            string strOperations = FormatInnerXml(
                DomUtil.GetElementInnerXml(order_dom.DocumentElement, "operations"));
            */

            strHtml =
                "<table class='orderinfo'>" +
                BuildHtmlLine("编号", strIndex) +
                BuildHtmlLine("状态", strState) +
                BuildHtmlLine("订购渠道", strSeller) +
                BuildHtmlLine("经费来源", strSource) +
                BuildHtmlLine("时间范围", strRange) +
                BuildHtmlLine("包含期数", strIssueCount) +
                BuildHtmlLine("复本数", strCopy) +
                BuildHtmlLine("单价", strPrice) +
                BuildHtmlLine("总价格", strTotalPrice) +
                BuildHtmlLine("订购时间", strOrderTime) +
                BuildHtmlLine("订单号", strOrderID) +
                BuildHtmlLine("馆藏分配", strDistribute) +
                BuildHtmlLine("注释", strComment) +
                BuildHtmlEncodedLine("渠道地址", strSellerAddress) +
                BuildHtmlLine("批次号", strBatchNo) +
                BuildHtmlLine("参考ID", strRefID) +
                BuildHtmlLine("父记录ID", strParent) +
                "</table>";

            return 0;
        }

        // 包装后的版本
        // 获得期记录 HTML 字符串。不包括外面的<html><body>
        int GetIssueInfoString(string strIssueXml,
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            if (string.IsNullOrEmpty(strIssueXml) == true)
                return 0;

            XmlDocument issue_dom = new XmlDocument();
            try
            {
                issue_dom.LoadXml(strIssueXml);
            }
            catch (Exception ex)
            {
                strError = "期记录XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            return GetIssueInfoString(issue_dom,
                out strHtml,
                out strError);
        }

        // 获得期记录 HTML 字符串。不包括外面的<html><body>
        int GetIssueInfoString(XmlDocument issue_dom,
out string strHtml,
out string strError)
        {
            strHtml = "";
            strError = "";

            string strParent = DomUtil.GetElementInnerText(issue_dom.DocumentElement, "parent");
            string strPublishTime = DomUtil.GetElementInnerText(issue_dom.DocumentElement, "publishTime");
            string strState = DomUtil.GetElementInnerText(issue_dom.DocumentElement, "state");
            string strIssue = DomUtil.GetElementInnerText(issue_dom.DocumentElement, "issue");
            string strZong = DomUtil.GetElementInnerText(issue_dom.DocumentElement, "zong");
            string strVolume = DomUtil.GetElementInnerText(issue_dom.DocumentElement, "volume");

            string strComment = DomUtil.GetElementInnerText(issue_dom.DocumentElement, "comment");
            string strBatchNo = DomUtil.GetElementInnerText(issue_dom.DocumentElement, "batchNo");

            string strRefID = DomUtil.GetElementInnerText(issue_dom.DocumentElement, "refID");

            /*
            string strOperations = FormatInnerXml(
                DomUtil.GetElementInnerXml(order_dom.DocumentElement, "operations"));
            */

            string strOrderRecords = "";
            XmlNodeList nodes = issue_dom.DocumentElement.SelectNodes("orderInfo/*");
            int i = 0;
            foreach (XmlNode cur_node in nodes)
            {
                string strOrderRecord = cur_node.OuterXml;
                string strOrderRecPath = "";
                string strOrderRecordHtml = "";
                if (cur_node != null)
                {
                    strOrderRecPath = DomUtil.GetAttr(cur_node, "recPath");
                    int nRet = GetOrderInfoString(strOrderRecord,
    out strOrderRecordHtml,
    out strError);
                    if (nRet == -1)
                    {
                        strError = "GetIssueInfoString()内调用GetOrderInfoString()时出错: " + strError;
                        return -1;
                    }
                }
                strOrderRecords += BuildHtmlEncodedLine("订购记录 " + (i + 1).ToString() + "/" + nodes.Count.ToString(),
                    strOrderRecPath, strOrderRecordHtml);

                i++;
            }


            strHtml =
                "<table class='issueinfo'>" +
                BuildHtmlLine("出版时间", strPublishTime) +
                BuildHtmlLine("状态", strState) +
                BuildHtmlLine("期号", strIssue) +
                BuildHtmlLine("总期号", strZong) +
                BuildHtmlLine("卷号", strVolume) +

                BuildHtmlLine("注释", strComment) +

                strOrderRecords +

                BuildHtmlLine("批次号", strBatchNo) +
                BuildHtmlLine("参考ID", strRefID) +
                BuildHtmlLine("父记录ID", strParent) +
                "</table>";

            return 0;
        }

        // 包装后的版本
        // 获得评注记录 HTML 字符串。不包括外面的<html><body>
        int GetCommentInfoString(string strCommentXml,
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            if (string.IsNullOrEmpty(strCommentXml) == true)
                return 0;

            XmlDocument comment_dom = new XmlDocument();
            try
            {
                comment_dom.LoadXml(strCommentXml);
            }
            catch (Exception ex)
            {
                strError = "评注记录XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            return GetCommentInfoString(comment_dom,
                out strHtml,
                out strError);
        }

        // 获得评注记录 HTML 字符串。不包括外面的<html><body>
        int GetCommentInfoString(XmlDocument comment_dom,
out string strHtml,
out string strError)
        {
            strHtml = "";
            strError = "";

            string strParent = DomUtil.GetElementInnerText(comment_dom.DocumentElement, "parent");
            string strTitle = DomUtil.GetElementInnerText(comment_dom.DocumentElement, "title");
            string strState = DomUtil.GetElementInnerText(comment_dom.DocumentElement, "state");
            string strCreator = DomUtil.GetElementInnerText(comment_dom.DocumentElement, "creator");
            string strType = DomUtil.GetElementInnerText(comment_dom.DocumentElement, "type");
            string strOrderSuggestion = DomUtil.GetElementInnerText(comment_dom.DocumentElement, "orderSuggestion");

            string strContent = DomUtil.GetElementInnerText(comment_dom.DocumentElement, "content");
            strContent = strContent.Replace("\\r", "\r\n");
            strContent = ParseHttpString(
    strContent);
            strContent = GetHtmlContentFromPureText(
                strContent,
                Text2HtmlStyle.P);
            string strRefID = DomUtil.GetElementInnerText(comment_dom.DocumentElement, "refID");

            string strOperations = FormatInnerXml(
                DomUtil.GetElementInnerXml(comment_dom.DocumentElement, "operations"));

            strHtml =
                "<table class='commentinfo'>" +
                BuildHtmlLine("标题", strTitle) +
                BuildHtmlLine("作者", strCreator) +
                BuildHtmlLine("状态", strState) +
                BuildHtmlLine("评注类型", strType) +
                BuildHtmlLine("订购建议", strOrderSuggestion) +
                BuildHtmlEncodedLine("正文", strContent) +
                BuildHtmlEncodedLine("操作历史", strOperations) +
                BuildHtmlLine("参考ID", strRefID) +
                BuildHtmlLine("父记录ID", strParent) +
                "</table>";

            return 0;
        }

        // 包装后的版本
        // 获得帐户记录 HTML 字符串。不包括外面的<html><body>
        int GetAccountInfoString(string strAccountXml,
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            if (string.IsNullOrEmpty(strAccountXml) == true)
                return 0;

            XmlDocument account_dom = new XmlDocument();
            try
            {
                account_dom.LoadXml(strAccountXml);
            }
            catch (Exception ex)
            {
                strError = "帐户记录XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            return GetAccountInfoString(account_dom,
                out strHtml,
                out strError);
        }

        // 获得帐户记录 HTML 字符串。不包括外面的<html><body>
        int GetAccountInfoString(XmlDocument account_dom,
out string strHtml,
out string strError)
        {
            strHtml = "";
            strError = "";

            string strName = DomUtil.GetAttr(account_dom.DocumentElement, "name");
            string strType = DomUtil.GetAttr(account_dom.DocumentElement, "type");
            string strRights = DomUtil.GetAttr(account_dom.DocumentElement, "rights");
            string strLibraryCode = DomUtil.GetAttr(account_dom.DocumentElement, "libraryCode");
            string strAccess = DomUtil.GetAttr(account_dom.DocumentElement, "access");
            string strComment = DomUtil.GetAttr(account_dom.DocumentElement, "comment");

            strHtml =
                "<table class='accountinfo'>" +
                BuildHtmlLine("用户名", strName) +
                BuildHtmlLine("图书馆代码", strLibraryCode) +
                BuildHtmlLine("类型", strType) +
                BuildClassHtmlEncodedLine("权限", "", 
                strRights.Replace(",", ",<wbr/>"), "rights") +
                BuildHtmlLine("存取代码", strAccess) +
                BuildHtmlLine("注释", strComment) +
                "</table>";

            return 0;
        }

        // 将纯文本中的"http://"替换为<a>命令
        static string ParseHttpString(
            string strText)
        {
            string strResult = "";
            int nCur = 0;
            for (; ; )
            {
                int nStart = strText.IndexOf("http://", nCur);
                if (nStart == -1)
                {
                    strResult += ReplaceLeadingBlank(HttpUtility.HtmlEncode(strText.Substring(nCur)));
                    break;
                }

                // 复制nCur到nStart一段
                strResult += ReplaceLeadingBlank(HttpUtility.HtmlEncode(strText.Substring(nCur, nStart - nCur)));

                int nEnd = strText.IndexOfAny(new char[] { ' ', ',', ')', '(', '\r', '\n', '\"', '\'' },
                    nStart + 1);
                if (nEnd == -1)
                    nEnd = strText.Length;

                string strUrl = strText.Substring(nStart, nEnd - nStart);

                string strLeft = "<a href='" + strUrl + "' target='_blank'>";
                string strRight = "</a>";

                strResult += strLeft + HttpUtility.HtmlEncode(strUrl) + strRight;

                nCur = nEnd;
            }

            return strResult;
        }

        // 把一个字符串开头的连续空白替换为&nbsp;
        static string ReplaceLeadingBlank(string strText)
        {
            if (strText == "")
                return "";
            strText = strText.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
            return strText;
        }

        enum Text2HtmlStyle
        {
            BR = 0,
            P = 1,
        }

        // 把纯文本变为适合html显示的格式
        static string GetHtmlContentFromPureText(
            string strText,
            Text2HtmlStyle style)
        {
            string[] aLine = strText.Replace("\r", "").Split(new char[] { '\n' });
            string strResult = "";
            for (int i = 0; i < aLine.Length; i++)
            {
                string strLine = aLine[i];

                if (style == Text2HtmlStyle.BR)
                {
                    strResult += strLine + "<br/>";
                }
                else if (style == Text2HtmlStyle.P)
                {
                    if (String.IsNullOrEmpty(strLine) == true)
                        strResult += "<p>&nbsp;</p>";
                    else
                    {
                        strResult += "<p>" + strLine + "</p>";
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(strLine) == true)
                        strResult += "<p>&nbsp;</p>";
                    else
                    {
                        strResult += "<p>" + strLine + "</p>";
                    }
                }
            }

            return strResult;
        }

        #endregion

        private void listView_records_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.DownPannelVisible == true)
            {
                if (this.listView_records.SelectedItems.Count == 1)
                {
                    /*
                    int index = this.listView_records.SelectedIndices[0];

                    string strFileName = this.m_tempFileNames[index];
                     * */
                    ListViewItem item = this.listView_records.SelectedItems[0];

                    if (this.StoreInTempFile == true)
                    {
                        OperLogItemInfo info = (OperLogItemInfo)item.Tag;

                        int index = info.IndexOfTempFilename;
                        string strFileName = this.m_tempFileNames[index];

                        string strTargetFileName = MainForm.DataDir + "\\xml.xml";

                        File.Copy(strFileName, strTargetFileName, true);

                        this.webBrowser_xml.Navigate(strTargetFileName);
                    }
                    else
                    {
                        string strXml = "";
                        string strError = "";
                        // 从服务器获得
                        // return:
                        //      -1  出错
                        //      0   正常
                        //      1   用户中断
                        int nRet = GetXml(item,
                out strXml,
                out strError);
                        if (nRet == 1)
                            return;
                        if (nRet == -1)
                        {
                            Global.SetHtmlString(this.webBrowser_xml,
                                strError,
                                this.MainForm.DataDir,
                                "operlogerror");
                        }
                        else
                        {
                            Global.SetXmlString(this.webBrowser_xml,
        strXml,
        this.MainForm.DataDir,
        "operlogexml");

                            string strHtml = "";
                            // 创建解释日志记录内容的 HTML 字符串
                            // return:
                            //      -1  出错
                            //      0   成功
                            //      1   未知的操作类型
                            nRet = GetHtmlString(strXml,
                                true,
                                out strHtml,
                                out strError);
                            if (nRet == -1 || nRet == 1)
                                Global.SetHtmlString(this.webBrowser_html,
                                    strError,
                                    this.MainForm.DataDir,
                                    "operlogerror_html");
                            else
                            {
                                if (string.IsNullOrEmpty(strHtml) == true)
                                    Global.SetHtmlString(this.webBrowser_html,
                                        NOTSUPPORT,
                                        this.MainForm.DataDir,
                                        "operloghtml");
                                else
                                    Global.SetHtmlString(this.webBrowser_html,
                                        strHtml,
                                        this.MainForm.DataDir,
                                        "operloghtml");
                            }

                        }

                    }
                }
                else
                {
                    Global.ClearHtmlPage(this.webBrowser_xml,
                        this.MainForm.DataDir);
                    Global.ClearHtmlPage(this.webBrowser_html,
                        this.MainForm.DataDir);
                }
            }

            DoViewOperlog(false);
        }

        LibraryChannelCollection Channels = null;

        // return:
        //      -1  出错
        //      0   正常
        //      1   用户中断
        int GetXml(ListViewItem item,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";
            int nRet = 0;

            OperLogItemInfo info = (OperLogItemInfo)item.Tag;

            this.Channels.Close();  // 中断前面的所有操作

            LibraryChannel channel = this.Channels.NewChannel(MainForm.LibraryServerUrl);
            channel.Url = this.MainForm.LibraryServerUrl;
            try
            {
                string strLogFileName = ListViewUtil.GetItemText(item, COLUMN_FILENAME);
                string strIndex = ListViewUtil.GetItemText(item, COLUMN_INDEX);
                long lIndex = -1;
                if (Int64.TryParse(strIndex, out lIndex) == false)
                {
                    strError = "序号值 '" + strIndex + "' 格式不正确";
                    return -1;
                }

                if (info.InCacheFile == false)
                {
                    // 从服务器获取
                    long lHintNext = -1;
                    long lAttachmentTotalLength = 0;
                    byte[] attachment_data = null;
                    // 获得日志
                    // result.Value
                    //      -1  error
                    //      0   file not found
                    //      1   succeed
                    //      2   超过范围
                    long lRet = channel.GetOperLog(
                        null,
                        strLogFileName,
                        lIndex,
                        info.Hint,
                        "level-" + this.MainForm.OperLogLevel.ToString(),
                        "", // strFilter
                        out strXml,
                        out lHintNext,
                        0,  // lAttachmentFragmentStart,
                        0,  // nAttachmentFramengLength,
                        out attachment_data,
                        out lAttachmentTotalLength,
                        out strError);
                    if (channel.ErrorCode == ErrorCode.RequestCanceled)
                        return 1;
                    if (lRet != 1)
                        return -1;
                }
                else
                {
                    string strCacheFilename = PathUtil.MergePath(this.MainForm.OperLogCacheDir, strLogFileName);
                    using (Stream stream = File.Open(
strCacheFilename,
FileMode.Open,
FileAccess.ReadWrite,
FileShare.ReadWrite))
                    {
                        long lHint = info.Hint;

                        if (lHint == -1)
                        {
                            // return:
                            //      -1  error
                            //      0   成功
                            //      1   到达文件末尾或者超出
                            nRet = LocationRecord(stream,
                lIndex,
                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                        else
                        {
                            // 根据暗示找到
                            if (lHint == stream.Length)
                            {
                                strError = "lHint [" + lHint.ToString() + "] 超过文件尺寸";
                                return -1;
                            }

                            if (lHint > stream.Length)
                            {
                                strError = "lHint参数值不正确";
                                return -1;
                            }
                            if (stream.Position != lHint)
                                stream.Seek(lHint, SeekOrigin.Begin);
                        }

                        long lAttachmentTotalLength = 0;
                        nRet = ReadCachedEnventLog(
                            stream,
                            out strXml,
                            out lAttachmentTotalLength,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }

                return 0;
            }
            finally
            {
                channel.Close();
                this.Channels.RemoveChannel(channel);
            }
        }

#if NO
        void ReleaseAllChannelsBut(LibraryChannel channel)
        {
            for (int i = 0; i < this.Channels.Count; i++)
            {
                LibraryChannel cur_channel = this.Channels[i];

                if (cur_channel == channel)
                    continue;

                channel.Abort();
                this.Channels.RemoveChannel(channel);
                i--;
            }
        }
#endif

        // 填充listviewitem第一、第二列以外的其它列
        int FillListViewItem(ListViewItem item,
            string strXml,
            long lAttachmentTotalLength,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装载到DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNode librarycode_node = null;
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out librarycode_node);
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = DomUtil.GetElementText(dom.DocumentElement, "operTime");

            strOperTime = DateTimeUtil.Rfc1123DateTimeStringToLocal(strOperTime, "s");  // Sortable date/time pattern; conforms to ISO 8601

            if (librarycode_node == null)
                item.SubItems.Add("<无>");
            else
                item.SubItems.Add(strLibraryCode);
            item.SubItems.Add(strOperation + (string.IsNullOrEmpty(strAction) == false ? " / " + strAction : ""));
            item.SubItems.Add(strOperator);
            item.SubItems.Add(strOperTime);

            if (lAttachmentTotalLength > 0)
                item.SubItems.Add(lAttachmentTotalLength.ToString());
            else
                item.SubItems.Add("");

            return 0;
        }

        private void OperLogForm_Activated(object sender, EventArgs e)
        {
            this.MainForm.stopManager.Active(this.stop);

            this.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            this.MainForm.MenuItem_font.Enabled = false;
            this.MainForm.MenuItem_restoreDefaultFont.Enabled = false;

        }

        private void listView_records_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第2列为id，排序风格特殊
            if (nClickColumn == 1)
                sortStyle = ColumnSortStyle.RightAlign;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_records.Columns,
                true);

            // 排序
            this.listView_records.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView_records.ListViewItemSorter = null;

        }

        // 获得日志文件名
        private void button_loadFilenams_Click(object sender, EventArgs e)
        {
            int x = 0;
            int y = 0;
            API.GetEditCurrentCaretPos(
                this.textBox_filenames,
                out x,
                out y);

            string strLine = "";

            if (this.textBox_filenames.Lines.Length > 0)
                strLine = this.textBox_filenames.Lines[y];

            GetOperLogFilenameDlg dlg = new GetOperLogFilenameDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            if (String.IsNullOrEmpty(strLine) == false)
                dlg.OperLogFilenames.Add(strLine);

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            string strText = "";
            if (dlg.OperLogFilenames.Count == 1)
                strText = dlg.OperLogFilenames[0];
            else
            {
                for (int i = 0; i < dlg.OperLogFilenames.Count; i++)
                {
                    if (i != 0)
                        strText += "\r\n";
                    strText += dlg.OperLogFilenames[i];
                }
            }
            Global.SetLineText(this.textBox_filenames, y, strText);

            this.textBox_filenames.Focus();

            // API.PostMessage(this.Handle, WM_SETCARETPOS, x, y);
        }

        /*
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SETCARETPOS:
                    {
                        API.SetEditCurrentCaretPos(this.textBox_filenames,
                            (int)m.WParam, 
                            (int)m.LParam,
                            true);
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }*/



        ProgressEstimate estimate = new ProgressEstimate();

        int DoRecord(string strLogFileName,
            string strXml,
            bool bInCacheFile,
            long lHint,
            long lIndex,
            long lAttachmentTotalLength,
            object param,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

            OperLogItemInfo info = new OperLogItemInfo();

            if (this.StoreInTempFile == true)
            {
                // 创建临时文件
                string strTempFileName = Path.GetTempFileName();
                Stream stream = File.Create(strTempFileName);

                // 写入xml内容
                byte[] buffer = Encoding.UTF8.GetBytes(strXml);
                stream.Write(buffer, 0, buffer.Length);

                stream.Close();

                m_tempFileNames.Add(strTempFileName);
                info.IndexOfTempFilename = m_tempFileNames.Count - 1;
            }
            else
            {
                info.Hint = lHint;
                info.InCacheFile = bInCacheFile;
            }
            ListViewItem item = new ListViewItem(strLogFileName, 0);
            item.SubItems.Add(lIndex.ToString());  // 序号从0开始计数
            this.listView_records.Items.Add(item);
            item.Tag = info;

            int nRet = FillListViewItem(item,
                strXml,
                lAttachmentTotalLength,
                out strError);
            if (nRet == -1)
                return -1;

            if ((lIndex % 100) == 0)
                this.listView_records.ForceUpdate();

            return 0;
        }

        // 装载日志记录
        private void button_loadLogRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this.tabControl_main.SelectedTab = this.tabPage_logRecords;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在初始化浏览器组件 ...");
            stop.BeginLoop();


            this.Update();
            this.MainForm.Update();

#if DELAY_UPDATE
            this.listView_records.BeginUpdate();
#endif
#if NO
            this.listView_records.Items.Clear();
            // 2008/11/22 new add
            this.SortColumns.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_records.Columns);


            this.ClearAllTempFiles();

            Global.ClearHtmlPage(this.webBrowser_xml,
                this.MainForm.DataDir);
            Global.ClearHtmlPage(this.webBrowser_html,
                this.MainForm.DataDir);
#endif
            this.Clear();

            try
            {
                stop.SetMessage("正在准备日志文件名 ...");
                List<string> lines = new List<string>();
                for (int i = 0; i < this.textBox_filenames.Lines.Length; i++)
                {
                    Application.DoEvents();

                    if (stop != null)
                    {
                        if (stop.State != 0)
                            return;
                    }

                    string strLine = this.textBox_filenames.Lines[i];
                    lines.Add(strLine);
                }

                string strStyle = "";
                if (this.MainForm.AutoCacheOperlogFile == true)
                    strStyle = "autocache";

                nRet = ProcessFiles(this,
                    stop,
                    this.estimate,
                    Channel,
                    lines,
                    this.MainForm.OperLogLevel,
                    strStyle,
                    this.MainForm.OperLogCacheDir,
                    null,   // param,
                    DoRecord,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {

#if DELAY_UPDATE
                this.listView_records.EndUpdate();
#endif

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }

            this.MainForm.StatusBarMessage = "总共耗费时间: " + this.estimate.GetTotalTime().ToString();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

#if OLDOLDOLD
        // 获得一个日志文件的尺寸
        // return:
        //      -1  error
        //      0   file not found
        //      1   found
        int GetFileSize(string strLogFileName,
            out long lTotalSize,
            out string strError)
        {
            strError = "";
            lTotalSize = 0;

            stop.SetMessage("正获得日志文件 " + strLogFileName + " 的尺寸...");

            string strXml = "";
            long lAttachmentTotalLength = 0;
            byte[] attachment_data = null;

            // 获得日志文件尺寸
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            //      2   超过范围
            long lRet = Channel.GetOperLog(
                stop,
                strLogFileName,
                -1,    // lIndex,
                -1, // lHint,
                out strXml,
                out lTotalSize,
                0,  // lAttachmentFragmentStart,
                0,  // nAttachmentFramengLength,
                out attachment_data,
                out lAttachmentTotalLength,
                out strError);
            if (lRet == 0)
            {
                lTotalSize = 0;
                Debug.Assert(lTotalSize == 0, "");
                return 0;
            }
            if (lRet != 1)
                return -1;
            Debug.Assert(lTotalSize >= 0, "");

            return 1;
        }

        // 装入一个日志文件中的若干记录
        // return:
        //      -1  error
        //      0   file not found
        //      1   found
        int LoadSomeRecords(string strLogFileName,
            string strRange,
            ref long lProgressValue,
            ref long lSize,
            out string strError)
        {
            strError = "";

            stop.SetMessage("正在装入日志文件 " + strLogFileName + " 中的记录。"
                + "剩余时间 " + ProgressEstimate.Format(this.estimate.Estimate(lProgressValue)) + " 已经过时间 " + ProgressEstimate.Format(this.estimate.delta_passed));

            string strXml = "";
            long lAttachmentTotalLength = 0;
            byte[] attachment_data = null;

            long lFileSize = 0;

            // 获得日志文件尺寸
            long lRet = Channel.GetOperLog(
                stop,
                strLogFileName,
                -1,    // lIndex,
                -1, // lHint,
                out strXml,
                out lFileSize,
                0,  // lAttachmentFragmentStart,
                0,  // nAttachmentFramengLength,
                out attachment_data,
                out lAttachmentTotalLength,
                out strError);
            // 2010/12/13
            if (lRet == 0)
                return 0;

            // stop.SetProgressRange(0, lTotalSize);

            if (String.IsNullOrEmpty(strRange) == true)
                strRange = "0-9999999999";

            RangeList rl = new RangeList(strRange);

#if DELAY_UPDATE
            this.listView_records.BeginUpdate();
#endif
            try
            {
                for (int i = 0; i < rl.Count; i++)
                {
                    RangeItem ri = (RangeItem)rl[i];

                    long lHint = -1;
                    long lHintNext = -1;
                    for (long lIndex = ri.lStart; lIndex < ri.lStart + ri.lLength; lIndex++)
                    {
                        Application.DoEvents();

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断1";
                                goto ERROR1;
                            }
                        }

                        lHint = lHintNext;

                        // 获得日志
                        // result.Value
                        //      -1  error
                        //      0   file not found
                        //      1   succeed
                        //      2   超过范围
                        lRet = Channel.GetOperLog(
                            stop,
                            strLogFileName,
                            lIndex,
                            lHint,
                            out strXml,
                            out lHintNext,
                            0,  // lAttachmentFragmentStart,
                            0,  // nAttachmentFramengLength,
                            out attachment_data,
                            out lAttachmentTotalLength,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                        if (lRet == 0)
                            return 0;

                        if (lRet == 2)
                            break;

#if NO
                            // 2011/12/30
                            // 日志记录可能动态地增加了，超过了原先为ProgressBar设置的范围
                            if (lFizeTotalSize < (int)lHintNext)
                            {
                                lFizeTotalSize = lHintNext;

                                stop.SetProgressRange(0, lFizeTotalSize);
                            }
#endif
                        // 校正
                        if (lProgressValue + lHintNext > lSize)
                        {
                            lSize = lProgressValue + lHintNext;

                            stop.SetProgressRange(0, lSize);
                            this.estimate.SetRange(0, lSize);
                        }

                            stop.SetProgressValue(lProgressValue + lHintNext);

                        if (lIndex % 100 == 0)
                        {
                            stop.SetMessage("正在装入日志文件 " + strLogFileName + " 中的记录 "+lIndex.ToString()+" 。"
    + "剩余时间 " + ProgressEstimate.Format(this.estimate.Estimate(lProgressValue + lHintNext)) + " 已经过时间 " + ProgressEstimate.Format(this.estimate.delta_passed));
                        }

                        if (string.IsNullOrEmpty(strXml) == false)
                        {
                            OperLogItemInfo info = new OperLogItemInfo();

                            if (this.StoreInTempFile == true)
                            {
                                // 创建临时文件
                                string strTempFileName = Path.GetTempFileName();
                                Stream stream = File.Create(strTempFileName);

                                // 写入xml内容
                                byte[] buffer = Encoding.UTF8.GetBytes(strXml);
                                stream.Write(buffer, 0, buffer.Length);

                                stream.Close();

                                m_tempFileNames.Add(strTempFileName);
                                info.IndexOfTempFilename = m_tempFileNames.Count - 1;
                            }
                            else
                            {
                                info.Hint = lHint;
                            }
                            ListViewItem item = new ListViewItem(strLogFileName, 0);
                            item.SubItems.Add(lIndex.ToString());  // 序号从0开始计数
                            this.listView_records.Items.Add(item);
                            item.Tag = info;

                            int nRet = FillListViewItem(item,
                                strXml,
                                lAttachmentTotalLength,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
                    }
                }

                lProgressValue += lFileSize;
            }
            finally
            {
#if DELAY_UPDATE
                this.listView_records.EndUpdate();
#endif
            }

            return 1;
        ERROR1:
            return -1;
        }

#endif

        private void button_getSingleLogFilename_Click(object sender, EventArgs e)
        {
            GetOperLogFilenameDlg dlg = new GetOperLogFilenameDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            if (String.IsNullOrEmpty(this.textBox_logFileName.Text) == false)
                dlg.OperLogFilenames.Add(this.textBox_logFileName.Text);

            dlg.SingleMode = true;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // string strText = "";
            if (dlg.OperLogFilenames.Count == 1)
                textBox_logFileName.Text = dlg.OperLogFilenames[0];
            else
            {
                Debug.Assert(false, "");
            }
        }


        // 
        /// <summary>
        /// 根据日志文件名文件，装载日志记录
        /// </summary>
        /// <param name="strFilename">日志文件名的文件</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int LoadFromFilenamesFile(
            string strFilename,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(strFilename, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + strFilename + " 失败: " + ex.Message;
                return -1;
            }

            this.textBox_filenames.Text = sr.ReadToEnd();
            sr.Close();

            this.tabControl_main.SelectedTab = this.tabPage_logRecords;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在初始化浏览器组件 ...");
            stop.BeginLoop();


            this.Update();
            this.MainForm.Update();

#if NO
            this.listView_records.Items.Clear();
            // 2008/11/22 new add
            this.SortColumns.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_records.Columns);


            this.ClearAllTempFiles();

            Global.ClearHtmlPage(this.webBrowser_xml,
                this.MainForm.DataDir);
            Global.ClearHtmlPage(this.webBrowser_html,
                this.MainForm.DataDir);
#endif
            this.Clear();

            try
            {
                stop.SetMessage("正在准备日志文件名 ...");
                List<string> lines = new List<string>();
                for (int i = 0; i < this.textBox_filenames.Lines.Length; i++)
                {
                    Application.DoEvents();

                    if (stop != null)
                    {
                        if (stop.State != 0)
                            return 0;
                    }

                    string strLine = this.textBox_filenames.Lines[i];
                    if (String.IsNullOrEmpty(strLine) == true)
                        continue;

                    lines.Add(strLine);
                }

                string strStyle = "";
                if (this.MainForm.AutoCacheOperlogFile == true)
                    strStyle = "autocache";

                nRet = ProcessFiles(this,
    stop,
    this.estimate,
    Channel,
    lines,
    this.MainForm.OperLogLevel,
    strStyle,
    this.MainForm.OperLogCacheDir,
    null,   // param,
    DoRecord,
    out strError);
                if (nRet == -1)
                    return -1;
            }
            finally
            {
                /*
                if (sr != null)
                    sr.Close();
                 * */

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }

            return 0;
            /*
        ERROR1:
            return -1;
             * */
        }

        private void button_repair_findSourceFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要修复的源日志文件名";
            dlg.FileName = this.textBox_repair_sourceFilename.Text;
            // dlg.InitialDirectory = 
            dlg.Filter = "日志文件 (*.log)|*.log|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_repair_sourceFilename.Text = dlg.FileName;
        }

        private void button_repair_findTargetFilename_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定修复操作要创建的目标日志文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = this.textBox_repair_targetFilename.Text;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "日志文件 (*.log)|*.log|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_repair_targetFilename.Text = dlg.FileName;
        }

        private void button_repair_repair_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this.EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在修复日志文件 "+this.textBox_repair_sourceFilename.Text+" ...");
            stop.BeginLoop();
            try
            {
                nRet = RepairLogFile(this.textBox_repair_sourceFilename.Text,
                        this.textBox_repair_targetFilename.Text,
                        out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);
            }

            MessageBox.Show(this, "修复完成。\r\n\r\n共丢弃 "+nRet.ToString()+" 个段落。\r\n\r\n修复后的内容已经写入目标文件 " + this.textBox_repair_targetFilename.Text);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // return:
        //      -1  出错
        //      >=0 丢弃的段落数
        int RepairLogFile(string strSourceFilename,
            string strTargetFilename,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            int nTryCount = 0;

            if (String.IsNullOrEmpty(strSourceFilename) == true)
            {
                strError = "源文件名不能为空";
                return -1;
            }

            if (String.IsNullOrEmpty(strTargetFilename) == true)
            {
                strError = "目标文件名不能为空";
                return -1;
            }

            Stream target = null;
            Stream source = null;

            try
            {
                source = File.Open(
                    strSourceFilename,
                    FileMode.Open,
                    FileAccess.ReadWrite, // Read会造成无法打开 2007/5/22
                    FileShare.ReadWrite);
            }
            catch (FileNotFoundException /*ex*/)
            {
                strError = "源日志文件 " + strSourceFilename + "没有找到";
                return -1;   // file not found
            }
            catch (Exception ex)
            {
                strError = "打开源日志文件 '" + strSourceFilename + "' 时发生错误: " + ex.Message;
                return -1;
            }

            try
            {
                // 如果文件存在，就打开，如果文件不存在，就创建一个新的
                target = File.Open(
                    strTargetFilename,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);
            }
            catch (Exception ex)
            {
                strError = "打开或创建文件 '" + strTargetFilename + "' 时发生错误: " + ex.Message;
                return -1;
            }

            stop.SetProgressRange(0, source.Length);

            bool bTry = false;
            // TODO: 要汇报丢弃的段数
            for (; ; )
            {
                Application.DoEvents();

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断1";
                        return -1;
                    }
                }

                long lStart = source.Position;

                nRet = VerifyLogRecord(
                    source,
                    out strError);
                if (nRet == -1)
                {
                    if (source.Length <= lStart + 1)
                        break;
                    source.Seek(lStart + 1, SeekOrigin.Begin);
                    bTry = true;
                    continue;
                }

                if (bTry == true)
                {
                    nTryCount++;
                    bTry = false;
                }

                stop.SetProgressValue(source.Position);

                long lBodyLength = source.Position - lStart;
                // 写入目标文件
                source.Seek(lStart, SeekOrigin.Begin);

                int chunk_size = 4096;
                byte[] chunk = new byte[chunk_size];
                long writed_length = 0;
                for (; ; )
                {
                    int nThisSize = Math.Min(chunk_size, (int)(lBodyLength - writed_length));
                    int nReaded = source.Read(chunk, 0, nThisSize);
                    if (nReaded < nThisSize)
                    {
                        strError = "读入不足";
                        return -1;
                    }

                    target.Write(chunk, 0, nReaded);

                    writed_length += nReaded;
                    if (writed_length >= lBodyLength)
                        break;
                }

                if (source.Position >= source.Length)
                    break;
            }


            source.Close();
            target.Close();

            return nTryCount;
        }

        // 从当前位置开始，校验是否为一个日志记录
        static int VerifyLogRecord(
            Stream stream,
            out string strError)
        {
            strError = "";

            long lStart = stream.Position;	// 记忆起始位置


            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "VerifyLogRecord()从偏移量 " + lStart.ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。起始位置不正确";
                return -1;
            }

            Int64 lRecordLength = BitConverter.ToInt64(length, 0);

            if (lRecordLength == 0)
            {
                strError = "VerifyLogRecord()从偏移量 " + lStart.ToString() + " 开始读入了8个byte，其整数值为0，表明不是正常的记录长度";
                return -1;
            }

            Debug.Assert(lRecordLength != 0, "");

            if (lRecordLength < 0)
            {
                strError = "lRecordLength = " + lRecordLength.ToString() + "不正常，为负数";
                return -1;
            }

            if (lRecordLength > stream.Length - (lStart + 8))
            {
                strError = "lRecordLength = "+lRecordLength.ToString()+"不正常，超过文件尾部";
                return -1;
            }

            // 验证xml事项
            nRet = VerifyEntry(stream,
                out strError);
            if (nRet == -1)
                return -1;

            // 读出attachment事项
            nRet = VerifyAttachmentEntry(
                stream,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 验证一个事项(string类型)
        static int VerifyEntry(
            Stream stream,
            /*
            out string strMetaData,
            out string strBody,
             * */
            out string strError)
        {
            /*
            strMetaData = "";
            strBody = "";
             * */
            strError = "";

            long lStart = stream.Position;  // 保留起始位置

            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "VerifyEntry()从偏移量 " + lStart.ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。起始位置不正确";
                return -1;
            }

            Int64 lEntryLength = BitConverter.ToInt64(length, 0);

            if (lEntryLength < 0)
            {
                strError = "XML记录体部分长度(" + lEntryLength.ToString() + ")小于0，错误";
                return -1;
            }
            if (lEntryLength > stream.Length - stream.Position)
            {
                strError = "XML记录体长度(" + lEntryLength.ToString() + ")超过文件剩余部分尺寸";
                return -1;
            }

            // metadata长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "VerifyEntry()从偏移量 " + (lStart+8).ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。metadata长度部分不足";
                return -1;
            }

            Int64 lMetaDataLength = BitConverter.ToInt64(length, 0);

            if (lMetaDataLength < 0)
            {
                strError = "metadata长度(" + lMetaDataLength.ToString() + ")小于0，错误";
                return -1;
            }
            if (lMetaDataLength > 100 * 1024)
            {
                strError = "metadata长度("+lMetaDataLength.ToString()+")超过100K，不正常";
                return -1;
            }

            /*
            if (lMetaDataLength > 0)
            {
                byte[] metadatabody = new byte[(int)lMetaDataLength];

                nRet = stream.Read(metadatabody, 0, (int)lMetaDataLength);
                if (nRet < (int)lMetaDataLength)
                {
                    strError = "metadata不足其长度定义";
                    return -1;
                }

                strMetaData = Encoding.UTF8.GetString(metadatabody);
            }
             * */
            if (lMetaDataLength > stream.Length - stream.Position)
            {
                strError = "XML记录体metadata部分长度(" + lMetaDataLength.ToString() + ")超过文件剩余部分尺寸";
                return -1;
            } 
            stream.Seek(lMetaDataLength, SeekOrigin.Current);

            long lBodyStart = stream.Position;

            // strBody长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "VerifyEntry()从偏移量 " + lBodyStart.ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。body长度部分不足";
                return -1;
            }

            Int64 lBodyLength = BitConverter.ToInt64(length, 0);

            if (lBodyLength < 0)
            {
                strError = "body长度(" + lBodyLength.ToString() + ")小于0，错误";
                return -1;
            }
            if (lBodyLength > 1000 * 1024)
            {
                strError = "body长度("+lBodyLength.ToString()+")超过1000K，不正常";
                return -1;
            }

            /*
            if (lBodyLength > 0)
            {
                byte[] xmlbody = new byte[(int)lBodyLength];

                nRet = stream.Read(xmlbody, 0, (int)lBodyLength);
                if (nRet < (int)lBodyLength)
                {
                    strError = "body不足其长度定义";
                    return -1;
                }

                strBody = Encoding.UTF8.GetString(xmlbody);
            }
             * */
            if (lBodyLength > stream.Length - stream.Position)
            {
                strError = "XML记录体body部分长度(" + lBodyLength.ToString() + ")超过文件剩余部分尺寸";
                return -1;
            } 
            stream.Seek(lBodyLength, SeekOrigin.Current);

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lEntryLength + 8)
            {
                strError = "entry长度经检验不正确";
                return -1;
            }

            /*
            if (lEntryLength != lMetaDataLength + 8 + lBodyLength + 8)
            {

            }*/

            return 0;
        }

        // 验证一个事项(Stream类型)
        static int VerifyAttachmentEntry(
            Stream stream,
            /*
            out string strMetaData,
            ref Stream streamBody,
             * */
            out string strError)
        {
            strError = "";
            /*
            strMetaData = "";
             * */

            long lStart = stream.Position;  // 保留起始位置

            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "VerifyAttachmentEntry()从偏移量 " + lStart.ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。起始位置不正确";
                return -1;
            }

            Int64 lEntryLength = BitConverter.ToInt64(length, 0);
            if (lEntryLength < 0)
            {
                strError = "Attachment长度(" + lEntryLength.ToString() + ")小于0，错误";
                return -1;
            }
            if (lEntryLength > stream.Length - stream.Position)
            {
                strError = "Attachment长度(" + lEntryLength.ToString() + ")超过文件剩余部分尺寸";
                return -1;
            }

            // metadata长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "VerifyAttachmentEntry()从偏移量 " + lStart.ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。metadata长度位置不足8bytes";
                return -1;
            }

            Int64 lMetaDataLength = BitConverter.ToInt64(length, 0);

            if (lMetaDataLength < 0)
            {
                strError = "metadata长度(" + lMetaDataLength.ToString() + ")小于0，错误";
                return -1;
            } 
            if (lMetaDataLength > 100 * 1024)
            {
                strError = "metadata长度超过100K，不正常";
                return -1;
            }

            /*
            if (lMetaDataLength > 0)
            {
                byte[] metadatabody = new byte[(int)lMetaDataLength];

                nRet = stream.Read(metadatabody, 0, (int)lMetaDataLength);
                if (nRet < (int)lMetaDataLength)
                {
                    strError = "metadata不足其长度定义";
                    return -1;
                }

                strMetaData = Encoding.UTF8.GetString(metadatabody);
            }*/
            if (lMetaDataLength > stream.Length - stream.Position)
            {
                strError = "attachment体metadata部分长度(" + lMetaDataLength.ToString() + ")超过文件剩余部分尺寸";
                return -1;
            }
            stream.Seek(lMetaDataLength, SeekOrigin.Current);

            long lBodyStart = stream.Position;
            // body长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "VerifyAttachmentEntry()从偏移量 " + lBodyStart.ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。body长度部分不足";
                return -1;
            }

            Int64 lBodyLength = BitConverter.ToInt64(length, 0);

            if (lBodyLength < 0)
            {
                strError = "body长度(" + lBodyLength.ToString() + ")小于0，错误";
                return -1;
            } 
            if (lBodyLength > stream.Length - stream.Position)
            {
                strError = "记录格式不正确，body长度("+lBodyLength.ToString()+")超过文件剩余部分尺寸";
                return -1;
            }

            /*
            if (lBodyLength > 0)
            {
                if (streamBody == null)
                {
                    // 优化
                    stream.Seek(lBodyLength, SeekOrigin.Current);
                }
                else
                {
                    // 把数据dump到输出流中
                    int chunk_size = 4096;
                    byte[] chunk = new byte[chunk_size];
                    long writed_length = 0;
                    for (; ; )
                    {
                        int nThisSize = Math.Min(chunk_size, (int)(lBodyLength - writed_length));
                        int nReaded = stream.Read(chunk, 0, nThisSize);
                        if (nReaded < nThisSize)
                        {
                            strError = "读入不足";
                            return -1;
                        }

                        if (streamBody != null)
                            streamBody.Write(chunk, 0, nReaded);

                        writed_length += nReaded;
                        if (writed_length >= lBodyLength)
                            break;
                    }
                }

            }
            */
            if (lBodyLength > stream.Length - stream.Position)
            {
                strError = "attachment体body部分长度(" + lBodyLength.ToString() + ")超过文件剩余部分尺寸";
                return -1;
            }
            stream.Seek(lBodyLength, SeekOrigin.Current);

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lEntryLength + 8)
            {
                strError = "entry长度经检验不正确";
                return -1;
            }

            return 0;
        }

        private void button_repir_findVerifyFolderName_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.Description = "请指定要验证的日志文件所在的目录:";
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dlg.SelectedPath = this.textBox_repair_verifyFolderName.Text;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_repair_verifyFolderName.Text = dlg.SelectedPath;

        }

        private void button_repair_verify_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.textBox_repair_verifyFolderName.Text == "")
            {
                strError = "尚未指定要验证的目录";
                goto ERROR1;
            }

            List<string> errorfilenames = null;
            int nFileCount = 0;

            this.EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在验证目录 "+this.textBox_repair_verifyFolderName.Text +" 中的所有日志文件 ...");
            stop.BeginLoop();
            try
            {
                // return:
                //      -1  运行出错
                //      >=0 发生错误的文件数。文件名在errorfilenames中
                nRet = VerifyLogFiles(this.textBox_repair_verifyFolderName.Text,
                    out nFileCount,
                    out errorfilenames,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);
            }

            MessageBox.Show(this, "验证完成。\r\n\r\n共验证 "+nFileCount+" 个日志文件，其中有 "+nRet.ToString()+" 个日志文件发现格式错误。\r\n\r\n" 
                + StringUtil.MakePathList(errorfilenames, "\r\n"));
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // return:
        //      -1  运行出错
        //      >=0 发生错误的文件数。文件名在errorfilenames中
        int VerifyLogFiles(string strDirectory,
            out int nFileCount,
            out List<string> errorfilenames,
            out string strError)
        {
            strError = "";
            errorfilenames = new List<string>();
            nFileCount = 0;


            // 列出所有日志文件
            DirectoryInfo di = new DirectoryInfo(strDirectory);

            FileInfo[] fis = di.GetFiles("*.log");

            Array.Sort(fis, new FileInfoCompare());

            nFileCount = fis.Length;

            for (int i = 0; i < fis.Length; i++)
            {
                Application.DoEvents();

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断1";
                        return -1;
                    }
                }

                string strFileName = fis[i].FullName;

                // return:
                //      -1  出错
                //      >=0 丢弃的段落数
                int nRet = VerifyLogFile(strFileName,
                    out strError);
                if (nRet == -1)
                {
                    strError = "验证日志文件 '" + strFileName + "' 时发生运行错误: " + strError;
                    return -1;
                }
                if (nRet > 0)
                    errorfilenames.Add(strFileName);
            }

            return errorfilenames.Count;
        }

        class FileInfoCompare : IComparer
        {

            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer.Compare(Object x, Object y)
            {
                return ((new CaseInsensitiveComparer()).Compare(((FileInfo)x).Name, ((FileInfo)y).Name));
            }
        }

        // return:
        //      -1  出错
        //      >=0 丢弃的段落数
        int VerifyLogFile(string strSourceFilename,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            int nTryCount = 0;

            if (String.IsNullOrEmpty(strSourceFilename) == true)
            {
                strError = "源文件名不能为空";
                return -1;
            }


            Stream source = null;

            try
            {
                source = File.Open(
                    strSourceFilename,
                    FileMode.Open,
                    FileAccess.ReadWrite, // Read会造成无法打开 2007/5/22
                    FileShare.ReadWrite);
            }
            catch (FileNotFoundException /*ex*/)
            {
                strError = "源日志文件 " + strSourceFilename + "没有找到";
                return -1;   // file not found
            }
            catch (Exception ex)
            {
                strError = "打开源日志文件 '" + strSourceFilename + "' 时发生错误: " + ex.Message;
                return -1;
            }

            stop.SetMessage("正在验证日志文件 " + strSourceFilename + " ...");
            stop.SetProgressRange(0, source.Length);

            bool bTry = false;
            // TODO: 要汇报丢弃的段数
            for (; ; )
            {
                Application.DoEvents();

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断1";
                        return -1;
                    }
                }

                long lStart = source.Position;

                nRet = VerifyLogRecord(
                    source,
                    out strError);
                if (nRet == -1)
                {
                    if (source.Length <= lStart + 1)
                        break;
                    source.Seek(lStart + 1, SeekOrigin.Begin);
                    bTry = true;
                    continue;
                }

                if (bTry == true)
                {
                    nTryCount++;
                    bTry = false;
                }

                stop.SetProgressValue(source.Position);

                if (source.Position >= source.Length)
                    break;
            }


            source.Close();

            return nTryCount;
        }

        private void textBox_filenames_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.textBox_filenames.Text) == true)
                this.button_loadLogRecords.Enabled = false;
            else
                this.button_loadLogRecords.Enabled = true;
        }

        private void button_getTodayFilename_Click(object sender, EventArgs e)
        {
            textBox_logFileName.Text = DateTimeUtil.DateTimeToString8(DateTime.Now) + ".log";
        }

        delegate void Delegate_QuickSetFilenames(Control control);

        void QuickSetFilenames(Control control)
        {
            string strStartDate = "";
            string strEndDate = "";

            string strName = control.Text.Replace(" ","").Trim();

            if (strName == "本周")
            {
                DateTime now = DateTime.Now;
                int nDelta = (int)now.DayOfWeek; // 0-6 sunday - saturday
                DateTime start = now - new TimeSpan(nDelta, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                // strEndDate = DateTimeUtil.DateTimeToString8(start + new TimeSpan(7, 0,0,0));
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "本月")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 6) + "01";
            }
            else if (strName == "本年")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 4) + "0101";
            }
            else if (strName == "最近七天" || strName == "最近7天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(7-1, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十天" || strName == "最近30天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(30-1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十一天" || strName == "最近31天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(31-1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三百六十五天" || strName == "最近365天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(365-1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近十年" || strName == "最近10年")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(10 * 365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else
            {
                MessageBox.Show(this, "无法识别的周期 '" + strName + "'");
                return;
            }

            List<string> LogFileNames = null;
            string strWarning = "";
            string strError = "";
            // 根据日期范围，发生日志文件名
            // parameters:
            //      strStartDate    起始日期。8字符
            //      strEndDate  结束日期。8字符
            // return:
            //      -1  错误
            //      0   成功
            int nRet = OperLogStatisForm.MakeLogFileNames(strStartDate,
                strEndDate,
                true,  // 是否包含扩展名 ".log"
        out LogFileNames,
        out strWarning,
        out strError);
            if (nRet == -1)
                goto ERROR1;

            if (string.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, strWarning);

            string strText = "";
            for (int i = 0; i < LogFileNames.Count; i++)
            {
                if (i != 0)
                    strText += "\r\n";
                strText += LogFileNames[i];
            }
            this.textBox_filenames.Text = strText;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        private void comboBox_quickSetFilenames_SelectedIndexChanged(object sender, EventArgs e)
        {
            Delegate_QuickSetFilenames d = new Delegate_QuickSetFilenames(QuickSetFilenames);
            this.BeginInvoke(d, new object[] { sender });
        }

        void DoViewOperlog(bool bOpenWindow)
        {
            string strError = "";
            string strHtml = "";
            string strXml = "";

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (this.MainForm.PanelFixedVisible == false
                    && (m_operlogViewer == null || m_operlogViewer.Visible == false))
                    return;
            }

            if (this.listView_records.SelectedItems.Count != 1)
            {
                // 2012/10/2
                if (this.m_operlogViewer != null)
                    this.m_operlogViewer.Clear();

                return;
            }

            ListViewItem item = this.listView_records.SelectedItems[0];
            string strFilename = ListViewUtil.GetItemText(item, COLUMN_FILENAME);
            string strIndex = ListViewUtil.GetItemText(item, COLUMN_INDEX);

            // 从服务器获得
            // return:
            //      -1  出错
            //      0   正常
            //      1   用户中断
            int nRet = GetXml(item,
    out strXml,
    out strError);
            if (nRet == 1)
                return;
            if (nRet == -1)
            {
                goto ERROR1;
            }
            else
            {

                // 创建解释日志记录内容的 HTML 字符串
                // return:
                //      -1  出错
                //      0   成功
                //      1   未知的操作类型
                nRet = GetHtmlString(strXml,
                    true,
                    out strHtml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    strHtml = strError;
            }

            bool bNew = false;
            if (this.m_operlogViewer == null
                || (bOpenWindow == true && this.m_operlogViewer.Visible == false))
            {
                m_operlogViewer = new CommentViewerForm();
                MainForm.SetControlFont(m_operlogViewer, this.Font, false);
                m_operlogViewer.SuppressScriptErrors = this.MainForm.SuppressScriptErrors;
                bNew = true;
            }

            m_operlogViewer.MainForm = this.MainForm;  // 必须是第一句

            if (bNew == true)
                m_operlogViewer.InitialWebBrowser();

            m_operlogViewer.Text = "日志记录 '" + strFilename + " : " + strIndex + "'";
            m_operlogViewer.HtmlString = (string.IsNullOrEmpty(strHtml) == true ? NOTSUPPORT : strHtml);
            m_operlogViewer.XmlString = strXml;
            m_operlogViewer.FormClosed -= new FormClosedEventHandler(m_viewer_FormClosed);
            m_operlogViewer.FormClosed += new FormClosedEventHandler(m_viewer_FormClosed);

            if (bOpenWindow == true)
            {
                if (m_operlogViewer.Visible == false)
                {
                    this.MainForm.AppInfo.LinkFormState(m_operlogViewer, "operlog_viewer_state");
                    m_operlogViewer.Show(this);
                    m_operlogViewer.Activate();

                    this.MainForm.CurrentPropertyControl = null;
                }
                else
                {
                    if (m_operlogViewer.WindowState == FormWindowState.Minimized)
                        m_operlogViewer.WindowState = FormWindowState.Normal;
                    m_operlogViewer.Activate();
                }
            }
            else
            {
                if (m_operlogViewer.Visible == true)
                {

                }
                else
                {
                    if (this.MainForm.CurrentPropertyControl != m_operlogViewer.MainControl)
                        m_operlogViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, "DoViewOperlog() 出错: " + strError);
        }

        void m_viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_operlogViewer != null)
            {
                this.MainForm.AppInfo.UnlinkFormState(m_operlogViewer);
                this.m_operlogViewer = null;
            }
        }

        private void listView_records_DoubleClick(object sender, EventArgs e)
        {
            DoViewOperlog(true);
        }

        private void toolStripButton_closeDownPanel_Click(object sender, EventArgs e)
        {
            this.DownPannelVisible = false;
        }

        private void listView_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("查找(&F)");
            menuItem.Click += new System.EventHandler(this.menu_find_Click);
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("筛选(&I) ["+this.listView_records.SelectedItems.Count.ToString()+"]");
            menuItem.Click += new System.EventHandler(this.menu_filter_Click);
            if (this.listView_records.SelectedItems.Count == 0)
                menuItem.Enabled = false; 
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAllLines_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("打印解释内容(&P) ["+this.listView_records.SelectedItems.Count.ToString()+"]");
            menuItem.Click += new System.EventHandler(this.menu_printHtml_Click);
            if (this.listView_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_records, new Point(e.X, e.Y));
        }

        void menu_selectAllLines_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            bool bOldUpdate = this.listView_records.SuppressUpdate;
            this.listView_records.SuppressUpdate = true;    // 禁止中间刷新
            try
            {
                // TODO: 暂时禁止Update功能
                ListViewUtil.SelectAllLines(this.listView_records);
            }
            finally
            {
                this.listView_records.SuppressUpdate = bOldUpdate;
                this.listView_records.ForceUpdate();
                this.Cursor = oldCursor;
            }
        }

        void DoStopPrint(object sender, StopEventArgs e)
        {
        }

        // 打印解释内容
        void menu_printHtml_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选定要打印的行";
                goto ERROR1;
            }

            List<string> filenames = new List<string>();
            string strFileNamePrefix = this.MainForm.DataDir + "\\~operlog_print_";
            string strFilename = strFileNamePrefix + (1).ToString() + ".html";
            filenames.Add(strFilename);

            File.Delete(strFilename);

            StreamUtil.WriteText(strFilename,
    "<html>" +
    GetHeadString(false) +
    "<body>");

            Stop stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联

            stop.OnStop += new StopEventHandler(this.DoStopPrint);
            stop.Initial("正在创建打印页面 ...");
            stop.BeginLoop();

            m_webExternalHost = new WebExternalHost();
            m_webExternalHost.Initial(this.MainForm, null);
            m_webExternalHost.IsInLoop = true;

            this.GetSummary += new GetSummaryEventHandler(OperLogForm_GetSummary);
            try
            {
                stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);
                int i = 0;
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    Application.DoEvents();

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    OperLogItemInfo info = (OperLogItemInfo)item.Tag;

                    string strLogFileName = ListViewUtil.GetItemText(item, COLUMN_FILENAME);
                    string strIndex = ListViewUtil.GetItemText(item, COLUMN_INDEX);

                    string strXml = "";
                    // 从服务器获得
                    // return:
                    //      -1  出错
                    //      0   正常
                    //      1   用户中断
                    int nRet = GetXml(item,
            out strXml,
            out strError);
                    if (nRet == 1)
                        return;
                    if (nRet == -1)

                        goto ERROR1;


                    Global.SetXmlString(this.webBrowser_xml,
    strXml,
    this.MainForm.DataDir,
    "operlogexml");

                    string strHtml = "";
                    // 创建解释日志记录内容的 HTML 字符串
                    // return:
                    //      -1  出错
                    //      0   成功
                    //      1   未知的操作类型
                    nRet = GetHtmlString(strXml,
                        false,
                        out strHtml,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                        strHtml = strError;

                    StreamUtil.WriteText(strFilename,
        "<p class='record_title'>" + strLogFileName + " : " + strIndex + "</p>" + strHtml);

                    stop.SetProgressValue( i + 1);
                    i++;
                }
            }
            finally
            {
                this.GetSummary -= new GetSummaryEventHandler(OperLogForm_GetSummary);
                if (m_webExternalHost != null)
                {
                    m_webExternalHost.IsInLoop = false;
                    m_webExternalHost.Destroy();
                    m_webExternalHost = null;
                }

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStopPrint);
                stop.Initial("打印页面创建完成");
                stop.HideProgress();

                if (stop != null) // 脱离关联
                {
                    stop.Unregister();	// 和容器关联
                    stop = null;
                }
            }

            StreamUtil.WriteText(strFilename,
"</body></html>");

            // TODO: 浏览器控件连接javascript host
            HtmlPrintForm printform = new HtmlPrintForm();

            printform.Text = "打印解释内容";
            printform.MainForm = this.MainForm;
            printform.Filenames = filenames;

            this.MainForm.AppInfo.LinkFormState(printform, "operlogform_printform_state");
            printform.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(printform);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void OperLogForm_GetSummary(object sender, GetSummaryEventArgs e)
        {
            if (StringUtil.HasHead(e.Command, "B:") == true)
            {
                e.Summary = this.m_webExternalHost.GetSummary(e.Command.Substring(2), false);
            }
            else if (StringUtil.HasHead(e.Command, "P:") == true)
            {
                e.Summary = this.m_webExternalHost.GetPatronSummary(e.Command.Substring(2));
            }
            else
                e.Summary = "不支持的命令 '"+e.Command+"'";
        }

        // 筛选
        void menu_filter_Click(object sender, EventArgs e)
        {
            OperLogFindDialog dlg = new OperLogFindDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Text = "筛选";
            dlg.Operations = m_strFindOperations;

            this.MainForm.AppInfo.LinkFormState(dlg, "operlogform_finddialog_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;


            m_strFindOperations = dlg.Operations;

            List<string> operations = StringUtil.FromListString(dlg.Operations);
            int nRemoveCount = 0;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            try
            {
#if NO
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    string strOperation = ListViewUtil.GetItemText(item, COLUMN_OPERATION);

                    foreach (string o in operations)
                    {
                        if (strOperation.IndexOf(o) == -1)
                        {
                            this.listView_records.Items.Remove(item);
                            nRemoveCount++;
                        }
                    }
                }
#endif
                List<ListViewItem> reserves = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.Items)
                {
                    if (item.Selected == false)
                    {
                        reserves.Add(item);
                        continue;
                    }

                    string strOperation = ListViewUtil.GetItemText(item, COLUMN_OPERATION);

                    foreach (string o in operations)
                    {
                        if (strOperation.IndexOf(o) != -1)
                            reserves.Add(item);
                    }
                }

                nRemoveCount = this.listView_records.Items.Count - reserves.Count;
                this.listView_records.Items.Clear();
                this.listView_records.BeginUpdate();
                foreach (ListViewItem item in reserves)
                {
                    this.listView_records.Items.Add(item);
                }
                this.listView_records.EndUpdate();
            }
            finally
            {
                this.Cursor = oldCursor;
            }
            MessageBox.Show(this, "共移走 '" + nRemoveCount.ToString() + "' 项");
            return;
        }

        string m_strFindOperations = "";    // 曾经用过的查找参数

        void menu_find_Click(object sender, EventArgs e)
        {
#if NO
            OperLogFindDialog dlg = new OperLogFindDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Operations = m_strFindOperations;

            this.MainForm.AppInfo.LinkFormState(dlg, "operlogform_finddialog_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;


            m_strFindOperations = dlg.Operations;

            int start_index = 0;
            if (this.listView_records.SelectedIndices.Count > 0)
                start_index = this.listView_records.SelectedIndices[0];

            List<string> operations = StringUtil.FromListString(dlg.Operations);
            ListViewItem found_item = null;
            int i = 0;
            foreach (ListViewItem item in this.listView_records.Items)
            {
                if (i <= start_index)
                {
                    i++;
                    continue;
                }

                string strOperation = ListViewUtil.GetItemText(item, COLUMN_OPERATION);

                foreach (string o in operations)
                {
                    if (strOperation.IndexOf(o) != -1)
                    {
                        found_item = item;
                        goto FOUND;
                    }
                }

                i++;
            }

            MessageBox.Show(this, "操作类型为 '"+dlg.Operations+"' 的日志记录没有找到");
            return;
        FOUND:
            ListViewUtil.ClearSelection(this.listView_records);
            found_item.Selected = true;
            found_item.EnsureVisible();
#endif
            Find("");
        }

        //
        // 摘要:
        //     处理对话框键。
        //
        // 参数:
        //   keyData:
        //     System.Windows.Forms.Keys 值之一，它表示要处理的键。
        //
        // 返回结果:
        //     如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理。
        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }

            if (keyData == Keys.F3)
            {
                // 继续查找
                Find("continue");
                return true;
            }

            // return false;
            return base.ProcessDialogKey(keyData);
        }

        // parameters:
        //      strStyle    下列之一 continue
        void Find(string strStyle)
        {
            if (StringUtil.IsInList("continue", strStyle) == true
                && string.IsNullOrEmpty(this.m_strFindOperations) == false)
            {
            }
            else
            {
                OperLogFindDialog dlg = new OperLogFindDialog();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.Operations = m_strFindOperations;

                this.MainForm.AppInfo.LinkFormState(dlg, "operlogform_finddialog_formstate");
                dlg.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(dlg);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                m_strFindOperations = dlg.Operations;
            }

            int start_index = 0;
            if (this.listView_records.SelectedIndices.Count > 0)
                start_index = this.listView_records.SelectedIndices[0];

            ListViewItem found_item = null;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            try
            {

                List<string> operations = StringUtil.FromListString(m_strFindOperations);
                int i = 0;
                foreach (ListViewItem item in this.listView_records.Items)
                {
                    if (i <= start_index)
                    {
                        i++;
                        continue;
                    }

                    string strOperation = ListViewUtil.GetItemText(item, COLUMN_OPERATION);

                    foreach (string o in operations)
                    {
                        if (strOperation.IndexOf(o) != -1)
                        {
                            found_item = item;
                            goto FOUND;
                        }
                    }

                    i++;
                }

            }
            finally
            {
                this.Cursor = oldCursor;
            }

            this.MainForm.StatusBarMessage = "操作类型为 '" + m_strFindOperations + "' 的日志记录没有找到";
            return;
        FOUND:
            ListViewUtil.ClearSelection(this.listView_records);
            found_item.Selected = true;
            found_item.EnsureVisible();
            this.MainForm.StatusBarMessage = "找到";
        }

        // 
        /// <summary>
        /// 是否显示读者借阅历史
        /// </summary>
        public bool DisplayReaderBorrowHistory
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "operlog_form",
                    "display_reader_borrow_history",
                    true);
            }
        }

        // 
        /// <summary>
        /// 是否显示册借阅历史
        /// </summary>
        public bool DisplayItemBorrowHistory
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                "operlog_form",
                "display_item_borrow_history",
                true);
            }
        }


        #region 改进后的批处理功能

        // parameters:
        //      bInCacheFile    lHint指示的是否为本地cache文件中的hint
        // return:
        //      -1  出错
        //      0   正常
        //      1   需要立即中断循环
        /// <summary>
        /// 用于处理一条日志记录的回调函数
        /// </summary>
        /// <param name="strLogFileName">日志文件名</param>
        /// <param name="strXml">日志记录 XML</param>
        /// <param name="bInCacheFile">是否在本地缓存文件中</param>
        /// <param name="lHint">日志记录位置暗示</param>
        /// <param name="lIndex">日志记录编号</param>
        /// <param name="lAttachmentTotalLength">附件尺寸</param>
        /// <param name="param">回调对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        ///      -1  出错
        ///      0   正常
        ///      1   需要立即中断循环
        /// </returns>
        public delegate int Delegate_doRecord(string strLogFileName,
            string strXml,
            bool bInCacheFile,
            long lHint,
            long lIndex,
            long lAttachmentTotalLength,
            object param,
            out string strError);

        // return:
        //      -1  出错
        //      0   正常结束
        //      1   中断
        /// <summary>
        /// 处理日志文件
        /// </summary>
        /// <param name="owner">宿主窗口</param>
        /// <param name="stop">停止对象</param>
        /// <param name="estimate">剩余时间估算器</param>
        /// <param name="channel">通讯通道</param>
        /// <param name="filenames">要参与处理的日志文件名集合</param>
        /// <param name="nLevel">从 dp2Library 服务器获取日志记录的详细级别</param>
        /// <param name="strStyle">处理风格。autocache</param>
        /// <param name="strCacheDir">日志本地缓存目录</param>
        /// <param name="param">回调对象</param>
        /// <param name="procDoRecord">回调函数</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        ///      -1  出错
        ///      0   正常结束
        ///      1   中断
        /// </returns>
        public static int ProcessFiles(
            IWin32Window owner,
            Stop stop,
            ProgressEstimate estimate,
            LibraryChannel channel,
            List<string> filenames,
            int nLevel,
            string strStyle,
            string strCacheDir,
            object param,
            Delegate_doRecord procDoRecord,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strCacheDir) == false)
                PathUtil.CreateDirIfNeed(strCacheDir);

            // ProgressEstimate estimate = new ProgressEstimate();
            bool bAutoCache = StringUtil.IsInList("autocache", strStyle);

            if (bAutoCache == true)
            {
                long lServerFileSize = 0;
                long lCacheFileSize = 0;
                // 象征性获得一个日志文件的尺寸，主要目的是为了触发一次通道登录
                // return:
                //      -1  error
                //      0   file not found
                //      1   found
                nRet = GetFileSize(
                    stop,
                    channel,
                    strCacheDir,
                    "20121001.log",
                    out lServerFileSize,
                    out lCacheFileSize,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 检查日志文件缓存目录的版本是否和当前用户的信息一致
                // return:
                //      -1  出错
                //      0   一致
                //      1   不一致
                nRet = DetectCacheVersionFile(
                    strCacheDir,
                    "version.xml",
                    channel.LibraryCodeList,
                    channel.Url,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                {
                    // 清空当前缓存目录
                    nRet = Global.DeleteDataDir(
                        owner,
                        strCacheDir,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    PathUtil.CreateDirIfNeed(strCacheDir);  // 重新创建目录

                    // 创建版本文件
                    nRet = CreateCacheVersionFile(
                        strCacheDir,
                        "version.xml",
                        channel.LibraryCodeList,
                        channel.Url,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

            }

            long lTotalSize = 0;
            List<string> lines = new List<string>();    // 经过处理后排除了不存在的文件名
            List<long> sizes = new List<long>();
            stop.SetMessage("正在准备获得日志文件尺寸 ...");
            foreach (string strLine in filenames)
            {
                Application.DoEvents();

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return 1;
                }

                if (String.IsNullOrEmpty(strLine) == true)
                    continue;

                string strFilename = strLine.Trim();
                // 去掉注释
                nRet = strFilename.IndexOf("#");
                if (nRet != -1)
                    strFilename = strFilename.Substring(0, nRet).Trim();

                if (String.IsNullOrEmpty(strFilename) == true)
                    continue;

                string strLogFilename = "";
                string strRange = "";

                nRet = strFilename.IndexOf(":");
                if (nRet != -1)
                {
                    strLogFilename = strFilename.Substring(0, nRet).Trim();
                    strRange = strFilename.Substring(nRet + 1).Trim();
                }
                else
                {
                    strLogFilename = strFilename.Trim();
                    strRange = "";
                }

                long lServerFileSize = 0;
                long lCacheFileSize = 0;
                // 获得一个日志文件的尺寸
                // return:
                //      -1  error
                //      0   file not found
                //      1   found
                nRet = GetFileSize(
                    stop,
                    channel,
                    strCacheDir,
                    strLogFilename,
                    out lServerFileSize,
                    out lCacheFileSize,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 0)
                    continue;

                if (lServerFileSize == 0)
                    continue;   // 0字节的文件当作不存在处理

                Debug.Assert(lServerFileSize >= 0, "");

                if (bAutoCache == true)
                {
                    if (lCacheFileSize > 0)
                        lTotalSize += lCacheFileSize;
                    else
                        lTotalSize += lServerFileSize;
                }
                else
                {
                    lTotalSize += lServerFileSize;
                }

                lines.Add(strFilename);

                // 记忆每个文件的尺寸，后面就不用获取了?
                sizes.Add(lServerFileSize);
            }

            if (stop != null)
                stop.SetProgressRange(0, lTotalSize);

            estimate.SetRange(0, lTotalSize);
            estimate.StartEstimate();

            long lDoneSize = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                Application.DoEvents();

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return 1;
                }

                string strLine = lines[i];
#if NO
                    if (String.IsNullOrEmpty(strLine) == true)
                        continue;
                    // 去掉注释
                    nRet = strLine.IndexOf("#");
                    if (nRet != -1)
                        strLine = strLine.Substring(0, nRet).Trim();

                    if (String.IsNullOrEmpty(strLine) == true)
                        continue;
#endif

                string strLogFilename = "";
                string strRange = "";

                nRet = strLine.IndexOf(":");
                if (nRet != -1)
                {
                    strLogFilename = strLine.Substring(0, nRet).Trim();
                    strRange = strLine.Substring(nRet + 1).Trim();
                }
                else
                {
                    strLogFilename = strLine.Trim();
                    strRange = "";
                }

                // return:
                //      -1  error
                //      0   正常结束
                //      1   用户中断
                nRet = ProcessFile(
                    owner,
                    stop,
                    estimate,
                    channel,
                    strLogFilename,
                    nLevel,
                    sizes[i],
                    strRange,
                    strStyle,
                    strCacheDir,
                    param,
                    procDoRecord,
                    ref lDoneSize,
                    ref lTotalSize,
                    out strError);
                if (nRet == -1)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                            return 0;
                    }
                    // MessageBox.Show(this, strError);
                    DialogResult result = MessageBox.Show(owner,
strError + "\r\n\r\n是否继续处理?",
"OperLogForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.No)
                        return 1;
                }
                if (nRet == 1)
                    return 1;
            }

            return 0;
        }

        // 获得一个日志文件的尺寸
        // return:
        //      -1  error
        //      0   file not found
        //      1   found
        static int GetFileSize(
            Stop stop,
            LibraryChannel channel,
            string strCacheDir,
            string strLogFileName,
            out long lServerFileSize,
            out long lCacheFileSize,
            out string strError)
        {
            strError = "";
            lServerFileSize = 0;
            lCacheFileSize = 0;

            string strCacheFilename = PathUtil.MergePath(strCacheDir, strLogFileName);

            FileInfo fi = new FileInfo(strCacheFilename);
            if (fi.Exists == true)
                lCacheFileSize = fi.Length;

            stop.SetMessage("正获得日志文件 " + strLogFileName + " 的尺寸...");

            string strXml = "";
            long lAttachmentTotalLength = 0;
            byte[] attachment_data = null;

            // 获得日志文件尺寸
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            //      2   超过范围
            long lRet = channel.GetOperLog(
                stop,
                strLogFileName,
                -1,    // lIndex,
                -1, // lHint,
                "level-0",
                        "", // strFilter
                out strXml,
                out lServerFileSize,
                0,  // lAttachmentFragmentStart,
                0,  // nAttachmentFramengLength,
                out attachment_data,
                out lAttachmentTotalLength,
                out strError);
            if (lRet == 0)
            {
                lServerFileSize = 0;
                Debug.Assert(lServerFileSize == 0, "");
                return 0;
            }
            if (lRet != 1)
                return -1;
            Debug.Assert(lServerFileSize >= 0, "");

            return 1;
        }

        // 检查日志文件缓存目录的版本是否和当前用户的信息一致
        // return:
        //      -1  出错
        //      0   一致
        //      1   不一致
        static int DetectCacheVersionFile(
    string strCacheDir,
    string strVersionFileName,
    string strLibraryCodeList,
    string strDp2LibraryServerUrl,
    out string strError)
        {
            strError = "";

            string strVersionFilePath = PathUtil.MergePath(strCacheDir, strVersionFileName);
            if (File.Exists(strVersionFilePath) == false)
                return 1;

                XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strVersionFilePath);
            }
            catch (Exception ex)
            {
                strError = "创建日志缓存版本文件 '" + strVersionFilePath + "' 时出错: " + ex.Message;
                return -1;
            }

            string strCurrentLibraryCode = DomUtil.GetAttr(dom.DocumentElement, "libraryCodeList");
            string strCurrentServerUrl = DomUtil.GetAttr(dom.DocumentElement, "libraryServerUrl");

            if (strLibraryCodeList != strCurrentLibraryCode
                || strCurrentServerUrl != strDp2LibraryServerUrl)
                return 1;

            return 0;
        }

        // 创建表示缓存版本的文件
        // 记载了当前用户管辖的馆代码，dp2Library服务器的地址
        static int CreateCacheVersionFile(
            string strCacheDir,
            string strVersionFileName,
            string strLibraryCodeList,
            string strDp2LibraryServerUrl,
            out string strError)
        {
            strError = "";

            string strVersionFilePath = PathUtil.MergePath(strCacheDir, strVersionFileName);
            try
            {
                File.Delete(strVersionFilePath);

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                DomUtil.SetAttr(dom.DocumentElement, "libraryCodeList", strLibraryCodeList);
                DomUtil.SetAttr(dom.DocumentElement, "libraryServerUrl", strDp2LibraryServerUrl);
                dom.Save(strVersionFilePath);
            }
            catch (Exception ex)
            {
                strError = "创建日志缓存版本文件 '" + strVersionFilePath + "' 时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // 创建日志文件的metadata文件，记载服务器端文件尺寸
        static int CreateCacheMetadataFile(
            string strCacheDir,
            string strLogFileName,
            long lServerFileSize,
            out string strError)
        {
            strError = "";

            string strCacheMetaDataFilename = PathUtil.MergePath(strCacheDir, strLogFileName + ".meta");
            try
            {
                File.Delete(strCacheMetaDataFilename);

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                DomUtil.SetAttr(dom.DocumentElement, "serverFileSize", lServerFileSize.ToString());
                dom.Save(strCacheMetaDataFilename);
            }
            catch (Exception ex)
            {
                strError = "创建metadata文件 '" + strCacheMetaDataFilename + "' 时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // 删除一个日志文件的本地缓存文件
        static int DeleteCacheFile(
    string strCacheDir,
    string strLogFileName,
    out string strError)
        {
            strError = "";

            string strCacheFilename = PathUtil.MergePath(strCacheDir, strLogFileName);
            string strCacheMetaDataFilename = PathUtil.MergePath(strCacheDir, strLogFileName + ".meta");
            try
            {
                File.Delete(strCacheMetaDataFilename);
                File.Delete(strCacheFilename);
            }
            catch (Exception ex)
            {
                strError = "删除日志缓存文件 '" + strCacheFilename + "' 时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        static int PrepareCacheFile(
            string strCacheDir,
            string strLogFileName,
            long lServerFileSize,
            out bool bCacheFileExist,
            out Stream stream,
            out string strError)
        {
            strError = "";
            stream = null;
            // 观察本地缓存文件是否存在
             bCacheFileExist = false;
            XmlDocument metadata_dom = new XmlDocument();

            string strCacheFilename = PathUtil.MergePath(strCacheDir, strLogFileName);
            string strCacheMetaDataFilename = PathUtil.MergePath(strCacheDir, strLogFileName + ".meta");

            if (File.Exists(strCacheFilename) == true
                && File.Exists(strCacheMetaDataFilename) == true)
            {
                bCacheFileExist = true;

                // 观察metadata
                try
                {
                    metadata_dom.Load(strCacheMetaDataFilename);
                }
                catch (FileNotFoundException)
                {
                    bCacheFileExist = false;    // 虽然数据文件存在，也需要重新获取
                }
                catch (Exception ex)
                {
                    strError = "装载metadata文件 '" + strCacheMetaDataFilename + "' 时出错: " + ex.Message;
                    return -1;
                }

                // 对比文件尺寸
                string strFileSize = DomUtil.GetAttr(metadata_dom.DocumentElement, "serverFileSize");
                if (string.IsNullOrEmpty(strFileSize) == true)
                {
                    strError = "metadata中缺乏fileSize属性";
                    return -1;
                }
                long lTempFileSize = 0;
                if (Int64.TryParse(strFileSize, out lTempFileSize) == false)
                {
                    strError = "metadata中缺乏fileSize属性值 '" + strFileSize + "' 格式错误";
                    return -1;
                }

                if (lTempFileSize != lServerFileSize)
                    bCacheFileExist = false;

            }
            // 如果文件存在，就打开，如果文件不存在，就创建一个新的
            stream = File.Open(
strCacheFilename,
FileMode.OpenOrCreate,
FileAccess.ReadWrite,
FileShare.ReadWrite);

            if (bCacheFileExist == false)
                stream.SetLength(0);

            return 0;
        }

        // 根据记录编号，定位到记录起始位置
        // parameters:
        // return:
        //      -1  error
        //      0   成功
        //      1   到达文件末尾或者超出
        static int LocationRecord(Stream stream,
            long lIndex,
            out string strError)
        {
            strError = "";

            if (stream.Position != 0)
                stream.Seek(0, SeekOrigin.Begin);

            for (long i = 0; i < lIndex; i++)
            {
                byte[] length = new byte[8];

                int nRet = stream.Read(length, 0, 8);
                if (nRet < 8)
                {
                    strError = "起始位置不正确";
                    return -1;
                }

                Int64 lLength = BitConverter.ToInt64(length, 0);

                stream.Seek(lLength, SeekOrigin.Current);
            }

            if (stream.Position >= stream.Length)
                return 1;
            return 0;
        }

        // 将日志写入文件
        // 不处理异常
        static void WriteCachedEnventLog(
            Stream stream,
            string strXmlBody,
            long lAttachmentLength)
        {
            long lStart = stream.Position;	// 记忆起始位置

            byte[] length = new byte[8];

            // 清空
            for (int i = 0; i < length.Length; i++)
            {
                length[i] = 0;
            }

            stream.Write(length, 0, 8);	// 临时写点数据,占据记录总长度位置

            if (string.IsNullOrEmpty(strXmlBody) == false)
            {
                // 写入xml事项
                WriteCachedEntry(
                    stream,
                    strXmlBody,
                    lAttachmentLength);
            }

            long lRecordLength = stream.Position - lStart - 8;

            // 写入记录总长度
            if (stream.Position != lStart)
                stream.Seek(lStart, SeekOrigin.Begin);

            length = BitConverter.GetBytes((long)lRecordLength);

            stream.Write(length, 0, 8);

            // 迫使写入物理文件
            stream.Flush();

            // 文件指针回到末尾位置
            stream.Seek(lRecordLength, SeekOrigin.Current);
        }

        // 写入一个事项(string类型)
        static void WriteCachedEntry(
            Stream stream,
            string strBody,
            long lAttachmentLength)
        {
            byte[] length = new byte[8];

            // 记忆起始位置
            long lEntryStart = stream.Position;

            // strBody长度
            byte[] xmlbody = Encoding.UTF8.GetBytes(strBody);

            length = BitConverter.GetBytes((long)xmlbody.Length);

            stream.Write(length, 0, 8);  // body长度

            if (xmlbody.Length > 0)
            {
                // xml body本身
                stream.Write(xmlbody, 0, xmlbody.Length);
            }

            byte[] lengthbody = null;


            lengthbody = BitConverter.GetBytes(lAttachmentLength);
            length = BitConverter.GetBytes((long)lengthbody.Length);


            stream.Write(length, 0, 8);	// metadata长度

            Debug.Assert(lengthbody.Length == 8, "");
            stream.Write(lengthbody, 0, lengthbody.Length);
        }

        // 读出一个事项(string类型)
        // | body length (8bytes)| ... bodydata | attachment length | ... attachment |
        static int ReadCachedEntry(
            Stream stream,
            out string strBody,
            out long lTotalAttachmentLength,
            out string strError)
        {
            strBody = "";
            strError = "";
            lTotalAttachmentLength = 0;

            long lStart = stream.Position;  // 保留起始位置

            byte[] length = new byte[8];

            // strBody长度
            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "strBody长度位置不足8bytes";
                return -1;
            }

            Int64 lBodyLength = BitConverter.ToInt64(length, 0);

            if (lBodyLength > 1000 * 1024)
            {
                strError = "记录格式不正确，body长度超过1000K";
                return -1;
            }

            if (lBodyLength > 0)
            {
                byte[] xmlbody = new byte[(int)lBodyLength];

                nRet = stream.Read(xmlbody, 0, (int)lBodyLength);
                if (nRet < (int)lBodyLength)
                {
                    strError = "body不足其长度定义";
                    return -1;
                }

                strBody = Encoding.UTF8.GetString(xmlbody);
            }

            // attachment长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "attachment长度位置不足8bytes";
                return -1;
            }

            Int64 lAttachmentLength = BitConverter.ToInt64(length, 0);

            if (lAttachmentLength != 8)
            {
                strError = "记录格式不正确，lAttachmentLength != 8";
                return -1;
            }

            // attahment data
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "attachment data位置不足8bytes";
                return -1;
            }

            lTotalAttachmentLength = BitConverter.ToInt64(length, 0);

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lBodyLength + 8 + lAttachmentLength + 8)
            {
                Debug.Assert(false, "");
                strError = "entry长度经检验不正确";
                return -1;
            }

            return 0;
        }

        // 
        /// <summary>
        /// 从本地缓存的日志文件当前位置读出一条日志记录
        /// </summary>
        /// <param name="stream">Stream 对象</param>
        /// <param name="strXmlBody">返回日志记录 XML</param>
        /// <param name="lTotalAttachmentLength">返回日志附件尺寸</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        static int ReadCachedEnventLog(
            Stream stream,
            out string strXmlBody,
            out long lTotalAttachmentLength,
            out string strError)
        {
            strError = "";
            strXmlBody = "";
            lTotalAttachmentLength = 0;

            long lStart = stream.Position;	// 记忆起始位置

            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "ReadEnventLog()从偏移量 " + lStart.ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。起始位置不正确";
                return -1;
            }

            Int64 lRecordLength = BitConverter.ToInt64(length, 0);

            if (lRecordLength == 0)
                return 0;   // 表示这是一个空记录

            Debug.Assert(lRecordLength != 0, "");


            // 读出xml事项
            nRet = ReadCachedEntry(stream,
                out strXmlBody,
                out lTotalAttachmentLength,
                out strError);
            if (nRet == -1)
                return -1;

            // 文件指针自然指向末尾位置
            // this.m_stream.Seek(lRecordLength, SeekOrigin.Current);

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lRecordLength + 8)
            {
                Debug.Assert(false, "");
                strError = "Record长度经检验不正确: stream.Position - lStart ["
                    + (stream.Position - lStart).ToString()
                    + "] 不等于 lRecordLength + 8 ["
                    + (lRecordLength + 8).ToString()
                    + "]";
                return -1;
            }

            return 0;
        }

        // 装入一个日志文件中的若干记录
        // parameters:
        //      strCacheDir 存储本地缓存文件的目录
        //      lServerFileSize 服务器端日志文件的尺寸。如果为-1，表示函数内会自动获取
        //      lSize   进度条所采用的最大尺寸。如果必要，可能会被本函数推动
        // return:
        //      -1  error
        //      0   正常结束
        //      1   用户中断
        static int ProcessFile(
            IWin32Window owner,
            Stop stop,
            ProgressEstimate estimate,
            LibraryChannel channel,
            string strLogFileName,
            int nLevel,
            long lServerFileSize,
            string strRange,
            string strStyle,
            string strCacheDir,
            object param,
            Delegate_doRecord procDoRecord,
            ref long lProgressValue,
            ref long lSize,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

            stop.SetMessage("正在装入日志文件 " + strLogFileName + " 中的记录。"
                + "剩余时间 " + ProgressEstimate.Format(estimate.Estimate(lProgressValue)) + " 已经过时间 " + ProgressEstimate.Format(estimate.delta_passed));

            string strXml = "";
            long lAttachmentTotalLength = 0;
            byte[] attachment_data = null;

            long lFileSize = 0;
            if (lServerFileSize == -1)
            {
                lServerFileSize = 0;

                // 获得服务器端日志文件尺寸
                lRet = channel.GetOperLog(
                    stop,
                    strLogFileName,
                    -1,    // lIndex,
                    -1, // lHint,
                        "level-" + nLevel.ToString(),
                        "", // strFilter
                    out strXml,
                    out lServerFileSize,
                    0,  // lAttachmentFragmentStart,
                    0,  // nAttachmentFramengLength,
                    out attachment_data,
                    out lAttachmentTotalLength,
                    out strError);
                // 2010/12/13
                if (lRet == 0)
                    return 0;
            }

            Stream stream = null;
            bool bCacheFileExist = false;
            bool bRemoveCacheFile = false;  // 是否要自动删除未全部完成的本地缓存文件

            bool bAutoCache = StringUtil.IsInList("autocache", strStyle);

            if (bAutoCache == true)
            {
                nRet = PrepareCacheFile(
                    strCacheDir,
                    strLogFileName,
                    lServerFileSize,
                    out bCacheFileExist,
                    out stream,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (bCacheFileExist == false && stream != null)
                    bRemoveCacheFile = true;
            }

            try
            {
                if (bCacheFileExist == true)
                    lFileSize = stream.Length;
                else
                    lFileSize = lServerFileSize;

                // stop.SetProgressRange(0, lTotalSize);

                if (String.IsNullOrEmpty(strRange) == true)
                    strRange = "0-9999999999";

                RangeList rl = new RangeList(strRange);

                for (int i = 0; i < rl.Count; i++)
                {
                    RangeItem ri = (RangeItem)rl[i];

                    OperLogInfo[] records = null;
                    long lStartRecords = 0;

                    long lHint = -1;
                    long lHintNext = -1;
                    for (long lIndex = ri.lStart; lIndex < ri.lStart + ri.lLength; lIndex++)
                    {
                        Application.DoEvents();

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断1";
                                goto ERROR1;
                            }
                        }

                        if (lIndex == ri.lStart)
                            lHint = -1;
                        else
                            lHint = lHintNext;

                        if (bCacheFileExist == true)
                        {
                            if (lHint == -1)
                            {
                                // return:
                                //      -1  error
                                //      0   成功
                                //      1   到达文件末尾或者超出
                                nRet = LocationRecord(stream,
                    lIndex,
                    out strError);
                                if (nRet == -1)
                                    return -1;
                            }
                            else
                            {
                                // 根据暗示找到
                                if (lHint == stream.Length)
                                    break;

                                if (lHint > stream.Length)
                                {
                                    strError = "lHint参数值不正确";
                                    return -1;
                                }
                                if (stream.Position != lHint)
                                    stream.Seek(lHint, SeekOrigin.Begin);
                            }

                            nRet = ReadCachedEnventLog(
                                stream,
                                out strXml,
                                out lAttachmentTotalLength,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            lHintNext = stream.Position;

                        }
                        else
                        {
                            if (records == null || lIndex - ri.lStart >= lStartRecords + records.Length)
                            {
                                int nCount = -1;
                                if (ri.lLength >= Int32.MaxValue)
                                    nCount = -1;
                                else
                                    nCount = (int)ri.lLength;

                                // 获得日志
                                // return:
                                //      -1  error
                                //      0   file not found
                                //      1   succeed
                                //      2   超过范围，本次调用无效
                                lRet = channel.GetOperLogs(
                                    stop,
                                    strLogFileName,
                                    lIndex,
                                    lHint,
                                    nCount,
                                    "level-" + nLevel.ToString(),
                                    "", // strFilter
                                    out records,
                                    out strError);
                                if (lRet == -1)
                                {
                                    DialogResult result = MessageBox.Show(owner,
    strError + "\r\n\r\n是否继续处理?",
    "OperLogForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                                    if (result == DialogResult.No)
                                        goto ERROR1;
                                    else
                                    {
                                        // TODO: 是否要在listview中装入一条表示出错的行?
                                        lHintNext = -1;
                                        continue;
                                    }
                                }
                                if (lRet == 0)
                                    return 0;

                                if (lRet == 2)
                                    break;

                                // records数组表示的起点位置
                                lStartRecords = lIndex - ri.lStart;
                            }
                            OperLogInfo info = records[lIndex - lStartRecords];

                            strXml = info.Xml;
                            lHintNext = info.HintNext;
                            lAttachmentTotalLength = info.AttachmentLength;

                            // 写入本地缓存的日志文件
                            if (stream != null)
                            {
                                try
                                {
                                    WriteCachedEnventLog(
                                        stream,
                                        strXml,
                                        lAttachmentTotalLength);
                                }
                                catch (Exception ex)
                                {
                                    strError = "写入本地缓存文件的时候出错: " + ex.Message;
                                    return -1;
                                }
                            }
                        }

#if NO
                            // 2011/12/30
                            // 日志记录可能动态地增加了，超过了原先为ProgressBar设置的范围
                            if (lFizeTotalSize < (int)lHintNext)
                            {
                                lFizeTotalSize = lHintNext;

                                stop.SetProgressRange(0, lFizeTotalSize);
                            }
#endif
                        if (lHintNext >= 0)
                        {
                            // 校正
                            if (lProgressValue + lHintNext > lSize)
                            {
                                lSize = lProgressValue + lHintNext;

                                stop.SetProgressRange(0, lSize);
                                estimate.SetRange(0, lSize);
                            }

                            stop.SetProgressValue(lProgressValue + lHintNext);
                        }

                        if (lIndex % 100 == 0)
                        {
                            stop.SetMessage("正在装入日志文件 " + strLogFileName + " 中的记录 " + lIndex.ToString() + " 。"
    + "剩余时间 " + ProgressEstimate.Format(estimate.Estimate(lProgressValue + lHintNext)) + " 已经过时间 " + ProgressEstimate.Format(estimate.delta_passed));
                        }

                        //
                        if (procDoRecord != null)
                        {
                            nRet = procDoRecord(strLogFileName,
        strXml,
        bCacheFileExist,
        lHint,
        lIndex,
        lAttachmentTotalLength,
        param,
        out strError);
                            if (nRet == -1)
                            {
                                DialogResult result = MessageBox.Show(owner,
                                    strLogFileName + " : " + lIndex.ToString() + "\r\n" +  strError + "\r\n\r\n是否继续处理?",
"OperLogForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                                if (result == DialogResult.No)
                                    return -1;
                            }
                            if (nRet == 1)
                                return 1;
                        }

                    }
                }

                // 创建本地缓存的日志文件的metadata文件
                if (bCacheFileExist == false && stream != null)
                {
                    nRet = CreateCacheMetadataFile(
                        strCacheDir,
                        strLogFileName,
                        lServerFileSize,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                bRemoveCacheFile = false;   // 不删除
            }
            finally
            {
                if (stream != null)
                    stream.Close();

                if (bRemoveCacheFile == true)
                {
                    string strError1 = "";
                    nRet = DeleteCacheFile(
                        strCacheDir,
                        strLogFileName,
                        out strError1);
                    if (nRet == -1)
                        MessageBox.Show(owner, strError1);
                }
            }

            lProgressValue += lFileSize;
            return 0;
        ERROR1:
            return -1;
        }
        #endregion
    }

    /// <summary>
    /// 日志事项信息。用于 ListViewItem 的 Tag 对象
    /// </summary>
    internal class OperLogItemInfo
    {
        public int IndexOfTempFilename = -1;
        public long Hint = -1;
        public bool InCacheFile = false;    // Hint是否指的是本地缓存文件中的值
    }

    /// <summary>
    /// 获得摘要事件
    /// </summary>
    /// <param name="sender">触发者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetSummaryEventHandler(object sender,
GetSummaryEventArgs e);

    /// <summary>
    /// 获得摘要事件的参数
    /// </summary>
    public class GetSummaryEventArgs : EventArgs
    {
        /// <summary>
        /// [in] 命令字符串。
        /// "P:R0000001" "B:0000001|中文图书实体/1"
        /// </summary>
        public string Command = ""; // [in] "P:R0000001" "B:0000001|中文图书实体/1"
        /// <summary>
        /// [out] 返回摘要字符串
        /// </summary>
        public string Summary = ""; // [out]
    }
}