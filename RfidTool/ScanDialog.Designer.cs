
namespace RfidTool
{
    partial class ScanDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_barcode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listView_tags = new System.Windows.Forms.ListView();
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_tou = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_title = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_accessNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_eas = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_afi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_oi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_aoi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_shelfLocation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_antenna = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_readerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_protocol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_write = new System.Windows.Forms.Button();
            this.textBox_processingBarcode = new System.Windows.Forms.TextBox();
            this.button_clearProcessingBarcode = new System.Windows.Forms.Button();
            this.label_message = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.button_test = new System.Windows.Forms.Button();
            this.label_title = new System.Windows.Forms.Label();
            this.columnHeader_tid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "条码号(&B):";
            // 
            // textBox_barcode
            // 
            this.textBox_barcode.Font = new System.Drawing.Font("Microsoft YaHei UI", 20F);
            this.textBox_barcode.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_barcode.Location = new System.Drawing.Point(10, 32);
            this.textBox_barcode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox_barcode.Name = "textBox_barcode";
            this.textBox_barcode.Size = new System.Drawing.Size(332, 67);
            this.textBox_barcode.TabIndex = 1;
            this.textBox_barcode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_barcode_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 165);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "待选标签(&T):";
            // 
            // listView_tags
            // 
            this.listView_tags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_tags.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_uid,
            this.columnHeader_pii,
            this.columnHeader_tou,
            this.columnHeader_title,
            this.columnHeader_accessNo,
            this.columnHeader_eas,
            this.columnHeader_afi,
            this.columnHeader_oi,
            this.columnHeader_aoi,
            this.columnHeader_shelfLocation,
            this.columnHeader_antenna,
            this.columnHeader_readerName,
            this.columnHeader_protocol,
            this.columnHeader_tid});
            this.listView_tags.FullRowSelect = true;
            this.listView_tags.HideSelection = false;
            this.listView_tags.Location = new System.Drawing.Point(10, 188);
            this.listView_tags.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listView_tags.MultiSelect = false;
            this.listView_tags.Name = "listView_tags";
            this.listView_tags.ShowItemToolTips = true;
            this.listView_tags.Size = new System.Drawing.Size(902, 347);
            this.listView_tags.TabIndex = 3;
            this.listView_tags.UseCompatibleStateImageBehavior = false;
            this.listView_tags.View = System.Windows.Forms.View.Details;
            this.listView_tags.SelectedIndexChanged += new System.EventHandler(this.listView_tags_SelectedIndexChanged);
            this.listView_tags.DoubleClick += new System.EventHandler(this.listView_tags_DoubleClick);
            this.listView_tags.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_tags_MouseUp);
            // 
            // columnHeader_uid
            // 
            this.columnHeader_uid.Name = "columnHeader_uid";
            this.columnHeader_uid.Text = "UID";
            this.columnHeader_uid.Width = 221;
            // 
            // columnHeader_pii
            // 
            this.columnHeader_pii.Name = "columnHeader_pii";
            this.columnHeader_pii.Text = "PII(条码号)";
            this.columnHeader_pii.Width = 181;
            // 
            // columnHeader_tou
            // 
            this.columnHeader_tou.Name = "columnHeader_tou";
            this.columnHeader_tou.Text = "TOU(用途)";
            this.columnHeader_tou.Width = 160;
            // 
            // columnHeader_title
            // 
            this.columnHeader_title.Text = "Title(书名)";
            this.columnHeader_title.Width = 137;
            // 
            // columnHeader_accessNo
            // 
            this.columnHeader_accessNo.Text = "索取号";
            this.columnHeader_accessNo.Width = 140;
            // 
            // columnHeader_eas
            // 
            this.columnHeader_eas.Text = "EAS(防盗标志)";
            this.columnHeader_eas.Width = 159;
            // 
            // columnHeader_afi
            // 
            this.columnHeader_afi.Text = "AFI";
            this.columnHeader_afi.Width = 100;
            // 
            // columnHeader_oi
            // 
            this.columnHeader_oi.Name = "columnHeader_oi";
            this.columnHeader_oi.Text = "OI(所属机构)";
            this.columnHeader_oi.Width = 260;
            // 
            // columnHeader_aoi
            // 
            this.columnHeader_aoi.Text = "AOI(非标准所属机构)";
            this.columnHeader_aoi.Width = 210;
            // 
            // columnHeader_shelfLocation
            // 
            this.columnHeader_shelfLocation.Text = "ShelfLocation";
            this.columnHeader_shelfLocation.Width = 120;
            // 
            // columnHeader_antenna
            // 
            this.columnHeader_antenna.Name = "columnHeader_antenna";
            this.columnHeader_antenna.Text = "天线";
            // 
            // columnHeader_readerName
            // 
            this.columnHeader_readerName.Name = "columnHeader_readerName";
            this.columnHeader_readerName.Text = "读写器";
            this.columnHeader_readerName.Width = 160;
            // 
            // columnHeader_protocol
            // 
            this.columnHeader_protocol.Text = "协议";
            this.columnHeader_protocol.Width = 260;
            // 
            // button_write
            // 
            this.button_write.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_write.Enabled = false;
            this.button_write.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.button_write.Location = new System.Drawing.Point(723, 539);
            this.button_write.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_write.Name = "button_write";
            this.button_write.Size = new System.Drawing.Size(188, 45);
            this.button_write.TabIndex = 4;
            this.button_write.Text = "写入(&W)";
            this.button_write.UseVisualStyleBackColor = true;
            this.button_write.Click += new System.EventHandler(this.button_write_Click);
            // 
            // textBox_processingBarcode
            // 
            this.textBox_processingBarcode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_processingBarcode.Font = new System.Drawing.Font("Microsoft YaHei UI", 20F);
            this.textBox_processingBarcode.Location = new System.Drawing.Point(348, 32);
            this.textBox_processingBarcode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox_processingBarcode.Name = "textBox_processingBarcode";
            this.textBox_processingBarcode.ReadOnly = true;
            this.textBox_processingBarcode.Size = new System.Drawing.Size(332, 67);
            this.textBox_processingBarcode.TabIndex = 5;
            this.textBox_processingBarcode.TextChanged += new System.EventHandler(this.textBox_processingBarcode_TextChanged);
            // 
            // button_clearProcessingBarcode
            // 
            this.button_clearProcessingBarcode.Enabled = false;
            this.button_clearProcessingBarcode.Location = new System.Drawing.Point(686, 32);
            this.button_clearProcessingBarcode.Name = "button_clearProcessingBarcode";
            this.button_clearProcessingBarcode.Size = new System.Drawing.Size(74, 38);
            this.button_clearProcessingBarcode.TabIndex = 6;
            this.button_clearProcessingBarcode.Text = "清除";
            this.button_clearProcessingBarcode.UseVisualStyleBackColor = true;
            this.button_clearProcessingBarcode.Click += new System.EventHandler(this.button_clearProcessingBarcode_Click);
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Font = new System.Drawing.Font("宋体", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_message.Location = new System.Drawing.Point(10, 539);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(707, 45);
            this.label_message.TabIndex = 7;
            this.label_message.Text = "请扫入条码号 ...";
            // 
            // button_test
            // 
            this.button_test.Location = new System.Drawing.Point(686, 76);
            this.button_test.Name = "button_test";
            this.button_test.Size = new System.Drawing.Size(74, 38);
            this.button_test.TabIndex = 8;
            this.button_test.Text = "测试";
            this.button_test.UseVisualStyleBackColor = true;
            this.button_test.Visible = false;
            this.button_test.Click += new System.EventHandler(this.button_test_Click);
            // 
            // label_title
            // 
            this.label_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_title.Font = new System.Drawing.Font("宋体", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_title.Location = new System.Drawing.Point(10, 101);
            this.label_title.Name = "label_title";
            this.label_title.Size = new System.Drawing.Size(899, 69);
            this.label_title.TabIndex = 9;
            // 
            // columnHeader_tid
            // 
            this.columnHeader_tid.Text = "TID";
            this.columnHeader_tid.Width = 300;
            // 
            // ScanDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(921, 593);
            this.Controls.Add(this.button_test);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_clearProcessingBarcode);
            this.Controls.Add(this.textBox_processingBarcode);
            this.Controls.Add(this.button_write);
            this.Controls.Add(this.listView_tags);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_barcode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label_title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ScanDialog";
            this.ShowIcon = false;
            this.Text = "扫描并写入";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ScanDialog_FormClosed);
            this.Load += new System.EventHandler(this.ScanDialog_Load);
            this.VisibleChanged += new System.EventHandler(this.ScanDialog_VisibleChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_barcode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listView_tags;
        private System.Windows.Forms.ColumnHeader columnHeader_uid;
        private System.Windows.Forms.ColumnHeader columnHeader_pii;
        private System.Windows.Forms.ColumnHeader columnHeader_tou;
        private System.Windows.Forms.ColumnHeader columnHeader_oi;
        private System.Windows.Forms.Button button_write;
        private System.Windows.Forms.ColumnHeader columnHeader_antenna;
        private System.Windows.Forms.ColumnHeader columnHeader_readerName;
        private System.Windows.Forms.TextBox textBox_processingBarcode;
        private System.Windows.Forms.Button button_clearProcessingBarcode;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.ColumnHeader columnHeader_aoi;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ColumnHeader columnHeader_eas;
        private System.Windows.Forms.ColumnHeader columnHeader_protocol;
        private System.Windows.Forms.Button button_test;
        private System.Windows.Forms.ColumnHeader columnHeader_afi;
        private System.Windows.Forms.ColumnHeader columnHeader_title;
        private System.Windows.Forms.ColumnHeader columnHeader_accessNo;
        private System.Windows.Forms.ColumnHeader columnHeader_shelfLocation;
        private System.Windows.Forms.Label label_title;
        private System.Windows.Forms.ColumnHeader columnHeader_tid;
    }
}