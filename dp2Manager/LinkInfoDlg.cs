using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.GUI;
using DigitalPlatform.rms.Client;

namespace dp2Manager
{
	/// <summary>
	/// Summary description for LinkInfoDlg.
	/// </summary>
	public class LinkInfoDlg : System.Windows.Forms.Form
	{
		public string CreateNewServerPath = "";

		public LinkInfoCollection LinkInfos = null;

		private System.Windows.Forms.ListView listView_linkInfo;
		private System.Windows.Forms.ColumnHeader columnHeader_serverPath;
		private System.Windows.Forms.ColumnHeader columnHeader_localPath;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public LinkInfoDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LinkInfoDlg));
            this.listView_linkInfo = new System.Windows.Forms.ListView();
            this.columnHeader_serverPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_localPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView_linkInfo
            // 
            this.listView_linkInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_linkInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_serverPath,
            this.columnHeader_localPath});
            this.listView_linkInfo.FullRowSelect = true;
            this.listView_linkInfo.HideSelection = false;
            this.listView_linkInfo.Location = new System.Drawing.Point(12, 12);
            this.listView_linkInfo.Name = "listView_linkInfo";
            this.listView_linkInfo.Size = new System.Drawing.Size(368, 223);
            this.listView_linkInfo.TabIndex = 0;
            this.listView_linkInfo.UseCompatibleStateImageBehavior = false;
            this.listView_linkInfo.View = System.Windows.Forms.View.Details;
            this.listView_linkInfo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_linkInfo_MouseUp);
            // 
            // columnHeader_serverPath
            // 
            this.columnHeader_serverPath.Text = "服务器路径";
            this.columnHeader_serverPath.Width = 203;
            // 
            // columnHeader_localPath
            // 
            this.columnHeader_localPath.Text = "本地路径";
            this.columnHeader_localPath.Width = 210;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(224, 241);
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
            this.button_Cancel.Location = new System.Drawing.Point(305, 241);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "放弃";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // LinkInfoDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(392, 276);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_linkInfo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LinkInfoDlg";
            this.ShowInTaskbar = false;
            this.Text = "配置关联目录";
            this.Load += new System.EventHandler(this.LinkInfoDlg_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void LinkInfoDlg_Load(object sender, System.EventArgs e)
		{
		
			if (this.LinkInfos != null)
				FillList();

			if (CreateNewServerPath != "")
			{
				this.NewItem(CreateNewServerPath);
			}
		}

		void FillList()
		{
			this.listView_linkInfo.Items.Clear();

			for(int i=0;i<LinkInfos.Count;i++)
			{
				LinkInfo info = (LinkInfo)LinkInfos[i];

				ListViewItem item = new ListViewItem(ResPath.GetReverseRecordPath(info.ServerPath), 0);
				item.SubItems.Add(info.LocalPath);

				this.listView_linkInfo.Items.Add(item);
			}


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

		private void listView_linkInfo_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			bool bSelected = false;
			if (this.listView_linkInfo.SelectedIndices.Count > 0)
				bSelected = true;


			ContextMenu contextMenu = new ContextMenu();
			MenuItem menuItem = null;

			menuItem = new MenuItem("修改(&M)");
			menuItem.Click += new System.EventHandler(this.menu_modify_Click);
			if (bSelected == false)
				menuItem.Enabled = false;
			contextMenu.MenuItems.Add(menuItem);

			menuItem = new MenuItem("新增(&N)");
			menuItem.Click += new System.EventHandler(this.menu_new_Click);
			contextMenu.MenuItems.Add(menuItem);

			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			menuItem = new MenuItem("删除(&D)");
			menuItem.Click += new System.EventHandler(this.menu_delete_Click);
			if (bSelected == false)
				menuItem.Enabled = false;
			contextMenu.MenuItems.Add(menuItem);

			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			menuItem = new MenuItem("下载服务器端相关文件到本地(&O)");
			menuItem.Click += new System.EventHandler(this.menu_downloadFiles_Click);
			if (bSelected == false)
				menuItem.Enabled = false;
			contextMenu.MenuItems.Add(menuItem);



			contextMenu.Show(this.listView_linkInfo, new Point(e.X, e.Y) );			
		}

		void menu_modify_Click(object sender, System.EventArgs e)
		{
			Debug.Assert(this.LinkInfos != null, "");


			if (this.listView_linkInfo.SelectedIndices.Count == 0)
			{
				MessageBox.Show(this, "尚未选定要修改的事项");
				return;
			}

			OneLinkInfoDlg dlg = new OneLinkInfoDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Text = "修改一个连接关系";
			dlg.textBox_serverPath.Text = this.listView_linkInfo.SelectedItems[0].Text;
			dlg.textBox_localPath.Text = this.listView_linkInfo.SelectedItems[0].SubItems[1].Text;

			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			LinkInfo info = (LinkInfo)LinkInfos[this.listView_linkInfo.SelectedIndices[0]];
			info.ServerPath = dlg.textBox_serverPath.Text;
			info.LocalPath = dlg.textBox_localPath.Text;

			this.LinkInfos.Changed = true;

			FillList();

			int nRet = 0;
			string strError = "";
			nRet = info.Link(out strError);
			if (nRet == -1)
				MessageBox.Show(this, strError);
		}

		void menu_new_Click(object sender, System.EventArgs e)
		{

			this.NewItem("");
		}

		void NewItem(string strServerPath)
		{
			Debug.Assert(this.LinkInfos != null, "");

			OneLinkInfoDlg dlg = new OneLinkInfoDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Text = "指定一个新的连接关系";
			dlg.textBox_serverPath.Text = strServerPath;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			LinkInfo info = new LinkInfo();
			info.ServerPath = dlg.textBox_serverPath.Text;
			info.LocalPath = dlg.textBox_localPath.Text;

			this.LinkInfos.Add(info);
			this.LinkInfos.Changed = true;

			FillList();

			int nRet = 0;
			string strError = "";
			nRet = info.Link(out strError);
			if (nRet == -1)
				MessageBox.Show(this, strError);
		}

		void menu_delete_Click(object sender, System.EventArgs e)
		{
			Debug.Assert(this.LinkInfos != null, "");

			if (this.listView_linkInfo.SelectedIndices.Count == 0)
			{
				MessageBox.Show(this, "尚未选定要删除的事项");
				return;
			}
			this.LinkInfos.RemoveAt(this.listView_linkInfo.SelectedIndices[0]);
			this.LinkInfos.Changed = true;

			FillList();
		}

		void menu_downloadFiles_Click(object sender, System.EventArgs e)
		{
			Debug.Assert(this.LinkInfos != null, "");

			if (this.listView_linkInfo.SelectedIndices.Count == 0)
			{
				MessageBox.Show(this, "尚未选定要删除的事项");
				return;
			}
			LinkInfo info = (LinkInfo)LinkInfos[this.listView_linkInfo.SelectedIndices[0]];

			string strError;
			int nRet = info.DownloadFilesToLocalDir(out strError);
			if (nRet == -1)
				MessageBox.Show(this, strError);
			else 
			{
				MessageBox.Show(this, "下载完成");
			}
		}
	}
}
