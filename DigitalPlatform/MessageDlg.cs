using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using DigitalPlatform.GUI;

namespace DigitalPlatform
{
	/// <summary>
	/// Summary description for MessageDlg.
	/// </summary>
	public class MessageDlg : System.Windows.Forms.Form
	{
		public string Message = "";

        public string[] ButtonTexts = null;

		public MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
		public MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1;

        private System.Windows.Forms.TextBox textBox_message;
        private CheckBox checkBox_noAsk;
		private System.Windows.Forms.Button button_1;
		private System.Windows.Forms.Button button_2;
		private System.Windows.Forms.Button button_3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MessageDlg()
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
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageDlg));
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.checkBox_noAsk = new System.Windows.Forms.CheckBox();
            this.button_1 = new System.Windows.Forms.Button();
            this.button_2 = new System.Windows.Forms.Button();
            this.button_3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Window;
            this.textBox_message.Location = new System.Drawing.Point(12, 12);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(424, 193);
            this.textBox_message.TabIndex = 0;
            // 
            // checkBox_noAsk
            // 
            this.checkBox_noAsk.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_noAsk.AutoSize = true;
            this.checkBox_noAsk.Location = new System.Drawing.Point(12, 211);
            this.checkBox_noAsk.Name = "checkBox_noAsk";
            this.checkBox_noAsk.Size = new System.Drawing.Size(174, 16);
            this.checkBox_noAsk.TabIndex = 1;
            this.checkBox_noAsk.Text = "以后遇相同情况不再询问(&N)";
            // 
            // button_1
            // 
            this.button_1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_1.Location = new System.Drawing.Point(188, 241);
            this.button_1.Name = "button_1";
            this.button_1.Size = new System.Drawing.Size(76, 22);
            this.button_1.TabIndex = 2;
            this.button_1.Text = "是(&Y)";
            this.button_1.Click += new System.EventHandler(this.button_1_Click);
            // 
            // button_2
            // 
            this.button_2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_2.Location = new System.Drawing.Point(270, 241);
            this.button_2.Name = "button_2";
            this.button_2.Size = new System.Drawing.Size(74, 22);
            this.button_2.TabIndex = 3;
            this.button_2.Text = "否(&N)";
            this.button_2.Click += new System.EventHandler(this.button_2_Click);
            // 
            // button_3
            // 
            this.button_3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_3.Location = new System.Drawing.Point(361, 241);
            this.button_3.Name = "button_3";
            this.button_3.Size = new System.Drawing.Size(75, 22);
            this.button_3.TabIndex = 4;
            this.button_3.Text = "取消(&C)";
            this.button_3.Click += new System.EventHandler(this.button_3_Click);
            // 
            // MessageDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(448, 276);
            this.Controls.Add(this.button_3);
            this.Controls.Add(this.button_2);
            this.Controls.Add(this.button_1);
            this.Controls.Add(this.checkBox_noAsk);
            this.Controls.Add(this.textBox_message);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MessageDlg";
            this.ShowInTaskbar = false;
            this.Text = "MessageDlg";
            this.Load += new System.EventHandler(this.MessageDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void MessageDlg_Load(object sender, System.EventArgs e)
		{
			if (this.defaultButton == MessageBoxDefaultButton.Button1)
				this.AcceptButton = button_1;
			if (this.defaultButton == MessageBoxDefaultButton.Button2)
				this.AcceptButton = button_2;
			if (this.defaultButton == MessageBoxDefaultButton.Button3)
				this.AcceptButton = button_3;

			textBox_message.Text = Message;

            // this.textBox_message.Select(0, 0);

            {
                if (buttons == MessageBoxButtons.AbortRetryIgnore)
                {
                    button_1.Text = "终止 Abort";
                    button_1.Enabled = true;

                    button_2.Text = "重试 Retry";
                    button_2.Enabled = true;

                    button_3.Text = "忽略 Ignore";
                    button_3.Enabled = true;
                }

                if (buttons == MessageBoxButtons.OK)
                {
                    button_1.Text = "确定 OK";
                    button_1.Enabled = true;

                    button_2.Text = "";
                    button_2.Enabled = false;

                    button_3.Text = "";
                    button_3.Enabled = false;

                }

                if (buttons == MessageBoxButtons.OKCancel)
                {
                    button_1.Text = "确定 OK";
                    button_1.Enabled = true;

                    button_2.Text = "取消 Cancel";
                    button_2.Enabled = true;

                    button_3.Text = "";
                    button_3.Enabled = false;
                }

                if (buttons == MessageBoxButtons.RetryCancel)
                {
                    button_1.Text = "重试 Retry";
                    button_1.Enabled = true;

                    button_2.Text = "取消 Cancel";
                    button_2.Enabled = true;

                    button_3.Text = "";
                    button_3.Enabled = false;
                }

                if (buttons == MessageBoxButtons.YesNo)
                {
                    button_1.Text = "是 Yes";
                    button_1.Enabled = true;

                    button_2.Text = "否 No";
                    button_2.Enabled = true;

                    button_3.Text = "";
                    button_3.Enabled = false;
                }

                if (buttons == MessageBoxButtons.YesNoCancel)
                {
                    button_1.Text = "是 Yes";
                    button_1.Enabled = true;

                    button_2.Text = "否 No";
                    button_2.Enabled = true;

                    button_3.Text = "取消 Cancel";
                    button_3.Enabled = true;
                }
            }

            if (this.ButtonTexts != null)
            {
                SetButtonText(this.ButtonTexts);
            }

            this.BeginInvoke(new Delegate_Initial(_initial));
        }

        /*public*/ delegate void Delegate_Initial();

        void _initial()
        {
            this.textBox_message.SelectionStart = this.textBox_message.Text.Length;
            this.textBox_message.ScrollToCaret();


            if (this.AcceptButton != null)
                ((Button)this.AcceptButton).Focus();
        }

        public void SetButtonText(string [] texts)
        {
            if (texts == null)
                return;

            if (texts.Length > 0)
            {
                button_1.Text = texts[0];
                button_1.Enabled = true;
            }

            if (texts.Length > 1)
            {
                button_2.Text = texts[1];
                button_2.Enabled = true;
            }

            if (texts.Length > 2)
            {
                button_3.Text = texts[2];
                button_3.Enabled = true;
            }
        }

		private void button_1_Click(object sender, System.EventArgs e)
		{
			if (buttons == MessageBoxButtons.AbortRetryIgnore)
			{
				this.DialogResult = DialogResult.Abort;
			}

			if (buttons == MessageBoxButtons.OK)
			{
				this.DialogResult = DialogResult.OK;
			}

			if (buttons == MessageBoxButtons.OKCancel)
			{
				this.DialogResult = DialogResult.OK;

			}

			if (buttons == MessageBoxButtons.RetryCancel)
			{
				this.DialogResult = DialogResult.Retry;
			}

			if (buttons == MessageBoxButtons.YesNo)
			{
				this.DialogResult = DialogResult.Yes;
			}

			if (buttons == MessageBoxButtons.YesNoCancel)
			{
				this.DialogResult = DialogResult.Yes;
			}
			this.Close();

		}

		private void button_2_Click(object sender, System.EventArgs e)
		{
			if (buttons == MessageBoxButtons.AbortRetryIgnore)
			{
				this.DialogResult = DialogResult.Retry;
			}

			if (buttons == MessageBoxButtons.OK)
			{
			}

			if (buttons == MessageBoxButtons.OKCancel)
			{
				this.DialogResult = DialogResult.Cancel;
			}

			if (buttons == MessageBoxButtons.RetryCancel)
			{
				this.DialogResult = DialogResult.Cancel;
			}

			if (buttons == MessageBoxButtons.YesNo)
			{
				this.DialogResult = DialogResult.No;
			}

			if (buttons == MessageBoxButtons.YesNoCancel)
			{
				this.DialogResult = DialogResult.No;
			}
			this.Close();

		}

		private void button_3_Click(object sender, System.EventArgs e)
		{
			if (buttons == MessageBoxButtons.AbortRetryIgnore)
			{
				this.DialogResult = DialogResult.Ignore;
			}

			if (buttons == MessageBoxButtons.OK)
			{
			}

			if (buttons == MessageBoxButtons.OKCancel)
			{
			}

			if (buttons == MessageBoxButtons.RetryCancel)
			{
			}

			if (buttons == MessageBoxButtons.YesNo)
			{
			}

			if (buttons == MessageBoxButtons.YesNoCancel)
			{
				this.DialogResult = DialogResult.Cancel;
			}

			this.Close();
		}

		public static DialogResult Show(IWin32Window owner, 
			string strText,
			string strCaption,
			MessageBoxButtons buttons,
			MessageBoxDefaultButton defaultButton,
			ref bool bChecked,
            string [] button_texts = null,
            string strCheckBoxText = "")
		{
			MessageDlg dlg = new MessageDlg();
            Font font = GuiUtil.GetDefaultFont();
            if (font != null)
                dlg.Font = font;
            if (string.IsNullOrEmpty(strCheckBoxText) == false)
                dlg.checkBox_noAsk.Text = strCheckBoxText;
			dlg.checkBox_noAsk.Checked = bChecked;
			dlg.buttons = buttons;
			dlg.defaultButton = defaultButton;
			dlg.Message = strText;
            dlg.Text = strCaption;
            dlg.ButtonTexts = button_texts;
            dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(owner);

			bChecked = dlg.checkBox_noAsk.Checked;

			return dlg.DialogResult;
		}
	}
}
