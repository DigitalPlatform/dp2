using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using DigitalPlatform.Text ;

namespace DigitalPlatform.Xml
{
	public class AttrNameDlg : System.Windows.Forms.Form
	{
		public ElementItem m_element = null;	// 基准元素
		public ArrayList aExistAttr = null;
		private System.Windows.Forms.Label label_info;
		private System.Windows.Forms.Button button_cancel;
		private System.Windows.Forms.Button button_ok;
		public System.Windows.Forms.TextBox textBox_strElementName;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox textBox_value;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox checkBox_URI;
		public System.Windows.Forms.TextBox textBox_URI;
		private System.ComponentModel.Container components = null;

		public AttrNameDlg()
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
            this.label_info = new System.Windows.Forms.Label();
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_ok = new System.Windows.Forms.Button();
            this.textBox_strElementName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_value = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_URI = new System.Windows.Forms.CheckBox();
            this.textBox_URI = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label_info
            // 
            this.label_info.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_info.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_info.Location = new System.Drawing.Point(12, 9);
            this.label_info.Name = "label_info";
            this.label_info.Size = new System.Drawing.Size(360, 24);
            this.label_info.TabIndex = 0;
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(292, 216);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(80, 26);
            this.button_cancel.TabIndex = 7;
            this.button_cancel.Text = "取消";
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // button_ok
            // 
            this.button_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ok.Location = new System.Drawing.Point(206, 216);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(80, 26);
            this.button_ok.TabIndex = 6;
            this.button_ok.Text = "确认";
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // textBox_strElementName
            // 
            this.textBox_strElementName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_strElementName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_strElementName.Location = new System.Drawing.Point(69, 41);
            this.textBox_strElementName.Name = "textBox_strElementName";
            this.textBox_strElementName.Size = new System.Drawing.Size(303, 21);
            this.textBox_strElementName.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "属性名:";
            // 
            // textBox_value
            // 
            this.textBox_value.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_value.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_value.Location = new System.Drawing.Point(68, 68);
            this.textBox_value.Name = "textBox_value";
            this.textBox_value.Size = new System.Drawing.Size(304, 21);
            this.textBox_value.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "属性值:";
            // 
            // checkBox_URI
            // 
            this.checkBox_URI.AutoSize = true;
            this.checkBox_URI.Location = new System.Drawing.Point(12, 102);
            this.checkBox_URI.Name = "checkBox_URI";
            this.checkBox_URI.Size = new System.Drawing.Size(96, 16);
            this.checkBox_URI.TabIndex = 12;
            this.checkBox_URI.Text = "名字空间URI:";
            this.checkBox_URI.CheckedChanged += new System.EventHandler(this.checkBox_URI_CheckedChanged);
            // 
            // textBox_URI
            // 
            this.textBox_URI.Enabled = false;
            this.textBox_URI.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_URI.Location = new System.Drawing.Point(68, 124);
            this.textBox_URI.Name = "textBox_URI";
            this.textBox_URI.Size = new System.Drawing.Size(304, 21);
            this.textBox_URI.TabIndex = 11;
            // 
            // AttrNameDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(384, 254);
            this.Controls.Add(this.checkBox_URI);
            this.Controls.Add(this.textBox_URI);
            this.Controls.Add(this.textBox_value);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label_info);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.textBox_strElementName);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AttrNameDlg";
            this.Text = "新属性";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		public void SetInfo(string strTitle,
			string strInfo,
			ElementItem item)
		{
			this.Text = strTitle;
			this.label_info .Text = strInfo;
			this.m_element = item;
			
			if (m_element.attrs == null)
				return;
			foreach(Item attr in m_element.attrs )
			{
				if (this.aExistAttr == null)
					this.aExistAttr = new ArrayList ();
				aExistAttr.Add (attr.Name );
			}
		}

		public bool IsExist(string strName)
		{
			if (this.aExistAttr == null)
				return false;
			foreach(string strAttrName in this.aExistAttr )
			{
				if (strName == strAttrName)
					return true;
			}
			return false;
		}

		private void button_ok_Click(object sender, System.EventArgs e)
		{
			//1.先判断是否为空
			if (this.textBox_strElementName .Text == "")
			{
				MessageBox.Show (this,"尚未输入属性名");
				return;
			}

			//2.判断是否合法
			//上面已经判断，所以这里不用判断空了
			string strName = this.textBox_strElementName .Text ;
			string charFirst = strName.Substring(0,1);
						
			if (StringUtil.RegexCompare("[a-zA-Z_]",charFirst) == false)
			{
				MessageBox.Show(this,"'" + strName + "'不是Xml合法的属性名，请输入其它属性名。");
				return;
			}

			//判断是否已存在
			if (this.IsExist (strName) == true)
			{
				MessageBox.Show(this,"'" + this.m_element.Name + "'已包含'" + strName + "'属性，不能有同名属性，请输入其它属性名。");
				return;
			}


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


	}
}
