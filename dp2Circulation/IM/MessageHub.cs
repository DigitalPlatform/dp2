using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

// using Microsoft.AspNet.SignalR.Client.Hubs;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;
using DigitalPlatform.MessageClient;

namespace dp2Circulation
{
    /// <summary>
    /// 负责消息交换的类
    /// 能响应外部发来的检索请求
    /// </summary>
    public class MessageHub : MessageConnection
    {
        // 检索响应的事件
        public event SearchResponseEventHandler SearchResponseEvent = null;

        public MainForm MainForm = null;

        public WebBrowser webBrowser1 = null;

        internal LibraryChannelPool _channelPool = new LibraryChannelPool();

        public void Initial(MainForm main_form,
            WebBrowser webBrowser)
        {
            this.MainForm = main_form;
            this.webBrowser1 = webBrowser;

            ClearHtml();

            this._channelPool.BeforeLogin += new BeforeLoginEventHandle(_channelPool_BeforeLogin);

#if NO
            if (string.IsNullOrEmpty(this.dp2MServerUrl) == false)
            {
                this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
            }

            _timer.Interval = 1000 * 30;
            _timer.Elapsed += _timer_Elapsed;
#endif
            base.Initial();
        }

        public override void Destroy()
        {
            base.Destroy();

            this._channelPool.BeforeLogin -= new BeforeLoginEventHandle(_channelPool_BeforeLogin);
            this._channelPool.Close();
        }

        void _channelPool_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                e.LibraryServerUrl = this.MainForm.LibraryServerUrl;
                bool bIsReader = false;

                e.UserName = this.MainForm.AppInfo.GetString(
                "default_account",
                "username",
                "");

                e.Password = this.MainForm.AppInfo.GetString(
"default_account",
"password",
"");
                e.Password = this.MainForm.DecryptPasssword(e.Password);

                bIsReader =
this.MainForm.AppInfo.GetBoolean(
"default_account",
"isreader",
false);
                Debug.Assert(this.MainForm != null, "");

                string strLocation = this.MainForm.AppInfo.GetString(
                "default_account",
                "location",
                "");
                e.Parameters = "location=" + strLocation;
                if (bIsReader == true)
                    e.Parameters += ",type=reader";

                // 2014/9/13
                e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

#if SN
                // 从序列号中获得 expire= 参数值
                string strExpire = this.MainForm.GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false)
                    e.Parameters += ",expire=" + strExpire;
#endif

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // TODO: 可以出现对话框，但要注意跨线程的问题
            // TODO: 当首次登录对话框没有输入密码的时候，这里就必须出现对话框询问密码了
            e.Cancel = true;
        }

        public LibraryChannel GetChannel()
        {
            string strServerUrl = this.MainForm.LibraryServerUrl;
            string strUserName = this.MainForm.DefaultUserName;

            return this._channelPool.GetChannel(strServerUrl, strUserName);
        }

        public override string dp2MServerUrl
        {
            get
            {
                // dp2MServer URL
                return this.MainForm.AppInfo.GetString("config",
                    "im_server_url",
                    "http://dp2003.com:8083/dp2MServer");
            }
            set
            {
                this.MainForm.AppInfo.SetString("config",
                    "im_server_url",
                    value);
            }
        }

        #region IE 控件显示

        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(this.MainForm.DataDir, "history.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";

            {
                HtmlDocument doc = webBrowser1.Document;

                if (doc == null)
                {
                    webBrowser1.Navigate("about:blank");
                    doc = webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            Global.WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }

        delegate void Delegate_AppendHtml(string strText);
        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                this.webBrowser1.BeginInvoke(new Action<string>(AppendHtml), strText);
                return;
            }

            Global.WriteHtml(this.webBrowser1,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
                this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        // parameters:
        //      nWarningLevel   0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public override void OutputText(string strText, int nWarningLevel = 0)
        {
            string strClass = "normal";
            if (nWarningLevel == 1)
                strClass = "warning";
            else if (nWarningLevel >= 2)
                strClass = "error";
            this.AppendHtml("<div class='debug " + strClass + "'>" + HttpUtility.HtmlEncode(DateTime.Now.ToShortTimeString() + " " + strText).Replace("\r\n", "<br/>") + "</div>");
        }

#if NO
        void AddInfoLine(string strContent)
        {
#if NO
            string strText = "<div class='item'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
#endif
            OutputText(strContent, 0);
        }
#endif

        public override void OnAddMessageRecieved(string strName, string strContent)
        {
            if (strName == null)
                strName = "";
            if (strContent == null)
                strContent = "";

            string strText = "<div class='item'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strName).Replace("\r\n", "<br/>") + "</div>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
        }

#if NO
        void AddErrorLine(string strContent)
        {
#if NO
            string strText = "<div class='item error'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
#endif
            OutputText(strContent, 2);
        }
#endif

        #endregion


        // bool tryingToReconnect = false;

        // 响应 server 发来的消息 SearchBiblio
        public override void OnSearchBiblioRecieved(
            string searchID,
            string dbNameList,
             string queryWord,
             string fromList,
             string matchStyle,
             string formatList,
             long maxResults)
        {
            // 单独给一个线程来执行
#if NO
            try
            {
                await Task.Factory.StartNew(() => SearchAndResponse(
                    searchID,
                    dbNameList,
                    queryWord,
                    fromList,
                    matchStyle,
                    formatList,
                    maxResults));
            }
            catch(Exception ex)
            {
                AddErrorLine(ex.Message);
            }
#endif

#if NO
            Task task = Task.Factory.StartNew(() => SearchAndResponse(
                    searchID,
                    dbNameList,
                    queryWord,
                    fromList,
                    matchStyle,
                    formatList,
                    maxResults));
            task.Wait();
            if (task.IsFaulted)
            {
                AddErrorLine(GetExceptionText(task.Exception));
            }
            task.Dispose();
#endif
            Task.Factory.StartNew(() => SearchAndResponse(
        searchID,
        dbNameList,
        queryWord,
        fromList,
        matchStyle,
        formatList,
        maxResults));
        }


        // TODO: 本函数最好放在一个工作线程内执行
        // Form Close 的时候要及时中断工作线程
        public void SearchAndResponse(
            string searchID,
            string dbNameList,
            string queryWord,
            string fromList,
            string matchStyle,
            string formatList,
            long maxResults)
        {
            IList<BiblioRecord> records = new List<BiblioRecord>();

            string strResultSetName = "default";

            LibraryChannel channel = GetChannel();
            try
            {
                string strError = "";
                string strQueryXml = "";
                long lRet = channel.SearchBiblio(null,
                    dbNameList,
                    queryWord,
                    (int)maxResults,
                    fromList,
                    matchStyle,
                    "zh",
                    strResultSetName,
                    "", // strSearchStyle
                    "", // strOutputStyle
                    out strQueryXml,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.CirculationClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        Response(
searchID,
0,
0,
records,
strError);  // 出错信息大概为 not found。
                        return;
                    }
                    Response(
        searchID,
        -1,
        0,
        records,
        strError);
                    return;
                }
                else
                {
                    long lHitCount = lRet;

                    long lStart = 0;
                    long lPerCount = Math.Min(50, lHitCount);
                    DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                    // 装入浏览格式
                    for (; ; )
                    {
                        string strBrowseStyle = "id,xml";

                        lRet = channel.GetSearchResult(
            null,
            strResultSetName,
            lStart,
            lPerCount,
            strBrowseStyle,
            "zh", // this.Lang,
            out searchresults,
            out strError);
                        if (lRet == -1)
                        {
                            // 报错
                            Response(
                searchID,
                -1,
                0,
                records,
                strError);
                            return;
                        }

                        if (searchresults.Length == 0)
                        {
                            // 报错
                            Response(
                searchID,
                -1,
                0,
                records,
                "GetSearchResult() searchResult empty");
                            return;
                        }

                        records.Clear();
                        foreach (DigitalPlatform.CirculationClient.localhost.Record record in searchresults)
                        {
                            BiblioRecord biblio = new BiblioRecord();
                            biblio.RecPath = record.Path;
                            biblio.Data = record.RecordBody.Xml;
                            records.Add(biblio);
                        }

                        Response(
                            searchID,
                            lHitCount,
                            lStart,
                            records,
                            "");

                        lStart += searchresults.Length;
                        // lCount -= searchresults.Length;
                        if (lStart >= lHitCount || lPerCount <= 0)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AddErrorLine("SearchAndResponse() 出现异常: " + ex.Message);
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
        }

        public override void OnSearchResponseRecieved(string searchID,
            long resultCount,
            long start,
            IList<BiblioRecord> records,
            string errorInfo)
        {
#if NO
            int i = 0;
            foreach (BiblioRecord record in records)
            {
                AddInfoLine((i + 1).ToString());
                AddInfoLine("data: " + record.Data);
                i++;
            }
#endif
            if (SearchResponseEvent != null)
            {
                SearchResponseEventArgs e = new SearchResponseEventArgs();
                e.SsearchID = searchID;
                e.ResultCount = resultCount;
                e.Start = start;
                e.Records = records;
                e.ErrorInfo = errorInfo;
                this.SearchResponseEvent(this, e);
            }
        }

        public bool ShareBiblio
        {
            get
            {
                // 共享书目数据
                if (this.MainForm == null || this.MainForm.AppInfo == null)
                    return false;
                return this.MainForm.AppInfo.GetBoolean(
                    "message",
                    "share_biblio",
                    false);
            }
            set
            {
                if (this.MainForm == null || this.MainForm.AppInfo == null)
                    return;
                bool bOldValue = this.MainForm.AppInfo.GetBoolean(
                    "message",
                    "share_biblio",
                    false);
                if (bOldValue != value)
                {
                    this.MainForm.AppInfo.SetBoolean(
                        "message",
                        "share_biblio",
                        value);
                    if (this.IsConnected)
                        this.Login();    // 重新登录
                }
            }
        }

        public override void Login()
        {
#if NO
            ChatLoginDialog dlg = new ChatLoginDialog();
            dlg.ShowDialog(this.MainForm);

            if (dlg.DialogResult != System.Windows.Forms.DialogResult.OK)
                return;

            Login(dlg.UserName,
            dlg.Password,
            Guid.NewGuid().ToString(),   // this.MainForm.ServerUID,
            dlg.ShareBiblio ? "biblio_search" : "");
#endif
            Login("",
                "",
                this.MainForm.ServerUID,    // 测试用 Guid.NewGuid().ToString(),
                this.MainForm.LibraryName,
                this.ShareBiblio ? "biblio_search" : "");
        }

        public override void WaitTaskComplete(Task task)
        {
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
                Thread.Sleep(200);
            }
        }
    }


    /// <summary>
    /// 检索响应事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void SearchResponseEventHandler(object sender,
    SearchResponseEventArgs e);

    /// <summary>
    /// 检索响应事件的参数
    /// </summary>
    public class SearchResponseEventArgs : EventArgs
    {
        public string SsearchID = "";   // 检索请求的 ID
        public long ResultCount = 0;    // 整个结果集中记录个数
        public long Start = 0;  // Records 从整个结果集的何处开始
        public IList<BiblioRecord> Records = null;  // 命中的书目记录集合
        public string ErrorInfo = "";   // 错误信息
    }
}
