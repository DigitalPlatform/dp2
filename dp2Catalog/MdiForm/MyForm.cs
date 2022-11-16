// #define USE_STOP

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CommonControl;

namespace dp2Catalog
{
    /// <summary>
    /// 通用的 MDI 子窗口基类。提供了通讯通道和窗口尺寸维持等通用设施
    /// </summary>
    public class MyForm : Form, ILoopingHost
    {
        internal int _processing = 0;    // 长操作嵌套计数器。如果大于0，表示正在处理，不希望窗口关闭

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

#if USE_STOP
        internal DigitalPlatform.Stop stop = null;
#endif

#if USE_STOP
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
#endif

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
        /// 窗口 Load 时被触发
        /// </summary>
        public virtual void OnMyFormLoad()
        {
            if (this.MainForm == null)
                return;

            this._loopingHost.StopManager = this.MainForm.stopManager;

#if USE_STOP
            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif
            {
                _floatingMessage = new FloatingMessageForm(this, true);
                // _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);

                if (this.MainForm != null)
                    this.MainForm.Move += new EventHandler(MainForm_Move);
            }
        }

        /// <summary>
        /// 窗口 Closing 时被触发
        /// </summary>
        /// <param name="e">事件参数</param>
        public virtual void OnMyFormClosing(FormClosingEventArgs e)
        {
#if USE_STOP
            if ((stop != null && stop.State == 0)    // 0 表示正在处理
    || _processing > 0)
            {
                MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。或等待长时操作完成");
                e.Cancel = true;
                return;
            }
#else
            if (HasLooping() || _processing > 0)
            {
                MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。或等待长时操作完成");
                e.Cancel = true;
                return;
            }
#endif
        }

        // 在 base.OnFormClosed(e); 之前调用
        /// <summary>
        /// 窗口 Closed 时被触发。在 base.OnFormClosed(e) 之前被调用
        /// </summary>
        public virtual void OnMyFormClosed()
        {
#if USE_STOP
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif

            if (this.MainForm != null)
                this.MainForm.Move -= new EventHandler(MainForm_Move);

            CloseFloatingMessage();
        }

        /// <summary>
        /// Form 装载事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnLoad(EventArgs e)
        {
            this.OnMyFormLoad();
            base.OnLoad(e);
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

        /// <summary>
        /// Form 激活事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnActivated(EventArgs e)
        {

            base.OnActivated(e);
        }

        #region 浮动窗口

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

        public void ClearMessage()
        {
            this._floatingMessage.Text = "";
        }

        public void AppendFloatingMessage(string strText)
        {
            this._floatingMessage.Text += strText;
        }

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

        #endregion

        #region ILoopingHost

#if OLD
        List<Looping> _loopings = new List<Looping>();
        object _syncRoot_loopings = new object();

        public Looping BeginLoop(StopEventHandler handler,
            string text)
        {
            var looping = new Looping(handler, text);
            lock (_syncRoot_loopings)
            {
                _loopings.Add(looping);
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
                    if (looping.Progress != null && looping.Progress.State == 0)
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
    }

}
