using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;



namespace DigitalPlatform.GcatClient
{
	/// <summary>
	/// Summary description for QuestionDlg.
	/// </summary>
	public class QuestionDlg : System.Windows.Forms.Form
	{
		public System.Windows.Forms.Label label_messageTitle;
		public System.Windows.Forms.TextBox textBox_question;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox textBox_result;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public QuestionDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuestionDlg));
            this.label_messageTitle = new System.Windows.Forms.Label();
            this.textBox_question = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_result = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_messageTitle
            // 
            this.label_messageTitle.AutoSize = true;
            this.label_messageTitle.Location = new System.Drawing.Point(7, 7);
            this.label_messageTitle.Name = "label_messageTitle";
            this.label_messageTitle.Size = new System.Drawing.Size(35, 12);
            this.label_messageTitle.TabIndex = 4;
            this.label_messageTitle.Text = "text:";
            // 
            // textBox_question
            // 
            this.textBox_question.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_question.Location = new System.Drawing.Point(9, 21);
            this.textBox_question.MaxLength = 0;
            this.textBox_question.Multiline = true;
            this.textBox_question.Name = "textBox_question";
            this.textBox_question.ReadOnly = true;
            this.textBox_question.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_question.Size = new System.Drawing.Size(482, 272);
            this.textBox_question.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 300);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "答案(&A):";
            // 
            // textBox_result
            // 
            this.textBox_result.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_result.Location = new System.Drawing.Point(88, 297);
            this.textBox_result.Name = "textBox_result";
            this.textBox_result.Size = new System.Drawing.Size(403, 21);
            this.textBox_result.TabIndex = 1;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(337, 321);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(74, 22);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(416, 321);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 22);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // QuestionDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(500, 353);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_result);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_question);
            this.Controls.Add(this.label_messageTitle);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "QuestionDlg";
            this.ShowInTaskbar = false;
            this.Text = "创建著者号 - 请回答提问";
            this.Load += new System.EventHandler(this.QuestionDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (textBox_result.Text == "")
			{
				MessageBox.Show(this, "尚未输入答案");
				return;
			}
		
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();		
		}

		private void QuestionDlg_Load(object sender, System.EventArgs e)
		{
			this.AcceptButton = this.button_OK;
			this.textBox_result.Focus();
		}


	}
}
