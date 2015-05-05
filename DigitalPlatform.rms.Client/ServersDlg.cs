using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using DigitalPlatform.GUI;

namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// Summary description for ServersDlg.
	/// </summary>
	public class ServersDlg : System.Windows.Forms.Form
	{
		public ServerCollection Servers = null;	// 引用

		bool m_bChanged = false;

		public System.Windows.Forms.ListView ListView;
		private System.Windows.Forms.ColumnHeader columnHeader_url;
		private System.Windows.Forms.ColumnHeader columnHeader_userName;
		private System.Windows.Forms.ColumnHeader columnHeader_savePassword;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
        private Button button_newServer;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ServersDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServersDlg));
            this.ListView = new System.Windows.Forms.ListView();
            this.columnHeader_url = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_userName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_savePassword = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_newServer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ListView
            // 
            this.ListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_url,
            this.columnHeader_userName,
            this.columnHeader_savePassword});
            this.ListView.FullRowSelect = true;
            this.ListView.HideSelection = false;
            this.ListView.Location = new System.Drawing.Point(12, 12);
            this.ListView.Name = "ListView";
            this.ListView.Size = new System.Drawing.Size(440, 192);
            this.ListView.TabIndex = 0;
            this.ListView.UseCompatibleStateImageBehavior = false;
            this.ListView.View = System.Windows.Forms.View.Details;
            this.ListView.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.ListView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp);
            // 
            // columnHeader_url
            // 
            this.columnHeader_url.Text = "服务器URL";
            this.columnHeader_url.Width = 200;
            // 
            // columnHeader_userName
            // 
            this.columnHeader_userName.Text = "用户名";
            this.columnHeader_userName.Width = 150;
            // 
            // columnHeader_savePassword
            // 
            this.columnHeader_savePassword.Text = "是否保存密码";
            this.columnHeader_savePassword.Width = 150;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(296, 241);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(377, 241);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_newServer
            // 
            this.button_newServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_newServer.Location = new System.Drawing.Point(12, 210);
            this.button_newServer.Name = "button_newServer";
            this.button_newServer.Size = new System.Drawing.Size(113, 23);
            this.button_newServer.TabIndex = 3;
            this.button_newServer.Text = "新增服务器(&N)";
            this.button_newServer.UseVisualStyleBackColor = true;
            this.button_newServer.Click += new System.EventHandler(this.button_newServer_Click);
            // 
            // ServersDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(464, 276);
            this.Controls.Add(this.button_newServer);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.ListView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ServersDlg";
            this.ShowInTaskbar = false;
            this.Text = "服务器地址和缺省帐户管理";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ServersDlg_Closing);
            this.Load += new System.EventHandler(this.ServersDlg_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void ServersDlg_Load(object sender, System.EventArgs e)
		{
			FillList();
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			// OK和Cancel退出本对话框,其实 Servers中的内容已经修改。
			// 为了让Cancel退出有放弃整体修改的效果，请调主在初始化Servers
			// 属性的时候用一个克隆的ServerCollection对象。
		
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		void FillList()
		{
			ListView.Items.Clear();

			if (Servers == null)
				return;

			for(int i = 0;i<Servers.Count; i++)
			{
				Server server = (Server)Servers[i];

				ListViewItem item = new ListViewItem(server.Url, 0);

				ListView.Items.Add(item);

				item.SubItems.Add(server.DefaultUserName);
				item.SubItems.Add(server.SavePassword == true ? "是" : "否");

			}


		}

		private void listView1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			ContextMenu contextMenu = new ContextMenu();
			MenuItem menuItem = null;

			bool bSelected = ListView.SelectedItems.Count > 0;

			//
			menuItem = new MenuItem("修改(&M)");
			menuItem.Click += new System.EventHandler(this.menu_modifyServer);
			if (bSelected == false) 
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);


			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			menuItem = new MenuItem("删除(&D)");
			menuItem.Click += new System.EventHandler(this.menu_deleteServer);
			if (bSelected == false)
				menuItem.Enabled = false;
			contextMenu.MenuItems.Add(menuItem);

			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			//
			menuItem = new MenuItem("新增(&N)");
			menuItem.Click += new System.EventHandler(this.menu_newServer);
			contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("上移(&U)");
            menuItem.Click += new System.EventHandler(this.menu_up);
            if (ListViewUtil.MoveItemEnabled(this.ListView, true) == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);
            //
            menuItem = new MenuItem("下移(&N)");
            menuItem.Click += new System.EventHandler(this.menu_down);
            if (ListViewUtil.MoveItemEnabled(this.ListView, false) == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

			contextMenu.Show(ListView, new Point(e.X, e.Y) );		
		}

        void menu_up(object sender, System.EventArgs e)
        {
            string strError = "";
            List<int> indices = null;

            if (ListViewUtil.MoveItemUpDown(this.ListView,
                true,
                out indices,
                out strError) == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            if (indices != null
                && indices.Count >= 2)
            {
                Server o = (Server)Servers[indices[0]];
                Servers.RemoveAt(indices[0]);
                Servers.Insert(indices[1], o);

                Servers.Changed = true;
                m_bChanged = true;
            }
        }

        void menu_down(object sender, System.EventArgs e)
        {
            string strError = "";
            List<int> indices = null;

            if (ListViewUtil.MoveItemUpDown(this.ListView,
                false,
                out indices,
                out strError) == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            if (indices != null
                && indices.Count >= 2)
            {
                Server o = (Server)Servers[indices[0]];
                Servers.RemoveAt(indices[0]);
                Servers.Insert(indices[1], o);

                Servers.Changed = true;
                m_bChanged = true;
            }
        }

		void menu_deleteServer(object sender, System.EventArgs e)
		{
			if (ListView.SelectedIndices.Count == 0)
			{
				MessageBox.Show(this, "尚未选择要删除的事项 ...");
				return;
			}

			DialogResult msgResult = MessageBox.Show(this,
				"确实要删除所选择的事项",
				"ServersDlg",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);

			if (msgResult != DialogResult.Yes) 
			{
				return;
			}

			for(int i=ListView.SelectedIndices.Count-1;i>=0;i--)
			{
				Servers.RemoveAt(ListView.SelectedIndices[i]);
			}

			Servers.Changed = true;

			FillList();

			m_bChanged = true;
		}
		

		void menu_modifyServer(object sender, System.EventArgs e)
		{
			if (ListView.SelectedIndices.Count == 0)
			{
				MessageBox.Show(this, "尚未选择要修改的事项 ...");
				return;
			}

			int nActiveLine = ListView.SelectedIndices[0];
			// ListViewItem item = listView1.Items[nActiveLine];

			LoginDlg dlg = new LoginDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

			dlg.Text = "修改缺省帐户参数";

			dlg.textBox_password.Text = ((Server)Servers[nActiveLine]).DefaultPassword;
			dlg.textBox_serverAddr.Text = ((Server)Servers[nActiveLine]).Url;
			dlg.textBox_userName.Text = ((Server)Servers[nActiveLine]).DefaultUserName;
			dlg.checkBox_savePassword.Checked = ((Server)Servers[nActiveLine]).SavePassword;

			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			((Server)Servers[nActiveLine]).DefaultPassword = dlg.textBox_password.Text;
			((Server)Servers[nActiveLine]).Url = dlg.textBox_serverAddr.Text;
			((Server)Servers[nActiveLine]).DefaultUserName = dlg.textBox_userName.Text;
			((Server)Servers[nActiveLine]).SavePassword = dlg.checkBox_savePassword.Checked;

			Servers.Changed = true;

			FillList();

		// 选择一行
		// parameters:
		//		nIndex	要设置选择标记的行。如果==-1，表示清除全部选择标记但不选择。
		//		bMoveFocus	是否同时移动focus标志到所选择行
			ListViewUtil.SelectLine(ListView, 
				nActiveLine,
				true);

			m_bChanged = true;

		}


		void menu_newServer(object sender, System.EventArgs e)
		{
			int nActiveLine = -1;
			if (ListView.SelectedIndices.Count != 0)
			{
				nActiveLine = ListView.SelectedIndices[0];
			}


			// ListViewItem item = listView1.Items[nActiveLine];

			LoginDlg dlg = new LoginDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

			dlg.Text = "新增服务器地址和缺省帐户";

            if (nActiveLine == -1)
            {   
                // 无参考事项情形的新增
                dlg.textBox_serverAddr.Text = "http://dp2003.com/dp2kernel";
                dlg.textBox_userName.Text = "public";
            }
            else
			{
				dlg.textBox_password.Text = ((Server)Servers[nActiveLine]).DefaultPassword;
				dlg.textBox_serverAddr.Text = ((Server)Servers[nActiveLine]).Url;
				dlg.textBox_userName.Text = ((Server)Servers[nActiveLine]).DefaultUserName;
				dlg.checkBox_savePassword.Checked = ((Server)Servers[nActiveLine]).SavePassword;
			}

			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			Server server = Servers.NewServer(nActiveLine);
			server.DefaultPassword = dlg.textBox_password.Text;
			server.Url = dlg.textBox_serverAddr.Text;
			server.DefaultUserName = dlg.textBox_userName.Text;
			server.SavePassword = dlg.checkBox_savePassword.Checked;

			Servers.Changed = true;

			FillList();

			// 选择一行
			// parameters:
			//		nIndex	要设置选择标记的行。如果==-1，表示清除全部选择标记但不选择。
			//		bMoveFocus	是否同时移动focus标志到所选择行
			ListViewUtil.SelectLine(ListView, 
				Servers.Count - 1,
				true);

			m_bChanged = true;

		}


		private void listView1_DoubleClick(object sender, System.EventArgs e)
		{
			menu_modifyServer(null, null);
		}

		private void ServersDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.DialogResult != DialogResult.OK)
			{
				if (m_bChanged == true)
				{
					DialogResult msgResult = MessageBox.Show(this,
						"要放弃在对话框中所做的全部修改么?",
						"ServersDlg",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2);
					if (msgResult == DialogResult.No) 
					{
						e.Cancel = true;
						return;
					}
				}
			}

		}

        private void button_newServer_Click(object sender, EventArgs e)
        {
            menu_newServer(null, null);
        }

	}
}
