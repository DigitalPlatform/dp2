namespace dp2Circulation
{
    partial class IssueManageControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IssueManageControl));
            this.TreeView = new System.Windows.Forms.TreeView();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_treeView = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip_treeTool = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_newIssue = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_newAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_moveUp = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_moveDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_delete = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel_orderInfo = new System.Windows.Forms.TableLayoutPanel();
            this.label_orderInfo_message = new System.Windows.Forms.Label();
            this.orderDesignControl1 = new DigitalPlatform.CommonControl.OrderDesignControl();
            this.imageList_treeIcon = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_treeView.SuspendLayout();
            this.toolStrip_treeTool.SuspendLayout();
            this.tableLayoutPanel_orderInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // TreeView
            // 
            this.TreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeView.HideSelection = false;
            this.TreeView.Location = new System.Drawing.Point(0, 0);
            this.TreeView.Margin = new System.Windows.Forms.Padding(0);
            this.TreeView.Name = "TreeView";
            this.TreeView.Size = new System.Drawing.Size(302, 141);
            this.TreeView.TabIndex = 0;
            this.TreeView.DoubleClick += new System.EventHandler(this.TreeView_DoubleClick);
            this.TreeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseUp);
            this.TreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_AfterSelect);
            this.TreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseDown);
            this.TreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeView_BeforeSelect);
            this.TreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeView_KeyDown);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tableLayoutPanel_treeView);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tableLayoutPanel_orderInfo);
            this.splitContainer_main.Size = new System.Drawing.Size(361, 267);
            this.splitContainer_main.SplitterDistance = 141;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 1;
            // 
            // tableLayoutPanel_treeView
            // 
            this.tableLayoutPanel_treeView.ColumnCount = 2;
            this.tableLayoutPanel_treeView.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_treeView.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_treeView.Controls.Add(this.TreeView, 0, 0);
            this.tableLayoutPanel_treeView.Controls.Add(this.toolStrip_treeTool, 1, 0);
            this.tableLayoutPanel_treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_treeView.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_treeView.Name = "tableLayoutPanel_treeView";
            this.tableLayoutPanel_treeView.RowCount = 1;
            this.tableLayoutPanel_treeView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_treeView.Size = new System.Drawing.Size(361, 141);
            this.tableLayoutPanel_treeView.TabIndex = 2;
            // 
            // toolStrip_treeTool
            // 
            this.toolStrip_treeTool.Dock = System.Windows.Forms.DockStyle.Right;
            this.toolStrip_treeTool.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_newIssue,
            this.toolStripButton_modify,
            this.toolStripSeparator2,
            this.toolStripButton_newAll,
            this.toolStripSeparator1,
            this.toolStripButton_moveUp,
            this.toolStripButton_moveDown,
            this.toolStripSeparator3,
            this.toolStripButton_delete});
            this.toolStrip_treeTool.Location = new System.Drawing.Point(302, 0);
            this.toolStrip_treeTool.Name = "toolStrip_treeTool";
            this.toolStrip_treeTool.Size = new System.Drawing.Size(59, 141);
            this.toolStrip_treeTool.TabIndex = 1;
            this.toolStrip_treeTool.Text = "toolStrip1";
            // 
            // toolStripButton_newIssue
            // 
            this.toolStripButton_newIssue.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_newIssue.Image")));
            this.toolStripButton_newIssue.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_newIssue.Name = "toolStripButton_newIssue";
            this.toolStripButton_newIssue.Size = new System.Drawing.Size(56, 22);
            this.toolStripButton_newIssue.Text = "增一";
            this.toolStripButton_newIssue.ToolTipText = "新增一个期节点";
            this.toolStripButton_newIssue.Click += new System.EventHandler(this.toolStripButton_newIssue_Click);
            // 
            // toolStripButton_modify
            // 
            this.toolStripButton_modify.Enabled = false;
            this.toolStripButton_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modify.Image")));
            this.toolStripButton_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modify.Name = "toolStripButton_modify";
            this.toolStripButton_modify.Size = new System.Drawing.Size(56, 22);
            this.toolStripButton_modify.Text = "修改";
            this.toolStripButton_modify.ToolTipText = "修改当前选中的期节点";
            this.toolStripButton_modify.Click += new System.EventHandler(this.toolStripButton_modify_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(56, 6);
            // 
            // toolStripButton_newAll
            // 
            this.toolStripButton_newAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_newAll.Image")));
            this.toolStripButton_newAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_newAll.Name = "toolStripButton_newAll";
            this.toolStripButton_newAll.Size = new System.Drawing.Size(56, 22);
            this.toolStripButton_newAll.Text = "增全";
            this.toolStripButton_newAll.ToolTipText = "增全所有期节点，从当前已有的最后一个期节点开始";
            this.toolStripButton_newAll.Click += new System.EventHandler(this.toolStripButton_newAll_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(56, 6);
            // 
            // toolStripButton_moveUp
            // 
            this.toolStripButton_moveUp.Enabled = false;
            this.toolStripButton_moveUp.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_moveUp.Image")));
            this.toolStripButton_moveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_moveUp.Name = "toolStripButton_moveUp";
            this.toolStripButton_moveUp.Size = new System.Drawing.Size(56, 22);
            this.toolStripButton_moveUp.Text = "上移";
            this.toolStripButton_moveUp.Visible = false;
            this.toolStripButton_moveUp.Click += new System.EventHandler(this.toolStripButton_moveUp_Click);
            // 
            // toolStripButton_moveDown
            // 
            this.toolStripButton_moveDown.Enabled = false;
            this.toolStripButton_moveDown.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_moveDown.Image")));
            this.toolStripButton_moveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_moveDown.Name = "toolStripButton_moveDown";
            this.toolStripButton_moveDown.Size = new System.Drawing.Size(56, 22);
            this.toolStripButton_moveDown.Text = "下移";
            this.toolStripButton_moveDown.Visible = false;
            this.toolStripButton_moveDown.Click += new System.EventHandler(this.toolStripButton_moveDown_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(56, 6);
            this.toolStripSeparator3.Visible = false;
            // 
            // toolStripButton_delete
            // 
            this.toolStripButton_delete.Enabled = false;
            this.toolStripButton_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_delete.Image")));
            this.toolStripButton_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_delete.Name = "toolStripButton_delete";
            this.toolStripButton_delete.Size = new System.Drawing.Size(56, 22);
            this.toolStripButton_delete.Text = "删除";
            this.toolStripButton_delete.ToolTipText = "删除当前选中的期节点";
            this.toolStripButton_delete.Click += new System.EventHandler(this.toolStripButton_delete_Click);
            // 
            // tableLayoutPanel_orderInfo
            // 
            this.tableLayoutPanel_orderInfo.ColumnCount = 2;
            this.tableLayoutPanel_orderInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_orderInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_orderInfo.Controls.Add(this.label_orderInfo_message, 0, 0);
            this.tableLayoutPanel_orderInfo.Controls.Add(this.orderDesignControl1, 1, 0);
            this.tableLayoutPanel_orderInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_orderInfo.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_orderInfo.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_orderInfo.Name = "tableLayoutPanel_orderInfo";
            this.tableLayoutPanel_orderInfo.RowCount = 1;
            this.tableLayoutPanel_orderInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_orderInfo.Size = new System.Drawing.Size(361, 118);
            this.tableLayoutPanel_orderInfo.TabIndex = 1;
            // 
            // label_orderInfo_message
            // 
            this.label_orderInfo_message.AutoSize = true;
            this.label_orderInfo_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_orderInfo_message.Location = new System.Drawing.Point(3, 0);
            this.label_orderInfo_message.Name = "label_orderInfo_message";
            this.label_orderInfo_message.Size = new System.Drawing.Size(1, 118);
            this.label_orderInfo_message.TabIndex = 0;
            this.label_orderInfo_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // orderDesignControl1
            // 
            this.orderDesignControl1.AutoScroll = true;
            this.orderDesignControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.orderDesignControl1.Changed = true;
            this.orderDesignControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.orderDesignControl1.Location = new System.Drawing.Point(6, 0);
            this.orderDesignControl1.Margin = new System.Windows.Forms.Padding(0);
            this.orderDesignControl1.Name = "orderDesignControl1";
            this.orderDesignControl1.NewlyOrderTotalCopy = 0;
            this.orderDesignControl1.Size = new System.Drawing.Size(355, 118);
            this.orderDesignControl1.TabIndex = 0;
            this.orderDesignControl1.TargetRecPath = "";
            // 
            // imageList_treeIcon
            // 
            this.imageList_treeIcon.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_treeIcon.ImageStream")));
            this.imageList_treeIcon.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_treeIcon.Images.SetKeyName(0, "recieve_none.bmp");
            this.imageList_treeIcon.Images.SetKeyName(1, "recieve_not_complete.bmp");
            this.imageList_treeIcon.Images.SetKeyName(2, "recieve_complete.bmp");
            // 
            // IssueManageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer_main);
            this.Name = "IssueManageControl";
            this.Size = new System.Drawing.Size(361, 267);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_treeView.ResumeLayout(false);
            this.tableLayoutPanel_treeView.PerformLayout();
            this.toolStrip_treeTool.ResumeLayout(false);
            this.toolStrip_treeTool.PerformLayout();
            this.tableLayoutPanel_orderInfo.ResumeLayout(false);
            this.tableLayoutPanel_orderInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TreeView TreeView;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private DigitalPlatform.CommonControl.OrderDesignControl orderDesignControl1;
        private System.Windows.Forms.ImageList imageList_treeIcon;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_orderInfo;
        private System.Windows.Forms.Label label_orderInfo_message;
        private System.Windows.Forms.ToolStrip toolStrip_treeTool;
        private System.Windows.Forms.ToolStripButton toolStripButton_newIssue;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_delete;
        private System.Windows.Forms.ToolStripButton toolStripButton_modify;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_newAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton_moveUp;
        private System.Windows.Forms.ToolStripButton toolStripButton_moveDown;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_treeView;
    }
}
