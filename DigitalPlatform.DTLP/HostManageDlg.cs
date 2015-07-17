using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using DigitalPlatform.Xml;
using DigitalPlatform.DTLP;

namespace DigitalPlatform.DTLP
{
	/// <summary>
	/// Summary description for HostManageDlg.
	/// </summary>
	public class HostManageDlg : System.Windows.Forms.Form
	{
		public ApplicationInfo	applicationInfo = null;

		bool bChanged = false;

		private System.Windows.Forms.Button button_delHost;
		private System.Windows.Forms.Button button_ModiHost;
		private System.Windows.Forms.Button button_newHost;
		private System.Windows.Forms.ListView listView_hosts;
		private System.Windows.Forms.ColumnHeader columnHeader_address;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Button button_OK;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public HostManageDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HostManageDlg));
            this.button_delHost = new System.Windows.Forms.Button();
            this.button_ModiHost = new System.Windows.Forms.Button();
            this.button_newHost = new System.Windows.Forms.Button();
            this.listView_hosts = new System.Windows.Forms.ListView();
            this.columnHeader_address = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_delHost
            // 
            this.button_delHost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_delHost.Location = new System.Drawing.Point(372, 219);
            this.button_delHost.Name = "button_delHost";
            this.button_delHost.Size = new System.Drawing.Size(67, 22);
            this.button_delHost.TabIndex = 3;
            this.button_delHost.Text = "删除";
            this.button_delHost.Click += new System.EventHandler(this.button_delHost_Click);
            // 
            // button_ModiHost
            // 
            this.button_ModiHost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_ModiHost.Location = new System.Drawing.Point(96, 219);
            this.button_ModiHost.Name = "button_ModiHost";
            this.button_ModiHost.Size = new System.Drawing.Size(82, 22);
            this.button_ModiHost.TabIndex = 2;
            this.button_ModiHost.Text = "修改(&M)...";
            this.button_ModiHost.Click += new System.EventHandler(this.button_ModiHost_Click);
            // 
            // button_newHost
            // 
            this.button_newHost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_newHost.Location = new System.Drawing.Point(9, 219);
            this.button_newHost.Name = "button_newHost";
            this.button_newHost.Size = new System.Drawing.Size(83, 22);
            this.button_newHost.TabIndex = 1;
            this.button_newHost.Text = "新增(&N)...";
            this.button_newHost.Click += new System.EventHandler(this.button_newHost_Click);
            // 
            // listView_hosts
            // 
            this.listView_hosts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_hosts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_address});
            this.listView_hosts.FullRowSelect = true;
            this.listView_hosts.HideSelection = false;
            this.listView_hosts.Location = new System.Drawing.Point(9, 9);
            this.listView_hosts.Name = "listView_hosts";
            this.listView_hosts.Size = new System.Drawing.Size(430, 206);
            this.listView_hosts.TabIndex = 0;
            this.listView_hosts.UseCompatibleStateImageBehavior = false;
            this.listView_hosts.View = System.Windows.Forms.View.Details;
            this.listView_hosts.SelectedIndexChanged += new System.EventHandler(this.listView_hosts_SelectedIndexChanged);
            this.listView_hosts.DoubleClick += new System.EventHandler(this.listView_hosts_DoubleClick);
            // 
            // columnHeader_address
            // 
            this.columnHeader_address.Text = "服务器地址";
            this.columnHeader_address.Width = 426;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.AutoSize = true;
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(372, 253);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(67, 22);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(300, 253);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(67, 22);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // HostManageDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(448, 285);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_delHost);
            this.Controls.Add(this.button_ModiHost);
            this.Controls.Add(this.button_newHost);
            this.Controls.Add(this.listView_hosts);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HostManageDlg";
            this.ShowInTaskbar = false;
            this.Text = "服务器地址配置";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.HostManageDlg_Closing);
            this.Load += new System.EventHandler(this.HostManageDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void HostManageDlg_Load(object sender, System.EventArgs e)
		{
		
			FillHostsList();

			listView_hosts_SelectedIndexChanged(null, null);
		}

		private void button_newHost_Click(object sender, System.EventArgs e)
		{
			HostNameDlg dlg = new HostNameDlg();
            dlg.Font = this.Font;
			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);

			if (dlg.DialogResult == DialogResult.OK) 
			{
				ListViewItem item =
					new ListViewItem(dlg.textBox_hostAddress.Text,
					0);

				listView_hosts.Items.Add(item);

				bChanged = true;
			}
		}

		private void button_ModiHost_Click(object sender, System.EventArgs e)
		{
			if (listView_hosts.SelectedItems.Count == 0) 
			{
				MessageBox.Show(this, "请先选择要修改的host事项");
				return;
			}


			HostNameDlg dlg = new HostNameDlg();

            dlg.Font = this.Font;
            dlg.textBox_hostAddress.Text = listView_hosts.SelectedItems[0].Text;
			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);

			if (dlg.DialogResult == DialogResult.OK) 
			{
				listView_hosts.SelectedItems[0].Text = dlg.textBox_hostAddress.Text;

				bChanged = true;
			}

		}

		private void button_delHost_Click(object sender, System.EventArgs e)
		{
			if (listView_hosts.SelectedIndices.Count == 0) 
			{
				MessageBox.Show(this, "请先选择要删除的host事项");
				return;
			}	
	
			for(int i=listView_hosts.SelectedIndices.Count-1;i>=0;i--)
			{
				listView_hosts.Items.RemoveAt(listView_hosts.SelectedIndices[i]);
			}

			bChanged = true;
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			SaveHostList();

			this.DialogResult = DialogResult.OK;
			this.Close();

		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		void FillHostsList()
		{
			listView_hosts.Items.Clear();

			ArrayList aHost = HostArray.LoadHosts(applicationInfo);

			for(int i=0; i<aHost.Count; i++) 
			{

				ListViewItem item =
					new ListViewItem((string)aHost[i],
					0);
				//item.SubItems.Add(Convert.ToString(retItemList[i].Length));

				listView_hosts.Items.Add(item);
			}
		}

		void SaveHostList()
		{
			ArrayList aHost = new ArrayList();
			for(int i=0;i<listView_hosts.Items.Count; i++) 
			{
				aHost.Add(listView_hosts.Items[i].Text);
			}
			HostArray.SaveHosts(applicationInfo,
				aHost);

			//记住save,保存信息XML文件
			applicationInfo.Save();

			applicationInfo = null;	// 避免后面再用这个对象

			bChanged = false;
		}

		private void listView_hosts_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (listView_hosts.SelectedItems.Count == 0) 
			{
				button_delHost.Enabled = false;
				button_ModiHost.Enabled = false;
			}
			else 
			{
				button_delHost.Enabled = true;
				button_ModiHost.Enabled = true;
			}
	
		}

		private void HostManageDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (bChanged == true) 
			{
				DialogResult msgResult = MessageBox.Show(this,
					"服务器地址已经被修改。\r\n是否保存?",
					"DtlpResDirControl",
					MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1);
				if (msgResult == DialogResult.Cancel) 
				{
					e.Cancel = true;
					return;
				}

				if (msgResult == DialogResult.Yes) 
				{
					SaveHostList();
					return;
				}
			}
		}

        private void listView_hosts_DoubleClick(object sender, EventArgs e)
        {
            button_ModiHost_Click(null, null);
        }

	}
}
