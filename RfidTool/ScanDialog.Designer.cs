
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_barcode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listView_tags = new System.Windows.Forms.ListView();
            this.columnHeader_uid = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_pii = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_tou = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_oi = new System.Windows.Forms.ColumnHeader();
            this.button_write = new System.Windows.Forms.Button();
            this.columnHeader_antenna = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_readerName = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "条码号(&B):";
            // 
            // textBox_barcode
            // 
            this.textBox_barcode.Font = new System.Drawing.Font("Microsoft YaHei UI", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBox_barcode.Location = new System.Drawing.Point(12, 43);
            this.textBox_barcode.Name = "textBox_barcode";
            this.textBox_barcode.Size = new System.Drawing.Size(326, 67);
            this.textBox_barcode.TabIndex = 1;
            this.textBox_barcode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_barcode_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 137);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 28);
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
            this.columnHeader_oi,
            this.columnHeader_antenna,
            this.columnHeader_readerName});
            this.listView_tags.FullRowSelect = true;
            this.listView_tags.HideSelection = false;
            this.listView_tags.Location = new System.Drawing.Point(12, 168);
            this.listView_tags.MultiSelect = false;
            this.listView_tags.Name = "listView_tags";
            this.listView_tags.Size = new System.Drawing.Size(818, 254);
            this.listView_tags.TabIndex = 3;
            this.listView_tags.UseCompatibleStateImageBehavior = false;
            this.listView_tags.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_uid
            // 
            this.columnHeader_uid.Name = "columnHeader_uid";
            this.columnHeader_uid.Text = "UID";
            this.columnHeader_uid.Width = 160;
            // 
            // columnHeader_pii
            // 
            this.columnHeader_pii.Name = "columnHeader_pii";
            this.columnHeader_pii.Text = "PII(条码号)";
            this.columnHeader_pii.Width = 160;
            // 
            // columnHeader_tou
            // 
            this.columnHeader_tou.Name = "columnHeader_tou";
            this.columnHeader_tou.Text = "TOU(用途)";
            this.columnHeader_tou.Width = 160;
            // 
            // columnHeader_oi
            // 
            this.columnHeader_oi.Name = "columnHeader_oi";
            this.columnHeader_oi.Text = "OI(所属机构)";
            this.columnHeader_oi.Width = 260;
            // 
            // button_write
            // 
            this.button_write.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_write.Font = new System.Drawing.Font("Microsoft YaHei UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button_write.Location = new System.Drawing.Point(608, 428);
            this.button_write.Name = "button_write";
            this.button_write.Size = new System.Drawing.Size(222, 60);
            this.button_write.TabIndex = 4;
            this.button_write.Text = "写入(&W)";
            this.button_write.UseVisualStyleBackColor = true;
            // 
            // columnHeader_antenna
            // 
            this.columnHeader_antenna.Name = "columnHeader_antenna";
            this.columnHeader_antenna.Text = "天线";
            // 
            // columnHeader_readerName
            // 
            this.columnHeader_readerName.Name = "columnHeader_readerName";
            this.columnHeader_readerName.Text = "读卡器";
            this.columnHeader_readerName.Width = 160;
            // 
            // ScanDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 500);
            this.Controls.Add(this.button_write);
            this.Controls.Add(this.listView_tags);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_barcode);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
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
    }
}