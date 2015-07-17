using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DigitalPlatform.Xml
{
	/// <summary>
	/// Summary description for AddNsDefineDlg.
	/// </summary>
	public class AddNsDefineDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox textBox_prefix;
		private System.Windows.Forms.Label label2;
		public System.Windows.Forms.TextBox textBox_uri;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		public System.Windows.Forms.Label label_message;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AddNsDefineDlg()
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_prefix = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_uri = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label_message = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "前缀:";
            // 
            // textBox_prefix
            // 
            this.textBox_prefix.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_prefix.Location = new System.Drawing.Point(104, 88);
            this.textBox_prefix.Name = "textBox_prefix";
            this.textBox_prefix.Size = new System.Drawing.Size(100, 21);
            this.textBox_prefix.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "URI:";
            // 
            // textBox_uri
            // 
            this.textBox_uri.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_uri.Location = new System.Drawing.Point(104, 120);
            this.textBox_uri.Name = "textBox_uri";
            this.textBox_uri.Size = new System.Drawing.Size(300, 21);
            this.textBox_uri.TabIndex = 3;
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(248, 157);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(329, 157);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label_message
            // 
            this.label_message.Location = new System.Drawing.Point(12, 9);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(392, 66);
            this.label_message.TabIndex = 6;
            // 
            // AddNsDefineDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(416, 192);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_uri);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_prefix);
            this.Controls.Add(this.label1);
            this.Name = "AddNsDefineDlg";
            this.Text = "新增名字空间定义";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (textBox_prefix.Text == "")
			{
				MessageBox.Show(this, "尚未指定前缀");
				return;
			}

			if (textBox_uri.Text == "")
			{
				MessageBox.Show(this, "尚未指定URI");
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
