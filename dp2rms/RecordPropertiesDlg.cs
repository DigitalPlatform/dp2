using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace dp2rms
{
	/// <summary>
	/// Summary description for RecordPropertiesDlg.
	/// </summary>
	public class RecordPropertiesDlg : System.Windows.Forms.Form
	{
		public System.Windows.Forms.TextBox textBox_content;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public RecordPropertiesDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecordPropertiesDlg));
            this.textBox_content = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox_content
            // 
            this.textBox_content.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_content.Location = new System.Drawing.Point(12, 12);
            this.textBox_content.Multiline = true;
            this.textBox_content.Name = "textBox_content";
            this.textBox_content.ReadOnly = true;
            this.textBox_content.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_content.Size = new System.Drawing.Size(368, 252);
            this.textBox_content.TabIndex = 0;
            // 
            // RecordPropertiesDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(392, 276);
            this.Controls.Add(this.textBox_content);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RecordPropertiesDlg";
            this.ShowInTaskbar = false;
            this.Text = "¼ÇÂ¼ÊôÐÔ";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
	}
}
