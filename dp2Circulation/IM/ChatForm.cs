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
using System.Collections;

using Microsoft.Win32;

using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.MessageClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    public partial class ChatForm : MyForm
    {
        const int COLUMN_ICON = 0;
        const int COLUMN_GROUPNAME = 1;
        const int COLUMN_NEWMESSAGECOUNT = 2;

        ChatHost _host = new ChatHost();

        public ChatForm()
        {
            this.UseLooping = true; // 2022/11/2

            InitializeComponent();

            this.webBrowser1.Width = 300;
            this.panel_input.Width = 300;

            {
                _host.ChatForm = this;
                webBrowser1.ObjectForScripting = _host;
            }
        }

        private void IMForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            this.ClearHtml();
            this.LoadRows();

            if (Program.MainForm != null && Program.MainForm.MessageHub != null)
            {
                Program.MainForm.MessageHub.ConnectionStateChange += MessageHub_ConnectionStateChange;
                Program.MainForm.MessageHub.AddMessage += MessageHub_AddMessage;
            }
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            // await DoLoadMessage(_currentGroupInfo.GroupName, "", true);
        }

        string[] default_groups = new string[] {
        "<default>",
        };

        void FillGroupList(List<string> names)
        {
            this.dpTable_groups.Rows.Clear();
            foreach (string name in names)
            {
                /*
                DpRow row = new DpRow();
                row.Add(new DpCell());
                row.Add(new DpCell { Text = name });
                this.dpTable_groups.Rows.Add(row);
                */
                AddNewRow(name, "");
            }

            if (this.dpTable_groups.Rows.Count > 0)
                this.dpTable_groups.SelectRange(this.dpTable_groups.Rows[0],
                    this.dpTable_groups.Rows[0]);
        }

        async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                await FillDeltaMessage();
        }

        async void MessageHub_ConnectionStateChange(object sender, ConnectionEventArgs e)
        {
            if (_inGetMessage > 0)
                return;

            if (e.Action == "Reconnected"
                || e.Action == "Connected")
            {
                await FillDeltaMessage();
            }
        }


        async Task FillDeltaMessage()
        {
            if (_currentGroupInfo == null)
                return;

            // 补充获取先前在没有连接时间段的消息
            bool bUrlChanged = Program.MainForm.MessageHub.dp2MServerUrl != _currentUrl;

            _currentUrl = Program.MainForm.MessageHub.dp2MServerUrl;

            /*
            string strLastTime = "";
            if (_currentGroupInfo.LastMessage != null && bUrlChanged == false)
                strLastTime = _currentGroupInfo.LastMessage.publishTime.ToString("G");


            _edgeRecord = _currentGroupInfo.LastMessage;

            // Task.Factory.StartNew(() => DoLoadMessage(_currentGroupName, strLastTime + "~", bUrlChanged));
            await DoLoadMessage(_currentGroupInfo.GroupName, strLastTime + "~", bUrlChanged);
            // TODO: 填充消息的时候，如果和上次最末一条同样时间，则要从返回结果集合中和上次最末一条 ID 匹配的后一条开始填充
            */

            var range = _currentGroupInfo.StartDate + "~";
            await DoLoadMessage(_currentGroupInfo.GroupName, range, true);
        }

        async void MessageHub_AddMessage(object sender, AddMessageEventArgs e)
        {
            // this.BeginInvoke(new Action<AddMessageEventArgs>(AddMessage), e);
            await AddMessage(e);
        }

        // 如果必要，加入时间提示行
        void AddTimeLine(MessageRecord record)
        {
            DateTime lastTime = new DateTime(0);
            if (_currentGroupInfo.LastMessage != null)
                lastTime = _currentGroupInfo.LastMessage.publishTime;

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

        static bool GroupNameEqual(string string1, string string2)
        {
            string name1 = string1;
            if (string1.IndexOf(":") != -1)
                name1 = StringUtil.ParseTwoPart(string1, ":")[1];

            string name2 = string2;
            if (string2.IndexOf(":") != -1)
                name2 = StringUtil.ParseTwoPart(string2, ":")[1];

            return name1 == name2;
        }

        static string GetPureName(string text)
        {
            if (text.IndexOf(":") == -1)
                return text;

            return StringUtil.ParseTwoPart(text, ":")[1];
        }

        static bool GroupNameContains(string[] names, string name)
        {
            /*
            foreach (var current in names)
            {
                if (GroupNameEqual(current, name))
                    return true;
            }

            return false;
            */
            return GroupNameIndexOf(names, name) != -1;
        }

        static int GroupNameIndexOf(string[] names, string name)
        {
            // 2020/4/13
            if (names == null)
                return -1;

            int i = 0;
            foreach (var current in names)
            {
                if (GroupNameEqual(current, name))
                    return i;
                i++;
            }

            return -1;
        }

        // 更新左侧群名列表。
        // 增补群名，和更新名字右侧的新消息数字
        void UpdateGroupNameList(string[] groups)
        {
            if (groups == null)
                return;

            // 存储发现以后剩下的名字
            List<string> names = new List<string>(groups);
            foreach (var row in this.dpTable_groups.Rows)
            {
                string name = row[COLUMN_GROUPNAME].Text;

                int index = GroupNameIndexOf(names.ToArray(), name);
                if (index != -1)
                {
                    if (name != _currentGroupInfo.GroupName)
                    {
                        string old_value = row[COLUMN_NEWMESSAGECOUNT].Text;
                        this.Invoke((Action)(() =>
                        {
                            // 注意所在线程应该为界面线程
                            row[COLUMN_NEWMESSAGECOUNT].Text = IncValue(old_value, 1);
                        }));
                    }
                    names.RemoveAt(index);    // 去掉一个发现的名字
                }
            }

            // 剩下部分群名在列表中没有找到，需要添加到群名列表中
            this.Invoke((Action)(() =>
            {
                foreach (var name in names)
                {
                    AddNewRow(GetPureName(name), "1");
                }
            }));
            return;

            string IncValue(string old_value, int delta)
            {
                if (Int32.TryParse(old_value, out int count) == false)
                    count = 0;
                return (count + delta).ToString();
            }
        }

        // 清除群名列表中右侧的新消息数字
        void ClearNewCount(string group_name)
        {
            foreach (var row in this.dpTable_groups.Rows)
            {
                string name = row[COLUMN_GROUPNAME].Text;
                if (name == group_name)
                {
                    this.Invoke((Action)(() =>
                    {
                        row[COLUMN_NEWMESSAGECOUNT].Text = "";
                    }));
                    return;
                }
            }
        }

        // TODO: 对不是当前群组的消息，要更新左侧群名列表右侧的新消息数显示
        async Task AddMessage(AddMessageEventArgs e)
        {
            // TODO: fragment 消息是否能处理?
            foreach (MessageRecord record in e.Records)
            {
                UpdateGroupNameList(record.groups);

                // 忽略不是当前群组的消息
                if (GroupNameContains(record.groups, _currentGroupInfo.GroupName) == false)
                    continue;

                AddTimeLine(record);

                // creator 要替换为用户名
                this.AddMessageLine(
                    IsMe(record) ? "right" : "left",
                    string.IsNullOrEmpty(record.userName) ? GetShortUserName(record.creator) : record.userName,
                    record.data);

                _currentGroupInfo.LastMessage = record;
            }
        }

        // 是否为自己发出的消息
        bool IsMe(MessageRecord record)
        {
            if (string.IsNullOrEmpty(Program.MainForm.MessageHub.UserName) == false
                && record.userName == Program.MainForm.MessageHub.UserName)
                return true;
            if (string.IsNullOrEmpty(Program.MainForm.MessageHub.UserName))
            {
                string strParameters = Program.MainForm.MessageHub.Parameters;
                Hashtable table = StringUtil.ParseParameters(strParameters, ',', '=', "url");
                string strLibraryName = (string)table["libraryName"];
                string strLibraryUID = (string)table["libraryUID"];
                string strLibraryUserName = (string)table["libraryUserName"];

                string strText = strLibraryUserName + "@" + strLibraryName + "|" + strLibraryUID;

                if (CompareUserName(record.creator, "~" + strText) == true)
                    return true;
            }

            return false;
        }

        // 从 xxxx@xxxx|xxxxx 中取得前两个部分
        static string GetShortUserName(string strText)
        {
            if (string.IsNullOrEmpty(strText))
                return "";

            int nRet = strText.IndexOf("|");
            if (nRet == -1)
                return strText;
            return strText.Substring(0, nRet);
        }

        static bool CompareUserName(string s1, string s2)
        {
            if (s1 == s2)
                return true;

            UserNameStruct userName1 = UserNameStruct.Build(s1);
            UserNameStruct userName2 = UserNameStruct.Build(s2);

            // 优先比较 UID
            if (string.IsNullOrEmpty(userName1.LibraryUID) == false
                && string.IsNullOrEmpty(userName2.LibraryUID) == false)
            {
                if (userName1.UserName == userName2.UserName
                    && userName1.LibraryUID == userName2.LibraryUID)
                    return true;
            }

            // 然后比较 LibraryName
            if (string.IsNullOrEmpty(userName1.LibraryUID) == true
    && string.IsNullOrEmpty(userName2.LibraryUID) == true)
            {
                if (userName1.UserName == userName2.UserName
                    && userName1.LibraryName == userName2.LibraryName)
                    return true;
            }

            return false;
        }

        class UserNameStruct
        {
            public string UserName = "";
            public string LibraryName = "";
            public string LibraryUID = "";
            public static UserNameStruct Build(string strText)
            {
                UserNameStruct result = new UserNameStruct();
                List<string> array1 = StringUtil.ParseTwoPart(strText, "@");
                result.UserName = array1[0];

                List<string> array2 = StringUtil.ParseTwoPart(array1[1], "|");
                result.LibraryName = array2[0];
                result.LibraryUID = array2[1];
                return result;
            }
        }

        private void IMForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void IMForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _redoLoadMesssageCount = 100; // 让重试尽快结束

            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            if (Program.MainForm != null && Program.MainForm.MessageHub != null)
            {
                Program.MainForm.MessageHub.AddMessage -= MessageHub_AddMessage;
                Program.MainForm.MessageHub.ConnectionStateChange -= MessageHub_ConnectionStateChange;
            }

            SaveRows();

            //CloseConnection();

            //this._channelPool.BeforeLogin -= new BeforeLoginEventHandle(_channelPool_BeforeLogin);
        }

        // 登录到 IM 服务器
        void SignIn()
        {

        }



        public override void UpdateEnable(bool bEnable)
        {
            this.textBox_input.Enabled = bEnable;
            this.button_send.Enabled = bEnable;
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
            /*
            string strText = "<div class='item'>"
+ "<div class='item_line'>"
+ " <div class='time'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            */
            AppendHtml(BuildTimeLine(strContent));
        }

        static string BuildTimeLine(string strContent)
        {
            return "<div class='item'>"
+ "<div class='item_line'>"
+ " <div class='time'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
        }

        void AddMessageLine(string left, string strName, string strContent)
        {
            /*
            if (strName == null)
                strName = "";
            if (strContent == null)
                strContent = "";

            string strText = "<div class='item'>"
+ "<div class='item_line_" + left + "'>"
+ " <div class='item_prefix_text_" + left + "'>" + HttpUtility.HtmlEncode(strName).Replace("\r\n", "<br/>") + "</div>"
+ " <div class='item_summary_" + left + "'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>").Replace(" ", "&nbsp;") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            */
            var strText = BuildMessageLine(left, strName, strContent);
            AppendHtml(strText);
        }

        static string BuildMessageLine(string left, string strName, string strContent)
        {
            if (strName == null)
                strName = "";
            if (strContent == null)
                strContent = "";

            return "<div class='item'>"
+ "<div class='item_line_" + left + "'>"
+ " <div class='item_prefix_text_" + left + "'>" + HttpUtility.HtmlEncode(strName).Replace("\r\n", "<br/>") + "</div>"
+ " <div class='item_summary_" + left + "'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>").Replace(" ", "&nbsp;") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
        }

        void AddErrorLine(string strContent)
        {
            AppendHtml(BuildErrorLine(strContent));
        }

        static string BuildErrorLine(string strContent)
        {
            return "<div class='item error'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
        }

        void ExpandErrorLine(string strContent)
        {
            webBrowser1.Document.InvokeScript("insertHtml",
                new object[] { BuildErrorLine(strContent) });
        }

        /// 清除已有的 HTML 显示
        public void ClearHtml()
        {
            string strBinDir = Program.MainForm.UserDir;

            string strCssUrl = Path.Combine(Program.MainForm.DataDir, "message.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = Path.Combine(Program.MainForm.DataDir, "IM\\chat.js");
            string strScriptHead = "\r\n<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-1.4.4.min.js\" ></script>"
    + "\r\n<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-ui-1.8.7.min.js\" ></script>"
+ "\r\n<script type=\"text/javascript\" charset='UTF-8' src=\"" + strJs + "\" ></script>";  // 
            string buttons = "<div class='buttons'><button class='expand' onclick='window.external.expand();'>查看更早的消息</button></div>";
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
                // "<html><head>" 
                "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n<head>"
                + strLink
                + strScriptHead.Replace("%bindir%", strBinDir)
                + "</head><body>"
                + buttons);
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
                string strGifFileName = Path.Combine(Program.MainForm.DataDir, "ajax-loader3.gif");
                AppendHtml("<h2 id='waiting1' align='center'><img src='" + strGifFileName + "' /></h2>"
                    + "<h2 id='waiting2' align='center'>" + HttpUtility.HtmlEncode(strText) + "</h2>",
                    false);
            }
        }

        // delegate void Delegate_AppendHtml(string strText);
        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        /// <param name="scrollToEnd">是否要卷滚到视图末尾</param>
        public void AppendHtml(string strText, bool scrollToEnd = true)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string, bool>(AppendHtml), strText, scrollToEnd);
                return;
            }

            Global.WriteHtml(this.webBrowser1,
                strText);

            if (scrollToEnd)
            {
                // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
                this.webBrowser1.Document.Window.ScrollTo(0,
                    this.webBrowser1.Document.Body.ScrollRectangle.Height);
            }
        }

        private void button_send_Click(object sender, EventArgs e)
        {
#if NO
            string strUserName = Program.MainForm.GetCurrentUserName();
            HubProxy.Invoke("Send", strUserName, this.textBox_input.Text);
            textBox_input.Text = string.Empty;
            textBox_input.Focus();
#endif
            if (string.IsNullOrEmpty(this.textBox_input.Text))
            {
                MessageBox.Show(this, "尚未输入文字");
                return;
            }
            Task.Factory.StartNew(() => SendMessage(_currentGroupInfo.GroupName,
                this.textBox_input.Text));
        }

        void SendMessage(string strGroupName, string strText)
        {
            this.EnableControls(false);
            try
            {
                List<MessageRecord> messages = new List<MessageRecord>();
                MessageRecord record = new MessageRecord();
                record.groups = new string[1] { strGroupName };
                record.data = strText;
                messages.Add(record);

                SetMessageRequest param = new SetMessageRequest("create",
                    "",
                   messages);

                SetMessageResult result = Program.MainForm.MessageHub.SetMessageAsync(param).Result;
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
            }
            finally
            {
                this.EnableControls(true);
            }
        }

        int _redoLoadMesssageCount = 0;

        int _inGetMessage = 0;  // 防止因为 ConnectionStateChange 事件导致重入

        // 装载已经存在的消息记录，追加到窗口后面
        async Task DoLoadMessage(string strGroupName,
            string strTimeRange,
            bool bClearAll)
        {
            if (_inGetMessage > 0)
                return;

            _inGetMessage++;
            try
            {
                string strError = "";

                if (Program.MainForm == null)
                    return;

                // TODO: 如果当前 Connection 尚未连接，则要促使它连接，然后重试 load
                if (Program.MainForm.MessageHub.IsConnected == false)
                {
                    if (_redoLoadMesssageCount < 5)
                    {
                        AddErrorLine("当前点对点连接尚未建立。重试操作中 ...");
                        Program.MainForm.MessageHub.CloseConnection();
                        await Program.MainForm.MessageHub.EnsureConnectAsync();
                        Thread.Sleep(5000);
                        _redoLoadMesssageCount++;
                        // await Task.Factory.StartNew(() => DoLoadMessage(strGroupName, strTimeRange, bClearAll));
                        await DoLoadMessage(strGroupName, strTimeRange, bClearAll);
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
                    _currentGroupInfo.LastMessage = null;
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
                        MessageResult result = await Program.MainForm.MessageHub.GetMessageAsync(
                            request,
                            FillMessage,
                            new TimeSpan(0, 1, 0),
                            cancel_token);
                        if (result.Value == -1)
                        {
                            //strError = result.ErrorInfo;
                            //goto ERROR1;
                            this.AddErrorLine(result.ErrorInfo);
                        }

                        // 2021/8/15
                        // 如果一行 message 也没有填入，要补加一个时间行
                        if (_currentGroupInfo.LastMessage == null)
                        {
                            AddTimeLine(_currentGroupInfo.StartDate);
                        }

                        // 把左侧列表中当前群名行的新消息数字清除
                        ClearNewCount(strGroupName);
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

        public void BeginExpandMessage()
        {
            DateTime.TryParse(_currentGroupInfo.StartDate, out DateTime time);
            time = time.AddDays(-1);
            var range = time.ToString("yyyy-MM-dd") + "~" + _currentGroupInfo.StartDate;

            _ = ExpandMessageAsync(_currentGroupInfo.GroupName, range);
        }

        // 在窗口前面插入消息
        async Task ExpandMessageAsync(string strGroupName,
            string strTimeRange)
        {
            if (_inGetMessage > 0)
                return;

            _inGetMessage++;
            try
            {
                string strError = "";

                if (Program.MainForm == null)
                    return;

                // TODO: 如果当前 Connection 尚未连接，则要促使它连接，然后重试 load
                if (Program.MainForm.MessageHub.IsConnected == false)
                {
                    if (_redoLoadMesssageCount < 5)
                    {
                        AddErrorLine("当前点对点连接尚未建立。重试操作中 ...");
                        Program.MainForm.MessageHub.CloseConnection();
                        await Program.MainForm.MessageHub.EnsureConnectAsync();
                        Thread.Sleep(5000);
                        _redoLoadMesssageCount++;
                        // await Task.Factory.StartNew(() => DoLoadMessage(strGroupName, strTimeRange, bClearAll));
                        await ExpandMessageAsync(strGroupName, strTimeRange);
                        return;
                    }
                    else
                    {
                        AddErrorLine("当前点对点连接尚未建立。停止重试。消息装载失败。");
                        _redoLoadMesssageCount = 0; // 以后再调用本函数，就重新计算重试次数
                        return;
                    }
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
                        List<MessageRecord> messages = new List<MessageRecord>();
                        MessageResult result = await Program.MainForm.MessageHub.GetMessageAsync(
                            request,
                            (StringBuilder c,
                            long totalCount,
                            long start,
                            IList<MessageRecord> records,
                            string errorInfo,
                            string errorCode) =>
                            {
                                if (records != null)
                                    messages.AddRange(records);
                            },
                            new TimeSpan(0, 1, 0),
                            cancel_token);
                        if (result.Value == -1)
                        {
                            // 在前部插入错误行
                            this.ExpandErrorLine(result.ErrorInfo);
                            return;
                        }

                        StringBuilder text = new StringBuilder();
                        StringBuilder cache = new StringBuilder();  // 用于拼接不完整消息文字
                        int timeline_count = 0;
                        foreach (var record in messages)
                        {
                            if (timeline_count == 0)
                            {
                                text.Append(BuildTimeLine(record.publishTime.ToString()));
                                timeline_count++;
                            }

                            if (string.IsNullOrEmpty(record.id)
&& cache.Length < MAX_MESSAGE_DATA_LENGTH)
                            {
                                cache.Append(record.data);
                                continue;
                            }
                            else
                            {
                                if (cache.Length > 0)
                                {
                                    cache.Append(record.data);
                                    string data = cache.ToString();
                                    cache.Clear();

                                    record.data = data;
                                }

                                // creator 要替换为用户名
                                text.Append(BuildMessageLine(
                                IsMe(record) ? "right" : "left",
                                string.IsNullOrEmpty(record.userName) ? GetShortUserName(record.creator) : record.userName,
                                record.data));
                            }
                        }

                        // 更新起始日期
                        {
                            var parts = StringUtil.ParseTwoPart(strTimeRange, "~");
                            _currentGroupInfo.StartDate = parts[0];
                        }

                        if (timeline_count == 0)
                            text.Append(BuildTimeLine(_currentGroupInfo.StartDate));

                        webBrowser1.Document.InvokeScript("insertHtml", new object[] { text.ToString() });

                        // 把左侧列表中当前群名行的新消息数字清除
                        ClearNewCount(strGroupName);
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

        public void InsertHtml(string html)
        {
            webBrowser1.Document.InvokeScript("insertHtml", new object[] { html });
        }


        // 增量获取时候的起点边界时间。要在这个 publishTime 的若干条记录中找到一条 id 为 的，它前面的记录要忽略
        MessageRecord _edgeRecord = null;
        List<MessageRecord> _edgeRecords = new List<MessageRecord>();

        string _currentUrl = "";
        // string _currentGroupName = "";  // "<default>";
        // MessageRecord _lastMessage = null;   // 当前消息显示界面中最后一条消息

        // return:
        //      true    这条记录已经被缓存，暂时不加入显示
        //      false   函数返回后继续处理，加入显示
        bool ProcessEdge(
            StringBuilder cache,
            MessageRecord record)
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
                FillMessage(cache, results);
            }

            return false;
        }

        // 拼接后 data 的最大长度
        const int MAX_MESSAGE_DATA_LENGTH = 1024 * 1024;

        void FillMessage(
            StringBuilder cache,
            IList<MessageRecord> records)
        {
            if (records != null)
            {
                foreach (MessageRecord record in records)
                {
                    AddTimeLine(record);

                    if (string.IsNullOrEmpty(record.id)
    && cache.Length < MAX_MESSAGE_DATA_LENGTH)
                    {
                        cache.Append(record.data);
                        continue;
                    }
                    else
                    {
                        if (cache.Length > 0)
                        {
                            cache.Append(record.data);
                            string data = cache.ToString();
                            cache.Clear();

                            record.data = data;
                        }

                        // creator 要替换为用户名
                        this.AddMessageLine(
                        IsMe(record) ? "right" : "left",
                        string.IsNullOrEmpty(record.userName) ? GetShortUserName(record.creator) : record.userName,
                        record.data);
                    }

                    _currentGroupInfo.LastMessage = record;
                }
            }
        }

        // 回调函数，用消息填充浏览器控件
        void FillMessage(
            StringBuilder cache,
            long totalCount,
            long start,
            IList<MessageRecord> records,
            string errorInfo,
            string errorCode)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                this.webBrowser1.Invoke(new Action<StringBuilder, long, long, IList<MessageRecord>, string, string>(FillMessage),
                    cache, totalCount, start, records, errorInfo, errorCode);
                return;
            }

            if (records != null)
            {
                foreach (MessageRecord record in records)
                {
                    if (ProcessEdge(cache, record))
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
                    FillMessage(cache, temp);

                    _currentGroupInfo.LastMessage = record;
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

        class GroupInfo
        {
            public string GroupName { get; set; }
            string _startDate = DateTime.Now.ToString("yyyy-MM-dd");
            public string StartDate
            {
                get { return _startDate; }
                set { _startDate = value; }
            }
            public MessageRecord LastMessage { get; set; }
        }

        GroupInfo _currentGroupInfo = null;

        // string _startDate = DateTime.Now.ToString("yyyy-MM-dd");

        private void dpTable_groups_SelectionChanged(object sender, EventArgs e)
        {
#if REMOVED
            string oldGroupName = _currentGroupName;
            string newGroupName = "";
            if (this.dpTable_groups.SelectedRows.Count == 1)
            {
                newGroupName = this.dpTable_groups.SelectedRows[0][1].Text;
            }

            if (newGroupName != _currentGroupName)
            {
                _currentGroupName = newGroupName;
                if (string.IsNullOrEmpty(_currentGroupName))
                {
                    ClearHtml();
                    _lastMessage = null;
                }
                else
                {
                    // 2021/8/13
                    var range = _startDate + "~";

                    _ = Task.Factory.StartNew(() => DoLoadMessage(_currentGroupName, range, true));
                }
            }
#endif
            var oldGroupInfo = _currentGroupInfo;
            GroupInfo newGroupInfo = null;
            if (this.dpTable_groups.SelectedRows.Count == 1)
            {
                newGroupInfo = this.dpTable_groups.SelectedRows[0].Tag as GroupInfo;
                Debug.Assert(newGroupInfo != null);
            }

            if (newGroupInfo != _currentGroupInfo)
            {
                _currentGroupInfo = newGroupInfo;
                if (string.IsNullOrEmpty(_currentGroupInfo?.GroupName))
                {
                    ClearHtml();
                    if (_currentGroupInfo != null)
                        _currentGroupInfo.LastMessage = null;
                }
                else
                {
                    // 2021/8/13
                    var range = _currentGroupInfo.StartDate + "~";

                    // _ = Task.Factory.StartNew(() => DoLoadMessage(_currentGroupInfo.GroupName, range, true));
                    _ = DoLoadMessage(_currentGroupInfo.GroupName, range, true);
                }
            }

        }

        private void dpTable_groups_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("添加群名");
            menuItem.Click += new System.EventHandler(this.menu_newGroupName_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除群名(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteGroupName_Click);
            if (this.dpTable_groups.SelectedRows.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.dpTable_groups, new Point(e.X, e.Y));
        }

        void menu_newGroupName_Click(object sender, EventArgs e)
        {
            var name = InputDlg.GetInput(this,
                "添加群名",
                "群名",
                "",
                this.Font);
            if (name == null)
                return;

            AddNewRow(name, "");
        }

        DpRow AddNewRow(string name, string new_message_count)
        {
            DpRow row = new DpRow();
            row.Add(new DpCell());
            row.Add(new DpCell { Text = name });
            row.Add(new DpCell { Text = new_message_count });
            row.Tag = new GroupInfo { GroupName = name };
            this.dpTable_groups.Rows.Add(row);
            return row;
        }

        void menu_deleteGroupName_Click(object sender, EventArgs e)
        {
            List<DpRow> rows = new List<DpRow>(this.dpTable_groups.SelectedRows);
            foreach (var row in rows)
            {
                this.dpTable_groups.Rows.Remove(row);
            }
        }

        void LoadRows()
        {
            string value = Program.MainForm.AppInfo.GetString("chatForm", "groups", "");
            List<string> names = null;
            if (string.IsNullOrEmpty(value) == false)
                names = JsonConvert.DeserializeObject<List<string>>(value);

            if (names == null || names.Count == 0)
                names = new List<string>(default_groups);

            FillGroupList(names);
        }

        void SaveRows()
        {
            List<string> names = new List<string>();
            foreach (var row in this.dpTable_groups.Rows)
            {
                string name = row[1].Text;
                names.Add(name);
            }

            string value = JsonConvert.SerializeObject(names);
            Program.MainForm.AppInfo.SetString("chatForm", "groups", value);
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
