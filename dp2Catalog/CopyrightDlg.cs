using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using DigitalPlatform;
using DigitalPlatform.Text;

namespace dp2Catalog
{
	/// <summary>
	/// Summary description for CopyrightDlg.
	/// </summary>
	public class CopyrightDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Label label_copyright;
		private System.Windows.Forms.TextBox textBox_environment;
        private PictureBox pictureBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public CopyrightDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			// this.Opacity = 0.75;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CopyrightDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.label_copyright = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_environment = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.Color.Honeydew;
            this.label1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 131);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(457, 42);
            this.label1.TabIndex = 0;
            this.label1.Text = "dp2编目前端 dp2Catalog V2.3";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_copyright
            // 
            this.label_copyright.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_copyright.BackColor = System.Drawing.Color.PaleGreen;
            this.label_copyright.Location = new System.Drawing.Point(12, 173);
            this.label_copyright.Name = "label_copyright";
            this.label_copyright.Size = new System.Drawing.Size(457, 41);
            this.label_copyright.TabIndex = 1;
            this.label_copyright.Text = "(C) 版权所有 2006-2015 数字平台(北京)软件有限责任公司 Digital Platform (Beijing) Software Corp. Ltd" +
    ".";
            this.label_copyright.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.Location = new System.Drawing.Point(12, 214);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(457, 18);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://www.dp2003.com";
            this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button_OK.AutoSize = true;
            this.button_OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_OK.Location = new System.Drawing.Point(192, 298);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(96, 29);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_environment
            // 
            this.textBox_environment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_environment.BackColor = System.Drawing.Color.Honeydew;
            this.textBox_environment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_environment.Location = new System.Drawing.Point(12, 235);
            this.textBox_environment.Multiline = true;
            this.textBox_environment.Name = "textBox_environment";
            this.textBox_environment.ReadOnly = true;
            this.textBox_environment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_environment.Size = new System.Drawing.Size(457, 60);
            this.textBox_environment.TabIndex = 6;
            this.textBox_environment.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(457, 116);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // CopyrightDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(481, 336);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.textBox_environment);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label_copyright);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CopyrightDlg";
            this.ShowInTaskbar = false;
            this.Text = "Copyright 版权";
            this.Load += new System.EventHandler(this.CopyrightDlg_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void button_OK_Click(object sender, System.EventArgs e)
		{
            /*
			for(int i = 0; i<100; i++) 
			{
				System.Threading.Thread.Sleep(10);

				this.Opacity = (double)1 - ((double)i/(double)100);
				this.Update();
				//Application.DoEvents();
			}*/

			Close();
		}

		private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			/*
			// Determine which link was clicked within the LinkLabel.
			MessageBox.Show(linkLabel1.Links[0].ToString());
			return;
			*/
			System.Diagnostics.Process.Start("iexplore",linkLabel1.Text);
		
		}

		private void CopyrightDlg_Load(object sender, System.EventArgs e)
		{
			label_copyright.Text = "(C) 版权所有 2006-2015 数字平台(北京)软件有限责任公司\r\nDigital Platform (Beijing) Software Corp. Ltd.";

			Assembly myAssembly;

			myAssembly = Assembly.GetAssembly(this.GetType());
			textBox_environment.Text = "本机 .NET Framework 版本: " + myAssembly.ImageRuntimeVersion
				+ "\r\n本软件: " + myAssembly.FullName
                + "\r\n\r\n本机 MAC 地址: " + StringUtil.MakePathList(SerialCodeForm.GetMacAddress());

			/*
			for(int i = 0; i<100; i++) 
			{
				System.Threading.Thread.Sleep(10);

				this.Opacity = ((double)(i+1)/(double)100);
				this.Update();

			}
			*/	
		}
	}
}
