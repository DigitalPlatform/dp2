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

using Microsoft.AspNet.SignalR.Client.Hubs;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;

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

            _timer.Interval = 1000 * 30;
            _timer.Elapsed += _timer_Elapsed;
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            AddInfoLine("tick connection state = " + this.Connection.State.ToString());

            if (this.Connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected)
            {
                AddInfoLine("自动重新连接 ...");
                this.Connect();
            }
        }

        // 确保连接和登录
        public void Connect()
        {
            if (string.IsNullOrEmpty(this.dp2MServerUrl) == false
                && (this.Connection == null || this.Connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected))
            {
                this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
            }
        }

        bool _exiting = false;  // 是否处在正在退出过程

        public void Finalize()
        {
            _timer.Stop();
            _exiting = true;
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
            // TODO: 当首次登录对话框没有输入密码的时候，这里就必须出现对话框询问密码了
            e.Cancel = true;
        }

        public LibraryChannel GetChannel()
        {
            string strServerUrl = this.MainForm.LibraryServerUrl;
            string strUserName = this.MainForm.DefaultUserName;

            return this._channelPool.GetChannel(strServerUrl, strUserName);
        }

        public string dp2MServerUrl
        {
            get
            {
                // dp2MServer URL
                return this.MainForm.AppInfo.GetString("config",
                    "im_server_url",
                    "http://dp2003.com:8083/dp2MServer");
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

        // parameters:
        //      nWarningLevel   0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public void OutputText(string strText, int nWarningLevel = 0)
        {
            string strClass = "normal";
            if (nWarningLevel == 1)
                strClass = "warning";
            else if (nWarningLevel >= 2)
                strClass = "error";
            this.AppendHtml("<div class='debug " + strClass + "'>" + HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>") + "</div>");
        }

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

        #endregion

        private void ConnectAsync(string strServerUrl)
        {
            AddInfoLine("正在连接服务器 " + strServerUrl + " ...");

            Connection = new HubConnection(strServerUrl);
            Connection.Closed += new Action(Connection_Closed);
            Connection.Reconnecting += Connection_Reconnecting;
            Connection.Reconnected += Connection_Reconnected;
            // Connection.Error += Connection_Error;


            HubProxy = Connection.CreateHubProxy("MyHub");


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


            AddInfoLine("停止 Timer");
            _timer.Stop();

            //EnableControls(true);
            //textBox_input.Focus();
            AddInfoLine("成功连接到 " + strServerUrl);

            this.MainForm.BeginInvoke(new Action(Login));
        }

        // bool tryingToReconnect = false;

        void Connection_Reconnecting()
        {
            // tryingToReconnect = true;
        }

        void Connection_Reconnected()
        {
            // tryingToReconnect = false;

            AddInfoLine("Connection_Reconnected");

            this.Login();
        }

        System.Timers.Timer _timer = new System.Timers.Timer();

        void Connection_Closed()
        {
            if (_exiting == false)
            {
                AddInfoLine("开启 Timer");
                _timer.Start(); 
            }
#if NO
            this.Invoke((Action)(() => panelChat.Visible = false));
            this.Invoke((Action)(() => buttonSend.Enabled = false));
            this.Invoke((Action)(() => this.labelStatusText.Text = "You have been disconnected."));
            this.Invoke((Action)(() => this.panelSignIn.Visible = true));
#endif
        }
        void Connection_Error(Exception obj)
        {
            AddErrorLine(obj.ToString());
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
            string libraryName,
            string propertyList)
        {
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("Login",
                    userName,
                    password,
                    libraryUID,
                    libraryName,
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
                this.MainForm.ServerUID,    // 测试用 Guid.NewGuid().ToString(),
                this.MainForm.LibraryName,
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

        // 关闭连接，并且不会引起自动重连接
        public void CloseConnection()
        {
            if (this.Connection != null)
            {
                Connection.Closed -= new Action(Connection_Closed);
                /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 xxxxxxx
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.NullReferenceException
Message: 未将对象引用设置到对象的实例。
Stack:
在 Microsoft.AspNet.SignalR.Client.Connection.Stop(TimeSpan timeout)
在 dp2Circulation.MessageHub.CloseConnection()
在 dp2Circulation.MessageHub.Close()
在 dp2Circulation.MainForm.MainForm_FormClosed(Object sender, FormClosedEventArgs e)
在 System.Windows.Forms.Form.OnFormClosed(FormClosedEventArgs e)
在 System.Windows.Forms.Form.WmClose(Message& m)
在 System.Windows.Forms.Form.WndProc(Message& m)
在 dp2Circulation.MainForm.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=2.4.5697.17821, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 5.1.2600 Service Pack 3 
操作时间 2015/8/7 10:51:56 (Fri, 07 Aug 2015 10:51:56 +0800) 
前端地址 xxxxx 经由 http://dp2003.com/dp2library 

                 * 
                 * */
                try
                {
                    this.Connection.Stop(new TimeSpan(0, 0, 5));
                }
                catch (System.NullReferenceException)
                {
                }
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

            try
            {

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
            catch(Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
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
        // 记录路径。这是本地路径，例如 “图书总库/1”
        public string RecPath { get; set; }
        // 图书馆 UID
        public string LibraryUID { get; set; }
        // 图书馆名
        public string LibraryName { get; set; }

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
