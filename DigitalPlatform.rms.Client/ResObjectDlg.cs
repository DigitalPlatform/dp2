using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.IO;

namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// 管理一个资源的对话框
	/// 管理上传和下载.下载是立即的.上传是滞后的.
	/// </summary>
	public class ResObjectDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox textBox_serverName;
		private System.Windows.Forms.Label label2;
		public System.Windows.Forms.TextBox textBox_state;
		private System.Windows.Forms.Label label4;
		public System.Windows.Forms.TextBox textBox_localPath;
		private System.Windows.Forms.Button button_findLocalPath;
		private System.Windows.Forms.Label label5;
		public System.Windows.Forms.TextBox textBox_size;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Label label3;
		public System.Windows.Forms.TextBox textBox_mime;
		public System.Windows.Forms.TextBox textBox_timestamp;
		private System.Windows.Forms.Label label6;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ResObjectDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResObjectDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_serverName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_state = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_localPath = new System.Windows.Forms.TextBox();
            this.button_findLocalPath = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_size = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_mime = new System.Windows.Forms.TextBox();
            this.textBox_timestamp = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "资源ID (&I):";
            // 
            // textBox_serverName
            // 
            this.textBox_serverName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox_serverName.Location = new System.Drawing.Point(112, 12);
            this.textBox_serverName.Name = "textBox_serverName";
            this.textBox_serverName.ReadOnly = true;
            this.textBox_serverName.Size = new System.Drawing.Size(291, 21);
            this.textBox_serverName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "状态(&S):";
            // 
            // textBox_state
            // 
            this.textBox_state.Location = new System.Drawing.Point(112, 43);
            this.textBox_state.Name = "textBox_state";
            this.textBox_state.ReadOnly = true;
            this.textBox_state.Size = new System.Drawing.Size(100, 21);
            this.textBox_state.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "本地物理路径(&P):";
            // 
            // textBox_localPath
            // 
            this.textBox_localPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_localPath.Location = new System.Drawing.Point(14, 104);
            this.textBox_localPath.Name = "textBox_localPath";
            this.textBox_localPath.Size = new System.Drawing.Size(351, 21);
            this.textBox_localPath.TabIndex = 7;
            // 
            // button_findLocalPath
            // 
            this.button_findLocalPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findLocalPath.Location = new System.Drawing.Point(371, 104);
            this.button_findLocalPath.Name = "button_findLocalPath";
            this.button_findLocalPath.Size = new System.Drawing.Size(32, 23);
            this.button_findLocalPath.TabIndex = 8;
            this.button_findLocalPath.Text = "...";
            this.button_findLocalPath.Click += new System.EventHandler(this.button_findLocalPath_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 139);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "尺寸(&S):";
            // 
            // textBox_size
            // 
            this.textBox_size.Location = new System.Drawing.Point(112, 136);
            this.textBox_size.Name = "textBox_size";
            this.textBox_size.ReadOnly = true;
            this.textBox_size.Size = new System.Drawing.Size(100, 21);
            this.textBox_size.TabIndex = 10;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(331, 176);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 11;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(331, 205);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 12;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 173);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 13;
            this.label3.Text = "媒体类型(&M):";
            // 
            // textBox_mime
            // 
            this.textBox_mime.Location = new System.Drawing.Point(112, 168);
            this.textBox_mime.Name = "textBox_mime";
            this.textBox_mime.Size = new System.Drawing.Size(187, 21);
            this.textBox_mime.TabIndex = 14;
            // 
            // textBox_timestamp
            // 
            this.textBox_timestamp.Location = new System.Drawing.Point(112, 200);
            this.textBox_timestamp.Name = "textBox_timestamp";
            this.textBox_timestamp.ReadOnly = true;
            this.textBox_timestamp.Size = new System.Drawing.Size(187, 21);
            this.textBox_timestamp.TabIndex = 16;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 205);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 15;
            this.label6.Text = "时间戳(&T):";
            // 
            // ResObjectDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(418, 235);
            this.Controls.Add(this.textBox_timestamp);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox_mime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_size);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_findLocalPath);
            this.Controls.Add(this.textBox_localPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_state);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_serverName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ResObjectDlg";
            this.ShowInTaskbar = false;
            this.Text = "资源文件";
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
			//dlg.InitialDirectory = "c:\\" ;
			//dlg.FileName = itemSelected.Text ;
			dlg.Filter = "All files (*.*)|*.*" ;
			dlg.FilterIndex = 2 ;
			dlg.RestoreDirectory = true ;

			if(dlg.ShowDialog() != DialogResult.OK) 
				return;

			FileInfo fileInfo = new FileInfo(dlg.FileName);

			this.textBox_localPath.Text = fileInfo.FullName;
			this.textBox_size.Text = Convert.ToString(fileInfo.Length);
			this.textBox_state.Text = "尚未上载";

#if NO
			textBox_mime.Text = API.MimeTypeFrom(ReadFirst256Bytes(dlg.FileName),
				"");
#endif
            textBox_mime.Text = PathUtil.MimeTypeFrom(dlg.FileName);
		}

#if NO
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
#endif




	}
}
