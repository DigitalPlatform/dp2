using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Web;
using System.Reflection;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using ClosedXML.Excel;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Script;
using DigitalPlatform.Marc;

using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;

using DigitalPlatform.dp2.Statis;

namespace dp2Circulation
{
    /// <summary>
    /// 打印财产账窗
    /// </summary>
    public partial class AccountBookForm : BatchPrintFormBase
    {
        /// <summary>
        /// 曾经用过的记录路径文件全路径
        /// </summary>
        public string RecPathFilePath = "";
        /// <summary>
        /// 曾经用过的册条码号文件全路径
        /// </summary>
        public string BarcodeFilePath = "";

        // ******************************
        // WordXML输出特性
        bool WordXmlTruncate = false;
        bool WordXmlOutputStatisPart = true;

        string m_strWordMlNsUri = "http://schemas.microsoft.com/office/word/2003/wordml";
        string m_strWxUri = "http://schemas.microsoft.com/office/word/2003/auxHint";

        // 曾经用过的WordXml输出文件名
        string ExportWordXmlFilename = "";

        // *******************************
        // 文本输出特性
        bool TextTruncate = false;
        bool TextOutputStatisPart = true;

        // 装载数据时的方式
        string SourceStyle = "";    // "batchno" "barcodefile" "recpathfile"

        // 曾经用过的输出文本文件名
        string ExportTextFilename = "";

        // refid -- 订购记录path 对照表
        Hashtable refid_table = new Hashtable();
        // 订购记录path -- 订购记录XML对照表
        Hashtable orderxml_table = new Hashtable();

        string BatchNo = "";    // 面板输入的批次号
        string LocationString = ""; // 面板输入的馆藏地点

        /// <summary>
        /// 浏览事项 ImageIndex 类型 : 出错
        /// </summary>
        public const int TYPE_ERROR = 0;
        /// <summary>
        /// 浏览事项 ImageIndex 类型 : 普通
        /// </summary>
        public const int TYPE_NORMAL = 1;
        /// <summary>
        /// 浏览事项 ImageIndex 类型 : 已勾选
        /// </summary>
        public const int TYPE_CHECKED = 2;

        // 参与排序的列号数组
        SortColumns SortColumns_in = new SortColumns();

        #region 列号
        /// <summary>
        /// 浏览列序号: 册条码号
        /// </summary>
        public static int COLUMN_BARCODE = 0;    // 册条码号
        /// <summary>
        /// 浏览列序号: 摘要
        /// </summary>
        public static int COLUMN_SUMMARY = 1;    // 摘要
        /// <summary>
        /// 浏览列序号: 错误信息
        /// </summary>
        public static int COLUMN_ERRORINFO = 1;  // 错误信息
        /// <summary>
        /// 浏览列序号: ISBN/ISSN
        /// </summary>
        public static int COLUMN_ISBNISSN = 2;           // ISBN/ISSN
        /// <summary>
        /// 浏览列序号: 册记录状态
        /// </summary>
        public static int COLUMN_STATE = 3;      // 状态
        /// <summary>
        /// 浏览列序号: 索取号
        /// </summary>
        public static int COLUMN_ACCESSNO = 4; // 索取号
        /// <summary>
        /// 浏览列序号: 出版时间
        /// </summary>
        public static int MERGED_COLUMN_PUBLISHTIME = 5;          // 出版时间
        /// <summary>
        /// 浏览列序号: 卷期
        /// </summary>
        public static int MERGED_COLUMN_VOLUME = 6;          // 卷期
        /// <summary>
        /// 浏览列序号: 馆藏地点
        /// </summary>
        public static int COLUMN_LOCATION = 7;   // 馆藏地点
        /// <summary>
        /// 浏览列序号: 册价格
        /// </summary>
        public static int COLUMN_PRICE = 8;      // 价格
        /// <summary>
        /// 浏览列序号: 册类型
        /// </summary>
        public static int COLUMN_BOOKTYPE = 9;   // 册类型
        /// <summary>
        /// 浏览列序号: 登录号
        /// </summary>
        public static int COLUMN_REGISTERNO = 10; // 登录号
        /// <summary>
        /// 浏览列序号: 注释
        /// </summary>
        public static int COLUMN_COMMENT = 11;    // 注释
        /// <summary>
        /// 浏览列序号: 合并注释
        /// </summary>
        public static int COLUMN_MERGECOMMENT = 12;   // 合并注释
        /// <summary>
        /// 浏览列序号: 批次号
        /// </summary>
        public static int COLUMN_BATCHNO = 13;    // 批次号
        /// <summary>
        /// 浏览列序号: 册记录路径
        /// </summary>
        public static int COLUMN_RECPATH = 14;   // 册记录路径
        /// <summary>
        /// 浏览列序号: 种记录路径
        /// </summary>
        public static int COLUMN_BIBLIORECPATH = 15; // 种记录路径
        /// <summary>
        /// 浏览列序号: 册参考ID
        /// </summary>
        public static int COLUMN_REFID = 16; // 参考ID

        /// <summary>
        /// 浏览列序号: 类别 (从订购记录中来)
        /// </summary>
        public static int EXTEND_COLUMN_CLASS = 17;             // 类别
        /// <summary>
        /// 浏览列序号: 书目号 (从订购记录中来)
        /// </summary>
        public static int EXTEND_COLUMN_CATALOGNO = 18;          // 书目号
        /// <summary>
        /// 浏览列序号: 订购时间 (从订购记录中来)
        /// </summary>
        public static int EXTEND_COLUMN_ORDERTIME = 19;        // 订购时间
        /// <summary>
        /// 浏览列序号: 订单号 (从订购记录中来)
        /// </summary>
        public static int EXTEND_COLUMN_ORDERID = 20;          // 订单号
        /// <summary>
        /// 浏览列序号: 渠道 (从订购记录中来)
        /// </summary>
        public static int EXTEND_COLUMN_SELLER = 21;             // 渠道
        /// <summary>
        /// 浏览列序号: 经费来源 (从订购记录中来)
        /// </summary>
        public static int EXTEND_COLUMN_SOURCE = 22;             // 经费来源

        /// <summary>
        /// 浏览列序号: 订购价
        /// </summary>
        public static int EXTEND_COLUMN_ORDERPRICE = 23;    // (订购记录中的)订购价

        /// <summary>
        /// 浏览列序号: 到书价
        /// </summary>
        public static int EXTEND_COLUMN_ACCEPTPRICE = 24;    // (订购记录中的)到书价


        #endregion

        // const int WM_LOADSIZE = API.WM_USER + 201;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AccountBookForm()
        {
            InitializeComponent();
        }

        private void AccountBookForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

            this.MainForm.AppInfo.LoadMdiSize += new EventHandler(AppInfo_LoadMdiSize);
            this.MainForm.AppInfo.SaveMdiSize += new EventHandler(AppInfo_SaveMdiSize);

            CreateColumnHeader(this.listView_in);

#if NO
            LoadSize();

            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            // 2009/2/2 
            this.comboBox_load_type.Text = this.MainForm.AppInfo.GetString(
                "accountbookform",
                "publication_type",
                "图书");

            // 2012/11/26
            this.checkBox_load_fillOrderInfo.Checked = this.MainForm.AppInfo.GetBoolean(
    "accountbookform",
    "fillOrderInfo",
    true);

            this.checkBox_load_fillBiblioSummary.Checked = this.MainForm.AppInfo.GetBoolean(
    "accountbookform",
    "fillBiblioSummary",
    true);

            this.BarcodeFilePath = this.MainForm.AppInfo.GetString(
                "accountbookform",
                "barcode_filepath",
                "");

            this.BatchNo = this.MainForm.AppInfo.GetString(
                "accountbookform",
                "batchno",
                "");

            this.LocationString = this.MainForm.AppInfo.GetString(
                "accountbookform",
                "location_string",
                "");

            this.comboBox_sort_sortStyle.Text = this.MainForm.AppInfo.GetString(
                "accountbookform",
                "sort_style",
                "<无>");

            // API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);
        }

        void AppInfo_SaveMdiSize(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_in);
            this.MainForm.AppInfo.SetString(
                "accountbookform",
                "list_in_width",
                strWidths);
        }

        void AppInfo_LoadMdiSize(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            string strWidths = this.MainForm.AppInfo.GetString(
               "accountbookform",
               "list_in_width",
               "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_in,
                    strWidths,
                    true);
            }
        }

        private void AccountBookForm_FormClosing(object sender,
            FormClosingEventArgs e)
        {
#if NO
            if (stop != null)
            {

                // Debug.Assert(false, "");
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }

            }
#endif
        }

        private void AccountBookForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                // 2009/2/2 
                this.MainForm.AppInfo.SetString(
                    "accountbookform",
                    "publication_type",
                    this.comboBox_load_type.Text);

                // 2012/11/26
                this.MainForm.AppInfo.SetBoolean(
        "accountbookform",
        "fillOrderInfo",
        this.checkBox_load_fillOrderInfo.Checked);

                this.MainForm.AppInfo.SetBoolean(
        "accountbookform",
        "fillBiblioSummary",
        this.checkBox_load_fillBiblioSummary.Checked);

                this.MainForm.AppInfo.SetString(
                    "accountbookform",
                    "barcode_filepath",
                    this.BarcodeFilePath);

                this.MainForm.AppInfo.SetString(
                    "accountbookform",
                    "batchno",
                    this.BatchNo);

                this.MainForm.AppInfo.SetString(
                    "accountbookform",
                    "location_string",
                    this.LocationString);

                this.MainForm.AppInfo.SetString(
                    "accountbookform",
                    "sort_style",
                    this.comboBox_sort_sortStyle.Text);

                CloseErrorInfoForm();

                this.MainForm.AppInfo.LoadMdiSize -= new EventHandler(AppInfo_LoadMdiSize);
                this.MainForm.AppInfo.SaveMdiSize -= new EventHandler(AppInfo_SaveMdiSize);
            }
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            /*
    switch (m.Msg)
    {
        case WM_LOADSIZE:
            LoadSize();
            return;
    }
             * */
            base.DefWndProc(ref m);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            // load page
            this.button_load_loadFromBatchNo.Enabled = bEnable;
            this.button_load_loadFromRecPathFile.Enabled = bEnable;
            this.button_load_loadFromBarcodeFile.Enabled = bEnable;

            this.checkBox_load_fillBiblioSummary.Enabled = bEnable;
            this.checkBox_load_fillOrderInfo.Enabled = bEnable;

            this.comboBox_load_type.Enabled = bEnable;

            // next button
            if (bEnable == true)
                SetNextButtonEnable();
            else
                this.button_next.Enabled = false;

            // print page
            this.button_print_optionHTML.Enabled = bEnable;
            this.button_print_optionText.Enabled = bEnable;
            this.button_print_optionWordXml.Enabled = bEnable;

            this.button_print_outputTextFile.Enabled = bEnable;
            this.button_print_printNormalList.Enabled = bEnable;
            this.button_print_outputWordXmlFile.Enabled = bEnable;

            this.button_print_outputExcelFile.Enabled = bEnable;

            this.button_print_runScript.Enabled = bEnable;
            this.button_print_createNewScriptFile.Enabled = bEnable;

        }



        // 检查路径所从属书目库是否为图书/期刊库？
        // return:
        //      -1  error
        //      0   不符合要求。提示信息在strError中
        //      1   符合要求
        internal override int CheckItemRecPath(string strPubType,
            string strItemRecPath,
            out string strError)
        {
            strError = "";

            string strItemDbName = Global.GetDbName(strItemRecPath);
            string strBiblioDbName = this.MainForm.GetBiblioDbNameFromItemDbName(strItemDbName);
            if (String.IsNullOrEmpty(strBiblioDbName) == true)
            {
                strError = "实体库 '" + strItemDbName + "' 未找到对应的书目库名";
                return -1;
            }

            string strIssueDbName = this.MainForm.GetIssueDbName(strBiblioDbName);

            if (strPubType == "图书")
            {
                if (String.IsNullOrEmpty(strIssueDbName) == false)
                {
                    strError = "路径 '" + strItemRecPath + "' 所从属的书目库 '" + strBiblioDbName + "' 为期刊型，和当前出版物类型 '" + strPubType + "' 不一致";
                    return 0;
                }
                return 1;
            }

            if (strPubType == "连续出版物")
            {
                if (String.IsNullOrEmpty(strIssueDbName) == true)
                {
                    strError = "路径 '" + strItemRecPath + "' 所从属的书目库 '" + strBiblioDbName + "' 为图书型，和当前出版物类型 '" + strPubType + "' 不一致";
                    return 0;
                }
                return 1;
            }

            strError = "CheckItemRecPath() 未知的出版物类型 '" + strPubType + "'";
            return -1;
        }



#if NO
        class RecordInfo
        {
            public DigitalPlatform.LibraryClient.localhost.Record Record = null;    // 册记录
            public XmlDocument Dom = null;  // 册记录XML装入DOM
            public string BiblioRecPath = "";
            public SummaryInfo SummaryInfo = null;  // 摘要信息
        }

        // 准备DOM和书目摘要等
        int GetSummaries(
            List<DigitalPlatform.LibraryClient.localhost.Record> records,
            out List<RecordInfo> infos,
            out string strError)
        {
            strError = "";
            infos = new List<RecordInfo>();

            // 准备DOM和书目摘要
            for (int i = 0; i < records.Count; i++)
            {
                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断1";
                        return -1;
                    }
                }

                RecordInfo info = new RecordInfo();
                info.Record = records[i];
                infos.Add(info);

                if (info.Record.RecordBody == null)
                {
                    strError = "请升级dp2Kernel到最新版本";
                    return -1;
                }

                if (info.Record.RecordBody.Result.ErrorCode != ErrorCodeValue.NoError)
                    continue;

                info.Dom = new XmlDocument();
                try
                {
                    info.Dom.LoadXml(info.Record.RecordBody.Xml);
                }
                catch (Exception ex)
                {
                    strError = "册记录的XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                // 准备书目记录路径
                string strParentID = DomUtil.GetElementText(info.Dom.DocumentElement,
"parent");
                string strBiblioDbName = this.MainForm.GetBiblioDbNameFromItemDbName(Global.GetDbName(info.Record.Path));
                if (string.IsNullOrEmpty(strBiblioDbName) == true)
                {
                    strError = "根据册记录路径 '" + info.Record.Path + "' 获得书目库名时出错";
                    return -1;
                }
                info.BiblioRecPath = strBiblioDbName + "/" + strParentID;


            }

            // 准备摘要
            if (this.checkBox_load_fillBiblioSummary.Checked == true)
            {
                // 归并书目记录路径
                List<string> bibliorecpaths = new List<string>();
                foreach (RecordInfo info in infos)
                {
                    bibliorecpaths.Add(info.BiblioRecPath);
                }

                // 去重
                StringUtil.RemoveDupNoSort(ref bibliorecpaths);

                // 看看cache中是否已经存在，如果已经存在则不再从服务器取
                for (int i = 0; i < bibliorecpaths.Count; i++ )
                {
                    string strPath = bibliorecpaths[i];
                    SummaryInfo summary = (SummaryInfo)this.m_summaryTable[strPath];
                    if (summary != null)
                    {
                        bibliorecpaths.RemoveAt(i);
                        i--;
                    }
                }

                // 从服务器获取
                if (bibliorecpaths.Count > 0)
                {
                REDO_GETBIBLIOINFO_0:
                    string strCommand = "@path-list:" + StringUtil.MakePathList(bibliorecpaths);

                    string[] formats = new string[2];
                    formats[0] = "summary";
                    formats[1] = "@isbnissn";
                    string[] results = null;
                    byte[] timestamp = null;

                    // stop.SetMessage("正在装入书目记录 '" + bibliorecpaths[0] + "' 等的摘要 ...");

                    // TODO: 有没有可能希望取的事项数目一次性取得没有取够?
                REDO_GETBIBLIOINFO:
                    long lRet = Channel.GetBiblioInfos(
                        stop,
                        strCommand,
                    "",
                        formats,
                        out results,
                        out timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        DialogResult temp_result = MessageBox.Show(this,
        strError + "\r\n\r\n是否重试?",
        "AccountBookForm",
        MessageBoxButtons.RetryCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_GETBIBLIOINFO;
                    }
                    if (lRet == -1 || lRet == 0)
                    {
                        if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                            strError = "书目记录 '" + StringUtil.MakePathList(bibliorecpaths) + "' 不存在";

                        strError = "获得书目摘要时发生错误: " + strError;
                        // 如果results.Length表现正常，其实还可以继续处理
                        if (results != null /* && results.Length == 2 * bibliorecpaths.Count */)
                        {
                        }
                        else
                            return -1;
                    }


                    if (results != null/* && results.Length == 2 * bibliorecpaths.Count*/)
                    {
                        // Debug.Assert(results != null && results.Length == 2 * bibliorecpaths.Count, "results必须包含 " + (2 * bibliorecpaths.Count).ToString() + " 个元素");

                        // 放入缓存
                        for (int i = 0; i < results.Length / 2; i++)
                        {
                            SummaryInfo summary = new SummaryInfo();

                            summary.Summary = results[i*2];
                            summary.ISBnISSn = results[i*2+1];

                            this.m_summaryTable[bibliorecpaths[i]] = summary;
                        }
                    }

                    if (results != null && results.Length != 2 * bibliorecpaths.Count)
                    {
                        // 没有取够，需要继续处理
                        bibliorecpaths.RemoveRange(0, results.Length / 2);
                        goto REDO_GETBIBLIOINFO_0;
                    }
                }

                // 挂接到每个记录附近
                foreach (RecordInfo info in infos)
                {
                    SummaryInfo summary = (SummaryInfo)this.m_summaryTable[info.BiblioRecPath];
                    if (summary == null)
                    {
                        strError = "缓存中找不到书目记录 '" + info.BiblioRecPath + "' 的摘要事项";
                        return -1;
                    }

                    info.SummaryInfo = summary;
                }

                // 避免cache占据的内存太多
                if (this.m_summaryTable.Count > 1000)
                    this.m_summaryTable.Clear();
            }

            return 0;
        }
#endif

        // 处理一小批记录的装入
        internal override int DoLoadRecords(List<string> lines,
            List<ListViewItem> items,
            bool bFillSummaryColumn,
            string[] summary_col_names,
            out string strError)
        {
            strError = "";

#if DEBUG
            if (items != null)
            {
                Debug.Assert(lines.Count == items.Count, "");
            }
#endif

            List<DigitalPlatform.LibraryClient.localhost.Record> records = new List<DigitalPlatform.LibraryClient.localhost.Record>();

            // 集中获取全部册记录信息
            for (; ; )
            {
                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断1";
                    return -1;
                }

                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                string[] paths = new string[lines.Count];
                lines.CopyTo(paths);
            REDO_GETRECORDS:
                long lRet = this.Channel.GetBrowseRecords(
                    this.stop,
                    paths,
                    "id,xml",
                    out searchresults,
                    out strError);
                if (lRet == -1)
                {
                    DialogResult temp_result = MessageBox.Show(this,
    strError + "\r\n\r\n是否重试?",
    "AccountBookForm",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO_GETRECORDS;
                    return -1;
                }


                records.AddRange(searchresults);

                // 去掉已经做过的一部分
                /*
                for (int i = 0; i < searchresults.Length; i++)
                {
                    lines.RemoveAt(0);
                }
                */
                lines.RemoveRange(0, searchresults.Length);

                if (lines.Count == 0)
                    break;
            }

            // 准备DOM和书目摘要等
            List<RecordInfo> infos = null;
            int nRet = GetSummaries(
                bFillSummaryColumn,
                summary_col_names,
                records,
                out infos,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(records.Count == infos.Count, "");

            List<OrderInfo> orderinfos = new List<OrderInfo>();

            this.listView_in.BeginUpdate();
            try
            {

                for (int i = 0; i < infos.Count; i++)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断1";
                            return -1;
                        }
                    }

                    RecordInfo info = infos[i];

                    if (info.Record.RecordBody == null)
                    {
                        strError = "请升级 dp2Kernel 到最新版本";
                        return -1;
                    }
                    // stop.SetMessage("正在装入路径 " + strLine + " 对应的记录...");


                    string strOutputItemRecPath = "";
                    ListViewItem item = null;

                    if (items != null)
                        item = items[i];

                    // 根据册条码号，装入册记录
                    // return: 
                    //      -2  册条码号已经在list中存在了
                    //      -1  出错
                    //      1   成功
                    nRet = LoadOneItem(
                        this.comboBox_load_type.Text,
                        bFillSummaryColumn,
                        summary_col_names,
                        "@path:" + info.Record.Path,
                        info,
                        this.listView_in,
                        null,
                        out strOutputItemRecPath,
                        ref item,
                        out strError);
                    /*
                    if (nRet == -2)
                        nDupCount++;
                     * */
                    /*
                    if (nRet == -1)
                        goto ERROR1;
                     * */


                    // 准备装入订购信息
                    if (nRet != -1 && this.checkBox_load_fillOrderInfo.Checked == true)
                    {
                        Debug.Assert(item != null, "");
                        string strRefID = ListViewUtil.GetItemText(item, COLUMN_REFID);
                        if (String.IsNullOrEmpty(strRefID) == false)
                        {
                            OrderInfo order_info = new OrderInfo();
                            order_info.ItemRefID = strRefID;
                            order_info.ListViewItem = item;
                            orderinfos.Add(order_info);
                        }
                    }

                }
            }
            finally
            {
                this.listView_in.EndUpdate();
            }

            // 从服务器获得订购记录的路径
            if (orderinfos.Count > 0)
            {
                nRet = LoadOrderInfo(
                    orderinfos,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        // 根据册记录refid，转换为订购记录的recpath，然后获得订购记录XML
        int LoadOrderInfo(
            List<OrderInfo> orderinfos,
            out string strError)
        {
            strError = "";

            List<string> refids = new List<string>();
            foreach (OrderInfo info in orderinfos)
            {
                Debug.Assert(string.IsNullOrEmpty(info.ItemRefID) == false, "");
                refids.Add(info.ItemRefID);
            }

            // TODO: 如果只有一两个refid，可以简化为直接获取，用原来的方法

            // 已经是一小批，可以一次性获取
            string strBiblio = "";
            string strResult = "";
            string strItemRecPath = "";
            byte[] item_timestamp = null;
            string strBiblioRecPath = "";
            long lRet = 0;
            string strRecordName = "";

            if (this.comboBox_load_type.Text == "图书")
            {
                strRecordName = "订购记录";
                lRet = this.Channel.GetOrderInfo(stop,
                     "@item-refid-list:" + StringUtil.MakePathList(refids),
                     "get-path-list",
                     out strResult,
                     out strItemRecPath,
                     out item_timestamp,
                     "", // strBiblioType,
                     out strBiblio,
                     out strBiblioRecPath,
                     out strError);
            }
            else
            {
                strRecordName = "期记录";
                lRet = this.Channel.GetIssueInfo(stop,
                     "@item-refid-list:" + StringUtil.MakePathList(refids),
                     "get-path-list",
                     out strResult,
                     out strItemRecPath,
                     out item_timestamp,
                     "", // strBiblioType,
                     out strBiblio,
                     out strBiblioRecPath,
                     out strError);
            }

            if (lRet == -1)
                return -1;

            List<string> recpaths = new List<string>(strResult.Split(new char[] { ',' }));
            Debug.Assert(refids.Count == recpaths.Count, "");

            // List<string> notfound_refids = new List<string>();
            List<string> errors = new List<string>();
            {
                int i = 0;
                foreach (string recpath in recpaths)
                {
                    OrderInfo info = orderinfos[i];

                    if (string.IsNullOrEmpty(recpath) == true)
                    {
                        // notfound_refids.Add(recpaths[i]);
                        ListViewUtil.ChangeItemText(info.ListViewItem,
                            EXTEND_COLUMN_CATALOGNO,
                            "册参考ID '" + info.ItemRefID + "' 没有找到对应的" + strRecordName);
                    }
                    else if (recpath[0] == '!')
                        errors.Add(recpath.Substring(1));
                    else
                        info.OrderRecPath = recpath;

                    i++;
                }
            }

            if (errors.Count > 0)
                strError = "获得" + strRecordName + "的过程发生错误: " + StringUtil.MakePathList(errors);

#if NO
            if (notfound_refids.Count > 0)
            {
                if (string.IsNullOrEmpty(strError) == false)
                    strError += ";\r\n";

                strError += "下列 册记录参考ID 没有找到: " + StringUtil.MakePathList(notfound_refids);
            }
#endif

            if (string.IsNullOrEmpty(strError) == false)
                return -1;

            // 成批获得订购记录
            List<string> order_recpaths = new List<string>();
            foreach (OrderInfo info in orderinfos)
            {
                if (String.IsNullOrEmpty(info.OrderRecPath) == false)
                    order_recpaths.Add(info.OrderRecPath);
            }

            if (order_recpaths.Count > 0)
            {
                List<string> lines = new List<string>();
                lines.AddRange(order_recpaths);

                List<DigitalPlatform.LibraryClient.localhost.Record> records = new List<DigitalPlatform.LibraryClient.localhost.Record>();

                // 集中获取全部册记录信息
                for (; ; )
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断1";
                            return -1;
                        }
                    }

                    DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                    string[] paths = new string[lines.Count];
                    lines.CopyTo(paths);
                REDO_GETRECORDS:
                    lRet = this.Channel.GetBrowseRecords(
                        this.stop,
                        paths,
                        "id,xml",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        DialogResult temp_result = MessageBox.Show(this,
        strError + "\r\n\r\n是否重试?",
        "AccountBookForm",
        MessageBoxButtons.RetryCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_GETRECORDS;
                        return -1;
                    }


                    records.AddRange(searchresults);

                    // 去掉已经做过的一部分
                    /*
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        lines.RemoveAt(0);
                    }
                     * */
                    lines.RemoveRange(0, searchresults.Length);

                    if (lines.Count == 0)
                        break;
                }

                // 把命中的XML记录记载到orderinfos的对应位置
                foreach (OrderInfo info in orderinfos)
                {
                    if (String.IsNullOrEmpty(info.OrderRecPath) == true)
                        continue;
                    int index = order_recpaths.IndexOf(info.OrderRecPath);
                    if (index == -1)
                    {
                        Debug.Assert(false, "");
                        strError = strRecordName + "路径在 order_recpaths 中没有找到";
                        return -1;
                    }

                    DigitalPlatform.LibraryClient.localhost.Record record = records[index];
                    if (record.RecordBody != null
                        && record.RecordBody.Result != null
                        && record.RecordBody.Result.ErrorCode != ErrorCodeValue.NoError)
                    {
                        ListViewUtil.ChangeItemText(info.ListViewItem,
    EXTEND_COLUMN_CATALOGNO,
    "获取" + strRecordName + " '" + info.OrderRecPath + "' 时出错: " + record.RecordBody.Result.ErrorString);
                        continue;
                    }

                    if (record.RecordBody != null)
                    {
                        info.OrderXml = record.RecordBody.Xml;

                        FillOrderColumns(info, this.comboBox_load_type.Text);
                    }
                }
            }

            return 0;
        }

        internal class OrderInfo
        {
            public ListViewItem ListViewItem = null;    // 列表事项
            public string ItemRefID = "";       // 册记录REFID
            public string OrderRecPath = "";    // 订购记录路径
            public string OrderXml = "";    // 订购记录XML
        }

        /// <summary>
        /// 清除列表中现存的内容，准备装入新内容
        /// </summary>
        public override void ClearBefore()
        {
            base.ClearBefore();

            this.listView_in.Items.Clear();
            this.SortColumns_in.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_in.Columns);

            this.refid_table.Clear();
            this.orderxml_table.Clear();
        }

#if NO
        // 从记录路径文件装载
        /// <summary>
        /// 从记录路径文件装载
        /// </summary>
        /// <param name="strRecPathFilename">记录路径文件名(全路径)</param>
        /// <param name="bClearBefore">是否要在装载前情况浏览列表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错，错误信息在 strError 参数返回; 0: 成功</returns>
        public int LoadFromRecPathFile(string strRecPathFilename,
            bool bClearBefore,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (bClearBefore == true)
                ClearBefore();

            string strTimeMessage = "";

            StreamReader sr = null;
            try
            {
                // 打开文件
                sr = new StreamReader(strRecPathFilename);

                EnableControls(false);
                // MainForm.ShowProgress(true);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在初始化浏览器组件 ...");
                stop.BeginLoop();
                this.Update();
                this.MainForm.Update();

                try
                {
                    // this.m_nGreenItemCount = 0;

                    // 逐行读入文件内容
                    // 测算文件行数
                    int nLineCount = 0;
                    for (; ; )
                    {
                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断1";
                                goto ERROR1;
                            }
                        }

                        string strLine = "";
                        strLine = sr.ReadLine();

                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        // 检查路径所从属书目库是否为图书/期刊库？
                        // return:
                        //      -1  error
                        //      0   不符合要求。提示信息在strError中
                        //      1   符合要求
                        nRet = CheckItemRecPath(this.comboBox_load_type.Text,
                            strLine,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                        }

                        nLineCount++;
                        // stop.SetMessage("正在装入册条码号 " + strLine + " 对应的记录...");
                    }

                    // 设置进度范围
                    stop.SetProgressRange(0, nLineCount);
                    sr.Close();

                    ProgressEstimate estimate = new ProgressEstimate();
                    estimate.SetRange(0, nLineCount);
                    estimate.Start();

                    List<string> lines = new List<string>();
                    // 正式开始处理
                    sr = new StreamReader(strRecPathFilename);
                    for (int i = 0; ; i++)
                    {
                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断1";
                                goto ERROR1;
                            }
                        }

                        string strLine = "";
                        strLine = sr.ReadLine();

                        stop.SetProgressValue(i);

                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (strLine[0] == '#')
                            continue;   // 注释行

                        lines.Add(strLine);
                        if (lines.Count >= 100)
                        {
                            if (lines.Count > 0)
                                stop.SetMessage("(" + i.ToString() + " / " + nLineCount.ToString() + ") 正在装入路径 " + lines[0] + " 等记录。"
                                    + "剩余时间 " + ProgressEstimate.Format(estimate.Estimate(i)) + " 已经过时间 " + ProgressEstimate.Format(estimate.delta_passed));

                            // 处理一小批记录的装入
                            nRet = DoLoadRecords(lines,
                                null,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            lines.Clear();
                        }
                    }

                    // 最后剩下的一批
                    if (lines.Count > 0)
                    {
                        if (lines.Count > 0)
                            stop.SetMessage("(" + nLineCount.ToString() + " / " + nLineCount.ToString() + ") 正在装入路径 " + lines[0] + " 等记录...");

                        // 处理一小批记录的装入
                        nRet = DoLoadRecords(lines,
                            null,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        lines.Clear();
                    }

                    strTimeMessage = "共装入册记录 " + nLineCount.ToString() + " 条。耗费时间: " + estimate.GetTotalTime().ToString();
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("装入完成。");
                    stop.HideProgress();

                    EnableControls(true);
                    // MainForm.ShowProgress(false);
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                sr.Close();
            }

            this.MainForm.StatusBarMessage = strTimeMessage;

            return 0;
        ERROR1:
            return -1;
        }
#endif

        // 根据记录路径文件装载
        // TODO: 是否要检查记录路径重复?
        private void button_load_loadFromRecPathFile_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的记录路径文件名";
            dlg.FileName = this.RecPathFilePath;
            // dlg.InitialDirectory = 
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.SourceStyle = "recpathfile";

            // int nDupCount = 0;
            nRet = LoadFromRecPathFile(dlg.FileName,
                this.comboBox_load_type.Text,
                this.checkBox_load_fillBiblioSummary.Checked,
                new string[] { "summary", "@isbnissn" },
                (System.Windows.Forms.Control.ModifierKeys == Keys.Control ? false : true),
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 记忆文件名
            this.RecPathFilePath = dlg.FileName;
            this.Text = "打印财产帐 " + Path.GetFileName(this.RecPathFilePath);

            /*
            if (nDupCount != 0)
            {
                MessageBox.Show(this, "装入过程中有 " + nDupCount.ToString() + "个重复条码事项被忽略。");
            }
             * */

            // 汇报数据装载情况。
            // return:
            //      0   尚未装载任何数据    
            //      1   装载已经完成
            //      2   虽然装载了数据，但是其中有错误事项
            int nState = ReportLoadState(out strError);
            if (nState != 1)
                goto ERROR1;
            return;
        ERROR1:
            this.Text = "打印财产帐";
            MessageBox.Show(this, strError);
        }

#if NO
        // 根据记录路径文件装载
        private void button_load_loadFromRecPathFile_Click(object sender, EventArgs e)
        {
            int nRet = 0;

            this.ClearErrorInfoForm();

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的记录路径文件名";
            dlg.FileName = this.RecPathFilePath;
            // dlg.InitialDirectory = 
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.SourceStyle = "recpathfile";

            int nDupCount = 0;

            string strError = "";
            StreamReader sr = null;
            try
            {
                // 打开文件
                sr = new StreamReader(dlg.FileName);


                EnableControls(false);
                // MainForm.ShowProgress(true);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在初始化浏览器组件 ...");
                stop.BeginLoop();
                this.Update();
                this.MainForm.Update();

                try
                {
                    if (Control.ModifierKeys == Keys.Control)
                    {
                    }
                    else
                    {
                        this.listView_in.Items.Clear();
                        this.SortColumns_in.Clear();
                        SortColumns.ClearColumnSortDisplay(this.listView_in.Columns);

                        this.refid_table.Clear();
                        this.orderxml_table.Clear();
                    }


                    // this.m_nGreenItemCount = 0;

                    // 逐行读入文件内容
                    // 测算文件行数
                    int nLineCount = 0;
                    for (; ; )
                    {
                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断1";
                                goto ERROR1;
                            }
                        }


                        string strLine = "";
                        strLine = sr.ReadLine();

                        if (strLine == null)
                            break;

                        nLineCount++;
                        // stop.SetMessage("正在装入册条码号 " + strLine + " 对应的记录...");
                    }

                    // 设置进度范围
                    stop.SetProgressRange(0, nLineCount);
                    // stop.SetProgressValue(0);

                    // 逐行处理
                    // 文件回头?
                    // sr.BaseStream.Seek(0, SeekOrigin.Begin);

                    sr.Close();

                    sr = new StreamReader(dlg.FileName);


                    for (int i = 0; ; i++)
                    {
                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断1";
                                goto ERROR1;
                            }
                        }

                        string strLine = "";
                        strLine = sr.ReadLine();

                        stop.SetProgressValue(i);

                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (strLine[0] == '#')
                            continue;   // 注释行

                        // 检查路径所从属书目库是否为图书/期刊库？
                        // return:
                        //      -1  error
                        //      0   不符合要求。提示信息在strError中
                        //      1   符合要求
                        nRet = CheckItemRecPath(this.comboBox_load_type.Text,
                            strLine,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                        }



                        stop.SetMessage("正在装入路径 " + strLine + " 对应的记录...");


                        string strOutputItemRecPath = "";
                        // 根据册条码号，装入册记录
                        // return: 
                        //      -2  册条码号已经在list中存在了
                        //      -1  出错
                        //      1   成功
                        nRet = LoadOneItem(
                            this.comboBox_load_type.Text,
                            "@path:" + strLine,
                            this.listView_in,
                            null,
                            out strOutputItemRecPath,
                            out strError);
                        if (nRet == -2)
                            nDupCount++;
                        /*
                        if (nRet == -1)
                            goto ERROR1;
                         * */

                    }
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("装入完成。");
                    stop.HideProgress();

                    EnableControls(true);
                    // MainForm.ShowProgress(false);
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                sr.Close();
            }

            // 记忆文件名
            this.RecPathFilePath = dlg.FileName;
            this.Text = "打印财产帐 " + Path.GetFileName(this.RecPathFilePath);

            if (nDupCount != 0)
            {
                MessageBox.Show(this, "装入过程中有 " + nDupCount.ToString() + "个重复条码事项被忽略。");
            }

            // 汇报数据装载情况。
            // return:
            //      0   尚未装载任何数据    
            //      1   装载已经完成
            //      2   虽然装载了数据，但是其中有错误事项
            int nState = ReportLoadState(out strError);
            if (nState != 1)
                goto ERROR1;
            return;
        ERROR1:
            this.Text = "打印财产帐";
            MessageBox.Show(this, strError);
        }
#endif

#if NO
        int ConvertItemBarcodeToRecPath(
            List<string> barcodes,
            out List<string> recpaths,
            out string strError)
        {
            strError = "";
            recpaths = null;

        REDO_GETITEMINFO:
            string strBiblio = "";
            string strResult = "";
            long lRet = this.Channel.GetItemInfo(stop,
                "@barcode-list:" + StringUtil.MakePathList(barcodes),
                "get-path-list",
                out strResult,
                "", // strBiblioType,
                out strBiblio,
                out strError);
            if (lRet == -1)
                return -1;
            recpaths = StringUtil.SplitList(strResult);
            Debug.Assert(barcodes.Count == recpaths.Count, "");

            List<string> notfound_barcodes = new List<string>();
            List<string> errors = new List<string>();
            {
                int i = 0;
                foreach (string recpath in recpaths)
                {
                    if (string.IsNullOrEmpty(recpath) == true)
                        notfound_barcodes.Add(barcodes[i]);
                    else if (recpath[0] == '!')
                        errors.Add(recpath.Substring(1));
                    i++;
                }
            }

            if (errors.Count > 0)
            {
                strError = "转换册条码号的过程发生错误: " + StringUtil.MakePathList(errors);

                DialogResult temp_result = MessageBox.Show(this,
strError + "\r\n\r\n是否重试?",
"AccountBookForm",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (temp_result == DialogResult.Retry)
                    goto REDO_GETITEMINFO;
                return -1;
            }

            if (notfound_barcodes.Count > 0)
            {
                if (string.IsNullOrEmpty(strError) == false)
                    strError += ";\r\n";

                strError += "下列册条码号没有找到: " + StringUtil.MakePathList(notfound_barcodes);
                DialogResult temp_result = MessageBox.Show(this,
strError + "\r\n\r\n是否继续处理?",
"AccountBookForm",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (temp_result == DialogResult.Cancel)
                    return -1;
            }

            /*
            if (string.IsNullOrEmpty(strError) == false)
                return -1;
             * */
            // 把空字符串和 ！ 打头的都去掉
            for (int i = 0; i < recpaths.Count; i++)
            {
                string recpath = recpaths[i];
                if (string.IsNullOrEmpty(recpath) == true)
                {
                    recpaths.RemoveAt(i);
                    i--;
                }
                else if (recpath[0] == '!')
                {
                    recpaths.RemoveAt(i);
                    i--;
                }
            }

            return 0;
        }
#endif

#if NO
        // 根据册条码号文件得到记录路径文件
        int ConvertBarcodeFile(string strBarcodeFilename,
            string strRecPathFilename,
            out int nDupCount,
            out string strError)
        {
            nDupCount = 0;
            strError = "";
            int nRet = 0;

            StreamReader sr = null;
            StreamWriter sw = null;

            try
            {
                // 打开文件
                sr = new StreamReader(strBarcodeFilename);

                sw = new StreamWriter(strRecPathFilename);

                EnableControls(false);
                // MainForm.ShowProgress(true);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在将册条码号转换为记录路径 ...");
                stop.BeginLoop();
                this.Update();
                this.MainForm.Update();

                try
                {
#if NO
                    if (Control.ModifierKeys == Keys.Control)
                    {
                    }
                    else
                    {
                        this.listView_in.Items.Clear();
                        this.SortColumns_in.Clear();
                        SortColumns.ClearColumnSortDisplay(this.listView_in.Columns);

                        this.refid_table.Clear();
                        this.orderxml_table.Clear();
                    }
#endif

                    Hashtable barcode_table = new Hashtable();
                    // this.m_nGreenItemCount = 0;

                    // 逐行读入文件内容
                    // 测算文件行数
                    int nLineCount = 0;
                    List<string> lines = new List<string>();
                    for (; ; )
                    {
                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断1";
                                goto ERROR1;
                            }
                        }

                        string strLine = "";
                        strLine = sr.ReadLine();

                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (strLine[0] == '#')
                            continue;   // 注释行

                        if (barcode_table[strLine] != null)
                        {
                            nDupCount++;
                            continue;
                        }

                        barcode_table[strLine] = true;
                        lines.Add(strLine);
                        nLineCount++;
                        // stop.SetMessage("正在装入册条码号 " + strLine + " 对应的记录...");
                    }

                    barcode_table.Clear(); // 腾出空间

                    // 设置进度范围
                    stop.SetProgressRange(0, nLineCount);
                    // stop.SetProgressValue(0);

                    // 逐行处理
                    // 文件回头?
                    // sr.BaseStream.Seek(0, SeekOrigin.Begin);

                    sr.Close();
                    sr = null;


                    int i = 0;
                    List<string> temp_lines = new List<string>();
                    foreach (string strLine in lines)
                    {
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断1";
                            goto ERROR1;
                        }

                        stop.SetProgressValue(i++);

                        // stop.SetMessage("正在装入册条码号 " + strLine + " 对应的记录...");

                        temp_lines.Add(strLine);
                        if (temp_lines.Count >= 100)
                        {
                            // 将册条码号转换为册记录路径
                            List<string> recpaths = null;
                            nRet = ConvertItemBarcodeToRecPath(
                                temp_lines,
                                out recpaths,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            foreach (string recpath in recpaths)
                            {
                                sw.WriteLine(recpath);
                            }
                            temp_lines.Clear();
                        }
                    }

                    // 最后一批
                    if (temp_lines.Count > 0)
                    {
                        // 将册条码号转换为册记录路径
                        List<string> recpaths = null;
                        nRet = ConvertItemBarcodeToRecPath(
                            temp_lines,
                            out recpaths,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        foreach (string recpath in recpaths)
                        {
                            sw.WriteLine(recpath);
                        }
                        temp_lines.Clear();
                    }
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("装入完成。");
                    stop.HideProgress();

                    EnableControls(true);
                    // MainForm.ShowProgress(false);
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
                if (sw != null)
                    sw.Close();

            }

            return 0;
        ERROR1:
            return -1;
        }
#endif

        // 根据条码号文件装载
        private void button_load_loadFromBarcodeFile_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";

            bool bClearBefore = true;
            if (System.Windows.Forms.Control.ModifierKeys == Keys.Control)
                bClearBefore = false;

            if (bClearBefore == true)
                ClearBefore();

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的条码号文件名";
            dlg.FileName = this.BarcodeFilePath;
            // dlg.InitialDirectory = 
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.SourceStyle = "barcodefile";

            int nDupCount = 0;
            string strRecPathFilename = Path.GetTempFileName();
            try
            {
                nRet = ConvertBarcodeFile(dlg.FileName,
                    strRecPathFilename,
                    out nDupCount,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = LoadFromRecPathFile(strRecPathFilename,
                    this.comboBox_load_type.Text,
                    this.checkBox_load_fillBiblioSummary.Checked,
                    new string[] { "summary", "@isbnissn" },
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                if (string.IsNullOrEmpty(strRecPathFilename) == false)
                {
                    File.Delete(strRecPathFilename);
                    strRecPathFilename = "";
                }
            }

            if (nDupCount != 0)
            {
                MessageBox.Show(this, "装入过程中有 " + nDupCount.ToString() + "个重复条码事项被忽略。");
            }

            // 记忆文件名
            this.BarcodeFilePath = dlg.FileName;
            this.Text = "打印财产帐 " + Path.GetFileName(this.BarcodeFilePath);

            // 汇报数据装载情况。
            // return:
            //      0   尚未装载任何数据    
            //      1   装载已经完成
            //      2   虽然装载了数据，但是其中有错误事项
            int nState = ReportLoadState(out strError);
            if (nState != 1)
                goto ERROR1;
            return;
        ERROR1:
            this.Text = "打印财产帐";
            MessageBox.Show(this, strError);
        }


#if NO
        // 根据条码号文件装载
        private void button_load_loadFromBarcodeFile_Click(object sender, EventArgs e)
        {
            int nRet = 0;

            this.ClearErrorInfoForm();
            this.m_summaryTable.Clear();

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的条码号文件名";
            dlg.FileName = this.BarcodeFilePath;
            // dlg.InitialDirectory = 
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.SourceStyle = "barcodefile";

            int nDupCount = 0;

            string strError = "";
            StreamReader sr = null;
            try
            {
                // 打开文件
                sr = new StreamReader(dlg.FileName);


                EnableControls(false);
                // MainForm.ShowProgress(true);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在初始化浏览器组件 ...");
                stop.BeginLoop();
                this.Update();
                this.MainForm.Update();

                try
                {
                    if (Control.ModifierKeys == Keys.Control)
                    {
                    }
                    else
                    {
                        this.listView_in.Items.Clear();
                        this.SortColumns_in.Clear();
                        SortColumns.ClearColumnSortDisplay(this.listView_in.Columns);

                        this.refid_table.Clear();
                        this.orderxml_table.Clear();
                    }


                    // this.m_nGreenItemCount = 0;

                    // 逐行读入文件内容
                    // 测算文件行数
                    int nLineCount = 0;
                    for (; ; )
                    {
                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断1";
                                goto ERROR1;
                            }
                        }


                        string strLine = "";
                        strLine = sr.ReadLine();

                        if (strLine == null)
                            break;

                        nLineCount++;
                        // stop.SetMessage("正在装入册条码号 " + strLine + " 对应的记录...");
                    }

                    // 设置进度范围
                    stop.SetProgressRange(0, nLineCount);
                    // stop.SetProgressValue(0);

                    // 逐行处理
                    // 文件回头?
                    // sr.BaseStream.Seek(0, SeekOrigin.Begin);

                    sr.Close();

                    sr = new StreamReader(dlg.FileName);


                    for (int i=0; ; i++)
                    {
                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断1";
                                goto ERROR1;
                            }
                        }

                        string strLine = "";
                        strLine = sr.ReadLine();

                        stop.SetProgressValue(i);

                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (strLine[0] == '#')
                            continue;   // 注释行

                        stop.SetMessage("正在装入册条码号 " + strLine + " 对应的记录...");


                        string strOutputItemRecPath = "";
                        // 根据册条码号，装入册记录
                        // return: 
                        //      -2  册条码号已经在list中存在了
                        //      -1  出错
                        //      1   成功
                        nRet = LoadOneItem(
                            this.comboBox_load_type.Text,
                            strLine,
                            null,
                            this.listView_in,
                            null,
                            out strOutputItemRecPath,
                            out strError);
                        if (nRet == -2)
                            nDupCount++;

                        // 检查路径所从属书目库是否为图书/期刊库？
                        // return:
                        //      -1  error
                        //      0   不符合要求。提示信息在strError中
                        //      1   符合要求
                        nRet = CheckItemRecPath(this.comboBox_load_type.Text,
                            strOutputItemRecPath,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            GetErrorInfoForm().WriteHtml("册条码号为 " + strLine + " 的册记录 " + strError + "\r\n");
                        }

                        /*
                        if (nRet == -1)
                            goto ERROR1;
                         * */

                    }
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("装入完成。");
                    stop.HideProgress();

                    EnableControls(true);
                    // MainForm.ShowProgress(false);
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                sr.Close();
            }

            // 记忆文件名
            this.BarcodeFilePath = dlg.FileName;
            this.Text = "打印财产帐 " + Path.GetFileName(this.BarcodeFilePath);

            if (nDupCount != 0)
            {
                MessageBox.Show(this, "装入过程中有 " +nDupCount.ToString() + "个重复条码事项被忽略。");
            }

            // 汇报数据装载情况。
            // return:
            //      0   尚未装载任何数据    
            //      1   装载已经完成
            //      2   虽然装载了数据，但是其中有错误事项
            int nState = ReportLoadState(out strError);
            if (nState != 1)
                goto ERROR1;
            return;
        ERROR1:
            this.Text = "打印财产帐";
            MessageBox.Show(this, strError);
        }
#endif

        // 设置listview栏目标题
        void CreateColumnHeader(ListView list)
        {
            ColumnHeader columnHeader_barcode = new ColumnHeader();
            ColumnHeader columnHeader_state = new ColumnHeader();
            ColumnHeader columnHeader_location = new ColumnHeader();
            ColumnHeader columnHeader_price = new ColumnHeader();
            ColumnHeader columnHeader_bookType = new ColumnHeader();
            ColumnHeader columnHeader_comment = new ColumnHeader();
            ColumnHeader columnHeader_borrower = new ColumnHeader();
            ColumnHeader columnHeader_borrowDate = new ColumnHeader();
            ColumnHeader columnHeader_borrowPeriod = new ColumnHeader();
            ColumnHeader columnHeader_recpath = new ColumnHeader();
            ColumnHeader columnHeader_errorInfo = new ColumnHeader();
            ColumnHeader columnHeader_registerNo = new ColumnHeader();
            ColumnHeader columnHeader_mergeComment = new ColumnHeader();
            ColumnHeader columnHeader_batchNo = new ColumnHeader();
            ColumnHeader columnHeader_biblioRecpath = new ColumnHeader();
            ColumnHeader columnHeader_refID = new ColumnHeader();
            ColumnHeader columnHeader_accessno = new ColumnHeader();

            ColumnHeader columnHeader_isbnIssn = new ColumnHeader();
            ColumnHeader columnHeader_publishTime = new ColumnHeader();
            ColumnHeader columnHeader_volume = new ColumnHeader();
            ColumnHeader columnHeader_class = new ColumnHeader();
            ColumnHeader columnHeader_catalogNo = new ColumnHeader();
            ColumnHeader columnHeader_orderTime = new ColumnHeader();
            ColumnHeader columnHeader_orderID = new ColumnHeader();
            ColumnHeader columnHeader_seller = new ColumnHeader();
            ColumnHeader columnHeader_source = new ColumnHeader();
            ColumnHeader columnHeader_orderPrice = new ColumnHeader();
            ColumnHeader columnHeader_acceptPrice = new ColumnHeader();


            list.Columns.Clear();

            list.Columns.AddRange(new ColumnHeader[] {
            columnHeader_barcode,
            columnHeader_errorInfo,
            columnHeader_isbnIssn,
            columnHeader_state,

            columnHeader_accessno,
            columnHeader_publishTime,
            columnHeader_volume,

            columnHeader_location,
            columnHeader_price,
            columnHeader_bookType,
            columnHeader_registerNo,
            columnHeader_comment,
            columnHeader_mergeComment,
            columnHeader_batchNo,

                /*
            columnHeader_borrower,
            columnHeader_borrowDate,
            columnHeader_borrowPeriod,
                 * */

            columnHeader_recpath,
            columnHeader_biblioRecpath,
            columnHeader_refID,

            columnHeader_class,
            columnHeader_catalogNo,
            columnHeader_orderTime,
            columnHeader_orderID,
            columnHeader_seller,
            columnHeader_source,
            columnHeader_orderPrice,
            columnHeader_acceptPrice
            });

            // 
            // columnHeader_isbnIssn
            // 
            columnHeader_isbnIssn.Text = "ISBN/ISSN";
            columnHeader_isbnIssn.Width = 160;
            // 
            // columnHeader_volume
            // 
            columnHeader_volume.Text = "卷期";
            columnHeader_volume.Width = 100;
            // 
            // columnHeader_publishTime
            // 
            columnHeader_publishTime.Text = "出版时间";
            columnHeader_publishTime.Width = 100;
            // 
            // columnHeader_class
            // 
            columnHeader_class.Text = "类别";
            columnHeader_class.Width = 100;
            // 
            // columnHeader_catalogNo
            // 
            columnHeader_catalogNo.Text = "书目号";
            columnHeader_catalogNo.Width = 100;
            // 
            // columnHeader_orderTime
            // 
            columnHeader_orderTime.Text = "订购时间";
            columnHeader_orderTime.Width = 150;
            // 
            // columnHeader_orderID
            // 
            columnHeader_orderID.Text = "订单号";
            columnHeader_orderID.Width = 150;
            // 
            // columnHeader_seller
            // 
            columnHeader_seller.Text = "渠道";
            columnHeader_seller.Width = 150;
            // 
            // columnHeader_source
            // 
            columnHeader_source.Text = "经费来源";
            columnHeader_source.Width = 150;

            // 
            // columnHeader_orderPrice
            // 
            columnHeader_orderPrice.Text = "订购价";
            columnHeader_orderPrice.Width = 150;

            // 
            // columnHeader_acceptPrice
            // 
            columnHeader_acceptPrice.Text = "到书价";
            columnHeader_acceptPrice.Width = 150;



            // 
            // columnHeader_refID
            // 
            columnHeader_refID.Text = "参考ID";
            columnHeader_refID.Width = 100;



            // 
            // columnHeader_barcode
            // 
            columnHeader_barcode.Text = "册条码号";
            columnHeader_barcode.Width = 150;
            // 
            // columnHeader_errorInfo
            // 
            columnHeader_errorInfo.Text = "摘要/错误信息";
            columnHeader_errorInfo.Width = 200;
            // 
            // columnHeader_state
            // 
            columnHeader_state.Text = "状态";
            columnHeader_state.Width = 100;
            // 
            // columnHeader_location
            // 
            columnHeader_location.Text = "馆藏地点";
            columnHeader_location.Width = 150;
            // 
            // columnHeader_price
            // 
            columnHeader_price.Text = "册价格";
            columnHeader_price.Width = 150;
            // 
            // columnHeader_bookType
            // 
            columnHeader_bookType.Text = "册类型";
            columnHeader_bookType.Width = 150;
            // 
            // columnHeader_registerNo
            // 
            columnHeader_registerNo.Text = "登录号";
            columnHeader_registerNo.Width = 150;
            // 
            // columnHeader_comment
            // 
            columnHeader_comment.Text = "附注";
            columnHeader_comment.Width = 150;
            // 
            // columnHeader_mergeComment
            // 
            columnHeader_mergeComment.Text = "合并注释";
            columnHeader_mergeComment.Width = 150;
            // 
            // columnHeader_batchNo
            // 
            columnHeader_batchNo.Text = "批次号";
            // 
            // columnHeader_borrower
            // 
            columnHeader_borrower.Text = "借阅者";
            columnHeader_borrower.Width = 150;
            // 
            // columnHeader_borrowDate
            // 
            columnHeader_borrowDate.Text = "借阅日期";
            columnHeader_borrowDate.Width = 150;
            // 
            // columnHeader_borrowPeriod
            // 
            columnHeader_borrowPeriod.Text = "借阅期限";
            columnHeader_borrowPeriod.Width = 150;
            // 
            // columnHeader_recpath
            // 
            columnHeader_recpath.Text = "册记录路径";
            columnHeader_recpath.Width = 200;
            // 
            // columnHeader_biblioRecpath
            // 
            columnHeader_biblioRecpath.Text = "种记录路径";
            columnHeader_biblioRecpath.Width = 200;
            // 
            // columnHeader_accessno
            // 
            columnHeader_accessno.Text = "索取号";
            columnHeader_accessno.Width = 200;
        }

#if NO
        // 书目记录路径 --> SummaryInfo
        Hashtable m_summaryTable = new Hashtable();
        class SummaryInfo
        {
            public string Summary = "";
            public string ISBnISSn = "";
        }
#endif

        internal override void SetError(ListView list,
            ref ListViewItem item,
            string strBarcodeOrRecPath,
            string strError)
        {
            if (item == null)
            {
                item = new ListViewItem(strBarcodeOrRecPath, 0);
                list.Items.Add(item);
            }
            else
            {
                Debug.Assert(item.ListView == list, "");
            }

            // item.SubItems.Add(strError);
            ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, strError);

            SetItemColor(item, TYPE_ERROR);

            // 将新加入的事项滚入视野
            list.EnsureVisible(list.Items.IndexOf(item));
        }

#if NO
        // 根据册条码号或者记录路径，装入册记录
        // parameters:
        //      strBarcodeOrRecPath 册条码号或者记录路径。如果内容前缀为"@path:"则表示为路径
        //      strMatchLocation    附加的馆藏地点匹配条件。如果==null，表示没有这个附加条件(注意，""和null含义不同，""表示确实要匹配这个值)
        // return: 
        //      -2  册条码号或者记录路径已经在list中存在了(行没有加入listview中)
        //      -1  出错(注意表示出错的行已经加入listview中了)
        //      0   因为馆藏地点不匹配，没有加入list中
        //      1   成功
        int LoadOneItem(
            string strPubType,
            string strBarcodeOrRecPath,
            RecordInfo info,
            ListView list,
            string strMatchLocation,
            out string strOutputItemRecPath,
            ref ListViewItem item,
            out string strError)
        {
            strError = "";
            strOutputItemRecPath = "";
            long lRet = 0;

            // 判断是否有 @path: 前缀，便于后面分支处理
            bool bIsRecPath = StringUtil.HasHead(strBarcodeOrRecPath, "@path:");

            string strItemText = "";
            string strBiblioText = "";

            // string strItemRecPath = "";
            string strBiblioRecPath = "";
            XmlDocument item_dom = null;
            string strBiblioSummary = "";
            string strISBnISSN = "";

            if (info == null)
            {
                byte[] item_timestamp = null;

            REDO_GETITEMINFO:
                lRet = Channel.GetItemInfo(
                    stop,
                    strBarcodeOrRecPath,
                    "xml",
                    out strItemText,
                    out strOutputItemRecPath,
                    out item_timestamp,
                    "recpath",
                    out strBiblioText,
                    out strBiblioRecPath,
                    out strError);
                if (lRet == -1)
                {
                    DialogResult temp_result = MessageBox.Show(this,
    strError + "\r\n\r\n是否重试?",
    "AccountBookForm",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO_GETITEMINFO;
                }
                if (lRet == -1 || lRet == 0)
                {
#if NO
                    if (item == null)
                    {
                        item = new ListViewItem(strBarcodeOrRecPath, 0);
                        list.Items.Add(item);
                    }
                    else
                    {
                        Debug.Assert(item.ListView == list, "");
                    }

                    // item.SubItems.Add(strError);
                    ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, strError);

                    SetItemColor(item, TYPE_ERROR);

                    // 将新加入的事项滚入视野
                    list.EnsureVisible(list.Items.IndexOf(item));
#endif
                    SetError(list,
                        ref item,
                        strBarcodeOrRecPath,
                        strError);
                    goto ERROR1;
                }

                SummaryInfo summary = (SummaryInfo)this.m_summaryTable[strBiblioRecPath];
                if (summary != null)
                {
                    strBiblioSummary = summary.Summary;
                    strISBnISSN = summary.ISBnISSn;
                }

                if (strBiblioSummary == ""
                    && this.checkBox_load_fillBiblioSummary.Checked == true)
                {
                    string[] formats = new string[2];
                    formats[0] = "summary";
                    formats[1] = "@isbnissn";
                    string[] results = null;
                    byte[] timestamp = null;

                    stop.SetMessage("正在装入书目记录 '" + strBiblioRecPath + "' 的摘要 ...");

                    Debug.Assert(String.IsNullOrEmpty(strBiblioRecPath) == false, "strBiblioRecPath值不能为空");
                REDO_GETBIBLIOINFO:
                    lRet = Channel.GetBiblioInfos(
                        stop,
                        strBiblioRecPath,
                    "",
                        formats,
                        out results,
                        out timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        DialogResult temp_result = MessageBox.Show(this,
        strError + "\r\n\r\n是否重试?",
        "AccountBookForm",
        MessageBoxButtons.RetryCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_GETBIBLIOINFO;
                    }
                    if (lRet == -1 || lRet == 0)
                    {
                        if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                            strError = "书目记录 '" + strBiblioRecPath + "' 不存在";

                        strBiblioSummary = "获得书目摘要时发生错误: " + strError;
                    }
                    else
                    {
                        Debug.Assert(results != null && results.Length == 2, "results必须包含2个元素");
                        strBiblioSummary = results[0];
                        strISBnISSN = results[1];

                        // 避免cache占据的内存太多
                        if (this.m_summaryTable.Count > 1000)
                            this.m_summaryTable.Clear();

                        if (summary == null)
                        {
                            summary = new SummaryInfo();
                            summary.Summary = strBiblioSummary;
                            summary.ISBnISSn = strISBnISSN;
                            this.m_summaryTable[strBiblioRecPath] = summary;
                        }
                    }
                }

                // 剖析一个册的xml记录，取出有关信息放入listview中
                if (item_dom == null)
                {
                    item_dom = new XmlDocument();
                    try
                    {
                        item_dom.LoadXml(strItemText);
                    }
                    catch (Exception ex)
                    {
                        strError = "册记录的XML装入DOM时出错: " + ex.Message;
                        goto ERROR1;
                    }
                }

            }
            else
            {
                // record 不为空调用时，对调用时参数strBarcodeOrRecPath不作要求

                strBarcodeOrRecPath = "@path:" + info.Record.Path;
                bIsRecPath = true;

                if (info.Record.RecordBody.Result.ErrorCode != ErrorCodeValue.NoError)
                {
#if NO
                    if (item == null)
                        item = new ListViewItem(strBarcodeOrRecPath, 0);


                    item.SubItems.Add(info.Record.RecordBody.Result.ErrorString);

                    SetItemColor(item, TYPE_ERROR);
                    list.Items.Add(item);

                    // 将新加入的事项滚入视野
                    list.EnsureVisible(list.Items.Count - 1);
#endif
                    SetError(list,
    ref item,
    strBarcodeOrRecPath,
    info.Record.RecordBody.Result.ErrorString);
                    goto ERROR1;
                }

                strItemText = info.Record.RecordBody.Xml;
                strOutputItemRecPath = info.Record.Path;

                //
                item_dom = info.Dom;
                strBiblioRecPath = info.BiblioRecPath;
                if (info.SummaryInfo != null)
                {
                    strBiblioSummary = info.SummaryInfo.Summary;
                    strISBnISSN = info.SummaryInfo.ISBnISSn;
                }
            }


            // 附加的馆藏地点匹配
            if (strMatchLocation != null)
            {
                // TODO: #reservation, 情况如何处理?
                string strLocation = DomUtil.GetElementText(item_dom.DocumentElement,
                    "location");

                // 2013/3/26
                if (strLocation == null)
                    strLocation = "";

                if (strMatchLocation != strLocation)
                    return 0;
            }

            if (item == null)
            {
                item = AddToListView(list,
                    item_dom,
                    strOutputItemRecPath,
                    strBiblioSummary,
                    strISBnISSN,
                    strBiblioRecPath);

                // 图标
                // item.ImageIndex = TYPE_NORMAL;
                SetItemColor(item, TYPE_NORMAL);

                // 将新加入的事项滚入视野
                list.EnsureVisible(list.Items.Count - 1);

#if NO
                // 填充需要从订购库获得的栏目信息
                if (this.checkBox_load_fillOrderInfo.Checked == true)
                    FillOrderColumns(item, strPubType);
#endif
            }
            else
            {
                SetListViewItemText(item_dom,
    true,
    strOutputItemRecPath,
    strBiblioSummary,
    strISBnISSN,
    strBiblioRecPath,
    item);
                SetItemColor(item, TYPE_NORMAL);
            }

            return 1;
        ERROR1:
            return -1;
        }
#endif

        // 获得新旧值的新部分
        static string GetNewPart(string strValue)
        {
            string strOldValue = "";
            string strNewValue = "";

            // 分离 "old[new]" 内的两个值
            OrderDesignControl.ParseOldNewValue(strValue,
                out strOldValue,
                out strNewValue);

            return strNewValue;
        }

        // 以前的版本
        // 填充需要从订购库获得的栏目信息
        void FillOrderColumns(ListViewItem item,
            string strPubType)
        {
            string strRefID = ListViewUtil.GetItemText(item, COLUMN_REFID);
            if (String.IsNullOrEmpty(strRefID) == true)
                return;

            bool bSeries = false;
            if (strPubType == "连续出版物")
                bSeries = true;


            string strError = "";
            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            string strOrderOrIssueRecPath = "";

            // 图书
            if (bSeries == false)
            {
                // 获得所连接的一条订购记录(的路径)
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetLinkedOrderRecordRecPath(strRefID,
                out strOrderOrIssueRecPath,
                out strError);
                if (nRet != 1)
                {
                    goto ERROR1;
                }

                string strItemXml = "";
                // 根据记录路径获得一条订购记录
                nRet = GetOrderRecord(strOrderOrIssueRecPath,
                    out strItemXml,
                    out strError);
                if (nRet != 1)
                {
                    goto ERROR1;
                }

                try
                {
                    dom.LoadXml(strItemXml);
                }
                catch (Exception ex)
                {
                    strError = "订购记录XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }
            }
            else
            {
                // 期刊

                string strPublishTime = ListViewUtil.GetItemText(item, MERGED_COLUMN_PUBLISHTIME);

                if (String.IsNullOrEmpty(strPublishTime) == true)
                {
                    strError = "出版日期为空，无法定位期记录";
                    goto ERROR1;
                }

                // 获得所连接的一条期记录(的路径)
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetLinkedIssueRecordRecPath(strRefID,
                    strPublishTime,
                    out strOrderOrIssueRecPath,
                    out strError);
                if (nRet != 1)
                {
                    goto ERROR1;
                }

                string strItemXml = "";
                // 根据记录路径获得一条期记录
                nRet = GetIssueRecord(strOrderOrIssueRecPath,
                    out strItemXml,
                    out strError);
                if (nRet != 1)
                {
                    goto ERROR1;
                }

                try
                {
                    dom.LoadXml(strItemXml);
                }
                catch (Exception ex)
                {
                    strError = "期记录XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                // 要通过refid定位到具体的一个订购xml片段

                string strOrderXml = "";
                // 从期记录中获得和一个refid有关的订购记录片段
                // parameters:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetSubOrderRecord(dom,
                    strRefID,
                    out strOrderXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "从期记录中获得包含 '" + strRefID + "' 的订购记录片段时出错: " + strError;
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    strError = "从期记录中没有找到包含 '" + strRefID + "' 的订购记录片段";
                    goto ERROR1;
                }

                try
                {
                    dom.LoadXml(strOrderXml);
                }
                catch (Exception ex)
                {
                    strError = "(从期记录中)获得的订购片断XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }
            }

            string strCatalogNo = DomUtil.GetElementText(dom.DocumentElement,
                "catalogNo");
            string strOrderID = DomUtil.GetElementText(dom.DocumentElement,
                "orderID");
            string strClass = DomUtil.GetElementText(dom.DocumentElement,
                "class");
            string strOrderTime = DomUtil.GetElementText(dom.DocumentElement,
                "orderTime");
            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            // 2009/7/24 
            string strSeller = DomUtil.GetElementText(dom.DocumentElement,
                "seller");
            string strSource = DomUtil.GetElementText(dom.DocumentElement,
                "source");
            strSource = GetNewPart(strSource);  // 只需要新的值

#if NO
            // 检查total price是否正确
            {
                string strCurrentOldPrice = "";
                string strCurrentNewPrice = "";

                // 分离 "old[new]" 内的两个值
                OrderDesignControl.ParseOldNewValue(strPrice,
                    out strCurrentOldPrice,
                    out strCurrentNewPrice);

                strPrice = strCurrentOldPrice;  // 原始订购价
            }
#endif
            string strOrderPrice = "";  // 订购记录中的订购价
            string strAcceptPrice = "";    // 订购记录中的到书价

            {
                string strCurrentOldPrice = "";
                string strCurrentNewPrice = "";

                // 分离 "old[new]" 内的两个值
                OrderDesignControl.ParseOldNewValue(strPrice,
                    out strCurrentOldPrice,
                    out strCurrentNewPrice);

                strOrderPrice = strCurrentOldPrice;  // 订购记录中的订购价
                strAcceptPrice = strCurrentNewPrice;    // 订购记录中的到书价
            }

            try
            {
                strOrderTime = DateTimeUtil.LocalTime(strOrderTime);
            }
            catch (Exception ex)
            {
                strOrderTime = "时间字符串 '" + strOrderTime + "' 格式错误: " + ex.Message;
            }

            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_CATALOGNO, strCatalogNo);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_ORDERID, strOrderID);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_CLASS, strClass);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_ORDERTIME, strOrderTime);

            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_ORDERPRICE, strOrderPrice);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_ACCEPTPRICE, strAcceptPrice);

            // ListViewUtil.ChangeItemText(item, MERGED_COLUMN_ORDERRECPATH, strOrderOrIssueRecPath);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_SELLER, strSeller);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_SOURCE, strSource);

            string strDistribute = DomUtil.GetElementText(dom.DocumentElement,
                "distribute");
            if (String.IsNullOrEmpty(strDistribute) == false)
            {
                // 增补refid -- 记录路径对照关系
                List<string> refids = PrintAcceptForm.GetLocationRefIDs(strDistribute);

                for (int i = 0; i < refids.Count; i++)
                {
                    string strCurrentRefID = refids[i];
                    this.refid_table[strCurrentRefID] = strOrderOrIssueRecPath;
                }
            }

            return;
        ERROR1:
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_CATALOGNO, strError);
        }


        // 填充需要从订购库获得的栏目信息
        void FillOrderColumns(OrderInfo info,
            string strPubType)
        {
            string strError = "";
            int nRet = 0;

            Debug.Assert(string.IsNullOrEmpty(info.OrderXml) == false, "");

            ListViewItem item = info.ListViewItem;

            bool bSeries = false;
            if (strPubType == "连续出版物")
                bSeries = true;

            XmlDocument dom = new XmlDocument();
            string strOrderOrIssueRecPath = "";

            // 图书
            if (bSeries == false)
            {
                try
                {
                    dom.LoadXml(info.OrderXml);
                }
                catch (Exception ex)
                {
                    strError = "订购记录XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }
            }
            else
            {
                try
                {
                    dom.LoadXml(info.OrderXml);
                }
                catch (Exception ex)
                {
                    strError = "期记录XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                string strOrderXml = "";
                // 从期记录中获得和一个refid有关的订购记录片段
                // parameters:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetSubOrderRecord(dom,
                    info.ItemRefID,
                    out strOrderXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "从期记录 " + info.OrderRecPath + " 中获得包含 '" + info.ItemRefID + "' 的订购记录片段时出错: " + strError;
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    strError = "从期记录 " + info.OrderRecPath + " 中没有找到包含 '" + info.ItemRefID + "' 的订购记录片段";
                    goto ERROR1;
                }

                try
                {
                    dom.LoadXml(strOrderXml);
                }
                catch (Exception ex)
                {
                    strError = "(从期记录 " + info.OrderRecPath + " 中)获得的订购片断XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }
            }

            string strCatalogNo = DomUtil.GetElementText(dom.DocumentElement,
                "catalogNo");
            string strOrderID = DomUtil.GetElementText(dom.DocumentElement,
                "orderID");
            string strClass = DomUtil.GetElementText(dom.DocumentElement,
                "class");
            string strOrderTime = DomUtil.GetElementText(dom.DocumentElement,
                "orderTime");
            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            // 2009/7/24 
            string strSeller = DomUtil.GetElementText(dom.DocumentElement,
                "seller");
            string strSource = DomUtil.GetElementText(dom.DocumentElement,
                "source");
            strSource = GetNewPart(strSource);  // 只需要新的值

#if NO
            // 检查total price是否正确
            {
                string strCurrentOldPrice = "";
                string strCurrentNewPrice = "";

                // 分离 "old[new]" 内的两个值
                OrderDesignControl.ParseOldNewValue(strPrice,
                    out strCurrentOldPrice,
                    out strCurrentNewPrice);

                strPrice = strCurrentOldPrice;  // 原始订购价
            }
#endif
            string strOrderPrice = "";  // 订购记录中的订购价
            string strAcceptPrice = "";    // 订购记录中的到书价

            {
                string strCurrentOldPrice = "";
                string strCurrentNewPrice = "";

                // 分离 "old[new]" 内的两个值
                OrderDesignControl.ParseOldNewValue(strPrice,
                    out strCurrentOldPrice,
                    out strCurrentNewPrice);

                strOrderPrice = strCurrentOldPrice;  // 订购记录中的订购价
                strAcceptPrice = strCurrentNewPrice;    // 订购记录中的到书价
            }


            try
            {
                strOrderTime = DateTimeUtil.LocalTime(strOrderTime);
            }
            catch (Exception ex)
            {
                strOrderTime = "时间字符串 '" + strOrderTime + "' 格式错误: " + ex.Message;
            }

            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_CATALOGNO, strCatalogNo);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_ORDERID, strOrderID);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_CLASS, strClass);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_ORDERTIME, strOrderTime);

            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_ORDERPRICE, strOrderPrice);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_ACCEPTPRICE, strAcceptPrice);

            // ListViewUtil.ChangeItemText(item, MERGED_COLUMN_ORDERRECPATH, strOrderOrIssueRecPath);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_SELLER, strSeller);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_SOURCE, strSource);

            string strDistribute = DomUtil.GetElementText(dom.DocumentElement,
                "distribute");
            if (String.IsNullOrEmpty(strDistribute) == false)
            {
                // 增补refid -- 记录路径对照关系
                List<string> refids = PrintAcceptForm.GetLocationRefIDs(strDistribute);

                for (int i = 0; i < refids.Count; i++)
                {
                    string strCurrentRefID = refids[i];
                    this.refid_table[strCurrentRefID] = strOrderOrIssueRecPath;
                }
            }

            return;
        ERROR1:
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_CATALOGNO, strError);
        }


        // 从期记录中获得和一个refid有关的订购记录片段
        // parameters:
        //      -1  error
        //      0   not found
        //      1   found
        static int GetSubOrderRecord(XmlDocument dom,
            string strRefID,
            out string strOrderXml,
            out string strError)
        {
            strError = "";
            strOrderXml = "";

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("orderInfo/*/distribute");
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
                    DigitalPlatform.Location location = locations[j];

                    if (location.RefID == strRefID)
                    {
                        strOrderXml = node.ParentNode.OuterXml;
                        return 1;
                    }
                }
            }

            return 0;
        }

        // 2009/2/2
        // 获得所连接的一条期记录(的路径)
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetLinkedIssueRecordRecPath(string strRefID,
            string strPublishTime,
            out string strIssueRecPath,
            out string strError)
        {
            strError = "";
            strIssueRecPath = "";

            // 先从cache中找
            strIssueRecPath = (string)this.refid_table[strRefID];
            if (String.IsNullOrEmpty(strIssueRecPath) == false)
                return 1;

            long lRet = Channel.SearchIssue(
                stop,
                "<all>",
                strRefID,
                -1,
                "册参考ID",
                "exact",
                this.Lang,
                "refid",   // strResultSetName
                "",    // strSearchStyle
                "", // strOutputStyle
                out strError);
            if (lRet == -1)
                return -1;

            if (lRet == 0)
            {
                strError = "参考ID '" + strRefID + "' 没有命中任何期记录";
                return 0;
            }

            if (lRet > 1)
            {
                strError = "参考ID '" + strRefID + "' 命中多条(" + lRet.ToString() + ")订购记录";
                return -1;
            }

            long lHitCount = lRet;

            DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

            // 装入浏览格式

            lRet = Channel.GetSearchResult(
                stop,
                "refid",   // strResultSetName
                0,
                lHitCount,
                "id",   // "id,cols",
                this.Lang,
                out searchresults,
                out strError);
            if (lRet == -1)
            {
                strError = "GetSearchResult() error: " + strError;
                return -1;
            }

            if (lRet == 0)
            {
                strError = "GetSearchResult() error : 未命中";
                return -1;
            }

            strIssueRecPath = searchresults[0].Path;

            // 加入cache
            if (this.refid_table.Count > 1000)
                this.refid_table.Clear();   // 限制hashtable的最大尺寸
            this.refid_table[strRefID] = strIssueRecPath;

            return (int)lHitCount;
        }

        // 2009/2/2
        // 根据记录路径获得一条期刊记录
        int GetIssueRecord(string strRecPath,
            out string strItemXml,
            out string strError)
        {
            strError = "";
            strItemXml = "";

            // 先从cache中找
            strItemXml = (string)this.orderxml_table[strRecPath];
            if (String.IsNullOrEmpty(strItemXml) == false)
                return 1;


            string strBiblioText = "";

            string strItemRecPath = "";
            string strBiblioRecPath = "";

            byte[] item_timestamp = null;

            string strIndex = "@path:" + strRecPath;

            long lRet = Channel.GetIssueInfo(
                stop,
                strIndex,   // strPublishTime
                // "", // strBiblioRecPath
                "xml",
                out strItemXml,
                out strItemRecPath,
                out item_timestamp,
                "recpath",
                out strBiblioText,
                out strBiblioRecPath,
                out strError);
            if (lRet == -1 || lRet == 0)
                return (int)lRet;

            // 加入cache
            if (this.orderxml_table.Count > 500)
                this.orderxml_table.Clear();   // 限制hashtable的最大尺寸
            this.orderxml_table[strRecPath] = strItemXml;

            return 1;
        }

        // 获得所连接的一条订购记录(的路径)
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetLinkedOrderRecordRecPath(string strRefID,
            out string strOrderRecPath,
            out string strError)
        {
            strError = "";
            strOrderRecPath = "";

            // 先从cache中找
            strOrderRecPath = (string)this.refid_table[strRefID];
            if (String.IsNullOrEmpty(strOrderRecPath) == false)
                return 1;

            long lRet = Channel.SearchOrder(
                stop,
                "<all>",
                strRefID,
                -1,
                "册参考ID",
                "exact",
                this.Lang,
                "refid",   // strResultSetName
                "",    // strSearchStyle
                "", // strOutputStyle
                out strError);
            if (lRet == -1)
                return -1;

            if (lRet == 0)
            {
                strError = "参考ID '" + strRefID + "' 没有命中任何订购记录";
                return 0;
            }

            if (lRet > 1)
            {
                strError = "参考ID '" + strRefID + "' 命中多条(" + lRet.ToString() + ")订购记录";
                return -1;
            }

            long lHitCount = lRet;

            DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

            // 装入浏览格式

            lRet = Channel.GetSearchResult(
                stop,
                "refid",   // strResultSetName
                0,
                lHitCount,
                "id",   // "id,cols",
                this.Lang,
                out searchresults,
                out strError);
            if (lRet == -1)
            {
                strError = "GetSearchResult() error: " + strError;
                return -1;
            }

            if (lRet == 0)
            {
                strError = "GetSearchResult() error : 未命中";
                return -1;
            }

            strOrderRecPath = searchresults[0].Path;

            // 加入cache
            if (this.refid_table.Count > 1000)
                this.refid_table.Clear();   // 限制hashtable的最大尺寸
            this.refid_table[strRefID] = strOrderRecPath;

            return (int)lHitCount;
        }

        // 根据记录路径获得一条订购记录
        int GetOrderRecord(string strRecPath,
            out string strItemXml,
            out string strError)
        {
            strError = "";
            strItemXml = "";

            // 先从cache中找
            strItemXml = (string)this.orderxml_table[strRecPath];
            if (String.IsNullOrEmpty(strItemXml) == false)
                return 1;


            string strBiblioText = "";

            string strItemRecPath = "";
            string strBiblioRecPath = "";

            byte[] item_timestamp = null;

            string strIndex = "@path:" + strRecPath;

            long lRet = Channel.GetOrderInfo(
                stop,
                strIndex,
                // "",
                "xml",
                out strItemXml,
                out strItemRecPath,
                out item_timestamp,
                "recpath",
                out strBiblioText,
                out strBiblioRecPath,
                out strError);
            if (lRet == -1 || lRet == 0)
                return (int)lRet;

            // 加入cache
            if (this.orderxml_table.Count > 500)
                this.orderxml_table.Clear();   // 限制hashtable的最大尺寸
            this.orderxml_table[strRecPath] = strItemXml;

            return 1;
        }

        // 设置事项的背景、前景颜色，和图标
        static void SetItemColor(ListViewItem item,
            int nType)
        {
            if (nType == TYPE_ERROR)
            {
                item.BackColor = System.Drawing.Color.Red;
                item.ForeColor = System.Drawing.Color.White;
                item.ImageIndex = TYPE_ERROR;
            }
            else if (nType == TYPE_CHECKED)
            {
                item.BackColor = System.Drawing.Color.Green;
                item.ForeColor = System.Drawing.Color.White;
                item.ImageIndex = TYPE_CHECKED;
            }
            else if (nType == TYPE_NORMAL)
            {
                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;
                item.ImageIndex = TYPE_NORMAL;
            }
            else
            {
                Debug.Assert(false, "未知的image type");
            }

        }

        // 根据册记录 DOM 设置 ListViewItem 除第一列以外的文字
        // parameters:
        //      bSetBarcodeColumn   是否要设置条码列内容(第一列)
        internal override void SetListViewItemText(XmlDocument dom,
            byte[] baTimestamp,
            bool bSetBarcodeColumn,
            string strRecPath,
            string strBiblioRecPath,
            string[] summary_col_names,
            SummaryInfo summary,
            ListViewItem item)
        {
            string strBiblioSummary = "";
            string strISBnISSN = "";

            if (summary != null && summary.Values != null)
            {
                if (summary.Values.Length > 0)
                    strBiblioSummary = summary.Values[0];
                if (summary.Values.Length > 1)
                    strISBnISSN = summary.Values[1];
            }

            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");
            string strPublishTime = DomUtil.GetElementText(dom.DocumentElement,
                "publishTime");
            string strVolume = DomUtil.GetElementText(dom.DocumentElement,
                "volume");


            string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");
            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            string strBookType = DomUtil.GetElementText(dom.DocumentElement,
                "bookType");
            string strRegisterNo = DomUtil.GetElementText(dom.DocumentElement,
                "registerNo");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");
            string strMergeComment = DomUtil.GetElementText(dom.DocumentElement,
                "mergeComment");
            string strBatchNo = DomUtil.GetElementText(dom.DocumentElement,
                "batchNo");
            string strAccessNo = DomUtil.GetElementText(dom.DocumentElement,
                "accessNo");
            string strRefID = DomUtil.GetElementText(dom.DocumentElement,
                "refID");
            /*
            string strBorrower = DomUtil.GetElementText(dom.DocumentElement,
                "borrower");
            string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement,
                "borrowDate");
            // 2007/6/20 
            strBorrowDate = DateTimeUtil.LocalTime(strBorrowDate, "u");
            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement,
                "borrowPeriod");
             * */
            // 2011/6/13
            string strSeller = DomUtil.GetElementText(dom.DocumentElement,
                "seller");
            string strSource = DomUtil.GetElementText(dom.DocumentElement,
                "source");

            ListViewUtil.ChangeItemText(item, COLUMN_SUMMARY, strBiblioSummary);

            ListViewUtil.ChangeItemText(item, COLUMN_STATE, strState);
            ListViewUtil.ChangeItemText(item, COLUMN_ACCESSNO, strAccessNo);
            ListViewUtil.ChangeItemText(item, MERGED_COLUMN_PUBLISHTIME, strPublishTime);
            ListViewUtil.ChangeItemText(item, MERGED_COLUMN_VOLUME, strVolume);

            ListViewUtil.ChangeItemText(item, COLUMN_ISBNISSN, strISBnISSN);


            ListViewUtil.ChangeItemText(item, COLUMN_LOCATION, strLocation);
            ListViewUtil.ChangeItemText(item, COLUMN_PRICE, strPrice);
            ListViewUtil.ChangeItemText(item, COLUMN_BOOKTYPE, strBookType);
            ListViewUtil.ChangeItemText(item, COLUMN_REGISTERNO, strRegisterNo);
            ListViewUtil.ChangeItemText(item, COLUMN_COMMENT, strComment);
            ListViewUtil.ChangeItemText(item, COLUMN_MERGECOMMENT, strMergeComment);
            ListViewUtil.ChangeItemText(item, COLUMN_BATCHNO, strBatchNo);

            /*
            ListViewUtil.ChangeItemText(item, COLUMN_BORROWER, strBorrower);
            ListViewUtil.ChangeItemText(item, COLUMN_BORROWDATE, strBorrowDate);
            ListViewUtil.ChangeItemText(item, COLUMN_BORROWPERIOD, strBorrowPeriod);
             * */

            ListViewUtil.ChangeItemText(item, COLUMN_RECPATH, strRecPath);
            ListViewUtil.ChangeItemText(item, COLUMN_BIBLIORECPATH, strBiblioRecPath);
            ListViewUtil.ChangeItemText(item, COLUMN_REFID, strRefID);

            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_SELLER, strSeller);
            ListViewUtil.ChangeItemText(item, EXTEND_COLUMN_SOURCE, strSource);


            if (bSetBarcodeColumn == true)
            {
                string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
                    "barcode");
                ListViewUtil.ChangeItemText(item, COLUMN_BARCODE, strBarcode);
            }

            SetItemColor(item, TYPE_NORMAL);
        }

        internal override ListViewItem AddToListView(ListView list,
            XmlDocument dom,
            byte[] baTimestamp,
            string strRecPath,
            string strBiblioRecPath,
            string[] summary_col_names,
            SummaryInfo summary)
        {
            string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");

            ListViewItem item = new ListViewItem(strBarcode, 0);

            SetListViewItemText(dom,
                baTimestamp,
                false,
                strRecPath,
                strBiblioRecPath,
                summary_col_names,
                summary,
                item);
            list.Items.Add(item);
            // 图标
            // item.ImageIndex = TYPE_NORMAL;
            // SetItemColor(item, TYPE_NORMAL);

            return item;
        }

        void SetNextButtonEnable()
        {
            string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_load)
            {
                // 汇报数据装载情况。
                // return:
                //      0   尚未装载任何数据    
                //      1   装载已经完成
                //      2   虽然装载了数据，但是其中有错误事项
                int nState = ReportLoadState(out strError);

                if (nState != 1)
                {
                    this.button_next.Enabled = false;
                }
                else
                    this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_sort)
            {
                this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
                this.button_next.Enabled = false;
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }
        }

        // 下一步
        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_load)
            {
                this.tabControl_main.SelectedTab = this.tabPage_sort;
                this.button_next.Enabled = true;
                this.comboBox_sort_sortStyle.Focus();
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_sort)
            {
                // 进行排序
                // return:
                //      -1  出错
                //      0   没有必要排序
                //      1   已完成排序
                int nRet = DoSort(this.comboBox_sort_sortStyle.Text,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.tabControl_main.SelectedTab = this.tabPage_print;
                this.button_next.Enabled = false;
                this.button_print_printNormalList.Focus();
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }

            this.SetNextButtonEnable();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 汇报数据装载情况。
        // return:
        //      0   尚未装载任何数据    
        //      1   装载已经完成
        //      2   虽然装载了数据，但是其中有错误事项
        int ReportLoadState(out string strError)
        {
            strError = "";

            int nRedCount = 0;
            int nWhiteCount = 0;

            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {
                ListViewItem item = this.listView_in.Items[i];

                if (item.ImageIndex == TYPE_ERROR)
                    nRedCount++;
                else
                    nWhiteCount++;
            }

            if (nRedCount != 0)
            {
                strError = "列表中有 " + nRedCount + " 个错误事项(红色行)。请修改数据后重新装载。";
                return 2;
            }

            if (nWhiteCount == 0)
            {
                strError = "尚未装载数据事项。";
                return 0;
            }

            strError = "数据事项装载正确。";
            return 1;
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_load)
            {
                this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_sort)
            {
                this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
                this.button_next.Enabled = false;
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }
        }


        // 计算绿色事项的数目
        int GetGreenItemCount()
        {
            int nCount = 0;
            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {
                if (this.listView_in.Items[i].ImageIndex == TYPE_CHECKED)
                    nCount++;
            }
            return nCount;
        }

        // 查找条码匹配的ListViewItem事项
        static ListViewItem FindItem(ListView list,
            string strBarcode)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];
                if (strBarcode == item.Text)
                    return item;
            }

            return null;    // not found
        }

        // 打印全部事项清单
        private void button_print_printNormalList_Click(object sender, EventArgs e)
        {
            // string strError = "";

            EnableControls(false);

            try
            {

                int nErrorCount = 0;
                int nUncheckedCount = 0;

                List<ListViewItem> items = new List<ListViewItem>();
                for (int i = 0; i < this.listView_in.Items.Count; i++)
                {
                    ListViewItem item = this.listView_in.Items[i];

                    items.Add(item);

                    if (item.ImageIndex == TYPE_ERROR)
                        nErrorCount++;
                    if (item.ImageIndex == TYPE_NORMAL)
                        nUncheckedCount++;
                }

                PrintList(this.comboBox_load_type.Text + " 全部事项清单", items);
                return;

            }
            finally
            {
                EnableControls(true);
            }
            /*
        ERROR1:
            MessageBox.Show(this, strError);
             * */
        }

        private static Stylesheet GenerateStyleSheet()
        {
            return new Stylesheet(
                new Fonts(
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 0 - The default font.
                        new FontSize() { Val = 11 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 1 - The bold font.
                        new Bold(),
                        new FontSize() { Val = 11 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 2 - The Italic font.
                        new Italic(),
                        new FontSize() { Val = 11 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 2 - The Times Roman font. with 16 size
                        new FontSize() { Val = 16 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Times New Roman" })
                ),
                new Fills(
                    new Fill(                                                           // Index 0 - The default fill.
                        new PatternFill() { PatternType = PatternValues.None }),
                    new Fill(                                                           // Index 1 - The default fill of gray 125 (required)
                        new PatternFill() { PatternType = PatternValues.Gray125 }),
                    new Fill(                                                           // Index 2 - The yellow fill.
                        new PatternFill(
                            new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "FFFFFF00" } }
                        ) { PatternType = PatternValues.Solid })
                ),
                new Borders(
                    new Border(                                                         // Index 0 - The default border.
                        new LeftBorder(),
                        new RightBorder(),
                        new TopBorder(),
                        new BottomBorder(),
                        new DiagonalBorder()),
                    new Border(                                                         // Index 1 - Applies a Left, Right, Top, Bottom border to a cell
                        new LeftBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new RightBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new TopBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new BottomBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder())
                ),
                new CellFormats(
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 },                          // Index 0 - The default cell style.  If a cell does not have a style index applied it will use this style combination instead
                    new CellFormat() { FontId = 1, FillId = 0, BorderId = 0, ApplyFont = true },       // Index 1 - Bold 
                    new CellFormat() { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true },       // Index 2 - Italic
                    new CellFormat() { FontId = 3, FillId = 0, BorderId = 0, ApplyFont = true },       // Index 3 - Times Roman
                    new CellFormat() { FontId = 0, FillId = 2, BorderId = 0, ApplyFill = true },       // Index 4 - Yellow Fill
                // 5 textwrap
                    new CellFormat(                                                                   // Index 5 - Alignment
                        new Alignment() { Vertical = VerticalAlignmentValues.Center, WrapText = BooleanValue.FromBoolean(true) }
                    ) { /*FontId = 1, FillId = 0, BorderId = 0, */ApplyAlignment = true },

                    // 6 align center
                    new CellFormat(
                        new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }
                    ) { ApplyAlignment = true },


                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true }      // Index 6 - Border
                )
            ); // return
        }

        string ExportExcelFilename = "";

        void PrintList(
            string strTitle,
            List<ListViewItem> items)
        {
            string strError = "";

            // 创建一个html文件，并显示在HtmlPrintForm中。

            List<string> filenames = null;
            try
            {
                // Debug.Assert(false, "");

                // 构造html页面
                int nRet = BuildHtml(
                    items,
                    out filenames,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                HtmlPrintForm printform = new HtmlPrintForm();

                printform.Text = "打印" + strTitle;
                printform.MainForm = this.MainForm;
                printform.Filenames = filenames;

                this.MainForm.AppInfo.LinkFormState(printform, "printform_state");
                printform.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(printform);
            }
            finally
            {
                if (filenames != null)
                    Global.DeleteFiles(filenames);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void WriteWordXmlHead(XmlTextWriter writer)
        {
            writer.WriteStartDocument();

            // <?mso-application progid="Word.Document"?>
            writer.WriteProcessingInstruction("mso-application",
                "progid=\"Word.Document\"");

            // <w:wordDocument>
            writer.WriteStartElement("w", "wordDocument", m_strWordMlNsUri);

            writer.WriteAttributeString(
                "xmlns",
                "v",
                null,
                "urn:schemas-microsoft-com:vml");

            writer.WriteAttributeString(
    "xmlns",
    "w10",
    null,
    "urn:schemas-microsoft-com:office:word");

            writer.WriteAttributeString(
"xmlns",
"sl",
null,
"http://schemas.microsoft.com/schemaLibrary/2003/core");

            writer.WriteAttributeString(
"xmlns",
"aml",
null,
"http://schemas.microsoft.com/aml/2001/core");


            writer.WriteAttributeString(
                "xmlns",
                "wx",
                null,
                m_strWxUri);

            writer.WriteAttributeString(
"xmlns",
"o",
null,
"urn:schemas-microsoft-com:office:office");

            // <w:body>
            writer.WriteStartElement("w", "body", m_strWordMlNsUri);

            // <wx:sect>
            writer.WriteStartElement("wx", "sect", m_strWxUri);
        }

        void WriteWordXmlTail(XmlTextWriter writer)
        {
            // <wx:sect>
            writer.WriteEndElement();

            // <w:body>
            writer.WriteEndElement();

            // <w:wordDocument>
            writer.WriteEndElement();

            writer.WriteEndDocument();
        }

        // 关于来源的描述。
        // 如果为"batchno"方式，则为批次号；如果为"barcodefile"方式，则为条码号文件名(纯文件名); 如果为"recpathfile"方式，则为记录路径文件名(纯文件名)
        /// <summary>
        /// 关于来源的描述。
        /// 如果为"batchno"方式，则为批次号；如果为"barcodefile"方式，则为条码号文件名(纯文件名); 如果为"recpathfile"方式，则为记录路径文件名(纯文件名)
        /// </summary>
        public string SourceDescription
        {
            get
            {
                if (this.SourceStyle == "batchno")
                {
                    string strText = "";

                    if (String.IsNullOrEmpty(this.BatchNo) == false)
                        strText += "批次号 " + this.BatchNo;

                    if (String.IsNullOrEmpty(this.LocationString) == false
                        && this.LocationString != "<不指定>")
                    {
                        if (String.IsNullOrEmpty(strText) == false)
                            strText += "; ";
                        strText += "馆藏地 " + this.LocationString;
                    }

                    return this.BatchNo;
                }
                else if (this.SourceStyle == "barcodefile")
                {
                    return "条码号文件 " + Path.GetFileName(this.BarcodeFilePath);
                }
                else if (this.SourceStyle == "recpathfile")
                {
                    return "记录路径文件 " + Path.GetFileName(this.RecPathFilePath);
                }
                else
                {
                    Debug.Assert(this.SourceStyle == "", "");
                    return "";
                }
            }
        }

        // 输出到Word Xml文件
        int OutputToWordXmlFile(
            List<ListViewItem> items,
            XmlTextWriter writer,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            Hashtable macro_table = new Hashtable();

            string strNamePath = "accountbook_printoption_wordxml";

            // 获得打印参数
            PrintOption option = new AccountBookPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);

            // 检查当前排序状态和包含种价格列之间是否存在矛盾
            if (bHasBiblioPriceColumn(option) == true)
            {

                if (this.SortColumns_in.Count != 0
                    && this.SortColumns_in[0].No == COLUMN_BIBLIORECPATH)
                {
                }
                else
                {
                    MessageBox.Show(this, "由于当前打印用到了 “种价格”列，为保证打印结果的准确，程序自动按 ‘种记录路径’ 列对全部列表事项进行一次自动排序。\r\n\r\n为避免这里的自动排序，可在打印前用鼠标左键点栏标题进行符合自己意愿的排序，只要最后一次点的是‘种记录路径’栏标题即可。");
                    ForceSortColumnsIn(COLUMN_BIBLIORECPATH);
                }
            }

            // 2009/7/24 changed
            if (this.SourceStyle == "batchno")
            {
                macro_table["%batchno%"] = HttpUtility.HtmlEncode(this.BatchNo); // 批次号
                macro_table["%location%"] = HttpUtility.HtmlEncode(this.LocationString); // 馆藏地点 用HtmlEncode()的原因是要防止里面出现的“<不指定>”字样
            }
            else
            {
                Debug.Assert(this.SourceStyle == "barcodefile"
                    || this.SourceStyle == "recpathfile"
                    || this.SourceStyle == "",
                    "");

                macro_table["%batchno%"] = "";
                macro_table["%location%"] = "";
            }

            // macro_table["%pagecount%"] = nPageCount.ToString();
            // macro_table["%linesperpage%"] = option.LinesPerPage.ToString();
            macro_table["%date%"] = DateTime.Now.ToLongDateString();

            // 2009/7/24 changed
            if (this.SourceStyle == "barcodefile")
            {
                macro_table["%barcodefilepath%"] = this.BarcodeFilePath;
                macro_table["%barcodefilename%"] = Path.GetFileName(this.BarcodeFilePath);
            }
            else
            {
                Debug.Assert(this.SourceStyle == "batchno"
                    || this.SourceStyle == "recpathfile"
                    || this.SourceStyle == "",
                    "");

                macro_table["%barcodefilepath%"] = "";
                macro_table["%barcodefilename%"] = "";
            }

            // 2009/7/30 
            if (this.SourceStyle == "recpathfile")
            {
                macro_table["%recpathfilepath%"] = this.RecPathFilePath;
                macro_table["%recpathfilename%"] = Path.GetFileName(this.RecPathFilePath);
            }
            else
            {
                Debug.Assert(this.SourceStyle == "batchno"
                    || this.SourceStyle == "barcodefile"
                    || this.SourceStyle == "",
                    "");

                macro_table["%recpathfilepath%"] = "";
                macro_table["%recpathfilename%"] = "";
            }

            macro_table["%sourcedescription%"] = this.SourceDescription;

            WriteWordXmlHead(writer);

            // 输出统计信息页
            if (this.WordXmlOutputStatisPart == true)
            {
                int nItemCount = items.Count;
                int nBiblioCount = GetBiblioCount(items);
                string strTotalPrice = GetTotalPrice(items);

                macro_table["%itemcount%"] = nItemCount.ToString();
                macro_table["%bibliocount%"] = nBiblioCount.ToString();
                macro_table["%totalprice%"] = strTotalPrice;

                macro_table["%pageno%"] = "1";

                // 2008/11/23 
                macro_table["%datadir%"] = this.MainForm.DataDir;   // 便于引用datadir下templates目录内的某些文件
                ////macro_table["%libraryserverdir%"] = this.MainForm.LibraryServerDir;  // 便于引用服务器端的CSS文件

                string strTemplateFilePath = option.GetTemplatePageFilePath("统计页");
                if (String.IsNullOrEmpty(strTemplateFilePath) == false)
                {
                    /*
%date% 财产帐簿 -- %sourcedescription%
册数: %itemcount%
种数: %bibliocount%
总价: %totalprice%
------------
批次号: %batchno%
馆藏地点: %location%
条码号文件: %barcodefilepath%
记录路径文件: %recpathfilepath%
                     * * */

                    // 根据模板打印
                    string strContent = "";
                    // 能自动识别文件内容的编码方式的读入文本文件内容模块
                    // return:
                    //      -1  出错
                    //      0   文件不存在
                    //      1   文件存在
                    nRet = Global.ReadTextFileContent(strTemplateFilePath,
                        out strContent,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    string strResult = StringUtil.MacroString(macro_table,
                        strContent);
                    string[] lines = strResult.Split(new string[] { "\r\n" },
                        StringSplitOptions.None);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        WriteParagraph(writer, lines[i]);
                    }
                }
                else
                {
                    // 缺省的固定内容打印

                    /*
                    BuildPageTop(option,
                        macro_table,
                        strFileName,
                        false);
                     * */

                    // 内容行

                    WriteParagraph(writer, "册数\t" + nItemCount.ToString());
                    WriteParagraph(writer, "种数\t" + nBiblioCount.ToString());
                    WriteParagraph(writer, "总价\t" + strTotalPrice);

                    WriteParagraph(writer, "----------");


                    if (this.SourceStyle == "batchno")
                    {
                        // 2008/11/22 
                        if (String.IsNullOrEmpty(this.BatchNo) == false)
                        {
                            WriteParagraph(writer, "批次号\t" + this.BatchNo);
                        }
                        if (String.IsNullOrEmpty(this.LocationString) == false
                            && this.LocationString != "<不指定>")
                        {
                            WriteParagraph(writer, "馆藏地点\t" + this.LocationString);
                        }
                    }

                    if (this.SourceStyle == "barcodefile")
                    {
                        if (String.IsNullOrEmpty(this.BarcodeFilePath) == false)
                        {
                            WriteParagraph(writer, "条码号文件\t" + this.BarcodeFilePath);
                        }
                    }

                    // 2009/7/30 
                    if (this.SourceStyle == "recpathfile")
                    {
                        if (String.IsNullOrEmpty(this.RecPathFilePath) == false)
                        {
                            WriteParagraph(writer, "记录路径文件\t" + this.RecPathFilePath);
                        }
                    }

                    WriteParagraph(writer, "----------");
                    WriteParagraph(writer, "");
                }
            }

            string strMarcFilterFilePath = option.GetTemplatePageFilePath("MARC过滤器");
            if (String.IsNullOrEmpty(strMarcFilterFilePath) == false)
            {
#if NO
                ColumnFilterDocument filter = null;

                this.ColumnTable = new Hashtable();
                nRet = PrepareMarcFilter(strMarcFilterFilePath,
                    out filter,
                    out strError);
                if (nRet == -1)
                    return -1;

                //
                if (filter != null)
                    this.AssemblyFilter = filter.Assembly;
                else
                    this.AssemblyFilter = null;

                this.MarcFilter = filter;
#endif
                nRet = PrepareMarcFilter(strMarcFilterFilePath, out strError);
                if (nRet == -1)
                    return -1;
            }

            // 构造表格标题和标题行
            WriteTableBegin(writer,
                option,
                macro_table);

            // 表格行循环
            for (int i = 0; i < items.Count; i++)
            {
                BuildWordXmlTableLine(option,
                    items,
                    i,
                    writer,
                    this.WordXmlTruncate);
            }

            WriteTableEnd(writer);

            WriteWordXmlTail(writer);

            return 0;
        }

        int BuildWordXmlTableLine(PrintOption option,
            List<ListViewItem> items,
            int nIndex,
            XmlTextWriter writer,
            bool bCutText)
        {
            string strError = "";
            int nRet = 0;

            if (nIndex >= items.Count)
            {
                strError = "error: nIndex(" + nIndex.ToString() + ") >= items.Count(" + items.Count.ToString() + ")";
                goto ERROR1;
            }

            ListViewItem item = items[nIndex];
            string strMARC = "";
            string strOutMarcSyntax = "";

            this.ColumnTable.Clear();   // 清除上一记录处理时残余的内容

            if (this.MarcFilter != null
                || option.HasEvalue() == true)
            {

                // TODO: 有错误要明显报出来，否则容易在打印出来后才发现，就晚了

                // 获得MARC格式书目记录
                string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);

                if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                {
                    nRet = GetMarc(strBiblioRecPath,
                        out strMARC,
                        out strOutMarcSyntax,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (this.MarcFilter != null)
                    {
                        this.MarcFilter.Host.UiItem = item; // 当前正在处理的 ListViewItem

                        // 触发filter中的Record相关动作
                        nRet = this.MarcFilter.DoRecord(
                            null,
                            strMARC,
                            strOutMarcSyntax,
                            nIndex,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                }
            }

            // <w:tr>
            writer.WriteStartElement("w", "tr", m_strWordMlNsUri);

            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                // int nIndex = nPage * option.LinesPerPage + nLine;

                /*
                if (nIndex >= items.Count)
                    break;

                ListViewItem item = items[nIndex];
                 * */

                string strContent = "";
                if (string.IsNullOrEmpty(column.Evalue) == false)
                {
                    Jurassic.ScriptEngine engine = new Jurassic.ScriptEngine();
                    engine.EnableExposedClrTypes = true;
                    engine.SetGlobalValue("syntax", strOutMarcSyntax);
                    engine.SetGlobalValue("biblio", new MarcRecord(strMARC));
                    strContent = engine.Evaluate(column.Evalue).ToString();
                }
                else
                {
                    strContent = GetColumnContent(item,
                        column.Name);

                    if (strContent == "!!!#")
                    {
                        // strContent = ((nPage * option.LinesPerPage) + nLine + 1).ToString();
                        strContent = (nIndex + 1).ToString();
                    }

                    if (strContent == "!!!biblioPrice")
                    {
                        // 看看自己是不是处在切换边沿
                        string strCurLineBiblioRecPath = GetColumnContent(item, "biblioRecpath");

                        string strNextLineBiblioRecPath = "";

                        if (nIndex < items.Count - 1)
                        {
                            ListViewItem next_item = items[nIndex + 1];
                            strNextLineBiblioRecPath = GetColumnContent(next_item, "biblioRecpath");
                        }

                        if (strCurLineBiblioRecPath != strNextLineBiblioRecPath)
                        {
                            // 处在切换边沿

                            // 汇总前面的册价格
                            strContent = ComputeBiblioPrice(items, nIndex).ToString();
                            // bBiblioSumLine = true;
                        }
                        else
                        {
                            // 其他普通行
                            strContent = "";    //  "&nbsp;";
                        }
                    }
                }

                if (bCutText == true)
                {
                    // 截断字符串
                    if (column.MaxChars != -1)
                    {
                        if (strContent.Length > column.MaxChars)
                        {
                            strContent = strContent.Substring(0, column.MaxChars);
                            strContent += "...";
                        }
                    }
                }

                if (String.IsNullOrEmpty(strContent) == true)
                    strContent = "";    //  "&nbsp;";

                // string strClass = Global.GetLeft(column.Name);

                // <w:tc>
                writer.WriteStartElement("w", "tc", m_strWordMlNsUri);

                WriteParagraph(writer, strContent);

                // <w:tc>
                writer.WriteEndElement();
            }

            /*
            if (bBiblioSumLine == false)
            {
                StreamUtil.WriteText(strFileName,
        "<tr class='content'>");
            }
            else
            {
                StreamUtil.WriteText(strFileName,
        "<tr class='content_biblio_sum'>");
            }*/

            // <w:tr>
            writer.WriteEndElement();
            // sw.WriteLine(strLineContent);
            return 0;
        ERROR1:
            // <w:tr>
            writer.WriteStartElement("w", "tr", m_strWordMlNsUri);

            // <w:tc>
            writer.WriteStartElement("w", "tc", m_strWordMlNsUri);

            WriteParagraph(writer, strError);

            // <w:tc>
            writer.WriteEndElement();

            // <w:tr>
            writer.WriteEndElement();

            return -1;
        }

        void WriteBorderDef(XmlTextWriter writer,
            string strElementName,
            int nSz,
            int nBorderWidth)
        {
            // <w:top w:val="single" w:sz="10" wx:bdrwidth="10" w:space="0" w:color="auto" /> 
            writer.WriteStartElement("w", strElementName, m_strWordMlNsUri);

            // w:val="single" 单线条
            writer.WriteAttributeString("w", "val", m_strWordMlNsUri,
                "single");

            // w:sz="10"
            writer.WriteAttributeString("w", "sz", m_strWordMlNsUri,
                nSz.ToString());

            // wx:bdrwidth="10"
            writer.WriteAttributeString("wx", "bdrwidth", m_strWxUri,
                nBorderWidth.ToString());

            // w:color="auto"
            writer.WriteAttributeString("w", "color", m_strWordMlNsUri,
                "auto");

            // <w:top>
            writer.WriteEndElement();
        }

        void WriteMarginDef(XmlTextWriter writer,
            string strElementName,
            int nWidth)
        {
            // <w:left w:w="10" w:type="dxa" />
            writer.WriteStartElement("w", strElementName, m_strWordMlNsUri);

            // w:w="10"
            writer.WriteAttributeString("w", "w", m_strWordMlNsUri,
                nWidth.ToString());

            // w:type="dxa"
            writer.WriteAttributeString("w", "type", m_strWordMlNsUri,
                "dxa");

            writer.WriteEndElement();
        }

        void WriteTableProperty(XmlTextWriter writer)
        {
            // <w:tblPr>
            writer.WriteStartElement("w", "tblPr", m_strWordMlNsUri);

            // 表格单元定义
            // <w:tblCellMar>
            writer.WriteStartElement("w", "tblCellMar", m_strWordMlNsUri);

            WriteMarginDef(writer,
                "left",
                100);
            WriteMarginDef(writer,
                "right",
                100);

            // </w:tblCellMar>
            writer.WriteEndElement();


            // 表格边框定义
            // <w:tblBorders>
            writer.WriteStartElement("w", "tblBorders", m_strWordMlNsUri);

            // 上
            WriteBorderDef(writer, "top", 10, 10);
            WriteBorderDef(writer, "left", 10, 10);
            WriteBorderDef(writer, "bottom", 10, 10);
            WriteBorderDef(writer, "right", 10, 10);

            WriteBorderDef(writer, "insideH", 1, 1);
            WriteBorderDef(writer, "insideV", 1, 1);

            // </w:tblBorders>
            writer.WriteEndElement();

            // </w:tblPr>
            writer.WriteEndElement();
        }

        void WriteParagraph(XmlTextWriter writer,
            string strText)
        {
            // <w:p>
            writer.WriteStartElement("w", "p", m_strWordMlNsUri);
            // <w:r>
            writer.WriteStartElement("w", "r", m_strWordMlNsUri);
            // <w:t>
            writer.WriteStartElement("w", "t", m_strWordMlNsUri);

            writer.WriteString(strText);

            // <w:t>
            writer.WriteEndElement();
            // <w:r>
            writer.WriteEndElement();
            // <w:p>
            writer.WriteEndElement();
        }

        // 构造表格标题和标题行
        int WriteTableBegin(
            XmlTextWriter writer,
            PrintOption option,
            Hashtable macro_table)
        {

            // 表格标题
            string strTableTitleText = option.TableTitle;

            if (String.IsNullOrEmpty(strTableTitleText) == false)
            {

                strTableTitleText = StringUtil.MacroString(macro_table,
                    strTableTitleText);

                WriteParagraph(writer, strTableTitleText);
            }

            // <w:tbl>
            writer.WriteStartElement("w", "tbl", m_strWordMlNsUri);

            WriteTableProperty(writer);

            // <w:tr>
            writer.WriteStartElement("w", "tr", m_strWordMlNsUri);

            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                string strCaption = column.Caption;

                // 如果没有caption定义，就挪用name定义
                if (String.IsNullOrEmpty(strCaption) == true)
                    strCaption = column.Name;

                // string strClass = Global.GetLeft(column.Name);

                // <w:tc>
                writer.WriteStartElement("w", "tc", m_strWordMlNsUri);

                WriteParagraph(writer, strCaption);

                // <w:tc>
                writer.WriteEndElement();
            }

            // <w:tr>
            writer.WriteEndElement();

            return 0;
        }

        void WriteTableEnd(XmlTextWriter writer)
        {
            writer.WriteEndElement();
        }

        static void WriteValuePair(IXLWorksheet sheet,
            int nRowIndex,
            string strName,
            string strValue)
        {
            sheet.Cell(nRowIndex, 1).Value = strName;
            sheet.Cell(nRowIndex, 2).Value = strValue;
        }

        // 输出到文本文件
        int OutputToTextFile(
            List<ListViewItem> items,
            StreamWriter sw,
            ref XLWorkbook doc,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            Hashtable macro_table = new Hashtable();

            string strNamePath = "accountbook_printoption_text";

            // 获得打印参数
            PrintOption option = new AccountBookPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);

            // 检查当前排序状态和包含种价格列之间是否存在矛盾
            if (bHasBiblioPriceColumn(option) == true)
            {

                if (this.SortColumns_in.Count != 0
                    && this.SortColumns_in[0].No == COLUMN_BIBLIORECPATH)
                {
                }
                else
                {
                    MessageBox.Show(this, "由于当前打印用到了 “种价格”列，为保证打印结果的准确，程序自动按 ‘种记录路径’ 列对全部列表事项进行一次自动排序。\r\n\r\n为避免这里的自动排序，可在打印前用鼠标左键点栏标题进行符合自己意愿的排序，只要最后一次点的是‘种记录路径’栏标题即可。");
                    ForceSortColumnsIn(COLUMN_BIBLIORECPATH);
                }
            }

            // 2009/7/24 changed
            if (this.SourceStyle == "batchno")
            {
                macro_table["%batchno%"] = HttpUtility.HtmlEncode(this.BatchNo); // 批次号
                macro_table["%location%"] = HttpUtility.HtmlEncode(this.LocationString); // 馆藏地点 用HtmlEncode()的原因是要防止里面出现的“<不指定>”字样
            }
            else
            {
                macro_table["%batchno%"] = "";
                macro_table["%location%"] = "";
            }

            // macro_table["%pagecount%"] = nPageCount.ToString();
            // macro_table["%linesperpage%"] = option.LinesPerPage.ToString();
            macro_table["%date%"] = DateTime.Now.ToLongDateString();

            // 2009/7/24 changed
            if (this.SourceStyle == "barcodefile")
            {
                macro_table["%barcodefilepath%"] = this.BarcodeFilePath;
                macro_table["%barcodefilename%"] = Path.GetFileName(this.BarcodeFilePath);
            }
            else
            {
                macro_table["%barcodefilepath%"] = "";
                macro_table["%barcodefilename%"] = "";
            }

            // 2009/7/30 
            if (this.SourceStyle == "recpathfile")
            {
                macro_table["%recpathfilepath%"] = this.RecPathFilePath;
                macro_table["%recpathfilename%"] = Path.GetFileName(this.RecPathFilePath);
            }
            else
            {
                Debug.Assert(this.SourceStyle == "batchno" || this.SourceStyle == "barcodefile", "");

                macro_table["%recpathfilepath%"] = "";
                macro_table["%recpathfilename%"] = "";
            }

            macro_table["%sourcedescription%"] = this.SourceDescription;

            IXLWorksheet sheet = null;

            // 输出统计信息页
            if (this.TextOutputStatisPart == true)
            {
                if (doc != null)
                {
                    sheet = doc.Worksheets.Add("统计页");
                    sheet.Style.Font.FontName = this.Font.Name;
                }

                int nItemCount = items.Count;
                int nBiblioCount = GetBiblioCount(items);
                string strTotalPrice = GetTotalPrice(items);

                macro_table["%itemcount%"] = nItemCount.ToString();
                macro_table["%bibliocount%"] = nBiblioCount.ToString();
                macro_table["%totalprice%"] = strTotalPrice;

                macro_table["%pageno%"] = "1";

                // 2008/11/23 
                macro_table["%datadir%"] = this.MainForm.DataDir;   // 便于引用datadir下templates目录内的某些文件
                ////macro_table["%libraryserverdir%"] = this.MainForm.LibraryServerDir;  // 便于引用服务器端的CSS文件

                string strTemplateFilePath = option.GetTemplatePageFilePath("统计页");
                if (String.IsNullOrEmpty(strTemplateFilePath) == false)
                {
                    /*
                     * TODO：修改为纯文本方式
<html>
<head>
	<LINK href='%libraryserverdir%/accountbook.css' type='text/css' rel='stylesheet'>
</head>
<body>
	<div class='pageheader'>%date% 财产帐簿 -- %sourcedescription% -- (共 %pagecount% 页)</div>
	<div class='tabletitle'>%date% 财产帐簿 -- %sourcedescription%</div>
	<div class='itemcount'>册数: %itemcount%</div>
	<div class='bibliocount'>种数: %bibliocount%</div>
	<div class='totalprice'>总价: %totalprice%</div>
	<div class='sepline'><hr/></div>
	<div class='batchno'>批次号: %batchno%</div>
	<div class='location'>馆藏地点: %location%</div>
	<div class='location'>条码号文件: %barcodefilepath%</div>
	<div class='location'>记录路径文件: %recpathfilepath%</div>
	<div class='pagefooter'>%pageno%/%pagecount%</div>
</body>
</html>
                     * * */

                    // 根据模板打印
                    string strContent = "";
                    // 能自动识别文件内容的编码方式的读入文本文件内容模块
                    // return:
                    //      -1  出错
                    //      0   文件不存在
                    //      1   文件存在
                    nRet = Global.ReadTextFileContent(strTemplateFilePath,
                        out strContent,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    string strResult = StringUtil.MacroString(macro_table,
                        strContent);

                    if (sw != null)
                        sw.WriteLine(strResult);

                    // TODO: string --> excel page
                }
                else
                {
                    // 缺省的固定内容打印

                    // 内容行
                    if (sw != null)
                    {
                        sw.WriteLine("册数\t" + nItemCount.ToString());
                        sw.WriteLine("种数\t" + nBiblioCount.ToString());
                        sw.WriteLine("总价\t" + strTotalPrice);

                        sw.WriteLine("----------");


                        if (this.SourceStyle == "batchno")
                        {
                            // 2008/11/22 
                            if (String.IsNullOrEmpty(this.BatchNo) == false)
                            {
                                sw.WriteLine("批次号\t" + this.BatchNo);
                            }
                            if (String.IsNullOrEmpty(this.LocationString) == false
                                && this.LocationString != "<不指定>")
                            {
                                sw.WriteLine("馆藏地点\t" + this.LocationString);
                            }
                        }

                        if (this.SourceStyle == "barcodefile")
                        {
                            if (String.IsNullOrEmpty(this.BarcodeFilePath) == false)
                            {
                                sw.WriteLine("条码号文件\t" + this.BarcodeFilePath);
                            }
                        }

                        // 2009/7/30 
                        if (this.SourceStyle == "recpathfile")
                        {
                            if (String.IsNullOrEmpty(this.RecPathFilePath) == false)
                            {
                                sw.WriteLine("记录路径文件\t" + this.RecPathFilePath);
                            }
                        }


                        sw.WriteLine("----------");
                        sw.WriteLine("");
                    }

                    if (doc != null)
                    {
#if NO
                        int nLineIndex = 2;

                        doc.WriteExcelLine(
    nLineIndex++,
    "册数",
    nItemCount.ToString());

                        doc.WriteExcelLine(
    nLineIndex++,
    "种数",
    nBiblioCount.ToString());

                        doc.WriteExcelLine(
nLineIndex++,
"总价",
strTotalPrice);
#endif

                        int nLineIndex = 2;

                        WriteValuePair(sheet,
    nLineIndex++,
    "册数",
    nItemCount.ToString());

                        WriteValuePair(sheet,
    nLineIndex++,
    "种数",
    nBiblioCount.ToString());

                        WriteValuePair(sheet,
nLineIndex++,
"总价",
strTotalPrice);
                    }

                }

            }

            string strMarcFilterFilePath = option.GetTemplatePageFilePath("MARC过滤器");
            if (String.IsNullOrEmpty(strMarcFilterFilePath) == false)
            {
                nRet = PrepareMarcFilter(strMarcFilterFilePath, out strError);
                if (nRet == -1)
                    return -1;
            }

            if (doc != null)
            {
                sheet = doc.Worksheets.Add("财产帐");
                sheet.Style.Font.FontName = this.Font.Name;

#if NO
                Columns columns = new Columns();
                DocumentFormat.OpenXml.Spreadsheet.Column column = new DocumentFormat.OpenXml.Spreadsheet.Column();
                column.Min = 4;
                column.Max = 4;
                column.Width = 40;
                column.CustomWidth = true;
                columns.Append(column);

                doc.WorkSheet.InsertAt(columns, 0);
#endif
#if NO
                List<int> widths = new List<int>(new int [] {4,4,4,40});
                SetColumnWidth(doc, widths);
#endif
            }

            // 构造表格标题和标题行
            BuildTextPageTop(option,
                macro_table,
                sw,
                sheet);

            stop.SetProgressValue(0);
            stop.SetProgressRange(0, items.Count);

            // 表格行循环
            for (int i = 0; i < items.Count; i++)
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                BuildTextTableLine(option,
                    items,
                    i,
                    sw,
                    // ref doc,
                    sheet,
                    this.TextTruncate);

                stop.SetProgressValue(i + 1);
            }

            return 0;
        }

        static void SetColumnWidth(ExcelDocument doc,
            List<int> widths)
        {
            Columns columns = new Columns();
            uint i = 1;
            foreach (int width in widths)
            {
                DocumentFormat.OpenXml.Spreadsheet.Column column = new DocumentFormat.OpenXml.Spreadsheet.Column();
                if (width != -1)
                {
                    // min max 表示列范围编号
                    column.Min = UInt32Value.FromUInt32(i);
                    column.Max = UInt32Value.FromUInt32(i);

                    column.Width = width;
                    column.CustomWidth = true;
                    columns.Append(column);
                }
                i++;
            }

            doc.WorkSheet.InsertAt(columns, 0);
        }

        // 构造表格标题和标题行
        int BuildTextPageTop(PrintOption option,
            Hashtable macro_table,
            StreamWriter sw,
            // ref ExcelDocument doc
            IXLWorksheet sheet
            )
        {
            // 表格标题
            string strTableTitleText = option.TableTitle;

            if (String.IsNullOrEmpty(strTableTitleText) == false)
            {

                strTableTitleText = StringUtil.MacroString(macro_table,
                    strTableTitleText);

                if (sw != null)
                {
                    sw.WriteLine(strTableTitleText);
                    sw.WriteLine("");
                }

                if (sheet != null)
                {
#if NO
                    doc.WriteExcelTitle(0,
    option.Columns.Count,  // nTitleCols,
    strTableTitleText,
    6);
#endif
                    var header = sheet.Range(1, 1,
                        1, option.Columns.Count).Merge();
                    header.Value = strTableTitleText;
                    header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    // header.Style.Font.FontName = "微软雅黑";
                    header.Style.Font.Bold = true;
                    header.Style.Font.FontSize = 16;
                }
            }

            string strColumnTitleLine = "";

            List<int> widths = new List<int>();

            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                widths.Add(column.WidthChars);

                string strCaption = column.Caption;

                // 如果没有caption定义，就挪用name定义
                if (String.IsNullOrEmpty(strCaption) == true)
                    strCaption = column.Name;

                // string strClass = Global.GetLeft(column.Name);

                if (i != 0)
                    strColumnTitleLine += "\t";

                strColumnTitleLine += strCaption;

                if (sheet != null)
                {
#if NO
                                    doc.WriteExcelCell(
            2,
            i,
            strCaption,
            true);
#endif
                    var cell = sheet.Cell(2 + 1, i + 1);
                    cell.Value = strCaption;
                    // cell.Style.Font.FontName = "微软雅黑";
                    cell.Style.Font.Bold = true;

                    if (column.WidthChars != -1)
                        sheet.Column(i + 1).Width = column.WidthChars;
                }
            }

            if (sw != null)
                sw.WriteLine(strColumnTitleLine);

#if NO
            if (doc != null)
                SetColumnWidth(doc, widths);
#endif


            return 0;
        }

        const int _nTopIndex = 3;

        int BuildTextTableLine(PrintOption option,
            List<ListViewItem> items,
            int nIndex,
            StreamWriter sw,
            // ref ExcelDocument doc,
            IXLWorksheet sheet,
            bool bCutText)
        {
            string strError = "";
            int nRet = 0;

            if (nIndex >= items.Count)
            {
                strError = "error: nIndex(" + nIndex.ToString() + ") >= items.Count(" + items.Count.ToString() + ")";
                goto ERROR1;
            }

            ListViewItem item = items[nIndex];
            string strMARC = "";
            string strOutMarcSyntax = "";

            this.ColumnTable.Clear();   // 清除上一记录处理时残余的内容

            if (this.MarcFilter != null
                || option.HasEvalue() == true)
            {

                // TODO: 有错误要明显报出来，否则容易在打印出来后才发现，就晚了

                // 获得MARC格式书目记录
                string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);

                if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                {
                    nRet = GetMarc(strBiblioRecPath,
                        out strMARC,
                        out strOutMarcSyntax,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (this.MarcFilter != null)
                    {
                        this.MarcFilter.Host.UiItem = item; // 当前正在处理的 ListViewItem

                        // 触发filter中的Record相关动作
                        nRet = this.MarcFilter.DoRecord(
                            null,
                            strMARC,
                            strOutMarcSyntax,
                            nIndex,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                }
            }

            // 栏目内容
            string strLineContent = "";

            // bool bBiblioSumLine = false;    // 是否为种的最后一行(汇总行)
            List<CellData> cells = new List<CellData>();
            int nColIndex = 0;

            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];
                bool bNumber = false;

                // int nIndex = nPage * option.LinesPerPage + nLine;

                /*
                if (nIndex >= items.Count)
                    break;

                ListViewItem item = items[nIndex];
                 * */
                string strContent = "";
                if (string.IsNullOrEmpty(column.Evalue) == false)
                {
                    Jurassic.ScriptEngine engine = new Jurassic.ScriptEngine();
                    engine.EnableExposedClrTypes = true;
                    engine.SetGlobalValue("syntax", strOutMarcSyntax);
                    engine.SetGlobalValue("biblio", new MarcRecord(strMARC));
                    strContent = engine.Evaluate(column.Evalue).ToString();

                }
                else
                {
                    strContent = GetColumnContent(item,
                        column.Name);

                    if (strContent == "!!!#")
                    {
                        // strContent = ((nPage * option.LinesPerPage) + nLine + 1).ToString();
                        strContent = (nIndex + 1).ToString();
                        bNumber = true;
                    }

                    if (strContent == "!!!biblioPrice")
                    {
                        // 看看自己是不是处在切换边沿
                        string strCurLineBiblioRecPath = GetColumnContent(item, "biblioRecpath");

                        string strNextLineBiblioRecPath = "";

                        if (nIndex < items.Count - 1)
                        {
                            ListViewItem next_item = items[nIndex + 1];
                            strNextLineBiblioRecPath = GetColumnContent(next_item, "biblioRecpath");
                        }

                        if (strCurLineBiblioRecPath != strNextLineBiblioRecPath)
                        {
                            // 处在切换边沿

                            // 汇总前面的册价格
                            strContent = ComputeBiblioPrice(items, nIndex).ToString();
                            // bBiblioSumLine = true;
                        }
                        else
                        {
                            // 其他普通行
                            strContent = "";    //  "&nbsp;";
                        }

                    }
                }

                if (bCutText == true)
                {
                    // 截断字符串
                    if (column.MaxChars != -1)
                    {
                        if (strContent.Length > column.MaxChars)
                        {
                            strContent = strContent.Substring(0, column.MaxChars);
                            strContent += "...";
                        }
                    }
                }

                if (String.IsNullOrEmpty(strContent) == true)
                    strContent = "";    //  "&nbsp;";

                // string strClass = Global.GetLeft(column.Name);

                if (i != 0)
                    strLineContent += "\t";

                strLineContent += strContent;

                if (sheet != null)
                {
#if NO
                    CellData cell = new CellData(nColIndex++, strContent,
            !bNumber,
            5);
                    cells.Add(cell);
#endif
                    IXLCell cell = sheet.Cell(nIndex + _nTopIndex + 1, nColIndex + 1);
                    if (bNumber == true)
                        cell.Value = strContent;
                    else
                        cell.SetValue(strContent);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    nColIndex++;
                }
            }

            /*
            if (bBiblioSumLine == false)
            {
                StreamUtil.WriteText(strFileName,
        "<tr class='content'>");
            }
            else
            {
                StreamUtil.WriteText(strFileName,
        "<tr class='content_biblio_sum'>");
            }*/
            if (sw != null)
                sw.WriteLine(strLineContent);

#if NO
            if (doc != null)
                doc.WriteExcelLine(nIndex + _nTopIndex,
                    cells,
                    WriteExcelLineStyle.AutoString);  // WriteExcelLineStyle.None
#endif

            return 0;
        ERROR1:
            if (sw != null)
                sw.WriteLine(strError);
            if (sheet != null)
            {
#if NO
            List<CellData> temp_cells = new List<CellData>();
            temp_cells.Add(new CellData(0, strError));
            doc.WriteExcelLine(nIndex + _nTopIndex, temp_cells);
#endif
                IXLCell cell = sheet.Cell(nIndex + _nTopIndex + 1, 1);
                cell.Value = strError;

            }
            return -1;
        }

        // 构造html页面
        int BuildHtml(
            List<ListViewItem> items,
            out List<string> filenames,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            Hashtable macro_table = new Hashtable();

            string strNamePath = "accountbook_printoption_html";

            // 获得打印参数
            PrintOption option = new AccountBookPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);

            // 检查当前排序状态和包含种价格列之间是否存在矛盾
            if (bHasBiblioPriceColumn(option) == true)
            {

                if (this.SortColumns_in.Count != 0
                    && this.SortColumns_in[0].No == COLUMN_BIBLIORECPATH)
                {
                }
                else
                {
                    MessageBox.Show(this, "由于当前打印用到了 “种价格”列，为保证打印结果的准确，程序自动按 ‘种记录路径’ 列对全部列表事项进行一次自动排序。\r\n\r\n为避免这里的自动排序，可在打印前用鼠标左键点栏标题进行符合自己意愿的排序，只要最后一次点的是‘种记录路径’栏标题即可。");
                    ForceSortColumnsIn(COLUMN_BIBLIORECPATH);
                }


            }


            // 计算出页总数
            int nTablePageCount = items.Count / option.LinesPerPage;
            if ((items.Count % option.LinesPerPage) != 0)
                nTablePageCount++;

            int nPageCount = nTablePageCount + 1;

            // 2009/7/24 changed
            if (this.SourceStyle == "batchno")
            {
                // 2008/11/22 
                macro_table["%batchno%"] = HttpUtility.HtmlEncode(this.BatchNo); // 批次号
                macro_table["%location%"] = HttpUtility.HtmlEncode(this.LocationString); // 馆藏地点 用HtmlEncode()的原因是要防止里面出现的“<不指定>”字样
            }
            else
            {
                macro_table["%batchno%"] = "";
                macro_table["%location%"] = "";
            }

            macro_table["%pagecount%"] = nPageCount.ToString();
            macro_table["%linesperpage%"] = option.LinesPerPage.ToString();
            macro_table["%date%"] = DateTime.Now.ToLongDateString();

            // 2009/7/24 changed
            if (this.SourceStyle == "barcodefile")
            {
                macro_table["%barcodefilepath%"] = this.BarcodeFilePath;
                macro_table["%barcodefilename%"] = Path.GetFileName(this.BarcodeFilePath);
            }
            else
            {
                macro_table["%barcodefilepath%"] = "";
                macro_table["%barcodefilename%"] = "";
            }

            // 2009/7/30 
            if (this.SourceStyle == "recpathfile")
            {
                macro_table["%recpathfilepath%"] = this.RecPathFilePath;
                macro_table["%recpathfilename%"] = Path.GetFileName(this.RecPathFilePath);
            }
            else
            {
                Debug.Assert(this.SourceStyle == "batchno"
                    || this.SourceStyle == "barcodefile"
                    || this.SourceDescription == "",
                    "");

                macro_table["%recpathfilepath%"] = "";
                macro_table["%recpathfilename%"] = "";
            }

            macro_table["%sourcedescription%"] = this.SourceDescription;

            filenames = new List<string>();    // 每页一个文件，这个数组存放了所有文件名

            string strFileNamePrefix = Path.Combine(this.MainForm.DataDir, "~accountbook");

            string strFileName = "";

            // 输出统计信息页
            {
                int nItemCount = items.Count;
                int nBiblioCount = GetBiblioCount(items);
                string strTotalPrice = GetTotalPrice(items);

                macro_table["%itemcount%"] = nItemCount.ToString();
                macro_table["%bibliocount%"] = nBiblioCount.ToString();
                macro_table["%totalprice%"] = strTotalPrice;

                macro_table["%pageno%"] = "1";

                // 2008/11/23 
                macro_table["%datadir%"] = this.MainForm.DataDir;   // 便于引用datadir下templates目录内的某些文件
                ////macro_table["%libraryserverdir%"] = this.MainForm.LibraryServerDir;  // 便于引用服务器端的CSS文件
                // 2009/10/10 
                macro_table["%cssfilepath%"] = this.GetAutoCssUrl(option, "accountbook.css");  // 便于引用服务器端或“css”模板的CSS文件

                // strFileName = strFileNamePrefix + "0" + ".html";
                strFileName = strFileNamePrefix + "0-" + Guid.NewGuid().ToString() + ".html";

                filenames.Add(strFileName);

                string strTemplateFilePath = option.GetTemplatePageFilePath("统计页");
                if (String.IsNullOrEmpty(strTemplateFilePath) == false)
                {
                    /*
<html>
<head>
	老用法<LINK href='%libraryserverdir%/accountbook.css' type='text/css' rel='stylesheet'>
	新用法<LINK href='%cssfilepath%' type='text/css' rel='stylesheet'>
</head>
<body>
	<div class='pageheader'>%date% 财产帐簿 -- %sourcedescription% -- (共 %pagecount% 页)</div>
	<div class='tabletitle'>%date% 财产帐簿 -- %barcodefilepath%</div>
	<div class='itemcount'>册数: %itemcount%</div>
	<div class='bibliocount'>种数: %bibliocount%</div>
	<div class='totalprice'>总价: %totalprice%</div>
	<div class='sepline'><hr/></div>
	<div class='batchno'>批次号: %batchno%</div>
	<div class='location'>馆藏地点: %location%</div>
	<div class='location'>条码号文件: %barcodefilepath%</div>
	<div class='location'>记录路径文件: %recpathfilepath%</div>
	<div class='sepline'><hr/></div>
	<div class='pagefooter'>%pageno%/%pagecount%</div>
</body>
</html>
                     * * */

                    // 根据模板打印
                    string strContent = "";
                    // 能自动识别文件内容的编码方式的读入文本文件内容模块
                    // return:
                    //      -1  出错
                    //      0   文件不存在
                    //      1   文件存在
                    nRet = Global.ReadTextFileContent(strTemplateFilePath,
                        out strContent,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    string strResult = StringUtil.MacroString(macro_table,
                        strContent);
                    StreamUtil.WriteText(strFileName,
                        strResult);
                }
                else
                {
                    // 缺省的固定内容打印

                    BuildHtmlPageTop(option,
                        macro_table,
                        strFileName,
                        false);

                    // 内容行

                    StreamUtil.WriteText(strFileName,
                        "<div class='itemcount'>册数: " + nItemCount.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='bibliocount'>种数: " + nBiblioCount.ToString() + "</div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='totalprice'>总价: " + strTotalPrice + "</div>");

                    StreamUtil.WriteText(strFileName,
                        "<div class='sepline'><hr/></div>");


                    if (this.SourceStyle == "batchno")
                    {

                        // 2008/11/22 
                        if (String.IsNullOrEmpty(this.BatchNo) == false)
                        {
                            StreamUtil.WriteText(strFileName,
                                "<div class='batchno'>批次号: " + this.BatchNo + "</div>");
                        }
                        if (String.IsNullOrEmpty(this.LocationString) == false
                            && this.LocationString != "<不指定>")
                        {
                            StreamUtil.WriteText(strFileName,
                                "<div class='location'>馆藏地点: " + this.LocationString + "</div>");
                        }
                    }


                    if (this.SourceStyle == "barcodefile")
                    {
                        if (String.IsNullOrEmpty(this.BarcodeFilePath) == false)
                        {
                            StreamUtil.WriteText(strFileName,
                                "<div class='barcodefilepath'>条码号文件: " + this.BarcodeFilePath + "</div>");
                        }
                    }

                    // 2009/7/30
                    if (this.SourceStyle == "recpathfile")
                    {
                        if (String.IsNullOrEmpty(this.RecPathFilePath) == false)
                        {
                            StreamUtil.WriteText(strFileName,
                                "<div class='recpathfilepath'>记录路径文件: " + this.RecPathFilePath + "</div>");
                        }
                    }

                    /*
                    StreamUtil.WriteText(strFileName,
                        "<div class='sepline'><hr/></div>");


                    StreamUtil.WriteText(strFileName,
                        "<div class='sender'>移交者: </div>");
                    StreamUtil.WriteText(strFileName,
                        "<div class='recipient'>接受者: </div>");
                     * */


                    BuildHtmlPageBottom(option,
                        macro_table,
                        strFileName,
                        false);
                }

            }


            string strMarcFilterFilePath = option.GetTemplatePageFilePath("MARC过滤器");
            if (String.IsNullOrEmpty(strMarcFilterFilePath) == false)
            {
#if NO
                ColumnFilterDocument filter = null;

                this.ColumnTable = new Hashtable();
                nRet = PrepareMarcFilter(strMarcFilterFilePath,
                    out filter,
                    out strError);
                if (nRet == -1)
                    return -1;

                //
                if (filter != null)
                    this.AssemblyFilter = filter.Assembly;
                else
                    this.AssemblyFilter = null;

                this.MarcFilter = filter;
#endif
                nRet = PrepareMarcFilter(strMarcFilterFilePath, out strError);
                if (nRet == -1)
                    return -1;
            }


            // 表格页循环
            for (int i = 0; i < nTablePageCount; i++)
            {
                macro_table["%pageno%"] = (i + 1 + 1).ToString();

                // strFileName = strFileNamePrefix + (i + 1).ToString() + ".html";
                strFileName = strFileNamePrefix + (i + 1).ToString() + "-" + Guid.NewGuid().ToString() + ".html";

                filenames.Add(strFileName);

                BuildHtmlPageTop(option,
                    macro_table,
                    strFileName,
                    true);
                // 行循环
                for (int j = 0; j < option.LinesPerPage; j++)
                {
                    BuildHtmlTableLine(option,
                        items,
                        strFileName, i, j);
                }

                BuildHtmlPageBottom(option,
                    macro_table,
                    strFileName,
                    true);
            }

            /*
            for (int i = 0; i < this.listView_in.Items.Count; i++)
            {

            }
             * */


            return 0;
        }


        // 用于缩进格式的tab字符串
        static string IndentString(int nLevel)
        {
            if (nLevel <= 0)
                return "";
            return new string('\t', nLevel);
        }

        /*
        // 2009/10/10 
        // 获得css文件的路径(或者http:// 地址)。将根据是否具有“统计页”来自动处理
        string GetAutoCssUrl(PrintOption option)
        {
            string strCssFilePath = option.GetTemplatePageFilePath("css");  // 大小写不敏感
            if (String.IsNullOrEmpty(strCssFilePath) == false)
                return strCssFilePath;
            else
                return this.MainForm.LibraryServerDir + "/accountbook.css";    // 缺省的
        }*/

        // 2009/10/10 
        // 获得css文件的路径(或者http:// 地址)。将根据是否具有“统计页”来自动处理
        // parameters:
        //      strDefaultCssFileName   “css”模板缺省情况下，将采用的虚拟目录中的css文件名，纯文件名
        string GetAutoCssUrl(PrintOption option,
            string strDefaultCssFileName)
        {
            string strCssFilePath = option.GetTemplatePageFilePath("css");  // 大小写不敏感
            if (String.IsNullOrEmpty(strCssFilePath) == false)
                return strCssFilePath;
            else
            {
                // return this.MainForm.LibraryServerDir + "/" + strDefaultCssFileName;    // 缺省的
                return PathUtil.MergePath(this.MainForm.DataDir, strDefaultCssFileName);    // 缺省的
            }
        }

        int BuildHtmlPageTop(PrintOption option,
            Hashtable macro_table,
            string strFileName,
            bool bOutputTable)
        {
            /*
            string strLibraryServerUrl = this.MainForm.AppInfo.GetString(
    "config",
    "circulation_server_url",
    "");
            int pos = strLibraryServerUrl.LastIndexOf("/");
            if (pos != -1)
                strLibraryServerUrl = strLibraryServerUrl.Substring(0, pos);
             * */

            // 2009/10/10 changed
            string strCssUrl = GetAutoCssUrl(option, "accountbook.css");

            string strLink = IndentString(2) + "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />\r\n";

            StreamUtil.WriteText(strFileName,
                "<html>\r\n"
                + IndentString(1) + "<head>\r\n" + strLink
                + IndentString(1) + "</head>\r\n"
                + IndentString(1) + "<body>\r\n");


            // 页眉
            string strPageHeaderText = option.PageHeader;

            if (String.IsNullOrEmpty(strPageHeaderText) == false)
            {
                strPageHeaderText = StringUtil.MacroString(macro_table,
                    strPageHeaderText);

                StreamUtil.WriteText(strFileName,
                    IndentString(2) + "<div class='pageheader'>" + strPageHeaderText + "</div><!-- 页眉 -->\r\n");

                /*
                StreamUtil.WriteText(strFileName,
                    "<hr class='pageheader' />");
                 * */
            }

            // 表格标题
            string strTableTitleText = option.TableTitle;

            if (String.IsNullOrEmpty(strTableTitleText) == false)
            {

                strTableTitleText = StringUtil.MacroString(macro_table,
                    strTableTitleText);

                StreamUtil.WriteText(strFileName,
                    IndentString(2) + "<div class='tabletitle'>" + strTableTitleText + "</div><!-- 表格标题 -->\r\n");
            }

            if (bOutputTable == true)
            {

                // 表格开始
                StreamUtil.WriteText(strFileName,
                    IndentString(2) + "<table class='table'><!-- 内容表格开始 -->\r\n");   //   border='1'

                // 栏目标题
                StreamUtil.WriteText(strFileName,
                    IndentString(3) + "<tr class='column'><!-- 栏目标题行开始 -->\r\n");

                for (int i = 0; i < option.Columns.Count; i++)
                {
                    Column column = option.Columns[i];

                    string strCaption = column.Caption;

                    // 如果没有caption定义，就挪用name定义
                    if (String.IsNullOrEmpty(strCaption) == true)
                        strCaption = column.Name;

                    string strClass = StringUtil.GetLeft(column.Name);
                    if (strClass.Length > 0 && strClass[0] == '@')
                    {
                        strClass = "ext_" + strClass.Substring(1);
                    }

                    StreamUtil.WriteText(strFileName,
                        IndentString(4) + "<td class='" + strClass + "'>" + strCaption + "</td>\r\n");
                }

                StreamUtil.WriteText(strFileName,
                    IndentString(3) + "</tr><!-- 栏目标题行结束 -->\r\n");

            }

            return 0;
        }

        // 汇总种价格。假定nIndex处在切换行(同一种内最后一行)
        decimal ComputeBiblioPrice(List<ListViewItem> items,
            int nIndex)
        {
            decimal total = 0;
            string strBiblioRecPath = GetColumnContent(items[nIndex], "biblioRecpath");
            for (int i = nIndex; i >= 0; i--)
            {
                ListViewItem item = items[i];

                string strCurBiblioRecPath = GetColumnContent(item, "biblioRecpath");

                if (strCurBiblioRecPath != strBiblioRecPath)
                    break;

                string strPrice = GetColumnContent(item, "price");

                // 提取出纯数字
                string strPurePrice = PriceUtil.GetPurePrice(strPrice);

                if (String.IsNullOrEmpty(strPurePrice) == true)
                    continue;

                total += Convert.ToDecimal(strPurePrice);
            }

            return total;
        }

        // 是否包含种价格列?
        static bool bHasBiblioPriceColumn(PrintOption option)
        {
            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                string strText = StringUtil.GetLeft(column.Name);


                if (strText == "biblioPrice"
                    || strText == "种价格")
                    return true;
            }

            return false;
        }

#if NO
        // 获得MARC格式书目记录
        int GetMarc(ListViewItem item,
            out string strMARC,
            out string strOutMarcSyntax,
            out string strError)
        {
            strError = "";
            strMARC = "";
            strOutMarcSyntax = "";
            int nRet = 0;

            string[] formats = new string[1];
            formats[0] = "xml";

            string[] results = null;
            byte[] timestamp = null;

            string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);

            Debug.Assert(String.IsNullOrEmpty(strBiblioRecPath) == false, "strBiblioRecPath值不能为空");

            long lRet = Channel.GetBiblioInfos(
                    null, // stop,
                    strBiblioRecPath,
                    formats,
                    out results,
                    out timestamp,
                    out strError);
            if (lRet == -1 || lRet == 0)
            {
                if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                    strError = "书目记录 '" + strBiblioRecPath + "' 不存在";

                strError = "获得书目记录时发生错误: " + strError;
                return -1;
            }

            string strXml = results[0];

            // 转换为MARC格式
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            nRet = MarcUtil.Xml2Marc(strXml,
                false,
                "", // strMarcSyntax
                out strOutMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }
#endif

        int BuildHtmlTableLine(PrintOption option,
            List<ListViewItem> items,
            string strFileName,
            int nPage,
            int nLine)
        {
            // 栏目内容
            string strLineContent = "";
            int nRet = 0;

            bool bBiblioSumLine = false;    // 是否为种的最后一行(汇总行)

            int nIndex = nPage * option.LinesPerPage + nLine;

            if (nIndex >= items.Count)
                goto END1;

            ListViewItem item = items[nIndex];

            string strMARC = "";
            string strOutMarcSyntax = "";

            this.ColumnTable.Clear();   // 清除上一记录处理时残余的内容

            if (this.MarcFilter != null
                || option.HasEvalue() == true)
            {
                string strError = "";

                // TODO: 有错误要明显报出来，否则容易在打印出来后才发现，就晚了

                // 获得MARC格式书目记录
                string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);

                if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                {
                    // TODO: 可以 cache，提高速度
                    nRet = GetMarc(strBiblioRecPath,
                        out strMARC,
                        out strOutMarcSyntax,
                        out strError);
                    if (nRet == -1)
                    {
                        strLineContent = strError;
                        goto END1;
                    }

                    if (this.MarcFilter != null)
                    {
                        this.MarcFilter.Host.UiItem = item; // 当前正在处理的 ListViewItem

                        // 触发filter中的Record相关动作
                        nRet = this.MarcFilter.DoRecord(
                            null,
                            strMARC,
                            strOutMarcSyntax,
                            nIndex,
                            out strError);
                        if (nRet == -1)
                        {
                            strLineContent = strError;
                            goto END1;
                        }
                    }
                }
            }

            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                /*
                int nIndex = nPage * option.LinesPerPage + nLine;

                if (nIndex >= items.Count)
                    break;

                ListViewItem item = items[nIndex];
                 * */

                string strContent = "";

                if (string.IsNullOrEmpty(column.Evalue) == false)
                {
                    Jurassic.ScriptEngine engine = new Jurassic.ScriptEngine();
                    engine.EnableExposedClrTypes = true;
                    engine.SetGlobalValue("syntax", strOutMarcSyntax);
                    engine.SetGlobalValue("biblio", new MarcRecord(strMARC));
                    strContent = engine.Evaluate(column.Evalue).ToString();

                }
                else
                {

                    strContent = GetColumnContent(item,
                        column.Name);

                    if (strContent == "!!!#")
                        strContent = ((nPage * option.LinesPerPage) + nLine + 1).ToString();

                    if (strContent == "!!!biblioPrice")
                    {
                        // 看看自己是不是处在切换边沿
                        string strCurLineBiblioRecPath = GetColumnContent(item, "biblioRecpath");

                        string strNextLineBiblioRecPath = "";

                        if (nIndex < items.Count - 1)
                        {
                            ListViewItem next_item = items[nIndex + 1];
                            strNextLineBiblioRecPath = GetColumnContent(next_item, "biblioRecpath");
                        }

                        if (strCurLineBiblioRecPath != strNextLineBiblioRecPath)
                        {
                            // 处在切换边沿

                            // 汇总前面的册价格
                            strContent = ComputeBiblioPrice(items, nIndex).ToString();
                            bBiblioSumLine = true;
                        }
                        else
                        {
                            // 其他普通行
                            strContent = "&nbsp;";
                        }
                    }

                }

                // 截断字符串
                if (column.MaxChars != -1)
                {
                    if (strContent.Length > column.MaxChars)
                    {
                        strContent = strContent.Substring(0, column.MaxChars);
                        strContent += "...";
                    }
                }

                if (String.IsNullOrEmpty(strContent) == true)
                    strContent = "&nbsp;";

                string strClass = StringUtil.GetLeft(column.Name);
                if (strClass.Length > 0 && strClass[0] == '@')
                {
                    strClass = "ext_" + strClass.Substring(1);
                }

                strLineContent +=
                    IndentString(4) + "<td class='" + strClass + "'>" + strContent + "</td>\r\n";
            }

        END1:

            string strOdd = "";
            if (((nLine + 1) % 2) != 0) // 用每页内的行号来计算奇数
                strOdd = " odd";

            string strBiblioSum = "";
            if (bBiblioSumLine == true)
                strBiblioSum = " biblio_sum";

            // 2009/10/10 changed
            StreamUtil.WriteText(strFileName,
                IndentString(3) + "<tr class='content" + strBiblioSum + strOdd + "'><!-- 内容行"
                + (bBiblioSumLine == true ? "(书目汇总)" : "")
                + (nIndex + 1).ToString() + " -->\r\n");

            StreamUtil.WriteText(strFileName,
                strLineContent);

            StreamUtil.WriteText(strFileName,
                IndentString(3) + "</tr>\r\n");

            return 0;
        }


        // 获得栏目内容
        string GetColumnContent(ListViewItem item,
            string strColumnName)
        {
            // 去掉"-- ?????"部分
            /*
            string strText = strColumnName;
            int nRet = strText.IndexOf("--", 0);
            if (nRet != -1)
                strText = strText.Substring(0, nRet).Trim();
             * */

            string strText = StringUtil.GetLeft(strColumnName);

            // 2009/10/8
            // 要求ColumnTable值
            if (strText.Length > 0 && strText[0] == '@')
            {
                strText = strText.Substring(1);

                /*
                if (this.ColumnTable.Contains(strText) == false)
                    return "error:列 '" + strText + "' 在ColumnTable中没有找到";
                 * */

                return (string)this.ColumnTable[strText];
            }

            try
            {

                // 要中英文都可以
                switch (strText)
                {
                    case "no":
                    case "序号":
                        return "!!!#";  // 特殊值，表示序号
                    case "barcode":
                    case "册条码号":
                        return item.SubItems[COLUMN_BARCODE].Text;
                    case "errorInfo":
                    case "summary":
                    case "摘要":
                        return item.SubItems[COLUMN_SUMMARY].Text;

                    case "isbnIssn":
                    case "ISBN/ISSN":
                        return item.SubItems[COLUMN_ISBNISSN].Text;
                    case "publishTime":
                    case "出版时间":
                        return item.SubItems[MERGED_COLUMN_PUBLISHTIME].Text;
                    case "volume":
                    case "卷期":
                        return item.SubItems[MERGED_COLUMN_VOLUME].Text;
                    case "orderClass":
                    case "订购类别":
                        return item.SubItems[EXTEND_COLUMN_CLASS].Text;
                    case "catalogNo":
                    case "书目号":
                        return item.SubItems[EXTEND_COLUMN_CATALOGNO].Text;
                    case "orderTime":
                    case "订购时间":
                        return item.SubItems[EXTEND_COLUMN_ORDERTIME].Text;
                    case "orderID":
                    case "订单号":
                        return item.SubItems[EXTEND_COLUMN_ORDERID].Text;
                    case "seller":
                    case "书商":
                    case "渠道":
                        return item.SubItems[EXTEND_COLUMN_SELLER].Text;
                    case "source":
                    case "经费来源":
                        return item.SubItems[EXTEND_COLUMN_SOURCE].Text;

                    case "orderPrice":
                    case "订购价":
                        return ListViewUtil.GetItemText(item, EXTEND_COLUMN_ORDERPRICE);

                    case "acceptPrice":
                    case "到书价":
                        return ListViewUtil.GetItemText(item, EXTEND_COLUMN_ACCEPTPRICE);

                    case "state":
                        return item.SubItems[COLUMN_STATE].Text;
                    case "location":
                    case "馆藏地点":
                        return item.SubItems[COLUMN_LOCATION].Text;
                    case "price":
                    case "册价格":
                        return item.SubItems[COLUMN_PRICE].Text;
                    case "bookType":
                        return item.SubItems[COLUMN_BOOKTYPE].Text;
                    case "registerNo":
                        return item.SubItems[COLUMN_REGISTERNO].Text;
                    case "comment":
                        return item.SubItems[COLUMN_COMMENT].Text;
                    case "mergeComment":
                        return item.SubItems[COLUMN_MERGECOMMENT].Text;
                    case "batchNo":
                        return item.SubItems[COLUMN_BATCHNO].Text;
                    /*
                case "borrower":
                    return item.SubItems[COLUMN_BORROWER].Text;
                case "borrowDate":
                    return item.SubItems[COLUMN_BORROWDATE].Text;
                case "borrowPeriod":
                    return item.SubItems[COLUMN_BORROWPERIOD].Text;
                     * */
                    case "recpath":
                        return item.SubItems[COLUMN_RECPATH].Text;
                    case "biblioRecpath":
                    case "种记录路径":
                        return item.SubItems[COLUMN_BIBLIORECPATH].Text;
                    case "accessNo":
                    case "索书号":
                    case "索取号":
                        return item.SubItems[COLUMN_ACCESSNO].Text;
                    case "biblioPrice":
                    case "种价格":
                        return "!!!biblioPrice";  // 特殊值，表示种价格
                    case "refID":
                    case "参考ID":
                        return item.SubItems[COLUMN_REFID].Text;
                    default:
                        {
                            if (this.ColumnTable.Contains(strText) == false)
                                return "未知栏目 '" + strText + "'";

                            return (string)this.ColumnTable[strText];
                        }
                }
            }
            catch
            {
                return null;    // 表示没有这个subitem下标
            }

        }

        int BuildHtmlPageBottom(PrintOption option,
            Hashtable macro_table,
            string strFileName,
            bool bOutputTable)
        {


            if (bOutputTable == true)
            {
                // 表格结束
                StreamUtil.WriteText(strFileName,
                    IndentString(2) + "</table><!-- 内容表格结束 -->\r\n");
            }

            // 页脚
            string strPageFooterText = option.PageFooter;

            if (String.IsNullOrEmpty(strPageFooterText) == false)
            {
                /*
                StreamUtil.WriteText(strFileName,
                    "<hr class='pagefooter' />");
                 * */


                strPageFooterText = StringUtil.MacroString(macro_table,
                    strPageFooterText);

                StreamUtil.WriteText(strFileName,
                    IndentString(2) + "<div class='pagefooter'>" + strPageFooterText + "</div><!-- 页脚 -->\r\n");
            }


            StreamUtil.WriteText(strFileName, IndentString(1) + "</body>\r\n</html>");

            return 0;
        }

        static int GetBiblioCount(List<ListViewItem> items)
        {
            List<string> paths = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strText = "";

                try
                {
                    strText = item.SubItems[COLUMN_BIBLIORECPATH].Text;
                }
                catch
                {
                    continue;
                }
                paths.Add(strText);
            }

            // 排序
            paths.Sort();

            int nCount = 0;
            string strPrev = "";
            for (int i = 0; i < paths.Count; i++)
            {
                if (strPrev != paths[i])
                {
                    nCount++;
                    strPrev = paths[i];
                }
            }

            return nCount;
        }

#if NO
        static decimal GetTotalPrice(List<ListViewItem> items)
        {
            decimal total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strPrice = "";

                try
                {
                    strPrice = item.SubItems[COLUMN_PRICE].Text;
                }
                catch
                {
                    continue;
                }

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                // 提取出纯数字
                string strPurePrice = PriceUtil.GetPurePrice(strPrice);

                if (String.IsNullOrEmpty(strPurePrice) == true)
                    continue;

                total += Convert.ToDecimal(strPurePrice);
            }

            return total;
        }
#endif

        static string GetTotalPrice(List<ListViewItem> items)
        {
            List<string> prices = new List<string>();
            foreach (ListViewItem item in items)
            {
                string strPrice = "";

                try
                {
                    strPrice = item.SubItems[COLUMN_PRICE].Text;
                }
                catch
                {
                    continue;
                }

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                prices.Add(strPrice);
            }

            string strError = "";
            string strTotalPrice = "";
            // 汇总价格
            // 货币单位不同的，互相独立
            // 本函数还有另外一个版本，是返回List<string>的
            // return:
            //      -1  error
            //      0   succeed
            int nRet = PriceUtil.TotalPrice(prices,
            out strTotalPrice,
            out strError);
            if (nRet == -1)
                return strError;

            return strTotalPrice;
        }

        private void listView_in_DoubleClick(object sender, EventArgs e)
        {
            LoadToEntityForm(this.listView_in);
        }


        void LoadToEntityForm(ListView list)
        {
            if (list.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装载的事项");
                return;
            }

            string strBarcode = list.SelectedItems[0].Text;
            string strRecPath = ListViewUtil.GetItemText(list.SelectedItems[0], COLUMN_RECPATH);

            EntityForm form = new EntityForm();

            form.MainForm = this.MainForm;
            form.MdiParent = this.MainForm;
            form.Show();

            if (String.IsNullOrEmpty(strBarcode) == false)
                form.LoadItemByBarcode(strBarcode, false);
            else
                form.LoadItemByRecPath(strRecPath, false);
        }

#if NO
        // 检索 批次号 和 馆藏地点 将命中的记录路径写入文件
        // parameters:
        //      strBatchNo 要限定的批次号。如果为 "" 表示批次号为空，而 null 表示不指定批次号
        //      strLocation 要限定的馆藏地点名称。如果为 "" 表示馆藏地点为空，而 null 表示不指定馆藏地点
        int SearchBatchNoAndLocation(
            string strBatchNo,
            string strLocation,
            string strOutputFilename,
            out string strError)
        {
            strError = "";
            long lRet = 0;

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索批次号 '"+strBatchNo+"' 和馆藏地点 '"+strLocation+"' ...");
            stop.BeginLoop();

            try
            {


                string strQueryXml = "";

                if (strBatchNo != null
                    && strLocation != null)
                {
                    string strBatchNoQueryXml = "";
                    lRet = Channel.SearchItem(
        stop,
         this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
        strBatchNo,
        -1,
        "批次号",
        "exact",
        this.Lang,
        "null",   // strResultSetName
        "",    // strSearchStyle
        "__buildqueryxml", // strOutputStyle
        out strError);
                    if (lRet == -1)
                        return -1;
                    strBatchNoQueryXml = strError;

                    string strLocationQueryXml = "";
                    lRet = Channel.SearchItem(
        stop,
         this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
        strLocation,
        -1,
        "馆藏地点",
        "exact",
        this.Lang,
        "null",   // strResultSetName
        "",    // strSearchStyle
        "__buildqueryxml", // strOutputStyle
        out strError);
                    if (lRet == -1)
                        return -1;
                    strLocationQueryXml = strError;

                    // 合并成一个检索式
                    strQueryXml = "<group>" + strBatchNoQueryXml + "<operator value='AND'/>" + strLocationQueryXml + "</group>";    // !!!
#if DEBUG
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strQueryXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "合并检索式进入DOM时出错: " + ex.Message;
                        return -1;
                    }
#endif


                }
                else if (strBatchNo != null)
                {
                    stop.SetMessage("正在检索批次号 '" + strBatchNo + "' ...");

                    lRet = Channel.SearchItem(
        stop,
         this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
        strBatchNo,
        -1,
        "批次号",
        "exact",
        this.Lang,
        "null",   // strResultSetName
        "",    // strSearchStyle
        "__buildqueryxml", // strOutputStyle
        out strError);
                    if (lRet == -1)
                        return -1;
                    strQueryXml = strError;
                }
                else if (strLocation != null)
                {
                    stop.SetMessage("正在检索馆藏地点 '" + strLocation + "' ...");

                    lRet = Channel.SearchItem(
    stop,
    this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
    strLocation,    // strBatchNo, BUG !!!
    -1,
    "馆藏地点",
    "exact",
    this.Lang,
    "null",   // strResultSetName
    "",    // strSearchStyle
    "__buildqueryxml", // strOutputStyle
    out strError);
                    if (lRet == -1)
                        return -1;
                    strQueryXml = strError;
                }
                else
                {
                    Debug.Assert(strBatchNo == null && strLocation == null,
                        "");
                    lRet = Channel.SearchItem(
    stop,
    this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
    "", // strBatchNo,
    -1,
    "__id",
    "left",
    this.Lang,
    "null",   // strResultSetName
    "",    // strSearchStyle
    "__buildqueryxml", // strOutputStyle
    out strError);
                    if (lRet == -1)
                        return -1;
                    strQueryXml = strError;
                }

                long lHitCount = 0;

                using (StreamWriter sw = new StreamWriter(strOutputFilename))
                {
                    lRet = Channel.Search(stop,
        strQueryXml,
        "default",
        "id",   // 只要记录路径
        out strError);
                    if (lRet == -1)
                        return -1;
                    if (lRet == 0)
                        return 0;   // 没有命中

                    lHitCount = lRet;

                    stop.SetProgressRange(0, lHitCount);

                    long lStart = 0;
                    long lPerCount = Math.Min(150, lHitCount);
                    DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                    // 装入浏览格式
                    for (; ; )
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                        }

                        // stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                        lRet = Channel.GetSearchResult(
                            stop,
                            "default",   // strResultSetName
                            lStart,
                            lPerCount,
                            "id",
                            this.Lang,
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                            return -1;

                        if (lRet == 0)
                        {
                            strError = "GetSearchResult() error";
                            return -1;
                        }

                        // 处理浏览结果
                        foreach (DigitalPlatform.LibraryClient.localhost.Record record in searchresults)
                        {
                            sw.WriteLine(record.Path);
                        }

                        lStart += searchresults.Length;
                        // lCount -= searchresults.Length;
                        if (lStart >= lHitCount || lPerCount <= 0)
                            break;

                        stop.SetProgressValue(lStart);
                    }
                }


                return (int)lHitCount;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
                // MainForm.ShowProgress(false);
            }

        }
#endif

        // 根据批次号检索装载
        private void button_load_loadFromBatchNo_Click(object sender, EventArgs e)
        {
            SearchByBatchnoForm dlg = new SearchByBatchnoForm();
            MainForm.SetControlFont(dlg, this.Font, false);

            // 2008/11/30 
            dlg.BatchNo = this.BatchNo;
            dlg.ItemLocation = this.LocationString;

            dlg.CfgSectionName = "AccountBookForm_SearchByBatchnoForm";
            dlg.GetLocationValueTable -= new GetValueTableEventHandler(dlg_GetLocationValueTable);
            dlg.GetLocationValueTable += new GetValueTableEventHandler(dlg_GetLocationValueTable);

            dlg.GetBatchNoTable -= new GetKeyCountListEventHandler(dlg_GetBatchNoTable);
            dlg.GetBatchNoTable += new GetKeyCountListEventHandler(dlg_GetBatchNoTable);
            dlg.RefDbName = "";
            /*
                dlg.RefDbName = EntityForm.GetDbName(this.entityEditControl1.RecPath);
             * */
            dlg.MainForm = this.MainForm;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.SourceStyle = "batchno";

            // 2008/11/22 
            this.BatchNo = dlg.BatchNo;
            this.LocationString = dlg.ItemLocation;

            string strMatchLocation = dlg.ItemLocation;
            if (strMatchLocation == "<不指定>")
                strMatchLocation = null;    // null和""的区别很大

            string strBatchNo = dlg.BatchNo;
            if (strBatchNo == "<不指定>")
                strBatchNo = null;    // null和""的区别很大

            string strError = "";

            bool bClearBefore = true;
            if (System.Windows.Forms.Control.ModifierKeys == Keys.Control)
                bClearBefore = false;

            if (bClearBefore == true)
                ClearBefore();

            string strRecPathFilename = Path.GetTempFileName();

            try
            {
                // 检索 批次号 和 馆藏地点 将命中的记录路径写入文件
                int nRet = SearchBatchNoAndLocation(
                    this.comboBox_load_type.Text,
                    strBatchNo,
                    strMatchLocation,
                    strRecPathFilename,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = LoadFromRecPathFile(strRecPathFilename,
                    this.comboBox_load_type.Text,
                    this.checkBox_load_fillBiblioSummary.Checked,
                    new string[] { "summary", "@isbnissn" },
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            }
            finally
            {
                if (string.IsNullOrEmpty(strRecPathFilename) == false)
                {
                    File.Delete(strRecPathFilename);
                    strRecPathFilename = "";
                }
            }
            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        // 根据批次号检索装载
        private void button_load_loadFromBatchNo_Click(object sender, EventArgs e)
        {
            SearchByBatchnoForm dlg = new SearchByBatchnoForm();
            MainForm.SetControlFont(dlg, this.Font, false);

            // 2008/11/30 
            dlg.BatchNo = this.BatchNo;
            dlg.ItemLocation = this.LocationString;

            dlg.CfgSectionName = "AccountBookForm_SearchByBatchnoForm";
            dlg.GetLocationValueTable -= new GetValueTableEventHandler(dlg_GetLocationValueTable);
            dlg.GetLocationValueTable += new GetValueTableEventHandler(dlg_GetLocationValueTable);

            dlg.GetBatchNoTable -= new GetKeyCountListEventHandler(dlg_GetBatchNoTable);
            dlg.GetBatchNoTable += new GetKeyCountListEventHandler(dlg_GetBatchNoTable);
            dlg.RefDbName = "";
            /*
                dlg.RefDbName = EntityForm.GetDbName(this.entityEditControl1.RecPath);
             * */
            dlg.MainForm = this.MainForm;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.SourceStyle = "batchno";

            // 2008/11/22 
            this.BatchNo = dlg.BatchNo;
            this.LocationString = dlg.ItemLocation;

            string strMatchLocation = dlg.ItemLocation;

            if (strMatchLocation == "<不指定>")
                strMatchLocation = null;    // null和""的区别很大

            string strError = "";

            if (Control.ModifierKeys == Keys.Control)
            {
            }
            else
            {
                this.listView_in.Items.Clear();
                this.SortColumns_in.Clear();
                SortColumns.ClearColumnSortDisplay(this.listView_in.Columns);

                this.refid_table.Clear();
                this.orderxml_table.Clear();
            }

            EnableControls(false);
            //MainForm.ShowProgress(true);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            try
            {
                stop.SetProgressRange(0, 100);
                // stop.SetProgressValue(0);

                long lRet = Channel.SearchItem(
                    stop,
                    // 2010/2/25 changed
                     this.comboBox_load_type.Text == "图书" ? "<all book>" : "<all series>",
                    dlg.BatchNo,
                    -1,
                    "批次号",
                    string.IsNullOrEmpty(dlg.BatchNo) == false ? "exact" : "left",  // 这样允许配次号为空检索
                    this.Lang,
                    "batchno",   // strResultSetName
                    "",    // strSearchStyle
                    "", // strOutputStyle
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    strError = "批次号 '"+dlg.BatchNo+"' 没有命中记录。";
                    goto ERROR1;
                }

                int nDupCount = 0;

                long lHitCount = lRet;

                stop.SetProgressRange(0, lHitCount);
                stop.SetProgressValue(0);


                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            MessageBox.Show(this, "用户中断");
                            return;
                        }
                    }


                    lRet = Channel.GetSearchResult(
                        stop,
                        "batchno",   // strResultSetName
                        lStart,
                        lCount,
                        "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetSearchResult() error: " + strError;
                        // Debug.Assert(false, "");
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        MessageBox.Show(this, "未命中");
                        return;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                MessageBox.Show(this, "用户中断");
                                return;
                            }
                        }

                        DigitalPlatform.LibraryClient.localhost.Record result_item = searchresults[i];

                        string strBarcode = result_item.Cols[0];
                        string strRecPath = result_item.Path;

                        /*
                        // 如果册条码号为空，则改用路径装载
                        // 2009/8/6 
                        if (String.IsNullOrEmpty(strBarcode) == true)
                        {
                            strBarcode = "@path:" + strRecPath;
                        }
                         * */

                        // 加速
                        strBarcode = "@path:" + strRecPath;

                        string strOutputItemRecPath = "";
                        ListViewItem item = null;
                        // 根据册条码号或者记录路径，装入册记录
                        // return: 
                        //      -2  册条码号已经在list中存在了
                        //      -1  出错
                        //      0   因为馆藏地点不匹配，没有加入list中
                        //      1   成功
                        int nRet = LoadOneItem(
                            this.comboBox_load_type.Text,
                            strBarcode,
                            null,
                            this.listView_in,
                            strMatchLocation,
                            out strOutputItemRecPath,
                            out item,
                            out strError);
                        if (nRet == -2)
                            nDupCount++;
                        /*
                        ReaderSearchForm.NewLine(
                            this.listView_records,
                            searchresults[i].Path,
                            searchresults[i].Cols);
                         * */
                        stop.SetProgressValue(lStart + i + 1);

                        // TODO: 是否要检查记录路径从属的图书或者期刊库是否正确?

                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

                if (this.listView_in.Items.Count == 0
                    && strMatchLocation != null)
                {
                    strError = "虽然批次号 '" + dlg.BatchNo + "' 命中了记录 " + lHitCount.ToString() + " 条, 但它们均未能匹配馆藏地点 '" + strMatchLocation + "' 。";
                    goto ERROR1;
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
                // MainForm.ShowProgress(false);
            }

            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif


        void dlg_GetBatchNoTable(object sender, GetKeyCountListEventArgs e)
        {
            Global.GetBatchNoTable(e,
                this,
                this.comboBox_load_type.Text,
                "item",
                this.stop,
                this.Channel);

#if NOOOOOOOOOOOOOOOOOOO
            string strError = "";

            if (e.KeyCounts == null)
                e.KeyCounts = new List<KeyCount>();

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在列出全部册批次号 ...");
            stop.BeginLoop();

            try
            {
                MainForm.SetProgressRange(100);
                MainForm.SetProgressValue(0);

                long lRet = Channel.SearchItem(
                    stop,
                    "<all>",
                    "", // strBatchNo
                    2000,  // -1,
                    "批次号",
                    "left",
                    this.Lang,
                    "batchno",   // strResultSetName
                    "keycount", // strOutputStyle
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    strError = "没有找到任何册批次号检索点";
                    return;
                }

                long lHitCount = lRet;

                long lStart = 0;
                long lCount = lHitCount;
                SearchResult[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            MessageBox.Show(this, "用户中断");
                            return;
                        }
                    }

                    lRet = Channel.GetSearchResult(
                        stop,
                        "batchno",   // strResultSetName
                        lStart,
                        lCount,
                        "keycount",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetSearchResult() error: " + strError;
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        // MessageBox.Show(this, "未命中");
                        return;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        if (searchresults[i].Cols == null)
                        {
                            strError = "请更新应用服务器和数据库内核到最新版本";
                            goto ERROR1;
                        }

                        KeyCount keycount = new KeyCount();
                        keycount.Key = searchresults[i].Path;
                        keycount.Count = Convert.ToInt32(searchresults[i].Cols[0]);
                        e.KeyCounts.Add(keycount);
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        void dlg_GetLocationValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";
            string[] values = null;
            int nRet = MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            e.values = values;
        }

        // return:
        //      -1  出错
        //      0   没有必要排序
        //      1   已完成排序
        int DoSort(string strSortStyle,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strSortStyle) == true
                || strSortStyle == "<无>")
                return 0;

            if (strSortStyle == "册条码号")
            {
                // 注：本函数如果发现第一列已经设置好，则不改变其方向。不过，这并不意味着其方向一定是升序
                this.SortColumns_in.SetFirstColumn(COLUMN_BARCODE,
                    this.listView_in.Columns,
                    false);
            }
            else if (strSortStyle == "登录号")
            {
                this.SortColumns_in.SetFirstColumn(COLUMN_REGISTERNO,
                    this.listView_in.Columns,
                    false);
            }
            else if (strSortStyle == "渠道")
            {
                this.SortColumns_in.SetFirstColumn(COLUMN_BARCODE,
                    this.listView_in.Columns,
                    false);
                this.SortColumns_in.SetFirstColumn(EXTEND_COLUMN_SELLER,
                    this.listView_in.Columns,
                    false);
            }
            else if (strSortStyle == "经费来源")
            {
                this.SortColumns_in.SetFirstColumn(COLUMN_BARCODE,
                    this.listView_in.Columns,
                    false);
                this.SortColumns_in.SetFirstColumn(EXTEND_COLUMN_SOURCE,
                    this.listView_in.Columns,
                    false);
            }
            else
            {
                strError = "未知的排序风格 '" + strSortStyle + "'";
                return -1;
            }

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            try
            {

                // 排序
                this.listView_in.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_in);
                this.listView_in.ListViewItemSorter = null;
            }
            finally
            {
                this.Cursor = oldCursor;
            }

            SetGroupBackcolor(
                this.listView_in,
                this.SortColumns_in[0].No);

            return 1;
        }

        void ForceSortColumnsIn(int nClickColumn)
        {
            // 注：本函数如果发现第一列已经设置好，则不改变其方向。不过，这并不意味着其方向一定是升序
            this.SortColumns_in.SetFirstColumn(nClickColumn,
                this.listView_in.Columns,
                false);

            // 排序
            this.listView_in.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_in);
            this.listView_in.ListViewItemSorter = null;

            SetGroupBackcolor(
                this.listView_in,
                nClickColumn);
        }

        /*
        // 修改排序数组，设置第一列，把原来的列号推后
        public static void ChangeSortColumns(ref List<int> SortColumns,
            int nFirstColumn)
        {
            // 从数组中移走已经存在的值
            SortColumns.Remove(nFirstColumn);
            // 放到首部
            SortColumns.Insert(0, nFirstColumn);
        }
         * */

        // 根据排序键值的变化分组设置颜色
        static void SetGroupBackcolor(
            ListView list,
            int nSortColumn)
        {
            string strPrevText = "";
            bool bDark = false;
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];

                if (item.ImageIndex == TYPE_ERROR)
                    continue;

                string strThisText = "";
                try
                {
                    strThisText = item.SubItems[nSortColumn].Text;
                }
                catch
                {
                }

                if (strThisText != strPrevText)
                {
                    // 变化颜色
                    if (bDark == true)
                        bDark = false;
                    else
                        bDark = true;

                    strPrevText = strThisText;
                }

                if (bDark == true)
                {
                    item.BackColor = System.Drawing.Color.LightGray;

                    if (item.ForeColor == item.BackColor)
                        item.ForeColor = System.Drawing.Color.Black;
                }
                else
                {
                    item.BackColor = System.Drawing.Color.White;

                    if (item.ForeColor == item.BackColor)
                        item.ForeColor = System.Drawing.Color.Black;
                }
            }
        }

        // 对集合内列表进行排序
        private void listView_in_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            this.SortColumns_in.SetFirstColumn(nClickColumn,
                this.listView_in.Columns);

            // 排序
            this.listView_in.ListViewItemSorter = new SortColumnsComparer(this.SortColumns_in);

            this.listView_in.ListViewItemSorter = null;

            SetGroupBackcolor(
                this.listView_in,
                nClickColumn);

        }

        private void listView_in_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新 [" + this.listView_in.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_refreshSelected_Click);
            if (this.listView_in.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            /*
            menuItem = new MenuItem("刷新全部行(&R)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_refreshAll_Click);
            contextMenu.MenuItems.Add(menuItem);
             * */

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除 [" + this.listView_in.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_deleteSelected_Click);
            if (this.listView_in.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_in, new Point(e.X, e.Y));
        }

        void menu_refreshSelected_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)((MenuItem)sender).Tag;
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in list.SelectedItems)
            {
                items.Add(item);

            }
            RefreshLines(
                COLUMN_RECPATH,
                items,
                this.checkBox_load_fillBiblioSummary.Checked,
                new string[] { "summary", "@isbnissn" });
        }

        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)((MenuItem)sender).Tag;

            ListViewUtil.SelectAllLines(list);
        }

#if NO
        void menu_refreshAll_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)((MenuItem)sender).Tag;

            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < list.Items.Count; i++)
            {
                items.Add(list.Items[i]);
            }
            RefreshLines(items, this.checkBox_load_fillBiblioSummary.Checked);
        }
#endif

        // 删除集合内列表中已经选定的行
        void menu_deleteSelected_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)((MenuItem)sender).Tag;


            if (list.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要删除的事项。");
                return;
            }

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in list.SelectedItems)
            {
                items.Add(item);
            }

            DialogResult result = MessageBox.Show(this,
"确实要删除选定的 " + items.Count.ToString() + " 个事项?",
"dp2Circulation",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            DeleteLines(items);

        }

        static void DeleteLines(List<ListViewItem> items)
        {
            if (items.Count == 0)
                return;
            ListView list = items[0].ListView;

            for (int i = 0; i < items.Count; i++)
            {
                list.Items.Remove(items[i]);
            }
        }

#if NO
        void RefreshLines(List<ListViewItem> items,
    bool bFillBiblioSummary)
        {
            string strError = "";
            string strTimeMessage = "";
            int nRet = 0;

            EnableControls(false);
            // MainForm.ShowProgress(true);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在刷新 ...");
            stop.BeginLoop();

            try
            {
                stop.SetProgressRange(0, items.Count);
                ProgressEstimate estimate = new ProgressEstimate();
                estimate.SetRange(0, items.Count);
                estimate.Start();

                int nLineCount = 0;
                List<string> lines = new List<string>();
                List<ListViewItem> part_items = new List<ListViewItem>();
                for (int i = 0; i < items.Count; i++)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断1";
                        goto ERROR1;
                    }

                    ListViewItem item = items[i];

                    stop.SetMessage("正在刷新 " + item.Text + " ...");
                    stop.SetProgressValue(i);

                    string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

                    lines.Add(strRecPath);
                    part_items.Add(item);
                    if (lines.Count >= 100)
                    {
                        if (lines.Count > 0)
                            stop.SetMessage("(" + i.ToString() + " / " + nLineCount.ToString() + ") 正在装入路径 " + lines[0] + " 等记录。"
                                + "剩余时间 " + ProgressEstimate.Format(estimate.Estimate(i)) + " 已经过时间 " + ProgressEstimate.Format(estimate.delta_passed));

                        // 处理一小批记录的装入
                        nRet = DoLoadRecords(lines,
                            part_items,
                            bFillBiblioSummary,
                            new string [] {"summary","@isbnissn"},
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        lines.Clear();
                        part_items.Clear();
                    }
                }

                // 最后剩下的一批
                if (lines.Count > 0)
                {
                    if (lines.Count > 0)
                        stop.SetMessage("(" + nLineCount.ToString() + " / " + nLineCount.ToString() + ") 正在装入路径 " + lines[0] + " 等记录...");

                    // 处理一小批记录的装入
                    nRet = DoLoadRecords(lines,
                        part_items,
                        bFillBiblioSummary,
                        new string[] { "summary", "@isbnissn" },
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    lines.Clear();
                    part_items.Clear();
                }

                strTimeMessage = "共刷新册信息 " + nLineCount.ToString() + " 条。耗费时间: " + estimate.GetTotalTime().ToString();

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("刷新完成。");
                stop.HideProgress();

                EnableControls(true);
                // MainForm.ShowProgress(false);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif

#if NO
        void RefreshLines(List<ListViewItem> items,
            bool bFillBiblioSummary)
        {
            string strError = "";

            EnableControls(false);
            // MainForm.ShowProgress(true);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在刷新 ...");
            stop.BeginLoop();

            try
            {
                stop.SetProgressRange(0, items.Count);
                // stop.SetProgressValue(0);


                for (int i = 0; i < items.Count; i++)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断1";
                        goto ERROR1;
                    }


                    ListViewItem item = items[i];

                    stop.SetMessage("正在刷新 " + item.Text + " ...");

                    int nRet = RefreshOneItem(item, bFillBiblioSummary, out strError);
                    /*
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                     * */

                    stop.SetProgressValue(i);
                }

                stop.SetProgressValue(items.Count);

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("刷新完成。");
                stop.HideProgress();

                EnableControls(true);
                // MainForm.ShowProgress(false);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif

#if NO
        public int RefreshOneItem(ListViewItem item,
            bool bFillBiblioSummary,
            out string strError)
        {
            strError = "";

            string strItemText = "";
            string strBiblioText = "";

            string strItemRecPath = "";
            string strBiblioRecPath = "";

            byte[] item_timestamp = null;

            string strBarcode = "";

            string strBarcodeOrRecPath = item.Text;
            if (StringUtil.HasHead(strBarcodeOrRecPath, "@path:") == true)
                strBarcode = strBarcodeOrRecPath;
            else
                strBarcode = "@path:" + item.SubItems[COLUMN_RECPATH].Text;
            REDO_GETITEMINFO:
            long lRet = Channel.GetItemInfo(
                stop,
                strBarcode,
                "xml",
                out strItemText,
                out strItemRecPath,
                out item_timestamp,
                "recpath",
                out strBiblioText,
                out strBiblioRecPath,
                out strError);
            if (lRet == -1)
            {
                DialogResult temp_result = MessageBox.Show(this,
strError + "\r\n\r\n是否重试?",
"AccountBookForm",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (temp_result == DialogResult.Retry)
                    goto REDO_GETITEMINFO;
            } 
            if (lRet == -1 || lRet == 0)
            {
                ListViewUtil.ChangeItemText(item, 1, strError);

                SetItemColor(item, TYPE_ERROR);
                goto ERROR1;
            }

            string strBiblioSummary = "";
            string strISBnISSN = "";

            SummaryInfo info = (SummaryInfo)this.m_summaryTable[strBiblioRecPath];
            if (info != null)
            {
                strBiblioSummary = info.Summary;
                strISBnISSN = info.ISBnISSn;
            }

            if (strBiblioSummary == ""
                && (this.checkBox_load_fillBiblioSummary.Checked == true || bFillBiblioSummary == true ) )
            {
                string[] formats = new string[2];
                formats[0] = "summary";
                formats[1] = "@isbnissn";
                string[] results = null;
                byte[] timestamp = null;

                Debug.Assert( String.IsNullOrEmpty(strBiblioRecPath) == false, "strBiblioRecPath值不能为空");
            REDO_GETBIBLIOINFO:
                lRet = Channel.GetBiblioInfos(
                    stop,
                    strBiblioRecPath,
                    formats,
                    out results,
                    out timestamp,
                    out strError);
                if (lRet == -1)
                {
                    DialogResult temp_result = MessageBox.Show(this,
    strError + "\r\n\r\n是否重试?",
    "AccountBookForm",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO_GETBIBLIOINFO;
                } 
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                        strError = "书目记录 '" + strBiblioRecPath + "' 不存在";

                    strBiblioSummary = "获得书目摘要时发生错误: " + strError;

                    // TODO: 如果results.Length表现正常，其实还可以继续处理
                }
                else
                {
                    Debug.Assert(results != null && results.Length == 2, "results必须包含2个元素");
                    strBiblioSummary = results[0];
                    strISBnISSN = results[1];

                    // 避免cache占据的内存太多
                    if (this.m_summaryTable.Count > 1000)
                        this.m_summaryTable.Clear();

                    if (info == null)
                    {
                        info = new SummaryInfo();
                        info.Summary = strBiblioSummary;
                        info.ISBnISSn = strISBnISSN;
                        this.m_summaryTable[strBiblioRecPath] = info;
                    }
                }
            }

            // 剖析一个册的xml记录，取出有关信息放入listview中
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemText);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }


            {
                SetListViewItemText(dom,
                    true,
                    strItemRecPath,
                    strBiblioSummary,
                    strISBnISSN,
                    strBiblioRecPath,
                    item);
            }

            // 图标
            // item.ImageIndex = TYPE_NORMAL;
            SetItemColor(item, TYPE_NORMAL);

            // 2009/7/25 
            // 填充需要从订购库获得的栏目信息
            if (this.checkBox_load_fillOrderInfo.Checked == true)
                FillOrderColumns(item, this.comboBox_load_type.Text);

            return 1;
        ERROR1:
            return -1;
        }
#endif

        // 输出到文本文件
        private void button_print_outputTextFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_in.Items.Count == 0)
            {
                strError = "没有可输出的行";
                goto ERROR1;
            }

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的文本文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.ExportTextFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "文本文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportTextFilename = dlg.FileName;

            OutputAcountBookTextFileDialog option_dialog = new OutputAcountBookTextFileDialog();
            MainForm.SetControlFont(option_dialog, this.Font, false);

            option_dialog.Truncate = this.TextTruncate;
            option_dialog.OutputStatisPart = this.TextOutputStatisPart;
            option_dialog.StartPosition = FormStartPosition.CenterScreen;
            option_dialog.ShowDialog(this);

            if (option_dialog.DialogResult != DialogResult.OK)
                return;

            this.TextTruncate = option_dialog.Truncate;
            this.TextOutputStatisPart = option_dialog.OutputStatisPart;

            bool bAppend = true;

            if (File.Exists(this.ExportTextFilename) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "文本文件 '" + this.ExportTextFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃输出)",
                    "AccountBookForm",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.No)
                    bAppend = false;
            }

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在构造财产帐 ...");
            stop.BeginLoop();

            // 创建文件
            StreamWriter sw = new StreamWriter(this.ExportTextFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8);
            try
            {
                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                int nCount = 0;
                List<ListViewItem> items = new List<ListViewItem>();
                for (int i = 0; i < this.listView_in.Items.Count; i++)
                {
                    ListViewItem item = this.listView_in.Items[i];

                    items.Add(item);
                    nCount++;
                }

                XLWorkbook doc = null;

                // 输出到文本文件
                int nRet = OutputToTextFile(
                    items,
                    sw,
                    ref doc,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.Cursor = oldCursor;

                string strExportStyle = "创建";
                if (bAppend == true)
                    strExportStyle = "追加";

                this.MainForm.StatusBarMessage = "财产帐簿内容 " + nCount.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportTextFilename;

            }
            finally
            {
                if (sw != null)
                    sw.Close();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 配置打印选项 WordXML
        private void button_print_optionWordXml_Click(object sender, EventArgs e)
        {
            // 配置标题和风格
            string strNamePath = "accountbook_printoption_wordxml";

            PrintOption option = new AccountBookPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);

            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.MainForm = this.MainForm;
            dlg.Text = this.comboBox_load_type.Text + " WordML 打印配置";
            dlg.DataDir = this.MainForm.DataDir;    // 允许新增模板页
            dlg.PrintOption = option;
            dlg.ColumnItems = new string[] {
                "no -- 序号",
                "barcode -- 册条码号",
                "summary -- 摘要",

                // 2009/7/24 
                "isbnIssn -- ISBN/ISSN",
                "publishTime -- 出版时间",
                "volume -- 卷期",
                "orderClass -- 订购类别",
                "catalogNo -- 书目号",
                "orderTime -- 订购时间",
                "orderID -- 订单号",
                "seller -- 渠道",
                "source -- 经费来源",
                "orderPrice -- 订购价",
                "acceptPrice -- 验收价",

                "accessNo -- 索取号",
                "state -- 状态",
                "location -- 馆藏地点",
                "price -- 册价格",
                "bookType -- 册类型",
                "registerNo -- 登录号",
                "comment -- 注释",
                "mergeComment -- 合并注释",
                "batchNo -- 批次号",
                /*
                "borrower -- 借阅者",
                "borrowDate -- 借阅日期",
                "borrowPeriod -- 借阅期限",
                 * */
                "recpath -- 册记录路径",
                "biblioRecpath -- 种记录路径",
                "biblioPrice -- 种价格",
                "refID -- 参考ID"
            };


            this.MainForm.AppInfo.LinkFormState(dlg, "accountbook_printoption_wordxml_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(this.MainForm.AppInfo,
                strNamePath);
        }

        // 配置打印选项 HTML
        private void button_print_optionHTML_Click(object sender, EventArgs e)
        {
            // 配置标题和风格
            string strNamePath = "accountbook_printoption_html";

            PrintOption option = new AccountBookPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);

            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.MainForm = this.MainForm;
            dlg.Text = this.comboBox_load_type.Text + " HTML 打印配置";
            dlg.DataDir = this.MainForm.DataDir;    // 允许新增模板页
            dlg.PrintOption = option;
            dlg.ColumnItems = new string[] {
                "no -- 序号",
                "barcode -- 册条码号",
                "summary -- 摘要",

                // 2009/7/24 
                "isbnIssn -- ISBN/ISSN",
                "publishTime -- 出版时间",
                "volume -- 卷期",
                "orderClass -- 订购类别",
                "catalogNo -- 书目号",
                "orderTime -- 订购时间",
                "orderID -- 订单号",
                "seller -- 渠道",
                "source -- 经费来源",
                "orderPrice -- 订购价",
                "acceptPrice -- 验收价",

                "accessNo -- 索取号",
                "state -- 状态",
                "location -- 馆藏地点",
                "price -- 册价格",
                "bookType -- 册类型",
                "registerNo -- 登录号",
                "comment -- 注释",
                "mergeComment -- 合并注释",
                "batchNo -- 批次号",
                /*
                "borrower -- 借阅者",
                "borrowDate -- 借阅日期",
                "borrowPeriod -- 借阅期限",
                 * */
                "recpath -- 册记录路径",
                "biblioRecpath -- 种记录路径",
                "biblioPrice -- 种价格",
                "refID -- 参考ID"
            };


            this.MainForm.AppInfo.LinkFormState(dlg, "accountbook_printoption_html_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(this.MainForm.AppInfo,
                strNamePath);
        }

        // 配置打印选项 纯文本文件
        private void button_print_optionText_Click(object sender, EventArgs e)
        {
            // 配置标题和风格
            string strNamePath = "accountbook_printoption_text";

            PrintOption option = new AccountBookPrintOption(this.MainForm.DataDir,
                this.comboBox_load_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);

            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.MainForm = this.MainForm;
            dlg.Text = this.comboBox_load_type.Text + " 纯文本 输出配置";
            dlg.DataDir = this.MainForm.DataDir;    // 允许新增模板页
            dlg.PrintOption = option;
            dlg.ColumnItems = new string[] {
                "no -- 序号",
                "barcode -- 册条码号",
                "summary -- 摘要",

                // 2009/7/24 
                "isbnIssn -- ISBN/ISSN",
                "publishTime -- 出版时间",
                "volume -- 卷期",
                "orderClass -- 订购类别",
                "catalogNo -- 书目号",
                "orderTime -- 订购时间",
                "orderID -- 订单号",
                "seller -- 渠道",
                "source -- 经费来源",
                "orderPrice -- 订购价",
                "acceptPrice -- 验收价",

                "accessNo -- 索取号",
                "state -- 状态",
                "location -- 馆藏地点",
                "price -- 册价格",
                "bookType -- 册类型",
                "registerNo -- 登录号",
                "comment -- 注释",
                "mergeComment -- 合并注释",
                "batchNo -- 批次号",
                /*
                "borrower -- 借阅者",
                "borrowDate -- 借阅日期",
                "borrowPeriod -- 借阅期限",
                 * */
                "recpath -- 册记录路径",
                "biblioRecpath -- 种记录路径",
                "biblioPrice -- 种价格",
                "refID -- 参考ID"
            };


            this.MainForm.AppInfo.LinkFormState(dlg, "accountbook_printoption_text_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(this.MainForm.AppInfo,
                strNamePath);
        }

        private void listView_in_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_in.SelectedIndices.Count == 0)
                this.MainForm.StatusBarMessage = "未选定行";
            else if (this.listView_in.SelectedIndices.Count == 1)
                this.MainForm.StatusBarMessage = "行号 " + (this.listView_in.SelectedIndices[0] + 1).ToString();
            else
            {
                this.MainForm.StatusBarMessage = "从行号 " + (this.listView_in.SelectedIndices[0] + 1).ToString() + " 起共选定了 " + this.listView_in.SelectedIndices.Count.ToString() + " 个事项";
            }
        }


        // 输出到WordXML文件
        private void button_print_outputWordXmlFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的WordML文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = this.ExportWordXmlFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "WordML文件 (*.xml)|*.xml|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportWordXmlFilename = dlg.FileName;

            OutputAcountBookTextFileDialog option_dialog = new OutputAcountBookTextFileDialog();
            MainForm.SetControlFont(option_dialog, this.Font, false);

            option_dialog.Text = "财产帐簿输出到WordML文件";
            option_dialog.MessageText = "请指定输出特性";
            option_dialog.Truncate = this.WordXmlTruncate;
            option_dialog.OutputStatisPart = this.WordXmlOutputStatisPart;
            option_dialog.StartPosition = FormStartPosition.CenterScreen;
            option_dialog.ShowDialog(this);

            if (option_dialog.DialogResult != DialogResult.OK)
                return;

            EnableControls(false);

            try
            {

                this.WordXmlTruncate = option_dialog.Truncate;
                this.WordXmlOutputStatisPart = option_dialog.OutputStatisPart;


                XmlTextWriter writer = null;

                try
                {
                    writer = new XmlTextWriter(this.ExportWordXmlFilename, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    strError = "创建文件 '" + ExportWordXmlFilename + "' 时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                try
                {
                    Cursor oldCursor = this.Cursor;
                    this.Cursor = Cursors.WaitCursor;

                    int nCount = 0;
                    List<ListViewItem> items = new List<ListViewItem>();
                    for (int i = 0; i < this.listView_in.Items.Count; i++)
                    {
                        ListViewItem item = this.listView_in.Items[i];

                        items.Add(item);
                        nCount++;
                    }


                    // 输出到Word XML文件
                    int nRet = OutputToWordXmlFile(
                        items,
                        writer,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    this.Cursor = oldCursor;

                    this.MainForm.StatusBarMessage = "财产帐簿内容 " + nCount.ToString() + "个 已成功创建到文件 " + this.ExportWordXmlFilename;
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                        writer = null;
                    }
                }
            }
            finally
            {
                EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void AccountBookForm_Activated(object sender, EventArgs e)
        {
            // 2009/8/13 
            this.MainForm.stopManager.Active(this.stop);
        }

        string m_strUsedScriptFilename = "";
        private void button_print_runScript_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定 C# 脚本文件";
            dlg.FileName = this.m_strUsedScriptFilename;
            dlg.Filter = "C# 脚本文件 (*.cs)|*.cs|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.m_strUsedScriptFilename = dlg.FileName;

            AccountBookHost host = null;
            Assembly assembly = null;

            nRet = PrepareScript(this.m_strUsedScriptFilename,
                out assembly,
                out host,
                out strError);
            if (nRet == -1)
                goto ERROR1;



            this.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行脚本 " + dlg.FileName + "</div>");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在针对浏览行 C# 脚本 ...");
            stop.BeginLoop();

            this.EnableControls(false);

            this.listView_in.Enabled = false;
            try
            {
                {
                    host.AccountBookForm = this;

                    StatisEventArgs args = new StatisEventArgs();
                    host.OnInitial(this, args);
                    if (args.Continue == ContinueType.SkipAll)
                        return;
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
                }

                if (stop != null)
                    stop.SetProgressRange(0, this.listView_in.Items.Count);

                {
                    host.AccountBookForm = this;

                    StatisEventArgs args = new StatisEventArgs();
                    host.OnBegin(this, args);
                    if (args.Continue == ContinueType.SkipAll)
                        return;
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
                }

                int i = 0;
                foreach (ListViewItem item in this.listView_in.Items)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    stop.SetProgressValue(i);


                    this.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode((i + 1).ToString()) + "</div>");

                    host.AccountBookForm = this;
                    host.ListViewItem = item;

                    StatisEventArgs args = new StatisEventArgs();
                    host.OnRecord(this, args);
                    if (args.Continue == ContinueType.SkipAll)
                        break;
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }

                    i++;
                }

                {
                    host.AccountBookForm = this;
                    host.ListViewItem = null;

                    StatisEventArgs args = new StatisEventArgs();
                    host.OnEnd(this, args);
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
                }
            }
            finally
            {
                if (host != null)
                    host.FreeResources();

                this.listView_in.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControls(true);

                this.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行脚本 " + dlg.FileName + "</div>");
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 获得浏览控件的 ListView 类型对象
        /// </summary>
        public ListView ListView
        {
            get
            {
                return this.listView_in;
            }
        }

        // 准备脚本环境
        int PrepareScript(string strCsFileName,
            out Assembly assembly,
            out AccountBookHost host,
            out string strError)
        {
            assembly = null;
            strError = "";
            host = null;

            string strContent = "";
            Encoding encoding;
            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // parameters:
            //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
            // return:
            //      -1  出错 strError中有返回值
            //      0   文件不存在 strError中有返回值
            //      1   文件存在
            //      2   读入的内容不是全部
            int nRet = FileUtil.ReadTextFileContent(strCsFileName,
                -1,
                out strContent,
                out encoding,
                out strError);
            if (nRet == -1)
                return -1;

            string strWarningInfo = "";
            string[] saAddRef = {
                                    // 2011/4/20 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.windows.forms.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",

									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
   									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.libraryclient.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Script.dll",  // 2011/8/25 新增
									Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
                Environment.CurrentDirectory + "\\dp2circulation.exe",
            };

            // 2013/12/16
            nRet = ScriptManager.GetRef(strCsFileName,
    ref saAddRef,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            // 直接编译到内存
            // parameters:
            //		refs	附加的refs文件路径。路径中可能包含宏%installdir%
            nRet = ScriptManager.CreateAssembly_1(strContent,
                saAddRef,
                "",
                out assembly,
                out strError,
                out strWarningInfo);
            if (nRet == -1)
                goto ERROR1;

            // 得到Assembly中Host派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "dp2Circulation.AccountBookHost");
            if (entryClassType == null)
            {
                strError = strCsFileName + " 中没有找到 dp2Circulation.MarcQueryHost 派生类";
                goto ERROR1;
            }

            // new一个Host派生对象
            host = (AccountBookHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            return 0;
        ERROR1:
            return -1;
        }

        private void button_print_createNewScriptFile_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要创建的 C# 脚本文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = "";
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "C#脚本文件 (*.cs)|*.cs|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                AccountBookHost.CreateStartCsFile(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "创建 C# 脚本时出错: " + ex.Message);
                return;
            }

            System.Diagnostics.Process.Start("notepad.exe", dlg.FileName);

            this.m_strUsedScriptFilename = dlg.FileName;
        }

        private void button_print_outputExcelFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_in.Items.Count == 0)
            {
                strError = "没有可输出的行";
                goto ERROR1;
            }

            XLWorkbook doc = null;

            // 询问文件名
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = this.ExportExcelFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportExcelFilename = dlg.FileName;

#if NO
            try
            {
                doc = ExcelDocument.Create(this.ExportExcelFilename);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

            doc.Stylesheet = GenerateStyleSheet();
#endif
            try
            {
                doc = new XLWorkbook(XLEventTracking.Disabled);
                File.Delete(this.ExportExcelFilename);
            }
            catch (Exception ex)
            {
                strError = "初始化 XLWorkbook 时出错: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            OutputAcountBookTextFileDialog option_dialog = new OutputAcountBookTextFileDialog();
            MainForm.SetControlFont(option_dialog, this.Font, false);

            option_dialog.Truncate = this.TextTruncate;
            option_dialog.OutputStatisPart = this.TextOutputStatisPart;
            option_dialog.StartPosition = FormStartPosition.CenterScreen;
            option_dialog.ShowDialog(this);

            if (option_dialog.DialogResult != DialogResult.OK)
                return;

            this.TextTruncate = option_dialog.Truncate;
            this.TextOutputStatisPart = option_dialog.OutputStatisPart;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在构造财产帐 ...");
            stop.BeginLoop();

            try
            {
                int nCount = 0;
                List<ListViewItem> items = new List<ListViewItem>();
                for (int i = 0; i < this.listView_in.Items.Count; i++)
                {
                    ListViewItem item = this.listView_in.Items[i];

                    items.Add(item);
                    nCount++;
                }

                // 输出到文本文件
                int nRet = OutputToTextFile(
                    items,
                    null,
                    ref doc,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (doc != null)
                {
                    // Close the document.
                    // doc.Close();
                    doc.SaveAs(this.ExportExcelFilename);
                }

                this.MainForm.StatusBarMessage = "财产帐簿内容 " + nCount.ToString() + "个 已成功输出到文件 " + this.ExportExcelFilename;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        // parameters:
        //      nIndex  事项的下标。也可以不使用，用 0 即可
        public int DoMarcFilter(ListViewItem item,
            int nIndex,
            out string strError)
        {
            strError = "";

            if (this.MarcFilter == null)
            {
                strError = "尚未初始化 MARC 过滤器。请使用 PrepareMarcFilter() 方法初始化";
                return -1;
            }


            string strMARC = "";
            string strOutMarcSyntax = "";
            int nRet = 0;

            // TODO: 有错误要明显报出来，否则容易在打印出来后才发现，就晚了

            // 获得MARC格式书目记录
            string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);
            nRet = GetMarc(strBiblioRecPath,
                out strMARC,
                out strOutMarcSyntax,
                out strError);
            if (nRet == -1)
                return -1;

            this.ColumnTable.Clear();   // 清除上一记录处理时残余的内容

            // 触发filter中的Record相关动作
            nRet = this.MarcFilter.DoRecord(
                null,
                strMARC,
                strOutMarcSyntax,
                nIndex,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

#endif
    }

    // 定义了特定缺省值的PrintOption派生类
    class AccountBookPrintOption : PrintOption
    {
        string PublicationType = "图书"; // 图书 连续出版物

        public AccountBookPrintOption(string strDataDir,
            string strPublicationType)
        {
            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

            this.PageHeaderDefault = "%date% 财产帐簿 -- 来源 %sourcedescription% -- (共 %pagecount% 页)";
            this.PageFooterDefault = "%pageno%/%pagecount%";

            this.TableTitleDefault = "%date% 财产帐簿";

            this.LinesPerPageDefault = 20;

            // 2008/9/5 
            // Columns缺省值
            Columns.Clear();

            // "no -- 序号",
            Column column = new Column();
            column.Name = "no -- 序号";
            column.Caption = "序号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "barcode -- 册条码号"
            column = new Column();
            column.Name = "barcode -- 册条码号";
            column.Caption = "册条码号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "accessNo -- 索取号"
            column = new Column();
            column.Name = "accessNo -- 索取号";
            column.Caption = "索取号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "summary -- 摘要"
            column = new Column();
            column.Name = "summary -- 摘要";
            column.Caption = "摘要";
            column.MaxChars = 15;
            this.Columns.Add(column);

            // "location -- 馆藏地点"
            column = new Column();
            column.Name = "location -- 馆藏地点";
            column.Caption = "馆藏地点";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "price -- 册价格"
            column = new Column();
            column.Name = "price -- 册价格";
            column.Caption = "册价格";
            column.MaxChars = -1;
            this.Columns.Add(column);

            /* 缺省时不要包含这个栏目。
            // "biblioPrice -- 种价格"
            column = new Column();
            column.Name = "biblioPrice -- 种价格";
            column.Caption = "种价格";
            column.MaxChars = -1;
            this.Columns.Add(column);
             * */

            // "biblioRecpath -- 种记录路径"
            column = new Column();
            column.Name = "biblioRecpath -- 种记录路径";
            column.Caption = "种记录路径";
            column.MaxChars = -1;
            this.Columns.Add(column);
        }

        public override void LoadData(ApplicationInfo ai,
    string strPath)
        {
            string strNamePath = strPath;
            if (this.PublicationType != "图书")
                strNamePath = "series_" + strNamePath;
            base.LoadData(ai, strNamePath);
        }

        public override void SaveData(ApplicationInfo ai,
            string strPath)
        {
            string strNamePath = strPath;
            if (this.PublicationType != "图书")
                strNamePath = "series_" + strNamePath;
            base.SaveData(ai, strNamePath);
        }
    }
}