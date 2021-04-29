
namespace dp2Inventory
{
    partial class ShelfDialog
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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listView_shelfList = new System.Windows.Forms.ListView();
            this.columnHeader_shelfNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_count = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listView_books = new System.Windows.Forms.ListView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(981, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Location = new System.Drawing.Point(0, 723);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 17, 0);
            this.statusStrip1.Size = new System.Drawing.Size(981, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listView_shelfList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView_books);
            this.splitContainer1.Size = new System.Drawing.Size(981, 698);
            this.splitContainer1.SplitterDistance = 327;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 2;
            // 
            // listView_shelfList
            // 
            this.listView_shelfList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_shelfNo,
            this.columnHeader_count});
            this.listView_shelfList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_shelfList.FullRowSelect = true;
            this.listView_shelfList.HideSelection = false;
            this.listView_shelfList.Location = new System.Drawing.Point(0, 0);
            this.listView_shelfList.Margin = new System.Windows.Forms.Padding(4);
            this.listView_shelfList.MultiSelect = false;
            this.listView_shelfList.Name = "listView_shelfList";
            this.listView_shelfList.Size = new System.Drawing.Size(327, 698);
            this.listView_shelfList.TabIndex = 0;
            this.listView_shelfList.UseCompatibleStateImageBehavior = false;
            this.listView_shelfList.View = System.Windows.Forms.View.Details;
            this.listView_shelfList.SelectedIndexChanged += new System.EventHandler(this.listView_shelfList_SelectedIndexChanged);
            // 
            // columnHeader_shelfNo
            // 
            this.columnHeader_shelfNo.Text = "层架编号";
            this.columnHeader_shelfNo.Width = 134;
            // 
            // columnHeader_count
            // 
            this.columnHeader_count.Text = "册数";
            this.columnHeader_count.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_count.Width = 105;
            // 
            // listView_books
            // 
            this.listView_books.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_books.HideSelection = false;
            this.listView_books.Location = new System.Drawing.Point(0, 0);
            this.listView_books.Margin = new System.Windows.Forms.Padding(4);
            this.listView_books.Name = "listView_books";
            this.listView_books.Size = new System.Drawing.Size(649, 698);
            this.listView_books.TabIndex = 0;
            this.listView_books.UseCompatibleStateImageBehavior = false;
            this.listView_books.View = System.Windows.Forms.View.Details;
            // 
            // ShelfDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(981, 745);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ShelfDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "书架";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView listView_shelfList;
        private System.Windows.Forms.ColumnHeader columnHeader_shelfNo;
        private System.Windows.Forms.ColumnHeader columnHeader_count;
        private System.Windows.Forms.ListView listView_books;
    }
}