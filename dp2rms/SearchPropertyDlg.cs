using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using DigitalPlatform.Xml;

namespace dp2rms
{
	/// <summary>
	/// Summary description for SearchPropertyDlg.
	/// </summary>
	public class SearchPropertyDlg : System.Windows.Forms.Form
	{
		public ApplicationInfo	ap = null;
		public string CfgTitle = "";


		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_maxCount;
		private System.Windows.Forms.CheckBox checkBox_autoDetectRange;
		private System.Windows.Forms.CheckBox checkBox_autoDetectRelation;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Label label2;
        private CheckBox checkBox_autoSplitWords;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SearchPropertyDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchPropertyDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_maxCount = new System.Windows.Forms.TextBox();
            this.checkBox_autoDetectRange = new System.Windows.Forms.CheckBox();
            this.checkBox_autoDetectRelation = new System.Windows.Forms.CheckBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_autoSplitWords = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "最大命中记录个数(&C):";
            // 
            // textBox_maxCount
            // 
            this.textBox_maxCount.Location = new System.Drawing.Point(143, 9);
            this.textBox_maxCount.Name = "textBox_maxCount";
            this.textBox_maxCount.Size = new System.Drawing.Size(120, 21);
            this.textBox_maxCount.TabIndex = 1;
            // 
            // checkBox_autoDetectRange
            // 
            this.checkBox_autoDetectRange.AutoSize = true;
            this.checkBox_autoDetectRange.Location = new System.Drawing.Point(11, 96);
            this.checkBox_autoDetectRange.Name = "checkBox_autoDetectRange";
            this.checkBox_autoDetectRange.Size = new System.Drawing.Size(180, 16);
            this.checkBox_autoDetectRange.TabIndex = 4;
            this.checkBox_autoDetectRange.Text = "自动探测 数值范围表达式(&R)";
            // 
            // checkBox_autoDetectRelation
            // 
            this.checkBox_autoDetectRelation.AutoSize = true;
            this.checkBox_autoDetectRelation.Location = new System.Drawing.Point(11, 118);
            this.checkBox_autoDetectRelation.Name = "checkBox_autoDetectRelation";
            this.checkBox_autoDetectRelation.Size = new System.Drawing.Size(180, 16);
            this.checkBox_autoDetectRelation.TabIndex = 5;
            this.checkBox_autoDetectRelation.Text = "自动探测 数量关系表达式(&N)";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(248, 127);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 25);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.AutoSize = true;
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(248, 158);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 25);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(141, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "注: -1表示不限定";
            // 
            // checkBox_autoSplitWords
            // 
            this.checkBox_autoSplitWords.AutoSize = true;
            this.checkBox_autoSplitWords.Location = new System.Drawing.Point(11, 63);
            this.checkBox_autoSplitWords.Name = "checkBox_autoSplitWords";
            this.checkBox_autoSplitWords.Size = new System.Drawing.Size(186, 16);
            this.checkBox_autoSplitWords.TabIndex = 3;
            this.checkBox_autoSplitWords.Text = "以空格为界自动切割检索词(&W)";
            this.checkBox_autoSplitWords.UseVisualStyleBackColor = true;
            // 
            // SearchPropertyDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(335, 195);
            this.Controls.Add(this.checkBox_autoSplitWords);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_autoDetectRelation);
            this.Controls.Add(this.checkBox_autoDetectRange);
            this.Controls.Add(this.textBox_maxCount);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SearchPropertyDlg";
            this.ShowInTaskbar = false;
            this.Text = "检索式属性";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.SearchPropertyDlg_Closing);
            this.Closed += new System.EventHandler(this.SearchPropertyDlg_Closed);
            this.Load += new System.EventHandler(this.SearchPropertyDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void SearchPropertyDlg_Load(object sender, System.EventArgs e)
		{
			if (ap == null || CfgTitle == "")
				return;

			textBox_maxCount.Text = Convert.ToString( ap.GetInt(CfgTitle, "maxcount", -1) );
			checkBox_autoDetectRange.Checked = Convert.ToBoolean( ap.GetInt(CfgTitle, "auto_detect_range", 0) );
			checkBox_autoDetectRelation.Checked = Convert.ToBoolean( ap.GetInt(CfgTitle, "auto_detect_relation", 0) );
            this.checkBox_autoSplitWords.Checked = Convert.ToBoolean(ap.GetInt(CfgTitle, "auto_split_words", 1));
		}

		private void SearchPropertyDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
		
		}

		private void SearchPropertyDlg_Closed(object sender, System.EventArgs e)
		{
		
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (ap != null && CfgTitle != "")
			{
				ap.SetInt(CfgTitle, "maxcount", Convert.ToInt32(textBox_maxCount.Text));
				ap.SetInt(CfgTitle, "auto_detect_range", Convert.ToInt32(checkBox_autoDetectRange.Checked));
				ap.SetInt(CfgTitle, "auto_detect_relation", Convert.ToInt32(checkBox_autoDetectRelation.Checked));
                ap.SetInt(CfgTitle, "auto_split_words", Convert.ToInt32(this.checkBox_autoSplitWords.Checked));
            }
		
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
