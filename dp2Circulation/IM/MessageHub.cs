using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;
using Microsoft.AspNet.SignalR.Client.Hubs;
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

namespace dp2Circulation
{
    /// <summary>
    /// 负责消息交换的类
    /// 能响应外部发来的检索请求
    /// </summary>
    public class MessageHub
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

            if (string.IsNullOrEmpty(this.dp2MServerUrl) == false)
            {
                this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
            }
        }

        // 确保连接和登录
        public void Connect()
        {
            if (string.IsNullOrEmpty(this.dp2MServerUrl) == false
                && this.Connection == null)
            {
                this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
            }
        }

        public void Close()
        {
            CloseConnection();

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
            e.Cancel = true;
        }

        public LibraryChannel GetChannel()
        {
            string strServerUrl = this.MainForm.LibraryServerUrl;
            string strUserName = this.MainForm.DefaultUserName;

            return this._channelPool.GetChannel(strServerUrl, strUserName);
        }

        string dp2MServerUrl
        {
            get
            {
                // dp2MServer URL
                return this.MainForm.AppInfo.GetString("config",
                    "im_server_url",
                    "http://dp2003.com/dp2MServer");
            }
        }

        private IHubProxy HubProxy
        {
            get;
            set;
        }

        private HubConnection Connection
        {
            get;
            set;
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

        void AddInfoLine(string strContent)
        {
            string strText = "<div class='item'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
        }

        void OnAddMessageRecieved(string strName, string strContent)
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

        void AddErrorLine(string strContent)
        {
            string strText = "<div class='item error'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
        }

        #endregion

        private void ConnectAsync(string strServerUrl)
        {
            AddInfoLine("正在连接服务器 " + strServerUrl + " ...");

            Connection = new HubConnection(strServerUrl);
            Connection.Closed += new Action(Connection_Closed);
            HubProxy = Connection.CreateHubProxy("MyHub");

            Connection.Reconnected += Connection_Reconnected;
            // Connection.Error += Connection_Error;

            HubProxy.On<string, string>("AddMessage",
                (name, message) =>
                this.MainForm.Invoke(new Action<string, string>(OnAddMessageRecieved), name, message
                )
                );

            HubProxy.On<string,string,string,string,string,string,long>("searchBiblio",
                (searchID, dbNameList, queryWord, fromList, macthStyle, formatList, maxResults) =>
                OnSearchBiblioRecieved(
                searchID,
                dbNameList,
                queryWord,
                fromList,
                macthStyle,
                formatList,
                maxResults)
                );

            HubProxy.On<string,
    long,
    long,
    IList<BiblioRecord>,
        string>("responseSearchBiblio", (searchID,
    resultCount,
    start,
    records,
    errorInfo) =>
 OnSearchResponseRecieved(searchID,
    resultCount,
    start,
    records,
    errorInfo)

);


            Task task = Connection.Start();
#if NO
            CancellationTokenSource token = new CancellationTokenSource();
            if (!task.Wait(60 * 1000, token.Token))
            {
                token.Cancel();
                // labelStatusText.Text = "time out";
                AddMessageLine("error", "time out");
                return;
            }
#endif
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
                Thread.Sleep(200);
            }

            if (task.IsFaulted == true)
            {
#if NO
                if (task.Exception is HttpRequestException)
                    labelStatusText.Text = "Unable to connect to server: start server bofore connection client.";
#endif
                AddErrorLine(GetExceptionText(task.Exception));
                return;
            }

            //EnableControls(true);
            //textBox_input.Focus();
            AddInfoLine("成功连接到 " + strServerUrl);

            this.MainForm.BeginInvoke(new Action(Login));
        }

        void Connection_Error(Exception obj)
        {
            AddErrorLine(obj.ToString());
        }

        void Connection_Reconnected()
        {
            AddInfoLine("Connection_Reconnected");

            this.Login();
        }

        // 响应 server 发来的消息 SearchBiblio
        void OnSearchBiblioRecieved(
            string searchID,
            string dbNameList,
             string queryWord,
             string fromList,
             string macthStyle,
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
                    macthStyle,
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
                    macthStyle,
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
        macthStyle,
        formatList,
        maxResults));
        }

        // 调用 server 端 ResponseSearchBiblio
        public async void Response(
            string searchID,
            long resultCount,
            long start,
            IList<BiblioRecord> records,
            string errorInfo)
        {
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseSearchBiblio",
    searchID,
    resultCount,
    start,
    records,
    errorInfo);
                if (result.Value == -1)
                {
                    AddErrorLine(result.ErrorInfo);
                    return;
                }
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
            }


#if NO
            Task<MessageResult> task = HubProxy.Invoke<MessageResult>("ResponseSearchBiblio",
searchID,
resultCount,
start,
records,
errorInfo);
            task.Wait();
            if (task.IsFaulted == true)
            {
                AddErrorLine(GetExceptionText(task.Exception));
                return;
            }
            if (task.Result.Value == -1)
            {
                AddErrorLine(task.Result.ErrorInfo);
            }
#endif
        }

        // 调用 server 端 Login
        public async void Login(
            string userName,
            string password,
            string libraryUID,
            string propertyList)
        {
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("Login",
                    userName,
                    password,
                    libraryUID,
                    propertyList);
                if (result.Value == -1)
                {
                    AddErrorLine(result.ErrorInfo);
                    return;
                }
                AddInfoLine("成功登录。属性为 " + propertyList);
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
            }
        }


        // TODO: 本函数最好放在一个工作线程内执行
        // Form Close 的时候要及时中断工作线程
        public void SearchAndResponse(
            string searchID,
            string dbNameList,
            string queryWord,
            string fromList,
            string macthStyle,
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
                    macthStyle,
                    "zh",
                    strResultSetName,
                    "", // strSearchStyle
                    "", // strOutputStyle
                    out strQueryXml,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
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

        void OnSearchResponseRecieved(string searchID,
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
        }

        public void Login()
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
                Guid.NewGuid().ToString(),   // this.MainForm.ServerUID,
                this.ShareBiblio ? "biblio_search" : "");

        }

        static string GetExceptionText(AggregateException exception)
        {
            StringBuilder text = new StringBuilder();
            foreach (Exception ex in exception.InnerExceptions)
            {
                text.Append(ex.Message + "\r\n");
                // text.Append(ex.ToString() + "\r\n");
            }

            return text.ToString();
        }

        void Connection_Closed()
        {
            // this.EnableControls(false);
#if NO
            this.Invoke((Action)(() => panelChat.Visible = false));
            this.Invoke((Action)(() => buttonSend.Enabled = false));
            this.Invoke((Action)(() => this.labelStatusText.Text = "You have been disconnected."));
            this.Invoke((Action)(() => this.panelSignIn.Visible = true));
#endif
        }

        void CloseConnection()
        {
            if (this.Connection != null)
            {
                this.Connection.Stop(new TimeSpan(0, 0, 5));
                this.Connection = null;
            }
        }


        // 发起一次书目检索
        // 发起检索成功后，调主应该用 SearchResponseEvent 事件接收检索结果
        // return:
        //      -1  出错
        //      0   没有检索目标
        //      1   成功发起检索
        public int BeginSearchBiblio(
            string inputSearchID,
            string dbNameList,
            string queryWord,
            string fromList,
            string macthStyle,
            string formatList,
            long maxResults,
            out string outputSearchID,
            out string strError)
        {
            strError = "";
            outputSearchID = "";

            Task<MessageResult> task = HubProxy.Invoke<MessageResult>("RequestSearchBiblio",
                inputSearchID,
                dbNameList,
                queryWord,
                fromList,
                macthStyle,
                formatList, 
                maxResults);

            while (task.IsCompleted == false)
            {
                Application.DoEvents();
                Thread.Sleep(200);
            }

            if (task.IsFaulted == true)
            {
                // AddErrorLine(GetExceptionText(task.Exception));
                strError = GetExceptionText(task.Exception);
                return -1;
            }

            MessageResult result = task.Result;
            if (result.Value == -1)
            {
                // AddErrorLine(result.ErrorInfo);
                strError = result.ErrorInfo;
                return -1;
            }
            if (result.Value == 0)
            {
                // AddErrorLine(result.ErrorInfo);
                strError = result.ErrorInfo;
                return 0;
            }
            // AddMessageLine("search ID:", result.String);
            outputSearchID = result.String;
            return 1;
        }

    }

    public class MessageResult
    {
        public string String { get; set; }  // 字符串类型的返回值
        public long Value { get; set; }      // 整数类型的返回值
        public string ErrorInfo { get; set; }   // 出错信息
    }

    public class BiblioRecord
    {
        public string RecPath { get; set; }
        public string Format { get; set; }
        public string Data { get; set; }
        public string Timestamp { get; set; }
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
