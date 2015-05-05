using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform;


namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// 导入配置文件的对话框
	/// </summary>
	public class OpenCfgFileDlg : System.Windows.Forms.Form
	{
		public System.Windows.Forms.TextBox textBox_mime;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Button button_OK;
		public System.Windows.Forms.TextBox textBox_size;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button button_findLocalPath;
		public System.Windows.Forms.TextBox textBox_localPath;
		private System.Windows.Forms.Label label4;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OpenCfgFileDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenCfgFileDlg));
            this.textBox_mime = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_size = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_findLocalPath = new System.Windows.Forms.Button();
            this.textBox_localPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox_mime
            // 
            this.textBox_mime.Location = new System.Drawing.Point(112, 96);
            this.textBox_mime.Name = "textBox_mime";
            this.textBox_mime.Size = new System.Drawing.Size(196, 21);
            this.textBox_mime.TabIndex = 25;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 24;
            this.label3.Text = "媒体类型(&M):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(329, 125);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 23;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(329, 96);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 22;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_size
            // 
            this.textBox_size.Location = new System.Drawing.Point(112, 66);
            this.textBox_size.Name = "textBox_size";
            this.textBox_size.ReadOnly = true;
            this.textBox_size.Size = new System.Drawing.Size(100, 21);
            this.textBox_size.TabIndex = 21;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 69);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 20;
            this.label5.Text = "尺寸(&S):";
            // 
            // button_findLocalPath
            // 
            this.button_findLocalPath.Location = new System.Drawing.Point(372, 30);
            this.button_findLocalPath.Name = "button_findLocalPath";
            this.button_findLocalPath.Size = new System.Drawing.Size(32, 23);
            this.button_findLocalPath.TabIndex = 19;
            this.button_findLocalPath.Text = "...";
            this.button_findLocalPath.Click += new System.EventHandler(this.button_findLocalPath_Click);
            // 
            // textBox_localPath
            // 
            this.textBox_localPath.Location = new System.Drawing.Point(12, 30);
            this.textBox_localPath.Name = "textBox_localPath";
            this.textBox_localPath.Size = new System.Drawing.Size(354, 21);
            this.textBox_localPath.TabIndex = 18;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 12);
            this.label4.TabIndex = 17;
            this.label4.Text = "本地物理路径(&P):";
            // 
            // OpenCfgFileDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(416, 160);
            this.Controls.Add(this.textBox_mime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_size);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_findLocalPath);
            this.Controls.Add(this.textBox_localPath);
            this.Controls.Add(this.label4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OpenCfgFileDlg";
            this.ShowInTaskbar = false;
            this.Text = "导入配置文件";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (textBox_localPath.Text == "")
			{
				MessageBox.Show(this, "尚未指定文件本地路径");
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

		private void button_findLocalPath_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "选择文件";
			dlg.Filter = "All files (*.*)|*.*" ;
			dlg.FilterIndex = 2 ;
			dlg.RestoreDirectory = true ;

			if(dlg.ShowDialog() != DialogResult.OK) 
				return;

			FileInfo fileInfo = new FileInfo(dlg.FileName);

			this.textBox_localPath.Text = fileInfo.FullName;
			this.textBox_size.Text = Convert.ToString(fileInfo.Length);

			textBox_mime.Text = API.MimeTypeFrom(ReadFirst256Bytes(dlg.FileName),
				"");
		}


		// 读取文件前256bytes
		byte[] ReadFirst256Bytes(string strFileName)
		{
			FileStream fileSource = File.Open(
				strFileName,
				FileMode.Open,
				FileAccess.Read, 
				FileShare.ReadWrite);

			byte[] result = new byte[Math.Min(256, fileSource.Length)];
			fileSource.Read(result, 0, result.Length);

			fileSource.Close();

			return result;
		}
	}
}
