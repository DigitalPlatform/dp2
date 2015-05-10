using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DigitalPlatform.Xml
{
	/// <summary>
	/// PropertyDlg 的摘要说明。
	/// </summary>
	public class PropertyDlg : System.Windows.Forms.Form
	{
		public System.Windows.Forms.TextBox textBox_message;
		private System.Windows.Forms.Button button_ok;
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PropertyDlg()
		{
			//
			// Windows 窗体设计器支持所必需的
			//
			InitializeComponent();

			//
			// TODO: 在 InitializeComponent 调用后添加任何构造函数代码
			//
		}

		/// <summary>
		/// 清理所有正在使用的资源。
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

		#region Windows 窗体设计器生成的代码
		/// <summary>
		/// 设计器支持所需的方法 - 不要使用代码编辑器修改
		/// 此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_message.Location = new System.Drawing.Point(12, 12);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.Size = new System.Drawing.Size(416, 289);
            this.textBox_message.TabIndex = 0;
            // 
            // button_ok
            // 
            this.button_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ok.AutoSize = true;
            this.button_ok.Location = new System.Drawing.Point(368, 307);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(60, 23);
            this.button_ok.TabIndex = 1;
            this.button_ok.Text = "确定";
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // PropertyDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(440, 342);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.textBox_message);
            this.Name = "PropertyDlg";
            this.Text = "PropertyDlg";
            this.Load += new System.EventHandler(this.PropertyDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void PropertyDlg_Load(object sender, System.EventArgs e)
		{
		
		}

		private void button_ok_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
