namespace dp2Circulation
{
    partial class IssueControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IssueControl));
            this.listView = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_publishTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_errorInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_issueNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_totalIssueNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_volumeNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_orderInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_batchNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_refID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_operations = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_recpath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_itemType = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // ListView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_publishTime,
            this.columnHeader_errorInfo,
            this.columnHeader_state,
            this.columnHeader_issueNo,
            this.columnHeader_totalIssueNumber,
            this.columnHeader_volumeNo,
            this.columnHeader_orderInfo,
            this.columnHeader_comment,
            this.columnHeader_batchNo,
            this.columnHeader_refID,
            this.columnHeader_operations,
            this.columnHeader_recpath});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.HideSelection = false;
            this.listView.LargeImageList = this.imageList_itemType;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listView.Name = "ListView";
            this.listView.Size = new System.Drawing.Size(350, 260);
            this.listView.SmallImageList = this.imageList_itemType;
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListView_ColumnClick);
            this.listView.DoubleClick += new System.EventHandler(this.ListView_DoubleClick);
            this.listView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseUp);
            // 
            // columnHeader_publishTime
            // 
            this.columnHeader_publishTime.Text = "出版时间(期定位信息)";
            this.columnHeader_publishTime.Width = 180;
            // 
            // columnHeader_errorInfo
            // 
            this.columnHeader_errorInfo.Text = "错误信息";
            this.columnHeader_errorInfo.Width = 120;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "状态";
            // 
            // columnHeader_issueNo
            // 
            this.columnHeader_issueNo.Text = "期号";
            this.columnHeader_issueNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_issueNo.Width = 77;
            // 
            // columnHeader_totalIssueNumber
            // 
            this.columnHeader_totalIssueNumber.Text = "总期号";
            this.columnHeader_totalIssueNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_totalIssueNumber.Width = 77;
            // 
            // columnHeader_volumeNo
            // 
            this.columnHeader_volumeNo.Text = "卷号";
            this.columnHeader_volumeNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_volumeNo.Width = 66;
            // 
            // columnHeader_orderInfo
            // 
            this.columnHeader_orderInfo.Text = "订购信息";
            this.columnHeader_orderInfo.Width = 200;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "附注";
            this.columnHeader_comment.Width = 231;
            // 
            // columnHeader_batchNo
            // 
            this.columnHeader_batchNo.Text = "批次号";
            this.columnHeader_batchNo.Width = 66;
            // 
            // columnHeader_refID
            // 
            this.columnHeader_refID.Text = "参考ID";
            this.columnHeader_refID.Width = 100;
            // 
            // columnHeader_operations
            // 
            this.columnHeader_operations.Text = "操作";
            this.columnHeader_operations.Width = 150;
            // 
            // columnHeader_recpath
            // 
            this.columnHeader_recpath.Text = "记录路径";
            this.columnHeader_recpath.Width = 120;
            // 
            // imageList_itemType
            // 
            this.imageList_itemType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_itemType.ImageStream")));
            this.imageList_itemType.TransparentColor = System.Drawing.Color.Magenta;
            this.imageList_itemType.Images.SetKeyName(0, "normal_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(1, "new_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(2, "changed_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(3, "deleted_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(4, "error_entity.bmp");
            // 
            // IssueControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "IssueControl";
            this.Size = new System.Drawing.Size(350, 260);
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.GUI.ListViewNF listView;
        private System.Windows.Forms.ColumnHeader columnHeader_publishTime;
        private System.Windows.Forms.ColumnHeader columnHeader_issueNo;
        private System.Windows.Forms.ColumnHeader columnHeader_totalIssueNumber;
        private System.Windows.Forms.ColumnHeader columnHeader_volumeNo;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_batchNo;
        private System.Windows.Forms.ColumnHeader columnHeader_errorInfo;
        private System.Windows.Forms.ColumnHeader columnHeader_recpath;
        private System.Windows.Forms.ColumnHeader columnHeader_orderInfo;
        private System.Windows.Forms.ImageList imageList_itemType;
        private System.Windows.Forms.ColumnHeader columnHeader_refID;
        private System.Windows.Forms.ColumnHeader columnHeader_operations;
    }
}
