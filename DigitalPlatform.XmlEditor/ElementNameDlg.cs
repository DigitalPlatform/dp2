using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using DigitalPlatform.Text;

namespace DigitalPlatform.Xml
{
	public class ElementNameDlg : System.Windows.Forms.Form
	{
		public bool HasNs = false;
		public bool bText = false;
		//public string strElementName = "";


		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox textBox_strElementName;
		private System.Windows.Forms.Button button_ok;
		private System.Windows.Forms.Button button_cancel;
		private System.Windows.Forms.Label label_info;
		public System.Windows.Forms.TextBox textBox_URI;
		private System.Windows.Forms.CheckBox checkBox_URI;
		public System.Windows.Forms.TextBox textBox_text;
		private System.Windows.Forms.CheckBox checkBox_text;
		private System.ComponentModel.Container components = null;

		public ElementNameDlg()
		{
			InitializeComponent();
		}
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


		#region Windows 窗体设计器生成的代码
		/// <summary>
		/// 设计器支持所需的方法 - 不要使用代码编辑器修改
		/// 此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_strElementName = new System.Windows.Forms.TextBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label_info = new System.Windows.Forms.Label();
            this.textBox_text = new System.Windows.Forms.TextBox();
            this.textBox_URI = new System.Windows.Forms.TextBox();
            this.checkBox_URI = new System.Windows.Forms.CheckBox();
            this.checkBox_text = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "元素名:";
            // 
            // textBox_strElementName
            // 
            this.textBox_strElementName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_strElementName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_strElementName.Location = new System.Drawing.Point(93, 58);
            this.textBox_strElementName.Name = "textBox_strElementName";
            this.textBox_strElementName.Size = new System.Drawing.Size(371, 25);
            this.textBox_strElementName.TabIndex = 2;
            // 
            // button_ok
            // 
            this.button_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ok.Location = new System.Drawing.Point(308, 344);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(75, 30);
            this.button_ok.TabIndex = 6;
            this.button_ok.Text = "确认";
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(389, 344);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(75, 30);
            this.button_cancel.TabIndex = 7;
            this.button_cancel.Text = "取消";
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label_info
            // 
            this.label_info.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_info.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_info.Location = new System.Drawing.Point(12, 12);
            this.label_info.Name = "label_info";
            this.label_info.Size = new System.Drawing.Size(452, 30);
            this.label_info.TabIndex = 0;
            this.label_info.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_text
            // 
            this.textBox_text.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_text.Enabled = false;
            this.textBox_text.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_text.Location = new System.Drawing.Point(93, 202);
            this.textBox_text.Multiline = true;
            this.textBox_text.Name = "textBox_text";
            this.textBox_text.Size = new System.Drawing.Size(371, 98);
            this.textBox_text.TabIndex = 4;
            // 
            // textBox_URI
            // 
            this.textBox_URI.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_URI.Enabled = false;
            this.textBox_URI.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_URI.Location = new System.Drawing.Point(93, 125);
            this.textBox_URI.Name = "textBox_URI";
            this.textBox_URI.Size = new System.Drawing.Size(371, 25);
            this.textBox_URI.TabIndex = 9;
            // 
            // checkBox_URI
            // 
            this.checkBox_URI.AutoSize = true;
            this.checkBox_URI.Location = new System.Drawing.Point(12, 100);
            this.checkBox_URI.Name = "checkBox_URI";
            this.checkBox_URI.Size = new System.Drawing.Size(118, 19);
            this.checkBox_URI.TabIndex = 10;
            this.checkBox_URI.Text = "名字空间URI:";
            this.checkBox_URI.CheckedChanged += new System.EventHandler(this.checkBox_URI_CheckedChanged);
            // 
            // checkBox_text
            // 
            this.checkBox_text.AutoSize = true;
            this.checkBox_text.Location = new System.Drawing.Point(12, 175);
            this.checkBox_text.Name = "checkBox_text";
            this.checkBox_text.Size = new System.Drawing.Size(71, 19);
            this.checkBox_text.TabIndex = 11;
            this.checkBox_text.Text = "文本：";
            this.checkBox_text.CheckedChanged += new System.EventHandler(this.checkBox_text_CheckedChanged);
            // 
            // ElementNameDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(8, 18);
            this.ClientSize = new System.Drawing.Size(476, 386);
            this.Controls.Add(this.checkBox_text);
            this.Controls.Add(this.checkBox_URI);
            this.Controls.Add(this.textBox_URI);
            this.Controls.Add(this.textBox_text);
            this.Controls.Add(this.textBox_strElementName);
            this.Controls.Add(this.label_info);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ElementNameDlg";
            this.Text = "新元素";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion


		public void SetInfo(string strTitle,
			string strInfo)
		{
			this.Text = strTitle;
			this.label_info .Text = strInfo;
		}

		private void button_ok_Click(object sender, System.EventArgs e)
		{

			if (this.textBox_strElementName .Text == "")
			{
				MessageBox.Show (this,"尚未输入元素名");
				return;
			}

			//上面已经判断，所以这里不用判断空了
			string strName = this.textBox_strElementName .Text ;
			string charFirst = strName.Substring(0,1);
						
			if (StringUtil.RegexCompare("[a-zA-Z_]",charFirst) == false)
			{
				MessageBox.Show("'" + strName + "'不是Xml合法的元素名，请输入其它元素名");
				return;
			}

/*
			if (this.checkBox_hasns.Checked == true)
			{
				// 其实前缀可以没有
				if (this.textBox_prefix.Text == ""
					|| this.textBox_namespaceURI.Text == "")
				{
					this.label_errorInfo.Text = "！ 前缀和URI都必须有值";
					return;
				}
				this.HasNs = true;
			}
*/			

			this.DialogResult = DialogResult.OK ;
			this.Close ();
		}

		private void button_cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel ;
			this.Close ();
		}

		private void checkBox_URI_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.checkBox_URI.Checked == true)
				this.textBox_URI.Enabled = true;
			else
			{
				this.textBox_URI.Enabled = false;
				this.textBox_URI.Text = "";
			}
		}

		private void checkBox_text_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.checkBox_text.Checked == true)
				this.textBox_text.Enabled = true;
			else
			{
				this.textBox_text.Enabled = false;
				this.textBox_text.Text = "";
			}
		}

        // 是否要输入URI字符串
        public bool InputUri
        {
            get
            {
                return this.checkBox_URI.Checked;
            }
            set
            {
                this.checkBox_URI.Checked = value;
            }
        }

/*
		private void checkBox_hasns_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.checkBox_hasns.Checked == true)
			{
				this.textBox_prefix.Enabled = true;
				this.textBox_namespaceURI.Enabled = true;
			}
			else
			{
				this.textBox_prefix.Enabled = false;
				this.textBox_namespaceURI.Enabled = false;
			}
		}
*/		


	}
}
