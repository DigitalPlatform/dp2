using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace dp2Manager
{
	/// <summary>
	/// Summary description for GlobalEditLogicNamesDlg.
	/// </summary>
	public class GlobalEditLogicNamesDlg : System.Windows.Forms.Form
	{
		public List<string[]> LogicNames = null;

		private System.Windows.Forms.TextBox textBox_text;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public GlobalEditLogicNamesDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GlobalEditLogicNamesDlg));
            this.textBox_text = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox_text
            // 
            this.textBox_text.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_text.Location = new System.Drawing.Point(8, 16);
            this.textBox_text.Multiline = true;
            this.textBox_text.Name = "textBox_text";
            this.textBox_text.Size = new System.Drawing.Size(304, 192);
            this.textBox_text.TabIndex = 0;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(320, 16);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(320, 45);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 216);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(384, 32);
            this.label1.TabIndex = 3;
            this.label1.Text = "格式要求: 每行一个事项。在一行中，左边为语言代码，右边为名字值，以逗号分隔";
            // 
            // GlobalEditLogicNamesDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(400, 256);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_text);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GlobalEditLogicNamesDlg";
            this.ShowInTaskbar = false;
            this.Text = "逻辑库名";
            this.Load += new System.EventHandler(this.GlobalEditLogicNamesDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void GlobalEditLogicNamesDlg_Load(object sender, System.EventArgs e)
		{
		
			if (this.LogicNames != null)
			{
				FillLogicNames();
			}
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
		
			BuildLogicNames();

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		void FillLogicNames()
		{
			string strText = "";
			for(int i=0;i<this.LogicNames.Count;i++)
			{
				string [] cols = (string [])this.LogicNames[i];
				strText += cols[1] + "," + cols[0] + "\r\n";
			}

			this.textBox_text.Text = strText;
		}

		void BuildLogicNames()
		{
			this.LogicNames.Clear();

			for(int i=0;i<this.textBox_text.Lines.Length;i++)
			{
				string strLine = this.textBox_text.Lines[i].Trim();
				if (strLine == "")
					continue;
				string strLang = "";
				string strValue = "";
				int nRet = strLine.IndexOf(",");
				if (nRet == -1)
					strLang = strLine;
				else 
				{
					strLang = strLine.Substring(0, nRet).Trim();
					strValue = strLine.Substring(nRet + 1).Trim();
				}
				string [] cols = new string[2];
				cols[0] = strValue;
				cols[1] = strLang;
				this.LogicNames.Add(cols);
			}
		}
	}
}
