using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.GUI;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonDialog;

namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// Summary description for ResRightList.
	/// </summary>
	public class ResRightList : System.Windows.Forms.ListView
	{
		public ResRightTree ResTree = null;
		public string Path = "";

		private System.Windows.Forms.ColumnHeader columnHeader_name;
		private System.Windows.Forms.ColumnHeader columnHeader_rights;
		private System.Windows.Forms.ImageList imageList_resIcon;
		private System.Windows.Forms.ColumnHeader columnHeader_state;
		private System.ComponentModel.IContainer components;

		public ResRightList()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.LargeImageList = imageList_resIcon;
			this.SmallImageList = imageList_resIcon;

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResRightList));
            this.columnHeader_name = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_rights = new System.Windows.Forms.ColumnHeader();
            this.imageList_resIcon = new System.Windows.Forms.ImageList(this.components);
            this.columnHeader_state = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "下级对象名";
            this.columnHeader_name.Width = 150;
            // 
            // columnHeader_rights
            // 
            this.columnHeader_rights.Text = "权限";
            this.columnHeader_rights.Width = 300;
            // 
            // imageList_resIcon
            // 
            this.imageList_resIcon.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_resIcon.ImageStream")));
            this.imageList_resIcon.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_resIcon.Images.SetKeyName(0, "");
            this.imageList_resIcon.Images.SetKeyName(1, "");
            this.imageList_resIcon.Images.SetKeyName(2, "");
            this.imageList_resIcon.Images.SetKeyName(3, "");
            this.imageList_resIcon.Images.SetKeyName(4, "");
            this.imageList_resIcon.Images.SetKeyName(5, "");
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "状态";
            this.columnHeader_state.Width = 150;
            // 
            // ResRightList
            // 
            this.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_rights,
            this.columnHeader_state});
            this.View = System.Windows.Forms.View.Details;
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ResRightList_MouseUp);
            this.ResumeLayout(false);

		}
		#endregion

		public void Initial(ResRightTree restree)
		{
			this.ResTree = restree;

			Fill();

            this.ResTree.OnNodeRightsChanged -= new NodeRightsChangedEventHandle(ResTree_OnNodeRightsChanged);
            this.ResTree.OnNodeRightsChanged += new NodeRightsChangedEventHandle(ResTree_OnNodeRightsChanged);
		}

        void ResTree_OnNodeRightsChanged(object sender, NodeRightsChangedEventArgs e)
        {
            // 令显示正确

            NodeInfo nodeinfo = null;
            for (int i = 0; i < this.Items.Count; i++)
            {
                ListViewItem item = this.Items[i];
                nodeinfo = (NodeInfo)item.Tag;

                if (nodeinfo == null)
                    continue;

                // nodeinfo由于和tree共享，所以内存是同步的，不用修改
                if (nodeinfo.TreeNode == e.Node)
                {
                    item.SubItems[1].Text = e.Rights;

                    if (nodeinfo.Rights == "")
                        item.ForeColor = SystemColors.GrayText;	// ControlPaint.LightLight(nodeNew.ForeColor);
                    else
                        item.ForeColor = SystemColors.WindowText;
                }

            }

        }


        public int Fill()
        {

            this.Items.Clear();

            Debug.Assert(this.ResTree != null, "this.ResTree == null错误");
            TreeNode curtreenode = this.ResTree.SelectedNode;

            if (curtreenode == null)
                return 0;


            for (int i = 0; i < curtreenode.Nodes.Count; i++)
            {
                TreeNode child = curtreenode.Nodes[i];
                ListViewItem item = new ListViewItem(child.Text, child.ImageIndex);

                NodeInfo nodeinfo = (NodeInfo)child.Tag;
                item.Tag = nodeinfo;
                if (nodeinfo != null)
                {
                    item.SubItems.Add(nodeinfo.Rights);
                    string strState = "";
                    strState = NodeInfo.GetNodeStateString(nodeinfo);
                    /*
                    if ((nodeinfo.NodeState & NodeState.Account) == NodeState.Account)
                        strState = "帐户定义";
                    if ((nodeinfo.NodeState & NodeState.Object) == NodeState.Object)
                    {
                        if (strState != "")
                            strState += ",";
                        strState = "对象";
                    }
                    */

                    item.SubItems.Add(strState);
                }

                if (nodeinfo == null || nodeinfo.Rights == "")
                    item.ForeColor = SystemColors.GrayText;	// ControlPaint.LightLight(nodeNew.ForeColor);
                else
                    item.ForeColor = SystemColors.WindowText;


                this.Items.Add(item);
            }

            return 0;
        }

		private void ResRightList_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			ContextMenu contextMenu = new ContextMenu();
			MenuItem menuItem = null;

			menuItem = new MenuItem("权限(&R)");
			menuItem.Click += new System.EventHandler(this.menu_editRights_Click);
			contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新(&F)");
            menuItem.Click += new System.EventHandler(this.menu_refresh_Click);
            contextMenu.MenuItems.Add(menuItem);


			contextMenu.Show(this, new Point(e.X, e.Y) );		
		}

        // 刷新
        void menu_refresh_Click(object sender, EventArgs e)
        {
            this.RefreshList();
        }

        public void RefreshList()
        {
            this.Fill();
        }

        /*
		// 编辑权限
		private void menu_editRights_Click(object sender, System.EventArgs e)
		{
			if (this.SelectedItems.Count == 0)
			{
				MessageBox.Show("尚未选择要编辑的事项...");
				return;
			}

			DigitalPlatform.CommonDialog.PropertyDlg dlg = new DigitalPlatform.CommonDialog.PropertyDlg();

			NodeInfo nodeinfo = (NodeInfo)this.SelectedItems[0].Tag;

			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.Text = "对象 '"+ this.SelectedItems[0].Text +"' 的权限";
			dlg.PropertyString = nodeinfo.Rights;
			dlg.CfgFileName = this.ResTree.PropertyCfgFileName;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;


			for(int i=0;i<this.SelectedItems.Count;i++)
			{
				// 令显示正确
				this.SelectedItems[i].SubItems[1].Text = dlg.PropertyString;
				// 令内存正确
				nodeinfo = (NodeInfo)this.SelectedItems[i].Tag;
				nodeinfo.Rights = dlg.PropertyString;

				if (nodeinfo.Rights == "")
					this.SelectedItems[i].ForeColor = SystemColors.GrayText;	// ControlPaint.LightLight(nodeNew.ForeColor);
				else
					this.SelectedItems[i].ForeColor = SystemColors.WindowText;

				nodeinfo.TreeNode.ForeColor = this.SelectedItems[i].ForeColor;

                this.ResTree.SetNodeRights(nodeinfo.TreeNode, nodeinfo.Rights);
			}

			this.ResTree.Changed = true;
		}
         */

        // 编辑权限
        private void menu_editRights_Click(object sender, System.EventArgs e)
        {
            if (this.SelectedItems.Count == 0)
            {
                MessageBox.Show("尚未选择要编辑的事项...");
                return;
            }

            DigitalPlatform.CommonDialog.CategoryPropertyDlg dlg = new DigitalPlatform.CommonDialog.CategoryPropertyDlg();

            NodeInfo nodeinfo = (NodeInfo)this.SelectedItems[0].Tag;

            string strRights = "";
            DialogResult result = this.ResTree.NodeRightsDlg(nodeinfo.TreeNode,
                out strRights);
            if (result != DialogResult.OK)
                return;

            // this.ResTree.SetNodeRights(nodeinfo.TreeNode, strRights);

            for (int i = 0; i < this.SelectedItems.Count; i++)
            {
                nodeinfo = (NodeInfo)this.SelectedItems[i].Tag;
                // nodeinfo.Rights = strRights;

                this.ResTree.SetNodeRights(nodeinfo.TreeNode, strRights);
            }
        }

	}
}
