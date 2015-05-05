namespace DigitalPlatform.DTLP
{
    partial class ViewAccessPointForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewAccessPointForm));
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader_key = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_fromName = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_fromValue = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_id = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_key,
            this.columnHeader_fromName,
            this.columnHeader_fromValue,
            this.columnHeader_id});
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(436, 248);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_key
            // 
            this.columnHeader_key.Text = "Key";
            this.columnHeader_key.Width = 171;
            // 
            // columnHeader_fromName
            // 
            this.columnHeader_fromName.Text = "¼ìË÷Í¾¾¶";
            this.columnHeader_fromName.Width = 150;
            // 
            // columnHeader_fromValue
            // 
            this.columnHeader_fromValue.Text = "À´Ô´";
            this.columnHeader_fromValue.Width = 150;
            // 
            // columnHeader_id
            // 
            this.columnHeader_id.Text = "¼ÇÂ¼ID";
            this.columnHeader_id.Width = 100;
            // 
            // ViewAccessPointForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 272);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ViewAccessPointForm";
            this.ShowInTaskbar = false;
            this.Text = "¹Û²ì¼ìË÷µã";
            this.Load += new System.EventHandler(this.ViewAccessPointForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader_key;
        private System.Windows.Forms.ColumnHeader columnHeader_fromName;
        private System.Windows.Forms.ColumnHeader columnHeader_fromValue;
        private System.Windows.Forms.ColumnHeader columnHeader_id;
    }
}