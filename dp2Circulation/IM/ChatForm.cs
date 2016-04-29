using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;
using DigitalPlatform.MessageClient;
using System.Collections;
using Microsoft.Win32;
// using Microsoft.AspNet.SignalR.Client.Hubs;


namespace dp2Circulation
{
    public partial class ChatForm : MyForm
    {
        public ChatForm()
        {
            InitializeComponent();

            this.webBrowser1.Width = 300;
            this.panel_input.Width = 300;
        }

        private void IMForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

            this.ClearHtml();

            if (this.MainForm != null && this.MainForm.MessageHub != null)
            {
                this.MainForm.MessageHub.ConnectionStateChange += MessageHub_ConnectionStateChange;
                this.MainForm.MessageHub.AddMessage += MessageHub_AddMessage;
            }
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            Task.Factory.StartNew(() => DoLoadMessage(_currentGroupName, "", true));
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                FillDeltaMessage();
        }

        void MessageHub_ConnectionStateChange(object sender, ConnectionEventArgs e)
        {
            if (_inGetMessage > 0)
                return;

            if (e.Action == "Reconnected"
                || e.Action == "Connected")
            {
                FillDeltaMessage();
            }
        }


        void FillDeltaMessage()
        {
            // 补充获取先前在没有连接时间段的消息
            bool bUrlChanged = this.MainForm.MessageHub.dp2MServerUrl != _currentUrl;
            //if (bUrlChanged == true)
            //    _lastMessage = null;

            _currentUrl = this.MainForm.MessageHub.dp2MServerUrl;

            string strLastTime = "";
            if (_lastMessage != null && bUrlChanged == false)
                strLastTime = _lastMessage.publishTime.ToString("G");

            _edgeRecord = _lastMessage;

            Task.Factory.StartNew(() => DoLoadMessage(_currentGroupName, strLastTime + "~", bUrlChanged));
            // TODO: 填充消息的时候，如果和上次最末一条同样时间，则要从返回结果集合中和上次最末一条 ID 匹配的后一条开始填充
        }

        void MessageHub_AddMessage(object sender, AddMessageEventArgs e)
        {
            this.BeginInvoke(new Action<AddMessageEventArgs>(AddMessage), e);
        }

        // 如果必要，加入时间提示行
        void AddTimeLine(MessageRecord record)
        {
            DateTime lastTime = new DateTime(0);
            if (_lastMessage != null)
                lastTime = _lastMessage.publishTime;

            if (lastTime.Date != record.publishTime.Date    // 不是同一天
    )
            {
                this.AddTimeLine(record.publishTime.ToString());
                return;
            }

            if (lastTime.Date != record.publishTime.Date    // 不是同一天
                || lastTime.Hour != record.publishTime.Hour // 不是同一小时
                || record.publishTime - lastTime > new TimeSpan(1, 0, 0)    // 和前一条差距超过一个小时
            )
            {
                if (record.publishTime.Date == DateTime.Now.Date)
                    this.AddTimeLine(record.publishTime.ToLongTimeString());    // 今天的时间，显示简略格式
                else
                    this.AddTimeLine(record.publishTime.ToString());
            }
        }

        void AddMessage(AddMessageEventArgs e)
        {
            foreach (MessageRecord record in e.Records)
            {
                AddTimeLine(record);

                // creator 要替换为用户名
                this.AddMessageLine(
                    IsMe(record) ? "right" : "left",
                    string.IsNullOrEmpty(record.userName) ? record.creator : record.userName,
                    record.data);

                _lastMessage = record;
            }
        }

        // 是否为自己发出的消息
        bool IsMe(MessageRecord record)
        {
            if (record.userName == this.MainForm.MessageHub.UserName)
                return true;
            if (string.IsNullOrEmpty(this.MainForm.MessageHub.UserName))
            {
                string strParameters = this.MainForm.MessageHub.Parameters;
                Hashtable table = StringUtil.ParseParameters(strParameters, ',', '=', "url");
                string strLibraryName = (string)table["libraryName"];
                string strLibraryUID = (string)table["libraryUID"];
                string strLibraryUserName = (string)table["libraryUserName"];

                string strText = strLibraryUserName + "@";
                if (string.IsNullOrEmpty(strLibraryName))
                    strText += strLibraryName;
                else
                    strText += strLibraryUID;

                if (record.creator == strText)
                    return true;
            }

            return false;
        }

        private void IMForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void IMForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _redoLoadMesssageCount = 100; // 让重试尽快结束

            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            if (this.MainForm != null && this.MainForm.MessageHub != null)
            {
                this.MainForm.MessageHub.AddMessage -= MessageHub_AddMessage;
                this.MainForm.MessageHub.ConnectionStateChange -= MessageHub_ConnectionStateChange;
            }

            //CloseConnection();

            //this._channelPool.BeforeLogin -= new BeforeLoginEventHandle(_channelPool_BeforeLogin);
        }

        // 登录到 IM 服务器
        void SignIn()
        {

        }



        public override void EnableControls(bool bEnable)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(EnableControls), bEnable);
                return;
            }

            this.textBox_input.Enabled = bEnable;
            this.button_send.Enabled = bEnable;

            // base.EnableControls(bEnable);
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

        void AddTimeLine(string strContent)
        {
            string strText = "<div class='item'>"
+ "<div class='item_line'>"
+ " <div class='time'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
        }

        void AddMessageLine(string left, string strName, string strContent)
        {
            if (strName == null)
                strName = "";
            if (strContent == null)
                strContent = "";

            string strText = "<div class='item'>"
+ "<div class='item_line_" + left + "'>"
+ " <div class='item_prefix_text_" + left + "'>" + HttpUtility.HtmlEncode(strName).Replace("\r\n", "<br/>") + "</div>"
+ " <div class='item_summary_" + left + "'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
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


        /// 清除已有的 HTML 显示
        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(this.MainForm.DataDir, "message.css");
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

        // parameters:
        //      strText 要显示的文字。如果为空，表示清除以前的显示，本次也不显示任何东西
        public void HtmlWaiting(WebBrowser webBrowser,
            string strText)
        {
            string[] ids = new[] { "waiting1", "waiting2" };
            foreach (string id in ids)
            {
                HtmlElement obj = this.webBrowser1.Document.GetElementById(id);
                if (obj != null)
                    obj.OuterHtml = "";
            }

            if (string.IsNullOrEmpty(strText) == false)
            {
                string strGifFileName = Path.Combine(this.MainForm.DataDir, "ajax-loader3.gif");
                AppendHtml("<h2 id='waiting1' align='center'><img src='" + strGifFileName + "' /></h2>"
                    + "<h2 id='waiting2' align='center'>" + HttpUtility.HtmlEncode(strText) + "</h2>");
            }
        }

        // delegate void Delegate_AppendHtml(string strText);
        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendHtml), strText);
                return;
            }

            Global.WriteHtml(this.webBrowser1,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
                this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        private void button_send_Click(object sender, EventArgs e)
        {
#if NO
            string strUserName = this.MainForm.GetCurrentUserName();
            HubProxy.Invoke("Send", strUserName, this.textBox_input.Text);
            textBox_input.Text = string.Empty;
            textBox_input.Focus();
#endif
            if (string.IsNullOrEmpty(this.textBox_input.Text))
            {
                MessageBox.Show(this, "尚未输入文字");
                return;
            }
            Task.Factory.StartNew(() => SendMessage("<default>", this.textBox_input.Text));
        }

        void SendMessage(string strGroupName, string strText)
        {
            this.EnableControls(false);

            List<MessageRecord> messages = new List<MessageRecord>();
            MessageRecord record = new MessageRecord();
            record.group = strGroupName;
            record.data = strText;
            messages.Add(record);

            SetMessageRequest param = new SetMessageRequest("create",
                "",
               messages);

            SetMessageResult result = this.MainForm.MessageHub.SetMessageAsync(param).Result;
            if (result.Value == -1)
            {
                this.Invoke((Action)(() => MessageBox.Show(this, result.ErrorInfo)));
            }
            else
            {
                // 调用成功后才把输入的文字清除
                this.Invoke((Action)(() => this.textBox_input.Text = ""
                    ));
            }

            this.EnableControls(true);
        }

        int _redoLoadMesssageCount = 0;

        int _inGetMessage = 0;  // 防止因为 ConnectionStateChange 事件导致重入

        // 装载已经存在的消息记录
        async void DoLoadMessage(string strGroupName,
            string strTimeRange,
            bool bClearAll)
        {
            if (_inGetMessage > 0)
                return;

            _inGetMessage++;
            try
            {
                string strError = "";

                if (this.MainForm == null)
                    return;

                // TODO: 如果当前 Connection 尚未连接，则要促使它连接，然后重试 load
                if (this.MainForm.MessageHub.IsConnected == false)
                {
                    if (_redoLoadMesssageCount < 5)
                    {
                        AddErrorLine("当前点对点连接尚未建立。重试操作中 ...");
                        this.MainForm.MessageHub.Connect();
                        Thread.Sleep(5000);
                        _redoLoadMesssageCount++;
                        Task.Factory.StartNew(() => DoLoadMessage(strGroupName, strTimeRange, bClearAll));
                        return;
                    }
                    else
                    {
                        AddErrorLine("当前点对点连接尚未建立。停止重试。消息装载失败。");
                        _redoLoadMesssageCount = 0; // 以后再调用本函数，就重新计算重试次数
                        return;
                    }
                }

                if (bClearAll)
                {
                    this.Invoke((Action)(() => this.ClearHtml()));
                    _lastMessage = null;
                }
                this.Invoke((Action)(() => this.HtmlWaiting(this.webBrowser1, "正在获取消息，请等待 ...")));

                EnableControls(false);
                try
                {
                    CancellationToken cancel_token = new CancellationToken();

                    string id = Guid.NewGuid().ToString();
                    GetMessageRequest request = new GetMessageRequest(id,
                        strGroupName, // "<default>" 表示默认群组
                        "",
                        strTimeRange,
                        0,
                        -1);
                    try
                    {
                        MessageResult result = await this.MainForm.MessageHub.GetMessageAsync(
                            request,
                            FillMessage,
                            new TimeSpan(0, 1, 0),
                            cancel_token);
#if NO
                    this.Invoke(new Action(() =>
                    {
                        SetTextString(this.webBrowser1, ToString(result));
                    }));
#endif
                        if (result.Value == -1)
                        {
                            //strError = result.ErrorInfo;
                            //goto ERROR1;
                            this.AddErrorLine(result.ErrorInfo);
                        }
                    }
                    catch (AggregateException ex)
                    {
                        strError = MessageConnection.GetExceptionText(ex);
                        goto ERROR1;
                    }
                    catch (Exception ex)
                    {
                        strError = ex.Message;
                        goto ERROR1;
                    }
                    return;
                }
                finally
                {
                    EnableControls(true);
                    this.Invoke((Action)(() => this.HtmlWaiting(this.webBrowser1, "")));
                }
            ERROR1:
                this.Invoke((Action)(() => MessageBox.Show(this, strError)));
            }
            finally
            {
                _inGetMessage--;
            }
        }

        // 增量获取时候的起点边界时间。要在这个 publishTime 的若干条记录中找到一条 id 为 的，它前面的记录要忽略
        MessageRecord _edgeRecord = null;
        List<MessageRecord> _edgeRecords = new List<MessageRecord>();

        string _currentUrl = "";
        string _currentGroupName = "<default>";
        MessageRecord _lastMessage = null;   // 当前消息显示界面中最后一条消息

        // return:
        //      true    这条记录已经被缓存，暂时不加入显示
        //      false   函数返回后继续处理，加入显示
        bool ProcessEdge(MessageRecord record)
        {
            if (_edgeRecord != null)
            {
                if (record.publishTime == _edgeRecord.publishTime)
                {
                    _edgeRecords.Add(record);
                    return true;
                }
                // 提取 id 位置后面的
                List<MessageRecord> results = new List<MessageRecord>();
                foreach (MessageRecord r in _edgeRecords)
                {
                    if (r.id == _edgeRecord.id)
                    {
                        results.Clear();
                        continue;
                    }
                    results.Add(r);
                }

                _edgeRecord = null;
                _edgeRecords.Clear();

                // results 加入显示
                FillMessage(results);
            }

            return false;
        }

        void FillMessage(IList<MessageRecord> records)
        {
            if (records != null)
            {
                foreach (MessageRecord record in records)
                {
                    AddTimeLine(record);

                    // creator 要替换为用户名
                    this.AddMessageLine(
                        IsMe(record) ? "right" : "left",
                        string.IsNullOrEmpty(record.userName) ? record.creator : record.userName,
                        record.data);

                    _lastMessage = record;
                }
            }
        }

        // 回调函数，用消息填充浏览器控件
        void FillMessage(long totalCount,
    long start,
    IList<MessageRecord> records,
    string errorInfo,
    string errorCode)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                this.webBrowser1.Invoke(new Action<long, long, IList<MessageRecord>, string, string>(FillMessage),
                    totalCount, start, records, errorInfo, errorCode);
                return;
            }

            if (records != null)
            {
                foreach (MessageRecord record in records)
                {
                    if (ProcessEdge(record))
                        continue;


#if NO
                    AddTimeLine(record);
                    // creator 要替换为用户名
                    this.AddMessageLine(
                        IsMe(record) ? "right" : "left",
                        string.IsNullOrEmpty(record.userName) ? record.creator : record.userName,
                        record.data);
#endif
                    List<MessageRecord> temp = new List<MessageRecord>() { record };
                    FillMessage(temp);

                    _lastMessage = record;
                }
            }
        }

        // 书目检索
        private void toolStripButton_searchBiblio_Click(object sender, EventArgs e)
        {
#if NO
            Task<MessageResult> task = HubProxy.Invoke<MessageResult>("RequestSearchBiblio",
                "<全部>",
                "中国",
                "<全部>",
                "left",
                "",
(Int64)100);

            while (task.IsCompleted == false)
            {
                Application.DoEvents();
                Thread.Sleep(200);
            }

            if (task.IsFaulted == true)
            {
                AddErrorLine(GetExceptionText(task.Exception));
                return;
            }

            MessageResult result = task.Result;
            if (result.Value == -1)
            {
                AddErrorLine(result.ErrorInfo);
                return;
            }
            if (result.Value == 0)
            {
                AddErrorLine(result.ErrorInfo);
                return;
            }
            AddMessageLine("search ID:", result.String);

            // 出现对话框

            // DoSearchBiblio();
#endif
        }

#if NO
        public async void DoSearchBiblio()
        {
            MessageResult result = await HubProxy.Invoke<MessageResult>(
                "RequestSearchBiblio",
                "<全部>",
                "中国",
                "<全部>",
                "left",
                100);
            if (result.Value == -1)
            {
                AddErrorLine(result.ErrorInfo);
                return;
            }

            string strSearchID = result.String;
            AddMessageLine("search ID:", result.String);
        }  
#endif
    }


}
