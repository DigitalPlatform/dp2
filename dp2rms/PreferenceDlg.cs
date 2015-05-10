using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;

namespace dp2rms
{
	/// <summary>
	/// Summary description for PreferenceDlg.
	/// </summary>
	public class PreferenceDlg : System.Windows.Forms.Form
	{
		public ApplicationInfo ap = null;
        public MainForm MainForm = null;

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.TabPage tabPage_pinyin;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_pinyin_pinyinDbPath;
        private Button button_findPinyinDbPath;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PreferenceDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreferenceDlg));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_pinyin = new System.Windows.Forms.TabPage();
            this.button_findPinyinDbPath = new System.Windows.Forms.Button();
            this.textBox_pinyin_pinyinDbPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage_pinyin.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_pinyin);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(376, 223);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_pinyin
            // 
            this.tabPage_pinyin.BackColor = System.Drawing.Color.Transparent;
            this.tabPage_pinyin.Controls.Add(this.button_findPinyinDbPath);
            this.tabPage_pinyin.Controls.Add(this.textBox_pinyin_pinyinDbPath);
            this.tabPage_pinyin.Controls.Add(this.label1);
            this.tabPage_pinyin.Location = new System.Drawing.Point(4, 22);
            this.tabPage_pinyin.Name = "tabPage_pinyin";
            this.tabPage_pinyin.Size = new System.Drawing.Size(368, 197);
            this.tabPage_pinyin.TabIndex = 0;
            this.tabPage_pinyin.Text = "拼音";
            this.tabPage_pinyin.UseVisualStyleBackColor = true;
            // 
            // button_findPinyinDbPath
            // 
            this.button_findPinyinDbPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findPinyinDbPath.AutoSize = true;
            this.button_findPinyinDbPath.Location = new System.Drawing.Point(307, 59);
            this.button_findPinyinDbPath.Name = "button_findPinyinDbPath";
            this.button_findPinyinDbPath.Size = new System.Drawing.Size(48, 23);
            this.button_findPinyinDbPath.TabIndex = 2;
            this.button_findPinyinDbPath.Text = "...";
            this.button_findPinyinDbPath.UseVisualStyleBackColor = true;
            this.button_findPinyinDbPath.Click += new System.EventHandler(this.button_findPinyinDbPath_Click);
            // 
            // textBox_pinyin_pinyinDbPath
            // 
            this.textBox_pinyin_pinyinDbPath.Location = new System.Drawing.Point(14, 32);
            this.textBox_pinyin_pinyinDbPath.Name = "textBox_pinyin_pinyinDbPath";
            this.textBox_pinyin_pinyinDbPath.Size = new System.Drawing.Size(341, 21);
            this.textBox_pinyin_pinyinDbPath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "拼音库路径:";
            // 
            // button_Cancel
            // 
            this.button_Cancel.AutoSize = true;
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(313, 241);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(232, 241);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // PreferenceDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(400, 276);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "PreferenceDlg";
            this.ShowInTaskbar = false;
            this.Text = "系统参数";
            this.Load += new System.EventHandler(this.PreferenceDlg_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_pinyin.ResumeLayout(false);
            this.tabPage_pinyin.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void PreferenceDlg_Load(object sender, System.EventArgs e)
		{
			if (ap != null)
			{
				textBox_pinyin_pinyinDbPath.Text = ap.GetString("pinyin",
					"pinyin_db_path",
					"");

			}
			else 
			{
				button_OK.Enabled = false;
			}
		
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (ap != null)
			{
				 ap.SetString("pinyin",
					"pinyin_db_path",
					textBox_pinyin_pinyinDbPath.Text);

			}

			this.DialogResult = DialogResult.OK;
			this.Close();

		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

        private void button_findPinyinDbPath_Click(object sender, EventArgs e)
        {
            // 选择目标数据库
            OpenResDlg dlg = new OpenResDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Text = "请选择拼音库";
            dlg.EnabledIndices = new int[] { ResTree.RESTYPE_DB };
            dlg.ap = this.MainForm.AppInfo;
            dlg.ApCfgTitle = "preferencedlg_findpinyinpathdlg";
            dlg.Path = this.textBox_pinyin_pinyinDbPath.Text;
            dlg.Initial(MainForm.Servers,
                MainForm.SearchPanel.Channels);
            // dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_pinyin_pinyinDbPath.Text = dlg.Path;
        }
	}
}
