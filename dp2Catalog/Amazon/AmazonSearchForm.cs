using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Xml;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Xml.Xsl;

using DigitalPlatform.Text;
using DigitalPlatform.GUI;

using AmazonProductAdvtApi;

using DigitalPlatform.Xml;
using DigitalPlatform;
using DigitalPlatform.Marc;
using DigitalPlatform.CirculationClient;

using dp2Secure;
using DigitalPlatform.AmazonInterface;
using DigitalPlatform.Core;

namespace dp2Catalog
{
    public partial class AmazonSearchForm : Form, ISearchForm
    {
        public const string API_VERSION = "2013-08-01"; // "2011-08-01";

        // 浏览事项的类型下标
        public const int BROWSE_TYPE_NORMAL = 0;   // 普通记录
        public const int BROWSE_TYPE_DIAG = 1;     // 诊断记录 或者 4
        public const int BROWSE_TYPE_BRIEF = 2;     // 简化格式
        public const int BROWSE_TYPE_FULL = 3;     // 详细格式

        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();

        public string Lang = "zh-CN";

        // public event EventHandler CurrentServerChanged = null;

        AutoResetEvent eventComplete = new AutoResetEvent(false);
        bool m_bError = false;   // 最近一次异步操作是否因报错结束
        Exception m_exception = null;
        bool m_bErrorBox = true;
        bool m_bSetProgress = true;

        DigitalPlatform.Stop stop = null;
        bool m_bInSearching = false;

        MainForm m_mainForm = null;

        public MainForm MainForm
        {
            get
            {
                return this.m_mainForm;
            }
            set
            {
                this.m_mainForm = value;
            }
        }

        public AmazonSearchForm()
        {
            InitializeComponent();

            this.amazonSimpleQueryControl_multiLine.WordVisible = false;

            ListViewProperty prop = new ListViewProperty();
            this.listView_browse.Tag = prop;
        }

        #region ISearchForm 接口实现

        // 对象、窗口是否还有效?
        public bool IsValid()
        {
            if (this.IsDisposed == true)
                return false;

            return true;
        }

        public string CurrentProtocol
        {
            get
            {
                return "amazon";
            }
        }

        public string CurrentResultsetPath
        {
            get
            {
                /*
                string strServerName = "";
                string strServerUrl = "";
                string strDbName = "";
                string strFrom = "";
                string strFromStyle = "";

                string strError = "";

                int nRet = dp2ResTree.GetNodeInfo(this.dp2ResTree1.SelectedNode,
                    out strServerName,
                    out strServerUrl,
                    out strDbName,
                    out strFrom,
                    out strFromStyle,
                    out strError);
                if (nRet == -1)
                    return "";

                return strServerName
                    + "/" + strDbName
                    + "/" + strFrom
                    + "/" + this.textBox_simple_queryWord.Text
                    + "/default";
                 * */
                if (m_searchParameters == null)
                    return "";
                return m_strCurrentSearchedServer + "/" + m_searchParameters["Power"] + "/" + m_searchParameters["ResponseGroup"];
            }
        }


        // 刷新一条MARC记录
        // return:
        //      -2  不支持
        //      -1  error
        //      0   相关窗口已经销毁，没有必要刷新
        //      1   已经刷新
        //      2   在结果集中没有找到要刷新的记录
        public int RefreshOneRecord(
            string strPathParam,
            string strAction,
            out string strError)
        {
            strError = "尚未实现";

            return -2;
        }

        public int SyncOneRecord(string strPath,
            ref long lVersion,
            ref string strSyntax,
            ref string strMARC,
            out string strError)
        {
            strError = "";
            return 0;
        }

        DateTime m_timeLastReload;

        // 获得一条MARC/XML记录
        // return:
        //      -1  error 包括not found
        //      0   found
        //      1   为诊断记录
        public int GetOneRecord(
            string strStyle,
            int nTest,  // 暂时使用
            string strPathParam, // int index,
            string strParameters,   // bool bHilightBrowseLine,
            out string strSavePath,
            out string strRecord,
            out string strXmlFragment,
            out string strOutStyle,
            out byte[] baTimestamp,
            out long lVersion,
            out DigitalPlatform.OldZ3950.Record record,
            out Encoding currrentEncoding,
            out LoginInfo logininfo,
            out string strError)
        {
            strXmlFragment = "";
            strRecord = "";
            record = null;
            strError = "";
            currrentEncoding = Encoding.UTF8;   //  this.CurrentEncoding;
            baTimestamp = null;
            strSavePath = "";
            strOutStyle = "marc";
            logininfo = new LoginInfo();
            lVersion = 0;
            int nRet = 0;

            if (strStyle != "marc" && strStyle != "xml")
            {
                strError = "dp2SearchForm只支持获取MARC格式记录和xml格式记录，不支持 '" + strStyle + "' 格式的记录";
                return -1;
            }

            int index = -1;
            string strPath = "";
            string strDirection = "";
            nRet = Global.ParsePathParam(strPathParam,
                out index,
                out strPath,
                out strDirection,
                out strError);
            if (nRet == -1)
                return -1;

            ListViewItem curItem = null;

            if (index == -1)
            {
                // 找到 Item 行
                curItem = ListViewUtil.FindItem(this.listView_browse, strPath, 0);
                if (curItem == null)
                {
                    strError = "路径为 '" + strPath + "' 的事项在列表中没有找到";
                    return -1;
                }

                index = this.listView_browse.Items.IndexOf(curItem);

                //      strDirection    方向。为 prev/next/current之一。current可以缺省。
                if (strDirection == "prev")
                {
                    if (index == 0)
                    {
                        strError = "到头";
                        return -1;
                    }
                    index--;
                }
                else if (strDirection == "next")
                {
                    index++;
                }
            }

            {

            REDO:
                if (index >= this.listView_browse.Items.Count)
                {
                    if (this.m_nCurrentPageNo >= this.m_nTotalPages - 1)
                    {
                        strError = "越过结果集尾部";
                        return -1;
                    }

                    nRet = DoGetNextBatch(out strError);
                    if (nRet == -1)
                        return -1;

                    WaitSearchFinish();

                    goto REDO;
                }

                curItem = this.listView_browse.Items[index];
            }

            bool bHilightBrowseLine = StringUtil.IsInList("hilight_browse_line", strParameters);
            bool bForceFullElementSet = StringUtil.IsInList("force_full", strParameters);

            if (bHilightBrowseLine == true)
            {
                // 修改listview中事项的选定状态
                for (int i = 0; i < this.listView_browse.SelectedItems.Count; i++)
                {
                    this.listView_browse.SelectedItems[i].Selected = false;
                }

                curItem.Selected = true;
                curItem.EnsureVisible();
            }

            strPath = curItem.Text;
            ItemInfo info = (ItemInfo)curItem.Tag;

            if (bForceFullElementSet == true && info.ElementSet != "F")
            {
                // 需要重新装载这一条记录
                List<ListViewItem> items = new List<ListViewItem>();
                items.Add(curItem);

#if NO
                // 观察和上次操作间隔的时间。保证大于一秒
                TimeSpan delta = DateTime.Now - m_timeLastReload;
                if (delta < new TimeSpan(0,0,1))
                    Thread.Sleep(1000);
#endif

                int nRedoCount = 0;
            REDO_RELOAD:
                m_bErrorBox = false;
                nRet = ReloadItems(items,
                    0,
                    "F",
                    out strError);
                m_timeLastReload = DateTime.Now;
                if (nRet == -1)
                {
                    return -1;
                }
                bool bError = WaitSearchFinish();

                if (bError == true && this.m_exception != null && this.m_exception is WebException)
                {
                    WebException ex = this.m_exception as WebException;
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        // 重做
                        if (nRedoCount < 2)
                        {
                            nRedoCount++;
                            Thread.Sleep(1000);
                            goto REDO_RELOAD;
                        }

                        // 询问是否重做
                        DialogResult result = MessageBox.Show(this,
"重新装载时发生错误:\r\n\r\n" + strError + "\r\n\r\n是否重试?\r\n\r\n(Yes: 重试; No: 中断操作",
"AmazonSearchForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            Thread.Sleep(1000);
                            goto REDO_RELOAD;
                        }
                        return -1;
                    }
                }

                info = (ItemInfo)curItem.Tag;
            }

            record = new DigitalPlatform.OldZ3950.Record();
            record.m_baRecord = Encoding.UTF8.GetBytes(info.Xml);
            record.m_strDBName = m_searchParameters != null ? m_searchParameters["SearchIndex"] : "";
            record.m_strSyntaxOID = info.PreferSyntaxOID; // ???
            record.m_strElementSetName = info.ElementSet;    // B F

            strSavePath = this.CurrentProtocol + ":" + strPath;

            string strOutputPath = "";

            {
                string strContent = info.Xml;
                if (strStyle == "marc")
                {
                    // TODO: 转换为MARC
                    nRet = ConvertXmlToMarc(
                        record.m_strSyntaxOID,
                        strContent,
                        out strRecord,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    strOutStyle = "marc";
                    if (string.IsNullOrEmpty(record.m_strSyntaxOID) == true)
                        record.m_strSyntaxOID = "1.2.840.10003.5.1"; // UNIMARC
                    return 0;
                }

                // 不是MARCXML格式
                strRecord = strContent;
                strOutStyle = "xml";
                return 0;
            }
        }

        // 得到通用格式
        public int GetItemInfo(ItemInfo info,
            out string strMARC,
            out string strError)
        {
            strMARC = "";
            strError = "";

            string strSytaxOID = "1.2.840.10003.5.1";

            int nRet = ConvertXmlToMarc(
    strSytaxOID,
    info.Xml,
    out strMARC,
    out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // XslCompiledTransform m_xt = null;

        Hashtable m_xsltTable = new Hashtable();

        int ConvertXmlToMarc(
            string strMarcSyntaxOID,
            string strXml,
            out string strMarc,
            out string strError)
        {
            strError = "";
            strMarc = "";

            if (string.IsNullOrEmpty(strMarcSyntaxOID) == true)
                strMarcSyntaxOID = "1.2.840.10003.5.1"; // UNIMARC

            XmlDocument domData = new XmlDocument();
            domData.LoadXml(strXml);

            if (Control.ModifierKeys != Keys.Control
                && strMarcSyntaxOID == "1.2.840.10003.5.1")
            {
                List<string> styles = new List<string>();
                if (this.Create856 == false)
                    styles.Add("!856");

                // 将亚马逊 XML 格式转换为 UNIMARC 格式
                int nRet = AmazonSearch.AmazonXmlToUNIMARC(domData.DocumentElement,
                    StringUtil.MakePathList(styles),
                    out strMarc,
                    out strError);
                if (nRet == -1)
                    return -1;
                return 0;
            }

            {

                XslCompiledTransform xt = (XslCompiledTransform)this.m_xsltTable[strMarcSyntaxOID];

                if (xt == null)
                {
                    string strPrefix = strMarcSyntaxOID.Replace(".", "_");
                    string strXsltFilename = Path.Combine(this.MainForm.DataDir, strPrefix + "/amazon.xslt");   // 1_2_840_10003_5_1

                    XmlDocument temp = new XmlDocument();
                    temp.Load(strXsltFilename);

                    XmlReader xr = new XmlNodeReader(temp);

                    // 把xsl加到XslTransform
                    xt = new XslCompiledTransform();
                    XsltSettings settings = new XsltSettings();
                    settings.EnableScript = true;
                    xt.Load(xr, settings, null);

                    this.m_xsltTable[strMarcSyntaxOID] = xt;
                }

                // 输出到的地方
                string strResultXml = "";
                using (TextWriter tw = new StringWriter())
                using (XmlTextWriter xw = new XmlTextWriter(tw))
                {
                    //执行转换 
                    xt.Transform(domData.CreateNavigator(), /*null,*/ xw /*, null*/);

                    // tw.Close();
                    tw.Flush();
                    strResultXml = tw.ToString();
                }

                string strMarcSyntax = "";
                string strOutMarcSyntax = "";
                // 从数据记录中获得MARC格式
                int nRet = MarcUtil.Xml2Marc(strResultXml,
                    true,
                    strMarcSyntax,
                    out strOutMarcSyntax,
                    out strMarc,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML转换到MARC记录时出错: " + strError;
                    return -1;
                }

                return 0;
            }
        }

        #endregion

        private void AmazonSearchForm_Load(object sender, EventArgs e)
        {
            this.MainForm.AppInfo.LoadMdiLayout += new EventHandler(AppInfo_LoadMdiLayout);
            this.MainForm.AppInfo.SaveMdiLayout += new EventHandler(AppInfo_SaveMdiLayout);

            LoadSize();

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联

            string strWidths = this.MainForm.AppInfo.GetString(
"amazonsearchform",
"record_list_column_width",
"");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_browse,
                    strWidths,
                    true);
            }
#if NO
            string strCfgFileName = Path.Combine( this.MainForm.DataDir, "amazon_searchcontrol.xml");
            string strError = "";
            int nRet = this.amazonQueryControl1.Initial(strCfgFileName, out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
#endif

            this.amazonSimpleQueryControl_simple.Initial();
            this.amazonSimpleQueryControl_simple.SetContentString(
                this.MainForm.AppInfo.GetString("amazonsearchform",
                "simple_query_content",
                "")
                );

            this.amazonSimpleQueryControl_multiLine.Initial();
            this.amazonSimpleQueryControl_multiLine.SetContentString(
    this.MainForm.AppInfo.GetString("amazonsearchform",
    "multiline_query_content",
    "")
    );
            // 让按钮文字显示出来
            this.RefreshUI();
        }

        private void AmazonSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void AmazonSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.Style = StopStyle.None;    // 需要强制中断
                stop.DoStop();

                stop.Unregister();	// 和容器关联
                stop = null;
            }

            DeleteTempFile();

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_browse);
                this.MainForm.AppInfo.SetString(
                    "amazonsearchform",
                    "record_list_column_width",
                    strWidths);

                this.MainForm.AppInfo.SetString("amazonsearchform",
                    "simple_query_content",
                    this.amazonSimpleQueryControl_simple.GetContentString(true));
                this.MainForm.AppInfo.SetString("amazonsearchform",
                    "multiline_query_content",
                    this.amazonSimpleQueryControl_multiLine.GetContentString(false));

                SaveSize();

                this.MainForm.AppInfo.LoadMdiLayout -= new EventHandler(AppInfo_LoadMdiLayout);
                this.MainForm.AppInfo.SaveMdiLayout -= new EventHandler(AppInfo_SaveMdiLayout);
            }
        }

        // 准备临时文件名
        void PrepareTempFile()
        {
            // 如果以前有临时文件名，就直接沿用
            if (string.IsNullOrEmpty(this.TempFilename) == true)
            {
                this.TempFilename = Path.Combine(this.MainForm.DataDir, "~webclient_response_" + Guid.NewGuid().ToString());
            }

            try
            {
                File.Delete(this.TempFilename);
            }
            catch
            {
            }
        }

        void DeleteTempFile()
        {
            // 删除临时文件
            if (string.IsNullOrEmpty(this.TempFilename) == false)
            {
                try
                {
                    File.Delete(this.TempFilename);
                }
                catch
                {
                }
                this.TempFilename = "";
            }
        }

        public void AppInfo_LoadMdiLayout(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            try
            {
                // 获得splitContainer_main的状态
                this.MainForm.LoadSplitterPos(
                    this.splitContainer_main,
                    "amazonsearchform",
                    "splitContainer_main");

#if NO
                // 获得splitContainer_up的状态
                this.MainForm.LoadSplitterPos(
                    this.splitContainer_up,
                    "amazonsearchform",
                    "splitContainer_up");

                // 获得splitContainer_queryAndResultInfo的状态
                this.MainForm.LoadSplitterPos(
                    this.splitContainer_queryAndResultInfo,
                    "amazonsearchform",
                    "splitContainer_queryAndResultInfo");
#endif
            }
            catch
            {
            }
        }

        void AppInfo_SaveMdiLayout(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                // 分割条位置
                // 保存splitContainer_main的状态
                this.MainForm.SaveSplitterPos(
                    this.splitContainer_main,
                    "amazonsearchform",
                    "splitContainer_main");
            }
        }

        public void LoadSize()
        {
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state");

            /*
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "dp2_search_state",
                MainForm.DefaultMdiWindowWidth,
                MainForm.DefaultMdiWindowHeight);
            */
        }

        public void SaveSize()
        {
            /*
            MainForm.AppInfo.SaveMdiChildFormStates(this,
                "dp2_search_state");
            */
            MainForm.AppInfo.SaveMdiChildFormStates(this,
    "mdi_form_state");

        }

        // 2016/4/8
        public void SetQueryContent(string strUse, string strWord)
        {
            this.tabControl_query.SelectedTab = this.tabPage_simple;
            this.amazonSimpleQueryControl_simple.SetContent(strUse, strWord);
        }

        // 准备用于首次检索的 URL
        int GetSimpleSearchRequestUrl(out string strUrl,
            out string strError)
        {
            strError = "";
            strUrl = "";

            string strText = "";
            int nRet = this.amazonSimpleQueryControl_simple.BuildQueryString(out strText, out strError);
            if (nRet == -1)
                return -1;

            dp2Catalog.SystemCfgForm.SecretInfo info = SystemCfgForm.GetSecretInfo(this.MainForm.UserDir,
                "");

            AmazonSignedRequestHelper helper = null;
            if (info == null || string.IsNullOrEmpty(info.AwsAccessKeyID) == true)
            {
                helper = new AmazonSignedRequestHelper(
                    //MY_AWS_ACCESS_KEY_ID,
                    //MY_AWS_SECRET_KEY,
     this.CurrentServer);
            }
            else
            {
                helper = new AmazonSignedRequestHelper(
                    info.AwsAccessKeyID,
                    info.AwsSecretKey,
                this.CurrentServer);
            }
            // IDictionary<string, string> r1 = this.amazonQueryControl1.ParameterTable;   // new Dictionary<string, String>();

            IDictionary<string, string> parameters = new Dictionary<string, String>();

            parameters["Service"] = "AWSECommerceService";
            parameters["Version"] = API_VERSION;    // "2011-08-01";
            parameters["Operation"] = "ItemSearch";
            parameters["SearchIndex"] = "Books";
            parameters["Power"] = strText;

            if (this.AlwaysUseFullElementSet == true)
                parameters["ResponseGroup"] = "Large";
            else
                parameters["ResponseGroup"] = "Small";

#if TTT
            parameters["AssociateTag"] = ASSOCIATEKEY;
#endif

            m_searchParameters = parameters;
            m_strCurrentSearchedServer = this.CurrentServer;

            strUrl = helper.Sign(parameters);
            return 0;
        }

        // 准备用于获得下一批浏览记录的请求的 URL
        int GetNextRequestUrl(out string strUrl,
            out string strError)
        {
            strError = "";
            strUrl = "";

            if (string.IsNullOrEmpty(this.m_strCurrentSearchedServer) == true)
            {
                strError = "this.m_strCurrentSearchedServer 为空";
                return -1;
            }

            dp2Catalog.SystemCfgForm.SecretInfo info = SystemCfgForm.GetSecretInfo(this.MainForm.UserDir,
    "");

            AmazonSignedRequestHelper helper = null;
            
            if (info == null || string.IsNullOrEmpty(info.AwsAccessKeyID) == true)
            {
                helper = new AmazonSignedRequestHelper(
                    //MY_AWS_ACCESS_KEY_ID,
                    //MY_AWS_SECRET_KEY,
                    this.m_strCurrentSearchedServer);    //  this.CurrentServer
            }
            else
            {
                helper = new AmazonSignedRequestHelper(
                    info.AwsAccessKeyID,
                    info.AwsSecretKey,
                    this.m_strCurrentSearchedServer);
            }

            if (this.m_nCurrentPageNo == -1)
            {
                strError = "m_nCurrentPageNo 尚未初始化";
                return -1;
            }

            // ItemPage URL 参数的值是从 1 开始计算的
            m_searchParameters["ItemPage"] = (this.m_nCurrentPageNo + 1 + 1).ToString();

            strUrl = helper.Sign(m_searchParameters);
            return 0;
        }

        // 准备用于重新装载的请求的 URL
        int GetReloadRequestUrl(
            List<ListViewItem> items,
            string strElementSet,
            out string strUrl,
            out string strError)
        {
            strError = "";
            strUrl = "";

            AmazonSignedRequestHelper helper = new AmazonSignedRequestHelper(
                //MY_AWS_ACCESS_KEY_ID,
                //MY_AWS_SECRET_KEY,
this.CurrentServer);

            if (items == null || items.Count == 0)
            {
                strError = "items 参数不应为空";
                return -1;
            }

            // 获得 ASIN 字符串
            StringBuilder asin = new StringBuilder(4096);
            foreach (ListViewItem item in items)
            {
                if (asin.Length > 0)
                    asin.Append(",");
                asin.Append(item.Text);
            }
            IDictionary<string, string> parameters = new Dictionary<string, String>();

            parameters["Service"] = "AWSECommerceService";
            parameters["Version"] = API_VERSION;    // "2011-08-01";
            parameters["Operation"] = "ItemLookup";
            parameters["ItemId"] = asin.ToString();
            parameters["IdType"] = "ASIN";
            if (strElementSet == "F")
                parameters["ResponseGroup"] = "Large";
            else
                parameters["ResponseGroup"] = "Small";

#if TTT
            parameters["AssociateTag"] = ASSOCIATEKEY;
#endif

            strUrl = helper.Sign(parameters);
            return 0;
        }

        string m_strCurrentServerCfgString = "";

        public string CurrentServerCfgString
        {
            get
            {
                if (string.IsNullOrEmpty(m_strCurrentServerCfgString) == false)
                    return m_strCurrentServerCfgString;

                return this.DefaultServerCfgString;
            }
            set
            {
                m_strCurrentServerCfgString = value;

                this.RefreshUI();
            }
        }

        public void RefreshUI()
        {
            this.button_searchSimple.Text = "检索 " + this.CurrentServerCountry;
        }

        public string CurrentServer
        {
            get
            {
                string strLeft = "";
                string strRight = "";
                AmazonQueryControl.ParseLeftRight(this.CurrentServerCfgString,
                    out strLeft,
                    out strRight);
                if (string.IsNullOrEmpty(strRight) == false)
                    return strRight;
                return strLeft;
            }
        }

        public string CurrentServerCountry
        {
            get
            {
                string strLeft = "";
                string strRight = "";
                AmazonQueryControl.ParseLeftRight(this.CurrentServerCfgString,
                    out strLeft,
                    out strRight);
                return strLeft;
            }
        }

        public string DefaultServerCfgString
        {
            get
            {
                return this.MainForm.AppInfo.GetString(
"amazon_search_form",
"default_server",
"中国\twebservices.amazon.cn");
            }
            set
            {
                this.MainForm.AppInfo.SetString(
"amazon_search_form",
"default_server",
value);
                this.RefreshUI();
            }
        }

        public string DefaultServer
        {
            get
            {
                string strLeft = "";
                string strRight = "";
                AmazonQueryControl.ParseLeftRight(this.DefaultServerCfgString,
                    out strLeft,
                    out strRight);
                if (string.IsNullOrEmpty(strRight) == false)
                    return strRight;
                return strLeft;
            }
        }

        // private const string DESTINATION = "webservices.amazon.cn";  // "ecs.amazonaws.com";

        // 简单检索
        private void button_searchSimple_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = DoSimpleSearch(out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        public int DoSearch()
        {
            string strError = "";
            // MessageBox.Show(this, ((ParameterTable)this.amazonQueryControl1.ParameterTable).Dump());

#if NO
            {
                string strText = "";
                int nRet = this.amazonSimpleQueryControl1.BuildQueryString(out strText, out strError);
                if (nRet == -1)
                    goto ERROR1;
                MessageBox.Show(this, strText);
            }
            return;
#endif

            if (this.tabControl_query.SelectedTab == this.tabPage_simple)
            {
                int nRet = DoSimpleSearch(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else if (this.tabControl_query.SelectedTab == this.tabPage_multiline)
            {
                int nRet = DoMultilineSearch(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        string m_strCurrentSearchedServer = ""; // 当前检索过的服务器名
        IDictionary<string, string> m_searchParameters = null;  // 当前检索参数
        int m_nCurrentPageNo = -1;  // 当前已经装入的最后一个页号
        long m_nTotalResults = 0;    // 当前检索命中的结果数
        long m_nTotalPages = 0;     // 命中结果的总页数
        string m_strError = ""; // 异步操作中用于保存出错信息

        private const string NAMESPACE = "http://webservices.amazon.com/AWSECommerceService/2013-08-01";

        WebClient webClient = null;
        public string TempFilename = "";

        #region 多行检索

        class MultiLineSearchInfo
        {
            public DateTime StartTime;  // 开始检索的时间  
            public int HitCount = 0;    // 总共命中后后装入浏览框的记录条数
            public List<string> HitWords = new List<string>();  // 命中的检索词和命中数
            public List<string> NotHitWords = new List<string>();

            public string CurrentWord = ""; // 当前正在检索的检索词
        }

        MultiLineSearchInfo m_multiSearchInfo = null;

        // 多行检索
        // 多行命中后 NextBatch 等按钮都失效
        int DoMultilineSearch(out string strError)
        {
            strError = "";
            int nRet = 0;

            this.ClearListViewItems();
            this.ClearResultSetParameters();
            this.amazonSimpleQueryControl_multiLine.Comment = "正在检索 ...";
            m_reloadInfo = null;
            this.m_strError = "";

            // 存储检索中间信息
            this.m_multiSearchInfo = new MultiLineSearchInfo();
            this.m_multiSearchInfo.StartTime = DateTime.Now;

            this.PrepareTempFile();

            this.BeginLoop("正在检索 ...");
            this.m_bErrorBox = true;

            StopStyle oldstyle = this.stop.Style |= StopStyle.EnableHalfStop;   // 柔和中断
            try
            {
                stop.SetProgressRange(0, this.textBox_mutiline_queryContent.Lines.Length);
                int i = 0;
                foreach (string s in this.textBox_mutiline_queryContent.Lines)
                {
                    Application.DoEvents(); // 让界面变化的效果尽快显示出来。另外好像还能加速检索响应?

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    if (string.IsNullOrEmpty(s) == true)
                        goto CONTINUE;
                    string strLine = s.Trim();
                    if (string.IsNullOrEmpty(strLine) == true)
                        goto CONTINUE;

                    // 多行检索中的一行检索
                    int nRedoCount = 0;
                    Thread.Sleep(1000);
                REDO:
                    nRet = SearchOneLine(strLine,
                        out strError);
                    if (nRet == -1)
                    {
                        if (this.m_exception != null && this.m_exception is WebException)
                        {
                            WebException e = this.m_exception as WebException;
                            if (e.Status == WebExceptionStatus.ProtocolError)
                            {
                                // 重做
                                if (nRedoCount < 2)
                                {
                                    nRedoCount++;
                                    Thread.Sleep(1000);
                                    goto REDO;
                                }

                                // 询问是否重做
                                DialogResult result = MessageBox.Show(this,
"检索 '" + strLine + "' 时发生错误:\r\n\r\n" + strError + "\r\n\r\n是否重试?\r\n\r\n(Yes: 重试; No: 跳过这一行继续检索后面的行； Cancel: 中断整个检索操作",
"AmazonSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                                if (result == System.Windows.Forms.DialogResult.Retry)
                                {
                                    Thread.Sleep(1000);
                                    goto REDO;
                                }
                                if (result == System.Windows.Forms.DialogResult.Cancel)
                                    return -1;
                                goto CONTINUE;
                            }
                        }
                        return -1;
                    }

                CONTINUE:
                    i++;
                    stop.SetProgressValue(i);
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            finally
            {
                EndLoop();
                this.stop.Style = oldstyle;
            }

            // 显示结果信息
            TimeSpan delta = DateTime.Now - this.m_multiSearchInfo.StartTime;
            StringBuilder text = new StringBuilder(4096);

            text.Append("检索 " + (this.m_multiSearchInfo.HitWords.Count + this.m_multiSearchInfo.NotHitWords.Count).ToString() + " 行用时 " + delta.ToString() + "\r\n");
            if (this.m_multiSearchInfo.HitWords.Count > 0)
            {
                text.Append("*** 以下 (" + this.m_multiSearchInfo.HitWords.Count + " 个) 检索词共命中 " + this.m_multiSearchInfo.HitCount + " 条:\r\n");
                foreach (string s in this.m_multiSearchInfo.HitWords)
                {
                    text.Append(s + "\r\n");
                }
            }
            if (this.m_multiSearchInfo.NotHitWords.Count > 0)
            {
                text.Append("*** 以下 (" + this.m_multiSearchInfo.NotHitWords.Count + " 个) 检索词没有命中:\r\n");
                foreach (string s in this.m_multiSearchInfo.NotHitWords)
                {
                    text.Append(s + "\r\n");
                }
            }
            this.amazonSimpleQueryControl_multiLine.Comment = text.ToString();

            // TODO: 禁止 NextBatch / FullBatch 按钮

            return 0;
        }

        // 准备多行检索中用于一行检索的 URL
        int GetOneLineSearchRequestUrl(
            string strLine,
            out string strUrl,
            out string strError)
        {
            strError = "";
            strUrl = "";

            if (string.IsNullOrEmpty(strLine) == true)
            {
                strError = "strLine 参数值不能为空";
                return -1;
            }

            this.amazonSimpleQueryControl_multiLine.Word = strLine;

            string strText = "";
            int nRet = this.amazonSimpleQueryControl_multiLine.BuildQueryString(out strText, out strError);
            if (nRet == -1)
                return -1;

            AmazonSignedRequestHelper helper = new AmazonSignedRequestHelper(
                //MY_AWS_ACCESS_KEY_ID,
                //MY_AWS_SECRET_KEY,
this.CurrentServer);

            IDictionary<string, string> parameters = new Dictionary<string, String>();

            parameters["Service"] = "AWSECommerceService";
            parameters["Version"] = API_VERSION;    // "2011-08-01";
            parameters["Operation"] = "ItemSearch";
            parameters["SearchIndex"] = "Books";
            parameters["Power"] = strText;

            if (this.AlwaysUseFullElementSet == true)
                parameters["ResponseGroup"] = "Large";
            else
                parameters["ResponseGroup"] = "Small";

#if TTT
            parameters["AssociateTag"] = ASSOCIATEKEY;
#endif

            m_searchParameters = parameters;
            m_strCurrentSearchedServer = this.CurrentServer;

            strUrl = helper.Sign(parameters);
            return 0;
        }

        // 多行检索中的一行检索
        int SearchOneLine(string strLine,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strUrl = "";
            nRet = GetOneLineSearchRequestUrl(
                strLine,
                out strUrl,
                out strError);
            if (nRet == -1)
                return -1;

            this.m_multiSearchInfo.CurrentWord = strLine;

            m_reloadInfo = null;

            this.PrepareTempFile();

            this.m_bError = false;
            this.m_exception = null;

#if NO
            if (this.webClient != null)
            {
                webClient.DownloadFileCompleted -= new AsyncCompletedEventHandler(webClient_MultiLineDownloadFileCompleted);
                this.webClient.Dispose();
                this.webClient = null;
            }
#endif
            webClient = new WebClient();

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_MultiLineDownloadFileCompleted);
            // webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            try
            {
                webClient.DownloadFileAsync(new Uri(strUrl, UriKind.Absolute),
                    this.TempFilename, null);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                this.m_bError = true;
                return -1;
            }

            // 等待检索结束
            bool bError = WaitSearchFinish();
            if (bError == true)
            {
                strError = this.m_strError;
                return -1;
            }

            // 如果要求每行的检索命中装入大于 10 条，需要在这里获取后面几批的浏览结果

            return 0;
        }

        void webClient_MultiLineDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Delegate_EndSearch d = new Delegate_EndSearch(EndMultiLineSearch);
            object[] args = new object[] { e };
            this.Invoke(d, args);

            if (this.eventComplete != null)
                this.eventComplete.Set();
        }

        void EndMultiLineSearch(AsyncCompletedEventArgs e)
        {
            string strError = "";
            if (e == null || e.Cancelled == true)
            {
                strError = "请求被取消";
                goto ERROR1;
            }

            if (e != null && e.Error != null)
            {
                strError = "请求过程发生错误: " + ExceptionUtil.GetExceptionMessage(e.Error);
                this.m_exception = e.Error; // 2013/4/25
                goto ERROR1;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(this.TempFilename);    ///

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("amazon", NAMESPACE);

            XmlNodeList errors = doc.DocumentElement.SelectNodes("amazon:Items/amazon:Request/amazon:Errors/amazon:Error", nsmgr);
            if (errors.Count > 0)
            {
                string strCode = DomUtil.GetElementText(errors[0], "amazon:Code", nsmgr);
                string strMessage = DomUtil.GetElementText(errors[0], "amazon:Message", nsmgr);
                if (strCode == "AWS.ECommerceService.NoExactMatches")
                {
                    strError = strCode;
                    goto ERROR1;
                }
                strError = strMessage;
                goto ERROR1;
            }

            if (this.m_reloadInfo == null)
            {
                // Items/TotalResults
                string strTotalResults = DomUtil.GetElementText(doc.DocumentElement,
                    "amazon:Items/amazon:TotalResults", nsmgr);
                Int64.TryParse(strTotalResults, out this.m_nTotalResults);

                // Items/TotalPages
                string strTotalPages = DomUtil.GetElementText(doc.DocumentElement,
                    "amazon:Items/amazon:TotalPages", nsmgr);
                Int64.TryParse(strTotalPages, out this.m_nTotalPages);

                // TODO: 显示单行命中的消息
                if (m_nTotalResults > 0)
                {
                    int nHitCount = (int)Math.Min(m_nTotalResults, 10);
                    this.m_multiSearchInfo.HitWords.Add(this.m_multiSearchInfo.CurrentWord + "\t" + nHitCount.ToString());
                    this.m_multiSearchInfo.HitCount += nHitCount;
                }
                else
                    this.m_multiSearchInfo.NotHitWords.Add(this.m_multiSearchInfo.CurrentWord);

                // this.amazonSimpleQueryControl_simple.Comment = "命中记录:\t" + m_nTotalResults.ToString();
            }

            int nRet = LoadResults(doc, false, out strError);
            if (nRet == -1)
                goto ERROR1;

            if (this.m_reloadInfo == null)
            {
                this.m_nCurrentPageNo++;    // 第一次 从 -1 ++ 正好等于 0
            }
            return;
        ERROR1:
            if (this.m_reloadInfo != null)
                this.m_reloadInfo.Cancel = true;

            this.DeleteTempFile();

            if (strError == "AWS.ECommerceService.NoExactMatches")
            {
                // TODO: 显示单行没有命中的消息
                // this.amazonSimpleQueryControl_simple.Comment = "没有命中";
                this.m_multiSearchInfo.NotHitWords.Add(this.m_multiSearchInfo.CurrentWord);
                return;
            }
            // TODO: 显示单行检索出错的消息
            // this.amazonSimpleQueryControl_simple.Comment = "检索发生错误:\r\n" + strError;
            this.m_bError = true;
            this.m_strError = strError;
        }

        #endregion // 多行检索

        // 简单检索
        // 本函数并未直接完成操作，后面还有一个异步处理的部分
        int DoSimpleSearch(out string strError)
        {
            strError = "";
            int nRet = 0;

            // this.listView_browse.Items.Clear();
            this.ClearListViewItems();
            this.ClearResultSetParameters();
            this.amazonSimpleQueryControl_simple.Comment = "正在检索 ...";
            this.m_strError = "";

            string strUrl = "";
            nRet = GetSimpleSearchRequestUrl(out strUrl,
                out strError);
            if (nRet == -1)
                return -1;

            m_reloadInfo = null;

            this.PrepareTempFile();

            this.BeginLoop("正在检索 ...");
            this.m_bErrorBox = true;

            this.PrepareTempFile();
            webClient = new WebClient();

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);
            if (m_bSetProgress == true)
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            try
            {
                webClient.DownloadFileAsync(new Uri(strUrl, UriKind.Absolute),
                    this.TempFilename, null);
            }
            catch (Exception ex)
            {
                EndLoop();
                strError = ExceptionUtil.GetAutoText(ex);
                this.m_bError = true;
                return -1;
            }

            return 0;
        }

        void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.stop.SetProgressValue(e.ProgressPercentage);
        }

        void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Delegate_EndSearch d = new Delegate_EndSearch(EndSimpleSearch);
            object[] args = new object[] { e };
            this.Invoke(d, args);

            if (this.eventComplete != null)
                this.eventComplete.Set();
        }

        delegate void Delegate_EndSearch(AsyncCompletedEventArgs e);

        void EndSimpleSearch(AsyncCompletedEventArgs e)
        {
            string strError = "";
            if (e == null || e.Cancelled == true)
            {
                strError = "请求被取消";
                goto ERROR1;
            }

            if (e != null && e.Error != null)
            {
                strError = "请求过程发生错误: " + ExceptionUtil.GetExceptionMessage(e.Error);
                goto ERROR1;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(this.TempFilename);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("amazon", NAMESPACE);

            XmlNodeList errors = doc.DocumentElement.SelectNodes("amazon:Items/amazon:Request/amazon:Errors/amazon:Error", nsmgr);
            if (errors.Count > 0)
            {
                string strCode = DomUtil.GetElementText(errors[0], "amazon:Code", nsmgr);
                string strMessage = DomUtil.GetElementText(errors[0], "amazon:Message", nsmgr);
                if (strCode == "AWS.ECommerceService.NoExactMatches")
                {
                    strError = strCode;
                    goto ERROR1;
                }
                strError = strMessage;
                goto ERROR1;
            }

#if NO
            XmlNodeList errorMessageNodes = doc.GetElementsByTagName("Message", NAMESPACE);
            if (errorMessageNodes != null && errorMessageNodes.Count > 0)
            {
                String message = errorMessageNodes.Item(0).InnerText;
                strError = "Error: " + message + " (but signature worked)";
                goto ERROR1;
            }
#endif

            if (this.m_reloadInfo == null)
            {
                // Items/TotalResults
                string strTotalResults = DomUtil.GetElementText(doc.DocumentElement,
                    "amazon:Items/amazon:TotalResults", nsmgr);
                Int64.TryParse(strTotalResults, out this.m_nTotalResults);

                // Items/TotalPages
                string strTotalPages = DomUtil.GetElementText(doc.DocumentElement,
                    "amazon:Items/amazon:TotalPages", nsmgr);
                Int64.TryParse(strTotalPages, out this.m_nTotalPages);

                this.amazonSimpleQueryControl_simple.Comment = "命中记录:\t" + m_nTotalResults.ToString();
            }

            // TODO: 可以用一个回调函数实现显示信息
            int nRet = LoadResults(doc, true, out strError);
            if (nRet == -1)
                goto ERROR1;

            if (this.m_reloadInfo == null)
            {
                // this.m_nCurrentPageNo = 0;
                this.m_nCurrentPageNo++;    // 第一次 从 -1 ++ 正好等于 0
                RefreshNextBatchButtons();
            }
            EndLoop();
            return;
        ERROR1:
            EndLoop();
            if (this.m_reloadInfo != null)
                this.m_reloadInfo.Cancel = true;

            this.DeleteTempFile();

            if (strError == "AWS.ECommerceService.NoExactMatches")
            {
                this.amazonSimpleQueryControl_simple.Comment = "没有命中";
                return;
            }

            if (this.m_bErrorBox == true)
            {
                MessageBox.Show(this, strError);
                this.amazonSimpleQueryControl_simple.Comment = "检索发生错误:\r\n" + strError;
            }
            this.m_bError = true;
        }

        // parameters:
        //      bSetProgress    是否要设置进度
        void BeginLoop(string strMessage,
            bool bSetProgress = true)
        {
            this.EnableControlsInSearching(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            if (bSetProgress == true)
            {
                stop.Initial(strMessage);
                stop.BeginLoop();
                stop.SetProgressRange(0, 100);
            }
            else
                stop.SetMessage(strMessage);

            this.m_bInSearching = true;

            if (eventComplete != null)
            {
                this.m_bError = false;
                this.m_exception = null;
                eventComplete.Reset();
            }

            this.m_bSetProgress = bSetProgress;

            Application.DoEvents(); // 让界面变化的效果尽快显示出来。另外好像还能加速检索响应?
        }

        void EndLoop()
        {
            if (this.m_bSetProgress == true)
            {
                stop.EndLoop();
                stop.Initial("");
                stop.HideProgress();
            }
            stop.OnStop -= new StopEventHandler(this.DoStop);

            this.EnableControlsInSearching(true);

            this.m_bInSearching = false;

            if (this.eventComplete != null)
                this.eventComplete.Set();
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.webClient != null)
                this.webClient.CancelAsync();
        }

        // 允许或者禁止所有控件
        void EnableControls(bool bEnable)
        {
            this.listView_browse.Enabled = bEnable;
            EnableControlsInSearching(bEnable);
        }

        // 允许或者禁止大部分控件，除listview以外
        void EnableControlsInSearching(bool bEnable)
        {
            this.amazonSimpleQueryControl_simple.Enabled = bEnable;

            this.button_searchSimple.Enabled = bEnable;
        }

        public void GetAllRecords()
        {
            while (true)
            {
                if (this.m_nCurrentPageNo == -1)
                    break;

                if (this.m_nTotalPages == 0)
                    break;

                if (this.m_nCurrentPageNo >= this.m_nTotalPages - 1)
                    break;

                string strError = "";
                int nRet = DoGetNextBatch(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                // 异步后半段出错如何探测到?

                if (WaitSearchFinish() == true)
                    break;
            }
        }

        public int NextBatch()
        {
            string strError = "";
            int nRet = DoGetNextBatch(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return 0;
            }

            return -1;
        }

        // 获得下一批浏览结果
        // 本函数并未直接完成操作，后面还有一个异步处理的部分
        int DoGetNextBatch(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.m_nTotalPages == 0)
            {
                strError = "尚未检索过";
                return -1;
            }

            if (this.m_nCurrentPageNo >= this.m_nTotalPages - 1)
            {
                strError = "结果集已经全部装入";
                return -1;
            }

            string strUrl = "";
            nRet = GetNextRequestUrl(out strUrl,
                out strError);
            if (nRet == -1)
                return -1;

            m_reloadInfo = null;

            this.BeginLoop("正在获取下一批记录 (" + ((this.m_nCurrentPageNo + 1 + 1) * 10 + 1).ToString() + "- ) ...");
            this.m_bErrorBox = true;

            this.PrepareTempFile();
            webClient = new WebClient();

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);
            if (m_bSetProgress == true)
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            try
            {
                webClient.DownloadFileAsync(new Uri(strUrl, UriKind.Absolute),
                    this.TempFilename, null);
            }
            catch (Exception ex)
            {
                this.EndLoop();
                strError = ExceptionUtil.GetAutoText(ex);
                this.m_bError = true;
                return -1;
            }

            return 0;
        }

        // 重新装载浏览记录的任务信息
        class ReloadTaskInfo
        {
            public string ElementSet = "";
            public List<ListViewItem> TotalItems = null;
            public List<ListViewItem> CurrentItems = null;
            public int StartIndex = 0;  // 开始的偏移
            public bool Cancel = false; // 是否中途放弃，或者因为出错而需要中断
        }

        ReloadTaskInfo m_reloadInfo = null;

        // 刷新一批浏览结果
        // 本函数并未直接完成操作，后面还有一个异步处理的部分
        int ReloadItems(List<ListViewItem> items,
            int nStartIndex,
            string strElementSet,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 第一轮开始
            if (nStartIndex == 0)
            {
                this.m_reloadInfo = new ReloadTaskInfo();
                this.m_reloadInfo.TotalItems = items;
                this.m_reloadInfo.ElementSet = strElementSet;

                this.stop.SetProgressRange(0, items.Count);
                this.stop.SetProgressValue(0);
            }

            this.stop.SetProgressValue(nStartIndex);

            Debug.Assert(this.m_reloadInfo != null, "");

            this.m_reloadInfo.StartIndex = nStartIndex;

            List<ListViewItem> temp = new List<ListViewItem>();
            int nCount = 0;
            for (int i = nStartIndex; i < items.Count && nCount < 10; i++)
            {
                temp.Add(items[i]);
                nCount++;
            }

            this.m_reloadInfo.CurrentItems = temp;

            string strUrl = "";
            nRet = GetReloadRequestUrl(
                temp,
                strElementSet,
                out strUrl,
                out strError);
            if (nRet == -1)
                return -1;

            this.BeginLoop("正在重新装载记录 " + (nStartIndex + 1).ToString() + " / " + items.Count.ToString() + " ...", false);

            this.PrepareTempFile();
            webClient = new WebClient();

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_ReloadDownloadFileCompleted);
            if (m_bSetProgress == true)
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            try
            {
                webClient.DownloadFileAsync(new Uri(strUrl, UriKind.Absolute),
                    this.TempFilename, null);
            }
            catch (Exception ex)
            {
                this.EndLoop();
                strError = ExceptionUtil.GetAutoText(ex);
                this.m_bError = true;
                return -1;
            }

            return 0;
        }

        void webClient_ReloadDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Delegate_EndSearch d = new Delegate_EndSearch(EndSimpleSearch);
            object[] args = new object[] { e };
            this.Invoke(d, args);

            // 如果没有完成，还要继续做
            if (this.m_reloadInfo.Cancel == false &&
                this.m_reloadInfo.StartIndex + this.m_reloadInfo.CurrentItems.Count < this.m_reloadInfo.TotalItems.Count)
            {
                string strError = "";
                int nRedoCount = 0;
                Thread.Sleep(1000);
            REDO:
                int nRet = ReloadItems(this.m_reloadInfo.TotalItems,
                    this.m_reloadInfo.StartIndex + this.m_reloadInfo.CurrentItems.Count,
                    this.m_reloadInfo.ElementSet,
                    out strError);
                if (nRet == -1)
                {
                    if (this.m_exception != null && this.m_exception is WebException)
                    {
                        WebException ex = this.m_exception as WebException;
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            // 重做
                            if (nRedoCount < 2)
                            {
                                nRedoCount++;
                                Thread.Sleep(1000);
                                goto REDO;
                            }

                            // 询问是否重做
                            DialogResult result = MessageBox.Show(this,
"重新装载时发生错误:\r\n\r\n" + strError + "\r\n\r\n是否重试?\r\n\r\n(Yes: 重试; No: 中断操作",
"AmazonSearchForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.Yes)
                            {
                                Thread.Sleep(1000);
                                goto REDO;
                            }
                        }
                    }
                    this.amazonSimpleQueryControl_simple.Comment = strError;
                }
                return;
            }

            // 清除进度条
            if (this.m_reloadInfo.StartIndex + this.m_reloadInfo.CurrentItems.Count >= this.m_reloadInfo.TotalItems.Count)
            {
                this.stop.HideProgress();
                this.stop.SetMessage("");
            }

            if (this.eventComplete != null)
                this.eventComplete.Set();
        }

        // 装入检索命中记录到浏览列表中
        int LoadResults(XmlDocument response_dom,
            bool bDisplayInfo,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strElementSet = "";

            // 建立 ASIN --> ListViewItem 对照表
            Hashtable table = new Hashtable();
            if (this.m_reloadInfo != null)
            {
                foreach (ListViewItem item in this.m_reloadInfo.CurrentItems)
                {
                    table[item.Text] = item;
                }
                strElementSet = this.m_reloadInfo.ElementSet;
            }
            else
            {
                if (this.AlwaysUseFullElementSet == true)
                    strElementSet = "F";
                else
                    strElementSet = "B";
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("amazon", NAMESPACE);

            XmlNodeList items = response_dom.DocumentElement.SelectNodes("amazon:Items/amazon:Item", nsmgr);

            foreach (XmlNode item in items)
            {
                var element = item as XmlElement;

                List<string> cols = null;
                string strASIN = "";
                nRet = ParseItemXml(element,
                    nsmgr,
                    out strASIN,
                    out cols,
                    out strError);
                if (nRet == -1)
                    return -1;

                ItemInfo info = new ItemInfo();
                info.Xml = item.OuterXml;
                info.ElementSet = strElementSet;

                ListViewItem listitem = new ListViewItem();
                if (this.m_reloadInfo != null)
                {
                    // 定位覆盖
                    listitem = (ListViewItem)table[strASIN];
                    if (listitem == null)
                    {
                        strError = "table 中没有找到 key 为 '" + strASIN + "' 的事项";
                        return -1;
                    }
                    for (int i = 0; i < cols.Count; i++)
                    {
                        ListViewUtil.ChangeItemText(listitem, i + 1, cols[i]);
                    }

                    table.Remove(strASIN);  // 用后从 hashtable 中删除
                }
                else
                {
                    listitem = new ListViewItem();
                    listitem.Text = strASIN;
                    foreach (string s in cols)
                    {
                        listitem.SubItems.Add(s);
                    }
                    this.listView_browse.Items.Add(listitem);
                    SetLineStyle(listitem);
                }
                listitem.Tag = info;

                if (info.ElementSet == "B")
                    listitem.ImageIndex = BROWSE_TYPE_BRIEF;
                else
                    listitem.ImageIndex = BROWSE_TYPE_FULL;
            }

            if (this.m_reloadInfo == null)
            {
                if (bDisplayInfo == true)
                {
                    this.amazonSimpleQueryControl_simple.Comment = "命中记录:\t" + m_nTotalResults.ToString()
                    + "\r\n已装入浏览记录:\t" + this.listView_browse.Items.Count.ToString();
                }
            }
            else
            {
                if (table.Count != 0)
                {
                    strError = "下列事项没有被重新装载 '" + Join(table.Keys) + "'";
                    return -1;
                }
            }

            return 0;
        }

        // 设置浏览行的显示风格
        void SetLineStyle(ListViewItem item)
        {
#if NO
            // 为第一列设置等宽字体
            if (item.SubItems.Count > 0)
                item.SubItems[0].Font = new Font(new FontFamily("Courier New"), this.Font.Size);
#endif
        }

        static string Join(ICollection list)
        {
            StringBuilder text = new StringBuilder(4096);
            foreach (string s in list)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append(s);
            }

            return text.ToString();
        }

        [Serializable]  // 为了 Copy / Paste
        public class ItemInfo
        {
            public string Xml = ""; // XML 记录
            public string ElementSet = ""; // "B" "F"
            public string PreferSyntaxOID = ""; // 优先要转换成的 MARC 格式
        }

        // 解析 <Item> 内的基本信息
        // parameters:
        //      strASIN
        //      cols    浏览列信息
        static int ParseItemXml(XmlElement root,
            XmlNamespaceManager nsmgr,
            out string strASIN,
            out List<string> cols,
            out string strError)
        {
            strError = "";
            strASIN = "";
            cols = new List<string>();

            strASIN = DomUtil.GetElementText(root, "amazon:ASIN", nsmgr);

            // ISBN
            cols.Add(
                GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:ISBN | amazon:ItemAttributes/amazon:ISSN",
                " ; ")
                );

            // title
            cols.Add(
                GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:Title",
                " ; ")
                );

            // author
            cols.Add(
    GetFieldValues(root,
    nsmgr,
    "amazon:ItemAttributes/amazon:Creator",
    " ; ")
    );
            // publisher
            cols.Add(
GetFieldValues(root,
nsmgr,
"amazon:ItemAttributes/amazon:Manufacturer",   // Publisher 在 Small 的时候没有提供
" ; ")
);
            // publish date
            cols.Add(
GetFieldValues(root,
nsmgr,
"amazon:ItemAttributes/amazon:PublicationDate", // PublicationDate 在 Small 的时候没有提供
" ; ")
);
            // EAN
            cols.Add(
                GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:EAN",
                " ; ")
                );

            return 0;
        }

        // 获得一种字段的值。如果字段多次出现，用 strSep 符号连接
        static string GetFieldValues(XmlNode root,
            XmlNamespaceManager nsmgr,
            string strXPath,
            string strSep)
        {
            StringBuilder text = new StringBuilder(4096);
            XmlNodeList nodes = root.SelectNodes(strXPath, nsmgr);
            foreach (XmlNode node in nodes)
            {
                if (text.Length > 0)
                    text.Append(strSep);
                text.Append(node.InnerText);
            }

            return text.ToString();
        }

        private void AmazonSearchForm_Activated(object sender, EventArgs e)
        {
            RefreshNextBatchButtons();

            // 工具条按钮
            if (this.listView_browse.SelectedItems.Count == 0)
            {
                //m_mainForm.toolButton_saveTo.Enabled = false;
                m_mainForm.toolButton_loadFullRecord.Enabled = false;
            }
            else
            {
                //m_mainForm.toolButton_saveTo.Enabled = true;
                m_mainForm.toolButton_loadFullRecord.Enabled = true;
            }
        }

        void RefreshNextBatchButtons()
        {
            if (this.m_nTotalPages != 0 && this.m_nCurrentPageNo != -1
    && this.m_nCurrentPageNo < this.m_nTotalPages - 1)
                m_mainForm.toolButton_nextBatch.Enabled = true;
            else
                m_mainForm.toolButton_nextBatch.Enabled = false;

            if (this.m_nTotalPages != 0 && this.m_nCurrentPageNo != -1
                && this.m_nCurrentPageNo < this.m_nTotalPages - 1)
                m_mainForm.toolButton_getAllRecords.Enabled = true;
            else
                m_mainForm.toolButton_getAllRecords.Enabled = false;
        }

        // 自动根据情况，装载到MARC或者XML记录窗
        void LoadDetail(int index,
            string strFormat)
        {
            // XML格式
            if (strFormat == "xml")
            {
                XmlDetailForm form = new XmlDetailForm();

                form.DisplayOriginPage = false;
                form.MdiParent = this.m_mainForm;
                form.MainForm = this.m_mainForm;
                form.Show();

                form.LoadRecord(this, index);
                return;
            }

            ListViewItem curItem = this.listView_browse.Items[index];
            ItemInfo info = (ItemInfo)curItem.Tag;
            if (info != null)
            {
                if (strFormat == "unimarc")
                    info.PreferSyntaxOID = "1.2.840.10003.5.1"; // UNIMARC
                else if (strFormat == "usmarc")
                    info.PreferSyntaxOID = "1.2.840.10003.5.10"; // USMARC
            }

            {
                MarcDetailForm exist_fixed = this.MainForm.FixedMarcDetailForm;

                MarcDetailForm form = new MarcDetailForm();

                form.MdiParent = this.m_mainForm;
                form.MainForm = this.m_mainForm;

#if NO
                // 继承自动识别的OID
                if (connection.TargetInfo != null
                    && connection.TargetInfo.DetectMarcSyntax == true)
                {
                    form.AutoDetectedMarcSyntaxOID = record.AutoDetectedSyntaxOID;
                }
#endif
                // 在已经有左侧窗口的情况下，普通窗口需要显示在右侧
                if (exist_fixed != null)
                {
                    if (exist_fixed != null)
                        exist_fixed.Activate();

                    form.SuppressSizeSetting = true;
                    this.MainForm.SetMdiToNormal();
                }
                form.Show();
                // 在已经有左侧窗口的情况下，普通窗口需要显示在右侧
                if (exist_fixed != null)
                {
                    this.MainForm.SetFixedPosition(form, "right");
                }

                form.LoadRecord(this, index);
            }
        }

        void menuItem_loadUniMarcDetail_Click(object sender, EventArgs e)
        {
            int nIndex = -1;

            if (this.listView_browse.SelectedIndices.Count > 0)
                nIndex = this.listView_browse.SelectedIndices[0];
            else
            {
                if (this.listView_browse.FocusedItem == null)
                    return;
                nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
            }

            LoadDetail(nIndex, "unimarc");

            this.ActiveDetailFormType = "unimarc";
        }

        void menuItem_loadUsMarcDetail_Click(object sender, EventArgs e)
        {
            int nIndex = -1;

            if (this.listView_browse.SelectedIndices.Count > 0)
                nIndex = this.listView_browse.SelectedIndices[0];
            else
            {
                if (this.listView_browse.FocusedItem == null)
                    return;
                nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
            }

            LoadDetail(nIndex, "usmarc");

            this.ActiveDetailFormType = "usmarc";
        }

        void menuItem_loadXmlDetail_Click(object sender, EventArgs e)
        {
            int nIndex = -1;

            if (this.listView_browse.SelectedIndices.Count > 0)
                nIndex = this.listView_browse.SelectedIndices[0];
            else
            {
                if (this.listView_browse.FocusedItem == null)
                    return;
                nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
            }

            LoadDetail(nIndex, "xml");
            this.ActiveDetailFormType = "xml";
        }

        private void listView_browse_DoubleClick(object sender, EventArgs e)
        {
            int nIndex = -1;

            if (this.listView_browse.SelectedIndices.Count > 0)
                nIndex = this.listView_browse.SelectedIndices[0];
            else
            {
                if (this.listView_browse.FocusedItem == null)
                    return;
                nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
            }

            LoadDetail(nIndex, this.ActiveDetailFormType);
        }

        void ClearListViewItems()
        {
            this.listView_browse.Items.Clear();

            ListViewUtil.ClearSortColumns(this.listView_browse);

#if NO
            // 清除所有需要确定的栏标题
            for (int i = 1; i < this.listView_browse.Columns.Count; i++)
            {
                this.listView_browse.Columns[i].Text = i.ToString();
            }
#endif
        }

        void ClearResultSetParameters()
        {
            this.m_nCurrentPageNo = -1; // 表示这是第一次检索
            this.m_nTotalResults = 0;
            this.m_nTotalPages = 0;
            this.m_searchParameters = null;
            this.m_strCurrentSearchedServer = "";
        }

        public bool AlwaysUseFullElementSet
        {
            get
            {
                // 亚马逊检索窗
                return this.MainForm.AppInfo.GetBoolean(
"amazon_search_form",
"always_use_full_elementset",
true);
            }
        }
        // 是否创建 856 字段
        public bool Create856
        {
            get
            {
                // 亚马逊检索窗
                return this.MainForm.AppInfo.GetBoolean(
"amazon_search_form",
"create_856",
true);
            }
        }

        // 为选定的行装入Full元素集的记录
        // 使用 ItemLookup API
        // TODO: 显示总体进度
        public void ReloadFullElementSet()
        {
            string strError = "";
            int nRet = 0;

            this.amazonSimpleQueryControl_simple.Comment = "";

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_browse.SelectedItems)
            {
                items.Add(item);
            }

            this.m_bErrorBox = true;
            nRet = ReloadItems(items,
                0,
                "F",
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
        }

        int m_nInSelectedIndexChanged = 0;

        private void listView_browse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_nInSelectedIndexChanged > 0)
                return;

            m_nInSelectedIndexChanged++;
            if (this.listView_browse.SelectedIndices.Count == 0)
                this.MainForm.StatusBarMessage = "";
            else
            {
                if (this.listView_browse.SelectedIndices.Count == 1)
                {
                    this.MainForm.StatusBarMessage = "第 " + (this.listView_browse.SelectedIndices[0] + 1).ToString() + " 行";
                }
                else
                {
                    this.MainForm.StatusBarMessage = "从 " + (this.listView_browse.SelectedIndices[0] + 1).ToString() + " 行开始，共选中 " + this.listView_browse.SelectedIndices.Count.ToString() + " 个事项";
                }
            }

            // 菜单动态变化
            if (this.listView_browse.SelectedItems.Count == 0)
            {
                m_mainForm.toolButton_loadFullRecord.Enabled = false;
            }
            else
            {
                m_mainForm.toolButton_loadFullRecord.Enabled = true;
            }

            m_nInSelectedIndexChanged--;
        }

        public string ActiveDetailFormType
        {
            get
            {
                return this.MainForm.AppInfo.GetString(
"amazonsearchform",
"active_detailform_type",
"unimarc");
            }
            set
            {
                this.MainForm.AppInfo.SetString(
"amazonsearchform",
"active_detailform_type",
value);
            }
        }

        public string ActiveDetailFormTypeCaption
        {
            get
            {
                string strValue = this.ActiveDetailFormType;
                if (strValue == "unimarc")
                    return "UNIMARC";
                else if (strValue == "usmarc")
                    return "MARC21";
                else if (strValue == "xml")
                    return "XML";
                return strValue;
            }
        }

        private void listView_browse_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            ToolStripMenuItem subMenuItem = null;

            int nSelectedCount = 0;
            nSelectedCount = this.listView_browse.SelectedItems.Count;


            menuItem = new ToolStripMenuItem("装入 " + this.ActiveDetailFormTypeCaption + " 记录窗(&M)");
            menuItem.Font = new Font(menuItem.Font, FontStyle.Bold);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            if (this.ActiveDetailFormType == "unimarc")
                menuItem.Click += new EventHandler(menuItem_loadUniMarcDetail_Click);
            else if (this.ActiveDetailFormType == "usmarc")
                menuItem.Click += new EventHandler(menuItem_loadUsMarcDetail_Click);
            else if (this.ActiveDetailFormType == "xml")
                menuItem.Click += new EventHandler(menuItem_loadXmlDetail_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            ToolStripSeparator sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("装入 UNIMARC 记录窗(&M)");
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadUniMarcDetail_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("装入 MARC21 记录窗(&S)");
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadUsMarcDetail_Click);
            contextMenu.Items.Add(menuItem);


            menuItem = new ToolStripMenuItem("装入 XML 记录窗(&X)");
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadXmlDetail_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cutToClipboard_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);


            menuItem = new ToolStripMenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyToClipboard_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("复制单列(&S)");
            if (this.listView_browse.SelectedIndices.Count == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            for (int i = 0; i < this.listView_browse.Columns.Count; i++)
            {
                subMenuItem = new ToolStripMenuItem("复制列 '" + this.listView_browse.Columns[i].Text + "'");
                subMenuItem.Tag = i;
                subMenuItem.Click += new System.EventHandler(this.menu_copySingleColumnToClipboard_Click);
                menuItem.DropDownItems.Add(subMenuItem);
            }

            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData != null
                && (iData.GetDataPresent(typeof(string)) == true
                || iData.GetDataPresent(typeof(MemLineCollection)) == true))
                bHasClipboardObject = true;
            else
                bHasClipboardObject = false;

            menuItem = new ToolStripMenuItem("粘贴[前插](&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteFromClipboard_insertBefore_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("粘贴[后插](&V)");
            menuItem.Click += new System.EventHandler(this.menu_pasteFromClipboard_insertAfter_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);



            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("全选(&A)");
            menuItem.Click += new EventHandler(MenuItem_selectAll_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("当前服务器(&C)");
            contextMenu.Items.Add(menuItem);

            try
            {
                List<string> servers = SystemCfgForm.ListAmazonServers(this.MainForm.DataDir,
                    this.Lang);
                string strOldCfg = this.CurrentServer;

                // 子菜单
                foreach (string s in servers)
                {
                    string strRight = AmazonQueryControl.GetRight(s);  // 右边的纯粹 host 部分
                    subMenuItem = new ToolStripMenuItem();
                    subMenuItem.Text = s.Replace("\t", " - ");
                    subMenuItem.Tag = s;

                    if (strRight == strOldCfg)
                    {
                        subMenuItem.BackColor = SystemColors.Info;
                        subMenuItem.ForeColor = SystemColors.InfoText;
                        subMenuItem.Checked = true;
                    }
                    subMenuItem.Click += new EventHandler(MenuItem_setCurrentServer_Click);
                    menuItem.DropDown.Items.Add(subMenuItem);
                }

            }
            catch (Exception ex)
            {
                subMenuItem = new ToolStripMenuItem();
                subMenuItem.Text = ex.Message;
                menuItem.DropDown.Items.Add(subMenuItem);
            }

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 装载详细记录
            menuItem = new ToolStripMenuItem("装载详细记录 [" + nSelectedCount.ToString() + "] (&F)...");
            menuItem.Click += new System.EventHandler(this.menu_reloadFullElementSet_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 追加保存到数据库
            menuItem = new ToolStripMenuItem("以追加方式保存到数据库 [" + nSelectedCount.ToString() + "] (&A)...");
            menuItem.Click += new System.EventHandler(this.menu_saveToDatabase_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);


            // 保存原始记录到ISO2709文件
            menuItem = new ToolStripMenuItem("保存到 MARC 文件 [" + nSelectedCount.ToString()
                + "] (&S)");
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_saveOriginRecordToIso2709_Click);
            contextMenu.Items.Add(menuItem);

            // 导出到 Excel 文件
            menuItem = new ToolStripMenuItem("导出到 Excel 文件 [" + nSelectedCount.ToString()
                + "] (&E)");
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_exportBrowseToExcel_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Show(this.listView_browse, e.Location);
        }

        void menuItem_exportBrowseToExcel_Click(object sender, EventArgs e)
        {
            Global.ExportLinesToExcel(this,
                this.listView_browse);
        }

        void menu_copyToClipboard_Click(object sender, EventArgs e)
        {
            Global.CopyLinesToClipboard(this,
                "AmazonSearchForm",
                this.listView_browse,
                false);
        }

        void menu_cutToClipboard_Click(object sender, EventArgs e)
        {
            Global.CopyLinesToClipboard(this,
                "AmazonSearchForm",
                this.listView_browse,
                true);
        }

        void menu_pasteFromClipboard_insertBefore_Click(object sender, EventArgs e)
        {
            Global.PasteLinesFromClipboard(this,
                "AmazonSearchForm",
                this.listView_browse,
                true);
        }

        void menu_pasteFromClipboard_insertAfter_Click(object sender, EventArgs e)
        {
            Global.PasteLinesFromClipboard(this,
                "AmazonSearchForm",
                this.listView_browse,
                false);
        }

        void menu_copySingleColumnToClipboard_Click(object sender, EventArgs e)
        {
            int nColumn = (int)((ToolStripMenuItem)sender).Tag;

            Global.CopyLinesToClipboard(this, nColumn, this.listView_browse, false);
        }

        void menu_reloadFullElementSet_Click(object sender, EventArgs e)
        {
            ReloadFullElementSet();
        }

        #region 保存到数据库

        DtlpSearchForm GetDtlpSearchForm()
        {
            DtlpSearchForm dtlp_searchform = null;

            dtlp_searchform = this.m_mainForm.TopDtlpSearchForm;

            if (dtlp_searchform == null)
            {
                // 新开一个dtlp检索窗
                FormWindowState old_state = this.WindowState;

                dtlp_searchform = new DtlpSearchForm();
                dtlp_searchform.MainForm = this.m_mainForm;
                dtlp_searchform.MdiParent = this.m_mainForm;
                dtlp_searchform.WindowState = FormWindowState.Minimized;
                dtlp_searchform.Show();

                this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                dtlp_searchform.WaitLoadFinish();
            }

            return dtlp_searchform;
        }

        dp2SearchForm GetDp2SearchForm()
        {
            dp2SearchForm dp2_searchform = null;

            dp2_searchform = this.m_mainForm.TopDp2SearchForm;

            if (dp2_searchform == null)
            {
                // 新开一个dp2检索窗
                FormWindowState old_state = this.WindowState;

                dp2_searchform = new dp2SearchForm();
                dp2_searchform.MainForm = this.m_mainForm;
                dp2_searchform.MdiParent = this.m_mainForm;
                dp2_searchform.WindowState = FormWindowState.Minimized;
                dp2_searchform.Show();

                this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                dp2_searchform.WaitLoadFinish();
            }

            return dp2_searchform;
        }


        void dlg_GetDp2SearchParam(object sender, GetDp2SearchParamEventArgs e)
        {
            dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

#if OLD_CHANNEL
            e.dp2Channels = dp2_searchform.Channels;
#endif
            e.MainForm = this.m_mainForm;
        }

        void dlg_GetDtlpSearchParam(object sender, GetDtlpSearchParamEventArgs e)
        {
            DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

            e.DtlpChannels = dtlp_searchform.DtlpChannels;
            e.DtlpChannel = dtlp_searchform.DtlpChannel;
        }

        // 选定的行中是否包含了Brief格式的记录
        bool HasSelectionContainBriefRecords()
        {

            foreach (ListViewItem item in this.listView_browse.SelectedItems)
            {
                ItemInfo info = (ItemInfo)item.Tag;
                if (info == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (info.ElementSet == "B")
                    return true;
            }

            return false;
        }

        // 追加保存到数据库
        void menu_saveToDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_browse.SelectedIndices.Count == 0)
            {
                strError = "尚未选定要保存记录的浏览行";
                goto ERROR1;
            }

            string strLastSavePath = m_mainForm.LastSavePath;
            if (String.IsNullOrEmpty(strLastSavePath) == false)
            {
                string strOutputPath = "";
                nRet = MarcDetailForm.ChangePathToAppendStyle(strLastSavePath,
                    out strOutputPath,
                    out strError);
                if (nRet == -1)
                {
                    m_mainForm.LastSavePath = ""; // 避免下次继续出错
                    goto ERROR1;
                }
                strLastSavePath = strOutputPath;
            }


            SaveRecordDlg dlg = new SaveRecordDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.SaveToDbMode = true;    // 不允许在textbox中修改路径

            dlg.MainForm = this.m_mainForm;
            dlg.GetDtlpSearchParam += new GetDtlpSearchParamEventHandle(dlg_GetDtlpSearchParam);
            dlg.GetDp2SearchParam += new GetDp2SearchParamEventHandle(dlg_GetDp2SearchParam);
            {
                dlg.RecPath = strLastSavePath;
                dlg.Text = "请选择目标数据库";
            }
            // dlg.StartPosition = FormStartPosition.CenterScreen;
            this.MainForm.AppInfo.LinkFormState(dlg, "SaveRecordDlg_state");
            dlg.UiState = this.MainForm.AppInfo.GetString("AmazonSearchForm", "SaveRecordDlg_uiState", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString("AmazonSearchForm", "SaveRecordDlg_uiState", dlg.UiState);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            m_mainForm.LastSavePath = dlg.RecPath;

            string strProtocol = "";
            string strPath = "";
            nRet = Global.ParsePath(dlg.RecPath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            bool bForceFull = false;

            if (HasSelectionContainBriefRecords() == true)
            {
                DialogResult result = MessageBox.Show(this,
"即将保存的记录中有Brief(简要)格式的记录，是否在保存前重新获取为Full(完整)格式的记录?\r\n\r\n(Yes: 是，要完整格式的记录; No: 否，依然保存简明格式的记录； Cancel: 取消，放弃整个保存操作",
"AmazonSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                    bForceFull = true;
            }

            // 首先获得详细记录
            if (bForceFull == true)
            {
                ReloadFullElementSet();
                bool bError = WaitSearchFinish();
            }

            // TODO: 禁止问号以外的其它ID
            DigitalPlatform.Stop stop = null;
            stop = new DigitalPlatform.Stop();
            stop.Register(m_mainForm.stopManager, true);	// 和容器关联

            stop.BeginLoop();

            this.EnableControls(false);
            try
            {

                // dtlp协议的记录保存
                if (strProtocol.ToLower() == "dtlp")
                {
                    DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

                    if (dtlp_searchform == null)
                    {
                        strError = "没有连接的或者打开的DTLP检索窗，无法保存记录";
                        goto ERROR1;
                    }

                    for (int i = 0; i < this.listView_browse.SelectedIndices.Count; i++)
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

                        int index = this.listView_browse.SelectedIndices[i];

                        byte[] baTimestamp = null;
                        string strSavePath = "";
                        string strOutStyle = "";
                        LoginInfo logininfo = null;
                        long lVersion = 0;
                        string strXmlFragment = "";
                        DigitalPlatform.OldZ3950.Record record = null;
                        Encoding currentEncoding = null;
                        string strMARC = "";

                        nRet = this.GetOneRecord(
                            "marc",
                            index,  // 即将废止
                            "index:" + index.ToString(),
                            bForceFull == true ? "force_full" : "", // false,
                            out strSavePath,
                            out strMARC,
                            out strXmlFragment,
                            out strOutStyle,
                            out baTimestamp,
                            out lVersion,
                            out record,
                            out currentEncoding,
                            out logininfo,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        string strMarcSyntax = "";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                            strMarcSyntax = "unimarc";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                            strMarcSyntax = "usmarc";

                        // TODO: 有些格式不适合保存到目标数据库

                        byte[] baOutputTimestamp = null;
                        string strOutputPath = "";
                        nRet = dtlp_searchform.SaveMarcRecord(
                            strPath,
                            strMARC,
                            baTimestamp,
                            out strOutputPath,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    MessageBox.Show(this, "保存成功");
                    return;
                }
                else if (strProtocol.ToLower() == "dp2library")
                {
                    dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                    if (dp2_searchform == null)
                    {
                        strError = "没有连接的或者打开的dp2检索窗，无法保存记录";
                        goto ERROR1;
                    }

                    string strDp2ServerName = "";
                    string strPurePath = "";
                    // 解析记录路径。
                    // 记录路径为如下形态 "中文图书/1 @服务器"
                    dp2SearchForm.ParseRecPath(strPath,
                        out strDp2ServerName,
                        out strPurePath);

                    string strTargetMarcSyntax = "";

                    try
                    {
                        NormalDbProperty prop = dp2_searchform.GetDbProperty(strDp2ServerName,
             dp2SearchForm.GetDbName(strPurePath));
                        strTargetMarcSyntax = prop.Syntax;
                        if (string.IsNullOrEmpty(strTargetMarcSyntax) == true)
                            strTargetMarcSyntax = "unimarc";
                    }
                    catch (Exception ex)
                    {
                        strError = "在获得目标库特性时出错: " + ex.Message;
                        goto ERROR1;
                    }

                    bool bSkip = false;
                    int nSavedCount = 0;

                    for (int i = 0; i < this.listView_browse.SelectedIndices.Count; i++)
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

                        int index = this.listView_browse.SelectedIndices[i];

                        byte[] baTimestamp = null;
                        string strSavePath = "";
                        string strOutStyle = "";
                        LoginInfo logininfo = null;
                        long lVersion = 0;
                        string strXmlFragment = "";
                        DigitalPlatform.OldZ3950.Record record = null;
                        Encoding currentEncoding = null;
                        string strMARC = "";

                        nRet = this.GetOneRecord(
                            "marc",
                            index,  // 即将废止
                            "index:" + index.ToString(),
                            bForceFull == true ? "force_full" : "", // false,
                            out strSavePath,
                            out strMARC,
                            out strXmlFragment,
                            out strOutStyle,
                            out baTimestamp,
                            out lVersion,
                            out record,
                            out currentEncoding,
                            out logininfo,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;


                        string strMarcSyntax = "";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                            strMarcSyntax = "unimarc";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                            strMarcSyntax = "usmarc";

                        // 有些格式不适合保存到目标数据库
                        if (strTargetMarcSyntax != strMarcSyntax)
                        {
                            if (bSkip == true)
                                continue;
                            strError = "记录 " + (index + 1).ToString() + " 的格式类型为 '" + strMarcSyntax + "'，和目标库的格式类型 '" + strTargetMarcSyntax + "' 不符合，因此无法保存到目标库";
                            DialogResult result = MessageBox.Show(this,
        strError + "\r\n\r\n要跳过这些记录而继续保存后面的记录么?\r\n\r\n(Yes: 跳过格式不吻合的记录，继续保存后面的; No: 放弃整个保存操作)",
        "AmazonSearchForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.No)
                                goto ERROR1;
                            bSkip = true;
                            continue;
                        }

                        string strProtocolPath = this.CurrentProtocol + ":"
    + this.CurrentResultsetPath
    + "/" + (index + 1).ToString();

                        string strOutputPath = "";
                        byte[] baOutputTimestamp = null;
                        string strComment = "copy from " + strProtocolPath; // strSavePath;
                        // return:
                        //      -2  timestamp mismatch
                        //      -1  error
                        //      0   succeed
                        nRet = dp2_searchform.SaveMarcRecord(
                            false,
                            strPath,
                            strMARC,
                            strMarcSyntax,
                            baTimestamp,
                            strXmlFragment,
                            strComment,
                            out strOutputPath,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        nSavedCount++;

                    }
                    MessageBox.Show(this, "共保存记录 " + nSavedCount.ToString() + " 条");
                    return;
                }
                else if (strProtocol.ToLower() == "z3950")
                {
                    strError = "目前暂不支持Z39.50协议的保存操作";
                    goto ERROR1;
                }
                else
                {
                    strError = "无法识别的协议名 '" + strProtocol + "'";
                    goto ERROR1;
                }
            }
            finally
            {
                stop.EndLoop();

                stop.Unregister();	// 和容器关联
                stop = null;

                this.EnableControls(true);
            }

            // return 0;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #endregion

        void dlg_GetEncoding(object sender, GetEncodingEventArgs e)
        {
            string strError = "";
            Encoding encoding = null;
            int nRet = this.m_mainForm.GetEncoding(e.EncodingName,
                out encoding,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }
            e.Encoding = encoding;
        }

        public void menuItem_saveOriginRecordToIso2709_Click(object sender,
    EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_browse.SelectedIndices.Count == 0)
            {
                strError = "尚未选定要保存记录的浏览行";
                goto ERROR1;
            }

            bool bForceFull = false;

            if (HasSelectionContainBriefRecords() == true)
            {
                DialogResult result = MessageBox.Show(this,
"即将保存的记录中有Brief(简要)格式的记录，是否在保存前重新获取为Full(完整)格式的记录?\r\n\r\n(Yes: 是，要完整格式的记录; No: 否，依然保存简明格式的记录； Cancel: 取消，放弃整个保存操作",
"AmazonSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                    bForceFull = true;
            }

            Encoding preferredEncoding = Encoding.UTF8;

#if NO
            {
                // 观察要保存的第一条记录的marc syntax
                int first_index = this.listView_browse.SelectedIndices[0];
                ListViewItem first_item = this.listView_browse.Items[first_index];

                preferredEncoding = connection.GetRecordsEncoding(
                    this.m_mainForm,
                    first_record.m_strSyntaxOID);

            }
#endif

            OpenMarcFileDlg dlg = new OpenMarcFileDlg();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.IsOutput = true;
            dlg.GetEncoding -= new GetEncodingEventHandler(dlg_GetEncoding);
            dlg.GetEncoding += new GetEncodingEventHandler(dlg_GetEncoding);
            dlg.FileName = m_mainForm.LastIso2709FileName;
            // dlg.CrLf = m_mainForm.LastCrLfIso2709;
            dlg.CrLfVisible = false;   // 2020/3/9
            dlg.RemoveField998Visible = false;
            //dlg.RemoveField998 = m_mainForm.LastRemoveField998;
            dlg.EncodingListItems = Global.GetEncodingList(true);
            dlg.EncodingName =
                (String.IsNullOrEmpty(m_mainForm.LastEncodingName) == true ? GetEncodingForm.GetEncodingName(preferredEncoding) : m_mainForm.LastEncodingName);
            dlg.EncodingComment = "注: 原始编码方式为 " + GetEncodingForm.GetEncodingName(preferredEncoding);
            dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
            dlg.EnableMarcSyntax = false;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            bool unimarc_modify_100 = dlg.UnimarcModify100;

            Encoding targetEncoding = null;

            if (dlg.EncodingName == "MARC-8")
            {
                strError = "暂不能使用这个编码方式保存记录。";
                goto ERROR1;
            }

            nRet = this.m_mainForm.GetEncoding(dlg.EncodingName,
                out targetEncoding,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            string strLastFileName = m_mainForm.LastIso2709FileName;
            string strLastEncodingName = m_mainForm.LastEncodingName;

            bool bExist = File.Exists(dlg.FileName);
            bool bAppend = false;

            if (bExist == true)
            {
                DialogResult result = MessageBox.Show(this,
        "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
        "AmazonSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    bAppend = true;

                if (result == DialogResult.No)
                    bAppend = false;

                if (result == DialogResult.Cancel)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }
            }

            // 检查同一个文件连续存时候的编码方式一致性
            if (strLastFileName == dlg.FileName
                && bAppend == true)
            {
                if (strLastEncodingName != ""
                    && strLastEncodingName != dlg.EncodingName)
                {
                    DialogResult result = MessageBox.Show(this,
                        "文件 '" + dlg.FileName + "' 已在先前已经用 " + strLastEncodingName + " 编码方式存储了记录，现在又以不同的编码方式 " + dlg.EncodingName + " 追加记录，这样会造成同一文件中存在不同编码方式的记录，可能会令它无法被正确读取。\r\n\r\n是否继续? (是)追加  (否)放弃操作",
                        "AmazonSearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                    {
                        strError = "放弃处理...";
                        goto ERROR1;
                    }

                }
            }

            m_mainForm.LastIso2709FileName = dlg.FileName;
            m_mainForm.LastCrLfIso2709 = dlg.CrLf;
            m_mainForm.LastEncodingName = dlg.EncodingName;
            // m_mainForm.LastRemoveField998 = dlg.RemoveField998;

            Stream s = null;

            try
            {
                s = File.Open(m_mainForm.LastIso2709FileName,
                     FileMode.OpenOrCreate);
                if (bAppend == false)
                    s.SetLength(0);
                else
                    s.Seek(0, SeekOrigin.End);
            }
            catch (Exception ex)
            {
                strError = "打开或创建文件 " + m_mainForm.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            try
            {

                // 首先获得详细记录
                if (bForceFull == true)
                {
                    ReloadFullElementSet();
                    bool bError = WaitSearchFinish();
                }

                for (int i = 0; i < this.listView_browse.SelectedIndices.Count; i++)
                {
                    int index = this.listView_browse.SelectedIndices[i];

                    byte[] baTimestamp = null;
                    string strSavePath = "";
                    string strOutStyle = "";
                    LoginInfo logininfo = null;
                    long lVersion = 0;
                    string strXmlFragment = "";
                    DigitalPlatform.OldZ3950.Record record = null;
                    Encoding currentEncoding = null;
                    string strMARC = "";

                    nRet = this.GetOneRecord(
                        "marc",
                        index,  // 即将废止
                        "index:" + index.ToString(),
                        bForceFull == true ? "force_full" : "", // false,
                        out strSavePath,
                        out strMARC,
                        out strXmlFragment,
                        out strOutStyle,
                        out baTimestamp,
                        out lVersion,
                        out record,
                        out currentEncoding,
                        out logininfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    byte[] baTarget = null;

                    string strMarcSyntax = "";
                    if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                        strMarcSyntax = "unimarc";
                    if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                        strMarcSyntax = "usmarc";
#if NO
                    if (dlg.RemoveField998 == true)
                    {
                        MarcRecord temp = new MarcRecord(strMARC);
                        temp.select("field[@name='998']").detach();
                        strMARC = temp.Text;
                    }
#endif
                    if (dlg.Mode880 == true && strMarcSyntax == "usmarc")
                    {
                        MarcRecord temp = new MarcRecord(strMARC);
                        MarcQuery.To880(temp);
                        strMARC = temp.Text;
                    }

                    // 将MARC机内格式转换为ISO2709格式
                    // parameters:
                    //		strMarcSyntax   "unimarc" "usmarc"
                    //		strSourceMARC		[in]机内格式MARC记录。
                    //		targetEncoding	[in]输出ISO2709的编码方式为 UTF8 codepage-936等等
                    //		baResult	[out]输出的ISO2709记录。字符集受nCharset参数控制。
                    //					注意，缓冲区末尾不包含0字符。
                    nRet = MarcUtil.CvtJineiToISO2709(
                        strMARC,
                        strMarcSyntax,
                        targetEncoding,
                        unimarc_modify_100 ? "unimarc_100" : "",
                        out baTarget,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    s.Write(baTarget, 0,
                        baTarget.Length);

                    if (dlg.CrLf == true)
                    {
                        byte[] baCrLf = targetEncoding.GetBytes("\r\n");
                        s.Write(baCrLf, 0,
                            baCrLf.Length);
                    }
                }

                // 
                if (bAppend == true)
                    m_mainForm.MessageText = this.listView_browse.SelectedIndices.Count.ToString()
                        + "条记录成功追加到文件 " + m_mainForm.LastIso2709FileName + " 尾部";
                else
                    m_mainForm.MessageText = this.listView_browse.SelectedIndices.Count.ToString()
                        + "条记录成功保存到新文件 " + m_mainForm.LastIso2709FileName + " 尾部";

            }
            catch (Exception ex)
            {
                strError = "写入文件 " + m_mainForm.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                s.Close();
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void MenuItem_selectAll_Click(object sender, EventArgs e)
        {
            m_nInSelectedIndexChanged++;    // 避免浪费速度
            ListViewUtil.SelectAllLines(this.listView_browse);
            m_nInSelectedIndexChanged--;
            listView_browse_SelectedIndexChanged(this, new EventArgs());
        }

        void MenuItem_setCurrentServer_Click(object sender, EventArgs e)
        {
            var subMenuItem = sender as ToolStripMenuItem;
            string strCfgLine = (string)subMenuItem.Tag;
            this.CurrentServerCfgString = strCfgLine;
        }

        // 检索按钮 上的 上下文菜单
        private void button_searchSimple_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            ToolStripMenuItem subMenuItem = null;

            menuItem = new ToolStripMenuItem("当前服务器(&C)");
            contextMenu.Items.Add(menuItem);

            try
            {
                List<string> servers = SystemCfgForm.ListAmazonServers(this.MainForm.DataDir,
                    this.Lang);
                string strOldCfg = this.CurrentServer;

                // 子菜单
                foreach (string s in servers)
                {
                    string strRight = AmazonQueryControl.GetRight(s);  // 右边的纯粹 host 部分
                    subMenuItem = new ToolStripMenuItem();
                    subMenuItem.Text = s.Replace("\t", " - ");
                    subMenuItem.Tag = s;

                    if (strRight == strOldCfg)
                    {
                        subMenuItem.BackColor = SystemColors.Info;
                        subMenuItem.ForeColor = SystemColors.InfoText;
                        subMenuItem.Checked = true;
                    }
                    subMenuItem.Click += new EventHandler(MenuItem_setCurrentServer_Click);
                    menuItem.DropDown.Items.Add(subMenuItem);
                }

            }
            catch (Exception ex)
            {
                subMenuItem = new ToolStripMenuItem();
                subMenuItem.Text = ex.Message;
                menuItem.DropDown.Items.Add(subMenuItem);
            }

            contextMenu.Show(this.button_searchSimple, e.Location);
        }

        // 捕获键盘的输入
        protected override bool ProcessDialogKey(
            Keys keyData)
        {
            // 回车
            if (keyData == Keys.Enter || keyData == Keys.LineFeed)
            {
                if (this.amazonSimpleQueryControl_simple.Focused == true)
                {
                    // 检索词那里回车
                    this.DoSearch();
                }
                else if (this.listView_browse.Focused == true)
                {
                    // 浏览框中回车
                    listView_browse_DoubleClick(this, null);
                }

                return true;
            }

            return base.ProcessDialogKey(keyData);
            // return false;
        }

        // TODO: 加入一旦窗口关闭就跳出循环的逻辑
        // return:
        //      true    已经报错
        //      false   正常
        bool WaitSearchFinish()
        {
            while (true)
            {
                Application.DoEvents();
                if (eventComplete.WaitOne(100) == true)
                    break;
            }

            return this.m_bError;
        }

        private void listView_browse_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_browse.Columns,
                true);

            // 排序
            this.listView_browse.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);
            this.listView_browse.ListViewItemSorter = null;
        }
    }
}
