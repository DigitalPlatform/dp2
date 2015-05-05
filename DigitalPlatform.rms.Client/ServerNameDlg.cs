using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// Summary description for ServerNameDlg.
	/// </summary>
	public class ServerNameDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox textBox_url;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Button button_OK;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ServerNameDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerNameDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_url = new System.Windows.Forms.TextBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器URL(&U):";
            // 
            // textBox_url
            // 
            this.textBox_url.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_url.Location = new System.Drawing.Point(12, 24);
            this.textBox_url.Name = "textBox_url";
            this.textBox_url.Size = new System.Drawing.Size(405, 21);
            this.textBox_url.TabIndex = 1;
            this.textBox_url.Text = "http://dp2003.com/dp2kernel";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.AutoSize = true;
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(361, 51);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 23);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(299, 51);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // ServerNameDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(429, 85);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.textBox_url);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ServerNameDlg";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "服务器URL地址";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (textBox_url.Text == "")
			{
				MessageBox.Show(this, "请指定服务器URL...");
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
	}
}
