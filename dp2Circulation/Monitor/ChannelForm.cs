using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 通道管理窗
    /// </summary>
    public partial class ChannelForm : MyForm
    {
        ListHistory<QueryState> _history = new ListHistory<QueryState>();

        const int COLUMN_IP = 0;
        const int COLUMN_VIA = 1;
        const int COLUMN_COUNT = 2;
        const int COLUMN_USERNAME = 3;
        const int COLUMN_LOCATION = 4;
        const int COLUMN_CALLCOUNT = 5;
        const int COLUMN_LIBRARYCODE = 6;
        const int COLUMN_LANG = 7;
        const int COLUMN_SESSIONID = 8;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ChannelForm()
        {
            InitializeComponent();
        }

        private void ChannelForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

            string strWidths = this.MainForm.AppInfo.GetString(
"channel_form",
"browse_list_column_width",
"");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_channel,
                    strWidths,
                    true);
            }

            this._history.StateChanged -= new StateChangedEventHandler(_history_StateChanged);
            this._history.StateChanged += new StateChangedEventHandler(_history_StateChanged);

            RefreshBackForwardButtons();
        }

        void _history_StateChanged(object sender, StateChangedEventArgs e)
        {
            this._queryState = (e.State as QueryState).Clone();
            this.label_channel_message.Text = this._queryState.Message;
        }

        private void ChannelForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ChannelForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_channel);
                this.MainForm.AppInfo.SetString(
                    "channel_form",
                    "browse_list_column_width",
                    strWidths);
            }
        }

        int DoSearch(
            string strUserName,
            string strIP,
            string strStyle,
            bool bRefresh,
            out string strError)
        {
            strError = "";
            //int nRet = 0;

            bool bIpCount = (strStyle == "ip-count");

            string strQuery = "";

            Hashtable table = new Hashtable();
            table["ip"] = strIP;
            table["username"] = strUserName;

            strQuery = StringUtil.BuildParameterString(table);

            // 确保延迟刷新被兑现
            this._history.EnsureAdd(this.listView_channel, this._queryState.Clone());    // 如果有必要则加入

            this.listView_channel.Items.Clear();

            EnableControls(false);

            LibraryChannel channel = this.GetChannel();

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得服务器通道信息 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            this.listView_channel.BeginUpdate();
            try
            {
                int nStart = 0;
                for (; ; )
                {
                    ChannelInfo[] contents = null;

                    long lRet = channel.GetChannelInfo(
                        this.stop,
                        strQuery,
                        strStyle,
                        nStart,
                        -1,
                        out contents,
                        out strError);

                    if (lRet == -1)
                        goto ERROR1;
                    if (lRet == 0)
                    {
                        strError = "不存在用户信息。";
                        goto ERROR1;   // not found
                    }

                    Debug.Assert(contents != null, "");

                    foreach (ChannelInfo info in contents)
                    {
                        ListViewItem item = new ListViewItem();
                        ListViewUtil.ChangeItemText(item, COLUMN_IP, AlignIpString(info.ClientIP));
                        ListViewUtil.ChangeItemText(item, COLUMN_VIA, info.Via);
                        ListViewUtil.ChangeItemText(item, COLUMN_USERNAME, info.UserName);
                        ListViewUtil.ChangeItemText(item, COLUMN_LIBRARYCODE, info.LibraryCode);

                        if (bIpCount == true)
                            ListViewUtil.ChangeItemText(item, COLUMN_COUNT, info.Count.ToString());

                        if (bIpCount == false)
                            ListViewUtil.ChangeItemText(item, COLUMN_CALLCOUNT, info.CallCount.ToString());

                        ListViewUtil.ChangeItemText(item, COLUMN_SESSIONID, info.SessionID);
                        ListViewUtil.ChangeItemText(item, COLUMN_LOCATION, info.Location);
                        ListViewUtil.ChangeItemText(item, COLUMN_LANG, info.Lang);

                        item.SubItems[0].Font = new Font("Courier New", this.Font.Size);

                        this.listView_channel.Items.Add(item);
                    }

                    nStart += contents.Length;
                    if (nStart >= lRet)
                        break;
                }

                this.label_channel_message.Text = this.listView_channel.Items.Count.ToString();

                this._queryState.Query = strQuery;
                this._queryState.Style = strStyle;
                this._queryState.Message = this.label_channel_message.Text;

                // 保存当前状态
                if (bRefresh == true)
                    this._history.Refresh(this.listView_channel, this._queryState.Clone(), true);    // 滞后刷新
                else
                    this._history.Add(this.listView_channel, this._queryState.Clone(), true);    // 滞后加入

#if NO
                if (bIpCount == true)
                    this._bCountMode = true;
                else
                    this._bCountMode = false;
#endif

            }
            finally
            {
                this.listView_channel.EndUpdate();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                this.ReturnChannel(channel);

                EnableControls(true);
            }

            RefreshBackForwardButtons();
            return 0;
        ERROR1:
            return -1;
        }

        // 强制关闭服务器通道
        // parameters:
        //      strIdType   为 ip/sessionid 之一
        // return:
        //      -1  出错
        //      >=0 实际关闭的通道数  
        int CloseChannels(
            string strIdType,
            List<string> sessionids,
            out string strError)
        {
            strError = "";

            int nCount = 0;
            EnableControls(false);

            LibraryChannel channel = this.GetChannel();

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在关闭指定的通道 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                List<List<string>> batchs = new List<List<string>>();
                List<string> batch = new List<string>();
                foreach (string id in sessionids)
                {
                    batch.Add(id);
                    if (batch.Count >= 100)
                    {
                        batchs.Add(batch);
                        batch = new List<string>();
                    }
                }
                if (batch.Count > 0)
                {
                    batchs.Add(batch);
                }

                foreach (List<string> one in batchs)
                {
                    ChannelInfo[] requests = new ChannelInfo[one.Count];
                    for(int i = 0;i<requests.Length;i++)
                    {
                        ChannelInfo info = new ChannelInfo();
                        if (strIdType == "sessionid")
                            info.SessionID = one[i];
                        else if (strIdType == "ip")
                            info.ClientIP = one[i];
                        else
                        {
                            strError = "未知的 strIdType 类型 '"+strIdType+"'";
                            return -1;
                        }
                        requests[i] = info;
                    }

                    ChannelInfo[] results = null;
                    long lRet = channel.ManageChannel(
this.stop,
"close",
"",
requests,
out results,
out strError);
                    if (lRet == -1)
                        return -1;
                    nCount += (int)lRet;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                this.ReturnChannel(channel);

                EnableControls(true);
            }

            return nCount;
        }

        // 规整 IP 字符串
        static string AlignIpString(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";
            StringBuilder text = new StringBuilder(20);
            string [] parts = strText.Split(new char[] {'.'});
            foreach (string s in parts)
            {
                if (text.Length > 0)
                    text.Append(".");
                text.Append(s.PadLeft(3, ' '));
            }

            return text.ToString();
        }

        // 去掉字符串里面的空格
        static string PureIpString(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            StringBuilder text = new StringBuilder(20);
            foreach (char c in strText)
            {
                if (c != ' ')
                    text.Append(c);
            }

            return text.ToString();
        }

        void RefreshBackForwardButtons()
        {
            this.toolStripButton_prevQuery.Enabled = this._history.CanBack();

            this.toolStripButton_nextQuery.Enabled = this._history.CanForward();
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.toolStrip_channel.Enabled = bEnable;
        }

        QueryState _queryState = new QueryState();

        private void toolStripButton_channel_prevQuery_Click(object sender, EventArgs e)
        {
            this._history.Back(this.listView_channel, this._queryState.Clone());
            RefreshBackForwardButtons();
        }

        private void toolStripButton_channel_nextQuery_Click(object sender, EventArgs e)
        {
            this._history.Forward(this.listView_channel, this._queryState.Clone());
            RefreshBackForwardButtons();
        }

        // bool _bCountMode = false;

        private void listView_channel_DoubleClick(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            if (this._queryState.IsCountMode() == true)
            {
                if (this.listView_channel.SelectedItems.Count == 0)
                {
                    strError = "尚未选定要详细察看的事项";
                    goto ERROR1;
                }
                // 获得双击的 IP
                string strIP = PureIpString(ListViewUtil.GetItemText(this.listView_channel.SelectedItems[0], COLUMN_IP));

                // 检索详细风格
                nRet = DoSearch(
    "", // this.toolStripTextBox_UserName.Text,
    strIP,
    "",
    false,
    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 参与排序的列号数组
        internal SortColumns SortColumns = new SortColumns();

        private void listView_channel_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            if (nClickColumn == COLUMN_COUNT
                || nClickColumn == COLUMN_CALLCOUNT)
                sortStyle = ColumnSortStyle.RightAlign;
            else if (nClickColumn == COLUMN_IP)
                sortStyle = ColumnSortStyle.IpAddress;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_channel.Columns,
                true);

            // 排序
            this.listView_channel.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            // this.listView1.ListViewItemSorter = null;
        }

        // 刷新
        private void toolStripButton_channel_refresh_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this._queryState == null)
                this._queryState = new QueryState();

            Hashtable table = StringUtil.ParseParameters(this._queryState.Query);

            nRet = DoSearch(
                (string)table["username"],
                (string)table["ip"],
                this._queryState.Style,
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 概览
        private void toolStripButton_channel_count_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this._history.Clear();

            string strStyle = "ip-count";

            nRet = DoSearch(
                this.toolStripTextBox_UserName.Text,
                this.toolStripTextBox_IP.Text,
                strStyle,
                false,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 详情
        private void toolStripButton_channel_detail_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this._history.Clear();

            string strStyle = "";

            nRet = DoSearch(
                this.toolStripTextBox_UserName.Text,
                this.toolStripTextBox_IP.Text,
                strStyle,
                false,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_channel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_channel_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("关闭通道 [" + this.listView_channel.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_channel_closeSelectedChannels_Click);
            if (this.listView_channel.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_channel, new Point(e.X, e.Y));		
        }

        void menu_channel_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllLines(this.listView_channel);
        }

        void menu_channel_closeSelectedChannels_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_channel.SelectedItems.Count == 0)
            {
                strError = "尚未选定要关闭的通道";
                goto ERROR1;
            }

            int nSubCount = 0;  // 统计每个 IP 对应的从属的通道的总数
            List<string> sessionids = new List<string>();
            foreach(ListViewItem item in this.listView_channel.SelectedItems)
            {
                if (this._queryState.IsCountMode() == false)
                {
                    string strSessionID = ListViewUtil.GetItemText(item, COLUMN_SESSIONID);
                    if (string.IsNullOrEmpty(strSessionID) == true)
                        continue;
                    sessionids.Add(strSessionID);
                }
                else
                {
                    string strIP = PureIpString(ListViewUtil.GetItemText(item, COLUMN_IP));
                    if (string.IsNullOrEmpty(strIP) == true)
                        continue;
                    sessionids.Add(strIP);

                    int count = 0;
                    string strCount = ListViewUtil.GetItemText(item, COLUMN_COUNT);
                    bool bRet = Int32.TryParse(strCount, out count);
                    Debug.Assert(bRet == true, "");

                    nSubCount += count;
                }
            }

            if (sessionids.Count == 0)
            {
                strError = "选定的事项中没有任何一项具有 会话 ID，无法进行关闭通道的操作";
                goto ERROR1;
            }

            string strText = "";
            if (this._queryState.IsCountMode() == true)
            {
                strText = "确实要关闭选定的 " + nSubCount.ToString() + " 个服务器通道?\r\n\r\n注意：关闭服务器通道会给正在进行的前端操作带来一定影响";
            }
            else
                strText = "确实要关闭选定的 " + sessionids.Count.ToString() + " 个服务器通道?\r\n\r\n注意：关闭服务器通道会给正在进行的前端操作带来一定影响";

            DialogResult result = MessageBox.Show(this,
strText,
"ChannelForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            // 强制关闭服务器通道
            // return:
            //      -1  出错
            //      >=0 实际关闭的通道数  
            int nRet = CloseChannels(
                this._queryState.IsCountMode() == true ? "ip" : "sessionid",
                sessionids,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "成功关闭 "+nRet.ToString()+" 个通道。显示未刷新");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

    }

    // 列表的历史
    class ListHistory<T> : List<ListItems<T>>
    {
        public event StateChangedEventHandler StateChanged = null;

        // 当前位置
        // Index == -1, Count == 0 表示最初状态，两个按钮都不可用
        // Back 时提取 Index - 1位置；Forward 时提取 Index + 1 位置
        public int Index = -1;

        public new void Clear()
        {
            base.Clear();

            this.Index = -1;
        }

        // 看看是否已经为滞后，需要补充加入
        public void EnsureAdd(ListView list, T state)
        {
            if (this.Index < 0 || this.Index >= this.Count)
                return;

#if NO
            ListItems<T> items = this[this.Index];

            if (items.Delay == true)
            {
                // 滞后加入
                items.State = state;
                items.Clear();
                foreach (ListViewItem item in list.Items)
                {
                    items.Add(item);
                }
                items.Delay = false;
            }
#endif
            DelaySet(list, state, this.Index);
        }

        // 刷新当前记忆
        public void Refresh(ListView list,
            T state,
            bool bDelay = false)
        {
#if NO
            if (this.Index + 1 < this.Count)
            {
                ListItems<T> items = this[this.Index + 1];

                items.State = state;

                if (bDelay == false)
                {
                    items.Clear();
                    foreach (ListViewItem item in list.Items)
                    {
                        items.Add(item);
                    }
                }
                else
                {
                    // 即便以前有内容，也要清除，以便以后可以延迟装载
                    items.Clear();
                    items.Delay = true;
                }
            }
#endif
            if (this.Index < this.Count && this.Index != -1)
            {
                ListItems<T> items = this[this.Index];

                items.State = state;

                if (bDelay == false)
                {
                    items.Clear();
                    foreach (ListViewItem item in list.Items)
                    {
                        items.Add(item);
                    }
                }
                else
                {
                    // 即便以前有内容，也要清除，以便以后可以延迟装载
                    items.Clear();
                    items.Delay = true;
                }
            }
        }

        // 在末尾增加一个记忆
        public void Add(ListView list,
            T state,
            bool bDelay = false)
        {
            while (this.Count > this.Index + 1)
                this.RemoveAt(this.Index + 1);

            ListItems<T> items = new ListItems<T>();
            items.State = state;
            if (bDelay == false)
            {
                foreach (ListViewItem item in list.Items)
                {
                    items.Add(item);
                }
            }
            else
            {
                items.Delay = true;
            }
            this.Add(items);

            this.Index++;
        }

        void DelaySet(ListView list, T state, int index)
        {
            // 滞后加入当前事项
            if (index < this.Count)
            {
                ListItems<T> items = this[index];
                if (items.Delay == true)
                {
                    // 滞后加入
                    items.State = state;
                    items.Clear();
                    foreach (ListViewItem item in list.Items)
                    {
                        items.Add(item);
                    }
                    items.Delay = false;
                }
            }
        }

        // 将 index - 1 位置的内容提取出来，当前位置改到 index - 1
        public void Back(ListView list, T state)
        {
            if (this.Index <= 0)
                return;

            DelaySet(list, state, this.Index);

            list.Items.Clear();
            this.Index--;
            {
                ListItems<T> items = this[this.Index];
                foreach (ListViewItem item in items)
                {
                    list.Items.Add(item);
                }

                if (this.StateChanged != null)
                {
                    StateChangedEventArgs e = new StateChangedEventArgs();
                    e.State = items.State;
                    this.StateChanged(this, e);
                }
            }
        }

        // 将 index + 1 位置的内容提取出来，当前位置改到 index + 1
        public void Forward(ListView list, T state)
        {
            if (this.Index + 1 >= this.Count)
                return;

            DelaySet(list, state, this.Index);

            list.Items.Clear();
            this.Index++;
            ListItems<T> items = this[this.Index];
            foreach (ListViewItem item in items)
            {
                list.Items.Add(item);
            }

            if (this.StateChanged != null)
            {
                StateChangedEventArgs e = new StateChangedEventArgs();
                e.State = items.State;
                this.StateChanged(this, e);
            }
        }

        public bool CanForward()
        {
            if (this.Index + 1 >= this.Count)
                return false;
            return true;
        }

        public bool CanBack()
        {
            if (this.Index > 0 /* && this.Count > 0*/)
                return true;
            return false;
        }
    }

    // 保存一组 ListViewItem
    class ListItems<T> : List<ListViewItem>
    {
        public T State = default(T); // 要连带保存的状态
        public bool Delay = false;  // 是否延迟记忆？ == true 的时候，表示 集合里面实际是空的，需要增补记忆
    }

    class QueryState
    {
        // public bool CountMode = false;

        public string Query = "";
        public string Style = "";
        public string Message = "";

        public bool IsCountMode()
        {
            if (Style == "ip-count")
                return true;
            return false;
        }

        public QueryState Clone()
        {
            QueryState new_state = new QueryState();
            new_state.Query = this.Query;
            new_state.Style = this.Style;
            new_state.Message = this.Message;

            return new_state;
        }
    }

    /// <summary>
    /// 状态改变事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void StateChangedEventHandler(object sender,
        StateChangedEventArgs e);

    /// <summary>
    /// 状态改变事件的参数
    /// </summary>
    public class StateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 状态对象
        /// </summary>
        public object State = null;
    }
}
