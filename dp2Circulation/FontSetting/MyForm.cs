using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;

using DigitalPlatform.CirculationClient.localhost;
using System.Web;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 通用的 MDI 子窗口基类。提供了通讯通道和窗口尺寸维持等通用设施
    /// </summary>
    public class MyForm : Form, IMdiWindow
    {

        /// <summary>
        /// 窗口是否为浮动状态
        /// </summary>
        public virtual bool Floating
        {
            get;
            set;
        }
        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = new LibraryChannel();

        /// <summary>
        /// 界面语言
        /// </summary>
        public string Lang = "zh";

        MainForm m_mainForm = null;

        /// <summary>
        /// 当前窗口所从属的框架窗口
        /// </summary>
        public MainForm MainForm
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

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
        }


        /// <summary>
        /// 窗口 Closing 时被触发
        /// </summary>
        /// <param name="e">事件参数</param>
        public virtual void OnMyFormClosing(FormClosingEventArgs e)
        {
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }
        }

        // 在 base.OnFormClosed(e); 之前调用
        /// <summary>
        /// 窗口 Closed 时被触发。在 base.OnFormClosed(e) 之前被调用
        /// </summary>
        public virtual void OnMyFormClosed()
        {
            if (this.Channel != null)
                this.Channel.Close();   // TODO: 最好限制一个时间，超过这个时间则Abort()

            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }

            if (this.MainForm != null && this.MainForm.AppInfo != null
                && Floating == false && this.SupressSizeSetting == false)
            {
                MainForm.AppInfo.SaveMdiChildFormStates(this,
                "mdi_form_state");
            }

            /*
            // 如果MDI子窗口不是MainForm刚刚准备退出时的状态，恢复它。为了记忆尺寸做准备
            if (this.WindowState != this.MainForm.MdiWindowState)
                this.WindowState = this.MainForm.MdiWindowState;
             * */
        }

        /// <summary>
        /// 通讯通道登录前被触发
        /// </summary>
        /// <param name="sender">调用者</param>
        /// <param name="e">事件参数</param>
        public virtual void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            this.MainForm.Channel_BeforeLogin(this, e);
        }

        public virtual void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            this.MainForm.Channel_AfterLogin(this, e);
        }

        internal void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

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
            }

            this.PerformLayout();
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
            this.OnMyFormClosed();

            base.OnFormClosed(e);
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
            // if (this.stop != null)
                this.MainForm.stopManager.Active(this.stop);

            this.MainForm.MenuItem_font.Enabled = true;
            this.MainForm.MenuItem_restoreDefaultFont.Enabled = true;

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

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在验证条码号 " + strBarcode + "...");
            stop.BeginLoop();

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
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

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
    }
}
