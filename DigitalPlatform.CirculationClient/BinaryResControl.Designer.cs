namespace DigitalPlatform.CirculationClient
{
    partial class BinaryResControl
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
            this.ListView = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_localPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_size = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_mime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_timestamp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_usage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // ListView
            // 
            this.ListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_id,
            this.columnHeader_state,
            this.columnHeader_localPath,
            this.columnHeader_size,
            this.columnHeader_mime,
            this.columnHeader_timestamp,
            this.columnHeader_usage});
            this.ListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListView.FullRowSelect = true;
            this.ListView.HideSelection = false;
            this.ListView.Location = new System.Drawing.Point(0, 0);
            this.ListView.Margin = new System.Windows.Forms.Padding(0);
            this.ListView.Name = "ListView";
            this.ListView.Size = new System.Drawing.Size(334, 284);
            this.ListView.TabIndex = 1;
            this.ListView.UseCompatibleStateImageBehavior = false;
            this.ListView.View = System.Windows.Forms.View.Details;
            this.ListView.DoubleClick += new System.EventHandler(this.ListView_DoubleClick);
            this.ListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListView_KeyDown);
            this.ListView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseUp);
            // 
            // columnHeader_id
            // 
            this.columnHeader_id.Text = "ID";
            this.columnHeader_id.Width = 100;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "状态";
            this.columnHeader_state.Width = 100;
            // 
            // columnHeader_localPath
            // 
            this.columnHeader_localPath.Text = "上载前物理路径";
            this.columnHeader_localPath.Width = 250;
            // 
            // columnHeader_size
            // 
            this.columnHeader_size.Text = "尺寸";
            this.columnHeader_size.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_size.Width = 100;
            // 
            // columnHeader_mime
            // 
            this.columnHeader_mime.Text = "媒体类型";
            this.columnHeader_mime.Width = 250;
            // 
            // columnHeader_timestamp
            // 
            this.columnHeader_timestamp.Text = "时间戳";
            this.columnHeader_timestamp.Width = 300;
            // 
            // columnHeader_usage
            // 
            this.columnHeader_usage.Text = "用途";
            this.columnHeader_usage.Width = 100;
            // 
            // BinaryResControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ListView);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "BinaryResControl";
            this.Size = new System.Drawing.Size(334, 284);
            this.ResumeLayout(false);

        }

        #endregion

        public DigitalPlatform.GUI.ListViewNF ListView;
        private System.Windows.Forms.ColumnHeader columnHeader_id;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_localPath;
        private System.Windows.Forms.ColumnHeader columnHeader_size;
        private System.Windows.Forms.ColumnHeader columnHeader_mime;
        private System.Windows.Forms.ColumnHeader columnHeader_timestamp;
        private System.Windows.Forms.ColumnHeader columnHeader_usage;
    }
}
