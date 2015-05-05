using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DigitalPlatform.Marc
{
	/// <summary>
	/// WaitDlg 的摘要说明。
	/// </summary>
	public class WaitDlg : System.Windows.Forms.Form
	{
		public System.Windows.Forms.Label label_message;
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public WaitDlg()
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
            this.label_message = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(12, 9);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(408, 33);
            this.label_message.TabIndex = 0;
            this.label_message.Text = "请等待...";
            // 
            // WaitDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(432, 51);
            this.Controls.Add(this.label_message);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WaitDlg";
            this.Text = "WaitDlg";
            this.ResumeLayout(false);

		}
		#endregion
	}
}
