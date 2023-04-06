using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Reflection;
using System.Xml;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using System.Runtime.Remoting;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.Script;
using DigitalPlatform.Marc;
using DigitalPlatform.CommonControl;
using DigitalPlatform.MarcDom;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Interfaces;
using DigitalPlatform.RFID;
using DigitalPlatform.Core;
// using DocumentFormat.OpenXml.Wordprocessing;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 通用的 MDI 子窗口基类。提供了通讯通道和窗口尺寸维持等通用设施
    /// </summary>
    public class MyForm : Form, IMdiWindow, ILoopingHost, IChannelHost, IEnableControl, IChannelLooping
    {
        #region test


#if SUPPORT_OLD_STOP
        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = new LibraryChannel();

        public DigitalPlatform.Stop _stop = null;

        /// <summary>
        /// 进度条和停止按钮
        /// </summary>
        public Stop Progress
        {
            get
            {
                return this._stop;
            }
        }
#endif

        public int _processing = 0;   // 是否正在进行处理中

        public string FormName
        {
            get
            {
                return this.GetType().ToString();
            }
        }

        public string FormCaption
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public System.Threading.CancellationTokenSource _cancel = new System.Threading.CancellationTokenSource();

        public System.Threading.CancellationToken CancelToken
        {
            get
            {
                return _cancel.Token;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_cancel != null)
                    {
                        if (_cancel.IsCancellationRequested == false)
                            _cancel?.Cancel();
                        _cancel?.Dispose();
                        _cancel = null;
                    }
                }
                catch (ObjectDisposedException) // 2021/12/30
                {
                }

#if SUPPORT_OLD_STOP
                if (this.Channel != null)
                    this.Channel.Dispose();

                // 2017/4/24
                if (_stop != null) // 脱离关联
                {
                    _stop.Unregister();	// 和容器关联
                    _stop = null;
                }
#endif
                CloseFloatingMessage();
            }

            base.Dispose(disposing);
        }

        public FloatingMessageForm _floatingMessage = null;

        public FloatingMessageForm FloatingMessageForm
        {
            get
            {
                return this._floatingMessage;
            }
            set
            {
                this._floatingMessage = value;
            }
        }

        public void CloseFloatingMessage()
        {
            if (_floatingMessage != null)
            {
                _floatingMessage.Close();
                _floatingMessage.Dispose();
                _floatingMessage = null;
            }
        }

        /// <summary>
        /// 窗口是否为浮动状态
        /// </summary>
        public virtual bool Floating
        {
            get;
            set;
        }

        /// <summary>
        /// 窗口是否为固定窗口。所谓固定窗口就是固定在某一侧的窗口
        /// </summary>
        public virtual bool Fixed
        {
            get;
            set;
        }

        /// <summary>
        /// 界面语言
        /// </summary>
        public string Lang = "zh";


        internal Timer _timer = null;

        public virtual void OnSelectedIndexChanged()
        {
        }

        public void TriggerSelectedIndexChanged()
        {
            if (this._timer == null)
            {
                this._timer = new Timer();
                this._timer.Interval = 500;
                this._timer.Tick += new EventHandler(_timer_Tick);
            }
            this._timer.Start();
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            this._timer.Stop();
            OnSelectedIndexChanged();
        }

        public void ShowMessageBox(string strText)
        {
            if (this.IsHandleCreated)
                this.Invoke((Action)(() =>
                {
                    try
                    {
                        MessageBox.Show(this, strText);
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                }));
        }

        #region looping

#if REMOVED
        bool _isActive = false;

        protected override void OnActivated(EventArgs e)
        {
            _isActive = true;
            base.OnActivated(e);
        }


        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            _isActive = false;
        }
#endif

#if OLD
        List<Looping> _loopings = new List<Looping>();
        object _syncRoot_loopings = new object();

        public Looping BeginLoop(StopEventHandler handler,
            string text,
            string style = null)
        {
            var looping = new Looping(handler, text/*, _isActive*/);
            lock (_syncRoot_loopings)
            {
                _loopings.Add(looping);
            }

            // 2022/10/29
            if (style != null)
            {
                if (StringUtil.IsInList("halfstop", style) == true)
                    looping.stop.Style = StopStyle.EnableHalfStop;
            }

            return looping;
        }

        public void EndLoop(Looping looping)
        {
            lock (_syncRoot_loopings)
            {
                _loopings.Remove(looping);
            }
            looping.Dispose();
        }

        public bool HasLooping()
        {
            lock (_syncRoot_loopings)
            {
                foreach (var looping in _loopings)
                {
                    if (looping.stop != null && looping.stop.State == 0)
                        return true;
                }
                return false;
            }
        }

        public Looping TopLooping
        {
            get
            {
                lock (_syncRoot_loopings)
                {
                    if (_loopings.Count == 0)
                        return null;
                    return (_loopings[_loopings.Count - 1]);
                }
            }
        }

#endif

        // 三种动作: GetChannel() BeginLoop() 和 EnableControl()
        // parameters:
        //          style 可以有如下子参数:
        //              disableControl
        //              timeout:hh:mm:ss 确保超时参数在 hh:mm:ss 以长
        // https://learn.microsoft.com/en-us/dotnet/api/system.timespan.parse?view=net-6.0
        // [ws][-]{ d | [d.]hh:mm[:ss[.ff]] }[ws]
        public Looping Looping(
            out LibraryChannel channel,
            string text = "",
            string style = null,
            StopEventHandler handler = null)
        {
            var controlDisabled = StringUtil.IsInList("disableControl", style);
            var timeout_string = StringUtil.GetParameterByPrefix(style, "timeout"); // 不小于这么多
            var settimeout_string = StringUtil.GetParameterByPrefix(style, "settimeout");   // 设置为这么多

            var serverUrl = StringUtil.GetParameterByPrefix(style, "serverUrl");
            if (string.IsNullOrEmpty(serverUrl) == false)
                serverUrl = StringUtil.UnescapeString(serverUrl);
            var userName = StringUtil.GetParameterByPrefix(style, "userName");

            channel = this.GetChannel(serverUrl, userName);

            var old_timeout = channel.Timeout;
            bool timeout_changed = false;
            if (string.IsNullOrEmpty(timeout_string) == false)
            {
                var new_timeout = TimeSpan.Parse(timeout_string);
                if (new_timeout > old_timeout)
                {
                    channel.Timeout = new_timeout;
                    timeout_changed = true;
                }
            }
            if (string.IsNullOrEmpty(settimeout_string) == false)
            {
                var new_timeout = TimeSpan.Parse(settimeout_string);
                if (new_timeout != old_timeout)
                {
                    channel.Timeout = new_timeout;
                    timeout_changed = true;
                }
            }

            var looping = _loopingHost.BeginLoop(
                handler == null ? this.DoStop : handler,
                text,
                style);

            if (controlDisabled)
                this.EnableControls(false);

            var channel_param = channel;
            looping.Closed = () =>
            {
                if (controlDisabled)
                    this.EnableControls(true);
                if (timeout_changed)
                    channel_param.Timeout = old_timeout;
                this.ReturnChannel(channel_param);
            };

            return looping;
        }

        // 两种动作: BeginLoop() 和 EnableControl()
        public Looping Looping(string text,
            string style = null,
            StopEventHandler handler = null)
        {
            var controlDisabled = StringUtil.IsInList("disableControl", style);

            var looping = _loopingHost.BeginLoop(
                handler == null ? this.DoStop : handler,
                text,
                style);

            if (controlDisabled)
                this.EnableControls(false);

            looping.Closed = () =>
            {
                if (controlDisabled)
                    this.EnableControls(true);
            };

            return looping;
        }


        internal LoopingHost _loopingHost = new LoopingHost();

        public Looping BeginLoop(StopEventHandler handler,
string text,
string style = null)
        {
            return _loopingHost.BeginLoop(handler, text, style);
        }

        public void EndLoop(Looping looping)
        {
            _loopingHost.EndLoop(looping);
        }

        public bool HasLooping()
        {
            return _loopingHost.HasLooping();
        }

        public Looping TopLooping
        {
            get
            {
                return _loopingHost.TopLooping;
            }
        }

        #endregion

        #endregion // of test


        /// <summary>
        /// 供 ShowDialog(?) 使用的窗口对象
        /// </summary>
        public IWin32Window SafeWindow
        {
            get
            {
                if (this.Visible == false)
                    return Program.MainForm;
                return this;
            }
        }



#if NO
        MainForm m_mainForm = null;

        /// <summary>
        /// 当前窗口所从属的框架窗口
        /// </summary>
        public virtual MainForm MainForm
        {
            get
            {
                if (this.MdiParent != null)
                    return (MainForm)this.MdiParent;
                return m_mainForm;
            }
            set
            {
                // 为了让脚本代码能兼容
                this.m_mainForm = value;
            }
        }
#endif
        /// <summary>
        /// 当前窗口所从属的框架窗口
        /// </summary>
        public virtual MainForm MainForm
        {
            get
            {
                return Program.MainForm;
            }
            set
            {
                // 为了让脚本代码能兼容
            }
        }

        public string GetGroupName()
        {
            return $"{this.GetType().ToString()}_{this.GetHashCode().ToString()}";
        }

        /// <summary>
        /// 窗口 Load 时被触发
        /// </summary>
        public virtual void OnMyFormLoad()
        {
            if (Program.MainForm == null)
                return;

            this.HelpRequested -= MyForm_HelpRequested;
            this.HelpRequested += MyForm_HelpRequested;

#if SUPPORT_OLD_STOP
            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            this.Channel.Idle -= Channel_Idle;
            this.Channel.Idle += Channel_Idle;
#endif
            if (this.UseLooping == true)
            {
                this._loopingHost.GroupName = this.GetGroupName();
                // 默认使用 MainForm 的 StopManager
                this._loopingHost.StopManager = Program.MainForm.stopManager;
                this._loopingHost.StopManager.CreateGroup(this.GetGroupName());
            }
            else
            {
#if SUPPORT_OLD_STOP
                _stop = new DigitalPlatform.Stop();
                _stop.Register(Program.MainForm?.stopManager, true);    // 和容器关联
#endif
            }

            {
                _floatingMessage = new FloatingMessageForm(this, true);
                // _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);

                // _floatingMessage.Text = "test";
                //_floatingMessage.Clicked += _floatingMessage_Clicked;
                if (Program.MainForm != null)
                    Program.MainForm.Move += new EventHandler(MainForm_Move);
            }
        }

        private void MyForm_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            OnHelpTriggered();
        }

#if SUPPORT_OLD_STOP
        bool _channelDoEvents = true;
        public bool ChannelDoEvents
        {
            get
            {
                return _channelDoEvents;
            }
            set
            {
                _channelDoEvents = value;
            }
        }

        void Channel_Idle(object sender, IdleEventArgs e)
        {
            if (_channelDoEvents)
                Application.DoEvents();
        }
#endif

        /// <summary>
        /// 窗口 Closing 时被触发
        /// </summary>
        /// <param name="e">事件参数</param>
        public virtual void OnMyFormClosing(FormClosingEventArgs e)
        {
            if (UseLooping)
            {
                if (HasLooping() || _processing > 0)
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。或等待长时操作完成");
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                throw new Exception("MyForm 不再支持 UseLooping == false");
#if SUPPORT_OLD_STOP
                if ((_stop != null && _stop.State == 0)    // 0 表示正在处理
                    || this._processing > 0)
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
#endif
            }
        }

        // 在 base.OnFormClosed(e); 之前调用
        /// <summary>
        /// 窗口 Closed 时被触发。在 base.OnFormClosed(e) 之前被调用
        /// </summary>
        public virtual void OnMyFormClosed()
        {
            try
            {
                if (_cancel != null
                    && _cancel.IsCancellationRequested == false)
                    _cancel?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

#if SUPPORT_OLD_STOP
            if (this.Channel != null)
            {
                this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
                this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
                this.Channel.Idle -= Channel_Idle;

                this.Channel.Close();   // TODO: 最好限制一个时间，超过这个时间则Abort()
            }

            if (_stop != null) // 脱离关联
            {
                _stop.Unregister();	// 和容器关联
                _stop = null;
            }
#endif
            if (this.UseLooping == true)
            {
                this._loopingHost.StopManager.DeleteGroup(this.GetGroupName());
            }

            // 原来

            if (Program.MainForm != null)
                Program.MainForm.Move -= new EventHandler(MainForm_Move);

#if NO
            if (_floatingMessage != null)
                _floatingMessage.Close();
#endif
            CloseFloatingMessage();
            /*
            // 如果MDI子窗口不是MainForm刚刚准备退出时的状态，恢复它。为了记忆尺寸做准备
            if (this.WindowState != Program.MainForm.MdiWindowState)
                this.WindowState = Program.MainForm.MdiWindowState;
             * */
            DeleteAllTempFiles();
        }



        void MainForm_Move(object sender, EventArgs e)
        {
            if (this._floatingMessage != null)
                this._floatingMessage.OnResizeOrMove();
        }

        public void ShowMessageAutoClear(string strMessage,
string strColor = "",
int delay = 2000,
bool bClickClose = false)
        {
            _ = Task.Run(() =>
            {
                ShowMessage(strMessage,
    strColor,
    bClickClose);
                System.Threading.Thread.Sleep(delay);
                // 中间一直没有变化才去消除它
                if (_floatingMessage.Text == strMessage)
                    ClearMessage();
            });
        }

        public void ShowMessage(string strMessage,
    string strColor = "",
    bool bClickClose = false)
        {
            if (this._floatingMessage == null)
                return;

            Color color = Color.FromArgb(80, 80, 80);

            if (strColor == "red")          // 出错
                color = Color.DarkRed;
            else if (strColor == "yellow")  // 成功，提醒
                color = Color.DarkGoldenrod;
            else if (strColor == "green")   // 成功
                color = Color.Green;
            else if (strColor == "progress")    // 处理过程
                color = Color.FromArgb(80, 80, 80);

            this._floatingMessage.SetMessage(strMessage, color, bClickClose);
        }

        // 线程安全
        public void ClearMessage()
        {
            if (this._floatingMessage == null)
                return;

            this._floatingMessage.Text = "";
        }

        // 线程安全
        public void AppendFloatingMessage(string strText)
        {
            this._floatingMessage.Text += strText;
        }

        // 线程安全
        public string FloatingMessage
        {
            get
            {
                if (this._floatingMessage == null)
                    return "";
                return this._floatingMessage.Text;
            }
            set
            {
                if (this._floatingMessage != null)
                    this._floatingMessage.Text = value;
            }
        }

        #region 新风格的 ChannelPool

        ChannelList _channelList = new ChannelList();

        // parameters:
        //      strStyle    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public virtual LibraryChannel GetChannel(string strServerUrl = ".",
            string strUserName = ".",
            GetChannelStyle style = GetChannelStyle.None/*2022/11/12 从 GUI 改为 None*/,   // GetChannelStyle.GUI,
            string strClientIP = "")
        {
            LibraryChannel channel = Program.MainForm.GetChannel(strServerUrl, strUserName, style, strClientIP);

            /*
            lock (_syncRoot_channelList)
            {
                _channelList.Add(channel);
            }
            */
            _channelList.AddChannel(channel);
            // TODO: 检查数组是否溢出
            return channel;
        }

        public virtual void ReturnChannel(LibraryChannel channel)
        {
            Program.MainForm.ReturnChannel(channel);
            /*
            lock (_syncRoot_channelList)
            {
                _channelList.Remove(channel);
            }
            */
            _channelList.RemoveChannel(channel);
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 huxh@xxx
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.InvalidOperationException
Message: 集合已修改；可能无法执行枚举操作。
Stack:
在 System.ThrowHelper.ThrowInvalidOperationException(ExceptionResource resource)
在 System.Collections.Generic.List`1.Enumerator.MoveNextRare()
在 dp2Circulation.MyForm.DoStop(Object sender, StopEventArgs e)
在 dp2Circulation.TaskList.DoStop(Object sender, StopEventArgs e)
在 dp2Circulation.QuickChargingForm.ClearTaskByRows(List`1 rows, Boolean bWarning)
在 dp2Circulation.QuickChargingForm.SmartSetFuncState(FuncState value, Boolean bClearInfoWindow, Boolean bDupAsClear)
在 dp2Circulation.QuickChargingForm.set_SmartFuncState(FuncState value)
在 dp2Circulation.MainForm.toolButton_borrow_Click(Object sender, EventArgs e)
在 System.Windows.Forms.ToolStripItem.RaiseEvent(Object key, EventArgs e)
在 System.Windows.Forms.ToolStripButton.OnClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleMouseUp(MouseEventArgs e)
在 System.Windows.Forms.ToolStrip.OnMouseUp(MouseEventArgs mea)
在 System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ToolStrip.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=2.28.6325.27243, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.1.7601 Service Pack 1
本机 MAC 地址: F44D3077D567 
操作时间 2017/5/18 13:25:07 (Thu, 18 May 2017 13:25:07 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 
 * */
        public void DoStop(object sender, StopEventArgs e)
        {
#if SUPPORT_OLD_STOP
            // 兼容旧风格
            if (this.Channel != null)
                this.Channel.Abort();
#endif

            /*
            lock (_syncRoot_channelList)
            {
                foreach (LibraryChannel channel in _channelList)
                {
                    if (channel != null)
                        channel.Abort();
                }
            }
            */
            _channelList.AbortAll();
        }

        public string CurrentUserName
        {
            get
            {
                return Program.MainForm?._currentUserName;
            }
        }

        // 当前用户能管辖的一个或者多个馆代码
        public string CurrentLibraryCodeList
        {
            get
            {
                return Program.MainForm?._currentLibraryCodeList;
            }
        }

        public string CurrentRights
        {
            get
            {
                return Program.MainForm?._currentUserRights;
            }
        }

        #endregion

        #region 旧风格的 Channel

#if SUPPORT_OLD_STOP
        /// <summary>
        /// 通讯通道登录前被触发
        /// </summary>
        /// <param name="sender">调用者</param>
        /// <param name="e">事件参数</param>
        public virtual void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            Program.MainForm?.Channel_BeforeLogin(sender, e); // 2015/11/4 原来是 this
        }

        public virtual void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            Program.MainForm?.Channel_AfterLogin(sender, e); // 2015/11/4 原来是 this
        }

#endif

#if NO
        // 获得全部可用的图书馆代码。注意，并不包含 "" (全局)
        public int GetAllLibraryCodes(out List<string> library_codes,
            out string strError)
        {
            strError = "";
            library_codes = new List<string>();

            string strValue = "";
            long lRet = this.Channel.GetSystemParameter(stop,
                "system", 
                "libraryCodes",
                out strValue, 
                out strError);
            if (lRet == -1)
                return -1;
            library_codes = StringUtil.SplitList(strValue);

            return 0;
        }
#endif

        #endregion

#if SUPPORT_OLD_STOP
        /// <summary>
        /// 开始一个循环
        /// </summary>
        /// <param name="strStyle">风格。如果包含 "halfstop"，表示停止按钮使用温和中断方式 </param>
        /// <param name="strMessage">要在状态行显示的消息文字</param>
        public void BeginLoop(string strStyle = "",
            string strMessage = "")
        {
            if (this.UseLooping == true)
                throw new ArgumentException("UseLooping 为 true 时不应该调用本函数");

            if (StringUtil.IsInList("halfstop", strStyle) == true)
                _stop.Style = StopStyle.EnableHalfStop;

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial(strMessage);
            _stop.BeginLoop();
        }

        /// <summary>
        /// 结束一个循环
        /// </summary>
        public void EndLoop()
        {
            if (this.UseLooping == true)
                throw new ArgumentException("UseLooping 为 true 时不应该调用本函数");

            _stop.EndLoop();
            _stop.OnStop -= new StopEventHandler(this.DoStop);
            _stop.Initial("");
            _stop.HideProgress();
            _stop.Style = StopStyle.None;
        }

        /// <summary>
        /// 循环是否结束？
        /// </summary>
        /// <returns>true: 循环已经结束; false: 循环尚未结束</returns>
        public bool IsStopped()
        {
            if (this.UseLooping == true)
                throw new ArgumentException("UseLooping 为 true 时不应该调用本函数");

            Application.DoEvents();	// 出让界面控制权

            if (this._stop != null)
            {
                if (_stop.State != 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 设置进度条上的文字显示
        /// </summary>
        /// <param name="strMessage">要显示的字符串</param>
        public void SetProgressMessage(string strMessage)
        {
            if (this.UseLooping == true)
                throw new ArgumentException("UseLooping == true 时不应调用本函数");
            _stop.SetMessage(strMessage);
        }
#endif

        /// <summary>
        /// 为当前窗口恢复缺省字体
        /// </summary>
        public void RestoreDefaultFont()
        {
            if (Program.MainForm != null)
            {
                Size oldsize = this.Size;
                if (Program.MainForm.DefaultFont == null)
                    MainForm.SetControlFont(this, Control.DefaultFont);
                else
                    MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
                this.Size = oldsize;
            }

            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetString(
                   this.FormName,
                   "default_font",
                   "");

                MainForm.AppInfo.SetString(
                    this.FormName,
                    "default_font_color",
                    "");
            }
        }

        // 设置字体
        /// <summary>
        /// 设置基本字体。会出现一个对话框询问要设定的字体
        /// </summary>
        public void SetBaseFont()
        {
            FontDialog dlg = new FontDialog();

            dlg.ShowColor = true;
            dlg.Color = this.ForeColor;
            dlg.Font = this.Font;
            dlg.ShowApply = true;
            dlg.ShowHelp = true;
            dlg.AllowVerticalFonts = false;

            dlg.Apply -= new EventHandler(dlgFont_Apply);
            dlg.Apply += new EventHandler(dlgFont_Apply);
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            Size oldsize = this.Size;

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

            this.Font = dlg.Font;
            this.ForeColor = dlg.Color;

            this.Size = oldsize;

            //ReLayout(this);

            SaveFontSetting();
        }

        void dlgFont_Apply(object sender, EventArgs e)
        {
            FontDialog dlg = (FontDialog)sender;

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;

            this.Font = dlg.Font;
            this.ForeColor = dlg.Color;

            //ReLayout(this);

            // 保存到配置文件
            SaveFontSetting();
        }

        /*
        static void ReLayout(Control parent_control)
        {
            foreach (Control control in parent_control.Controls)
            {
                ReLayout(control);

                control.ResumeLayout(false);
                control.PerformLayout();
            }

            parent_control.ResumeLayout(false);
        }*/

        /// <summary>
        /// 保存字体设置信息到配置参数存储
        /// </summary>
        public void SaveFontSetting()
        {
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                {
                    // Create the FontConverter.
                    System.ComponentModel.TypeConverter converter =
                        System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                    string strFontString = converter.ConvertToString(this.Font);

                    Program.MainForm.AppInfo.SetString(
                        this.FormName,
                        "default_font",
                        strFontString);
                }

                {
                    // Create the ColorConverter.
                    System.ComponentModel.TypeConverter converter =
                        System.ComponentModel.TypeDescriptor.GetConverter(typeof(Color));

                    string strFontColor = converter.ConvertToString(this.ForeColor);

                    MainForm.AppInfo.SetString(
                        this.FormName,
                        "default_font_color",
                        strFontColor);
                }
            }
        }

        /// <summary>
        /// 从配置参数存储中装载字体设置信息
        /// </summary>
        public void LoadFontSetting()
        {
            if (Program.MainForm == null)
                return;

            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                string strFontString = MainForm.AppInfo.GetString(
                    this.FormName,
                    "default_font",
                    "");  // "Arial Unicode MS, 12pt"

                if (String.IsNullOrEmpty(strFontString) == false)
                {
                    // Create the FontConverter.
                    System.ComponentModel.TypeConverter converter =
                        System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                    this.Font = (Font)converter.ConvertFromString(strFontString);
                }
                else
                {
                    // 沿用系统的缺省字体
                    if (Program.MainForm != null)
                    {
                        MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
                    }
                }

                string strFontColor = MainForm.AppInfo.GetString(
                    this.FormName,
                    "default_font_color",
                    "");

                if (String.IsNullOrEmpty(strFontColor) == false)
                {
                    // Create the ColorConverter.
                    System.ComponentModel.TypeConverter converter =
                        System.ComponentModel.TypeDescriptor.GetConverter(typeof(Color));

                    this.ForeColor = (Color)converter.ConvertFromString(strFontColor);
                }
                this.PerformLayout();
            }
        }

        /// <summary>
        /// Form 装载事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnLoad(EventArgs e)
        {
            this.LoadFontSetting();
            this.OnMyFormLoad();
            base.OnLoad(e);


            // 设置窗口尺寸状态
            // 一般派生类会在 EntityForm_Load() 函数中
            /*
            Program.MainForm.AppInfo.LoadMdiSize += new EventHandler(AppInfo_LoadMdiSize);
            Program.MainForm.AppInfo.SaveMdiSize += new EventHandler(AppInfo_SaveMdiSize);
             * 因此这里稍后一点处理尺寸初始化是必要的
             * 
             * * */
            if (Program.MainForm != null && Program.MainForm.AppInfo != null
                && Floating == false)
            {
                Program.MainForm?.AppInfo?.LoadMdiChildFormStates(this,
                        "mdi_form_state",
                        this.SuppressSizeSetting == true ? SizeStyle.Layout : SizeStyle.All);
            }
        }

        /// <summary>
        /// Form 关闭事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 在这里保存。如果靠后调用，可能会遇到 base.OnFormClosed() 里面相关事件被卸掉的问题
            if (Program.MainForm != null && Program.MainForm.AppInfo != null
    && Floating == false)
            {
                Program.MainForm?.AppInfo?.SaveMdiChildFormStates(this,
                    "mdi_form_state",
                    this.SuppressSizeSetting == true ? SizeStyle.Layout : SizeStyle.All);
            }

            base.OnFormClosed(e);
            this.OnMyFormClosed();  // 这里的顺序调整过 2015/11/10

            this.DisposeFreeControls();
        }

        /// <summary>
        /// 在 FormClosing 阶段，是否要越过 this.OnMyFormClosing(e)
        /// </summary>
        public bool SuppressFormClosing = false;

        /// <summary>
        /// 是否需要忽略尺寸设定的过程
        /// </summary>
        public bool SuppressSizeSetting = false;

        /// <summary>
        /// Form 即将关闭事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (this.SuppressFormClosing == false)
                this.OnMyFormClosing(e);
        }

        // 是否使用了新的 Looping 风格
        public bool UseLooping { get; set; }

        /// <summary>
        /// Form 激活事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnActivated(EventArgs e)
        {
            // 2017/4/23
            if (this.IsDisposed)
                return;

            if (Program.MainForm != null)
            {
                if (UseLooping) // 新的 Looping 风格
                {
                    /*
                    // 2022/10/29
                    Program.MainForm.stopManager?.Active(this.TopLooping?.Progress);
                    */
                    var result = this._loopingHost.StopManager.ActivateGroup(this.GetGroupName());
                    // Debug.Assert(result != null);
                }
                else
                {
#if SUPPORT_OLD_STOP
                    Program.MainForm?.stopManager?.Active(this._stop);
#endif
                }

                Program.MainForm.MenuItem_font.Enabled = true;
                Program.MainForm.MenuItem_restoreDefaultFont.Enabled = true;
            }

            base.OnActivated(e);
        }

        /*
        bool _isActive = false;

        protected override void OnDeactivate(EventArgs e)
        {
            _isActive = false;
            base.OnDeactivate(e);
        }
        */

        // 形式校验条码号
        // return:
        //      -2  服务器没有配置校验方法，无法校验
        //      -1  error
        //      0   不是合法的条码号
        //      1   是合法的读者证条码号
        //      2   是合法的册条码号
        /// <summary>
        /// 形式校验条码号
        /// </summary>
        /// <param name="strBarcode">要校验的条码号</param>
        /// <param name="strLibraryCodeList">馆代码列表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-2  服务器没有配置校验方法，无法校验</para>
        /// <para>-1  出错</para>
        /// <para>0   不是合法的条码号</para>
        /// <para>1   是合法的读者证条码号</para>
        /// <para>2   是合法的册条码号</para>
        /// </returns>
        public virtual int VerifyBarcode(
            string strLibraryCodeList,
            string strBarcode,
            out string strError)
        {
            strError = "";

            /*
            // EnableControls(false);
            // 2022/8/30
            var channel = this.GetChannel();
#if NO
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在验证条码号 " + strBarcode + "...");
            stop.BeginLoop();
#endif
            string strOldMessage = _stop.Initial("正在验证条码号 " + strBarcode + "...");
            */
            var looping = Looping(out LibraryChannel channel,
                "正在验证条码号 " + strBarcode + "...");
            try
            {
                return Program.MainForm.VerifyBarcode(
                    looping.Progress,
                    channel,
                    strLibraryCodeList,
                    strBarcode,
                    EnableControls,
                    out strError);
            }
            finally
            {
                looping.Dispose();
                /*
#if NO
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
#endif
                _stop.Initial(strOldMessage);
                this.ReturnChannel(channel);
                // EnableControls(true);
                */
            }
        }

        /// <summary>
        /// 向控制台输出 HTML
        /// </summary>
        /// <param name="strHtml">要输出的 HTML 字符串</param>
        public void OutputHtml(string strHtml)
        {
            Program.MainForm?.OperHistory?.AppendHtml(strHtml);
        }

        // parameters:
        //      nWarningLevel   0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public virtual void OutputText(string strText, int nWarningLevel = 0)
        {
            string strClass = "normal";
            if (nWarningLevel == 1)
                strClass = "warning";
            else if (nWarningLevel >= 2)
                strClass = "error";
            Program.MainForm?.OperHistory?.AppendHtml("<div class='debug " + strClass + "'>" + HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>") + "</div>");
        }

        /// <summary>
        /// MDI子窗口被通知事件发生
        /// </summary>
        /// <param name="e">事件类型</param>
        public virtual void OnNotify(ParamChangedEventArgs e)
        {

        }




        #region 种次号尾号相关

        public int ReleaseProtectedTailNumber(
dp2Circulation.CallNumberForm.MemoTailNumber number,
out string strError)
        {
            strError = "";

            string strOutputNumber = "";

            return ProtectTailNumber(
                "unmemo",
                number.ArrangeGroupName,
                number.Class,
                number.Number,
                out strOutputNumber,
                out strError);
        }

        // 保护或者释放保护一个尾号。
        // 所谓保护，就是把一个尾号交给 dp2library 记忆在内存中，防止后面取号的时候再用到这个号。
        // 注: 当用到这个号的册记录保存了，或者放弃了使用这个号，需要专门请求 dp2library 释放对这个号的保护
        // parameters:
        //      strAction   protect/unmemo 之一
        public int ProtectTailNumber(
            string strAction,
            string strArrangeGroupName,
            string strClass,
            string strTestNumber,
            out string strOutputNumber,
            out string strError)
        {
            strOutputNumber = "";

            if (string.IsNullOrEmpty(strTestNumber) == false
                && strTestNumber.Contains("/") == true)
            {
                strError = $"strTestNumber 参数值中不应包含 '/' ('{strTestNumber}')";
                return -1;
            }

            // EnableControls(false);

            Debug.Assert(strAction == "protect" || strAction == "unmemo", "");

            // 显示到操作历史中
            {
                string oper_name = "保护";
                if (strAction == "unmemo")
                    oper_name = "解除保护";
                string text = $"{oper_name} 种次号 '{strTestNumber}' (类号={strClass}, 排架体系名={strArrangeGroupName})";
                Program.MainForm.OperHistory.AppendHtml("<div class='debug green'>" + HttpUtility.HtmlEncode(text) + "</div>");
            }

            /*
            LibraryChannel channel = this.GetChannel();
            string strOldMessage = Progress.Initial(strAction == "protect" ? "正在请求保护尾号 ..." : "正在请求释放保护尾号 ...");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 1, 0);
            */
            var looping = Looping(out LibraryChannel channel,
                strAction == "protect" ? "正在请求保护尾号 ..." : "正在请求释放保护尾号 ...",
                "timeout:0:1:0");

            try
            {
                long lRet = channel.SetOneClassTailNumber(
                    looping.Progress,
                    strAction,
                    strArrangeGroupName,
                    strClass,
                    strTestNumber,
                    out strOutputNumber,
                    out strError);
                if (lRet == -1)
                {
                    Program.MainForm.OperHistory.AppendHtml("<div class='debug red'>" + HttpUtility.HtmlEncode($"返回出错:{strError}") + "</div>");
                    return -1;
                }

                Program.MainForm.OperHistory.AppendHtml("<div class='debug yellow'>" + HttpUtility.HtmlEncode($"返回成功:strOutputNumber={strOutputNumber}, lRet={lRet}, strError={strError}") + "</div>");
                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
                Progress.Initial(strOldMessage);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
                */
            }
        }

        // 获取书目记录的局部
        public int GetBiblioPart(string strBiblioRecPath,
            string strBiblioXml,
            string strPartName,
            out string strResultValue,
            out string strError)
        {
            /*
            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 0, 10);

#if NO
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在获取书目记录的局部 -- '" + strPartName + "'...");
            Progress.BeginLoop();
#endif

            string strOldMessage = Progress.Message;
            Progress.SetMessage("正在装入书目记录 " + strBiblioRecPath + " 的局部 ...");
            */
            var looping = Looping(out LibraryChannel channel,
                "正在装入书目记录 " + strBiblioRecPath + " 的局部 ...",
                "timeout:0:0:10");
            try
            {
                long lRet = channel.GetBiblioInfo(
                    looping.Progress,   // Progress.State == 0 ? Progress : null,
                    strBiblioRecPath,
                    strBiblioXml,
                    strPartName,    // 包含'@'符号
                    out strResultValue,
                    out strError);
                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
#if NO
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
#endif
                Progress.SetMessage(strOldMessage);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
                */
            }
        }

#if REMOVED
        // 
        /// <summary>
        /// 获取书目记录的局部
        /// </summary>
        /// <param name="channel">通讯通道</param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strBiblioXml">书目记录 XML</param>
        /// <param name="strPartName">局部名</param>
        /// <param name="strResultValue">返回结果字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
        public int GetBiblioPart(
            LibraryChannel channel,
            string strBiblioRecPath,
            string strBiblioXml,
            string strPartName,
            out string strResultValue,
            out string strError)
        {
            long lRet = channel.GetBiblioInfo(
                null,   // _stop,
                strBiblioRecPath,
                strBiblioXml,
                strPartName,    // 包含'@'符号
                out strResultValue,
                out strError);
            return (int)lRet;
        }
#endif

#if REMOVED
        //
        /// <summary>
        /// 获取书目摘要
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="strItemBarcode">册条码号</param>
        /// <param name="strConfirmItemRecPath">(册条码号发生重复时)用于确认的册记录路径</param>
        /// <param name="strBiblioRecPathExclude">要排除的书目记录路径列表，用逗号间隔。除开列表中的这些书目记录路径, 才返回摘要内容, 否则仅仅返回书目记录路径</param>
        /// <param name="strBiblioRecPath">返回书目记录路径</param>
        /// <param name="strSummary">返回书目摘要</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
        public int GetBiblioSummary(
            LibraryChannel channel,
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strBiblioRecPathExclude,
            out string strBiblioRecPath,
            out string strSummary,
            out string strError)
        {
            long lRet = channel.GetBiblioSummary(
                null, // _stop,
                strItemBarcode,
                strConfirmItemRecPath,
                strBiblioRecPathExclude,
                out strBiblioRecPath,
                out strSummary,
                out strError);
            return (int)lRet;
        }
#endif

        //
        /// <summary>
        /// 获取书目摘要
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <param name="strConfirmItemRecPath">(册条码号发生重复时)用于确认的册记录路径</param>
        /// <param name="strBiblioRecPathExclude">要排除的书目记录路径列表，用逗号间隔。除开列表中的这些书目记录路径, 才返回摘要内容, 否则仅仅返回书目记录路径</param>
        /// <param name="strBiblioRecPath">返回书目记录路径</param>
        /// <param name="strSummary">返回书目摘要</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
        public int GetBiblioSummary(
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strBiblioRecPathExclude,
            out string strBiblioRecPath,
            out string strSummary,
            out string strError)
        {
            /*
            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 0, 10);
            */
            var looping = Looping(out LibraryChannel channel,
                null,
                "timeout:0:0:10");
            try
            {
                long lRet = channel.GetBiblioSummary(
                    looping.Progress,
                    strItemBarcode,
                    strConfirmItemRecPath,
                    strBiblioRecPathExclude,
                    out strBiblioRecPath,
                    out strSummary,
                    out strError);
                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
                */
            }
        }

        // 获得第一个(实有的)日志文件日期
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public static int GetFirstOperLogDate(
            Stop stop,
            LibraryChannel channel,
            LogType logType,
            out string strFirstDate,
            out string strError)
        {
            strFirstDate = "";
            strError = "";

            DigitalPlatform.LibraryClient.localhost.OperLogInfo[] records = null;

            List<string> dates = new List<string>();
            List<string> styles = new List<string>();
            if ((logType & LogType.OperLog) != 0)
                styles.Add("getfilenames");
            if ((logType & LogType.AccessLog) != 0)
                styles.Add("getfilenames,accessLog");
            if (styles.Count == 0)
            {
                strError = "logStyle 参数值中至少要包含一种类型";
                return -1;
            }

            foreach (string style in styles)
            {
                // 获得日志
                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围，本次调用无效
                long lRet = channel.GetOperLogs(
                    stop,
                    "",
                    0,
                    -1,
                    1,
                    style,  // "getfilenames",
                    "", // strFilter
                    out records,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                    continue;

                if (records == null || records.Length < 1)
                {
                    strError = "records error";
                    return -1;
                }

                if (string.IsNullOrEmpty(records[0].Xml) == true
                    || records[0].Xml.Length < 8)
                {
                    strError = "records[0].Xml error";
                    return -1;
                }

                dates.Add(records[0].Xml.Substring(0, 8));
            }

            if (dates.Count == 0)
                return 0;

            // 取较小的一个
            if (dates.Count > 1)
                dates.Sort();
            strFirstDate = dates[0];
            return 1;
        }

        #endregion


        // 
        /// <summary>
        /// 列出可用的查重方案名
        /// </summary>
        /// <param name="strRecPath">发起记录路径。null 表示希望获取所有查重方案名</param>
        /// <param name="projectnames">返回可用的查重方案名字符串数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; >=0: 成功</returns>
        public int ListProjectNames(string strRecPath,
            out string[] projectnames,
            out string strError)
        {
            strError = "";
            projectnames = null;

            /*
            EnableControls(false);

            LibraryChannel channel = this.GetChannel();

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获取可用的查重方案名 ...");
            _stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获取可用的查重方案名 ...",
                "disableControl");
            try
            {
                string strBiblioDbName = Global.GetDbName(strRecPath);
                // string strBiblioDbName = null;

                long lRet = channel.ListDupProjectInfos(
                    looping.Progress,
                    strBiblioDbName,
                    out DupProjectInfo[] dpis,
                    out strError);
                if (lRet == -1)
                    return -1;

                projectnames = new string[dpis.Length];
                for (int i = 0; i < projectnames.Length; i++)
                {
                    projectnames[i] = dpis[i].Name;
                }

                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                this.ReturnChannel(channel);

                EnableControls(true);
                */
            }
        }


        /// <summary>
        /// 获得102相关信息
        /// </summary>
        /// <param name="strPublisherNumber">出版社号码</param>
        /// <param name="str102">返回 102 字符春</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
        public int Get102Info(string strPublisherNumber,
            out string str102,
            out string strError)
        {
            strError = "";
            str102 = "";

            string strDbName = Program.MainForm.GetUtilDbName("publisher");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未定义publisher类型的实用库名";
                return -1;
            }

            /*
            LibraryChannel channel = this.GetChannel();

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在获得102信息 ...");
            Progress.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得102信息 ...");
            try
            {
                string strAction = "";

                long lRet = channel.GetUtilInfo(
                    looping.Progress,
                    strAction,
                    strDbName,
                    "ISBN",
                    strPublisherNumber,
                    "v102",
                    out str102,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                    return 0;

                return 1;
            }
            finally
            {
                looping.Dispose();
                /*
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");

                this.ReturnChannel(channel);
                */
            }
        }

        // 
        /// <summary>
        /// 设置102相关信息
        /// </summary>
        /// <param name="strPublisherNumber">出版社号码</param>
        /// <param name="str102">102 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int Set102Info(string strPublisherNumber,
            string str102,
            out string strError)
        {
            strError = "";

            string strDbName = Program.MainForm.GetUtilDbName("publisher");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未定义publisher类型的实用库名";
                return -1;
            }

            /*
            LibraryChannel channel = this.GetChannel();

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在设置102信息 ...");
            Progress.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在设置102信息 ...");

            try
            {
                string strAction = "";

                long lRet = channel.SetUtilInfo(
                    looping.Progress,
                    strAction,
                    strDbName,
                    "ISBN",
                    "r",
                    "i",
                    "v102",
                    strPublisherNumber,
                    str102,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");

                this.ReturnChannel(channel);
                */
            }
        }

        // 
        /// <summary>
        /// 获得出版社相关信息
        /// </summary>
        /// <param name="strPublisherNumber">出版社号码</param>
        /// <param name="str210">返回 210 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
        public int GetPublisherInfo(string strPublisherNumber,
            out string str210,
            out string strError)
        {
            strError = "";
            str210 = "";

            string strDbName = Program.MainForm.GetUtilDbName("publisher");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未定义publisher类型的实用库名";
                return -1;
            }

            /*
            LibraryChannel channel = this.GetChannel();

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在获得出版社信息 ...");
            Progress.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得出版社信息 ...");
            try
            {
                string strAction = "";

                long lRet = channel.GetUtilInfo(
                    looping.Progress,
                    strAction,
                    strDbName,
                    "ISBN",
                    strPublisherNumber,
                    "v210",
                    out str210,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                    return 0;

                return 1;
            }
            finally
            {
                looping.Dispose();
                /*
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");

                this.ReturnChannel(channel);
                */
            }
        }

        // 
        /// <summary>
        /// 设置出版社相关信息
        /// </summary>
        /// <param name="strPublisherNumber">出版社号码</param>
        /// <param name="str210">210 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int SetPublisherInfo(string strPublisherNumber,
            string str210,
            out string strError)
        {
            strError = "";

            string strDbName = Program.MainForm.GetUtilDbName("publisher");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未定义publisher类型的实用库名";
                return -1;
            }

            /*
            LibraryChannel channel = this.GetChannel();

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在设置出版社信息 ...");
            Progress.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在设置出版社信息 ...");
            try
            {
                string strAction = "";

                long lRet = channel.SetUtilInfo(
                    looping.Progress,
                    strAction,
                    strDbName,
                    "ISBN",
                    "r",
                    "i",
                    "v210",
                    strPublisherNumber,
                    str210,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");

                this.ReturnChannel(channel);
                */
            }

        }


        #region 创建书目记录的浏览格式

        public int BuildBrowseText(string strXml,
            out string strBrowseText,
            out string strMarcSyntax,
            out string strColumnTitles,
            out string strError)
        {
            strError = "";
            strBrowseText = "";
            strMarcSyntax = "";
            strColumnTitles = "";

            int nRet = 0;

            string strMARC = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            nRet = MarcUtil.Xml2Marc(strXml,    // info.OldXml,
                true,
                null,
                out strMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML 转换到 MARC 记录时出错: " + strError;
                return -1;
            }
            if (string.IsNullOrEmpty(strMARC))
            {
                strError = "MARC 记录为空";
                return -1;
            }

            Debug.Assert(string.IsNullOrEmpty(strMarcSyntax) == false, "");

            nRet = BuildMarcBrowseText(
                strMarcSyntax,
                strMARC,
                out strBrowseText,
                out strColumnTitles,
                out strError);
            if (nRet == -1)
            {
                strError = "MARC 记录转换到浏览格式时出错: " + strError;
                return -1;
            }

            return 0;
        }

        // 创建MARC格式记录的浏览格式
        // paramters:
        //      strMARC MARC机内格式
        public static int BuildMarcBrowseText(
            string strMarcSyntax,
            string strMARC,
            out string strBrowseText,
            out string strColumnTitles,
            out string strError)
        {
            strBrowseText = "";
            strError = "";
            strColumnTitles = "";

            FilterHost host = new FilterHost();
            host.ID = "";
            host.MainForm = Program.MainForm;

            BrowseFilterDocument filter = null;

            string strFilterFileName = Path.Combine(Program.MainForm.DataDir, strMarcSyntax.Replace(".", "_") + "_cfgs\\marc_browse.fltx");

            int nRet = PrepareMarcFilter(
                host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 2021/12/4
            // 对于 USMARC 格式的记录，如果有 880 字段，则转为平行模式，并尽量采用其中的汉字内容
            if (strMarcSyntax == "usmarc")
            {
                MarcRecord record = new MarcRecord(strMARC);
                MarcQuery.ToParallel(record);

                strMARC = record.Text;
            }

            try
            {
                nRet = filter.DoRecord(null,
        strMARC,
        0,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                strBrowseText = host.ResultString;
                strColumnTitles = host.ColumnTitles;
            }
            finally
            {
                // 归还对象
                filter.FilterHost = null;   // 2016/1/23
                Program.MainForm.Filters.SetFilter(strFilterFileName, filter);
            }

            return 0;
        ERROR1:
            return -1;
        }

        public static int PrepareMarcFilter(
FilterHost host,
string strFilterFileName,
out BrowseFilterDocument filter,
out string strError)
        {
            strError = "";

            // 看看是否有现成可用的对象
            filter = (BrowseFilterDocument)Program.MainForm.Filters.GetFilter(strFilterFileName);

            if (filter != null)
            {
                filter.FilterHost = host;
                return 1;
            }

            // 新创建
            // string strFilterFileContent = "";

            filter = new BrowseFilterDocument();

            filter.FilterHost = host;
            filter.strOtherDef = "FilterHost Host = null;";

            filter.strPreInitial = " BrowseFilterDocument doc = (BrowseFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " Host = ("
                + "FilterHost" + ")doc.FilterHost;\r\n";

            // filter.Load(strFilterFileName);

            try
            {
                filter.Load(strFilterFileName);
            }
            catch (Exception ex)
            {
                strError = "MyForm filter.Load() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            string strCode = "";    // c#代码

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strBinDir = Environment.CurrentDirectory;

            string[] saAddRef1 = {
                                         strBinDir + "\\digitalplatform.core.dll",
                                         strBinDir + "\\digitalplatform.marcdom.dll",
										 // this.BinDir + "\\digitalplatform.marckernel.dll",
										 // this.BinDir + "\\digitalplatform.libraryserver.dll",
										 strBinDir + "\\digitalplatform.dll",
                                         strBinDir + "\\digitalplatform.Text.dll",
                                         strBinDir + "\\digitalplatform.IO.dll",
                                         strBinDir + "\\digitalplatform.Xml.dll",
                                         strBinDir + "\\dp2circulation.exe" };

            Assembly assembly = null;
            string strWarning = "";
            // string strLibPaths = "";

            string[] saRef2 = filter.GetRefs();

            string[] saRef = new string[saRef2.Length + saAddRef1.Length];
            Array.Copy(saRef2, saRef, saRef2.Length);
            Array.Copy(saAddRef1, 0, saRef, saRef2.Length, saAddRef1.Length);

            // 创建Script的Assembly
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                "", // strLibPaths,
                out assembly,
                out strError,
                out strWarning);

            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                {
                    goto ERROR1;
                }
                // MessageBox.Show(this, strWarning);
            }

            filter.Assembly = assembly;
            return 0;
        ERROR1:
            return -1;
        }

        #endregion

        // TODO: 值变化后要出现延时关闭的 floatingMessage
        public bool SearchShareBiblio
        {
            get
            {
                if (Program.MainForm != null && Program.MainForm.AppInfo != null)
                    return Program.MainForm.AppInfo.GetBoolean(
        "biblio_search_form",
        "search_sharebiblio",
        true);
                return false;
            }
            set
            {
                if (Program.MainForm != null && Program.MainForm.AppInfo != null)
                {
                    Program.MainForm.AppInfo.SetBoolean(
        "biblio_search_form",
        "search_sharebiblio",
        value);
                    if (value == true)
                        this.ShowMessage("使用共享网络", "");
                    else
                        this.ShowMessage("不使用共享网络", "");

                    this._floatingMessage.DelayClear(new TimeSpan(0, 0, 3));
                }
            }
        }

        public bool SearchZ3950
        {
            get
            {
                if (Program.MainForm != null && Program.MainForm.AppInfo != null)
                    return Program.MainForm.AppInfo.GetBoolean(
        "biblio_search_form",
        "search_z3950",
        false);
                return false;
            }
            set
            {
                if (Program.MainForm != null && Program.MainForm.AppInfo != null)
                {
                    Program.MainForm.AppInfo.SetBoolean(
        "biblio_search_form",
        "search_z3950",
        value);
                    if (value == true)
                        this.ShowMessage("使用 Z39.50", "");
                    else
                        this.ShowMessage("不使用 Z39.50", "");

                    this._floatingMessage.DelayClear(new TimeSpan(0, 0, 3));
                }
            }
        }

        // 获得当前用户能管辖的全部馆代码
        public List<string> GetOwnerLibraryCodes()
        {
            if (Global.IsGlobalUser(this.CurrentLibraryCodeList) == true)
                return Program.MainForm?.GetAllLibraryCode();

            return StringUtil.SplitList(this.CurrentLibraryCodeList);
        }

#if NO
        // 获得当前用户能管辖的全部馆代码
        public List<string> GetOwnerLibraryCodes()
        {
            if (Global.IsGlobalUser(this.Channel.LibraryCodeList) == true)
                return Program.MainForm.GetAllLibraryCode();

            return StringUtil.SplitList(this.Channel.LibraryCodeList);
        }
#endif

        #region 防止控件泄露

        // 不会被自动 Dispose 的 子 Control，放在这里托管，避免内存泄漏
        List<Control> _freeControls = new List<Control>();

        public void AddFreeControl(Control control)
        {
            ControlExtention.AddFreeControl(_freeControls, control);
        }

        public void RemoveFreeControl(Control control)
        {
            ControlExtention.RemoveFreeControl(_freeControls, control);
        }

        public void DisposeFreeControls()
        {
            ControlExtention.DisposeFreeControls(_freeControls);
        }

        #endregion


        public void ParseOneMacro(ParseOneMacroEventArgs e)
        {
            string strName = StringUtil.Unquote(e.Macro, "%%");  // 去掉百分号

            // 函数名：
            string strFuncName = "";
            string strParams = "";

            int nRet = strName.IndexOf(":");
            if (nRet == -1)
            {
                strFuncName = strName.Trim();
            }
            else
            {
                strFuncName = strName.Substring(0, nRet).Trim();
                strParams = strName.Substring(nRet + 1).Trim();
            }

            if (strName == "username")
            {
                e.Value = this.CurrentUserName;
                return;
            }

            string strValue = "";
            string strError = "";
            // 从marceditor_macrotable.xml文件中解析宏
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = MacroUtil.GetFromLocalMacroTable(
                Path.Combine(Program.MainForm.UserDir, "marceditor_macrotable.xml"),
                strName,
                e.Simulate,
                out strValue,
                out strError);
            if (nRet == -1)
            {
                e.Canceled = true;
                e.ErrorInfo = strError;
                return;
            }

            if (nRet == 1)
            {
                e.Value = strValue;
                return;
            }

            if (String.Compare(strFuncName, "IncSeed", true) == 0
                || String.Compare(strFuncName, "IncSeed+", true) == 0
                || String.Compare(strFuncName, "+IncSeed", true) == 0)
            {
                // 种次号库名, 指标名, 要填充到的位数
                string[] aParam = strParams.Split(new char[] { ',' });
                if (aParam.Length != 3 && aParam.Length != 2)
                {
                    strError = "IncSeed需要2或3个参数。";
                    goto ERROR1;
                }

                bool IncAfter = false;  // 是否为先取后加
                if (strFuncName[strFuncName.Length - 1] == '+')
                    IncAfter = true;

                string strZhongcihaoDbName = aParam[0].Trim();
                string strEntryName = aParam[1].Trim();
                strValue = "";

                LibraryChannel channel = this.GetChannel();

                try
                {

                    long lRet = 0;
                    if (e.Simulate == true)
                    {
                        // parameters:
                        //      strZhongcihaoGroupName  @引导种次号库名 !引导线索书目库名 否则就是 种次号组名
                        lRet = channel.GetZhongcihaoTailNumber(
        null,
        strZhongcihaoDbName,
        strEntryName,
        out strValue,
        out strError);
                        if (lRet == -1)
                            goto ERROR1;
                        if (string.IsNullOrEmpty(strValue) == true)
                        {
                            strValue = "1";
                        }
                    }
                    else
                    {
                        // parameters:
                        //      strZhongcihaoGroupName  @引导种次号库名 !引导线索书目库名 否则就是 种次号组名
                        lRet = channel.SetZhongcihaoTailNumber(
        null,
        IncAfter == true ? "increase+" : "increase",
        strZhongcihaoDbName,
        strEntryName,
        "1",
        out strValue,
        out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                }
                finally
                {
                    this.ReturnChannel(channel);
                }

                // 补足左方'0'
                if (aParam.Length == 3)
                {
                    int nWidth = 0;
                    try
                    {
                        nWidth = Convert.ToInt32(aParam[2]);
                    }
                    catch
                    {
                        strError = "第三参数应当为纯数字（表示补足的宽度）";
                        goto ERROR1;
                    }
                    e.Value = strValue.PadLeft(nWidth, '0');
                }
                else
                    e.Value = strValue;
                return;
            }

            e.Canceled = true;  // 不能解释处理
            return;
        ERROR1:
            e.Canceled = true;
            e.ErrorInfo = strError;
        }



        #region RFID 有关功能

#if REMOVED

        public class RfidChannel
        {
            public IpcClientChannel Channel { get; set; }
            public IRfid Object { get; set; }
        }

        // internal int _inRfidCall = 0; // >0 表示正在调用 RFID API 尚未返回

        public static RfidChannel StartRfidChannel(
            string strUrl,
            out string strError)
        {
            strError = "";

            RfidChannel result = new RfidChannel();

            result.Channel = new IpcClientChannel(Guid.NewGuid().ToString(), // 随机的名字，令多个 Channel 对象可以并存 
                    new BinaryClientFormatterSinkProvider());

            ChannelServices.RegisterChannel(result.Channel, true);
            bool bDone = false;
            try
            {
                result.Object = (IRfid)Activator.GetObject(typeof(IRfid),
                    strUrl);
                if (result.Object == null)
                {
                    strError = "无法连接到服务器 " + strUrl;
                    return null;
                }
                bDone = true;
                return result;
            }
            finally
            {
                if (bDone == false)
                    EndRfidChannel(result);
            }
        }

        public static void EndRfidChannel(RfidChannel channel)
        {
            if (channel != null && channel.Channel != null)
            {
                ChannelServices.UnregisterChannel(channel.Channel);
                channel.Channel = null;
            }
        }

#endif

#if REMOVED
        // return:
        //      -2  remoting服务器连接失败。驱动程序尚未启动
        //      -1  出错
        //      0   成功
        public static NormalResult SetEAS(
            RfidChannel channel,
            string reader_name,
            string tag_name,
            bool enable,
            out string strError)
        {
            strError = "";

            try
            {
                return channel.Object.SetEAS(reader_name,
                    tag_name,
                    enable);
            }
            // [System.Runtime.Remoting.RemotingException] = {"连接到 IPC 端口失败: 系统找不到指定的文件。\r\n "}
            catch (System.Runtime.Remoting.RemotingException ex)
            {
                strError = "针对 " + Program.MainForm.RfidCenterUrl + " 的 SetEAS() 操作失败: " + ex.Message;
                return new NormalResult { Value = -2, ErrorInfo = strError };
            }
            catch (Exception ex)
            {
                strError = "针对 " + Program.MainForm.RfidCenterUrl + " 的 SetEAS() 操作失败: " + ex.Message;
                return new NormalResult { Value = -1, ErrorInfo = strError };
            }
        }

#endif

        #endregion

        #region 人脸识别有关功能

        public class FaceChannel
        {
            public IpcClientChannel Channel { get; set; }
            public IBioRecognition Object { get; set; }
        }

        internal int _inFaceCall = 0; // >0 表示正在调用人脸识别 API 尚未返回

        public static FaceChannel StartFaceChannel(
    string strUrl,
    out string strError)
        {
            strError = "";

            FaceChannel result = new FaceChannel();

            result.Channel = new IpcClientChannel(Guid.NewGuid().ToString(), // 随机的名字，令多个 Channel 对象可以并存 
                    new BinaryClientFormatterSinkProvider());

            ChannelServices.RegisterChannel(result.Channel, true);
            bool bDone = false;
            try
            {
                result.Object = (IBioRecognition)Activator.GetObject(typeof(IBioRecognition),
                    strUrl);
                if (result.Object == null)
                {
                    strError = "无法连接到服务器 " + strUrl;
                    return null;
                }
                bDone = true;
                return result;
            }
            finally
            {
                if (bDone == false)
                    EndFaceChannel(result);
            }
        }

        public static void EndFaceChannel(FaceChannel channel)
        {
            if (channel != null && channel.Channel != null)
            {
                ChannelServices.UnregisterChannel(channel.Channel);
                channel.Channel = null;
            }
        }

        // return:
        //      -1  error
        //      0   放弃输入
        //      1   成功输入
        public async Task<RecognitionFaceResult> RecognitionFace(string strStyle)
        {
            if (string.IsNullOrEmpty(Program.MainForm.FaceReaderUrl) == true)
            {
                return new RecognitionFaceResult
                {
                    Value = -1,
                    ErrorInfo = "尚未配置 人脸识别接口URL 系统参数，无法读取人脸信息"
                };
            }

            FaceChannel channel = StartFaceChannel(
                Program.MainForm.FaceReaderUrl,
                out string strError);
            if (channel == null)
                return new RecognitionFaceResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            _inFaceCall++;
            try
            {
                return await Task./*Factory.StartNew*/Run<RecognitionFaceResult>(
                    () =>
                    {
                        return channel.Object.RecognitionFace(strStyle);
                    });
            }
            catch (Exception ex)
            {
                strError = "针对 " + Program.MainForm.FaceReaderUrl + " 的 RecongitionFace() 操作失败: " + ex.Message;
                return new RecognitionFaceResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            finally
            {
                _inFaceCall--;
                EndFaceChannel(channel);
            }
        }

#if NO
        public async Task<NormalResult> FaceGetState(string strStyle)
        {
            if (string.IsNullOrEmpty(Program.MainForm.FaceReaderUrl) == true)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "尚未配置 人脸识别接口URL 系统参数，无法读取人脸信息"
                };
            }

            FaceChannel channel = StartFaceChannel(
                Program.MainForm.FaceReaderUrl,
                out string strError);
            if (channel == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            _inFaceCall++;
            try
            {
                return await Task./*Factory.StartNew*/Run<NormalResult>(
                    () =>
                    {
                        return channel.Object.GetState(strStyle);
                    });
            }
            catch (Exception ex)
            {
                strError = "针对 " + Program.MainForm.FaceReaderUrl + " 的 GetState() 操作失败: " + ex.Message;
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            finally
            {
                _inFaceCall--;
                EndFaceChannel(channel);
            }
        }
#endif
        #endregion

        #region 人脸登记功能(从 ReaderInfoForm 移动过来)

        public Task<NormalResult> FaceNotifyTask(string event_name)
        {
            string strError = "";
            if (string.IsNullOrEmpty(Program.MainForm.FaceReaderUrl) == true)
            {
                strError = "尚未配置 人脸识别接口URL 系统参数，无法通知人脸中心";
                goto ERROR1;
            }

            FaceChannel channel = StartFaceChannel(
                Program.MainForm.FaceReaderUrl,
                out strError);
            if (channel == null)
                goto ERROR1;

            _inFaceCall++;
            try
            {
                try
                {
                    return Task./*Factory.StartNew*/Run<NormalResult>(
                        () =>
                        {
                            NormalResult temp_result = new NormalResult();
                            try
                            {
                                return channel.Object.Notify(event_name);
                            }
                            catch (RemotingException ex)
                            {
                                temp_result.ErrorInfo = ex.Message;
                                temp_result.Value = 0;  // 让调主认为没有出错
                                return temp_result;
                            }
                            catch (Exception ex)
                            {
                                temp_result.ErrorInfo = ex.Message;
                                temp_result.Value = -1;
                                return temp_result;
                            }
                        });
                }
                catch (Exception ex)
                {
                    strError = "针对 " + Program.MainForm.FaceReaderUrl + " 的 Notify() 操作失败: " + ex.Message;
                    goto ERROR1;
                }
            }
            finally
            {
                _inFaceCall--;
                EndFaceChannel(channel);
            }
        ERROR1:
            return Task.FromResult(
            new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            });
        }

        public async Task<NormalResult> CancelReadFeatureString()
        {
            string strError = "";
            NormalResult result = new NormalResult();

            if (string.IsNullOrEmpty(Program.MainForm.FaceReaderUrl) == true)
            {
                strError = "尚未配置 人脸识别接口URL 系统参数，无法读取人脸信息";
                goto ERROR1;
            }

            FaceChannel channel = StartFaceChannel(
                Program.MainForm.FaceReaderUrl,
                out strError);
            if (channel == null)
                goto ERROR1;

            _inFaceCall++;
            try
            {
                try
                {
                    return await Task./*Factory.StartNew*/Run<NormalResult>(
                        () =>
                        {
                            NormalResult temp_result = new NormalResult();
                            try
                            {
                                return channel.Object.CancelGetFeatureString();
                                //if (temp_result.Value == -1)
                                //    temp_result.ErrorInfo = "API cancel return error";
                                // return temp_result;
                            }
                            catch (RemotingException ex)
                            {
                                temp_result.ErrorInfo = ex.Message;
                                temp_result.Value = 0;  // 让调主认为没有出错
                                return temp_result;
                            }
                            catch (Exception ex)
                            {
                                temp_result.ErrorInfo = ex.Message;
                                temp_result.Value = -1;
                                return temp_result;
                            }
                        });
                }
                catch (Exception ex)
                {
                    strError = "针对 " + Program.MainForm.FaceReaderUrl + " 的 CancelReadFeatureString() 操作失败: " + ex.Message;
                    goto ERROR1;
                }
            }
            finally
            {
                _inFaceCall--;
                EndFaceChannel(channel);
            }
        ERROR1:
            result.ErrorInfo = strError;
            result.Value = -1;
            return result;
        }

        // return:
        //      -1  error
        //      0   放弃输入
        //      1   成功输入
        public async Task<GetFeatureStringResult> ReadFeatureString(
            byte[] imageData,
            string strExcludeBarcodes,
            string strStyle)
        {
            string strError = "";
            GetFeatureStringResult result = new GetFeatureStringResult();

            if (string.IsNullOrEmpty(Program.MainForm.FaceReaderUrl) == true)
            {
                strError = "尚未配置 人脸识别接口URL 系统参数，无法读取人脸信息";
                goto ERROR1;
            }

            FaceChannel channel = StartFaceChannel(
                Program.MainForm.FaceReaderUrl,
                out strError);
            if (channel == null)
                goto ERROR1;

            _inFaceCall++;
            try
            {
                return await GetFeatureString(channel,
                    imageData,
                    strExcludeBarcodes,
                    strStyle);
            }
            catch (Exception ex)
            {
                strError = "针对 " + Program.MainForm.FaceReaderUrl + " 的 GetFeatureString() 操作失败: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                _inFaceCall--;
                EndFaceChannel(channel);
            }
        ERROR1:
            result.ErrorInfo = strError;
            result.Value = -1;
            return result;
        }

#if NO
        class GetFeatureStringResult
        {
            public string Feature { get; set; }
            public string Version { get; set; }

            public int Value { get; set; }
            public string ErrorInfo { get; set; }
        }
#endif

        public Task<GetFeatureStringResult> GetFeatureString(FaceChannel channel,
            byte[] imageData,
            string strExcludeBarcodes,
            string strStyle)
        {
            return Task./*Factory.StartNew*/Run<GetFeatureStringResult>(
                () =>
                {
                    // 获得一个指纹特征字符串
                    // return:
                    //      -1  error
                    //      0   放弃输入
                    //      1   成功输入
                    return channel.Object.GetFeatureString(
                        imageData,
                        strExcludeBarcodes,
                        strStyle/*,
                    out string strFingerprint,
                    out string strVersion,
                    out string strError*/);
                    //return CallGetFeatureString(channel, strExcludeBarcodes, strStyle);
                });
        }

        // 2021/10/10
        public async Task<NormalResult> FaceGetStateAsync(string style)
        {
            string strError = "";

            if (string.IsNullOrEmpty(Program.MainForm.FaceReaderUrl) == true)
            {
                strError = "尚未配置 人脸识别接口URL 系统参数，无法进行 GetState() 调用";
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            FaceChannel channel = StartFaceChannel(
                Program.MainForm.FaceReaderUrl,
                out strError);
            if (channel == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            _inFaceCall++;
            try
            {
                try
                {
                    return await Task.Run(() =>
                    {
                        return channel.Object.GetState(style);
                    });
                }
                catch (RemotingException ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = ex.Message,
                        ErrorCode = "RequestError"
                    };
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "针对 " + Program.MainForm.FaceReaderUrl + " 的 GetState() 请求失败: " + ex.Message
                    };
                }
            }
            finally
            {
                _inFaceCall--;
                EndFaceChannel(channel);
            }
        }

#if NO
        GetFeatureStringResult CallGetFeatureString(FaceChannel channel,
    string strExcludeBarcodes,
    string strStyle)
        {
            GetFeatureStringResult result = new GetFeatureStringResult();
            try
            {
                // 获得一个指纹特征字符串
                // return:
                //      -1  error
                //      0   放弃输入
                //      1   成功输入
                return channel.Object.GetFeatureString(
                    strExcludeBarcodes,
                    strStyle/*,
                    out string strFingerprint,
                    out string strVersion,
                    out string strError*/);
#if NO
                result.Feature = strFingerprint;
                result.Version = strVersion;
                result.ErrorInfo = strError;
                result.Value = nRet;
                return result;
#endif
            }
            catch (Exception ex)
            {
                result.ErrorInfo = "GetFeatureString() 异常: " + ex.Message;
                result.Value = -1;
                return result;
            }
        }
#endif
        #endregion


        #region 指纹有关功能


        public class FingerprintChannel
        {
            public IpcClientChannel Channel { get; set; }
            public IFingerprint Object { get; set; }

            public string Version { get; set; }
            public string CfgInfo { get; set; }
        }

        // IpcClientChannel m_fingerPrintChannel = new IpcClientChannel();
        // IFingerprint m_fingerPrintObj = null;
        internal int _inFingerprintCall = 0; // >0 表示正在调用指纹 API 尚未返回

        public static FingerprintChannel StartFingerprintChannel(
            string strUrl,
            out string strError)
        {
            strError = "";

            FingerprintChannel result = new FingerprintChannel();

            result.Channel = new IpcClientChannel(Guid.NewGuid().ToString(), // 随机的名字，令多个 Channel 对象可以并存 
                    new BinaryClientFormatterSinkProvider());

            ChannelServices.RegisterChannel(result.Channel, true);
            bool bDone = false;
            try
            {
                result.Object = (IFingerprint)Activator.GetObject(typeof(IFingerprint),
                    strUrl);
                if (result.Object == null)
                {
                    strError = "无法连接到服务器 " + strUrl;
                    return null;
                }
                bDone = true;
                return result;
            }
            catch (Exception ex)
            {
                strError = $"连接指纹中心 '{strUrl}' 时出现异常: {ex.Message}";
                return null;
            }
            finally
            {
                if (bDone == false)
                    EndFingerprintChannel(result);
            }
        }

        public static void EndFingerprintChannel(FingerprintChannel channel)
        {
            if (channel != null && channel.Channel != null)
            {
                ChannelServices.UnregisterChannel(channel.Channel);
                channel.Channel = null;
            }
        }

        // return:
        //      -2  remoting服务器连接失败。驱动程序尚未启动
        //      -1  出错
        //      0   成功
        public int AddItems(
            FingerprintChannel channel,
            List<FingerprintItem> items,
            out string strError)
        {
            strError = "";

            try
            {
                int nRet = channel.Object.AddItems(items,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            // [System.Runtime.Remoting.RemotingException] = {"连接到 IPC 端口失败: 系统找不到指定的文件。\r\n "}
            catch (System.Runtime.Remoting.RemotingException ex)
            {
                strError = "针对 "
#if NEWFINGER
                    + $"{Program.MainForm.GetPalmName()}中心"
#else
                    + Program.MainForm.FingerprintReaderUrl
#endif
                    + " 的 AddItems() 操作失败: " + ex.Message;
                return -2;
            }
            catch (Exception ex)
            {
                strError = "针对 "
#if NEWFINGER
                    + $"{Program.MainForm.GetPalmName()}中心"
#else
                    + Program.MainForm.FingerprintReaderUrl
#endif
                    + " 的 AddItems() 操作失败: " + ex.Message;
                return -1;
            }

            return 0;
        }

        public async Task<NormalResult> FingerprintGetState(
            string url,
            string strStyle)
        {
            if (string.IsNullOrEmpty(url) == true)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "尚未配置 指纹接口URL 系统参数，无法获得指纹中心状态"
                };
            }

            var channel = StartFingerprintChannel(
                url,
                out string strError);
            if (channel == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            try
            {
                return await Task.Run<NormalResult>(
                    () =>
                    {
                        return channel.Object.GetState(strStyle);
                    });
            }
            catch (Exception ex)
            {
                strError = $"针对 {url} 的 GetState() 操作失败: " + ex.Message;
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            finally
            {
                EndFingerprintChannel(channel);
            }
        }

        #endregion

        #region 其他 API

        // 获得馆藏地列表
        public int GetLocationList(
            out List<string> list,
            out string strError)
        {
            strError = "";
            list = new List<string>();
            string strOutputInfo = "";

            /*
            LibraryChannel channel = this.GetChannel();
            */
            var looping = Looping(out LibraryChannel channel,
                null);
            try
            {
                long lRet = channel.GetSystemParameter(
looping.Progress,
"circulation",
"locationTypes",
out strOutputInfo,
out strError);
                if (lRet == -1)
                    return -1;
            }
            finally
            {
                looping.Dispose();
                /*
                this.ReturnChannel(channel);
                */
            }

            // 
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strOutputInfo;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            /*
<locationTypes>
    <item canborrow="yes" itembarcodeNullable="yes">流通库</item>
    <item>阅览室</item>
    <library code="分馆1">
        <item canborrow="yes">流通库</item>
        <item>阅览室</item>
    </library>
</locationTypes>
*/

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//item");
            foreach (XmlElement node in nodes)
            {
                string strText = node.InnerText;

                // 
                string strLibraryCode = "";
                XmlNode parent = node.ParentNode;
                if (parent.Name == "library")
                {
                    strLibraryCode = DomUtil.GetAttr(parent, "code");
                }

                list.Add(string.IsNullOrEmpty(strLibraryCode) ? strText : strLibraryCode + "/" + strText);
            }

            return 1;
        }

        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetTable(
            string strRecPath,
            string strStyleList,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            string strFormat = "table";
            if (string.IsNullOrEmpty(strStyleList) == false)
                strFormat += ":" + strStyleList.Replace(",", "|");

            LibraryChannel channel = this.GetChannel();
            /*
            var looping = Looping(out LibraryChannel channel,
                null);
            */
            try
            {
            REDO:
                long lRet = channel.GetBiblioInfos(
                    null,   // looping.Progress,
                    strRecPath,
                    "",
                    new string[] { strFormat },   // formats
                    out string[] results,
                    out byte[] baNewTimestamp,
                    out strError);
                if (lRet == 0)
                    return 0;
                if (lRet == -1)
                {
                    // 2021/9/23
                    bool bHideMessageBox = true;
                    string error = strError;
                    DialogResult result = this.TryGet(() =>
                    {
                        return MessageDialog.Show(this,
                        error + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
        MessageBoxButtons.YesNoCancel,
        MessageBoxDefaultButton.Button1,
        null,
        ref bHideMessageBox,
        new string[] { "重试", "跳过", "放弃" },
        20);
                    });
                    if (result == DialogResult.Cancel)
                        return -1;
                    else if (result == System.Windows.Forms.DialogResult.No)
                        return 0;
                    else
                        goto REDO;

                    // return -1;
                }
                if (results == null || results.Length == 0)
                {
                    strError = "results error";
                    return -1;
                }
                strXml = results[0];
                return 1;
            }
            finally
            {
                // looping.Dispose();
                this.ReturnChannel(channel);
            }
        }

        public void OnLoaderPrompt(object sender, MessagePromptEventArgs e)
        {
            // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
            if (e.Actions == "yes,no,cancel")
            {
                DialogResult result = AutoCloseMessageBox.Show(this,
    e.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
    20 * 1000,
    "MyForm");
                if (result == DialogResult.Cancel)
                    e.ResultAction = "no";
                else
                    e.ResultAction = "yes";
            }
        }

        #endregion

#if NO
        protected override bool ProcessDialogKey(
Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                // MessageBox.Show(this, "MyForm Help");
                OnHelpTriggered();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }
#endif

        string _helpUrl = "";
        public string HelpUrl
        {
            get
            {
                return _helpUrl;
            }
            set
            {
                _helpUrl = value;
            }
        }

        public virtual void OnHelpTriggered()
        {
            // TODO: 如果是多个 URL，需要出现一个对话框让用户选择。每个 URL 需要跟一个小标题
            if (string.IsNullOrEmpty(this.HelpUrl) == false)
            {
                // Process.Start("IExplore.exe", this.HelpUrl);
                Process.Start(this.HelpUrl);
                return;
            }
            // TODO: 最好跳转到一个帮助目录页面
            MessageBox.Show(this, "当前没有帮助链接");
        }

        // 写入统计日志
        // parameters:
        //      prompt_action   [out] 重试/取消
        // return:
        //      -2  UID 已经存在
        //      -1  出错。注意 prompt_action 中有返回值，表明已经提示和得到了用户反馈
        //      其他  成功
        public int WriteStatisLog(
            string strSender,
            string strSubject,
            string strXml,
            LibraryChannelExtension.delegate_prompt prompt,
            out string prompt_action,
            out string strError)
        {
            prompt_action = "";
            strError = "";

            LibraryChannel channel = this.GetChannel();
            try
            {
                var message = new MessageData
                {
                    strRecipient = "!statis",
                    strSender = strSender,
                    strSubject = strSubject,
                    strMime = "text/xml",
                    strBody = strXml
                };
                MessageData[] messages = new MessageData[]
                {
                    message
                };

            REDO:
                long lRet = channel.SetMessage(
                    "send",
                    "",
                    messages,
                    out MessageData[] output_messages,
                    out strError);
                if (lRet == -1)
                {
                    // 不使用 prompt
                    if (channel.ErrorCode == ErrorCode.AlreadyExist)
                        return -2;
                    if (prompt == null)
                        return -1;
                    // TODO: 遇到出错，提示人工介入处理
                    if (prompt != null)
                    {
                        var result = prompt(channel,
                            strError + "\r\n\r\n(重试) 重试写入; (取消) 取消写入",
                            new string[] { "重试", "取消" },
                            10);
                        if (result == "重试")
                            goto REDO;
                        prompt_action = result;
                        return -1;
                    }
                }

                return (int)lRet;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        public void SetError(string type, string error)
        {
            _errorTable.SetError(type, error);
        }

        public void ClearErrors(string type)
        {
            _errorTable.SetError(type, "");
        }

        internal ErrorTable _errorTable = null;

        #region 临时文件集合

        private static readonly Object _syncRootOfTempFilenames = new Object();

        List<string> _tempfilenames = new List<string>();

        public string MemoryTempFileName(string tempFileName)
        {
            lock (_syncRootOfTempFilenames)
            {
                _tempfilenames.Add(tempFileName);
            }

            return tempFileName;
        }

        string GetTempFileName()
        {
            string strTempFilePath = Path.Combine(Program.MainForm.UserTempDir, "~res_" + Guid.NewGuid().ToString());

            lock (_syncRootOfTempFilenames)
            {
                _tempfilenames.Add(strTempFilePath);
            }

            return strTempFilePath;
        }

        void DeleteAllTempFiles()
        {
            List<string> filenames = new List<string>();
            lock (_syncRootOfTempFilenames)
            {
                filenames.AddRange(this._tempfilenames);
                this._tempfilenames.Clear();
            }

            foreach (string filename in filenames)
            {
                try
                {
                    File.Delete(filename);
                }
                catch
                {
                }
            }
        }

        #endregion


        public void OnReturnChannel(object sender, ReturnChannelEventArgs e)
        {
            if (e.Looping != null)
                EndLoop(e.Looping);

            /*
            this._stop.EndLoop();
            this._stop.OnStop -= new StopEventHandler(this.DoStop);
            */
            this.ReturnChannel(e.Channel);
        }

        public void OnGetChannel(object sender, GetChannelEventArgs e)
        {
            e.Channel = this.GetChannel();
            /*
            this._stop.OnStop += new StopEventHandler(this.DoStop);
            this._stop.BeginLoop();
            */
            Debug.Assert(e.Looping == null);
            if (StringUtil.IsInList("beginLoop", e.Style))
                e.Looping = BeginLoop(this.DoStop, "");
        }

        #region EnableControls

        // 是否正在 disabled 状态？
        public bool InDisabledState
        {
            get
            {
                return _enableControlsLevel > 0;
            }
        }

        internal int _enableControlsLevel = 0;

        // 改变变量，并返回“是否有必要更新 Enable 显示状态”
        public bool NeedUpdateEnable(bool bEnable)
        {
            if (bEnable == false)
            {
                _enableControlsLevel++;
                Debug.Assert(_enableControlsLevel >= 0, 
                    $"++ 后 _enableControlsLevel({_enableControlsLevel}) 应该 >=0");
                if (_enableControlsLevel == 1)
                    return true;
            }
            else
            {
                _enableControlsLevel--;
                Debug.Assert(_enableControlsLevel >= 0,
                    $"-- 后 _enableControlsLevel({_enableControlsLevel}) 应该 >=0");
                if (_enableControlsLevel == 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public virtual void EnableControls(bool bEnable)
        {
            if (NeedUpdateEnable(bEnable) == false)
                return;

            Exception ex = null;
            this.TryInvoke((Action)(() =>
            {
                try
                {
                    UpdateEnable(bEnable);
                }
                catch (Exception e)
                {
                    ex = e;
                }
            }));
            if (ex != null)
                throw ex;
        }

        public virtual void UpdateEnable(bool bEnable)
        {
            throw new Exception("尚未实现 UpdateEnable()");
        }

        #endregion
    }

    public class FilterHost
    {
        public MainForm MainForm = null;
        public string ID = "";
        public string ResultString = "";    // 结果字符串。用 \t 字符分隔
        public string ColumnTitles = "";    // 栏目标题。用 \t 字符分隔 2015/8/11
    }

    public class BrowseFilterDocument : FilterDocument
    {
        public FilterHost FilterHost = null;
    }
}
