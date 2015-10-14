namespace dp2Circulation
{
    partial class EntityEditForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntityEditForm));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_autoSearchDup = new System.Windows.Forms.CheckBox();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_existing = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.entityEditControl_existing = new dp2Circulation.EntityEditControl();
            this.button_existing_undoMaskDelete = new System.Windows.Forms.Button();
            this.tableLayoutPanel_editing = new System.Windows.Forms.TableLayoutPanel();
            this.button_editing_undoMaskDelete = new System.Windows.Forms.Button();
            this.label_editing = new System.Windows.Forms.Label();
            this.panel_editing = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_next = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_prev = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_new = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_option = new System.Windows.Forms.ToolStripButton();
            this.entityEditControl_editing = new dp2Circulation.EntityEditControl();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.textBox_message = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_existing.SuspendLayout();
            this.tableLayoutPanel_editing.SuspendLayout();
            this.panel_editing.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(309, 306);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(370, 306);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_autoSearchDup
            // 
            this.checkBox_autoSearchDup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_autoSearchDup.AutoSize = true;
            this.checkBox_autoSearchDup.Checked = true;
            this.checkBox_autoSearchDup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_autoSearchDup.Location = new System.Drawing.Point(10, 281);
            this.checkBox_autoSearchDup.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_autoSearchDup.Name = "checkBox_autoSearchDup";
            this.checkBox_autoSearchDup.Size = new System.Drawing.Size(174, 16);
            this.checkBox_autoSearchDup.TabIndex = 1;
            this.checkBox_autoSearchDup.Text = "实时对册条码号进行查重(&R)";
            this.checkBox_autoSearchDup.UseVisualStyleBackColor = true;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(2, 58);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tableLayoutPanel_existing);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tableLayoutPanel_editing);
            this.splitContainer_main.Size = new System.Drawing.Size(413, 209);
            this.splitContainer_main.SplitterDistance = 200;
            this.splitContainer_main.SplitterWidth = 3;
            this.splitContainer_main.TabIndex = 5;
            // 
            // tableLayoutPanel_existing
            // 
            this.tableLayoutPanel_existing.ColumnCount = 1;
            this.tableLayoutPanel_existing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_existing.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_existing.Controls.Add(this.entityEditControl_existing, 0, 1);
            this.tableLayoutPanel_existing.Controls.Add(this.button_existing_undoMaskDelete, 0, 2);
            this.tableLayoutPanel_existing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_existing.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_existing.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_existing.Name = "tableLayoutPanel_existing";
            this.tableLayoutPanel_existing.RowCount = 3;
            this.tableLayoutPanel_existing.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_existing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_existing.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_existing.Size = new System.Drawing.Size(200, 209);
            this.tableLayoutPanel_existing.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "当前数据库中记录";
            // 
            // entityEditControl_existing
            // 
            this.entityEditControl_existing.AccessNo = "";
            this.entityEditControl_existing.AutoScroll = true;
            this.entityEditControl_existing.BackColor = System.Drawing.SystemColors.Control;
            this.entityEditControl_existing.Barcode = "";
            this.entityEditControl_existing.BatchNo = "";
            this.entityEditControl_existing.Binding = "";
            this.entityEditControl_existing.BindingCost = "";
            this.entityEditControl_existing.BookType = "";
            this.entityEditControl_existing.BorrowDate = "";
            this.entityEditControl_existing.Borrower = "";
            this.entityEditControl_existing.BorrowPeriod = "";
            this.entityEditControl_existing.Changed = false;
            this.entityEditControl_existing.Comment = "";
            this.entityEditControl_existing.CreateState = dp2Circulation.ItemDisplayState.Normal;
            this.entityEditControl_existing.DisplayMode = "full";
            this.entityEditControl_existing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityEditControl_existing.ErrorInfo = "";
            this.entityEditControl_existing.ForeColor = System.Drawing.SystemColors.ControlText;
            this.entityEditControl_existing.Initializing = true;
            this.entityEditControl_existing.Intact = "";
            this.entityEditControl_existing.Location = new System.Drawing.Point(2, 14);
            this.entityEditControl_existing.LocationString = "";
            this.entityEditControl_existing.Margin = new System.Windows.Forms.Padding(2);
            this.entityEditControl_existing.MemberBackColor = System.Drawing.Color.WhiteSmoke;
            this.entityEditControl_existing.MemberForeColor = System.Drawing.SystemColors.ControlText;
            this.entityEditControl_existing.MergeComment = "";
            this.entityEditControl_existing.MinimumSize = new System.Drawing.Size(56, 0);
            this.entityEditControl_existing.Name = "entityEditControl_existing";
            this.entityEditControl_existing.Operations = "";
            this.entityEditControl_existing.ParentId = "";
            this.entityEditControl_existing.Price = "";
            this.entityEditControl_existing.PublishTime = "";
            this.entityEditControl_existing.RecPath = "";
            this.entityEditControl_existing.RefID = "";
            this.entityEditControl_existing.RegisterNo = "";
            this.entityEditControl_existing.Seller = "";
            this.entityEditControl_existing.Size = new System.Drawing.Size(196, 167);
            this.entityEditControl_existing.Source = "";
            this.entityEditControl_existing.State = "";
            this.entityEditControl_existing.TabIndex = 1;
            this.entityEditControl_existing.TableMargin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.entityEditControl_existing.TablePadding = new System.Windows.Forms.Padding(12, 13, 12, 13);
            this.entityEditControl_existing.Volume = "";
            // 
            // button_existing_undoMaskDelete
            // 
            this.button_existing_undoMaskDelete.Location = new System.Drawing.Point(2, 185);
            this.button_existing_undoMaskDelete.Margin = new System.Windows.Forms.Padding(2);
            this.button_existing_undoMaskDelete.Name = "button_existing_undoMaskDelete";
            this.button_existing_undoMaskDelete.Size = new System.Drawing.Size(122, 22);
            this.button_existing_undoMaskDelete.TabIndex = 2;
            this.button_existing_undoMaskDelete.Text = "撤销标记删除(&U)";
            this.button_existing_undoMaskDelete.UseVisualStyleBackColor = true;
            this.button_existing_undoMaskDelete.Visible = false;
            // 
            // tableLayoutPanel_editing
            // 
            this.tableLayoutPanel_editing.ColumnCount = 1;
            this.tableLayoutPanel_editing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_editing.Controls.Add(this.button_editing_undoMaskDelete, 0, 2);
            this.tableLayoutPanel_editing.Controls.Add(this.label_editing, 0, 0);
            this.tableLayoutPanel_editing.Controls.Add(this.panel_editing, 0, 1);
            this.tableLayoutPanel_editing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_editing.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_editing.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_editing.Name = "tableLayoutPanel_editing";
            this.tableLayoutPanel_editing.RowCount = 3;
            this.tableLayoutPanel_editing.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_editing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_editing.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_editing.Size = new System.Drawing.Size(210, 209);
            this.tableLayoutPanel_editing.TabIndex = 0;
            // 
            // button_editing_undoMaskDelete
            // 
            this.button_editing_undoMaskDelete.Location = new System.Drawing.Point(2, 185);
            this.button_editing_undoMaskDelete.Margin = new System.Windows.Forms.Padding(2);
            this.button_editing_undoMaskDelete.Name = "button_editing_undoMaskDelete";
            this.button_editing_undoMaskDelete.Size = new System.Drawing.Size(122, 22);
            this.button_editing_undoMaskDelete.TabIndex = 1;
            this.button_editing_undoMaskDelete.Text = "撤销标记删除(&U)";
            this.button_editing_undoMaskDelete.UseVisualStyleBackColor = true;
            this.button_editing_undoMaskDelete.Click += new System.EventHandler(this.button_editing_undoMaskDelete_Click);
            // 
            // label_editing
            // 
            this.label_editing.AutoSize = true;
            this.label_editing.Location = new System.Drawing.Point(2, 0);
            this.label_editing.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_editing.Name = "label_editing";
            this.label_editing.Size = new System.Drawing.Size(89, 12);
            this.label_editing.TabIndex = 0;
            this.label_editing.Text = "正在编辑的记录";
            // 
            // panel_editing
            // 
            this.panel_editing.Controls.Add(this.toolStrip1);
            this.panel_editing.Controls.Add(this.entityEditControl_editing);
            this.panel_editing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_editing.Location = new System.Drawing.Point(2, 14);
            this.panel_editing.Margin = new System.Windows.Forms.Padding(2);
            this.panel_editing.Name = "panel_editing";
            this.panel_editing.Size = new System.Drawing.Size(206, 167);
            this.panel_editing.TabIndex = 8;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Right;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_next,
            this.toolStripButton_prev,
            this.toolStripButton_new,
            this.toolStripButton_option});
            this.toolStrip1.Location = new System.Drawing.Point(174, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(32, 167);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_next
            // 
            this.toolStripButton_next.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_next.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_next.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_next.Image")));
            this.toolStripButton_next.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_next.Name = "toolStripButton_next";
            this.toolStripButton_next.Size = new System.Drawing.Size(29, 20);
            this.toolStripButton_next.Text = "下一记录";
            this.toolStripButton_next.Click += new System.EventHandler(this.toolStripButton_next_Click);
            // 
            // toolStripButton_prev
            // 
            this.toolStripButton_prev.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_prev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_prev.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_prev.Image")));
            this.toolStripButton_prev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_prev.Name = "toolStripButton_prev";
            this.toolStripButton_prev.Size = new System.Drawing.Size(29, 20);
            this.toolStripButton_prev.Text = "上一记录";
            this.toolStripButton_prev.Click += new System.EventHandler(this.toolStripButton_prev_Click);
            // 
            // toolStripButton_new
            // 
            this.toolStripButton_new.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_new.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_new.Image")));
            this.toolStripButton_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_new.Name = "toolStripButton_new";
            this.toolStripButton_new.Size = new System.Drawing.Size(29, 20);
            this.toolStripButton_new.Text = "新建册记录";
            this.toolStripButton_new.Click += new System.EventHandler(this.toolStripButton_new_Click);
            // 
            // toolStripButton_option
            // 
            this.toolStripButton_option.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_option.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_option.Image")));
            this.toolStripButton_option.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_option.Name = "toolStripButton_option";
            this.toolStripButton_option.Size = new System.Drawing.Size(29, 20);
            this.toolStripButton_option.Text = "选项";
            this.toolStripButton_option.Click += new System.EventHandler(this.toolStripButton_option_Click);
            // 
            // entityEditControl_editing
            // 
            this.entityEditControl_editing.AccessNo = "";
            this.entityEditControl_editing.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.entityEditControl_editing.AutoScroll = true;
            this.entityEditControl_editing.BackColor = System.Drawing.SystemColors.Control;
            this.entityEditControl_editing.Barcode = "";
            this.entityEditControl_editing.BatchNo = "";
            this.entityEditControl_editing.Binding = "";
            this.entityEditControl_editing.BindingCost = "";
            this.entityEditControl_editing.BookType = "";
            this.entityEditControl_editing.BorrowDate = "";
            this.entityEditControl_editing.Borrower = "";
            this.entityEditControl_editing.BorrowPeriod = "";
            this.entityEditControl_editing.Changed = false;
            this.entityEditControl_editing.Comment = "";
            this.entityEditControl_editing.CreateState = dp2Circulation.ItemDisplayState.Normal;
            this.entityEditControl_editing.DisplayMode = "full";
            this.entityEditControl_editing.ErrorInfo = "";
            this.entityEditControl_editing.ForeColor = System.Drawing.SystemColors.ControlText;
            this.entityEditControl_editing.Initializing = true;
            this.entityEditControl_editing.Intact = "";
            this.entityEditControl_editing.Location = new System.Drawing.Point(0, 0);
            this.entityEditControl_editing.LocationString = "";
            this.entityEditControl_editing.Margin = new System.Windows.Forms.Padding(2);
            this.entityEditControl_editing.MemberBackColor = System.Drawing.Color.WhiteSmoke;
            this.entityEditControl_editing.MemberForeColor = System.Drawing.SystemColors.ControlText;
            this.entityEditControl_editing.MergeComment = "";
            this.entityEditControl_editing.MinimumSize = new System.Drawing.Size(56, 0);
            this.entityEditControl_editing.Name = "entityEditControl_editing";
            this.entityEditControl_editing.Operations = "";
            this.entityEditControl_editing.ParentId = "";
            this.entityEditControl_editing.Price = "";
            this.entityEditControl_editing.PublishTime = "";
            this.entityEditControl_editing.RecPath = "";
            this.entityEditControl_editing.RefID = "";
            this.entityEditControl_editing.RegisterNo = "";
            this.entityEditControl_editing.Seller = "";
            this.entityEditControl_editing.Size = new System.Drawing.Size(179, 167);
            this.entityEditControl_editing.Source = "";
            this.entityEditControl_editing.State = "";
            this.entityEditControl_editing.TabIndex = 0;
            this.entityEditControl_editing.TableMargin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.entityEditControl_editing.TablePadding = new System.Windows.Forms.Padding(12, 13, 12, 13);
            this.entityEditControl_editing.Volume = "";
            this.entityEditControl_editing.ContentChanged += new DigitalPlatform.ContentChangedEventHandler(this.entityEditControl_editing_ContentChanged);
            this.entityEditControl_editing.ControlKeyDown += new DigitalPlatform.ControlKeyEventHandler(this.entityEditControl_editing_ControlKeyDown);
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.textBox_message, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.splitContainer_main, 0, 1);
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(9, 10);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(417, 269);
            this.tableLayoutPanel_main.TabIndex = 0;
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(2, 2);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(413, 52);
            this.textBox_message.TabIndex = 0;
            // 
            // EntityEditForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(435, 338);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Controls.Add(this.checkBox_autoSearchDup);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "EntityEditForm";
            this.ShowInTaskbar = false;
            this.Text = "册信息";
            this.Activated += new System.EventHandler(this.EntityEditForm_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EntityEditForm_FormClosed);
            this.Load += new System.EventHandler(this.EntityEditForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_existing.ResumeLayout(false);
            this.tableLayoutPanel_existing.PerformLayout();
            this.tableLayoutPanel_editing.ResumeLayout(false);
            this.tableLayoutPanel_editing.PerformLayout();
            this.panel_editing.ResumeLayout(false);
            this.panel_editing.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.CheckBox checkBox_autoSearchDup;
        internal EntityEditControl entityEditControl_editing;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_existing;
        private System.Windows.Forms.Label label1;
        internal EntityEditControl entityEditControl_existing;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_editing;
        private System.Windows.Forms.Label label_editing;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.Button button_existing_undoMaskDelete;
        private System.Windows.Forms.Button button_editing_undoMaskDelete;
        private System.Windows.Forms.Panel panel_editing;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_prev;
        private System.Windows.Forms.ToolStripButton toolStripButton_next;
        private System.Windows.Forms.ToolStripButton toolStripButton_new;
        private System.Windows.Forms.ToolStripButton toolStripButton_option;
    }
}