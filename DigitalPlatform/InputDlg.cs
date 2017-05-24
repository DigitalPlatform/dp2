using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DigitalPlatform
{
	/// <summary>
	/// Summary description for InputDlg.
	/// </summary>
	public class InputDlg : System.Windows.Forms.Form
	{
		public System.Windows.Forms.Label label_title;
		public System.Windows.Forms.TextBox textBox_value;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public InputDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputDlg));
            this.label_title = new System.Windows.Forms.Label();
            this.textBox_value = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_title
            // 
            this.label_title.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_title.Location = new System.Drawing.Point(9, 9);
            this.label_title.Name = "label_title";
            this.label_title.Size = new System.Drawing.Size(355, 73);
            this.label_title.TabIndex = 0;
            this.label_title.Text = "请输入字符串:";
            this.label_title.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // textBox_value
            // 
            this.textBox_value.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_value.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_value.Location = new System.Drawing.Point(12, 85);
            this.textBox_value.Name = "textBox_value";
            this.textBox_value.Size = new System.Drawing.Size(352, 25);
            this.textBox_value.TabIndex = 1;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_OK.Location = new System.Drawing.Point(158, 116);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(100, 28);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(264, 116);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(100, 28);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // InputDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(8, 18);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(376, 156);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_value);
            this.Controls.Add(this.label_title);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InputDlg";
            this.ShowInTaskbar = false;
            this.Text = "输入框";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion


        // return:
        //      null    用户取消对话框
        //      其他    所输入的值
		public static string GetInput(
			IWin32Window owner,
			string strDlgTitle,
			string strTitle,
			string strDefaultValue,
            Font font = null)
		{
			InputDlg dlg = new InputDlg();
            if (font != null)
                dlg.Font = font;

			if (strDlgTitle != null)
				dlg.Text = strDlgTitle;

			if (strTitle != null)
				dlg.label_title.Text = strTitle;

			if (strDefaultValue != null)
				dlg.textBox_value.Text = strDefaultValue;

            dlg.StartPosition = FormStartPosition.CenterScreen; // 2008/10/17

            // 2017/4/6
            if (owner == null)
                dlg.TopMost = true;

			dlg.ShowDialog(owner);

			if (dlg.DialogResult != DialogResult.OK)
				return null;

			return dlg.textBox_value.Text;
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
		
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
