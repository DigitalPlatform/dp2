using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DigitalPlatform.Script
{
	/// <summary>
	/// Summary description for ScriptActionMenuDlg.
	/// </summary>
	public class ScriptActionMenuDlgOld : System.Windows.Forms.Form
	{
		public ScriptActionCollection Actions = null;

		public int SelectedIndex = -1;
		public ScriptAction SelectedAction = null;

        private DigitalPlatform.GUI.ListViewNF listView1;
		private System.Windows.Forms.ColumnHeader columnHeader_name;
		private System.Windows.Forms.ColumnHeader columnHeader_comment;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.ColumnHeader columnHeader_entry;
        private CheckBox checkBox_autoRun;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ScriptActionMenuDlgOld()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptActionMenuDlgOld));
            this.listView1 = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_entry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_autoRun = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_comment,
            this.columnHeader_entry});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(9, 9);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(399, 249);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "名称";
            this.columnHeader_name.Width = 148;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "说明";
            this.columnHeader_comment.Width = 204;
            // 
            // columnHeader_entry
            // 
            this.columnHeader_entry.Text = "入口函数";
            this.columnHeader_entry.Width = 150;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(253, 282);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 22);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.AutoSize = true;
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(333, 282);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 22);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_autoRun
            // 
            this.checkBox_autoRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_autoRun.AutoSize = true;
            this.checkBox_autoRun.Location = new System.Drawing.Point(9, 262);
            this.checkBox_autoRun.Name = "checkBox_autoRun";
            this.checkBox_autoRun.Size = new System.Drawing.Size(138, 16);
            this.checkBox_autoRun.TabIndex = 1;
            this.checkBox_autoRun.Text = "自动执行加亮事项(&A)";
            this.checkBox_autoRun.UseVisualStyleBackColor = true;
            // 
            // ScriptActionMenuDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(418, 313);
            this.Controls.Add(this.checkBox_autoRun);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ScriptActionMenuDlg";
            this.ShowInTaskbar = false;
            this.Text = "选择脚本功能";
            this.Load += new System.EventHandler(this.ScriptActionMenuDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void ScriptActionMenuDlg_Load(object sender, System.EventArgs e)
		{
			FillList();

            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                // 旁路
            }
            else
            {
                // 自动执行
                if (this.AutoRun == true)
                {
                    if (this.listView1.SelectedItems.Count == 1)
                    {
                        button_OK_Click(null, null);
                        return;
                    }

                }
            }
		}

		void FillList()
		{
			listView1.Items.Clear();
			if (Actions == null)
				return;

            ListViewItem first_item = null;

			for(int i=0;i<Actions.Count;i++)
			{
				ScriptAction action = (ScriptAction)Actions[i];

				ListViewItem item = new ListViewItem(action.Name, 0);
				item.SubItems.Add(action.Comment);
				item.SubItems.Add(action.ScriptEntry);

				if (action.Active == true)
                {
					item.Selected = true;

                    // 2009/2/24
                    if (first_item == null)
                        first_item = item;
                }

				listView1.Items.Add(item);
			}

            // 2009/2/24
            if (first_item != null)
            {
                listView1.FocusedItem = first_item;
                first_item.EnsureVisible();
            }
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (listView1.SelectedItems.Count == 0) 
			{
				MessageBox.Show(this, "尚未选择事项...");
				return;
			}

			this.SelectedIndex = listView1.SelectedIndices[0];
			if (Actions != null)
				this.SelectedAction = (ScriptAction)this.Actions[this.SelectedIndex];
			else
				this.SelectedAction = null;

		
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void listView1_DoubleClick(object sender, System.EventArgs e)
		{
			button_OK_Click(null, null);
		
		}

        // 是否自动运行
        public bool AutoRun
        {
            get
            {
                return checkBox_autoRun.Checked;
            }
            set
            {
                checkBox_autoRun.Checked = value;
            }
        }
	}
}
