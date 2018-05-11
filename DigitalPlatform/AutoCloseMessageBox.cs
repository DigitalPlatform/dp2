using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using System.Threading;

using DigitalPlatform.GUI;

namespace DigitalPlatform
{
    /// <summary>
    /// Summary description for AutoCloseMessageBox.
    /// </summary>
    public class AutoCloseMessageBox : System.Windows.Forms.Form
    {
        string m_strTitleText = "";

        public AutoResetEvent eventClose = new AutoResetEvent(false);	// true : initial state is signaled 

        public Thread threadWaitMessage = null;

        public int m_nTimeOut = 10 * 1000;	// 10秒

        public System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_OK;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public AutoCloseMessageBox()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                this.eventClose.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoCloseMessageBox));
            this.label_message = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(9, 7);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(382, 124);
            this.label_message.TabIndex = 0;
            this.label_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button_OK.AutoSize = true;
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_OK.Location = new System.Drawing.Point(172, 142);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(57, 27);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // AutoCloseMessageBox
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.button_OK;
            this.ClientSize = new System.Drawing.Size(400, 178);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label_message);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AutoCloseMessageBox";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "test";
            this.Closed += new System.EventHandler(this.AutoCloseMessageBox_Closed);
            this.Load += new System.EventHandler(this.AutoCloseMessageBox_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void AutoCloseMessageBox_Load(object sender, System.EventArgs e)
        {
            this.threadWaitMessage =
                new Thread(new ThreadStart(this.ThreadMain));
            this.threadWaitMessage.Start();
        }

        public override string Text
        {
            get
            {
                return m_strTitleText;
            }
            set
            {
                m_strTitleText = value;
            }
        }

        // 2008/12/1
        delegate string Delegate_SetTitleText(string strText);

        string SetTitleText(string strText)
        {
            string strOldText = this.Text;
            base.Text = strText;
            Application.DoEvents();
            this.Update();
            return strOldText;
        }

        delegate void Delegate_CloseWindow();

        void CloseWindow()
        {
            this.Close();
        }

        public void ThreadMain()
        {
            int nTimeLeft = m_nTimeOut;	// 剩下的时间

            WaitHandle[] events = new WaitHandle[1];

            events[0] = eventClose;


            if (m_nTimeOut == -1)
            {
                WaitHandle.WaitAny(events);
                return;
            }

            int nPerTime = 1000;


            while (true)
            {
                int nThisTime = Math.Min(nTimeLeft, nPerTime);

                int index = WaitHandle.WaitAny(events, nThisTime, false);

                if (index == WaitHandle.WaitTimeout)
                {
                    // 修改标题

                    string strText = m_strTitleText + " (" + Convert.ToString(nTimeLeft / 1000) + " 秒后本对话框会自动关闭)";

                    // 注意这里是多线程操作，需要间接调用
                    if (this.InvokeRequired == true)
                    {
                        Delegate_SetTitleText d = new Delegate_SetTitleText(SetTitleText);
                        try
                        {
                            this.Invoke(d, new object[] { strText });
                        }
                        catch (ObjectDisposedException)
                        {
                            return;
                        }
                    }
                    else
                    {
                        base.Text = strText;
                    }

                    if (nThisTime < nPerTime) // 最后一次已经作完
                    {
                        if (this.InvokeRequired == true)
                        {
                            Delegate_CloseWindow d = new Delegate_CloseWindow(CloseWindow);
                            this.Invoke(d);
                        }
                        else
                        {
                            this.Close();
                        }
                        this.DialogResult = System.Windows.Forms.DialogResult.Retry;
                        return;
                    }
                    nTimeLeft -= nPerTime;
                }
                else
                {
                    return;
                }
            }

        }

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void AutoCloseMessageBox_Closed(object sender, System.EventArgs e)
        {
            eventClose.Set();
        }

        public static void Show(string strText)
        {
            AutoCloseMessageBox.Show(null, strText);
        }

        // parameters:
        //      nTimeout    超时时间。毫秒数。-1 表示永不超时
        // return:
        //      DialogResult.Retry 表示超时了
        //      DialogResult.OK 表示点了 OK 按钮
        //      DialogResult.Cancel 表示点了右上角的 Close 按钮
        public static DialogResult Show(IWin32Window owner,
            string strText,
            int nTimeout = -1,
            string strTitle = null)
        {
            AutoCloseMessageBox dlg = new AutoCloseMessageBox();
            Font font = GuiUtil.GetDefaultFont();
            if (font != null)
                dlg.Font = font;

            if (nTimeout != -1)
                dlg.m_nTimeOut = nTimeout;

            if (strTitle != null)
                dlg.Text = strTitle;
            dlg.label_message.Text = strText;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            return dlg.ShowDialog(owner);
        }
    }
}
