using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Reflection;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.Script;
using DigitalPlatform.Marc;
using DigitalPlatform.CommonControl;
using DigitalPlatform.MarcDom;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 通用的 MDI 子窗口基类。提供了通讯通道和窗口尺寸维持等通用设施
    /// </summary>
    public class MyForm : Form, IMdiWindow
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Channel != null)
                    this.Channel.Dispose();
                CloseFloatingMessage();
            }
            base.Dispose(disposing);
        }

        internal FloatingMessageForm _floatingMessage = null;

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

        /// <summary>
        /// 供 ShowDialog(?) 使用的窗口对象
        /// </summary>
        public IWin32Window SafeWindow
        {
            get
            {
                if (this.Visible == false)
                    return this.MainForm;
                return this;
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
        /// 界面语言
        /// </summary>
        public string Lang = "zh";

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

        internal DigitalPlatform.Stop stop = null;

        /// <summary>
        /// 进度条和停止按钮
        /// </summary>
        public Stop Progress
        {
            get
            {
                return this.stop;
            }
        }

        internal int _processing = 0;   // 是否正在进行处理中

        string FormName
        {
            get
            {
                return this.GetType().ToString();
            }
        }

        internal string FormCaption
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public virtual void EnableControls(bool bEnable)
        {
            throw new Exception("尚未实现 EnableControls() ");
        }

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

        /// <summary>
        /// 窗口 Load 时被触发
        /// </summary>
        public virtual void OnMyFormLoad()
        {
            if (this.MainForm == null)
                return;

            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            this.Channel.Idle -= Channel_Idle;
            this.Channel.Idle += Channel_Idle;

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联

            {
                _floatingMessage = new FloatingMessageForm(this, true);
                // _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);

                // _floatingMessage.Text = "test";
                //_floatingMessage.Clicked += _floatingMessage_Clicked;
                if (this.MainForm != null)
                    this.MainForm.Move -= new EventHandler(MainForm_Move);
            }
        }

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

        /// <summary>
        /// 窗口 Closing 时被触发
        /// </summary>
        /// <param name="e">事件参数</param>
        public virtual void OnMyFormClosing(FormClosingEventArgs e)
        {
            if ((stop != null && stop.State == 0)    // 0 表示正在处理
                || this._processing > 0)
            {
                MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                e.Cancel = true;
                return;
            }
        }

        // 在 base.OnFormClosed(e); 之前调用
        /// <summary>
        /// 窗口 Closed 时被触发。在 base.OnFormClosed(e) 之前被调用
        /// </summary>
        public virtual void OnMyFormClosed()
        {
            if (this.Channel != null)
            {
                this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
                this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
                this.Channel.Idle -= Channel_Idle;

                this.Channel.Close();   // TODO: 最好限制一个时间，超过这个时间则Abort()
            }

            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }

            // 原来

            if (this.MainForm != null)
                this.MainForm.Move -= new EventHandler(MainForm_Move);

#if NO
            if (_floatingMessage != null)
                _floatingMessage.Close();
#endif
            CloseFloatingMessage();
            /*
            // 如果MDI子窗口不是MainForm刚刚准备退出时的状态，恢复它。为了记忆尺寸做准备
            if (this.WindowState != this.MainForm.MdiWindowState)
                this.WindowState = this.MainForm.MdiWindowState;
             * */
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

        void MainForm_Move(object sender, EventArgs e)
        {
            if (this._floatingMessage != null)
                this._floatingMessage.OnResizeOrMove();
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

        // parameters:
        //      strStyle    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public LibraryChannel GetChannel(string strServerUrl = ".",
            string strUserName = ".",
            GetChannelStyle style = GetChannelStyle.GUI)
        {
            LibraryChannel channel = this.MainForm.GetChannel(strServerUrl, strUserName, style);
            _channelList.Add(channel);
            // TODO: 检查数组是否溢出
            return channel;
        }

        public void ReturnChannel(LibraryChannel channel)
        {
            this.MainForm.ReturnChannel(channel);
            _channelList.Remove(channel);
        }

        List<LibraryChannel> _channelList = new List<LibraryChannel>();

        internal void DoStop(object sender, StopEventArgs e)
        {
            // 兼容旧风格
            if (this.Channel != null)
                this.Channel.Abort();

            foreach (LibraryChannel channel in _channelList)
            {
                if (channel != null)
                    channel.Abort();
            }
        }

        public string CurrentUserName
        {
            get
            {
                return this.MainForm._currentUserName;
            }
        }

        // 当前用户能管辖的一个或者多个馆代码
        public string CurrentLibraryCodeList
        {
            get
            {
                return this.MainForm._currentLibraryCodeList;
            }
        }

        public string CurrentRights
        {
            get
            {
                return this.MainForm._currentUserRights;
            }
        }

        #endregion

        #region 旧风格的 Channel

        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = new LibraryChannel();

        /// <summary>
        /// 通讯通道登录前被触发
        /// </summary>
        /// <param name="sender">调用者</param>
        /// <param name="e">事件参数</param>
        public virtual void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            this.MainForm.Channel_BeforeLogin(sender, e); // 2015/11/4 原来是 this
        }

        public virtual void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            this.MainForm.Channel_AfterLogin(sender, e); // 2015/11/4 原来是 this
        }

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

        /// <summary>
        /// 开始一个循环
        /// </summary>
        /// <param name="strStyle">风格。如果包含 "halfstop"，表示停止按钮使用温和中断方式 </param>
        /// <param name="strMessage">要在状态行显示的消息文字</param>
        public void BeginLoop(string strStyle = "",
            string strMessage = "")
        {
            if (StringUtil.IsInList("halfstop", strStyle) == true)
                stop.Style = StopStyle.EnableHalfStop;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial(strMessage);
            stop.BeginLoop();
        }

        /// <summary>
        /// 结束一个循环
        /// </summary>
        public void EndLoop()
        {
            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");
            stop.HideProgress();
            stop.Style = StopStyle.None;
        }

        /// <summary>
        /// 循环是否结束？
        /// </summary>
        /// <returns>true: 循环已经结束; false: 循环尚未结束</returns>
        public bool IsStopped()
        {
            Application.DoEvents();	// 出让界面控制权

            if (this.stop != null)
            {
                if (stop.State != 0)
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
            stop.SetMessage(strMessage);
        }

        /// <summary>
        /// 为当前窗口恢复缺省字体
        /// </summary>
        public void RestoreDefaultFont()
        {
            if (this.MainForm != null)
            {
                Size oldsize = this.Size;
                if (this.MainForm.DefaultFont == null)
                    MainForm.SetControlFont(this, Control.DefaultFont);
                else
                    MainForm.SetControlFont(this, this.MainForm.DefaultFont);
                this.Size = oldsize;
            }

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetString(
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
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                {
                    // Create the FontConverter.
                    System.ComponentModel.TypeConverter converter =
                        System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                    string strFontString = converter.ConvertToString(this.Font);

                    this.MainForm.AppInfo.SetString(
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
            if (this.MainForm == null)
                return;

            if (this.MainForm != null && this.MainForm.AppInfo != null)
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
                    if (this.MainForm != null)
                    {
                        MainForm.SetControlFont(this, this.MainForm.DefaultFont);
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
            this.OnMyFormLoad();
            base.OnLoad(e);

            this.LoadFontSetting();

            // 设置窗口尺寸状态
            // 一般派生类会在 EntityForm_Load() 函数中
            /*
            this.MainForm.AppInfo.LoadMdiSize += new EventHandler(AppInfo_LoadMdiSize);
            this.MainForm.AppInfo.SaveMdiSize += new EventHandler(AppInfo_SaveMdiSize);
             * 因此这里稍后一点处理尺寸初始化是必要的
             * 
             * * */
            if (this.MainForm != null && this.MainForm.AppInfo != null
                && Floating == false && this.SupressSizeSetting == false)
            {
                this.MainForm.AppInfo.LoadMdiChildFormStates(this,
                        "mdi_form_state");
            }
        }

        /// <summary>
        /// Form 关闭事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 在这里保存。如果靠后调用，可能会遇到 base.OnFormClosed() 里面相关事件被卸掉的问题
            if (this.MainForm != null && this.MainForm.AppInfo != null
    && Floating == false && this.SupressSizeSetting == false)
            {
                MainForm.AppInfo.SaveMdiChildFormStates(this,
                    "mdi_form_state");
            }

            base.OnFormClosed(e);
            this.OnMyFormClosed();  // 这里的顺序调整过 2015/11/10

            this.DisposeFreeControls();
        }

        /// <summary>
        /// 在 FormClosing 阶段，是否要越过 this.OnMyFormClosing(e)
        /// </summary>
        public bool SupressFormClosing = false;

        /// <summary>
        /// 是否需要忽略尺寸设定的过程
        /// </summary>
        public bool SupressSizeSetting = false;

        /// <summary>
        /// Form 即将关闭事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (this.SupressFormClosing == false)
                this.OnMyFormClosing(e);
        }

        /// <summary>
        /// Form 激活事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnActivated(EventArgs e)
        {
            if (this.MainForm != null)
            {
                // if (this.stop != null)
                this.MainForm.stopManager.Active(this.stop);

                this.MainForm.MenuItem_font.Enabled = true;
                this.MainForm.MenuItem_restoreDefaultFont.Enabled = true;
            }

            base.OnActivated(e);
        }

#if NO
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
        }
#endif

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

            // EnableControls(false);

#if NO
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在验证条码号 " + strBarcode + "...");
            stop.BeginLoop();
#endif
            string strOldMessage = stop.Initial("正在验证条码号 " + strBarcode + "...");

            try
            {
                return this.MainForm.VerifyBarcode(
                    stop,
                    Channel,
                    strLibraryCodeList,
                    strBarcode,
                    EnableControls,
                    out strError);
            }
            finally
            {
#if NO
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
#endif
                stop.Initial(strOldMessage);

                // EnableControls(true);
            }
        }

        /// <summary>
        /// 向控制台输出 HTML
        /// </summary>
        /// <param name="strHtml">要输出的 HTML 字符串</param>
        public void OutputHtml(string strHtml)
        {
            this.MainForm.OperHistory.AppendHtml(strHtml);
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
            this.MainForm.OperHistory.AppendHtml("<div class='debug " + strClass + "'>" + HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>") + "</div>");
        }

        /// <summary>
        /// MDI子窗口被通知事件发生
        /// </summary>
        /// <param name="e">事件类型</param>
        public virtual void OnNotify(ParamChangedEventArgs e)
        {

        }


        #region 配置文件相关

        // 包装版本
        // 获得配置文件
        // parameters:
        //      
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetCfgFileContent(string strBiblioDbName,
            string strCfgFileName,
            out string strContent,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            return GetCfgFileContent(strBiblioDbName + "/cfgs/" + strCfgFileName,
            out strContent,
            out baOutputTimestamp,
            out strError);
        }

        int m_nInGetCfgFile = 0;    // 防止GetCfgFile()函数重入 2008/3/6

        // 获得配置文件
        // parameters:
        //      
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetCfgFileContent(string strCfgFilePath,
            out string strContent,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            strContent = "";

            if (m_nInGetCfgFile > 0)
            {
                strError = "GetCfgFile() 重入了";
                return -1;
            }

            LibraryChannel channel = this.GetChannel();
            string strOldMessage = Progress.Initial("正在下载配置文件 ...");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 1, 0);

#if NO
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在下载配置文件 ...");
            Progress.BeginLoop();
#endif

            m_nInGetCfgFile++;

            try
            {
                Progress.SetMessage("正在下载配置文件 " + strCfgFilePath + " ...");
                string strMetaData = "";
                string strOutputPath = "";

                string strStyle = "content,data,metadata,timestamp,outputpath";

                long lRet = channel.GetRes(Progress,
                    MainForm.cfgCache,
                    strCfgFilePath,
                    strStyle,
                    null,
                    out strContent,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.NotFound)
                        return 0;

                    goto ERROR1;
                }
            }
            finally
            {
#if NO
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
#endif
                Progress.Initial(strOldMessage);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                m_nInGetCfgFile--;
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 获得配置文件
        // parameters:
        //      
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetCfgFile(string strBiblioDbName,
            string strCfgFileName,
            out string strOutputFilename,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            strOutputFilename = "";

            if (m_nInGetCfgFile > 0)
            {
                strError = "GetCfgFile() 重入了";
                return -1;
            }

            LibraryChannel channel = this.GetChannel();
            string strOldMessage = Progress.Initial("正在下载配置文件 ...");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 1, 0);

#if NO
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在下载配置文件 ...");
            Progress.BeginLoop();
#endif

            m_nInGetCfgFile++;

            try
            {
                string strPath = strBiblioDbName + "/cfgs/" + strCfgFileName;

                Progress.SetMessage("正在下载配置文件 " + strPath + " ...");
                string strMetaData = "";
                string strOutputPath = "";

                string strStyle = "content,data,metadata,timestamp,outputpath";

                long lRet = channel.GetResLocalFile(Progress,
                    MainForm.cfgCache,
                    strPath,
                    strStyle,
                    out strOutputFilename,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.NotFound)
                        return 0;

                    goto ERROR1;
                }

            }
            finally
            {
#if NO
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
#endif
                Progress.Initial(strOldMessage);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                m_nInGetCfgFile--;
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 保存配置文件
        public int SaveCfgFile(string strBiblioDbName,
            string strCfgFileName,
            string strContent,
            byte[] baTimestamp,
            out string strError)
        {
            strError = "";

            LibraryChannel channel = this.GetChannel();
            string strOldMessage = Progress.Initial("正在保存配置文件 ...");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 1, 0);

#if NO
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在保存配置文件 ...");
            Progress.BeginLoop();
#endif

            try
            {
                string strPath = strBiblioDbName + "/cfgs/" + strCfgFileName;

                Progress.SetMessage("正在保存配置文件 " + strPath + " ...");

                byte[] output_timestamp = null;
                string strOutputPath = "";

                long lRet = channel.WriteRes(
                    Progress,
                    strPath,
                    strContent,
                    true,
                    "",	// style
                    baTimestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
#if NO
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
#endif
                Progress.Initial(strOldMessage);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
            }

            return 1;
        ERROR1:
            return -1;
        }

        #endregion

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
        public int BuildMarcBrowseText(
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
            host.MainForm = this.MainForm;

            BrowseFilterDocument filter = null;

            string strFilterFileName = Path.Combine(this.MainForm.DataDir, strMarcSyntax.Replace(".", "_") + "_cfgs\\marc_browse.fltx");

            int nRet = this.PrepareMarcFilter(
                host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

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
                this.MainForm.Filters.SetFilter(strFilterFileName, filter);
            }

            return 0;
        ERROR1:
            return -1;
        }

        public int PrepareMarcFilter(
FilterHost host,
string strFilterFileName,
out BrowseFilterDocument filter,
out string strError)
        {
            strError = "";

            // 看看是否有现成可用的对象
            filter = (BrowseFilterDocument)this.MainForm.Filters.GetFilter(strFilterFileName);

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
            string strLibPaths = "";

            string[] saRef2 = filter.GetRefs();

            string[] saRef = new string[saRef2.Length + saAddRef1.Length];
            Array.Copy(saRef2, saRef, saRef2.Length);
            Array.Copy(saAddRef1, 0, saRef, saRef2.Length, saAddRef1.Length);

            // 创建Script的Assembly
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                strLibPaths,
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
                if (this.MainForm != null && this.MainForm.AppInfo != null)
                    return this.MainForm.AppInfo.GetBoolean(
        "biblio_search_form",
        "search_sharebiblio",
        true);
                return false;
            }
            set
            {
                if (this.MainForm != null && this.MainForm.AppInfo != null)
                {
                    this.MainForm.AppInfo.SetBoolean(
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

        // 获得当前用户能管辖的全部馆代码
        public List<string> GetOwnerLibraryCodes()
        {
            if (Global.IsGlobalUser(this.Channel.LibraryCodeList) == true)
                return this.MainForm.GetAllLibraryCode();

            return StringUtil.SplitList(this.Channel.LibraryCodeList);
        }

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
