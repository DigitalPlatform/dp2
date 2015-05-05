namespace dp2Circulation
{
    partial class OrderControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrderControl));
            this.listView = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_index = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_errorInfo = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_state = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_catalogNo = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_seller = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_source = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_range = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_issueCount = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_copy = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_price = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_totalPrice = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_orderTime = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_orderID = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_distribute = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_class = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_comment = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_batchNo = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_sellerAddress = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_refID = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_recpath = new System.Windows.Forms.ColumnHeader();
            this.imageList_itemType = new System.Windows.Forms.ImageList(this.components);
            this.columnHeader_operations = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // ListView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_index,
            this.columnHeader_errorInfo,
            this.columnHeader_state,
            this.columnHeader_catalogNo,
            this.columnHeader_seller,
            this.columnHeader_source,
            this.columnHeader_range,
            this.columnHeader_issueCount,
            this.columnHeader_copy,
            this.columnHeader_price,
            this.columnHeader_totalPrice,
            this.columnHeader_orderTime,
            this.columnHeader_orderID,
            this.columnHeader_distribute,
            this.columnHeader_class,
            this.columnHeader_comment,
            this.columnHeader_batchNo,
            this.columnHeader_sellerAddress,
            this.columnHeader_refID,
            this.columnHeader_operations,
            this.columnHeader_recpath});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.HideSelection = false;
            this.listView.LargeImageList = this.imageList_itemType;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Name = "ListView";
            this.listView.Size = new System.Drawing.Size(464, 224);
            this.listView.SmallImageList = this.imageList_itemType;
            this.listView.TabIndex = 1;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.DoubleClick += new System.EventHandler(this.ListView_DoubleClick);
            this.listView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseUp);
            this.listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListView_ColumnClick);
            // 
            // columnHeader_index
            // 
            this.columnHeader_index.Text = "编号";
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
            // columnHeader_catalogNo
            // 
            this.columnHeader_catalogNo.Text = "书目号";
            this.columnHeader_catalogNo.Width = 120;
            // 
            // columnHeader_seller
            // 
            this.columnHeader_seller.Text = "渠道";
            this.columnHeader_seller.Width = 120;
            // 
            // columnHeader_source
            // 
            this.columnHeader_source.Text = "经费来源";
            this.columnHeader_source.Width = 120;
            // 
            // columnHeader_range
            // 
            this.columnHeader_range.Text = "时间范围";
            this.columnHeader_range.Width = 120;
            // 
            // columnHeader_issueCount
            // 
            this.columnHeader_issueCount.Text = "包含期数";
            this.columnHeader_issueCount.Width = 80;
            // 
            // columnHeader_copy
            // 
            this.columnHeader_copy.Text = "复本数";
            this.columnHeader_copy.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_copy.Width = 66;
            // 
            // columnHeader_price
            // 
            this.columnHeader_price.Text = "价格";
            this.columnHeader_price.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_price.Width = 120;
            // 
            // columnHeader_totalPrice
            // 
            this.columnHeader_totalPrice.Text = "总价格";
            this.columnHeader_totalPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeader_orderTime
            // 
            this.columnHeader_orderTime.Text = "订购时间";
            this.columnHeader_orderTime.Width = 120;
            // 
            // columnHeader_orderID
            // 
            this.columnHeader_orderID.Text = "订单号";
            // 
            // columnHeader_distribute
            // 
            this.columnHeader_distribute.Text = "馆藏分配";
            this.columnHeader_distribute.Width = 120;
            // 
            // columnHeader_class
            // 
            this.columnHeader_class.Text = "类别";
            this.columnHeader_class.Width = 120;
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
            // columnHeader_sellerAddress
            // 
            this.columnHeader_sellerAddress.Text = "渠道地址";
            this.columnHeader_sellerAddress.Width = 100;
            // 
            // columnHeader_refID
            // 
            this.columnHeader_refID.Text = "参考ID";
            this.columnHeader_refID.Width = 100;
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
            // columnHeader_operations
            // 
            this.columnHeader_operations.Text = "操作";
            this.columnHeader_operations.Width = 150;
            // 
            // OrderControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView);
            this.Name = "OrderControl";
            this.Size = new System.Drawing.Size(464, 224);
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.GUI.ListViewNF listView;
        private System.Windows.Forms.ColumnHeader columnHeader_index;
        private System.Windows.Forms.ColumnHeader columnHeader_errorInfo;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_seller;
        private System.Windows.Forms.ColumnHeader columnHeader_range;
        private System.Windows.Forms.ColumnHeader columnHeader_copy;
        private System.Windows.Forms.ColumnHeader columnHeader_price;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.ColumnHeader columnHeader_batchNo;
        private System.Windows.Forms.ColumnHeader columnHeader_recpath;
        private System.Windows.Forms.ImageList imageList_itemType;
        private System.Windows.Forms.ColumnHeader columnHeader_totalPrice;
        private System.Windows.Forms.ColumnHeader columnHeader_orderTime;
        private System.Windows.Forms.ColumnHeader columnHeader_orderID;
        private System.Windows.Forms.ColumnHeader columnHeader_distribute;
        private System.Windows.Forms.ColumnHeader columnHeader_source;
        private System.Windows.Forms.ColumnHeader columnHeader_issueCount;
        private System.Windows.Forms.ColumnHeader columnHeader_catalogNo;
        private System.Windows.Forms.ColumnHeader columnHeader_class;
        private System.Windows.Forms.ColumnHeader columnHeader_sellerAddress;
        private System.Windows.Forms.ColumnHeader columnHeader_refID;
        private System.Windows.Forms.ColumnHeader columnHeader_operations;
    }
}
