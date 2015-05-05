using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DigitalPlatform.Marc
{
	/// <summary>
	/// IndicatorNameDlg 的摘要说明。
    /// 本对话框没有使用
	/// </summary>
	internal class IndicatorNameDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button_ok;
		public System.Windows.Forms.TextBox textBox_indicator;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button_cancel;
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;

        /// <summary>
        /// 构造函数
        /// </summary>
		public IndicatorNameDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IndicatorNameDlg));
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_ok = new System.Windows.Forms.Button();
            this.textBox_indicator = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(244, 116);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_cancel.TabIndex = 7;
            this.button_cancel.Text = "取消";
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // button_ok
            // 
            this.button_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ok.Location = new System.Drawing.Point(164, 116);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(76, 23);
            this.button_ok.TabIndex = 6;
            this.button_ok.Text = "确定";
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // textBox_indicator
            // 
            this.textBox_indicator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_indicator.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_indicator.Location = new System.Drawing.Point(9, 25);
            this.textBox_indicator.MaxLength = 2;
            this.textBox_indicator.Name = "textBox_indicator";
            this.textBox_indicator.Size = new System.Drawing.Size(310, 21);
            this.textBox_indicator.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "指示符:";
            // 
            // IndicatorNameDlg
            // 
            this.AcceptButton = this.button_ok;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(328, 148);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.textBox_indicator);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IndicatorNameDlg";
            this.ShowInTaskbar = false;
            this.Text = "IndicatorNameDlg";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void button_ok_Click(object sender, System.EventArgs e)
		{
			if (this.textBox_indicator.Text == "")
			{
				MessageBox.Show (this,"尚未输入指示符");
				return;
			}

			if (this.textBox_indicator.Text.Length != 2)
			{
				MessageBox.Show(this,"字段指示符2的长度必须为2位。");
				return;
			}
		
			this.DialogResult = DialogResult.OK ;
			this.Close();
		}


		private void button_cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel ;
			this.Close();
		}
	}
}
