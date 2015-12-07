using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DigitalPlatform.Script
{
	/// <summary>
	/// Summary description for FileNameDlg.
	/// </summary>
	public class FileNameDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Button button_OK;
		public System.Windows.Forms.TextBox textBox_fileName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboBox_fileType;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FileNameDlg()
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_fileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_fileType = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(246, 101);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(74, 23);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_OK.Location = new System.Drawing.Point(166, 101);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_fileName
            // 
            this.textBox_fileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_fileName.Location = new System.Drawing.Point(89, 32);
            this.textBox_fileName.Name = "textBox_fileName";
            this.textBox_fileName.Size = new System.Drawing.Size(231, 21);
            this.textBox_fileName.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "文件名:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "文件类型:";
            // 
            // comboBox_fileType
            // 
            this.comboBox_fileType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_fileType.Items.AddRange(new object[] {
            "main.cs",
            "metadata.xml",
            "references.xml",
            "marcfilter.fltx",
            "others..."});
            this.comboBox_fileType.Location = new System.Drawing.Point(89, 9);
            this.comboBox_fileType.Name = "comboBox_fileType";
            this.comboBox_fileType.Size = new System.Drawing.Size(231, 20);
            this.comboBox_fileType.TabIndex = 9;
            this.comboBox_fileType.Text = "others...";
            this.comboBox_fileType.SelectedIndexChanged += new System.EventHandler(this.comboBox_fileType_SelectedIndexChanged);
            // 
            // FileNameDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(330, 133);
            this.Controls.Add(this.comboBox_fileType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_fileName);
            this.Controls.Add(this.label1);
            this.Name = "FileNameDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "文件名";
            this.Load += new System.EventHandler(this.FileNameDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void FileNameDlg_Load(object sender, System.EventArgs e)
		{
			// 将combobox和textbox同步
			for(int i =0;i< comboBox_fileType.Items.Count; i++) 
			{
				if (String.Compare(textBox_fileName.Text,
					(string)comboBox_fileType.Items[i], true) == 0) 
				{
					textBox_fileName.Enabled = false;
					return;
				}
			}
		
		}


		private void comboBox_fileType_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (comboBox_fileType.Text == "others...") 
			{
				textBox_fileName.Text = "";
				textBox_fileName.Enabled = true;
			}
			else 
			{
				textBox_fileName.Text = comboBox_fileType.Text;
				textBox_fileName.Enabled = false;
			}
		
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (textBox_fileName.Text == "")
			{
				MessageBox.Show(this, "尚未指定文件名");
				this.DialogResult = DialogResult.None;
				return;
			}

			this.Close();
			this.DialogResult = DialogResult.OK;
		}
	}
}
