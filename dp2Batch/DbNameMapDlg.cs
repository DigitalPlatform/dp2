using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using DigitalPlatform.rms.Client;
using DigitalPlatform.GUI;
using DigitalPlatform.Library;

namespace dp2Batch
{
	/// <summary>
	/// Summary description for DbNameMapDlg.
	/// </summary>
	public class DbNameMapDlg : System.Windows.Forms.Form
	{
		public SearchPanel SearchPanel = null;
		public DbNameMap DbNameMap = new DbNameMap();

		private System.Windows.Forms.ListView listView_map;
		private System.Windows.Forms.ColumnHeader columnHeader_origin;
		private System.Windows.Forms.ColumnHeader columnHeader_target;
		private System.Windows.Forms.ColumnHeader columnHeader_writeMode;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.ToolTip toolTip_map;
		private System.ComponentModel.IContainer components;

		public DbNameMapDlg()
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DbNameMapDlg));
            this.listView_map = new System.Windows.Forms.ListView();
            this.columnHeader_origin = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_target = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_writeMode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.toolTip_map = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // listView_map
            // 
            this.listView_map.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_map.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_origin,
            this.columnHeader_target,
            this.columnHeader_writeMode});
            this.listView_map.FullRowSelect = true;
            this.listView_map.HideSelection = false;
            this.listView_map.Location = new System.Drawing.Point(12, 12);
            this.listView_map.Name = "listView_map";
            this.listView_map.Size = new System.Drawing.Size(402, 224);
            this.listView_map.TabIndex = 0;
            this.listView_map.UseCompatibleStateImageBehavior = false;
            this.listView_map.View = System.Windows.Forms.View.Details;
            this.listView_map.DoubleClick += new System.EventHandler(this.listView_map_DoubleClick);
            this.listView_map.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView_map_MouseMove);
            this.listView_map.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_map_MouseUp);
            // 
            // columnHeader_origin
            // 
            this.columnHeader_origin.Text = "源";
            this.columnHeader_origin.Width = 209;
            // 
            // columnHeader_target
            // 
            this.columnHeader_target.Text = "目标";
            this.columnHeader_target.Width = 265;
            // 
            // columnHeader_writeMode
            // 
            this.columnHeader_writeMode.Text = "写入方式";
            this.columnHeader_writeMode.Width = 115;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(258, 242);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 22);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(339, 242);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 22);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "放弃";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // DbNameMapDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(426, 276);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_map);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DbNameMapDlg";
            this.ShowInTaskbar = false;
            this.Text = "库名映射规则";
            this.Load += new System.EventHandler(this.DbNameMapDlg_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void DbNameMapDlg_Load(object sender, System.EventArgs e)
		{
			FillList();
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
            string strError = "";
            if (BuildMap(out strError) == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }
		
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
			this.listView_map.Items.Clear();

			for(int i=0;i<this.DbNameMap.Count;i++)
			{
				DbNameMapItem mapitem = this.DbNameMap[i];

				ListViewItem item = new ListViewItem(ResPath.GetReverseRecordPath(mapitem.Origin), 0);

				item.SubItems.Add(ResPath.GetReverseRecordPath(mapitem.Target));
				item.SubItems.Add(mapitem.Style);

				this.listView_map.Items.Add(item);
			}
		}

		int BuildMap(out string strError)
		{
            strError = "";

			this.DbNameMap.Clear();

			for(int i=0;i<this.listView_map.Items.Count;i++)
			{
				if (this.DbNameMap.NewItem(ResPath.GetRegularRecordPath(this.listView_map.Items[i].SubItems[0].Text),
					ResPath.GetRegularRecordPath(this.listView_map.Items[i].SubItems[1].Text),
					this.listView_map.Items[i].SubItems[2].Text,
                    out strError)== null)
                    return -1;
			}

            return 0;
		}

		private void listView_map_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ListViewItem selection = this.listView_map.GetItemAt(e.X, e.Y);

			if (selection != null)
			{
				string strText = "";
				int nRet = 	ListViewUtil.ColumnHitTest(this.listView_map,
					e.X);
				if (nRet == 0)
					strText = "源: " + selection.SubItems[0].Text;
				else if (nRet == 1)
					strText = "目标" + selection.SubItems[1].Text;

				this.toolTip_map.SetToolTip(this.listView_map,
					strText);
			}
			else
				this.toolTip_map.SetToolTip(this.listView_map, null);
		
		}

		private void listView_map_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			ContextMenu contextMenu = new ContextMenu();
			MenuItem menuItem = null;

			bool bSelected = this.listView_map.SelectedItems.Count > 0;

			//
			menuItem = new MenuItem("修改(&M)");
			menuItem.Click += new System.EventHandler(this.menu_modify);
			if (bSelected == false) 
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);

			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			menuItem = new MenuItem("删除(&D)");
			menuItem.Click += new System.EventHandler(this.menu_delete);
			if (bSelected == false)
				menuItem.Enabled = false;
			contextMenu.MenuItems.Add(menuItem);

			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			//
			menuItem = new MenuItem("新增(&N)");
			menuItem.Click += new System.EventHandler(this.menu_new);
			contextMenu.MenuItems.Add(menuItem);


			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			contextMenu.Show(this.listView_map, new Point(e.X, e.Y) );		
		}

		void menu_modify(object sender, EventArgs e)
		{
			if (this.listView_map.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "尚未选择要修改的事项");
				return;
			}

			DbNameMapItemDlg dlg = new DbNameMapItemDlg();
            dlg.Font = GuiUtil.GetDefaultFont();
            dlg.SearchPanel = this.SearchPanel;
			dlg.Origin = ResPath.GetRegularRecordPath(this.listView_map.SelectedItems[0].SubItems[0].Text);
			dlg.Target = ResPath.GetRegularRecordPath(this.listView_map.SelectedItems[0].SubItems[1].Text);
			dlg.WriteMode = this.listView_map.SelectedItems[0].SubItems[2].Text;

			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			this.listView_map.SelectedItems[0].SubItems[0].Text = ResPath.GetReverseRecordPath(dlg.Origin);
			this.listView_map.SelectedItems[0].SubItems[1].Text = ResPath.GetReverseRecordPath(dlg.Target);
			this.listView_map.SelectedItems[0].SubItems[2].Text = ResPath.GetReverseRecordPath(dlg.WriteMode);

		}

		void menu_delete(object sender, EventArgs e)
		{
			if (this.listView_map.SelectedItems.Count == 0)
			{
				MessageBox.Show(this, "尚未选择要删除的事项");
				return;
			}

			DialogResult result = MessageBox.Show(this,
				"确实要删除选定的事项?",
				"DbNameMapItemDlg",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);
			if (result == DialogResult.No) 
				return;

			ArrayList aIndex = new ArrayList();
			int i;

			for(i=0;i<this.listView_map.SelectedIndices.Count;i++)
			{
				aIndex.Add(this.listView_map.SelectedIndices[i]);
			}

			for(i=aIndex.Count - 1;i>=0;i--)
			{
				this.listView_map.Items.RemoveAt((int)aIndex[i]);
			}

		}

		void menu_new(object sender, EventArgs e)
		{
			DbNameMapItemDlg dlg = new DbNameMapItemDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

			dlg.SearchPanel = this.SearchPanel;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			ListViewItem item = new ListViewItem(ResPath.GetReverseRecordPath(dlg.Origin), 0);
			item.SubItems.Add(ResPath.GetReverseRecordPath(dlg.Target));
			item.SubItems.Add(dlg.WriteMode);

			this.listView_map.Items.Add(item);
		}

		private void listView_map_DoubleClick(object sender, System.EventArgs e)
		{
			menu_modify(null, null);
		}
	}
}
