namespace RfidTool
{
    partial class WriteErrorDialog
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
            this.listView_tags = new System.Windows.Forms.ListView();
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_errorInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_tu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_oi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_aoi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_eas = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_afi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_readerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_antenna = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_protocol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_tid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listView_tags
            // 
            this.listView_tags.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.listView_tags.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_uid,
            this.columnHeader_errorInfo,
            this.columnHeader_pii,
            this.columnHeader_tu,
            this.columnHeader_oi,
            this.columnHeader_aoi,
            this.columnHeader_eas,
            this.columnHeader_afi,
            this.columnHeader_readerName,
            this.columnHeader_antenna,
            this.columnHeader_protocol,
            this.columnHeader_tid});
            this.listView_tags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_tags.FullRowSelect = true;
            this.listView_tags.HideSelection = false;
            this.listView_tags.Location = new System.Drawing.Point(0, 0);
            this.listView_tags.Name = "listView_tags";
            this.listView_tags.Size = new System.Drawing.Size(800, 450);
            this.listView_tags.TabIndex = 3;
            this.listView_tags.UseCompatibleStateImageBehavior = false;
            this.listView_tags.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_uid
            // 
            this.columnHeader_uid.Text = "UID";
            this.columnHeader_uid.Width = 120;
            // 
            // columnHeader_errorInfo
            // 
            this.columnHeader_errorInfo.Text = "错误信息";
            this.columnHeader_errorInfo.Width = 205;
            // 
            // columnHeader_pii
            // 
            this.columnHeader_pii.Text = "PII";
            this.columnHeader_pii.Width = 210;
            // 
            // columnHeader_tu
            // 
            this.columnHeader_tu.Text = "TU(应用类别)";
            this.columnHeader_tu.Width = 151;
            // 
            // columnHeader_oi
            // 
            this.columnHeader_oi.Text = "OI(机构代码)";
            this.columnHeader_oi.Width = 163;
            // 
            // columnHeader_aoi
            // 
            this.columnHeader_aoi.Text = "AOI(非标机构代码)";
            this.columnHeader_aoi.Width = 160;
            // 
            // columnHeader_eas
            // 
            this.columnHeader_eas.Text = "EAS";
            this.columnHeader_eas.Width = 89;
            // 
            // columnHeader_afi
            // 
            this.columnHeader_afi.Text = "AFI";
            this.columnHeader_afi.Width = 100;
            // 
            // columnHeader_readerName
            // 
            this.columnHeader_readerName.Text = "读写器";
            this.columnHeader_readerName.Width = 100;
            // 
            // columnHeader_antenna
            // 
            this.columnHeader_antenna.Text = "天线编号";
            this.columnHeader_antenna.Width = 100;
            // 
            // columnHeader_protocol
            // 
            this.columnHeader_protocol.Text = "协议";
            this.columnHeader_protocol.Width = 260;
            // 
            // columnHeader_tid
            // 
            this.columnHeader_tid.Text = "TID";
            this.columnHeader_tid.Width = 300;
            // 
            // WriteErrorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.listView_tags);
            this.Name = "WriteErrorDialog";
            this.ShowIcon = false;
            this.Text = "写入出错的标签，会自动重写";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView_tags;
        private System.Windows.Forms.ColumnHeader columnHeader_uid;
        private System.Windows.Forms.ColumnHeader columnHeader_errorInfo;
        private System.Windows.Forms.ColumnHeader columnHeader_pii;
        private System.Windows.Forms.ColumnHeader columnHeader_tu;
        private System.Windows.Forms.ColumnHeader columnHeader_oi;
        private System.Windows.Forms.ColumnHeader columnHeader_aoi;
        private System.Windows.Forms.ColumnHeader columnHeader_eas;
        private System.Windows.Forms.ColumnHeader columnHeader_afi;
        private System.Windows.Forms.ColumnHeader columnHeader_readerName;
        private System.Windows.Forms.ColumnHeader columnHeader_antenna;
        private System.Windows.Forms.ColumnHeader columnHeader_protocol;
        private System.Windows.Forms.ColumnHeader columnHeader_tid;
    }
}