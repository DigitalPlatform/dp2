using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Text;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;

namespace DigitalPlatform.Script
{
	/// <summary>
	/// Summary description for CompileErrorDlg.
	/// </summary>
	public class CompileErrorDlg : System.Windows.Forms.Form
	{
		public ApplicationInfo	applicationInfo = null;

		public bool IsFltx = false;	// 是否为.fltx.cs文件

        bool bFirst = true;
        public NoHasSelTextBox textBox_errorInfo;
		public NoHasSelTextBox textBox_code;
		public System.Windows.Forms.Label label_codeFileName;
		private System.Windows.Forms.Label label_message;
        private SplitContainer splitContainer1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public CompileErrorDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompileErrorDlg));
            this.textBox_code = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.textBox_errorInfo = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.label_codeFileName = new System.Windows.Forms.Label();
            this.label_message = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_code
            // 
            this.textBox_code.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_code.HideSelection = false;
            this.textBox_code.Location = new System.Drawing.Point(0, 0);
            this.textBox_code.MaxLength = 0;
            this.textBox_code.Multiline = true;
            this.textBox_code.Name = "textBox_code";
            this.textBox_code.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_code.Size = new System.Drawing.Size(467, 168);
            this.textBox_code.TabIndex = 2;
            this.textBox_code.WordWrap = false;
            this.textBox_code.TextChanged += new System.EventHandler(this.textBox_code_TextChanged);
            this.textBox_code.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_code_KeyDown);
            this.textBox_code.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBox_code_MouseUp);
            // 
            // textBox_errorInfo
            // 
            this.textBox_errorInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_errorInfo.Location = new System.Drawing.Point(0, 0);
            this.textBox_errorInfo.MaxLength = 0;
            this.textBox_errorInfo.Multiline = true;
            this.textBox_errorInfo.Name = "textBox_errorInfo";
            this.textBox_errorInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_errorInfo.Size = new System.Drawing.Size(467, 117);
            this.textBox_errorInfo.TabIndex = 0;
            this.textBox_errorInfo.Text = "textBox1";
            this.textBox_errorInfo.WordWrap = false;
            this.textBox_errorInfo.DoubleClick += new System.EventHandler(this.textBox_errorInfo_DoubleClick);
            // 
            // label_codeFileName
            // 
            this.label_codeFileName.Location = new System.Drawing.Point(9, 7);
            this.label_codeFileName.Name = "label_codeFileName";
            this.label_codeFileName.Size = new System.Drawing.Size(404, 21);
            this.label_codeFileName.TabIndex = 1;
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(9, 321);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(467, 18);
            this.label_message.TabIndex = 2;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(9, 30);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.textBox_code);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBox_errorInfo);
            this.splitContainer1.Size = new System.Drawing.Size(467, 289);
            this.splitContainer1.SplitterDistance = 168;
            this.splitContainer1.TabIndex = 3;
            // 
            // CompileErrorDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 346);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.label_codeFileName);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CompileErrorDlg";
            this.ShowInTaskbar = false;
            this.Text = "编译错误";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.CompileErrorDlg_Closing);
            this.Load += new System.EventHandler(this.CompileErrorDlg_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void CompileErrorDlg_Load(object sender, System.EventArgs e)
		{
			// 恢复窗口尺寸
			int nWindowWidth = applicationInfo.GetInt("code_editor",
				"window_width",
				-1);
			int nWindowHeight = applicationInfo.GetInt("code_editor",
				"window_height",
				-1);
			if (nWindowHeight != -1) 
			{
				this.Size = new Size(nWindowWidth, nWindowHeight);

				int x = applicationInfo.GetInt("code_editor",
					"window_x",
					0);
				int y = applicationInfo.GetInt("code_editor",
					"window_y",
					0);
				this.Location = new Point(x, y);
			}

			this.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState),
				applicationInfo.GetString(
				"code_editor", "window_state", "Normal"));


			string strFontFace1 = applicationInfo.GetString("code_editor",
				"code_font_face",
				"Lucida Console");
			int nFontSize1 = 
				applicationInfo.GetInt("code_editor",
				"code_font_size",
				9);
			textBox_code.Font = new Font(strFontFace1, nFontSize1);

			string strFontFace2 = applicationInfo.GetString("code_editor",
				"error_font_face",
				"Lucida Console");
			int nFontSize2 = 
				applicationInfo.GetInt("code_editor",
				"error_font_size",
				9);
			textBox_errorInfo.Font = new Font(strFontFace2, nFontSize2);


			if (label_codeFileName.Text != "") 
			{
				bFirst = true;
				// 源代码本身
				try 
				{
                    using (StreamReader sr = new StreamReader(label_codeFileName.Text, true))
                    {
                        textBox_code.Text = sr.ReadToEnd();
                    }
				}
				catch
				{
					textBox_code.Text = "";
				}

				bFirst = false;

				/*
				// tabstop 为什么不起作用?

				int [] tabstops = {8};
				API.SetEditTabStops(textBox_code, tabstops);
				textBox_code.Invalidate();
				*/
			}

			/*
			textBox_errorInfo.Focus();
			API.SetEditCurrentCaretPos(
				textBox_errorInfo,
				0,
				0,
				true);
				*/

		}

		public void Initial(string strCodeFileName,
			string strErrorInfo)
		{
			label_codeFileName.Text = strCodeFileName;

			textBox_errorInfo.Text = strErrorInfo;
		}

		private void textBox_errorInfo_DoubleClick(object sender, System.EventArgs e)
		{
            if (textBox_errorInfo.Lines.Length == 0)
                return;

            // TODO: textbox内容不能折行
			int x =0;
			int y = 0;
			API.GetEditCurrentCaretPos(
				textBox_errorInfo,
				out x,
				out y);

			string strLine = textBox_errorInfo.Lines[y];

			// 析出"(行，列)"值

			int nRet = strLine.IndexOf("(");
			if (nRet == -1)
				return;
			strLine = strLine.Substring(nRet+1);
			nRet = strLine.IndexOf(")");
			if (nRet != -1)
				strLine = strLine.Substring(0, nRet);
			strLine = strLine.Trim();

			// 找到','
			nRet = strLine.IndexOf(",");
			if (nRet == -1)
				return;
			y = Convert.ToInt32(strLine.Substring(0, nRet).Trim()) - 1;
			x = Convert.ToInt32(strLine.Substring(nRet+1).Trim()) - 1;

			// MessageBox.Show(Convert.ToString(x) + " , "+Convert.ToString(y));

			textBox_code.Focus();
			textBox_code.DisableEmSetSelMsg = false;
			API.SetEditCurrentCaretPos(
				textBox_code,
				x,
				y,
				true);
			textBox_code.DisableEmSetSelMsg = true;
			OnCaretChanged();

		}

		private void CompileErrorDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (label_codeFileName.Text != "" 
				&& API.GetEditModify(textBox_code) == true) 
			{

				if (IsFltx == true)
				{
					MessageBox.Show(this, "警告: 文件 '" 
						+ label_codeFileName.Text +
						"' 不应在这里直修改(因为即便修改了，下次重新编译时，此.fltx.cs文件也会被程序从.fltx文件新创建的内容覆盖掉)。\r\n\r\n请修改对应的 .fltx 文件");
					return;
				}


				DialogResult msgResult = MessageBox.Show(this,
					"源代码已经被修改。\r\n是否保存到文件 " + label_codeFileName.Text + "?",
					"script",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1);
				if (msgResult == DialogResult.Cancel) 
				{
					e.Cancel = true;
					return;
				}

				if (msgResult == DialogResult.Yes) 
				{
					SaveCodeFile();
					return;
				}

			}
			applicationInfo.SetString(
				"code_editor", "window_state", 
				Enum.GetName(typeof(FormWindowState), this.WindowState));

			WindowState = FormWindowState.Normal;	// 是否先隐藏窗口?

			// 保存窗口尺寸
			applicationInfo.SetInt("code_editor",
				"window_width",
				this.Size.Width);
			applicationInfo.SetInt("code_editor",
				"window_height",
				this.Size.Height);
			applicationInfo.SetInt("code_editor",
				"window_x",
				this.Location.X);
			applicationInfo.SetInt("code_editor",
				"window_y",
				this.Location.Y);
		}

		void SaveCodeFile()
		{
			if (label_codeFileName.Text == "")
				return;
            using (StreamWriter sw = new StreamWriter(label_codeFileName.Text,
                false, Encoding.UTF8))
            {
                sw.Write(textBox_code.Text);
            }
			API.SetEditModify(textBox_code, false);
		}

		private void textBox_code_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			OnCaretChanged();
		}

		private void textBox_code_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			OnCaretChanged();
		}

		void OnCaretChanged()
		{
			int x =0;
			int y = 0;
			API.GetEditCurrentCaretPos(
				textBox_code,
				out x,
				out y);
			label_message.Text = Convert.ToString(y+1) + ", " + Convert.ToString(x+1);

		}

		private void textBox_code_TextChanged(object sender, System.EventArgs e)
		{
			if (IsFltx == true && bFirst == false)
			{
				MessageBox.Show(this, "警告: 文件 '" 
					+ label_codeFileName.Text 
					+ "'文件不能在这里直修改(因为即便修改了，下次重新编译时，此.fltx.cs文件也会被程序从.fltx文件新创建的内容覆盖掉)。\r\n\r\n请修改对应的 .fltx 文件");
				return;
			}
		}
	}
}
