using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalPlatform
{
    //在父窗口中定义,初始化按钮
    public class StopManager
    {
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

        public List<Stop> stops = new List<Stop>();
        bool bMultiple = false;

        // string m_strTipsSave = "";

        List<object> m_reverseButtons = null;	// 当停止按钮Enabled状态变化时，需要和它状态相反的按钮对象数组
        List<int> m_reverseButtonEnableStates = null;   // 0: diabled; 1: eanbled; -1: unknown

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
                // ((TextBox)m_messageBar).Text = strMessage;
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

        void InternalSetProgressBar(long lStart, long lEnd, long lValue)
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
        string Safe_SetStatusStripText(StatusStrip status_strip,
            string strText)
        {
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
        }

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

        #endregion

        // 2015/11/2
        string Safe_SetToolStripItemText(ToolStripItem status_strip,
string strText)
        {
            if (status_strip.Owner == null
                || status_strip.Owner.Parent == null)
                return null;

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
        }

        #region ToolStripStatusLabel

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

                // label.Owner.Update();    // 优化

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

        #endregion

        #region TextBox

        // 线程安全版本
        string Safe_SetTextBoxText(Control textbox,
            string strText)
        {
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
        }

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

        #endregion

#if NO
        #region Label

        // 线程安全版本
        string Safe_SetLabelText(Label label,
            string strText)
        {
            if (label.Parent != null && label.Parent.InvokeRequired)
            {
                Delegate_SetLabelText d = new Delegate_SetLabelText(SetLabelText);
                return (string)label.Parent.Invoke(d, new object[] { label, strText });
            }
            else
            {
                string strOldText = label.Text;

                label.Text = strText;
                label.Update();

                return strOldText;
            }
        }

        delegate string Delegate_SetLabelText(Label label,
            string strText);

        string SetLabelText(Label label,
            string strText)
        {
            string strOldText = label.Text;

            label.Text = strText;
            label.Update();

            return strOldText;
        }

        #endregion
#endif

        #region ProgressBar

        // 线程安全版本
        void Safe_SetProgressBar(ProgressBar progressbar,
            long lStart,
            long lEnd,
            long lValue)
        {
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
        }

        void SetRatio(long lEnd)
        {
            this.progress_ratio = (double)64000 / (double)lEnd;
            if (this.progress_ratio > 1.0)
                this.progress_ratio = 1.0;
        }

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


        #endregion

        #region ToolStripProgressBar

        // 线程安全版本
        void Safe_SetProgressBar(ToolStripProgressBar progressbar,
            long lStart,
            long lEnd,
            long lValue)
        {
            if (progressbar.Owner == null)
                return;

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
        }

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
        static bool Safe_EnableButton(Button button,
            bool bEnabled)
        {
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
        }

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

        #endregion

        #region ToolBarButton

        // 线程安全调用
        static bool Safe_EnableToolBarButton(ToolBarButton button,
            bool bEnabled)
        {
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
        }

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

        #endregion

        #region ToolStripButton

        // 线程安全调用
        static bool Safe_EnableToolStripButton(ToolStripButton button,
            bool bEnabled)
        {
            // 2014/12/26
            if (button.Owner == null)
                return false;

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
                    // ((Button)button).Enabled = (bRestore == true ? bOldState : bEnabled);
                    Safe_EnableButton((Button)button,
                        (bRestore == true ? bOldState : bEnableResult));

                }
                else if (button is ToolBarButton)
                {
                    // ((ToolBarButton)button).Enabled = bEnabled;

                    bOldState = Safe_EnableToolBarButton(((ToolBarButton)button),
                        bRestore == true ? bOldState : bEnableResult);
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
                else if (button is ToolStripButton)
                {
                    // ((ToolStripButton)button).Enabled = bEnabled;

                    bOldState = Safe_EnableToolStripButton(((ToolStripButton)button),
                        bRestore == true ? bOldState : bEnableResult);
                }

                // 主动disable前，记忆以前的状态
                if (bEnabled == false
                    && bSave == true)
                    m_reverseButtonEnableStates[i] = (bOldState == true ? 1 : 0);

            }
        }

        // 初始化一个按钮,在父窗口load时调
        // locks: 集合写锁
        public void Initial(object button,
            object statusBar,
            object progressBar)
        {

            WriteDebugInfo("collection write lock 1\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {

                m_stopButton = button;

                /*
				if (m_stopButton != null)
					m_strTipsSave = m_stopToolButton.ToolTipText;
				else
					m_strTipsSave = "";
                 */

                m_messageBar = statusBar;
                m_progressBar = progressBar;
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 1\r\n");
            }

        }


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

        /*
		void SetToolTipText()
		{
			if (m_stopToolButton != null) 
			{
				string strText = DebugInfo();
				if (strText != "0")
					m_stopToolButton.ToolTipText = m_strTipsSave + "\r\n[" + DebugInfo() + "]";
				else
					m_stopToolButton.ToolTipText = m_strTipsSave;
			}
		}
         */


        string DebugInfo()
        {
            /*
            string strText = "";

            this.m_lock.AcquireReaderLock(Stop.m_nLockTimeout);
            try 
            {
                for(int i=0;i<stops.Count;i++) 
                {
                    Stop temp = (Stop)stops[i];
                    strText += Convert.ToString(i) + "-" 
                        + temp.Name + "-["
                        + Convert.ToString(temp.State())
                        +"]\r\n";
                }
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
            }

            return strText;
            */
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

        // 激活按钮
        // 因为StopManager管辖很多Stop对象, 当一个Stop对象需要处于焦点状态,
        // 需要在StopManager内部数组中把这个Stop对象挪到队列尾部, 也就是相当于堆栈顶部
        // 的效果。然后, 把StopManager所关联的button设置为和Stop对象当前状态对应的
        // Enabled或者Disabled状态, 因为只有这样用户才能触发按钮。
        // StopManager管理了很多Stop状态，Active()函数相当于把某个Stop状态翻到可见的顶部。
        // locks: 集合写锁
        public bool Active(Stop stop)
        {
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

            InternalSetProgressBar(stop.ProgressMin, stop.ProgressMax, stop.ProgressValue);

            //SetToolTipText();

            return true;
        }

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

        // 当前激活了的Stop对象
        public Stop ActiveStop
        {
            get
            {
                if (stops.Count > 0)
                {
                    return (Stop)stops[stops.Count - 1];
                }

                return null;
            }
        }

        // 停止当前激活的一个Stop对象。本函数通常被单击停止按钮调
        public void DoStopActive()
        {
            if (stops.Count > 0)
            {
                Stop temp = (Stop)stops[stops.Count - 1];
                temp.DoStop();
            }

            //SetToolTipText();

        }

#if NOOOOOOOOOOO
        // 一半停止当前激活的一个Stop对象。本函数通常被单击停止按钮调
        public void DoHalfStopActive()
        {
            if (stops.Count > 0)
            {
                Stop temp = (Stop)stops[stops.Count - 1];
                temp.DoStop(true);
            }
        }
#endif

        // 停止所有Stop对象，但是不停止当前激活的那个Stop按钮。
        // locks: 集合写锁
        public void DoStopAllButActive()
        {
            WriteDebugInfo("collection write lock 5\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                for (int i = 0; i < stops.Count - 1; i++)
                {
                    Stop temp = (Stop)stops[i];
                    temp.DoStop();
                }
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 5\r\n");
            }

            //SetToolTipText();
        }

        // 停止所有Stop对象，包括当前激活的Stop对象。这是指stopExclude参数==null
        // 用stopExclude参数可以改变函数行为，不停止某个指定的对象。
        // locks: 集合写锁
        public void DoStopAll(Stop stopExclude)
        {
            WriteDebugInfo("collection write lock 6\r\n");
            this.m_collectionlock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                for (int i = 0; i < stops.Count; i++)
                {
                    Stop temp = (Stop)stops[i];
                    if (stopExclude != temp)
                        temp.DoStop();
                }
            }
            finally
            {
                this.m_collectionlock.ReleaseWriterLock();
                WriteDebugInfo("collection write unlock 6\r\n");

            }

            //SetToolTipText();
        }

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
    }
}
