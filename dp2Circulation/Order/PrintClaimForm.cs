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
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Web;   // HttpUtility

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Script;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Text;

using DigitalPlatform.dp2.Statis;

using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 打印催询单窗
    /// </summary>
    public partial class PrintClaimForm : MyForm
    {
        // 装载数据时的方式
        string SourceStyle = "";    // "bibliodatabase" "bibliorecpathfile" "orderdatabase" "orderrecpathfile"

        // 书商名和 OneSeller 对象的对照表
        Hashtable seller_table = new Hashtable();

        /// <summary>
        /// 正在进行催询操作
        /// </summary>
        bool Running = false;

#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;
#endif

        const int WM_LOADSIZE = API.WM_USER + 201;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PrintClaimForm()
        {
            InitializeComponent();
        }

        private void PrintClaimForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

#if NO
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.comboBox_source_type.Text = this.MainForm.AppInfo.GetString(
                "printclaimform",
                "publication_type",
                "连续出版物");

            this.checkBox_source_guess.Checked = this.MainForm.AppInfo.GetBoolean(
    "printclaimform",
    "guess",
    true);

            this.radioButton_inputStyle_biblioRecPathFile.Checked = this.MainForm.AppInfo.GetBoolean(
    "printclaimform",
    "inputstyle_bibliorecpathfile",
    false);


            this.radioButton_inputStyle_biblioDatabase.Checked = this.MainForm.AppInfo.GetBoolean(
                "printclaimform",
                "inputstyle_bibliodatabase",
                true);


            // 输入的记录路径文件名
            this.textBox_inputBiblioRecPathFilename.Text = this.MainForm.AppInfo.GetString(
                "printclaimform",
                "input_recpath_filename",
                "");


            // 输入的书目库名
            this.comboBox_inputBiblioDbName.Text = this.MainForm.AppInfo.GetString(
                "printclaimform",
                "input_bibliodbname",
                "<全部>");

            // 
            this.radioButton_inputStyle_orderRecPathFile.Checked = this.MainForm.AppInfo.GetBoolean(
"printclaimform",
"inputstyle_orderrecpathfile",
false);


            this.radioButton_inputStyle_orderDatabase.Checked = this.MainForm.AppInfo.GetBoolean(
                "printclaimform",
                "inputstyle_orderdatabase",
                false);

            // 输入的订购库记录路径文件名
            this.textBox_inputOrderRecPathFilename.Text = this.MainForm.AppInfo.GetString(
                "printclaimform",
                "input_orderrecpath_filename",
                "");


            // 输入的订购库名
            this.comboBox_inputOrderDbName.Text = this.MainForm.AppInfo.GetString(
                "printclaimform",
                "input_orderdbname",
                "");

            // *** 时间范围页

            this.checkBox_timeRange_usePublishTime.Checked = this.MainForm.AppInfo.GetBoolean(
                "printclaimform",
                "time_range_userPublishTime",
                true);

            this.checkBox_timeRange_useOrderTime.Checked = this.MainForm.AppInfo.GetBoolean(
    "printclaimform",
    "time_range_userOrderTime",
    true);

            this.comboBox_timeRange_afterOrder.Text = this.MainForm.AppInfo.GetString(
    "printclaimform",
    "time_range_afterOrder",
    "");

            this.checkBox_timeRange_none.Checked = this.MainForm.AppInfo.GetBoolean(
    "printclaimform",
    "time_range_none",
    false);

            // 时间范围
            this.textBox_timeRange.Text = this.MainForm.AppInfo.GetString(
                "printclaimform",
                "time_range",
                "");

            SetInputPanelEnabled(true);
            SetTimeRangeState(true);

            comboBox_source_type_SelectedIndexChanged(this, null);

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);
        }

        private void PrintClaimForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void PrintClaimForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif

            this.MainForm.AppInfo.SetString(
                "printclaimform",
                "publication_type",
                this.comboBox_source_type.Text);

            this.MainForm.AppInfo.SetBoolean(
                "printclaimform",
                "guess",
                this.checkBox_source_guess.Checked);

            this.MainForm.AppInfo.SetBoolean(
                "printclaimform",
                "inputstyle_bibliorecpathfile",
                this.radioButton_inputStyle_biblioRecPathFile.Checked);


            this.MainForm.AppInfo.SetBoolean(
                "printclaimform",
                "inputstyle_bibliodatabase",
                this.radioButton_inputStyle_biblioDatabase.Checked);



            // 输入的记录路径文件名
            this.MainForm.AppInfo.SetString(
                "printclaimform",
                "input_recpath_filename",
                this.textBox_inputBiblioRecPathFilename.Text);

            // 输入的书目库名
            this.MainForm.AppInfo.SetString(
                "printclaimform",
                "input_bibliodbname",
                this.comboBox_inputBiblioDbName.Text);

            // 
            this.MainForm.AppInfo.SetBoolean(
"printclaimform",
"inputstyle_orderrecpathfile",
this.radioButton_inputStyle_orderRecPathFile.Checked);


            this.MainForm.AppInfo.SetBoolean(
                "printclaimform",
                "inputstyle_orderdatabase",
                this.radioButton_inputStyle_orderDatabase.Checked);

            // 输入的订购库记录路径文件名
            this.MainForm.AppInfo.SetString(
                "printclaimform",
                "input_orderrecpath_filename",
                this.textBox_inputOrderRecPathFilename.Text);

            // 输入的订购库名
            this.MainForm.AppInfo.SetString(
                "printclaimform",
                "input_orderdbname",
                this.comboBox_inputOrderDbName.Text);

            // *** 时间范围页

            this.MainForm.AppInfo.SetBoolean(
                "printclaimform",
                "time_range_userPublishTime",
                this.checkBox_timeRange_usePublishTime.Checked);

            this.MainForm.AppInfo.SetBoolean(
    "printclaimform",
    "time_range_userOrderTime",
    this.checkBox_timeRange_useOrderTime.Checked);

            this.MainForm.AppInfo.SetString(
    "printclaimform",
    "time_range_afterOrder",
    this.comboBox_timeRange_afterOrder.Text);

            this.MainForm.AppInfo.SetBoolean(
    "printclaimform",
    "time_range_none",
    this.checkBox_timeRange_none.Checked);

            // 时间范围
            this.MainForm.AppInfo.SetString(
                "printclaimform",
                "time_range",
                this.textBox_timeRange.Text);

            SaveSize();
        }

        void LoadSize()
        {
#if NO
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state");
#endif

            string strWidths = this.MainForm.AppInfo.GetString(
                "printclaimform",
                "list_origin_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_origin,
                    strWidths,
                    true);
            }

            strWidths = this.MainForm.AppInfo.GetString(
    "printclaimform",
    "list_merged_width",
    "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_merged,
                    strWidths,
                    true);
            }
        }

        void SaveSize()
        {
#if NO
            MainForm.AppInfo.SaveMdiChildFormStates(this,
                "mdi_form_state");
#endif

            /*
            // 如果MDI子窗口不是MainForm刚刚准备退出时的状态，恢复它。为了记忆尺寸做准备
            if (this.WindowState != this.MainForm.MdiWindowState)
                this.WindowState = this.MainForm.MdiWindowState;
             * */

            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_origin);
            this.MainForm.AppInfo.SetString(
                "printclaimform",
                "list_origin_width",
                strWidths);

            strWidths = ListViewUtil.GetColumnWidthListString(this.listView_merged);
            this.MainForm.AppInfo.SetString(
                "printclaimform",
                "list_merged_width",
                strWidths);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOADSIZE:
                    LoadSize();
                    return;
            }
            base.DefWndProc(ref m);
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

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.comboBox_source_type.Enabled = bEnable;

            SetInputPanelEnabled(bEnable);

            /*
            this.checkBox_timeRange_usePublishTime.Enabled = bEnable;
            this.checkBox_timeRange_useOrderTime.Enabled = bEnable;
            this.checkBox_timeRange_none.Enabled = bEnable;
            this.comboBox_timeRange_afterOrder.Enabled = bEnable;
            this.textBox_timeRange.Enabled = bEnable;
            this.comboBox_timeRange_quickSet.Enabled = bEnable;
             * */
            SetInputPanelEnabled(bEnable);

            this.button_next.Enabled = bEnable;

            this.button_print.Enabled = bEnable;

            this.button_printOption.Enabled = bEnable;
        }

        public void SetBiblioRecPaths(List<string> recpaths)
        {
            this.textBox_inputBiblioRecPathFilename.Text = StringUtil.MakePathList(recpaths) + ","; // 确保至少有一个逗号
            this.radioButton_inputStyle_biblioRecPathFile.Checked = true;
        }

        public void SetOrderRecPaths(List<string> recpaths)
        {
            this.textBox_inputOrderRecPathFilename.Text = StringUtil.MakePathList(recpaths) + ","; // 确保至少有一个逗号
            this.radioButton_inputStyle_orderRecPathFile.Checked = true;
        }

        public PublicationType PublicationType
        {
            get
            {
                if (this.comboBox_source_type.Text == "图书")
                    return PublicationType.Book;
                return PublicationType.Series;
            }
            set
            {
                if (value == PublicationType.Book)
                    this.comboBox_source_type.Text = "图书";
                else
                    this.comboBox_source_type.Text = "连续出版物";
            }
        }

        // 输入风格
        /// <summary>
        /// 输入方式
        /// </summary>
        public PrintClaimInputStyle InputStyle
        {
            get
            {
                if (this.radioButton_inputStyle_biblioRecPathFile.Checked == true)
                    return PrintClaimInputStyle.BiblioRecPathFile;
                else if (this.radioButton_inputStyle_biblioDatabase.Checked == true)
                    return PrintClaimInputStyle.BiblioDatabase;
                else if (this.radioButton_inputStyle_orderRecPathFile.Checked == true)
                    return PrintClaimInputStyle.OrderRecPathFile;
                else
                    return PrintClaimInputStyle.OrderDatabase;
            }
        }

        // 在属性页中输出显示一个文本行
        void WriteTextLines(string strHtml)
        {
            /*
            string[] parts = strHtml.Replace("\r\n", "\n").Split(new char[] {'\n'});
            StringBuilder s = new StringBuilder(4096);
            foreach (string p in parts)
            {
                s.Append(HttpUtility.HtmlEncode(p) + "\r\n");
            }
            Global.WriteHtml(this.webBrowser_errorInfo,
                s.ToString());
             * */
            Global.WriteHtml(this.webBrowser_errorInfo,
                HttpUtility.HtmlEncode(strHtml));
        }

        // 在属性页中输出显示一个 HTML 字符串
        void WriteHtml(string strHtml)
        {
            Global.WriteHtml(this.webBrowser_errorInfo,
                strHtml);
        }

        static void OldSetHtmlString(WebBrowser webBrowser,
            string strHtml)
        {
            // 警告 这样调用，不会自动<body onload='...'>事件
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
                Debug.Assert(doc != null, "doc不应该为null");
            }

            doc = doc.OpenNew(true);
            doc.Write(strHtml);
        }

        // 2012//30
        // 
        /// <summary>
        /// 获得一条书目记录下属的全部订购记录路径
        /// </summary>
        /// <param name="sw">姚写入的 StreamWriter 对象</param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错；>=0: 返回实际写入的路径个数</returns>
        int GetChildOrderRedPath(StreamWriter sw,
            string strBiblioRecPath,
            out string strError)
        {
            strError = "";

            long lStart = 0;
            long lResultCount = 0;
            long lCount = -1;

            for (; ; )
            {
                EntityInfo[] orders = null;

                long lRet = Channel.GetOrders(
                    stop,
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    "onlygetpath",
                    this.Lang,
                    out orders,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                    return 0;

                lResultCount = lRet;

                Debug.Assert(orders != null, "");

                for (int i = 0; i < orders.Length; i++)
                {
                    if (orders[i].ErrorCode != ErrorCodeValue.NoError)
                    {
                        strError = "路径为 '" + orders[i].OldRecPath + "' 的订购记录装载中发生错误: " + orders[i].ErrorInfo;  // NewRecPath
                        return -1;
                    }

                    sw.Write(orders[i].OldRecPath + "\r\n");
                }

                lStart += orders.Length;
                if (lStart >= lResultCount)
                    break;
            }

            return (int)lStart;
        }

        // 2012/8/30
        // 检索订购库，将订购记录路径输出到文件
        int SearchOrderRecPath(
            string strRecPathFilename,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(this.comboBox_inputOrderDbName.Text) == true)
            {
                strError = "尚未指定订购库名";
                return -1;
            }

            // 创建文件
            StreamWriter sw = new StreamWriter(strRecPathFilename,
                false,	// append
                System.Text.Encoding.UTF8);

            try
            {

                long lRet = 0;

                lRet = Channel.SearchOrder(stop,
                     this.comboBox_inputOrderDbName.Text,
                     "",
                     -1,    // nPerMax
                     "__id",
                     "left",
                     this.Lang,
                     null,   // strResultSetName
                     "",    // strSearchStyle
                     "", // strOutputStyle
                     out strError);
                if (lRet == -1)
                    goto ERROR1;
                long lHitCount = lRet;

                long lStart = 0;
                long lCount = lHitCount;

                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    lRet = Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        "id",   // "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        goto ERROR1;
                    }

                    Debug.Assert(searchresults != null, "");

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        string strOrderRecPath = searchresults[i].Path;

                        sw.WriteLine(strOrderRecPath);
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共有记录 " + lHitCount.ToString() + " 个。已获得记录 " + lStart.ToString() + " 个");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 2012/8/30
        // 根据书目库记录路径文件，得到下属的订购库记录路径
        // parameters:
        //      strBiblioRecPathFilename    书目库记录路径文件名。也可以是逗号间隔的书目库记录路径列表
        int GetOrderRecPath(string strBiblioRecPathFilename,
            string strOutputFilename,
            out string strError)
        {
            strError = "";

            // 2015/1/28
            string strTempFileName = "";
            if (strBiblioRecPathFilename.IndexOf(",") != -1)
            {
                // 将内容先写入一个临时文件
                strTempFileName = this.MainForm.GetTempFileName("pcf_");

                using (StreamWriter sw = new StreamWriter(strTempFileName, false, Encoding.UTF8))
                {
                    sw.Write(strBiblioRecPathFilename.Replace(",", "\r\n"));
                }
                strBiblioRecPathFilename = strTempFileName;
            }

            // 创建文件
            using (StreamWriter sw = new StreamWriter(strOutputFilename,
                false,	// append
                System.Text.Encoding.UTF8))
            {

                try
                {
                    using (StreamReader sr = new StreamReader(strBiblioRecPathFilename, Encoding.UTF8))
                    {
                        for (; ; )
                        {
                            string strBiblioRecPath = sr.ReadLine();

                            if (strBiblioRecPath == null)
                                break;
                            strBiblioRecPath = strBiblioRecPath.Trim();
                            if (String.IsNullOrEmpty(strBiblioRecPath) == true)
                                continue;

                            // 检查书目库路径
                            {
                                string strDbName = Global.GetDbName(strBiblioRecPath);
                                BiblioDbProperty prop = this.MainForm.GetBiblioDbProperty(strDbName);

                                if (prop == null)
                                {
                                    strError = "记录路径 '" + strBiblioRecPath + "' 中的数据库名 '" + strDbName + "' 不是书目库名";
                                    return -1;
                                }

                                if (string.IsNullOrEmpty(prop.IssueDbName) == false)
                                {
                                    // 期刊库
                                    if (this.comboBox_source_type.Text != "连续出版物")
                                    {
                                        strError = "记录路径 '" + strBiblioRecPath + "' 中的书目库名 '" + strDbName + "' 不是图书类型";
                                        return -1;
                                    }
                                }
                                else
                                {
                                    // 图书库
                                    if (this.comboBox_source_type.Text != "图书")
                                    {
                                        strError = "记录路径 '" + strBiblioRecPath + "' 中的书目库名 '" + strDbName + "' 不是期刊类型";
                                        return -1;
                                    }
                                }
                            }

                            // 获得一条书目记录下属的全部订购记录路径
                            int nRet = GetChildOrderRedPath(sw,
                                strBiblioRecPath,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }

                    }
                }
                catch (Exception ex)
                {
                    strError = "处理文件 " + strBiblioRecPathFilename + " 时出错: " + ex.Message;
                    return -1;
                }
            }

            if (string.IsNullOrEmpty(strTempFileName) == false)
                File.Delete(strTempFileName);

            return 0;
        }

        // 2012/8/3-
        // 检索书目库， 获得特定批次号，或者所有书目记录，然后将下属的订购记录路径输出到文件
        int SearchBiblioOrderRecPath(
            string strBatchNo,
            string strRecPathFilename,
            out string strError)
        {
            strError = "";

#if NO
            string strDbNameList = GetBiblioDbNameList();

            if (String.IsNullOrEmpty(strDbNameList) == true)
            {
                if (this.comboBox_inputBiblioDbName.Text == "<全部>"
                    || this.comboBox_inputBiblioDbName.Text.ToLower() == "<all>"
                    || this.comboBox_inputBiblioDbName.Text == "")
                {
                    strError = "出版物类型 '" + this.comboBox_source_type.Text + "' 不存在匹配的书目库";
                    return -1;
                }

                strError = "尚未指定书目库名";
                return -1;
            }
#endif
            if (String.IsNullOrEmpty(this.comboBox_inputBiblioDbName.Text) == true)
            {
                strError = "尚未指定书目库名";
                return -1;
            }

            if (this.comboBox_inputBiblioDbName.Text == "<全部>")
            {
                if (this.comboBox_source_type.Text == "图书")
                    this.comboBox_inputBiblioDbName.Text = "<全部图书>";
                else
                    this.comboBox_inputBiblioDbName.Text = "<全部期刊>";
            }

            // 创建文件
            StreamWriter sw = new StreamWriter(strRecPathFilename,
                false,	// append
                System.Text.Encoding.UTF8);

            try
            {

                long lRet = 0;
                string strQueryXml = "";

                // 不指定批次号，意味着特定库全部条码
                if (String.IsNullOrEmpty(strBatchNo) == true)
                {
                    lRet = Channel.SearchBiblio(stop,
                         this.comboBox_inputBiblioDbName.Text,
                         "",
                         -1,    // nPerMax
                         "recid",
                         "left",
                         this.Lang,
                         null,   // strResultSetName
                         "",    // strSearchStyle
                         "", // strOutputStyle
                         out strQueryXml,
                         out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                else
                {
                    // 指定批次号。特定库。
                    lRet = Channel.SearchBiblio(stop,
                         this.comboBox_inputBiblioDbName.Text,
                         strBatchNo,
                         -1,    // nPerMax
                         "batchno",
                         "exact",
                         this.Lang,
                         null,   // strResultSetName
                         "",    // strSearchStyle
                         "", // strOutputStyle
                         out strQueryXml,
                         out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                long lHitCount = lRet;

                long lStart = 0;
                long lCount = lHitCount;


                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }


                    lRet = Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        "id",   // "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        goto ERROR1;
                    }

                    Debug.Assert(searchresults != null, "");

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        string strBiblioRecPath = searchresults[i].Path;
                        int nRet = GetChildOrderRedPath(sw,
                            strBiblioRecPath,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共有记录 " + lHitCount.ToString() + " 个。已获得记录 " + lStart.ToString() + " 个");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 2012/8/30
        // 构造订购库记录路径文件
        // return:
        //      0   普通返回
        //      1   要全部中断
        int MakeOrderRecPathFile(
            out string strTempRecPathFilename,
            out string strError)
        {
            strError = "";
            strTempRecPathFilename = "";

            int nRet = 0;

            // TODO: 临时文件是否删除?
            // 记录路径临时文件
            strTempRecPathFilename = this.MainForm.GetTempFileName("pcf_");

            // 整个书目库
            if (this.InputStyle == PrintClaimInputStyle.BiblioDatabase)
            {
                this.SourceStyle = "bibliodatabase";

                nRet = SearchBiblioOrderRecPath(
                    "", // this.tabComboBox_inputBatchNo.Text,
                    strTempRecPathFilename,
                    out strError);
                if (nRet == -1)
                    return -1;
                // strAccessPointName = "记录路径";
            }
            else if (this.InputStyle == PrintClaimInputStyle.BiblioRecPathFile)
            {
                this.SourceStyle = "bibliorecpathfile";

                // 根据书目库记录路径文件，得到下属的订购库记录路径
                nRet = GetOrderRecPath(this.textBox_inputBiblioRecPathFilename.Text,
                    strTempRecPathFilename,
                    out strError);
                if (nRet == -1)
                    return -1;

                // strAccessPointName = "记录路径";
            }
            else if (this.InputStyle == PrintClaimInputStyle.OrderDatabase)
            {
                this.SourceStyle = "orderdatabase";

                // 检索订购库，将订购记录路径输出到文件
                nRet = SearchOrderRecPath(
                    strTempRecPathFilename,
                    out strError);
                if (nRet == -1)
                    return -1;

            }
            else if (this.InputStyle == PrintClaimInputStyle.OrderRecPathFile)
            {
                strError = "本函数不能用于处理此情况";
                return -1;
            }
            else
            {
                Debug.Assert(false, "");
            }

            return 0;
        }

        // 准备时间过滤参数
        int PrepareTimeFilter(out TimeFilter filter,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            filter = new TimeFilter();

            try
            {
                filter.OrderTimeDelta = GetOrderTimeDelta(this.comboBox_timeRange_afterOrder.Text);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            if (this.checkBox_timeRange_none.Checked == true)
            {
                filter.Style = "none";
                return 0;
            }

            if (this.checkBox_timeRange_useOrderTime.Checked == false
                && this.checkBox_timeRange_usePublishTime.Checked == false)
            {
                strError = "要求出版时间 和 要求订购时间落入指定范围，需要至少选定一项(除非选定了“不过滤”)";
                return -1;
            }

            if (string.IsNullOrEmpty(this.textBox_timeRange.Text) == true)
            {
                strError = "尚未设定时间范围值";
                return -1;
            }

            if (checkBox_timeRange_usePublishTime.Checked == true
                && this.checkBox_timeRange_useOrderTime.Checked == true)
                filter.Style = "both";
            else if (checkBox_timeRange_usePublishTime.Checked == true
                && this.checkBox_timeRange_useOrderTime.Checked == false)
                filter.Style = "publishtime";
            else if (checkBox_timeRange_usePublishTime.Checked == false
    && this.checkBox_timeRange_useOrderTime.Checked == true)
                filter.Style = "ordertime";
            else
            {
                Debug.Assert(false, "");
            }

            // 缺省效果是永远的过去-今天现在
            DateTime startTime = new DateTime(0);
            DateTime endTime = DateTime.Now;

            nRet = Global.ParseTimeRangeString(this.textBox_timeRange.Text,
                true,
                out startTime,
                out endTime,
                out strError);
            if (nRet == -1)
                return -1;

            filter.StartTime = startTime;
            filter.EndTime = endTime;

            return 0;
        }

        // 获得数字值
        static double GetValue(string strValue)
        {
            if (strValue == "一")
                return 1;
            if (strValue == "二")
                return 2;
            if (strValue == "两")
                return 2;
            if (strValue == "三")
                return 3;
            if (strValue == "四")
                return 4;
            if (strValue == "五")
                return 5;
            if (strValue == "六")
                return 6;
            if (strValue == "七")
                return 7;
            if (strValue == "八")
                return 8;
            if (strValue == "九")
                return 9;
            if (strValue == "十")
                return 10;
            if (strValue == "零")
                return 0;
            if (strValue == "半")
                return 0.5;

            double v = 0;
            if (double.TryParse(strValue, out v) == false)
                throw new Exception("数字 '" + strValue + "' 格式错误");

            return v;
        }

        static TimeSpan GetOrderTimeDelta(string strNameParam)
        {
            if (string.IsNullOrEmpty(strNameParam) == true)
                return new TimeSpan();

            if (strNameParam == "立即")
                return new TimeSpan();

            string strName = strNameParam.Replace("后", "").Trim();
            strName = strName.Replace("个", "").Trim();

            if (strName.IndexOf("年") != -1)
            {
                string strNumber = strName.Replace("年", "").Trim();

                double v = GetValue(strNumber);

                return new TimeSpan((int)((double)365 * v), 0, 0, 0);
            }

            if (strName.IndexOf("月") != -1)
            {
                string strNumber = strName.Replace("月", "").Trim();

                double v = GetValue(strNumber);

                if (v >= 12)
                {
                    v = ((v / (double)12) * (double)365) + (v % (double)12) * (double)30.5;
                }
                else
                {
                    v = v * (double)30.5;
                }

                return new TimeSpan((int)v, 0, 0, 0);

            }
            if (strName.IndexOf("日") != -1)
            {
                string strNumber = strName.Replace("日", "").Trim();

                double v = GetValue(strNumber);

                return new TimeSpan((int)(v * 24), 0, 0);

            }
            if (strName.IndexOf("周") != -1)
            {
                string strNumber = strName.Replace("周", "").Trim();

                double v = GetValue(strNumber);

                return new TimeSpan((int)((double)7 * v), 0, 0, 0);
            }

            throw new Exception("无法识别的时间长度 '" + strNameParam + "'");
        }

#if NO
        static TimeSpan GetOrderTimeDelta(string strName)
        {
            if (string.IsNullOrEmpty(strName) == true)
                return new TimeSpan();

            // 立即
            if (strName == "立即")
                return new TimeSpan();

            // 一周后
            if (strName == "一周后")
                return new TimeSpan(7, 0, 0, 0);

            if (strName == "半年后")
                return new TimeSpan(182, 0, 0, 0);

            if (strName == "一年后")
                return new TimeSpan(365, 0, 0, 0);

            if (strName == "两年后")
                return new TimeSpan(2 * 365, 0, 0, 0);
            if (strName == "三年后")
                return new TimeSpan(3 * 365, 0, 0, 0);

            if (strName == "四年后")
                return new TimeSpan(4 * 365, 0, 0, 0);

            throw new Exception("不能识别的时间长度 '" + strName + "'");

        }
#endif

        // 对每个书目记录进行循环
        // return:
        //      0   普通返回
        //      1   要全部中断
        int DoLoop(out string strError,
            out string strWarning)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;
            // long lRet = 0;

            // bool bSyntaxWarned = false;
            // bool bFilterWarned = false;

            this.seller_table.Clear();

#if NO
            // 缺省效果是永远的过去-今天现在
            DateTime startTime = new DateTime(0);
            DateTime endTime = DateTime.Now;

            if (this.textBox_timeRange.Text != "")
            {
                nRet = Global.ParseTimeRangeString(this.textBox_timeRange.Text,
                    out startTime,
                    out endTime,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
#endif
            TimeFilter filter = null;
            // 准备时间过滤参数
            nRet = PrepareTimeFilter(out filter,
                out strError);
            if (nRet == -1)
            {
                strError = "时间范围设置不正确: " + strError;
                return -1;
            }

            // 清除错误信息窗口中残余的内容
            OldSetHtmlString(this.webBrowser_errorInfo, "<pre>");

            // 记录路径临时文件
            string strTempRecPathFilename = "";
            string strInputFilename = "";

            // string strInputFileName = "";   // 外部指定的输入文件，为条码号文件或者记录路径文件格式
            string strAccessPointName = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                if (this.InputStyle == PrintClaimInputStyle.OrderRecPathFile)
                {
                    this.SourceStyle = "orderrecpathfile";
                    if (this.textBox_inputOrderRecPathFilename.Text.IndexOf(",") == -1)
                        strInputFilename = this.textBox_inputOrderRecPathFilename.Text;
                    else
                    {
                        // 将内容先写入一个临时文件
                        strTempRecPathFilename = this.MainForm.GetTempFileName("pcf_");

                        using (StreamWriter sw = new StreamWriter(strTempRecPathFilename, false, Encoding.UTF8))
                        {
                            sw.Write(this.textBox_inputOrderRecPathFilename.Text.Replace(",", "\r\n"));
                        }
                        strInputFilename = strTempRecPathFilename;
                    }
                }
                else
                {
                    nRet = MakeOrderRecPathFile(
            out strTempRecPathFilename,
            out strError);
                    if (nRet == -1)
                        return -1;
                    strInputFilename = strTempRecPathFilename;
                }

                StreamReader sr = null;

                try
                {
                    sr = new StreamReader(strInputFilename, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    strError = "打开文件 " + strInputFilename + " 失败: " + ex.Message;
                    return -1;
                }

                IssueHost issue_host = null;
                BookHost order_host = null;

                if (this.comboBox_source_type.Text == "连续出版物"
                    || this.comboBox_source_type.Text == "期刊")
                {
                    issue_host = new IssueHost();
                    issue_host.Channel = this.Channel;
                    issue_host.Stop = this.stop;
                }
                else
                {
                    order_host = new BookHost();
                    order_host.Channel = this.Channel;
                    order_host.Stop = this.stop;
                }

                /*
                this.progressBar_records.Minimum = 0;
                this.progressBar_records.Maximum = (int)sr.BaseStream.Length;
                this.progressBar_records.Value = 0;
                 * */

                stop.SetProgressRange(0, sr.BaseStream.Length);

                try
                {
                    // int nCount = 0;

                    for (int nRecord=0; ;nRecord++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                DialogResult result = MessageBox.Show(this,
                                    "准备中断。\r\n\r\n确实要中断全部操作? (Yes 全部中断；No 中断循环，但是继续收尾处理；Cancel 放弃中断，继续操作)",
                                    "bibliostatisform",
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button3);

                                if (result == DialogResult.Yes)
                                {
                                    strError = "用户中断";
                                    return -1;
                                }
                                if (result == DialogResult.No)
                                    return 0;   // 假装loop正常结束

                                stop.Continue(); // 继续循环
                            }
                        }

                        string strOrderRecPath = sr.ReadLine();

                        if (strOrderRecPath == null)
                            break;

                        strOrderRecPath = strOrderRecPath.Trim();

                        if (String.IsNullOrEmpty(strOrderRecPath) == true)
                            continue;

                        if (strOrderRecPath[0] == '#')
                            continue;   // 注释行

                        // 检查订购库路径
                        {
                            string strDbName = Global.GetDbName(strOrderRecPath);
                            string strBiblioDbName = this.MainForm.GetBiblioDbNameFromOrderDbName(strDbName);
                            if (string.IsNullOrEmpty(strBiblioDbName) == true)
                            {
                                strError = "记录路径 '" + strOrderRecPath + "' 中的数据库名 '" + strDbName + "' 不是订购库名";
                                return -1;
                            }
                            BiblioDbProperty prop = this.MainForm.GetBiblioDbProperty(strBiblioDbName);
                            if (prop == null)
                            {
                                strError = "数据库名 '" + strBiblioDbName + "' 不是书目库名";
                                return -1;
                            }

                            if (string.IsNullOrEmpty(prop.IssueDbName) == false)
                            {
                                // 期刊库
                                if (this.comboBox_source_type.Text != "连续出版物")
                                {
                                    strError = "记录路径 '" + strOrderRecPath + "' 中的订购库名 '" + strDbName + "' 不是图书类型";
                                    return -1;
                                }
                            }
                            else
                            {
                                // 图书库
                                if (this.comboBox_source_type.Text != "图书")
                                {
                                    strError = "记录路径 '" + strOrderRecPath + "' 中的订购库名 '" + strDbName + "' 不是期刊类型";
                                    return -1;
                                }
                            }
                        }

                        stop.SetMessage("正在获取第 " + (nRecord + 1).ToString() + " 个订购记录，" + strAccessPointName + "为 " + strOrderRecPath);

                        stop.SetProgressValue(sr.BaseStream.Position);
                        // this.progressBar_records.Value = (int)sr.BaseStream.Position;

                        // 获得书目记录
                        // string strOutputRecPath = "";
                        // byte[] baTimestamp = null;

#if NO
                        string strAccessPoint = "";
                        if (this.InputStyle == PrintClaimInputStyle.BiblioDatabase)
                            strAccessPoint = strOrderRecPath;
                        else if (this.InputStyle == PrintClaimInputStyle.BiblioRecPathFile)
                            strAccessPoint = strOrderRecPath;
                        else
                        {
                            Debug.Assert(false, "");
                        }
#endif

#if NO
                        string strBiblio = "";
                        // string strBiblioRecPath = "";

                        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                        lRet = Channel.GetBiblioInfo(
                            stop,
                            strAccessPoint,
                            "", // strBiblioXml
                            "xml",   // strResultType
                            out strBiblio,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "获得书目记录 " + strAccessPoint + " 时发生错误: " + strError;
                            this.WriteHtml(strError + "\r\n");
                            continue;
                        }

                        if (lRet == 0)
                        {
                            strError = "书目记录" + strAccessPointName + " " + strOrderRecPath + " 对应的XML数据没有找到。";
                            this.WriteHtml(strError + "\r\n");
                            continue;
                        }

                        if (lRet > 1)
                        {
                            strError = "书目记录" + strAccessPointName + " " + strOrderRecPath + " 对应数据多于一条。";
                            this.WriteHtml(strError + "\r\n");
                            continue;
                        }

                        string strXml = "";

                        strXml = strBiblio;


                        // 看看是否在希望统计的范围内
                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "书目记录装入DOM发生错误: " + ex.Message;
                            continue;
                        }

                        // strXml中为书目记录
                        string strBiblioDbName = Global.GetDbName(strOrderRecPath);

                        string strSyntax = this.MainForm.GetBiblioSyntax(strBiblioDbName);
                        if (String.IsNullOrEmpty(strSyntax) == true)
                            strSyntax = "unimarc";

                        if (strSyntax == "usmarc" || strSyntax == "unimarc")
                        {
                            // 将XML书目记录转换为MARC格式
                            string strOutMarcSyntax = "";
                            string strMarc = "";

                            // 将MARCXML格式的xml记录转换为marc机内格式字符串
                            // parameters:
                            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                            nRet = MarcUtil.Xml2Marc(strXml,
                                false,
                                "", // strMarcSyntax
                                out strOutMarcSyntax,
                                out strMarc,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            if (String.IsNullOrEmpty(strOutMarcSyntax) == false)
                            {
                                if (strOutMarcSyntax != strSyntax
                                    && bSyntaxWarned == false)
                                {
                                    strWarning += "书目记录 " + strOrderRecPath + " 的syntax '" + strOutMarcSyntax + "' 和其所属数据库 '" + strBiblioDbName + "' 的定义syntax '" + strSyntax + "' 不一致\r\n";
                                    bSyntaxWarned = true;
                                }
                            }


                        }
                        else
                        {
                            // 不是MARC格式
                        }
#endif

                        if (this.comboBox_source_type.Text == "连续出版物"
                            || this.comboBox_source_type.Text == "期刊")
                        {
                            // 处理期刊
                            // return:
                            //      0   未处理
                            //      1   已处理
                            nRet = ProcessIssues(
                                issue_host,
                                filter,
                                strOrderRecPath);
                        }
                        else
                        {
                            // 处理图书
                            // return:
                            //      0   未处理
                            //      1   已处理
                            nRet = ProcessBooks(
                                order_host,
                                filter,
                                strOrderRecPath);
                        }

                        if (nRet == 0)
                            continue;

                        /*
                        // 处理一条书目记录以及其下的订购、期记录
                        nRet = host.LoadIssueRecords(strRecPath,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "在IssueHost中装入记录 " + strRecPath + " 的下属期记录时发生错误:" + strError;
                            this.WriteHtml(strError + "\r\n");
                            continue;
                        }

                        nRet = host.LoadOrderRecords(strRecPath,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "在IssueHost中装入记录 " + strRecPath + " 的下属订购记录时发生错误: " + strError;
                            this.WriteHtml(strError + "\r\n");
                            continue;
                        }

                        nRet = host.CreateIssues(out strError);
                        if (nRet == -1)
                        {
                            strError = "在IssueHost中CreateIssues() " + strRecPath + " error: " + strError;
                            this.WriteHtml(strError + "\r\n");
                            continue;
                        }

                        if (nRet != 0)
                        {
                            this.WriteHtml(host.BiblioRecPath + "\r\n" + host.DumpIssue() + "\r\n");
                        }
                         * */
                        /*
                        CONTINUE:
                        nCount++;
                         * */
                    }

                }
                finally
                {

                    if (sr != null)
                        sr.Close();
                }
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                if (string.IsNullOrEmpty(strTempRecPathFilename) == false)
                    File.Delete(strTempRecPathFilename);
            }

            return 0;
        }

        // return:
        //      0   未处理
        //      1   已处理
        int ProcessIssues(
            IssueHost issue_host,
            TimeFilter filter,
            string strOrderRecPath)
        {
            string strError = "";

            this.WriteTextLines("*** " + strOrderRecPath + " ");

            long lRet = 0;

            StringBuilder debugInfo = null;
            if (this.checkBox_debug.Checked == true)
                debugInfo = new StringBuilder();

            // 初始化控件
            // return:
            //      -1  error
            //      0   没有订购信息
            //      1   初始化成功
            int nRet = issue_host.Initial(strOrderRecPath,
                this.checkBox_source_guess.Checked,
                ref debugInfo,
                out strError);
            if (nRet == -1)
            {
                this.WriteTextLines(strError + "\r\n");
                return 0;
            }

            if (debugInfo != null)
                this.WriteTextLines(debugInfo.ToString());

            if (nRet == 0)
            {
                if (string.IsNullOrEmpty(strError) == false)
                    this.WriteTextLines(strError + "\r\n");
                return 0;
            }

            List<IssueInfo> issue_infos = null;
            // 获得期各种信息
            // 每期一行，按照书商名进行了汇总
            // return:
            //      -1  error
            //      0   没有任何信息
            //      >0  信息个数
            nRet = issue_host.GetIssueInfo(
                out issue_infos,
                out strError);
            if (nRet == -1)
            {
                this.WriteTextLines(strError + "\r\n");
                return 0;
            }

            if (nRet != 0)
            {
                this.WriteTextLines("从属于 " + issue_host.BiblioRecPath + "\r\n" + IssueHost.DumpIssueInfos(issue_infos) + "\r\n");

                // 将IssueInfo数组中处在指定时间范围以外的行移除
                string strDebugInfo = "";
                IssueHost.RemoveOutofTimeRangeIssueInfos(ref issue_infos,
                    filter,
                    out strDebugInfo);
                this.WriteTextLines(strDebugInfo + "\r\n");

                // 去除issue_infos中已经到齐的行
                IssueHost.RemoveArrivedIssueInfos(ref issue_infos);
                if (issue_infos.Count > 0)
                {
                    string strSummary = "";
                    string strISBnISSN = "";
                    string strTitle = "";

                    {
                        string[] formats = new string[3];
                        formats[0] = "summary";
                        formats[1] = "@isbnissn";
                        formats[2] = "@title";

                        string[] results = null;
                        byte[] timestamp = null;

                        Debug.Assert(String.IsNullOrEmpty(strOrderRecPath) == false, "strRecPath值不能为空");

                        lRet = Channel.GetBiblioInfos(
                            stop,
                            issue_host.BiblioRecPath, // strOrderRecPath,
                            "",
                            formats,
                            out results,
                            out timestamp,
                            out strError);
                        if (lRet == -1 || lRet == 0)
                        {
                            if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                                strError = "书目记录 '" + issue_host.BiblioRecPath + "' 不存在";

                            strSummary = "获得书目摘要时发生错误: " + strError;
                        }
                        else
                        {
                            Debug.Assert(results != null && results.Length == 3, "results必须包含3个元素");
                            strSummary = results[0];
                            strISBnISSN = results[1];
                            strTitle = results[2];
                        }
                    }

                    List<NamedIssueInfoCollection> series_results = null;
                    // 将IssueInfo数组排序后按照书商名拆分为独立的数组
                    series_results = IssueHost.SortIssueInfo(issue_infos);

                    for (int i = 0; i < series_results.Count; i++)
                    {
                        NamedIssueInfoCollection list = series_results[i];

                        string strAddressXml = "";

                        // 获得书商地址
                        if (list.Seller == "直订"
                            || list.Seller == "交换"
                            || list.Seller == "赠")
                        {
                            strAddressXml = issue_host.GetAddressXml(list.Seller);

                            if (String.IsNullOrEmpty(strAddressXml) == true)
                            {
                                this.WriteTextLines("*** 特殊渠道 " + list.Seller + " 未能在订购信息中发现书商地址\r\n");
                                return 1;
                            }

                            string strSellerName = "";

                            nRet = BuildSellerName(
                                list.Seller,
                                strTitle,
                                strAddressXml,
                                out strSellerName,
                                out strError);

                            list.Seller = strSellerName;
                        }


                        OneSeries series = new OneSeries();
                        series.BiblioRecPath = issue_host.BiblioRecPath;    //  strOrderRecPath;
                        series.BiblioSummary = strSummary;
                        series.ISSN = strISBnISSN;
                        series.Title = strTitle;
                        series.IssueInfos.AddRange(list);

                        OneSeller seller = GetOneSeller(list.Seller);

                        OneSeries exist_series = seller.FindOneSeries(series.BiblioRecPath);
                        if (exist_series == null)
                            seller.Add(series);
                        else
                            exist_series.MergeIssueInfos(series);

                        // 首次获得书商地址
                        if (String.IsNullOrEmpty(seller.AddressXml) == true)
                            seller.AddressXml = strAddressXml;
                    }
                }
            }

            return 1;
        }

        // return:
        //      0   未处理
        //      1   已处理
        int ProcessBooks(
            BookHost book_host,
            TimeFilter filter,
            string strOrderRecPath)
        {
            string strError = "";

            this.WriteTextLines("*** " + strOrderRecPath + " ");

            long lRet = 0;

            StringBuilder debugInfo = null;
            if (this.checkBox_debug.Checked == true)
                debugInfo = new StringBuilder();

            // 初始化控件
            // return:
            //      -1  error
            //      0   没有订购信息
            //      1   初始化成功
            int nRet = book_host.Initial(strOrderRecPath,
                out strError);
            if (nRet == -1)
            {
                this.WriteTextLines(strError + "\r\n");
                return 0;
            }

            if (nRet == 0)
            {
                if (string.IsNullOrEmpty(strError) == false)
                    this.WriteTextLines(strError + "\r\n");
                return 0;
            }

            List<IssueInfo> issue_infos = null;
            string strWarning = "";
            // 获得期各种信息
            // 每期一行，按照书商名进行了汇总
            // return:
            //      -1  error
            //      0   没有任何信息
            //      >0  信息个数
            nRet = book_host.GetOrderInfo(
                filter,
                out issue_infos,
                out strError,
                out strWarning);
            if (nRet == -1)
            {
                this.WriteTextLines(strError + "\r\n");
                return 0;
            }

            if (string.IsNullOrEmpty(strWarning) == false)
            {
                this.WriteTextLines("警告: " + strWarning + "\r\n");
            }

            if (nRet != 0)
            {
                this.WriteTextLines("从属于 " + book_host.BiblioRecPath + "\r\n" + BookHost.DumpOrderInfos(issue_infos) + "\r\n");

                // 将IssueInfo数组中处在指定时间范围以外的行移除
                string strDebugInfo = "";
                BookHost.RemoveOutofTimeRangeOrderInfos(ref issue_infos,
                    filter,
                    out strDebugInfo);
                this.WriteTextLines(strDebugInfo + "\r\n");

                // 去除issue_infos中已经到齐的行
                BookHost.RemoveArrivedOrderInfos(ref issue_infos);
                if (issue_infos.Count > 0)
                {

                    string strSummary = "";
                    string strISBnISSN = "";
                    string strTitle = "";

                    {
                        string[] formats = new string[3];
                        formats[0] = "summary";
                        formats[1] = "@isbnissn";
                        formats[2] = "@title";

                        string[] results = null;
                        byte[] timestamp = null;

                        Debug.Assert(String.IsNullOrEmpty(book_host.BiblioRecPath) == false, "book_host.BiblioRecPath值不能为空");

                        lRet = Channel.GetBiblioInfos(
                            stop,
                            book_host.BiblioRecPath,    // strOrderRecPath,
                            "",
                            formats,
                            out results,
                            out timestamp,
                            out strError);
                        if (lRet == -1 || lRet == 0)
                        {
                            if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                                strError = "书目记录 '" + book_host.BiblioRecPath + "' 不存在";

                            strSummary = "获得书目摘要时发生错误: " + strError;
                        }
                        else
                        {
                            Debug.Assert(results != null && results.Length == 3, "results必须包含3个元素");
                            strSummary = results[0];
                            strISBnISSN = results[1];
                            strTitle = results[2];
                        }
                    }

                    List<NamedIssueInfoCollection> series_results = null;
                    // 将IssueInfo数组排序后按照书商名拆分为独立的数组
                    series_results = BookHost.SortOrderInfo(issue_infos);

                    for (int i = 0; i < series_results.Count; i++)
                    {
                        NamedIssueInfoCollection list = series_results[i];

                        string strAddressXml = "";

                        // 获得书商地址
                        if (list.Seller == "直订"
                            || list.Seller == "交换"
                            || list.Seller == "赠")
                        {
                            strAddressXml = book_host.GetAddressXml(list.Seller);

                            if (String.IsNullOrEmpty(strAddressXml) == true)
                            {
                                this.WriteTextLines("*** 特殊渠道 " + list.Seller + " 未能在订购信息中发现书商地址\r\n");
                                return 1;
                            }

                            string strSellerName = "";

                            nRet = BuildSellerName(
                                list.Seller,
                                strTitle,
                                strAddressXml,
                                out strSellerName,
                                out strError);

                            list.Seller = strSellerName;
                        }


                        OneSeries series = new OneSeries();
                        series.BiblioRecPath = book_host.BiblioRecPath; // strOrderRecPath;
                        series.BiblioSummary = strSummary;
                        series.ISSN = strISBnISSN;
                        series.Title = strTitle;
                        series.IssueInfos.AddRange(list);

                        OneSeller seller = GetOneSeller(list.Seller);
                        OneSeries exist_series = seller.FindOneSeries(series.BiblioRecPath);
                        if (exist_series == null)
                            seller.Add(series);
                        else
                            exist_series.MergeIssueInfos(series);

                        // 首次获得书商地址
                        if (String.IsNullOrEmpty(seller.AddressXml) == true)
                            seller.AddressXml = strAddressXml;
                    }
                }
            }

            return 1;
        }

        // parameters:
        //      strStyle    渠道名 为 直订/交换/赠
        //      strTitle    期刊名
        static int BuildSellerName(
            string strStyle,
            string strTitle,
            string strAddressXml,
            out string strSellerName,
            out string strError)
        {
            strError = "";
            strSellerName = "";

            if (String.IsNullOrEmpty(strAddressXml) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root/>");
            try
            {
                dom.DocumentElement.InnerXml = strAddressXml;
            }
            catch (Exception ex)
            {
                strError = "地址XML格式不正确: " + ex.Message;
                return -1;
            }

            string strZipcode = DomUtil.GetElementText(dom.DocumentElement,
                "zipcode");
            string strAddress = DomUtil.GetElementText(dom.DocumentElement,
                "address");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement,
                "department");
            string strName = DomUtil.GetElementText(dom.DocumentElement,
                "name");

            if (strStyle == "直订")
            {
                if (String.IsNullOrEmpty(strDepartment) == false)
                    strSellerName = strDepartment
                        + "|" + strZipcode
                        + "|" + strAddress
                        + "|" + strDepartment;
                else
                    strSellerName = "《" + strTitle + "》编辑部"
                        + "|" + strZipcode
                        + "|" + strAddress
                        + "|" + strDepartment;

            }
            else if (strStyle == "交换")
            {
                if (String.IsNullOrEmpty(strDepartment) == false)
                    strSellerName = strDepartment
                        + "|" + strZipcode
                        + "|" + strAddress
                        + "|" + strDepartment;
                else
                    strSellerName = "《" + strTitle + "》提供者"
                        + "|" + strZipcode
                        + "|" + strAddress
                        + "|" + strDepartment;
            }
            else if (strStyle == "赠")
            {
                if (String.IsNullOrEmpty(strName) == false)
                {
                    strSellerName = strName
                        + "|" + strZipcode
                        + "|" + strAddress
                        + "|" + strDepartment
                        + "|" + strName;
                }
                else
                {
                    strSellerName = strDepartment
                        + "|" + strZipcode
                        + "|" + strAddress
                        + "|" + strDepartment
                        + "|" + strName;
                }
            }
            else
            {
                strError = "未知的渠道名 '" + strStyle + "'";
                return -1;
            }

            return 1;
        }

        OneSeller GetOneSeller(string strSeller)
        {
            object o = this.seller_table[strSeller];
            if (o != null)
                return (OneSeller)o;

            // 创建一个新的对象
            OneSeller seller = new OneSeller();
            seller.Seller = strSeller;
            this.seller_table[strSeller] = seller;
            return seller;
        }

        // 根据出版物类型，获得数据库名列表
        string GetBiblioDbNameList()
        {
            // 一般性的单个库名
            if (this.comboBox_inputBiblioDbName.Text != "<全部>"
                && this.comboBox_inputBiblioDbName.Text.ToLower() != "<all>"
                && this.comboBox_inputBiblioDbName.Text != "")
                return this.comboBox_inputBiblioDbName.Text;

            List<string> dbnames = new List<string>();
            if (this.MainForm.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty prop = this.MainForm.BiblioDbProperties[i];

                    if (String.IsNullOrEmpty(prop.OrderDbName) == true)
                        continue;   // 没有订购功能的书目库不在考虑之列

                    if (this.comboBox_source_type.Text == "图书")
                    {
                        if (String.IsNullOrEmpty(prop.IssueDbName) == false)
                            continue;
                    }
                    else
                    {
                        // 期刊。要求期库名不为空

                        if (String.IsNullOrEmpty(prop.IssueDbName) == true)
                            continue;
                    }

                    dbnames.Add(prop.DbName);
                }
            }

            if (dbnames.Count == 0)
                return "";

            return StringUtil.MakePathList(dbnames, ",");
        }

        // 检索获得特定批次号，或者所有书目记录路径(输出到文件)
        int SearchBiblioRecPath(
            string strBatchNo,
            string strRecPathFilename,
            out string strError)
        {
            strError = "";

            string strDbNameList = GetBiblioDbNameList();

            if (String.IsNullOrEmpty(strDbNameList) == true)
            {
                if (this.comboBox_inputBiblioDbName.Text == "<全部>"
                    || this.comboBox_inputBiblioDbName.Text.ToLower() == "<all>"
                    || this.comboBox_inputBiblioDbName.Text == "")
                {
                    strError = "出版物类型 '" + this.comboBox_source_type.Text + "' 不存在匹配的书目库";
                    return -1;
                }

                strError = "尚未指定书目库名";
                return -1;
            }

            // 创建文件
            StreamWriter sw = new StreamWriter(strRecPathFilename,
                false,	// append
                System.Text.Encoding.UTF8);

            try
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在检索 ...");
                stop.BeginLoop();

                EnableControls(false);

                try
                {
                    long lRet = 0;
                    string strQueryXml = "";

                    // 不指定批次号，意味着特定库全部条码
                    if (String.IsNullOrEmpty(strBatchNo) == true)
                    {
                        lRet = Channel.SearchBiblio(stop,
                             strDbNameList,
                             "",
                             -1,    // nPerMax
                             "recid",
                             "left",
                             this.Lang,
                             null,   // strResultSetName
                             "",    // strSearchStyle
                             "", // strOutputStyle
                             out strQueryXml,
                             out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else
                    {
                        // 指定批次号。特定库。
                        lRet = Channel.SearchBiblio(stop,
                             strDbNameList,
                             strBatchNo,
                             -1,    // nPerMax
                             "batchno",
                             "exact",
                             this.Lang,
                             null,   // strResultSetName
                             "",    // strSearchStyle
                             "", // strOutputStyle
                             out strQueryXml,
                             out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }

                    long lHitCount = lRet;

                    long lStart = 0;
                    long lCount = lHitCount;


                    DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                    // 装入浏览格式
                    for (; ; )
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                goto ERROR1;
                            }
                        }


                        lRet = Channel.GetSearchResult(
                            stop,
                            null,   // strResultSetName
                            lStart,
                            lCount,
                            "id",   // "id,cols",
                            this.Lang,
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        if (lRet == 0)
                        {
                            strError = "未命中";
                            goto ERROR1;
                        }

                        Debug.Assert(searchresults != null, "");


                        // 处理浏览结果
                        for (int i = 0; i < searchresults.Length; i++)
                        {
                            sw.Write(searchresults[i].Path + "\r\n");
                        }


                        lStart += searchresults.Length;
                        lCount -= searchresults.Length;

                        stop.SetMessage("共有记录 " + lHitCount.ToString() + " 个。已获得记录 " + lStart.ToString() + " 个");

                        if (lStart >= lHitCount || lCount <= 0)
                            break;
                    }

                }
                finally
                {
                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            return 0;
        ERROR1:
            return -1;
        }

        void SetInputPanelEnabled(bool bEnable)
        {
            if (bEnable == true)
            {
                this.radioButton_inputStyle_biblioDatabase.Enabled = true;
                this.radioButton_inputStyle_biblioRecPathFile.Enabled = true;
                this.radioButton_inputStyle_orderDatabase.Enabled = true;
                this.radioButton_inputStyle_orderRecPathFile.Enabled = true;

                if (this.radioButton_inputStyle_biblioRecPathFile.Checked == true)
                {
                    Debug.Assert(this.radioButton_inputStyle_biblioRecPathFile.Checked == true, "");
                    Debug.Assert(this.radioButton_inputStyle_biblioDatabase.Checked == false, "");
                    Debug.Assert(this.radioButton_inputStyle_orderRecPathFile.Checked == false, "");
                    Debug.Assert(this.radioButton_inputStyle_orderDatabase.Checked == false, "");


                    this.textBox_inputBiblioRecPathFilename.Enabled = true;
                    this.button_findInputBiblioRecPathFilename.Enabled = true;

                    this.comboBox_inputBiblioDbName.Enabled = false;

                    this.textBox_inputOrderRecPathFilename.Enabled = false;
                    this.button_findInputOrderRecPathFilename.Enabled = false;

                    this.comboBox_inputOrderDbName.Enabled = false;
                }
                else if (this.radioButton_inputStyle_biblioDatabase.Checked == true)
                {
                    Debug.Assert(this.radioButton_inputStyle_biblioRecPathFile.Checked == false, "");
                    Debug.Assert(this.radioButton_inputStyle_biblioDatabase.Checked == true, "");
                    Debug.Assert(this.radioButton_inputStyle_orderRecPathFile.Checked == false, "");
                    Debug.Assert(this.radioButton_inputStyle_orderDatabase.Checked == false, "");

                    this.textBox_inputBiblioRecPathFilename.Enabled = false;
                    this.button_findInputBiblioRecPathFilename.Enabled = false;

                    this.comboBox_inputBiblioDbName.Enabled = true;

                    this.textBox_inputOrderRecPathFilename.Enabled = false;
                    this.button_findInputOrderRecPathFilename.Enabled = false;

                    this.comboBox_inputOrderDbName.Enabled = false;
                }
                else if (this.radioButton_inputStyle_orderRecPathFile.Checked == true)
                {
                    Debug.Assert(this.radioButton_inputStyle_biblioRecPathFile.Checked == false, "");
                    Debug.Assert(this.radioButton_inputStyle_biblioDatabase.Checked == false, "");
                    Debug.Assert(this.radioButton_inputStyle_orderRecPathFile.Checked == true, "");
                    Debug.Assert(this.radioButton_inputStyle_orderDatabase.Checked == false, "");

                    this.textBox_inputBiblioRecPathFilename.Enabled = false;
                    this.button_findInputBiblioRecPathFilename.Enabled = false;

                    this.comboBox_inputBiblioDbName.Enabled = false;

                    this.textBox_inputOrderRecPathFilename.Enabled = true;
                    this.button_findInputOrderRecPathFilename.Enabled = true;

                    this.comboBox_inputOrderDbName.Enabled = false;
                }
                else if (this.radioButton_inputStyle_orderDatabase.Checked == true)
                {
                    Debug.Assert(this.radioButton_inputStyle_biblioRecPathFile.Checked == false, "");
                    Debug.Assert(this.radioButton_inputStyle_biblioDatabase.Checked == false, "");
                    Debug.Assert(this.radioButton_inputStyle_orderRecPathFile.Checked == false, "");
                    Debug.Assert(this.radioButton_inputStyle_orderDatabase.Checked == true, "");

                    this.textBox_inputBiblioRecPathFilename.Enabled = false;
                    this.button_findInputBiblioRecPathFilename.Enabled = false;

                    this.comboBox_inputBiblioDbName.Enabled = false;

                    this.textBox_inputOrderRecPathFilename.Enabled = false;
                    this.button_findInputOrderRecPathFilename.Enabled = false;

                    this.comboBox_inputOrderDbName.Enabled = true;
                }
                else
                {
                    // Debug.Assert(false, "不可能走到这里");
                }
            }
            else
            {
                this.radioButton_inputStyle_biblioDatabase.Enabled = false;
                this.radioButton_inputStyle_biblioRecPathFile.Enabled = false;
                this.radioButton_inputStyle_orderDatabase.Enabled = false;
                this.radioButton_inputStyle_orderRecPathFile.Enabled = false;

                this.textBox_inputBiblioRecPathFilename.Enabled = false;
                this.button_findInputBiblioRecPathFilename.Enabled = false;

                this.comboBox_inputBiblioDbName.Enabled = false;

                this.textBox_inputOrderRecPathFilename.Enabled = false;
                this.button_findInputOrderRecPathFilename.Enabled = false;

                this.comboBox_inputOrderDbName.Enabled = false;
            }
        }

        private void radioButton_inputStyle_biblioRecPathFile_CheckedChanged(object sender, EventArgs e)
        {
            SetInputPanelEnabled(true);
        }

        private void radioButton_inputStyle_biblioDatabase_CheckedChanged(object sender, EventArgs e)
        {
            SetInputPanelEnabled(true);
        }

        private void comboBox_inputBiblioDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_inputBiblioDbName.Items.Count > 0)
                return;

            // this.comboBox_inputBiblioDbName.Items.Add("<全部>");
            if (this.comboBox_source_type.Text == "图书")
                this.comboBox_inputBiblioDbName.Items.Add("<全部图书>");
            else
                this.comboBox_inputBiblioDbName.Items.Add("<全部期刊>");

            if (this.MainForm.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty prop = this.MainForm.BiblioDbProperties[i];

                    if (String.IsNullOrEmpty(prop.OrderDbName) == true)
                        continue;   // 没有订购功能的书目库不在考虑之列

                    if (this.comboBox_source_type.Text == "图书")
                    {
                        if (String.IsNullOrEmpty(prop.IssueDbName) == false)
                            continue;
                    }
                    else
                    {
                        // 期刊。要求期库名不为空

                        if (String.IsNullOrEmpty(prop.IssueDbName) == true)
                            continue;
                    }

                    this.comboBox_inputBiblioDbName.Items.Add(prop.DbName);
                }
            }

        }

        private void comboBox_source_type_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_inputBiblioDbName.Items.Count > 0)
            {
                this.comboBox_inputBiblioDbName.Items.Clear();
            }


#if NO
            // 检查一下当前已经选定的书目库名和出版物类型是否矛盾
             if (this.MainForm.BiblioDbProperties != null)
             {
                 for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                 {
                     BiblioDbProperty prop = this.MainForm.BiblioDbProperties[i];

                     if (strText == prop.DbName)
                     {
                         if (this.comboBox_source_type.Text == "图书"
                             && String.IsNullOrEmpty(prop.IssueDbName) == false)
                         {
                             this.comboBox_inputBiblioDbName.Text = "";
                             break;
                         }
                         else if (this.comboBox_source_type.Text == "连续出版物"
                             && String.IsNullOrEmpty(prop.IssueDbName) == true)
                         {
                             this.comboBox_inputBiblioDbName.Text = "";
                             break;
                         }
                     }
                 }
            }
#endif

            // 检查一下当前已经选定的订购库名和出版物类型是否矛盾
            if (this.comboBox_source_type.Text == "图书"
    && this.comboBox_inputBiblioDbName.Text == "<全部期刊>")
            {
                this.comboBox_inputBiblioDbName.Text = "<全部图书>";
                return;
            }
            if (this.comboBox_source_type.Text == "连续出版物"
&& this.comboBox_inputBiblioDbName.Text == "<全部图书>")
            {
                this.comboBox_inputBiblioDbName.Text = "<全部期刊>";
                return;
            }
            string strText = this.comboBox_inputBiblioDbName.Text;
            if (this.MainForm.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty prop = this.MainForm.BiblioDbProperties[i];

                    if (strText == prop.DbName)
                    {
                        if (this.comboBox_source_type.Text == "图书"
                            && String.IsNullOrEmpty(prop.IssueDbName) == false)
                        {
                            this.comboBox_inputBiblioDbName.Text = "";
                            break;
                        }
                        else if (this.comboBox_source_type.Text == "连续出版物"
                            && String.IsNullOrEmpty(prop.IssueDbName) == true)
                        {
                            this.comboBox_inputBiblioDbName.Text = "";
                            break;
                        }
                    }
                }
            }

            //

             if (this.comboBox_inputOrderDbName.Items.Count > 0)
             {
                 this.comboBox_inputOrderDbName.Items.Clear();
             }

             strText = this.comboBox_inputOrderDbName.Text;

             // 检查一下当前已经选定的订购库名和出版物类型是否矛盾
             if (this.comboBox_source_type.Text == "图书"
     && this.comboBox_inputOrderDbName.Text == "<全部期刊>")
             {
                 this.comboBox_inputOrderDbName.Text = "<全部图书>";
                 return;
             }
             if (this.comboBox_source_type.Text == "连续出版物"
&& this.comboBox_inputOrderDbName.Text == "<全部图书>")
             {
                 this.comboBox_inputOrderDbName.Text = "<全部期刊>";
                 return;
             }
             if (this.MainForm.BiblioDbProperties != null)
             {
                 for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                 {
                     BiblioDbProperty prop = this.MainForm.BiblioDbProperties[i];

                     if (strText == prop.OrderDbName)
                     {
                         if (this.comboBox_source_type.Text == "图书"
                             && String.IsNullOrEmpty(prop.IssueDbName) == false)
                         {
                             this.comboBox_inputOrderDbName.Text = "";
                             break;
                         }
                         else if (this.comboBox_source_type.Text == "连续出版物"
                             && String.IsNullOrEmpty(prop.IssueDbName) == true)
                         {
                             this.comboBox_inputOrderDbName.Text = "";
                             break;
                         }
                     }
                 }
             }
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strWarning = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_source)
            {
                if (InputStyle == PrintClaimInputStyle.BiblioRecPathFile)
                {
                    if (this.textBox_inputBiblioRecPathFilename.Text == "")
                    {
                        strError = "尚未指定输入的书目库记录路径文件名";
                        goto ERROR1;
                    }
                }
                else if (this.InputStyle == PrintClaimInputStyle.BiblioDatabase)
                {
                    if (this.comboBox_inputBiblioDbName.Text == "")
                    {
                        strError = "尚未指定书目库名";
                        goto ERROR1;
                    }
                }
                else if (InputStyle == PrintClaimInputStyle.OrderRecPathFile)
                {
                    if (this.textBox_inputOrderRecPathFilename.Text == "")
                    {
                        strError = "尚未指定输入的订购库记录路径文件名";
                        goto ERROR1;
                    }
                }
                else if (this.InputStyle == PrintClaimInputStyle.OrderDatabase)
                {
                    if (this.comboBox_inputOrderDbName.Text == "")
                    {
                        strError = "尚未指定订购库名";
                        goto ERROR1;
                    }
                }

                // 切换到日期范围page
                this.tabControl_main.SelectedTab = this.tabPage_timeRange;
                return;
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_timeRange)
            {
                if (this.textBox_timeRange.Text == "")
                {
                    strError = "尚未指定催询日期范围";
                    goto ERROR1;
                }
                this.tabControl_main.SelectedTab = this.tabPage_run;
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_run)
            {
                this.Running = true;
                try
                {
                    // 对每个书目记录进行循环
                    // return:
                    //      0   普通返回
                    //      1   要全部中断
                    int nRet = DoLoop(out strError,
                        out strWarning);
                    if (nRet == -1)
                        goto ERROR1;
                }
                finally
                {
                    this.Running = false;
                }

                MessageBox.Show(this, "统计完成。");
                this.tabControl_main.SelectedTab = this.tabPage_print;
                this.button_next.Enabled = false;
                return;
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
                this.button_next.Enabled = false;
            }

            if (String.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, "警告: \r\n" + strWarning);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.Running == true)
                return;

            if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
                this.button_next.Enabled = false;
                return;
            }

            this.button_next.Enabled = true;
        }

        // 打印催询单
        private void button_print_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.seller_table.Count == 0)
            {
                strError = "目前没有可打印的内容";
                goto ERROR1;
            }

            List<string> filenames = new List<string>();

            try
            {
                foreach (string strKey in this.seller_table.Keys)
                {
                    OneSeller seller = (OneSeller)this.seller_table[strKey];
                    string strFilename = "";
                    int nRet = PrintOneSeller(seller,
                        out strFilename,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    filenames.Add(strFilename);
                }

                HtmlPrintForm printform = new HtmlPrintForm();

                printform.Text = "打印催询单";
                printform.MainForm = this.MainForm;
                printform.Filenames = filenames;
                this.MainForm.AppInfo.LinkFormState(printform, "printclaim_htmlprint_formstate");
                printform.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(printform);

            }
            finally
            {
                if (filenames != null)
                {
                    Global.DeleteFiles(filenames);
                    filenames.Clear();
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得纯粹的书商名，丢掉'|'后面的部分
        static string GetPureSellerName(string strText)
        {
            int nRet = strText.IndexOf("|");
            if (nRet == -1)
                return strText;

            return strText.Substring(0, nRet);
        }

        // 构造纯文本的邮寄地址字符串
        // parameters:
        static int BuildAddressText(
            string strAddressXml,
            out string strText,
            out string strError)
        {
            strError = "";
            strText = "";

            if (String.IsNullOrEmpty(strAddressXml) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root/>");
            try
            {
                dom.DocumentElement.InnerXml = strAddressXml;
            }
            catch (Exception ex)
            {
                strError = "地址XML格式不正确: " + ex.Message;
                return -1;
            }

            string strZipcode = DomUtil.GetElementText(dom.DocumentElement,
                "zipcode");
            string strAddress = DomUtil.GetElementText(dom.DocumentElement,
                "address");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement,
                "department");
            string strName = DomUtil.GetElementText(dom.DocumentElement,
                "name");

            strText = "(" + strZipcode + ")" + strAddress + " " + strDepartment + " " + strName;

            return 1;
        }

        // 
        // 
        /// <summary>
        /// 关于来源的描述。
        /// 如果为"batchno"方式，则为批次号；
        /// 如果为"barcodefile"方式，则为条码号文件名(纯文件名); 
        /// 如果为"recpathfile"方式，则为记录路径文件名(纯文件名)
        /// </summary>
        public string SourceDescription
        {
            get
            {
                if (this.SourceStyle == "bibliodatabase")
                {
                    string strText = "";

                    if (String.IsNullOrEmpty(this.comboBox_inputBiblioDbName.Text) == false)
                        strText += "书目库 " + this.comboBox_inputBiblioDbName.Text;

                    return strText;
                }
                else if (this.SourceStyle == "bibliorecpathfile")
                {
                    return "书目库记录路径文件 " + Path.GetFileName(this.textBox_inputBiblioRecPathFilename.Text);
                }
                else if (this.SourceStyle == "orderdatabase")
                {
                    string strText = "";

                    if (String.IsNullOrEmpty(this.comboBox_inputOrderDbName.Text) == false)
                        strText += "订购库 " + this.comboBox_inputOrderDbName.Text;

                    return strText;
                }
                else if (this.SourceStyle == "orderrecpathfile")
                {
                    return "订购库记录路径文件 " + Path.GetFileName(this.textBox_inputOrderRecPathFilename.Text);
                }
                else
                {
                    Debug.Assert(this.SourceStyle == "", "");
                    return "";
                }
            }
        }

        // 打印属于一个书商的全部期刊信息
        int PrintOneSeller(OneSeller seller,
            out string strFilename,
            out string strError)
        {
            strError = "";
            strFilename = "";

            int nRet = 0;

            string strAddressText = "";
            if (String.IsNullOrEmpty(seller.AddressXml) == false)
            {
                nRet = BuildAddressText(
                    seller.AddressXml,
                    out strAddressText,
                    out strError);
                if (nRet == -1)
                    return -1;
            }


            Hashtable macro_table = new Hashtable();

            // 批次号或文件名 多个宏不方便判断后选用，只好合并为一个宏
            macro_table["%sourcedescription%"] = this.SourceDescription;


            // 获得打印参数
            PrintClaimPrintOption option = new PrintClaimPrintOption(this.MainForm.DataDir,
                this.comboBox_source_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                "printclaim_printoption");

            macro_table["%seller%"] = GetPureSellerName(seller.Seller); // 渠道名
            macro_table["%selleraddress%"] = strAddressText;    // 2009/9/17
            macro_table["%libraryname%"] = this.MainForm.LibraryName;
            /*
            macro_table["%pagecount%"] = nPageCount.ToString();
            macro_table["%linesperpage%"] = option.LinesPerPage.ToString();
             * */
            macro_table["%date%"] = DateTime.Now.ToLongDateString();


            // 需要将属于不同书商的文件名前缀区别开来
            string strFileNamePrefix = this.MainForm.DataDir + "\\~printclaim_" + seller.GetHashCode().ToString() + "_";

            strFilename = strFileNamePrefix + "0" + ".html";

            BuildPageTop(option,
                macro_table,
                strFilename,
                seller);

            // 输出信函内容
            {

                // 期刊种数
                macro_table["%seriescount%"] = seller.Count.ToString();
                // 相关的期数
                macro_table["%issuecount%"] = GetIssueCount(seller).ToString();
                // 缺的册数
                macro_table["%missingitemcount%"] = GetMissingItemCount(seller).ToString();

                macro_table["%datadir%"] = this.MainForm.DataDir;   // 便于引用datadir下templates目录内的某些文件
                //// macro_table["%libraryserverdir%"] = this.MainForm.LibraryServerDir;  // 便于引用服务器端的CSS文件

                string strTemplateFilePath = option.GetTemplatePageFilePath("信件正文");
                if (String.IsNullOrEmpty(strTemplateFilePath) == false)
                {
                    /*
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
                    StreamUtil.WriteText(strFilename,
                        strResult);
                }
                else
                {
                    // 缺省的固定内容打印
                    StreamUtil.WriteText(strFilename,
                        "<div class='letter'>");

                    string strAddressLine = "";
                    if (String.IsNullOrEmpty((string)macro_table["%selleraddress%"]) == false)
                        strAddressLine = "致：%selleraddress%<br/><br/>";

                    string strText = strAddressLine + "尊敬的 %seller%:<br/>我馆在贵处订购的以下"+this.TypeName+" %seriescount% 种，有 "
                        + (this.TypeName == "期刊" ? "%issuecount% 期 " : "")
                        +"共 %missingitemcount% 册至今未到。望尽快补齐为盼。谢谢。<br/><br/>%libraryname%<br/>%date%";
                    strText = StringUtil.MacroString(macro_table,
                        strText);

                    StreamUtil.WriteText(strFilename,
                        strText);

                    StreamUtil.WriteText(strFilename,
                        "</div>");
                }

            }

            for (int i = 0; i < seller.Count; i++)
            {
                OneSeries series = seller[i];

                PrintOneSeries(option,
                    macro_table,
                    series,
                    strFilename);
            }


            BuildPageBottom(option,
                macro_table,
                strFilename);

            return 0;
        }

        string TypeName
        {
            get
            {
                if (this.comboBox_source_type.Text == "连续出版物"
                    || this.comboBox_source_type.Text == "期刊")
                    return "期刊";
                return "图书";
            }
        }

        // 参与的期数
        static int GetIssueCount(OneSeller seller)
        {
            int nCount = 0;
            for (int i = 0; i < seller.Count; i++)
            {
                OneSeries series = seller[i];

                nCount += series.IssueInfos.Count;
            }

            return nCount;
        }

        // 缺的册数
        static int GetMissingItemCount(OneSeller seller)
        {
            int nCount = 0;
            for (int i = 0; i < seller.Count; i++)
            {
                OneSeries series = seller[i];

                for (int j = 0; j < series.IssueInfos.Count; j++)
                {
                    IssueInfo info = series.IssueInfos[j];

                    int nValue = 0;

                    try
                    {
                        nValue = Convert.ToInt32(info.MissingCount);
                    }
                    catch
                    {
                    }

                    nCount += nValue;
                }
            }

            return nCount;
        }

        // 输出一种期刊的信息
        void PrintOneSeries(PrintOption option,
            Hashtable macro_table, 
            OneSeries series,
            string strFilename)
        {

            string strClass = "";
            string strCaption = "";


            // 表格开始
            StreamUtil.WriteText(strFilename,
                "<br/><table class='table'>");   //   border='1'


            // 第一个栏目标题
            StreamUtil.WriteText(strFilename,
                "<tr class='column'>");

            strClass = "series_info";
            strCaption = this.TypeName + "信息";
            StreamUtil.WriteText(strFilename,
"<td class='" + strClass + "' colspan='5'>" + strCaption + "</td>");


            StreamUtil.WriteText(strFilename,
                "</tr>");

            // 期刊信息行
            StreamUtil.WriteText(strFilename,
                "<tr class='series_info'>");

            strClass = "series_info";
            strCaption = series.BiblioSummary;
            StreamUtil.WriteText(strFilename,
"<td class='" + strClass + "' colspan='" + option.Columns.Count.ToString() + "'>" + strCaption + "</td>");


            StreamUtil.WriteText(strFilename,
                "</tr>");

            // 第二个栏目标题

            StreamUtil.WriteText(strFilename,
    "</tr>");

            // 期刊信息行

            // 通栏：缺期信息
            StreamUtil.WriteText(strFilename,
                "<tr class='column'>");
            strClass = "missing_info";
            if (this.TypeName == "期刊")
                strCaption = "缺期信息";
            else
                strCaption = "缺书信息";

            StreamUtil.WriteText(strFilename,
                "<td class='" + strClass + "' colspan='" + option.Columns.Count.ToString() + "'>" + strCaption + "</td>");
            StreamUtil.WriteText(strFilename,
                "</tr>");

            /*
            StreamUtil.WriteText(strFilename,
                "<tr class='column'>");

            strClass = "publishTime";
            strCaption = "出版日期";
            StreamUtil.WriteText(strFilename,
"<td class='" + strClass + "'>" + strCaption + "</td>");

            strClass = "issue";
            strCaption = "期号";
            StreamUtil.WriteText(strFilename,
"<td class='" + strClass + "'>" + strCaption + "</td>");

            strClass = "orderCount";
            strCaption = "订购册数";
            StreamUtil.WriteText(strFilename,
"<td class='" + strClass + "'>" + strCaption + "</td>");

            strClass = "arrivedCount";
            strCaption = "实到册数";
            StreamUtil.WriteText(strFilename,
"<td class='" + strClass + "'>" + strCaption + "</td>");

            strClass = "missingCount";
            strCaption = "缺册数";
            StreamUtil.WriteText(strFilename,
"<td class='" + strClass + "'>" + strCaption + "</td>");

            StreamUtil.WriteText(strFilename,
"</tr>");
             * */
            // 栏目标题
            StreamUtil.WriteText(strFilename,
                "<tr class='column'>");

            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                strCaption = column.Caption;

                // 如果没有caption定义，就挪用name定义
                if (String.IsNullOrEmpty(strCaption) == true)
                    strCaption = column.Name;

                strClass = StringUtil.GetLeft(column.Name);

                StreamUtil.WriteText(strFilename,
                    "<td class='" + strClass + "'>" + strCaption + "</td>");
            }

            StreamUtil.WriteText(strFilename,
                "</tr>");


            // 内容行
            for (int i = 0; i < series.IssueInfos.Count; i++)
            {
                IssueInfo info = series.IssueInfos[i];

                // 不输出已经到齐的行
                if (info.MissingCount == "0")
                    continue;

                StreamUtil.WriteText(strFilename,
"<tr class='content'>");

                /*
                strClass = "publishTime";
                strCaption = info.PublishTime;
                StreamUtil.WriteText(strFilename,
    "<td class='" + strClass + "'>" + strCaption + "</td>");

                strClass = "issue";
                strCaption = info.Issue;
                StreamUtil.WriteText(strFilename,
    "<td class='" + strClass + "'>" + strCaption + "</td>");

                strClass = "orderCount";
                strCaption = info.OrderCount;
                StreamUtil.WriteText(strFilename,
    "<td class='" + strClass + "'>" + strCaption + "</td>");

                strClass = "arrivedCount";
                strCaption = info.ArrivedCount;
                StreamUtil.WriteText(strFilename,
    "<td class='" + strClass + "'>" + strCaption + "</td>");

                strClass = "missingCount";
                strCaption = info.MissingCount;
                StreamUtil.WriteText(strFilename,
    "<td class='" + strClass + "'>" + strCaption + "</td>");
                 * */
                for (int j = 0; j < option.Columns.Count; j++)
                {
                    Column column = option.Columns[j];

                    string strContent = GetColumnContent(info,
    column.Name);

                    strClass = StringUtil.GetLeft(column.Name);
                    StreamUtil.WriteText(strFilename,
                        "<td class='" + strClass + "'>" + strContent + "</td>");

                }

                StreamUtil.WriteText(strFilename,
    "</tr>");
            }

            // 表格结束
            StreamUtil.WriteText(strFilename,
                "</table>");
        }

        // 获得栏目内容
        string GetColumnContent(IssueInfo info,
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

            try
            {
                // TODO: 需要修改
                // 要中英文都可以
                switch (strText)
                {
                    case "publishTime":
                    case "出版日期":
                        {
                            string strPublishTime = "";
                            
                            if (string.IsNullOrEmpty(info.PublishTime) == false)
                                strPublishTime = DateTimeUtil.ForcePublishTime8(info.PublishTime);

                            if (string.IsNullOrEmpty(strPublishTime) == true
                                && string.IsNullOrEmpty(info.OrderTime) == false)
                            {
                                DateTime order_time = DateTimeUtil.FromRfc1123DateTimeString(info.OrderTime).ToLocalTime();

                                string strDelay = this.comboBox_timeRange_afterOrder.Text;
                                if (strDelay == "立即")
                                    strDelay = "";
                                if (string.IsNullOrEmpty(strDelay) == false)
                                    strDelay = " + " + strDelay;

                                return "? " + order_time.ToString("d") + strDelay;
                            }

                            return DateTimeUtil.Long8ToDateTime(strPublishTime).ToString("d");
                        }

                    case "issue":
                    case "期号":
                        return info.Issue;

                    case "orderCount":
                    case "订购册数":
                        return info.OrderCount;

                    case "arrivedCount":
                    case "实到册数":
                        return info.ArrivedCount;

                    case "missingCount":
                    case "缺册数":
                        return info.MissingCount;

                    default:
                        return "undefined column";
                }
            }
            catch
            {
                return null; 
            }
        }

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

        int BuildPageTop(PrintOption option,
            Hashtable macro_table,
            string strFileName,
            OneSeller seller)
        {
            // string strCssUrl = this.MainForm.LibraryServerDir + "/printclaim.css";
            // 2009/10/10 changed
            string strCssUrl = GetAutoCssUrl(option, "printclaim.css");

            /*
            // 2009/10/9
            string strCssFilePath = option.GetTemplatePageFilePath("css");  // 大小写不敏感
            if (String.IsNullOrEmpty(strCssFilePath) == false)
                strCssUrl = strCssFilePath;
            else
                strCssUrl = this.MainForm.LibraryServerDir + "/printclaim.css";    // 缺省的
             * */

            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

            StreamUtil.WriteText(strFileName,
                "<html><head>" + strLink + "</head><body>");

            /*
            // 页眉
            string strPageHeaderText = option.PageHeader;

            if (String.IsNullOrEmpty(strPageHeaderText) == false)
            {
                strPageHeaderText = Global.MacroString(macro_table,
                    strPageHeaderText);

                StreamUtil.WriteText(strFileName,
                    "<div class='pageheader'>" + strPageHeaderText + "</div>");
            }
             * */

            // 书商名称
            StreamUtil.WriteText(strFileName,
    "<div class='seller'>" + GetPureSellerName(seller.Seller) + "</div>");

            /*
            // 表格标题
            string strTableTitleText = option.TableTitle;

            if (String.IsNullOrEmpty(strTableTitleText) == false)
            {

                strTableTitleText = Global.MacroString(macro_table,
                    strTableTitleText);

                StreamUtil.WriteText(strFileName,
                    "<div class='tabletitle'>" + strTableTitleText + "</div>");
            }
             * */



            return 0;
        }


        int BuildPageBottom(PrintOption option,
            Hashtable macro_table,
            string strFileName)
        {

            /*
            // 页脚
            string strPageFooterText = option.PageFooter;

            if (String.IsNullOrEmpty(strPageFooterText) == false)
            {
                strPageFooterText = Global.MacroString(macro_table,
                    strPageFooterText);

                StreamUtil.WriteText(strFileName,
        "<div class='pagefooter'>" + strPageFooterText + "</div>");
            }*/

            StreamUtil.WriteText(strFileName, "</body></html>");

            return 0;
        }

        private void button_timeRange_clearTimeRange_Click(object sender, EventArgs e)
        {
            this.textBox_timeRange.Text = "";

        }

        private void button_timeRange_inputTimeRange_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            DateTime start;
            DateTime end;

            nRet = Global.ParseTimeRangeString(this.textBox_timeRange.Text,
                false,
                out start,
                out end,
                out strError);
            /*
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }*/


            TimeRangeDlg dlg = new TimeRangeDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "催询日期范围";
            dlg.StartDate = start;
            dlg.EndDate = end;
            dlg.AllowStartDateNull = true;  // 允许起点时间为空

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            this.textBox_timeRange.Text = Global.MakeTimeRangeString(dlg.StartDate, dlg.EndDate);

            this.comboBox_timeRange_quickSet.Text = ""; // 以免误会
        }

        // 快速设置
        private void comboBox_timeRange_quickSet_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_timeRange_quickSet.Text == "今天前")
            {
                this.textBox_timeRange.Text = Global.MakeTimeRangeString(new DateTime(0), DateTime.Now);
            }
            else if (this.comboBox_timeRange_quickSet.Text == "一月前")
            {
                DateTime now = DateTime.Now;
                DateTime time = new DateTime(now.Year, now.Month, 1);
                time = time - new TimeSpan(24, 0, 0);

                this.textBox_timeRange.Text = Global.MakeTimeRangeString(new DateTime(0), time);
            }
            else if (this.comboBox_timeRange_quickSet.Text == "半年前")
            {
                DateTime now = DateTime.Now;
                DateTime time = new DateTime(now.Year, now.Month, 1);

                if (time.Month >= 7)    // 2011/7/11 bug
                {
                    time = new DateTime(time.Year, time.Month - 6, 1);
                }
                else
                {
                    time = new DateTime(time.Year - 1, time.Month + 12 - 6, 1);
                }

                time = time - new TimeSpan(24, 0, 0);

                this.textBox_timeRange.Text = Global.MakeTimeRangeString(new DateTime(0), time);
            }
            else if (this.comboBox_timeRange_quickSet.Text == "一年前")
            {
                DateTime now = DateTime.Now;
                DateTime time = new DateTime(now.Year - 1, now.Month, 1);
                time = time - new TimeSpan(24, 0, 0);

                this.textBox_timeRange.Text = Global.MakeTimeRangeString(new DateTime(0), time);
            }
            else
            {
                // Console.Beep(); // 表示无法设置
            }
        }

        private void PrintClaimForm_Activated(object sender, EventArgs e)
        {
            this.MainForm.stopManager.Active(this.stop);
        }

        private void button_printOption_Click(object sender, EventArgs e)
        {
            // 配置标题和风格
            string strNamePath = "printclaim_printoption";

            PrintClaimPrintOption option = new PrintClaimPrintOption(this.MainForm.DataDir,
                this.comboBox_source_type.Text);
            option.LoadData(this.MainForm.AppInfo,
                strNamePath);


            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.MainForm = this.MainForm;
            dlg.DataDir = this.MainForm.DataDir;
            dlg.Text = this.comboBox_source_type.Text + " 催询单 打印参数";
            dlg.PrintOption = option;
            dlg.ColumnItems = new string[] {
                "publishTime -- 出版日期",
                "issue -- 期号",
                "orderCount -- 订购册数",
                "arrivedCount -- 实到册数",
                "missingCount -- 缺册数",
            };


            this.MainForm.AppInfo.LinkFormState(dlg, "printclaim_printoption_formstate");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(this.MainForm.AppInfo,
                strNamePath);
        }

        private void button_findInputBiblioRecPathFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的[书目库]记录路径文件名";
            if (this.textBox_inputBiblioRecPathFilename.Text.IndexOf(",") == -1)
                dlg.FileName = this.textBox_inputBiblioRecPathFilename.Text;
            // dlg.InitialDirectory = 
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_inputBiblioRecPathFilename.Text = dlg.FileName;
        }

        private void button_findInputOrderRecPathFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的[订购库]记录路径文件名";
            if (this.textBox_inputOrderRecPathFilename.Text.IndexOf(",") == -1)
                dlg.FileName = this.textBox_inputOrderRecPathFilename.Text;
            // dlg.InitialDirectory = 
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_inputOrderRecPathFilename.Text = dlg.FileName;
        }

        private void comboBox_inputOrderDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_inputOrderDbName.Items.Count > 0)
                return;

            if (this.comboBox_source_type.Text == "图书")
                this.comboBox_inputOrderDbName.Items.Add("<全部图书>");
            else
                this.comboBox_inputOrderDbName.Items.Add("<全部期刊>");

            if (this.MainForm.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty prop = this.MainForm.BiblioDbProperties[i];

                    if (String.IsNullOrEmpty(prop.OrderDbName) == true)
                        continue; 

                    if (this.comboBox_source_type.Text == "图书")
                    {
                        if (String.IsNullOrEmpty(prop.IssueDbName) == false)
                            continue;
                    }
                    else
                    {
                        // 期刊。要求期库名不为空

                        if (String.IsNullOrEmpty(prop.IssueDbName) == true)
                            continue;
                    }

                    this.comboBox_inputOrderDbName.Items.Add(prop.OrderDbName);
                }
            }
        }

        private void radioButton_inputStyle_orderRecPathFile_CheckedChanged(object sender, EventArgs e)
        {
            SetInputPanelEnabled(true);
        }

        private void radioButton_inputStyle_orderDatabase_CheckedChanged(object sender, EventArgs e)
        {
            SetInputPanelEnabled(true);
        }

        void SetTimeRangeState(bool bEnabled)
        {
            if (this.checkBox_timeRange_useOrderTime.Checked == true)
            {
                this.comboBox_timeRange_afterOrder.Enabled = bEnabled;
            }
            else
            {
                this.comboBox_timeRange_afterOrder.Enabled = false;
            }

            if (this.checkBox_timeRange_none.Checked == true)
            {
                this.checkBox_timeRange_useOrderTime.Enabled = false;
                this.checkBox_timeRange_usePublishTime.Enabled = false;
                this.comboBox_timeRange_afterOrder.Enabled = false;

                SetTimeRangeValueVisible(false);
            }
            else
            {
                this.checkBox_timeRange_useOrderTime.Enabled = bEnabled;
                this.checkBox_timeRange_usePublishTime.Enabled = bEnabled;
                // checkBox_timeRange_useOrderTime_CheckedChanged(null, null);

                SetTimeRangeValueVisible(bEnabled);
            }
        }

        private void checkBox_timeRange_useOrderTime_CheckedChanged(object sender, EventArgs e)
        {
            SetTimeRangeState(true);
        }

        // 设置时间范围值相关的界面元素的Enabled状态。包括“快速设置”部分
        void SetTimeRangeValueVisible(bool bVisible)
        {
            this.label_timerange.Visible = bVisible;
            this.textBox_timeRange.Visible = bVisible;
            this.button_timeRange_clearTimeRange.Visible = bVisible;
            this.button_timeRange_inputTimeRange.Visible = bVisible;
            this.groupBox_timeRange_quickSet.Visible = bVisible;
        }

        private void checkBox_timeRange_none_CheckedChanged(object sender, EventArgs e)
        {
            SetTimeRangeState(true);
        }

        private void comboBox_source_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox_source_type.Text == "图书")
                this.checkBox_source_guess.Enabled = false;
            else
                this.checkBox_source_guess.Enabled = true;
        }
    }

    /// <summary>
    /// 催询窗输入风格
    /// </summary>
    public enum PrintClaimInputStyle
    {
        /// <summary>
        /// 书目库记录路径文件
        /// </summary>
        BiblioRecPathFile = 1,    // 书目库记录路径文件
        /// <summary>
        /// 整个书目库
        /// </summary>
        BiblioDatabase = 2,     // 整个书目库
        /// <summary>
        /// 订购库记录路径文件
        /// </summary>
        OrderRecPathFile = 3,   // 订购库记录路径文件
        /// <summary>
        /// 整个订购库
        /// </summary>
        OrderDatabase = 4,      // 整个订购库
    }

    // 
    /// <summary>
    /// 一个书商所负责的所有期刊的缺期信息，连同书商地址
    /// </summary>
    public class OneSeller : List<OneSeries>
    {
        /// <summary>
        /// 渠道或书商名
        /// </summary>
        public string Seller = "";  // 书商名

        /// <summary>
        /// 地址 XML
        /// </summary>
        public string AddressXml = "";  // 联系地址信息

        // 
        /// <summary>
        /// 根据书目记录路径找到一个已经存在的OneSeries对象
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <returns>OneSeries 对象</returns>
        public OneSeries FindOneSeries(string strBiblioRecPath)
        {
            foreach (OneSeries series in this)
            {
                if (series.BiblioRecPath == strBiblioRecPath)
                    return series;
            }

            return null;
        }
    }

    // 
    /// <summary>
    /// 一种期刊的缺期信息
    /// </summary>
    public class OneSeries
    {
        /// <summary>
        /// 书目记录路径
        /// </summary>
        public string BiblioRecPath = "";   // 书目记录路径

        /// <summary>
        /// 书目摘要
        /// </summary>
        public string BiblioSummary = "";   // 书目摘要

        /// <summary>
        /// ISSN
        /// </summary>
        public string ISSN = "";

        /// <summary>
        /// 刊名
        /// </summary>
        public string Title = "";

        /// <summary>
        /// 期信息集合
        /// </summary>
        public List<IssueInfo> IssueInfos = new List<IssueInfo>();   // 缺期信息

        /// <summary>
        /// 计算两个数字相加的值
        /// </summary>
        /// <param name="s1">数字字符串1</param>
        /// <param name="s2">数字字符串2</param>
        /// <returns>结果字符串</returns>
        public static string Add(string s1, string s2)
        {
            long v1 = 0;
            Int64.TryParse(s1, out v1);
            long v2 = 0;
            Int64.TryParse(s2, out v2);

            return (v1 + v2).ToString();
        }

        /// <summary>
        /// 将 other_series 合并进入当前对象。合并是根据出版日期和期号进行的
        /// </summary>
        /// <param name="other_series">另一种期刊的信息</param>
        public void MergeIssueInfos(OneSeries other_series)
        {
            int nAppendCount = 0;
            foreach (IssueInfo other_info in other_series.IssueInfos)
            {
                string strOtherYearPart = IssueUtil.GetYearPart(other_info.PublishTime);
                bool bFound = false;
                foreach (IssueInfo info in this.IssueInfos)
                {
                    if (strOtherYearPart == IssueUtil.GetYearPart(info.PublishTime)
                        && other_info.Issue == info.Issue)
                    {
                        info.OrderCount = Add(info.OrderCount, other_info.OrderCount);
                        info.MissingCount = Add(info.MissingCount, other_info.MissingCount);
                        info.ArrivedCount = Add(info.ArrivedCount, other_info.ArrivedCount);
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                {
                    this.IssueInfos.Add(other_info);
                    nAppendCount++;
                }
            }

            // 需要重新排序
            if (nAppendCount > 0)
            {
                this.IssueInfos.Sort(new IssueInfoSorter());
            }
        }
    }

#if NO
    // 一个书商所负责的所有图书的缺信息，连同书商地址
    public class OneSellerMono : List<OneMono>
    {
        public string Seller = "";  // 书商名
        public string AddressXml = "";  // 联系地址信息
    }

    // 一种图书的缺信息
    public class OneMono
    {
        public string BiblioRecPath = "";   // 书目记录路径
        public string BiblioSummary = "";   // 书目摘要
        public string ISSN = "";
        public string Title = "";

        public List<OrderInfo> OrderInfos = new List<OrderInfo>();   // 缺期信息
    }
#endif

    // 催询单打印 定义了特定缺省值的PrintOption派生类
    internal class PrintClaimPrintOption : PrintOption
    {
        string PublicationType = "图书"; // 图书 连续出版物

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

        public PrintClaimPrintOption(string strDataDir,
            string strPublicationType)
        {
            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

            this.PageHeaderDefault = "%date% %seller% 催询单 - 批次号或文件名: %batchno_or_recpathfilename% - (共 %pagecount% 页)"; // TODO: 修改 batchno_or_recpathfilename
            this.PageFooterDefault = "%pageno%/%pagecount%";

            this.TableTitleDefault = "%date% %seller% 催询单";

            this.LinesPerPageDefault = 20;

            // Columns缺省值
            Columns.Clear();

            // "publishTime -- 出版日期",
            Column column = new Column();
            column.Name = "publishTime -- 出版日期";
            column.Caption = "出版日期";
            column.MaxChars = -1;
            this.Columns.Add(column);

            if (strPublicationType == "连续出版物"
                || strPublicationType == "期刊")
            {
                // "issue -- 期号"
                column = new Column();
                column.Name = "issue -- 期号";
                column.Caption = "期号";
                column.MaxChars = -1;
                this.Columns.Add(column);
            }


            // "orderCount -- 订购册数"
            column = new Column();
            column.Name = "orderCount -- 订购册数";
            column.Caption = "订购册数";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "arrivedCount -- 实到册数"
            column = new Column();
            column.Name = "arrivedCount -- 实到册数";
            column.Caption = "实到册数";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "missingCount -- 缺册数"
            column = new Column();
            column.Name = "missingCount -- 缺册数";
            column.Caption = "缺册数";
            column.MaxChars = -1;
            this.Columns.Add(column);

        }
    }

    // 
    /// <summary>
    /// 时间过滤器。描述时间过滤要求
    /// </summary>
    public class TimeFilter
    {
        /// <summary>
        /// 风格。"both" 二者都用(先用出版时间，如果没有则用订购时间) / "publishtime" 只用出版时间(没有出版时间则不处理) / "ordertime" 只用订购时间偏移(没有订购时间则不处理) / "none" 完全不过滤 
        /// </summary>
        public string Style = "both";   // "both" 二者都用(先用出版时间，如果没有则用订购时间) / "publishtime" 只用出版时间(没有出版时间则不处理) / "ordertime" 只用订购时间偏移(没有订购时间则不处理) / "none" 完全不过滤 
        // 出版日期范围
        // 缺省效果是永远的过去-今天现在
        /// <summary>
        /// 出版日期范围，开始时间。缺省为永远的过去
        /// </summary>
        public DateTime StartTime = new DateTime(0);
        /// <summary>
        /// 出版日期范围，结束时间。缺省为今天
        /// </summary>
        public DateTime EndTime = DateTime.Now;

        // 订购日期 + 偏移量 落入指定范围
        /// <summary>
        /// 订购时间偏移量
        /// </summary>
        public TimeSpan OrderTimeDelta = new TimeSpan();

        // 寻找实到1册以上的最后一期。这是一个技巧，因为如果某期虽然超过催缺的范围(较指定范围越过靠后)，但它实际上到了，表明比这期时间还要早的期应该也到了。这样就要考虑实际的情况，而不是拘泥操作者设定的时间
        /// <summary>
        /// 是否校验实际已到的期
        /// </summary>
        public bool VerifyArrivedIssue = false;
    }
}