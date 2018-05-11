namespace dp2Circulation
{
    partial class SelectColumnDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectColumnDialog));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_move_up = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_move_down = new System.Windows.Forms.ToolStripButton();
            this.listView_columns = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_index = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(495, 340);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(112, 34);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(376, 340);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(112, 34);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_move_up,
            this.toolStripButton_move_down});
            this.toolStrip1.Location = new System.Drawing.Point(16, 299);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip1.Size = new System.Drawing.Size(104, 31);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_move_up
            // 
            this.toolStripButton_move_up.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_move_up.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_move_up.Image")));
            this.toolStripButton_move_up.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_move_up.Name = "toolStripButton_move_up";
            this.toolStripButton_move_up.Size = new System.Drawing.Size(50, 28);
            this.toolStripButton_move_up.Text = "上移";
            this.toolStripButton_move_up.Click += new System.EventHandler(this.toolStripButton_move_up_Click);
            // 
            // toolStripButton_move_down
            // 
            this.toolStripButton_move_down.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_move_down.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_move_down.Image")));
            this.toolStripButton_move_down.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_move_down.Name = "toolStripButton_move_down";
            this.toolStripButton_move_down.Size = new System.Drawing.Size(50, 28);
            this.toolStripButton_move_down.Text = "下移";
            this.toolStripButton_move_down.Click += new System.EventHandler(this.toolStripButton_move_down_Click);
            // 
            // listView_columns
            // 
            this.listView_columns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_columns.CheckBoxes = true;
            this.listView_columns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_index,
            this.columnHeader_name});
            this.listView_columns.FullRowSelect = true;
            this.listView_columns.HideSelection = false;
            this.listView_columns.Location = new System.Drawing.Point(16, 16);
            this.listView_columns.MultiSelect = false;
            this.listView_columns.Name = "listView_columns";
            this.listView_columns.Size = new System.Drawing.Size(589, 271);
            this.listView_columns.TabIndex = 3;
            this.listView_columns.UseCompatibleStateImageBehavior = false;
            this.listView_columns.View = System.Windows.Forms.View.Details;
            this.listView_columns.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView_columns_ItemChecked);
            this.listView_columns.SelectedIndexChanged += new System.EventHandler(this.listView_columns_SelectedIndexChanged);
            // 
            // columnHeader_index
            // 
            this.columnHeader_index.Text = "原始序号";
            this.columnHeader_index.Width = 94;
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "列名";
            this.columnHeader_name.Width = 100;
            // 
            // SelectColumnDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(624, 392);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_columns);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SelectColumnDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "选择列";
            this.Load += new System.EventHandler(this.SelectColumnDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private DigitalPlatform.GUI.ListViewNF listView_columns;
        private System.Windows.Forms.ColumnHeader columnHeader_index;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_move_up;
        private System.Windows.Forms.ToolStripButton toolStripButton_move_down;
    }
}