using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalPlatform
{
    public class StopManager
    {
        public Control OwnerControl { get; set; }

        public event DisplayMessageEventHandler OnDisplayMessage = null;
        public event AskReverseButtonStateEventHandle OnAskReverseButtonState;

        public bool bDebug = false;
        public string DebugFileName = "";

        public ReaderWriterLock m_collectionlock = new ReaderWriterLock();
        public static int m_nLockTimeout = 5000;	// 5000=5秒

        // ToolBarButton m_stopToolButton = null;	// 用于停止的工具条按钮
        // Button	m_stopButton = null;	// 用于停止的按钮
        object m_stopButton = null;	// 用于停止的工具条按钮 可以是ToolBarButton ToolStripButton Button


        /*
		StatusBar m_messageStatusBar = null;	// 状态条
		Label m_messageLabel = null;	// 显示状态的Label
        */
        object m_messageBar = null;  // StatusBar StatusStrip Label TextBox

        object m_progressBar = null;    // ProgressBar

        double progress_ratio = 1.0;

        // public List<Stop> stops = new List<Stop>();
        private List<StopGroup> _groups = new List<StopGroup>();

        // 显示表面正在显示的 active Stop 对象
        private Stop _surfaceStop = null;

        // bool bMultiple = false;

        List<object> m_reverseButtons = null;	// 当停止按钮Enabled状态变化时，需要和它状态相反的按钮对象数组
        List<int> m_reverseButtonEnableStates = null;   // 0: diabled; 1: eanbled; -1: unknown

        public StopManager()
        {
        }

        void WriteDebugInfo(string strText)
        {
            if (bDebug == false)
                return;

            WriteText(this.DebugFileName, strText);
        }

        // 写入文本文件。
        // 如果文件不存在, 会自动创建新文件
        // 如果文件已经存在，则追加在尾部。
        public static void WriteText(string strFileName,
            string strText)
        {
            StreamWriter sw = new StreamWriter(strFileName,
                true,	// append
                System.Text.Encoding.UTF8);
            sw.Write(strText);
            sw.Close();
        }

        // 创建一个 group
        public StopGroup CreateGroup(string groupName)
        {
            if (groupName == null)
                groupName = "";
            try
            {
                lock (_groups)
                {
                    var existing = _groups
        .Where(o => o.Name == groupName)
        .FirstOrDefault();
                    if (existing != null)
                        throw new Exception($"名为 '{groupName}' 的 Group 对象已经存在");

                    var group = new StopGroup(groupName);
                    _groups.Add(group);
                    return group;
                }
            }
            finally
            {
                UpdateDisplay();
            }
        }

        // 删除一个 group
        public bool DeleteGroup(string groupName)
        {
            bool changed = false;
            try
            {
                lock (_groups)
                {
                    var group = _groups
                        .Where(o => o.Name == groupName)
                        .FirstOrDefault();
                    if (group == null)
                        return false;
                    _groups.Remove(group);
                    changed = true;
                    return true;
                }
            }
            finally
            {
                if (changed)
                    UpdateDisplay();
            }
        }

        public StopGroup ActivateGroup(string groupName)
        {
            bool found = false;
            try
            {
                lock (_groups)
                {
                    var existing = _groups
                        .Where(o => o.Name == groupName)
                        .FirstOrDefault();
                    if (existing == null)
                        return null;
                    _groups.Remove(existing);
                    _groups.Add(existing);  // 注: 在 _group 数组中靠后的元素，在显示角度更“顶层”
                    // TODO: 判断一下 existing 是否已经是 active 状态
                    found = true;
                    return existing;
                }
            }
            finally
            {
                if (found)
                    UpdateDisplay();
            }
        }

        /*
        public StopGroup GetActiveGroup()
        {
            lock (_groups)
            {
                return _groups.LastOrDefault();
            }
        }
        */

        // 查找一个 group
        public StopGroup FindGroup(string groupName)
        {
            lock (_groups)
            {
                return _groups
                    .Where(o => o.Name == groupName)
                    .FirstOrDefault();
            }
        }

        // 2007/8/2
        public void LinkReverseButtons(List<object> buttons)
        {
            // 检查
            for (int i = 0; i < buttons.Count; i++)
            {
                object button = buttons[i];

                if ((button is ToolBarButton)
                    || (button is Button)
                    || (button is ToolStripButton))
                {
                }
                else
                {
                    throw new Exception("button参数只能用ToolBarButton ToolStripButton或Button类型");
                }
            }

            if (m_reverseButtons == null
                || m_reverseButtons.Count == 0)
                m_reverseButtons = buttons;
            else
            {
                m_reverseButtons.AddRange(buttons);
            }
        }

        // 2007/8/2
        public void UnlinkReverseButtons(List<object> buttons)
        {
            // 检查
            for (int i = 0; i < buttons.Count; i++)
            {
                object button = buttons[i];

                if ((button is ToolBarButton)
                    || (button is Button)
                    || (button is ToolStripButton))
                {
                }
                else
                {
                    throw new Exception("button参数只能用ToolBarButton ToolStripButton或Button类型");
                }
            }

            if (m_reverseButtons == null
                || m_reverseButtons.Count == 0)
                return;

            for (int i = 0; i < buttons.Count; i++)
            {
                object button = buttons[i];

                m_reverseButtons.Remove(button);
            }
        }

        public void LinkReverseButton(object button)
        {
            if ((button is ToolBarButton)
                || (button is Button)
                || (button is ToolStripButton))
            {
            }
            else
            {
                throw new Exception("button参数只能用ToolBarButton ToolStripButton或Button类型");
            }

            if (m_reverseButtons == null)
                m_reverseButtons = new List<object>();

            m_reverseButtons.Add(button);
        }

        public void UnlinkReverseButton(object button)
        {
            if ((button is ToolBarButton)
        || (button is Button)
        || (button is ToolStripButton))
            {
            }
            else
            {
                throw new Exception("button参数只能用ToolBarButton ToolStripButton或Button类型");
            }

            if (m_reverseButtons == null)
                return;

            m_reverseButtons.Remove(button);
        }

        void InternalSetMessage(string strMessage)
        {
            if (m_messageBar is StatusStrip)
            {
                // ((StatusStrip)m_messageBar).Text = strMessage;
                Safe_SetStatusStripText(((StatusStrip)m_messageBar), strMessage);
            }
#if NO
            else if (m_messageBar is StatusBar)
            {
                // StatusBar 派生自 Control
                StatusBar statusbar = ((StatusBar)m_messageBar);

                Safe_SetStatusBarText(statusbar, strMessage);
            }
            else if (m_messageBar is Label)
            {
                // ((Label)m_messageBar).Text = strMessage;
                Safe_SetLabelText(((Label)m_messageBar), strMessage);
            }
#endif
            else if (m_messageBar is Control)
            {
                Safe_SetTextBoxText(((Control)m_messageBar), strMessage);
            }
            else if (m_messageBar is ToolStripStatusLabel)
            {
                // TODO: ToolStripStatusLabel 也是继承自 ToolStripItem。此处代码可以删除了

                // ((ToolStripStatusLabel)m_messageBar).Text = strMessage;
                Safe_SetToolStripStatusLabelText((ToolStripStatusLabel)m_messageBar,
                    strMessage);
            }
            else if (m_messageBar is ToolStripItem)
            {
                Safe_SetToolStripItemText((ToolStripItem)m_messageBar,
                    strMessage);
            }

            if (this.OnDisplayMessage != null)
            {
                DisplayMessageEventArgs e = new DisplayMessageEventArgs();
                e.Message = strMessage;
                this.OnDisplayMessage(this, e);
            }
        }

        // 最近一次同步过 Min Max 到 ProgressBar 的 Stop 对象
        Stop _controlStop = null;

        // parameters:
        //      stop    用来检查 Stop 的 Min Max 值是否和 ProgressBar 一致。
        //              如果不一致则需要立即重新设置 ProgressBar 的 Min Max
        void InternalSetProgressBar(
            Stop stop,
            long lStart,
            long lEnd,
            long lValue)
        {
            Debug.Assert(this.OwnerControl != null);
            TryInvoke(this.OwnerControl, () =>
            {
                if (lStart == lEnd
                && lStart == lValue
                && lValue == -1)
                {
                    _controlStop = null;
                }
                else if (stop != _controlStop)
                {
                    /*
                    Debug.Assert(stop.ProgressMin != -1);
                    Debug.Assert(stop.ProgressMax != -1);

                    if (stop.ProgressValue < stop.ProgressMin
                        || stop.ProgressValue > stop.ProgressMax)
                        throw new Exception($"ProgressValue 越过 ProgressMin 和 ProgressMax 范围");
                    */
                    // 补一次设置 Min Max Value
                    _internalSetProgressBar(stop.ProgressMin,
                        stop.ProgressMax,
                        stop.ProgressValue);
                }

                _internalSetProgressBar(
                    lStart, lEnd, lValue);

                _controlStop = stop;
            });
        }

        void _internalSetProgressBar(
    long lStart,
    long lEnd,
    long lValue)
        {
            if (m_progressBar is ProgressBar)
            {
                ProgressBar progressbar = ((ProgressBar)m_progressBar);

                Safe_SetProgressBar(progressbar,
                    lStart, lEnd, lValue);
            }

            if (m_progressBar is ToolStripProgressBar)
            {
                ToolStripProgressBar progressbar = ((ToolStripProgressBar)m_progressBar);

                Safe_SetProgressBar(progressbar,
                    lStart, lEnd, lValue);
            }
        }

        #region StatusStrip

        // 线程安全版本
        void Safe_SetStatusStripText(StatusStrip status_strip,
            string strText)
        {
            TryInvoke(status_strip, () =>
            {
                status_strip.Text = strText;
                status_strip.Update();
            });

#if REMOVED
            if (status_strip.Parent != null && status_strip.Parent.InvokeRequired)
            {
                Delegate_SetStatusStripText d = new Delegate_SetStatusStripText(SetStatusStripText);
                return (string)status_strip.Parent.Invoke(d, new object[] { status_strip, strText });
            }
            else
            {
                string strOldText = status_strip.Text;

                status_strip.Text = strText;

                status_strip.Update();

                return strOldText;
            }
#endif
        }

#if REMOVED
        delegate string Delegate_SetStatusStripText(StatusStrip status_strip,
            string strText);

        string SetStatusStripText(StatusStrip status_strip,
            string strText)
        {
            string strOldText = status_strip.Text;

            status_strip.Text = strText;

            status_strip.Update();

            return strOldText;
        }
#endif

        #endregion

        void Safe_SetToolStripItemText(ToolStripItem status_strip,
string strText)
        {
            var parent = EnsureParent(status_strip.Owner);
            if (parent == null
                /*|| status_strip.Owner.Parent == null*/)
                return /*null*/;

            TryInvoke(parent, () =>
            {
                status_strip.Text = strText;
                status_strip.Owner?.Update();
            });

#if REMOVED
            if (status_strip.Owner != null
                && status_strip.Owner.Parent != null
                && status_strip.Owner.Parent.InvokeRequired)
            {
                return (string)status_strip.Owner.Parent.Invoke(new Func<ToolStripItem, string, string>(Safe_SetToolStripItemText), status_strip, strText);
            }
            else
            {
                string strOldText = status_strip.Text;
                status_strip.Text = strText;
                return strOldText;
            }
#endif
        }

        #region ToolStripStatusLabel

        static void TryInvoke(Control control, Action method)
        {
            if (control != null && control.InvokeRequired)
                control.Invoke((Action)(method));
            else
                method.Invoke();
        }

        // 线程安全版本
        void Safe_SetToolStripStatusLabelText(ToolStripStatusLabel label,
            string strText)
        {
            var parent = EnsureParent(label.Owner);
            if (parent == null)
                return/* ""*/;

            TryInvoke(parent, () =>
            {
                // string strOldText = label.Text;

                label.Text = string.IsNullOrEmpty(strText) ? " " : strText; // 在某些情况下，空内容和非空内容会导致 label 高度变化，进而引起整个框架窗口刷新。为避免此情况，特意在空的时候设置一个空格字符。2017/4/24
                label.Owner?.Update();    // 2022/11/13 迫使文本立即显示出来
                // return strOldText;
            });
        }

#if REMOVED
        // 线程安全版本
        string Safe_SetToolStripStatusLabelText(ToolStripStatusLabel label,
            string strText)
        {
            if (label.Owner == null)
                return "";

            if (label.Owner.InvokeRequired)
            {
                Delegate_SetToolStripStatusLabelText d = new Delegate_SetToolStripStatusLabelText(SetToolStripStatusLabelText);
                return (string)label.Owner.Invoke(d, new object[] { label, strText });
            }
            else
            {
                string strOldText = label.Text;

                label.Text = string.IsNullOrEmpty(strText) ? " " : strText; // 在某些情况下，空内容和非空内容会导致 label 高度变化，进而引起整个框架窗口刷新。为避免此情况，特意在空的时候设置一个空格字符。2017/4/24

                label.Owner.Update();    // 2022/11/13 迫使文本立即显示出来
                return strOldText;
            }
        }

        delegate string Delegate_SetToolStripStatusLabelText(ToolStripStatusLabel label,
            string strText);

        string SetToolStripStatusLabelText(ToolStripStatusLabel label,
            string strText)
        {
            string strOldText = label.Text;

            label.Text = strText;

            label.Owner.Update();
            return strOldText;
        }
#endif

        #endregion

        #region TextBox

        Control EnsureParent(Control parent)
        {
            if (parent != null)
                return parent;
            return this.OwnerControl;
        }

        // 线程安全版本
        void Safe_SetTextBoxText(Control textbox,
            string strText)
        {
            var parent = EnsureParent(textbox.Parent);
            if (parent != null)
                TryInvoke(parent, () =>
                {
                    textbox.Text = strText;
                    textbox.Update();
                });

#if REMOVED
            if (textbox.Parent != null && textbox.Parent.InvokeRequired)
            {
                Delegate_SetTextBoxText d = new Delegate_SetTextBoxText(SetTextBoxText);
                return (string)textbox.Parent.Invoke(d, new object[] { textbox, strText });
            }
            else
            {
                string strOldText = textbox.Text;

                textbox.Text = strText;
                textbox.Update();
                return strOldText;
            }
#endif
        }

#if REMOVED
        delegate string Delegate_SetTextBoxText(Control textbox,
            string strText);

        string SetTextBoxText(Control textbox,
            string strText)
        {
            string strOldText = textbox.Text;

            textbox.Text = strText;
            textbox.Update();

            return strOldText;
        }
#endif

        #endregion

        #region ProgressBar

        // 线程安全版本
        void Safe_SetProgressBar(ProgressBar progressbar,
            long lStart,
            long lEnd,
            long lValue)
        {
            TryInvoke(progressbar, () =>
            {
                if (lEnd == -1 && lStart == -1 && lValue == -1)
                {
                    if (progressbar.Visible != false)   // 2008/3/17
                        progressbar.Visible = false;
                    if (lValue == -1)
                        return;
                }
                else
                {
                    if (progressbar.Visible == false)
                        progressbar.Visible = true;
                }

                if (lEnd >= 0)
                {
                    SetRatio(lEnd);
                    progressbar.Maximum = (int)(this.progress_ratio * (double)lEnd);
                }
                if (lStart >= 0)
                    progressbar.Minimum = (int)(this.progress_ratio * (double)lStart);

                if (lValue >= 0)
                    progressbar.Value = (int)(this.progress_ratio * (double)lValue);
            });

#if REMOVED
            if (progressbar.Parent != null && progressbar.Parent.InvokeRequired)
            {
                Delegate_SetProgressBar d = new Delegate_SetProgressBar(SetProgressBar);
                progressbar.Parent.Invoke(d, new object[] { progressbar, lStart, lEnd, lValue });
            }
            else
            {
                if (lEnd == -1 && lStart == -1 && lValue == -1)
                {
                    if (progressbar.Visible != false)   // 2008/3/17
                        progressbar.Visible = false;
                    if (lValue == -1)
                        return;
                }
                else
                {
                    if (progressbar.Visible == false)
                        progressbar.Visible = true;
                }

                if (lEnd >= 0)
                {
                    SetRatio(lEnd);
                    progressbar.Maximum = (int)(this.progress_ratio * (double)lEnd);
                }
                if (lStart >= 0)
                    progressbar.Minimum = (int)(this.progress_ratio * (double)lStart);

                if (lValue >= 0)
                    progressbar.Value = (int)(this.progress_ratio * (double)lValue);
            }
#endif
        }

        void SetRatio(long lEnd)
        {
            this.progress_ratio = (double)64000 / (double)lEnd;
            if (this.progress_ratio > 1.0)
                this.progress_ratio = 1.0;
        }

#if REMOVED
        delegate void Delegate_SetProgressBar(ProgressBar progressbar,
            long lStart, long lEnd, long lValue);

        void SetProgressBar(ProgressBar progressbar,
            long lStart, long lEnd, long lValue)
        {
            if (lEnd == -1 && lStart == -1 && lValue == -1)
            {
                if (progressbar.Visible != false)   // 2008/3/17
                    progressbar.Visible = false;
                if (lValue == -1)
                    return;
            }
            else
            {
                if (progressbar.Visible == false)
                    progressbar.Visible = true;
            }

            if (lEnd >= 0)
            {
                SetRatio(lEnd);
                progressbar.Maximum = (int)(this.progress_ratio * (double)lEnd);
            }
            if (lStart >= 0)
                progressbar.Minimum = (int)(this.progress_ratio * (double)lStart);

            if (lValue >= 0)
                progressbar.Value = (int)(this.progress_ratio * (double)lValue);
        }
#endif

        #endregion

        #region ToolStripProgressBar

        // 线程安全版本
        void Safe_SetProgressBar(ToolStripProgressBar progressbar,
            long lStart,
            long lEnd,
            long lValue)
        {
            var parent = EnsureParent(progressbar.Owner);
            if (parent == null)
                return;

            TryInvoke(parent, () =>
            {
                if (lEnd == -1 && lStart == -1 && lValue == -1)
                {
                    if (progressbar.Visible != false)   // 2008/3/17
                        progressbar.Visible = false;

                    if (lValue == -1)
                        return;
                }
                else
                {
                    if (progressbar.Visible == false)
                        progressbar.Visible = true;
                }

                if (lEnd >= 0)
                {
                    SetRatio(lEnd);
                    progressbar.Maximum = (int)(this.progress_ratio * (double)lEnd);
                }
                if (lStart >= 0)
                    progressbar.Minimum = (int)(this.progress_ratio * (double)lStart);

                if (lValue >= 0)
                    progressbar.Value = (int)(this.progress_ratio * (double)lValue);
            });

#if REMOVED
            if (progressbar.Owner.InvokeRequired)
            {
                Delegate_SetToolStrupProgressBar d = new Delegate_SetToolStrupProgressBar(SetProgressBar);
                progressbar.Owner.Invoke(d, new object[] { progressbar, lStart, lEnd, lValue });
            }
            else
            {
                if (lEnd == -1 && lStart == -1 && lValue == -1)
                {
                    if (progressbar.Visible != false)   // 2008/3/17
                        progressbar.Visible = false;

                    if (lValue == -1)
                        return;
                }
                else
                {
                    if (progressbar.Visible == false)
                        progressbar.Visible = true;
                }

                if (lEnd >= 0)
                {
                    SetRatio(lEnd);
                    progressbar.Maximum = (int)(this.progress_ratio * (double)lEnd);
                }
                if (lStart >= 0)
                    progressbar.Minimum = (int)(this.progress_ratio * (double)lStart);

                if (lValue >= 0)
                    progressbar.Value = (int)(this.progress_ratio * (double)lValue);
            }
#endif
        }

#if REMOVED
        delegate void Delegate_SetToolStrupProgressBar(ToolStripProgressBar progressbar,
            long lStart, long lEnd, long lValue);

        void SetProgressBar(ToolStripProgressBar progressbar,
            long lStart, long lEnd, long lValue)
        {
            if (lEnd == -1 && lStart == -1 && lValue == -1)
            {
                if (progressbar.Visible != false)   // 2008/3/17
                    progressbar.Visible = false;
                if (lValue == -1)
                    return;
            }
            else
            {
                if (progressbar.Visible == false)
                    progressbar.Visible = true;
            }

            if (lEnd >= 0)
            {
                SetRatio(lEnd);
                progressbar.Maximum = (int)(this.progress_ratio * (double)lEnd);
            }
            if (lStart >= 0)
                progressbar.Minimum = (int)(this.progress_ratio * (double)lStart);

            if (lValue >= 0)
                progressbar.Value = (int)(this.progress_ratio * (double)lValue);
        }
#endif

        #endregion

#if NO
        #region StatusBar

        // 线程安全版本
        string Safe_SetStatusBarText(StatusBar statusbar,
            string strText)
        {
            if (statusbar.Parent != null && statusbar.Parent.InvokeRequired)
            {
                Delegate_SetStatusBarText d = new Delegate_SetStatusBarText(SetStatusBarText);
                return (string)statusbar.Parent.Invoke(d, new object[] { statusbar, strText });
            }
            else
            {
                string strOldText = statusbar.Text;

                statusbar.Text = strText;
                statusbar.Update();

                return strOldText;
            }
        }

        delegate string Delegate_SetStatusBarText(StatusBar statusbar,
            string strText);

        string SetStatusBarText(StatusBar statusbar,
            string strText)
        {
            string strOldText = statusbar.Text;

            statusbar.Text = strText;
            statusbar.Update();

            return strOldText;
        }

        #endregion
#endif

        // 改变stop按钮Enabled状态。不记忆以前的状态
        void EnableStopButtons(bool bEnabled)
        {
            if (this.m_stopButton == null)
                return;

            if (this.m_stopButton is Button)
            {
                // ((Button)this.m_stopButton).Enabled = bEnabled;

                Button button = ((Button)this.m_stopButton);

                Safe_EnableButton(button, bEnabled);
            }
            else if (this.m_stopButton is ToolBarButton)
            {
                // ((ToolBarButton)this.m_stopButton).Enabled = bEnabled;

                ToolBarButton button = ((ToolBarButton)this.m_stopButton);

                Safe_EnableToolBarButton(button, bEnabled);
                /*
                if (button.Parent.InvokeRequired)
                {
                    Delegate_SetToolBarButtonEnable d = new Delegate_SetToolBarButtonEnable(SetToolBarButtonEnable);
                    button.Parent.Invoke(d, new object[] { button, bEnabled });
                }
                else
                {
                    button.Enabled = bEnabled;
                }
                 * */


            }
            else if (this.m_stopButton is ToolStripButton)
            {
                // ((ToolStripButton)this.m_stopButton).Enabled = bEnabled;

                ToolStripButton button = (ToolStripButton)this.m_stopButton;

                Safe_EnableToolStripButton(button, bEnabled);
                /*
                if (button.Owner.InvokeRequired)
                {
                    Delegate_SetToolStripButtonEnable d = new Delegate_SetToolStripButtonEnable(SetToolStripButtonEnable);
                    button.Owner.Invoke(d, new object[] { button, bEnabled });
                }
                else
                {
                    button.Enabled = bEnabled;
                }
                 * */
            }

        }

        #region Button

        // 线程安全调用
        static void Safe_EnableButton(Button button,
            bool bEnabled)
        {
            TryInvoke(button, () =>
            {
                button.Enabled = bEnabled;
            });

#if REMOVED
            if (button.Parent != null && button.Parent.InvokeRequired)
            {
                Delegate_SetButtonEnable d = new Delegate_SetButtonEnable(SetButtonEnable);
                return (bool)button.Parent.Invoke(d, new object[] { button, bEnabled });
            }
            else
            {
                bool bOldState = button.Enabled;

                button.Enabled = bEnabled;

                return bOldState;
            }
#endif
        }

#if REMOVED
        delegate bool Delegate_SetButtonEnable(Button button,
            bool bEnabled);

        static bool SetButtonEnable(Button button,
            bool bEnabled)
        {
            bool bOldState = button.Enabled;

            button.Enabled = bEnabled;

            button.Parent.Update();

            return bOldState;
        }
#endif

        #endregion

        #region ToolBarButton

        // 线程安全调用
        void Safe_EnableToolBarButton(ToolBarButton button,
            bool bEnabled)
        {
            var parent = EnsureParent(button.Parent);
            if (parent == null)
                return;
            TryInvoke(parent, () =>
            {
                button.Enabled = bEnabled;
            });

#if REMOVED
            if (button.Parent != null && button.Parent.InvokeRequired)
            {
                Delegate_SetToolBarButtonEnable d = new Delegate_SetToolBarButtonEnable(SetToolBarButtonEnable);
                return (bool)button.Parent.Invoke(d, new object[] { button, bEnabled });
            }
            else
            {
                bool bOldState = button.Enabled;

                button.Enabled = bEnabled;

                return bOldState;
            }
#endif
        }

#if REMOVED
        delegate bool Delegate_SetToolBarButtonEnable(ToolBarButton button,
            bool bEnabled);

        static bool SetToolBarButtonEnable(ToolBarButton button,
            bool bEnabled)
        {
            bool bOldState = button.Enabled;

            button.Enabled = bEnabled;

            button.Parent.Update();

            return bOldState;
        }
#endif

        #endregion

        #region ToolStripButton

        // 线程安全调用
        void Safe_EnableToolStripButton(ToolStripButton button,
            bool bEnabled)
        {
            var parent = EnsureParent(button.Owner);
            // 2014/12/26
            if (parent == null)
                return /*false*/;

            TryInvoke(parent, () =>
            {
                button.Enabled = bEnabled;
            });

#if REMOVED
            if (button.Owner.InvokeRequired)
            {
                Delegate_SetToolStripButtonEnable d = new Delegate_SetToolStripButtonEnable(SetToolStripButtonEnable);
                return (bool)button.Owner.Invoke(d, new object[] { button, bEnabled });
            }
            else
            {
                bool bOldState = button.Enabled;
                button.Enabled = bEnabled;
                return bOldState;
            }
#endif
        }

        delegate bool Delegate_SetToolStripButtonEnable(ToolStripButton button,
            bool bEnabled);

        static bool SetToolStripButtonEnable(ToolStripButton button,
            bool bEnabled)
        {
            bool bOldState = button.Enabled;

            button.Enabled = bEnabled;

            button.Owner.Update();

            return bOldState;
        }

        #endregion

        // 整体改变reverse_buttons的Enabled状态。
        // 注意，在false时，是要改变为disabled状态；而true时，则是要恢复原来记忆(disable前)的状态
        // parameters:
        //      bEnabled    true表示希望恢复按钮原来状态；false希望disable按钮
        //      parts   SaveEnabledState希望先保存按钮原来的值；RestoreEnabledState表示要恢复按钮原来的值
        void EnableReverseButtons(bool bEnabled,
            StateParts parts)
        {
            if (m_reverseButtons == null)
                return;

            if (m_reverseButtonEnableStates == null)
                m_reverseButtonEnableStates = new List<int>();

            /*
            if ((parts & StateParts.All) != 0)
                throw new Exception("StateParts枚举中除了SaveEnabledState和RestoreEnabledState值外，其他值对于本函数没有意义");
             * */

            bool bSave = false;
            if ((parts & StateParts.SaveEnabledState) != 0)
                bSave = true;

            bool bRestore = false;
            if ((parts & StateParts.RestoreEnabledState) != 0)
                bRestore = true;


            // 保证两个数组的大小一致
            while (m_reverseButtonEnableStates.Count < m_reverseButtons.Count)
            {
                m_reverseButtonEnableStates.Add(-1);
            }

            for (int i = 0; i < m_reverseButtons.Count; i++)
            {
                int nOldState = m_reverseButtonEnableStates[i];

                bool bEnableResult = bEnabled;
                // TODO: 状态不明的时候，可以通过事件从外部索取信息
                // 如果bEnabled要求true，而旧状态不明，那就询问
                if (nOldState == -1 && bEnabled == true)
                {
                    if (this.OnAskReverseButtonState != null)
                    {
                        AskReverseButtonStateEventArgs e = new AskReverseButtonStateEventArgs();
                        e.EnableQuestion = true;
                        e.Button = m_reverseButtons[i];
                        this.OnAskReverseButtonState(this, e);
                        bEnableResult = e.EnableResult;
                    }
                }

                bool bOldState = (nOldState == 1 ? true : false);

                object button = m_reverseButtons[i];
                if (button is Button)
                {
                    bOldState = ((Button)button).Enabled;
                    Safe_EnableButton((Button)button,
                        (bRestore == true ? bOldState : bEnableResult));

                }
                else if (button is ToolBarButton)
                {
                    /*bOldState = */
                    Safe_EnableToolBarButton(((ToolBarButton)button),
        bRestore == true ? bOldState : bEnableResult);
                }
                else if (button is ToolStripButton)
                {
                    /*bOldState = */
                    Safe_EnableToolStripButton(((ToolStripButton)button),
        bRestore == true ? bOldState : bEnableResult);
                }

                // 主动disable前，记忆以前的状态
                if (bEnabled == false
                    && bSave == true)
                    m_reverseButtonEnableStates[i] = (bOldState == true ? 1 : 0);
            }
        }

        // 为了兼容以前的脚本中的调用
        public void Initial(
    object button,
    object statusBar,
    object progressBar)
        {
            Initial(Application.OpenForms[0],
                button,
                statusBar,
                progressBar);
        }

        // 初始化一个按钮,在父窗口load时调
        // locks: 集合写锁
        public void Initial(
            Control owner,
            object button,
            object statusBar,
            object progressBar)
        {
            this.OwnerControl = owner;
            WriteDebugInfo("collection write lock 1\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                m_stopButton = button;

                AddStopButtonEvent(button);
                /*
				if (m_stopButton != null)
					m_strTipsSave = m_stopToolButton.ToolTipText;
				else
					m_strTipsSave = "";
                 */

                m_messageBar = statusBar;
                m_progressBar = progressBar;

                // 2023/4/4
                EnableStopButtons(false);
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 1\r\n");
            }
        }

        // 2023/4/11
        void AddStopButtonEvent(object button)
        {
            if (button is Control)
            {
                var control = (Control)button;

                control.Click += (o, e) =>
                {
                    if (Control.ModifierKeys == Keys.Control)
                        this.DoStopAll(null);    // 2012/3/25
                    else
                        this.DoStopActive();
                };
            }
            else if (button is ToolBarButton)
            {
                var control = (ToolBarButton)button;
                // 没有 control.Click
            }
            else if (button is ToolStripItem)
            {
                var control = (ToolStripItem)button;
                control.Click += (o, e) =>
                {
                    if (Control.ModifierKeys == Keys.Control)
                        this.DoStopAll(null);
                    else
                        this.DoStopActive();
                };
            }
        }

        public void RegisterStop(Stop stop,
            string groupName = "")
        {
            var group = FindGroup(groupName);

            // 临时补充创建一个名字为空的 group
            if (group == null && string.IsNullOrEmpty(groupName))
                group = this.CreateGroup("");

            if (group == null)
            {
                throw new Exception($"名为 '{groupName}' 的 group 对象不存在，注册 Stop 对象失败");
            }

            group.Add(stop);
        }

        public void UnregisterStop(Stop stop)
        {
            var group = stop.Group;
            if (group == null)
                throw new Exception($"Stop 对象 '{stop.Name}' 尚未加入过 Group，注销 Stop 对象失败");
            group.Remove(stop);
        }

#if REMOVED

        // 加入一个Stop对象。加入在非活动位置
        // locks: 集合写锁
        public void Add(Stop stop)
        {
            WriteDebugInfo("collection write lock 2\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                // stops.Add(stop);
                stops.Insert(0, stop);  // 修改后的效果，就不会改变激活的stop对象了
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 2\r\n");
            }

            //SetToolTipText();
        }

        string DebugInfo()
        {
            return Convert.ToString(stops.Count);
        }

        // 移走一个Stop对象
        // locks: 集合写锁
        public void Remove(Stop stop, bool bChangeState = true)
        {
            WriteDebugInfo("collection write lock 3\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                stops.Remove(stop);

                if (bChangeState == true)
                    ChangeState(null,
                        StateParts.All,
                        false); // false表示不加集合锁
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 3\r\n");
            }

            //SetToolTipText();
        }

#endif
        // 获得当前处于显示状态的 Stop
        // 注：如果顶层 group 没有 active stop，则继续找次顶层的
        Stop GetSurfaceStop()
        {
            lock (_groups)
            {
                Stop result = null;
                foreach (var group in _groups)
                {
                    var stop = group.GetActiveStop();
                    if (stop != null)
                        result = stop;
                }

                return result;
            }
        }


        // 刷新工具条、状态行显示。用于 active group 切换后
        public void UpdateDisplay()
        {
            /*
            var activeGroup = GetActiveGroup();
            if (activeGroup == null)
            {
                ClearDisplay();
                return;
            }
            */
            var active_stop = this.GetSurfaceStop();
            if (active_stop == null)
            {
                ClearDisplay();
                return;
            }

            // 停止按钮
            // var stop_button_enabled = activeGroup.GetStopButtonEnableState();
            var stop_button_enabled = active_stop.Group.GetStopButtonEnableState();
            EnableStopButtons(stop_button_enabled);
            EnableReverseButtons(!stop_button_enabled, StateParts.None);

            // var active_stop = activeGroup.GetActiveStop();
            if (active_stop == null)
            {
                ClearDisplay();
                return;
            }

            _surfaceStop = active_stop;
            // 状态行文本
            InternalSetMessage(active_stop.DisplayMessage);

            // 状态行 ProgressBar
            /*
            if (active_stop.ProgressMin == -1)
                throw new Exception("ProgressMin 尚未初始化");
            if (active_stop.ProgressMax == -1)
                throw new Exception("ProgressMax 尚未初始化");
            if (active_stop.ProgressValue < active_stop.ProgressMin
                || active_stop.ProgressValue > active_stop.ProgressMax)
                throw new Exception($"ProgressValue 越过 ProgressMin 和 ProgressMax 范围");
            */
            InternalSetProgressBar(
                active_stop,
                active_stop.ProgressMin,
                active_stop.ProgressMax,
                active_stop.ProgressValue);

            // 工具条要综合 active group 中所有 Stop 对象的状态，决定停止按钮和其它按钮的状态

            // 状态行显示 active group 中最顶层 Stop 对象的 progress 和 text

            /* 1) 当 stop.BeginLoop() 和 stop.EndLoop() 调用时
             *      停止按钮要综合 active group 中的 looping Stop 对象状态，
             *      让按钮 Enabled 发生变化
             * 2) stop.SetMessage() 调用时，Stop 对象内部 message 成员要记住最新字符串，
             *      如果 Stop 对象处在 active group 并且处在最顶层 looping 状态，则会刷新状态行文本显示
             *      注意，looping level 为 0 的 Stop 对象即便是最顶层也是透明的，不影响显示
             * 3) stop.SetProgressRange() 和 stop.SetProgressValue() 调用时,
             *      Stop 对象内部的三个 progress 相关值得要记住最新状态，
             *      如果 Stop 对象处在 active group 并且处在最顶层 looping 状态，则会刷新状态行的公共 progress 显示
             *      注意，looping level 为 0 的 Stop 对象即便是最顶层也是透明的，不影响显示
             * 4) stop.Register() 时，如果注册到一个 active group，它会处在最顶层位置，
             *      但它的 looping level 为 0，所以相当于透明的，不影响显示
             * 5) stop.Unregister() 时，如果是从一个 active group 注销，有可能会改变公共显示，
             *      但一般它早已 EndLoop(), looping level 为 0，所以其实不会影响显示
             * 6) 首次决定显示公共显示区的时候，要从 active group 中综合提取停止按钮状态，
             *      从最顶层 looping Stop 对象获得文本和进度刷新显示
             *      如果 active group 为 null，则显示初始空白状态
             * 7) 状态行的 Progress 要优化速度，保持一个上次用过的 Stop 对象指针，反复刷新的时候，
             *      如无必要不修改 Min 和 Max 值
             * 8) 工具条的按钮，enable 状态如果没有必要改变注意不要发生显示抖动
             * 9) 将来可以考虑给工具条停止按钮加上一个文字，表示“现在共有多少个 Stop 对象正在 looping”
             * 
             * 
             * */
        }

        void ClearDisplay()
        {
            _surfaceStop = null;
            // 停止按钮 Diabled，其它 Linked 按钮 Enabled
            EnableStopButtons(false);
            EnableReverseButtons(true, StateParts.None);
            // 状态行文本清空
            InternalSetMessage("");
            // 状态行 ProgressBar 隐藏
            InternalSetProgressBar(null, -1, -1, -1);
        }

        // 新的名字
        public bool Activate(Stop stop)
        {
            return Active(stop);
        }

#if REMOVED
        // 激活按钮
        // 因为StopManager管辖很多Stop对象, 当一个Stop对象需要处于焦点状态,
        // 需要在StopManager内部数组中把这个Stop对象挪到队列尾部, 也就是相当于堆栈顶部
        // 的效果。然后, 把StopManager所关联的button设置为和Stop对象当前状态对应的
        // Enabled或者Disabled状态, 因为只有这样用户才能触发按钮。
        // StopManager管理了很多Stop状态，Active()函数相当于把某个Stop状态翻到可见的顶部。
        // locks: 集合写锁
#endif
        // 把一个 Stop 对象拔高到所在 Group 的顶层 Stop 位置
        // 由于多个 Group 存在，此操作可能会影响显示，可能也不会影响显示
        public bool Active(Stop stop)
        {
            if (stop == null)
                return false;   // TODO: 是否要抛出异常？
            /*
             * 如果 stop.Group 是 active group，会改变 _surfaceStop 值
             * 
             * 
             * */
            // 已经是顶层 Stop 对象了
            if (stop == _surfaceStop)
                return false;
            // 先在所属的 group 中激活
            stop.MoveToTop();
            /*
            // 如果 Stop 对象属于 active group，则把它设置为顶层 Stop
            if (stop.Group == GetActiveGroup())
            {
                _surfaceStop = stop;
                // 刷新显示
                UpdateDisplay();
            }*/
            var surface_stop = this.GetSurfaceStop();
            if (surface_stop != _surfaceStop)   // surface stop 发生变化
            {
                UpdateDisplay();
            }

            return true;
#if REMOVED
            if (stop == null)
            {
                // 2007/8/1
                EnableStopButtons(false);
                EnableReverseButtons(true, StateParts.None);
                InternalSetMessage("");
                InternalSetProgressBar(-1, -1, -1);
                return false;
            }

            bool bFound = false;

            WriteDebugInfo("collection write lock 4\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                for (int i = 0; i < stops.Count; i++)
                {
                    if (stops[i] == stop)
                    {
                        bFound = true;
                        stops.RemoveAt(i);
                        stops.Add(stop);
                        break;
                    }
                }

                if (bFound)
                    InternalSetProgressBar(stop.ProgressMin, stop.ProgressMax, stop.ProgressValue);
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 4\r\n");
            }

            if (bFound == false)
            {
                // 2007/8/1
                EnableStopButtons(false);
                EnableReverseButtons(true,
                    StateParts.None);
                InternalSetMessage("");
                InternalSetProgressBar(-1, -1, -1);
                return false;
            }

            // Debug.Assert(stop.State != -1, "");

            //if (m_stopToolButton != null) //??
            //{
            if (stop.State == 0)
            {
                EnableStopButtons(true);
                EnableReverseButtons(false,
                    StateParts.None);
            }
            else
            {
                EnableStopButtons(false);
                EnableReverseButtons(true,
                    StateParts.None);
            }
            //}

            // 文字也要变化
            //if (m_messageStatusBar != null) 
            //{
            InternalSetMessage(stop.Message);
            //}

            // 原来 InternalSetProgressBar() 在这里

            //SetToolTipText();

            return true;
#endif
        }

#if REMOVED
        // 是否处在当前激活位置?
        public bool IsActive(Stop stop)
        {
            int index = this.stops.IndexOf(stop);

            if (index == -1)
                return false;

            if (index == this.stops.Count - 1)
                return true;

            return false;
        }
#endif

        // 当前处在显示状态的 Stop 对象
        public Stop SurfaceStop
        {
            get
            {
                return _surfaceStop;
            }
        }

        // 停止当前激活的一个Stop对象。本函数通常被单击停止按钮调
        public void DoStopActive()
        {
            /*
            if (stops.Count > 0)
            {
                Stop temp = (Stop)stops[stops.Count - 1];
                temp.DoStop();
            }

            //SetToolTipText();
            */
            _surfaceStop?.DoStop();
        }

        // 停止所有Stop对象，但是不停止当前激活的那个Stop按钮。
        // locks: 集合写锁
        public void DoStopAllButActive()
        {
            this.m_collectionlock.AcquireReaderLock(Stop.m_nLockTimeout);
            try
            {
                foreach (var group in _groups)
                {
                    foreach (var stop in group)
                    {
                        if (stop == _surfaceStop)
                            continue;
                        if (stop.IsInLoop)
                            stop.DoStop();
                    }
                }
            }
            finally
            {
                this.m_collectionlock.ReleaseReaderLock();
            }
        }

        // 停止所有Stop对象，包括当前激活的Stop对象。这是指stopExclude参数==null
        // 用stopExclude参数可以改变函数行为，不停止某个指定的对象。
        // locks: 集合写锁
        public void DoStopAll(Stop stopExclude)
        {
            this.m_collectionlock.AcquireReaderLock(Stop.m_nLockTimeout);
            try
            {
                foreach (var group in _groups)
                {
                    foreach (var stop in group)
                    {
                        if (stop == stopExclude)
                            continue;
                        if (stop.IsInLoop)
                            stop.DoStop();
                    }
                }
            }
            finally
            {
                this.m_collectionlock.ReleaseReaderLock();
            }
        }

        // 更新 Stop 对象的 Message 显示
        public void UpdateMessage(Stop stop)
        {
            /*
            var active_group = GetActiveGroup();
            if (active_group == null)
                return;
            if (stop.Group != active_group)
                return;
            */
            if (stop == _surfaceStop)
            {
                InternalSetMessage(stop.DisplayMessage);
            }
        }

        public void UpdateProgressRange(Stop stop)
        {
            if (stop == _surfaceStop)
            {
                if (stop.ProgressMin == -1)
                    throw new Exception("ProgressMin 尚未初始化");
                if (stop.ProgressMax == -1)
                    throw new Exception("ProgressMax 尚未初始化");
                if (stop.ProgressValue < stop.ProgressMin
                    || stop.ProgressValue > stop.ProgressMax)
                    throw new Exception($"ProgressValue 越过 ProgressMin 和 ProgressMax 范围");

                InternalSetProgressBar(
                    stop,
                    stop.ProgressMin,
                    stop.ProgressMax,
                    stop.ProgressValue);
            }
        }

        public void UpdateProgressValue(Stop stop)
        {
            if (stop == _surfaceStop)
            {
                // 2024/1/17
                if (stop.ProgressMin == -1
                    || stop.ProgressMax == -1)
                {
                    InternalSetProgressBar(
    stop,
    -1,
    -1,
    -1);
                    return;
                }

                if (stop.ProgressMin == -1)
                    throw new Exception("ProgressMin 尚未初始化");
                if (stop.ProgressMax == -1)
                    throw new Exception("ProgressMax 尚未初始化");
                if (stop.ProgressValue != -1)
                {
                    if (stop.ProgressValue < stop.ProgressMin
                        || stop.ProgressValue > stop.ProgressMax)
                        throw new Exception($"ProgressValue 越过 ProgressMin 和 ProgressMax 范围");
                }

                Debug.Assert(stop.ProgressMin != -1);
                Debug.Assert(stop.ProgressMax != -1);
                InternalSetProgressBar(
                    stop,
                    -1,
                    -1,
                    stop.ProgressValue);
            }
        }

#if REMOVED
        // 设状态, 供Stop调
        // locks: 集合读锁(如果bLock == true)
        public void ChangeState(Stop stop,
            StateParts parts,
            bool bLock)
        {
            if (stops.Count == 0)
                return;

            if (bLock == true)
            {
                WriteDebugInfo("collection read lock 7\r\n");
                this.m_collectionlock.AcquireReaderLock(Stop.m_nLockTimeout);
            }
            bool bLoop = false;
            Stop active = null;

            try
            {
                if (bMultiple == false)
                {
                    if (stop == null)
                    {
                        stop = (Stop)stops[stops.Count - 1];
                    }
                    else
                    {
                        if (stop != stops[stops.Count - 1])
                            return;
                    }
                    bLoop = stop.State == 0 ? true : false;
                    active = stop;
                }
                else
                {
                    bool bFound = false;
                    for (int i = 0; i < stops.Count; i++)
                    {
                        Stop temp = (Stop)stops[i];
                        if (bLoop == false)
                        {
                            if (temp.State == 0)
                                bLoop = true;
                        }

                        if (stop == temp)
                        {
                            bFound = true;
                            active = temp;
                            break;
                        }
                    }
                    if (bFound == false)
                        return;

                }
                // 移走


            }
            finally
            {
                if (bLock == true)
                {
                    this.m_collectionlock.ReleaseReaderLock();
                    WriteDebugInfo("collection read unlock 7\r\n");
                }
            }

            // 2022/6/29 移动到这里
            // TODO: 这里的整个逻辑有问题: 即便视觉上不表现，内存也应当兑现修改，以备后面切换时显示出来。动态不那么及时更新是可以的，但是关键状态变化，例如显示、隐藏等动作，一定要兑现到内存
            if (stop != null && stop.ProgressValue == -1)
            {
                if ((parts & StateParts.ProgressValue) != 0)
                {
                    /*
                    // testing
                    if (stop.Name == "test")
                        Debug.Assert(false);
                    */

                    // 如果为隐藏progress的意图，
                    // 将max和min都设置为-1，以便将来刷新的时候能体现隐藏
                    // 2011/10/12
                    stop.ProgressMax = -1;
                    stop.ProgressMin = -1;
                }
            }

            //if (m_stopToolButton != null) //???
            //{
            if ((parts & StateParts.StopButton) != 0)
                EnableStopButtons(bLoop);

            if ((parts & StateParts.ReverseButtons) != 0)
                EnableReverseButtons(!bLoop,
                    parts);
            //}

            //if (m_messageStatusBar != null) 
            //{
            if ((parts & StateParts.Message) != 0)
                InternalSetMessage(active.Message);
            //}

            if ((parts & StateParts.ProgressRange) != 0)
            {
                // active.ProgressValue = 0;   // 初始化
                active.ProgressValue = active.ProgressMin;   // 初始化 2008/5/16 changed
                InternalSetProgressBar(active.ProgressMin, active.ProgressMax, -1);
            }

            if ((parts & StateParts.ProgressValue) != 0)
            {
                /*
                if (active.ProgressValue == -1)
                {
                    // 如果为隐藏progress的意图，
                    // 将max和min都设置为-1，以便将来刷新的时候能体现隐藏
                    // 2008/3/10
                    active.ProgressMax = -1;
                    active.ProgressMin = -1;
                }
                 * */

                InternalSetProgressBar(-1, -1, active.ProgressValue);
            }

        }
#endif

    }

}
