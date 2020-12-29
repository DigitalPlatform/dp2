using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;
using DigitalPlatform.MessageClient;
using DigitalPlatform.CirculationClient;

namespace dp2ManageCenter.Message
{
    /// <summary>
    /// 聊天窗口
    /// </summary>
    public partial class ChatForm : Form
    {
        const int COLUMN_ICON = 0;
        const int COLUMN_GROUPNAME = 1;
        const int COLUMN_NEWMESSAGECOUNT = 2;

        string _userNameAndUrl = "";

        // 当前用户名和 dp2mserver 服务器 URL
        public string UserNameAndUrl
        {
            get
            {
                return _userNameAndUrl;
            }
            set
            {
                _userNameAndUrl = value;
                this.Text = $"聊天 {_userNameAndUrl}";

                // 2020/9/20
                RemoveEvents();

                _ = AddEventAsync();
            }
        }


        public ChatForm()
        {
            InitializeComponent();
        }

        private void ChatForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            RemoveEvents();
        }

        // TODO: 把装载信息的起始日期记载下来。删除一个范围后，要能自动修改这个起始日期
        void LoadSettings()
        {
            this.UiState = ClientInfo.Config.Get("chat", "uiState", "");
            this.UserNameAndUrl = ClientInfo.Config.Get("chat", "userNameAndUrl", "");
            this.LoadGroups(this.UserNameAndUrl);
            //this.LoadStartTime(this.UserNameAndUrl);
        }

        void SaveSettings()
        {
            ClientInfo.Config.Set("chat", "uiState", this.UiState);
            ClientInfo.Config.Set("chat", "userNameAndUrl", this.UserNameAndUrl);
            this.SaveGroups(this.UserNameAndUrl);
            this.SaveStartTime(this.UserNameAndUrl, this._currentGroupName);
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.splitContainer_main,
                    this.dpTable_messages,
                    this.dpTable_groups,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.splitContainer_main,
                    this.dpTable_messages,
                    this.dpTable_groups,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        #region 群名列表

        string[] default_groups = new string[] {
        "<default>",
        };

        void FillGroupList(List<GroupInfo> names)
        {
            // 2020/9/20
            this.dpTable_messages.Rows.Clear();

            this.dpTable_groups.Rows.Clear();
            foreach (var info in names)
            {
                var row = AddGroupNameNewRow(info.DisplayName, info.EchoName, "");
                // row.Tag = info;
            }

            // 自动选择第一个 group
            if (this.dpTable_groups.Rows.Count > 0)
                this.dpTable_groups.SelectRange(this.dpTable_groups.Rows[0],
                    this.dpTable_groups.Rows[0]);
        }

        DpRow AddGroupNameNewRow(string name,
            string echoName,
            string new_message_count)
        {
            DpRow row = new DpRow();
            row.Add(new DpCell());
            row.Add(new DpCell { Text = name });
            row.Add(new DpCell { Text = new_message_count });

            row.Tag = new GroupInfo
            {
                DisplayName = name,
                EchoName = echoName
            };

            this.dpTable_groups.Rows.Add(row);
            return row;
        }


        static string speical_chars = "@<>:/,";

        // 获得一个在 XML 中可以用于元素名的字符串
        public static string GetSection(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "a";

            StringBuilder text = new StringBuilder();
            foreach (char ch in strText)
            {
                if (speical_chars.IndexOf(ch) == -1)
                    text.Append(ch);
                else
                    text.Append("_");
            }

            return "a" + text.ToString();
        }

        // 消息显示时间范围的开始时间
        string _startDate = "";

        void LoadStartTime(string userNameAndUrl, string groupName)
        {
            this._startDate = ClientInfo.Config.Get("startDate",
                GetSection(userNameAndUrl) + "_" + GetSection(groupName),
                "");
            if (string.IsNullOrEmpty(this._startDate))
                this._startDate = DateTime.Now.ToString("yyyy-MM-dd");
        }

        void SaveStartTime(string userNameAndUrl, string groupName)
        {
            ClientInfo.Config.Set("startDate",
                GetSection(userNameAndUrl) + "_" + GetSection(groupName)
                , this._startDate);
        }

        void LoadGroups(string userNameAndUrl)
        {
            /*
            string value = ClientInfo.Config.Get("groupNames", GetSection(userNameAndUrl), "");
            List<string> names = null;
            if (string.IsNullOrEmpty(value) == false)
                names = JsonConvert.DeserializeObject<List<string>>(value);

            if (names == null || names.Count == 0)
                names = new List<string>(default_groups);

            FillGroupList(names);
            */
            string value = ClientInfo.Config.Get("groupNames", GetSection(userNameAndUrl), "");
            List<GroupInfo> names = null;
            try
            {
                if (string.IsNullOrEmpty(value) == false)
                    names = JsonConvert.DeserializeObject<List<GroupInfo>>(value);
            }
            catch
            {

            }

            if (names == null || names.Count == 0)
                names = new List<GroupInfo>();

            FillGroupList(names);
        }

        void SaveGroups(string userNameAndUrl)
        {
            /*
            List<string> names = new List<string>();
            foreach (var row in this.dpTable_groups.Rows)
            {
                string name = row[1].Text;
                names.Add(name);
            }

            string value = JsonConvert.SerializeObject(names);
            ClientInfo.Config.Set("groupNames", GetSection(userNameAndUrl), value);
            */
            List<GroupInfo> names = new List<GroupInfo>();
            foreach (var row in this.dpTable_groups.Rows)
            {
                names.Add(row.Tag as GroupInfo);
            }

            string value = JsonConvert.SerializeObject(names);
            ClientInfo.Config.Set("groupNames", GetSection(userNameAndUrl), value);
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:避免使用 Async Void 方法", Justification = "<挂起>")]
        private async void toolStripButton_selectAccount_Click(object sender, EventArgs e)
        {
            using (MessageAccountForm dlg = new MessageAccountForm())
            {
                dlg.Mode = "select";
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.Cancel)
                    return;
                var account = dlg.SelectedAccount;

                this.SaveSettings();

                this.UserNameAndUrl = account.UserName + "@" + account.ServerUrl;

                this.LoadGroups(this.UserNameAndUrl);
                this.LoadStartTime(this.UserNameAndUrl, _currentGroupName);
                if (string.IsNullOrEmpty(_currentGroupName) == false)
                    await LoadMessageAsync(_currentGroupName, this._startDate, "", "clearAll");
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

        async void menu_newGroupName_Click(object sender, EventArgs e)
        {
            string strError = "";

            var name = InputDlg.GetInput(this,
                "添加群名",
                "群名",
                "",
                this.Font);
            if (name == null)
                return;

            // TODO: 如果是 un:xxx,un:xxx 这样的形态，则需要转换为 ui:xxx,ui:xxx 的正规化形态
            var result = await CanonicalizeGroupName(name);
            if (result.Value == -1)
            {
                strError = $"正规化群名 '{name}' 时出错: {result.ErrorInfo}";
                goto ERROR1;
            }

            /*
            GroupInfo info = new GroupInfo
            {
                DisplayName = name,
                EchoName = result.ResultName
            };
            */

            var row = AddGroupNameNewRow(name, result.ResultName, "");
            // row.Tag = info;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public class GroupInfo
        {
            // 显示名
            public string DisplayName { get; set; }
            // dp2mserver 采用的正规群名
            public string EchoName { get; set; }
        }

        public class CanonicalizeGroupNameResult : NormalResult
        {
            public string ResultName { get; set; }
        }

        async Task<CanonicalizeGroupNameResult> CanonicalizeGroupName(string name)
        {
            if (name.StartsWith("un:"))
            {
                P2PConnection connection = await ConnectionPool.GetConnectionAsync(this.UserNameAndUrl);
                List<MessageRecord> messages = new List<MessageRecord>();
                MessageRecord record = new MessageRecord();
                record.groups = name.Split(new char[] { ',' });   // new string[1] { strGroupName };
                record.data = "test";
                messages.Add(record);

                SetMessageRequest param = new SetMessageRequest("echo",
                    "",
                   messages);
                var result = await connection.SetMessageAsyncLite(param);
                if (result.Value == -1)
                    return new CanonicalizeGroupNameResult
                    {
                        Value = -1,
                        ErrorInfo = result.ErrorInfo
                    };
                if (result.Results == null || result.Results.Count == 0)
                    return new CanonicalizeGroupNameResult
                    {
                        Value = -1,
                        ErrorInfo = "echo 请求返回的 Results 为空"
                    };
                return new CanonicalizeGroupNameResult
                {
                    Value = 0,
                    ResultName = BuildName(result.Results[0].groups)
                };
            }

            return new CanonicalizeGroupNameResult
            {
                Value = 0,
                ResultName = name
            };
        }

        void menu_deleteGroupName_Click(object sender, EventArgs e)
        {
            List<DpRow> rows = new List<DpRow>(this.dpTable_groups.SelectedRows);
            foreach (var row in rows)
            {
                this.dpTable_groups.Rows.Remove(row);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:避免使用 Async Void 方法", Justification = "<挂起>")]
        private async void dpTable_groups_SelectionChanged(object sender, EventArgs e)
        {
            string oldGroupName = _currentGroupName;
            string newGroupName = "";
            if (this.dpTable_groups.SelectedRows.Count == 1)
            {
                newGroupName = this.dpTable_groups.SelectedRows[0][1].Text;
            }
            else
            {
                this.dpTable_messages.Rows.Clear();
            }

            if (newGroupName != _currentGroupName)
            {
                SaveStartTime(this.UserNameAndUrl, _currentGroupName);
                _currentGroupName = newGroupName;
                LoadStartTime(this.UserNameAndUrl, _currentGroupName);

                if (string.IsNullOrEmpty(_currentGroupName))
                {
                    ClearMessageList();
                    _lastMessage = null;
                }
                else
                {
                    // string startDate = DateTime.Now.ToString("yyyy-MM-dd");
                    await LoadMessageAsync(_currentGroupName, this._startDate, "", "clearAll");
                }
            }

        }

        void ClearMessageList()
        {
            this.dpTable_messages.Rows.Clear();
        }

        void DislayProgress(string text)
        {
            this.toolStripLabel_message.Text = text;
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


        // 增量获取时候的起点边界时间。要在这个 publishTime 的若干条记录中找到一条 id 为 的，它前面的记录要忽略
        MessageRecord _edgeRecord = null;
        List<MessageRecord> _edgeRecords = new List<MessageRecord>();

        // string _currentUrl = "";
        string _currentGroupName = "";  // "<default>";
        MessageRecord _lastMessage = null;   // 当前消息显示界面中最后一条消息

        int _redoLoadMesssageCount = 0;

        int _inGetMessage = 0;  // 防止因为 ConnectionStateChange 事件导致重入

        // FillMessage 用到的临时变量
        int _currentIndex = -1;
        string _currentDate = "";

        // 装载已经存在的消息记录
        async Task LoadMessageAsync(string strGroupName,
            string strStartDate,
            string strEndDate,
            string strStyle
            // string strTimeRange,
            // bool bClearAll
            )
        {
            if (_inGetMessage > 0)
                return;

            bool bClearAll = StringUtil.IsInList("clearAll", strStyle);
            bool bInsertBefore = StringUtil.IsInList("insertBefore", strStyle);

            var get_result = await ConnectionPool.OpenConnectionAsync(this.UserNameAndUrl);
            if (get_result.Value == -1)
            {
                // TODO: 显示报错信息
                return;
            }
            P2PConnection connection = get_result.Connection;
            _inGetMessage++;
            try
            {
                string strError = "";

                if (Program.MainForm == null)
                    return;

#if NO
                // TODO: 如果当前 Connection 尚未连接，则要促使它连接，然后重试 load
                if (Program.MainForm.MessageHub.IsConnected == false)
                {
                    if (_redoLoadMesssageCount < 5)
                    {
                        AddErrorLine("当前点对点连接尚未建立。重试操作中 ...");
                        Program.MainForm.MessageHub.Connect();
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
#endif

                if (bClearAll)
                {
                    // this.Invoke((Action)(() => this.ClearHtml()));
                    this.Invoke((Action)(() =>
                    this.ClearMessageList()
                    ));
                    _lastMessage = null;
                }
                this.Invoke((Action)(() =>
                {
                    this.DislayProgress("正在获取消息，请等待 ...");
                    this.dpTable_messages.BeginUpdate();
                }));
                //EnableControls(false);
                try
                {
                    if (bInsertBefore)
                        _currentIndex = 0;
                    else
                        _currentIndex = -1;

                    _currentDate = strStartDate;

                    AddMessageDateLine(_currentIndex == -1 ? -1 : _currentIndex++, strStartDate);

                    CancellationToken cancel_token = new CancellationToken();

                    string id = Guid.NewGuid().ToString();
                    GetMessageRequest request = new GetMessageRequest(id,
                        strGroupName, // "<default>" 表示默认群组
                        "",
                        strStartDate + "~" + strEndDate,
                        0,
                        -1);
                    try
                    {
                        MessageResult result = await connection.GetMessageAsyncLite(
                            request,
                            FillMessage,
                            new TimeSpan(0, 1, 0),
                            cancel_token);
                        if (result.Value == -1)
                        {
                            //strError = result.ErrorInfo;
                            //goto ERROR1;
                            this.AddMessageErrorLine(_currentIndex == -1 ? -1 : _currentIndex++, result.ErrorInfo);
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
                    //EnableControls(true);
                    this.Invoke((Action)(() =>
                    {
                        this.dpTable_messages.EndUpdate();
                        if (_bVertBottom)
                            this.dpTable_messages.EnsureVisible(this.dpTable_messages.Rows.LastOrDefault());

                        this.DislayProgress("");
                    }));
                }
            ERROR1:
                //this.Invoke((Action)(() => MessageBox.Show(this, strError)));
                AddMessageErrorLine(_currentIndex == -1 ? -1 : _currentIndex++, strError);
            }
            finally
            {
                _inGetMessage--;
            }
        }

        #region 群名有关

        static bool GroupNameContains(string[] names, string name)
        {
            return GroupNameIndexOf(names, name) != -1;
        }

        static int GroupNameIndexOf(string[] names, string name)
        {
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
            try
            {
                if (groups == null)
                {
                    // ClientInfo.WriteInfoLog("groups == null");
                    return;
                }

                // 可能形态为 ui:xxxxx,ui:xxxxx

                // 存储发现以后剩下的名字
                List<string> names = new List<string>(groups);
                string name = BuildName(groups);
                foreach (var row in this.dpTable_groups.Rows)
                {
                    var info = row.Tag as GroupInfo;
                    if (info.DisplayName == name || info.EchoName == name)
                    {
                        string old_value = row[COLUMN_NEWMESSAGECOUNT].Text;
                        this.Invoke((Action)(() =>
                        {
                            // 注意所在线程应该为界面线程
                            row[COLUMN_NEWMESSAGECOUNT].Text = IncValue(old_value, 1);
                        }));
                        // ClientInfo.WriteInfoLog($"found row in dpTable_groups.Rows");
                        return;
                    }
                }

                // 剩下部分群名在列表中没有找到，需要添加到群名列表中
                this.Invoke((Action)(() =>
                {
                    int count = this.dpTable_groups.Rows.Count;

                    // TODO: 请求服务器得到 DisplayName
                    var row = AddGroupNameNewRow(name, name, "1");

                    // 如果是列表中第一次增加行，择自动选中它
                    if (count == 0)
                        row.Selected = true;
                }));
                return;
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"UpdateGroupNameList() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }

            string IncValue(string old_value, int delta)
            {
                if (Int32.TryParse(old_value, out int count) == false)
                    count = 0;
                return (count + delta).ToString();
            }
        }

#if OLD
        // 更新左侧群名列表。
        // 增补群名，和更新名字右侧的新消息数字
        void UpdateGroupNameList(string[] groups)
        {
            if (groups == null)
                return;

            // 可能形态为 ui:xxxxx,ui:xxxxx

            // 存储发现以后剩下的名字
            List<string> names = new List<string>(groups);
            foreach (var row in this.dpTable_groups.Rows)
            {
                string name = row[COLUMN_GROUPNAME].Text;

                int index = GroupNameIndexOf(names.ToArray(), name);
                if (index != -1)
                {
                    if (name != _currentGroupName)
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
                    AddGroupNameNewRow(GetPureName(name), "1");
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
#endif
        // 构造一个群名字字符串。先把每个部分排序，然后用逗号组合
        static string BuildName(string[] groups)
        {
            List<string> results = new List<string>();
            foreach (var group in groups)
            {
                string name1 = group;
                if (group.IndexOf(":") != -1)
                    name1 = StringUtil.ParseTwoPart(group, ":")[1];
                results.Add(name1);
            }

            return StringUtil.MakePathList(results, ",");
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


        #endregion

        // 拼接后 data 的最大长度
        const int MAX_MESSAGE_DATA_LENGTH = 1024 * 1024;

        void FillMessage(
    StringBuilder cache,
    long totalCount,
    long start,
    IList<MessageRecord> records,
    string errorInfo,
    string errorCode)
        {
            if (totalCount == -1)
            {
                StringBuilder text = new StringBuilder();
                text.Append("***\r\n");
                text.Append("totalCount=" + totalCount + "\r\n");
                text.Append("errorInfo=" + errorInfo + "\r\n");
                text.Append("errorCode=" + errorCode + "\r\n");

                AddMessageErrorLine(_currentIndex == -1 ? -1 : _currentIndex++, text.ToString());
            }

            if (records != null)
            {
                foreach (MessageRecord record in records)
                {
                    string data = "";   // 拼接完成的 data
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
                            data = cache.ToString();
                            cache.Clear();

                            record.data = data;
                        }
                    }

                    string current_date = record.publishTime.ToString("yyyy-MM-dd");
                    if (current_date != _currentDate)
                    {
                        AddMessageDateLine(_currentIndex == -1 ? -1 : _currentIndex++, current_date);
                        _currentDate = current_date;
                    }

                    AddMessageLine(_currentIndex == -1 ? -1 : _currentIndex++, record);
                    /*
                    StringBuilder text = new StringBuilder();
                    text.Append("***\r\n");
                    text.Append("id=" + HttpUtility.HtmlEncode(record.id) + "\r\n");
                    text.Append("data=" + HttpUtility.HtmlEncode(record.data) + "\r\n");
                    if (record.data != null)
                        text.Append("data.Length=" + record.data.Length + "\r\n");

                    if (string.IsNullOrEmpty(data) == false)
                        text.Append("concated data=" + HttpUtility.HtmlEncode(data) + "\r\n");

                    if (record.groups != null)
                        text.Append("groups=" + HttpUtility.HtmlEncode(string.Join(",", record.groups)) + "\r\n");
                    text.Append("creator=" + HttpUtility.HtmlEncode(record.creator) + "\r\n");
                    text.Append("userName=" + HttpUtility.HtmlEncode(record.userName) + "\r\n");

                    text.Append("format=" + HttpUtility.HtmlEncode(record.format) + "\r\n");
                    text.Append("type=" + HttpUtility.HtmlEncode(record.type) + "\r\n");
                    text.Append("thread=" + HttpUtility.HtmlEncode(record.thread) + "\r\n");

                    if (record.subjects != null)
                        text.Append("subjects=" + HttpUtility.HtmlEncode(string.Join(SUBJECT_DELIM, record.subjects)) + "\r\n");

                    text.Append("publishTime=" + HttpUtility.HtmlEncode(record.publishTime.ToString("G")) + "\r\n");
                    text.Append("expireTime=" + HttpUtility.HtmlEncode(record.expireTime) + "\r\n");
                    AppendHtml(this.webBrowser_message, text.ToString());
                    */
                }
            }
        }

        class CommandLine
        {
            public string Date { get; set; }
        }

        void AddMessageDateLine(int index, string strDate)
        {
            DpRow row = new DpRow();
            row.Add(new DpCell { Text = "" });
            row.Add(new DpCell { Text = "" });
            row.Add(new DpCell { Text = $"---- {strDate} ----" });
            row.Tag = new CommandLine { Date = strDate };
            // row.Style = DpRowStyle.Seperator;
            row.BackColor = Color.DarkGray;
            row.ForeColor = Color.Yellow;

            this.Invoke((Action)(() =>
            {
                if (index == -1)
                    this.dpTable_messages.Rows.Add(row);
                else
                    this.dpTable_messages.Rows.Insert(index, row);
            }));
        }

        // parameters:
        //      index   插入位置。如果为 -1，表示在最后追加；其他表示插入的位置
        void AddMessageLine(int index, MessageRecord record)
        {
            DpRow row = new DpRow();
            row.Add(new DpCell { Text = record.userName });
            row.Add(new DpCell { Text = record.publishTime.ToString() });
            row.Add(new DpCell { Text = record.data });
            row.Tag = record;
            row.BackColor = Color.Black;
            row.ForeColor = Color.LightGray;

            this.Invoke((Action)(() =>
            {
                if (index == -1)
                {
                    this.dpTable_messages.Rows.Add(row);

                    if (_bVertBottom && this.dpTable_messages.DelayUpdate == false)
                        this.dpTable_messages.EnsureVisible(row);
                }
                else
                {
                    this.dpTable_messages.Rows.Insert(index, row);
                }
            }));
        }

        const int MESSAGE_COLUMN_SENDER = 0;
        const int MESSAGE_COLUMN_TIME = 1;
        const int MESSAGE_COLUMN_TEXT = 2;

        void AddMessageErrorLine(int index, string strContent)
        {
            DpRow row = new DpRow();
            row.Add(new DpCell { Text = "error" });
            row.Add(new DpCell { Text = "" });
            row.Add(new DpCell { Text = strContent });
            row.BackColor = Color.DarkRed;
            row.ForeColor = Color.White;

            this.Invoke((Action)(() =>
            {
                if (index == -1)
                    this.dpTable_messages.Rows.Add(row);
                else
                    this.dpTable_messages.Rows.Insert(index, row);
            }));
        }

        private void dpTable_messages_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            int selected_count = this.dpTable_messages.SelectedRows.Count;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            // TODO: 根据选中的行是否为命令行，决定菜单文字如何提示。当只选定了命令行时，作用是减少显示消息的时间范围

            menuItem = new MenuItem($"从服务器删除消息 [{selected_count}] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteMessageFromServer_Click);
            if (selected_count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("查看消息正文(&T)");
            menuItem.Click += new System.EventHandler(this.menu_displayMessageText_Click);
            if (this.dpTable_messages.SelectedRows.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.dpTable_messages, new Point(e.X, e.Y));
        }

        // 查看消息正文
        void menu_displayMessageText_Click(object sender, EventArgs e)
        {
            StringBuilder text = new StringBuilder();
            int count = this.dpTable_messages.SelectedRows.Count;
            int i = 0;
            foreach (var row in this.dpTable_messages.SelectedRows)
            {
                var message = row.Tag as MessageRecord;
                if (message == null)
                    continue;
                if (count == 0)
                    text.Append(message.data);
                else
                    text.Append($"{(i++) + 1}) {message.data}");
            }

            MessageDlg.Show(this, text.ToString(), "消息正文");
        }

        // 是否有小于集合中任何一天的情况？
        static bool IsLittleThanAny(string current_date, List<string> dates)
        {
            foreach (var date in dates)
            {
                if (string.Compare(current_date, date) < 0)
                    return true;
            }
            return false;
        }

        // 从 _startDate 中排除若干天。算法是，如果一个日期大于或者等于 _startDate，就把这个日期的后一天作为新的 _startDate
        bool DeleteFromStartDate(List<CommandLine> commands)
        {
            List<string> dates = new List<string>();

            bool changed = false;
            foreach (var command in commands)
            {
                if (string.Compare(command.Date, this._startDate) >= 0)
                {
                    this._startDate = NextDate(command.Date, out bool geted);

                    if (geted == true)
                    {
                        changed = true;

                        // 视觉上删除这一天(以及以前)的全部消息行
                        dates.Add(command.Date);
                    }
                }
            }

            List<DpRow> delete_rows = new List<DpRow>();
            foreach (var row in this.dpTable_messages.Rows)
            {
                MessageRecord message = row.Tag as MessageRecord;
                if (message != null)
                {
                    string current_date = message.publishTime.ToString("yyyy-MM-dd");
                    if (dates.IndexOf(current_date) != -1)
                        delete_rows.Add(row);
                    else
                    {
                        // 小于 dates 中任何一天的 row 也要删除
                        if (IsLittleThanAny(current_date, dates))
                            delete_rows.Add(row);
                    }
                }

                CommandLine command = row.Tag as CommandLine;
                if (command != null)
                {
                    if (dates.IndexOf(command.Date) != -1)
                        delete_rows.Add(row);
                    else
                    {
                        // 小于 dates 中任何一天的 row 也要删除
                        if (IsLittleThanAny(command.Date, dates))
                            delete_rows.Add(row);
                    }
                }
            }

            foreach (var row in delete_rows)
            {
                this.dpTable_messages.Rows.Remove(row);
            }

            return changed;
        }

        // 从 dp2mserver 服务器上删除消息
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:避免使用 Async Void 方法", Justification = "<挂起>")]
        async void menu_deleteMessageFromServer_Click(object sender, EventArgs e)
        {
            string strError = "";

            // .Tag --> DpRow
            Hashtable table = new Hashtable();

            List<CommandLine> commands = new List<CommandLine>();
            List<MessageRecord> messages = new List<MessageRecord>();
            foreach (var row in this.dpTable_messages.SelectedRows)
            {
                CommandLine command = row.Tag as CommandLine;
                if (command != null)
                    commands.Add(command);

                MessageRecord record = row.Tag as MessageRecord;
                if (record != null)
                    messages.Add(record);

                table[row.Tag] = row;
            }

            if (messages.Count > 0)
            {
                DialogResult dialog_result = MessageBox.Show(this,
    "确实要从服务器删除选定的 " + this.dpTable_messages.SelectedRows.Count.ToString() + " 条消息? \r\n\r\n(警告：删除后消息无法恢复。请谨慎操作)",
    "ChatForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (dialog_result != DialogResult.Yes)
                    return;
            }

            var changed = DeleteFromStartDate(commands);
            if (changed)
            {
                SaveStartTime(this.UserNameAndUrl, this._currentGroupName);
                // 重新装载?
                // await LoadMessageAsync(_currentGroupName, this._startDate, "", "clearAll");
            }

            var result = await DeleteMessageAsync(messages);
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                goto ERROR1;
            }

            // 把服务端口已经删除的消息从 列表中视觉上删除
            if (result.SucceedRecords != null)
            {
                foreach (var record in result.SucceedRecords)
                {
                    DpRow row = table[record] as DpRow;
                    if (row != null)
                        this.dpTable_messages.Rows.Remove(row);
                }
            }

            // 报错
            if (result.Errors != null && result.Errors.Count > 0)
            {
                List<string> errors = new List<string>();
                int i = 0;
                foreach (var error in result.Errors)
                {
                    errors.Add($"{i + 1}) {error.ErrorInfo}");
                    i++;
                }
                MessageDlg.Show(this,
                    $"删除过程出错({result.Errors.Count}):\r\n" +
                    StringUtil.MakePathList(errors, "\r\n"),
                    "删除过程出错");
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public class DeleteMessageResult : NormalResult
        {
            // 出错信息
            public List<ErrorResult> Errors { get; set; }
            // 成功信息
            public List<MessageRecord> SucceedRecords { get; set; }
        }

        public class ErrorResult : NormalResult
        {
            public MessageRecord Message { get; set; }
        }

        async Task<DeleteMessageResult> DeleteMessageAsync(List<MessageRecord> messages)
        {
            var get_result = await ConnectionPool.OpenConnectionAsync(this.UserNameAndUrl);
            if (get_result.Value == -1)
                return new DeleteMessageResult
                {
                    Value = -1,
                    ErrorInfo = get_result.ErrorInfo
                };
            P2PConnection connection = get_result.Connection;

            List<ErrorResult> errors = new List<ErrorResult>();
            List<MessageRecord> succeeds = new List<MessageRecord>();

            // 只复制必要的字段。不影响原来 messages 集合中的元素
            foreach (var message in messages)
            {
                List<MessageRecord> records = new List<MessageRecord>();
                records.Add(new MessageRecord
                {
                    id = message.id,
                    groups = message.groups
                });

                SetMessageRequest request = new SetMessageRequest("delete", // expire
    "", // dontNotifyMe
    records);
                SetMessageResult result = await connection.SetMessageAsyncLite(request);
                if (result.Value == -1)
                    errors.Add(new ErrorResult
                    {
                        Value = (int)result.Value,
                        ErrorInfo = result.ErrorInfo,
                        ErrorCode = result.String,
                        Message = message
                    });
                else
                    succeeds.Add(message);
            }

            return new DeleteMessageResult
            {
                Errors = errors,
                SucceedRecords = succeeds
            };
        }


        // 在消息行上双击。如果是命令行，则执行命令
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:避免使用 Async Void 方法", Justification = "<挂起>")]
        private async void dpTable_messages_DoubleClick(object sender, EventArgs e)
        {
            if (this.dpTable_messages.SelectedRows.Count == 1)
            {
                await DoCommandAsync(this.dpTable_messages.SelectedRows[0]);
            }
        }

        async Task DoCommandAsync(DpRow row)
        {
            CommandLine line = row.Tag as CommandLine;
            if (line == null)
                return;

            // 在第一个命令行上双击，可以向前扩展装载一天的消息
            if (this.dpTable_messages.Rows.IndexOf(row) == 0)
            {
                this._startDate = PrevDate(line.Date);
                this.SaveStartTime(this.UserNameAndUrl, _currentGroupName);

                await LoadMessageAsync(_currentGroupName, this._startDate, line.Date, "insertBefore");

                // 清除以前的选择
                foreach (var c in this.dpTable_messages.SelectedRows)
                {
                    c.Selected = false;
                }
                // 选择更新后的第一行
                this.dpTable_messages.Rows[0].Selected = true;
                // this.dpTable_messages.ClearAllSelections();
            }
        }

        static string PrevDate(string strDate)
        {
            DateTime time = DateTime.Parse(strDate);
            return time.Subtract(TimeSpan.FromDays(1)).ToString("yyyy-MM-dd");
        }

        static string NextDate(string strDate, out bool changed)
        {
            DateTime time = DateTime.Parse(strDate);
            DateTime next = time.Add(TimeSpan.FromDays(1));
            // 检查一下，不让超过当前时间
            if (next > DateTime.Now)
            {
                changed = false;
                return strDate;
            }
            changed = true;
            return next.ToString("yyyy-MM-dd");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:避免使用 Async Void 方法", Justification = "<挂起>")]
        private async void button_send_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_input.Text))
            {
                MessageBox.Show(this, "尚未输入文字");
                return;
            }
            var result = await SendMessage(_currentGroupName, this.textBox_input.Text);
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                goto ERROR1;
            }

            // 调用成功后才把输入的文字清除
            this.Invoke((Action)(() => this.textBox_input.Text = ""
                ));
            return;
        ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
        }

        async Task<NormalResult> SendMessage(string strGroupName,
            string strText)
        {
            this.EnableControls(false);

            try
            {
                /*
                var get_result = await ConnectionPool.OpenConnectionAsync(this.UserNameAndUrl);
                if (get_result.Value == -1)
                {
                    return get_result;
                }
                P2PConnection connection = get_result.Connection;
                */
                P2PConnection connection = await ConnectionPool.GetConnectionAsync(this.UserNameAndUrl);


                List<MessageRecord> messages = new List<MessageRecord>();
                MessageRecord record = new MessageRecord();
                record.groups = strGroupName.Split(new char[] { ',' });   // new string[1] { strGroupName };
                record.data = strText;
                messages.Add(record);

                SetMessageRequest param = new SetMessageRequest("create",
                    "",
                   messages);

                var result = await connection.SetMessageAsyncLite(param);
                if (result.Value == -1)
                {
                    if (result.String == "_connectionNotFound")
                    {
                        // TODO: 尝试重新发送消息一次
                    }

                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = result.ErrorInfo
                    };
                }
                return new NormalResult();
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message
                };
            }
            finally
            {
                this.EnableControls(true);
            }
        }

        void EnableControls(bool bEnable)
        {
            this.Invoke((Action)(() =>
            {
                this.button_send.Enabled = bEnable;
                this.textBox_input.Enabled = bEnable;
            }));
        }

        // 当前窗口用过的 P2PConnection 对象
        List<P2PConnection> _usedConnections = new List<P2PConnection>();

        // 为当前 this.UserNameAndUrl 挂接 AddMessage 事件
        async Task AddEventAsync()
        {
            // 挂接事件
            var get_result = await ConnectionPool.OpenConnectionAsync(this.UserNameAndUrl);
            if (get_result.Value == -1)
                return;
            P2PConnection connection = get_result.Connection;

            // 记忆
            if (_usedConnections.IndexOf(connection) == -1)
                _usedConnections.Add(connection);

            connection.AddMessage -= Connection_AddMessage;
            connection.AddMessage += Connection_AddMessage;
        }

        // 2020/9/20
        // 解挂所有 AddMessage 事件
        void RemoveEvents()
        {
            if (_usedConnections == null)
                return;

            foreach (var connection in _usedConnections)
            {
                connection.AddMessage -= Connection_AddMessage;
            }

            _usedConnections.Clear();
        }

        GroupInfo Find(string[] groups)
        {
            string group_name = BuildName(groups);

            // 在当前所有群中找 GroupInfo
            foreach (var row in this.dpTable_groups.Rows)
            {
                var info = row.Tag as GroupInfo;
                if (group_name == info.DisplayName || group_name == info.EchoName)
                {
                    return info;
                }
            }
            return null;
        }

        // 用于拼接 data 的缓冲区
        StringBuilder _messageData = new StringBuilder();

        private void Connection_AddMessage(object sender, AddMessageEventArgs e)
        {
            P2PConnection connection = sender as P2PConnection;

            if (e.Records != null && e.Action == "create")
            {
                // TODO: 要 if e.Action; 更新的 action 要更新已经显示的行的内容，删除的 action 要兑现删除效果
                foreach (var record in e.Records)
                {
                    // ClientInfo.WriteInfoLog($"Connection_AddMessage() 收到消息 {record.ToString()}");
                    FormClientInfo.Speak("有新消息");

                    UpdateGroupNameList(record.groups);

                    // TODO: 要建立一个便于搜索的名字对照表
                    // 忽略不是当前群组的消息
                    var group_info = Find(record.groups);
                    if (group_info == null || group_info.DisplayName != _currentGroupName)
                        continue;
                    /*
                    if (GroupNameContains(record.groups, _currentGroupName) == false)
                        continue;
                    */

                    if (string.IsNullOrEmpty(record.id)
                        && _messageData.Length < MAX_MESSAGE_DATA_LENGTH)
                    {
                        _messageData.Append(record.data);
                        continue;
                    }

                    if (_messageData.Length > 0)
                    {
                        record.data = _messageData.ToString() + record.data;
                        _messageData.Clear();
                    }

                    AddMessageLine(_currentIndex == -1 ? -1 : _currentIndex++, record);
                }
            }
        }

        bool _bVertBottom = false;

        private void dpTable_messages_ScrollBarTouched(object sender, ScrollBarTouchedArgs e)
        {
            /*
            if (e.Action == "VertBottom")
                _bVertBottom = true;
            else
                _bVertBottom = false;
                */

            if (e.Action == "VertEndScroll")
            {
                API.ScrollInfoStruct si = new API.ScrollInfoStruct();

                si.cbSize = Marshal.SizeOf(si);
                si.fMask = API.SIF_RANGE | API.SIF_POS | API.SIF_PAGE;

                API.GetScrollInfo(this.dpTable_messages.Handle, API.SB_VERT, ref si);
                if (si.nPos + si.nPage >= si.nMax - 1)
                    _bVertBottom = true;
                else
                    _bVertBottom = false;
            }

            // DislayProgress($"e.Position={e.Action},_bVertBottom:{_bVertBottom}");
        }

        // 2020/9/30
        // 测试 Clear Connection
        private async void ToolStripMenuItem_clearConnection_Click(object sender, EventArgs e)
        {
            P2PConnection connection = await ConnectionPool.GetConnectionAsync(this.UserNameAndUrl);
            GetConnectionInfoRequest request = new GetConnectionInfoRequest
            {
                QueryWord = "!myself",
                Operation = "clear"
            };

            var result = await connection.GetConnectionInfoAsync(request,
                TimeSpan.FromSeconds(30),
                new CancellationToken());
            string text = $"result.ResultCount={result.ResultCount},result.ErrorInfo={result.ErrorInfo}";
            MessageBox.Show(this, text);
        }


#if OLD_VERSION
        async Task<SetMessageResult> DeleteMessageAsync(List<MessageRecord> messages)
        {
            List<MessageRecord> records = new List<MessageRecord>();
            // 只复制必要的字段。不影响原来 messages 集合中的元素
            foreach (var message in messages)
            {
                records.Add(new MessageRecord
                {
                    id = message.id,
                    groups = message.groups
                });
            }
            SetMessageRequest request = new SetMessageRequest("delete", // expire
                "", // dontNotifyMe
                records);

            var get_result = await ConnectionPool.GetConnectiontAsync(this.UserNameAndUrl);
            if (get_result.Value == -1)
                return new SetMessageResult
                {
                    Value = -1,
                    ErrorInfo = get_result.ErrorInfo
                };
            P2PConnection connection = get_result.Connection;

            SetMessageResult result = await connection.SetMessageAsyncLite(request);
            return result;
        }
#endif
    }
}
