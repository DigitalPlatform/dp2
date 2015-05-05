namespace dp2Circulation
{
    partial class OrderEditForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrderEditForm));
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_existing = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.orderEditControl_existing = new dp2Circulation.OrderEditControl();
            this.button_existing_undoMaskDelete = new System.Windows.Forms.Button();
            this.tableLayoutPanel_editing = new System.Windows.Forms.TableLayoutPanel();
            this.button_editing_undoMaskDelete = new System.Windows.Forms.Button();
            this.label_editing = new System.Windows.Forms.Label();
            this.panel_editing = new System.Windows.Forms.Panel();
            this.button_editing_nextRecord = new System.Windows.Forms.Button();
            this.button_editing_prevRecord = new System.Windows.Forms.Button();
            this.orderEditControl_editing = new dp2Circulation.OrderEditControl();
            this.checkBox_autoSearchDup = new System.Windows.Forms.CheckBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tableLayoutPanel_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_existing.SuspendLayout();
            this.tableLayoutPanel_editing.SuspendLayout();
            this.panel_editing.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel_main.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.textBox_message, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.splitContainer_main, 0, 1);
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(556, 336);
            this.tableLayoutPanel_main.TabIndex = 0;
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(3, 2);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(550, 65);
            this.textBox_message.TabIndex = 0;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(3, 71);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tableLayoutPanel_existing);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tableLayoutPanel_editing);
            this.splitContainer_main.Size = new System.Drawing.Size(550, 263);
            this.splitContainer_main.SplitterDistance = 267;
            this.splitContainer_main.TabIndex = 5;
            // 
            // tableLayoutPanel_existing
            // 
            this.tableLayoutPanel_existing.ColumnCount = 1;
            this.tableLayoutPanel_existing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_existing.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_existing.Controls.Add(this.orderEditControl_existing, 0, 1);
            this.tableLayoutPanel_existing.Controls.Add(this.button_existing_undoMaskDelete, 0, 2);
            this.tableLayoutPanel_existing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_existing.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_existing.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel_existing.Name = "tableLayoutPanel_existing";
            this.tableLayoutPanel_existing.RowCount = 3;
            this.tableLayoutPanel_existing.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_existing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_existing.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_existing.Size = new System.Drawing.Size(267, 263);
            this.tableLayoutPanel_existing.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "当前数据库中记录";
            // 
            // orderEditControl_existing
            // 
            this.orderEditControl_existing.AutoScroll = true;
            this.orderEditControl_existing.BatchNo = "";
            this.orderEditControl_existing.CatalogNo = "";
            this.orderEditControl_existing.Changed = false;
            this.orderEditControl_existing.Class = "";
            this.orderEditControl_existing.Comment = "";
            this.orderEditControl_existing.Copy = "";
            this.orderEditControl_existing.Distribute = "";
            this.orderEditControl_existing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.orderEditControl_existing.Index = "";
            this.orderEditControl_existing.Initializing = true;
            this.orderEditControl_existing.IssueCount = "";
            this.orderEditControl_existing.Location = new System.Drawing.Point(3, 17);
            this.orderEditControl_existing.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.orderEditControl_existing.MinimumSize = new System.Drawing.Size(100, 0);
            this.orderEditControl_existing.Name = "orderEditControl_existing";
            this.orderEditControl_existing.Operations = "";
            this.orderEditControl_existing.OrderID = "";
            this.orderEditControl_existing.OrderTime = "";
            this.orderEditControl_existing.ParentId = "";
            this.orderEditControl_existing.Price = "";
            this.orderEditControl_existing.Range = "";
            this.orderEditControl_existing.RecPath = "";
            this.orderEditControl_existing.RefID = "";
            this.orderEditControl_existing.Seller = "";
            this.orderEditControl_existing.SellerAddress = "";
            this.orderEditControl_existing.Size = new System.Drawing.Size(261, 212);
            this.orderEditControl_existing.Source = "";
            this.orderEditControl_existing.State = "";
            this.orderEditControl_existing.TabIndex = 1;
            this.orderEditControl_existing.TotalPrice = "";
            // 
            // button_existing_undoMaskDelete
            // 
            this.button_existing_undoMaskDelete.Location = new System.Drawing.Point(3, 233);
            this.button_existing_undoMaskDelete.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_existing_undoMaskDelete.Name = "button_existing_undoMaskDelete";
            this.button_existing_undoMaskDelete.Size = new System.Drawing.Size(163, 28);
            this.button_existing_undoMaskDelete.TabIndex = 2;
            this.button_existing_undoMaskDelete.Text = "撤销标记删除(&U)";
            this.button_existing_undoMaskDelete.UseVisualStyleBackColor = true;
            this.button_existing_undoMaskDelete.Visible = false;
            this.button_existing_undoMaskDelete.Click += new System.EventHandler(this.button_existing_undoMaskDelete_Click);
            // 
            // tableLayoutPanel_editing
            // 
            this.tableLayoutPanel_editing.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel_editing.ColumnCount = 1;
            this.tableLayoutPanel_editing.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_editing.Controls.Add(this.button_editing_undoMaskDelete, 0, 2);
            this.tableLayoutPanel_editing.Controls.Add(this.label_editing, 0, 0);
            this.tableLayoutPanel_editing.Controls.Add(this.panel_editing, 0, 1);
            this.tableLayoutPanel_editing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_editing.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_editing.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel_editing.Name = "tableLayoutPanel_editing";
            this.tableLayoutPanel_editing.RowCount = 3;
            this.tableLayoutPanel_editing.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_editing.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_editing.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_editing.Size = new System.Drawing.Size(279, 263);
            this.tableLayoutPanel_editing.TabIndex = 0;
            // 
            // button_editing_undoMaskDelete
            // 
            this.button_editing_undoMaskDelete.Location = new System.Drawing.Point(3, 233);
            this.button_editing_undoMaskDelete.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_editing_undoMaskDelete.Name = "button_editing_undoMaskDelete";
            this.button_editing_undoMaskDelete.Size = new System.Drawing.Size(163, 28);
            this.button_editing_undoMaskDelete.TabIndex = 1;
            this.button_editing_undoMaskDelete.Text = "撤销标记删除(&U)";
            this.button_editing_undoMaskDelete.UseVisualStyleBackColor = true;
            this.button_editing_undoMaskDelete.Click += new System.EventHandler(this.button_editing_undoMaskDelete_Click);
            // 
            // label_editing
            // 
            this.label_editing.AutoSize = true;
            this.label_editing.Location = new System.Drawing.Point(3, 0);
            this.label_editing.Name = "label_editing";
            this.label_editing.Size = new System.Drawing.Size(112, 15);
            this.label_editing.TabIndex = 0;
            this.label_editing.Text = "正在编辑的记录";
            // 
            // panel_editing
            // 
            this.panel_editing.BackColor = System.Drawing.SystemColors.Control;
            this.panel_editing.Controls.Add(this.button_editing_nextRecord);
            this.panel_editing.Controls.Add(this.button_editing_prevRecord);
            this.panel_editing.Controls.Add(this.orderEditControl_editing);
            this.panel_editing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_editing.Location = new System.Drawing.Point(3, 17);
            this.panel_editing.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel_editing.Name = "panel_editing";
            this.panel_editing.Size = new System.Drawing.Size(273, 212);
            this.panel_editing.TabIndex = 8;
            // 
            // button_editing_nextRecord
            // 
            this.button_editing_nextRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editing_nextRecord.Enabled = false;
            this.button_editing_nextRecord.Image = ((System.Drawing.Image)(resources.GetObject("button_editing_nextRecord.Image")));
            this.button_editing_nextRecord.Location = new System.Drawing.Point(244, 183);
            this.button_editing_nextRecord.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_editing_nextRecord.Name = "button_editing_nextRecord";
            this.button_editing_nextRecord.Size = new System.Drawing.Size(29, 28);
            this.button_editing_nextRecord.TabIndex = 2;
            this.button_editing_nextRecord.UseVisualStyleBackColor = true;
            this.button_editing_nextRecord.Click += new System.EventHandler(this.button_editing_nextRecord_Click);
            // 
            // button_editing_prevRecord
            // 
            this.button_editing_prevRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editing_prevRecord.Enabled = false;
            this.button_editing_prevRecord.Image = ((System.Drawing.Image)(resources.GetObject("button_editing_prevRecord.Image")));
            this.button_editing_prevRecord.Location = new System.Drawing.Point(244, 149);
            this.button_editing_prevRecord.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_editing_prevRecord.Name = "button_editing_prevRecord";
            this.button_editing_prevRecord.Size = new System.Drawing.Size(29, 28);
            this.button_editing_prevRecord.TabIndex = 1;
            this.button_editing_prevRecord.UseVisualStyleBackColor = true;
            this.button_editing_prevRecord.Click += new System.EventHandler(this.button_editing_prevRecord_Click);
            // 
            // orderEditControl_editing
            // 
            this.orderEditControl_editing.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.orderEditControl_editing.AutoScroll = true;
            this.orderEditControl_editing.BatchNo = "";
            this.orderEditControl_editing.CatalogNo = "";
            this.orderEditControl_editing.Changed = false;
            this.orderEditControl_editing.Class = "";
            this.orderEditControl_editing.Comment = "";
            this.orderEditControl_editing.Copy = "";
            this.orderEditControl_editing.Distribute = "";
            this.orderEditControl_editing.Index = "";
            this.orderEditControl_editing.Initializing = true;
            this.orderEditControl_editing.IssueCount = "";
            this.orderEditControl_editing.Location = new System.Drawing.Point(0, 0);
            this.orderEditControl_editing.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.orderEditControl_editing.MinimumSize = new System.Drawing.Size(75, 0);
            this.orderEditControl_editing.Name = "orderEditControl_editing";
            this.orderEditControl_editing.Operations = "";
            this.orderEditControl_editing.OrderID = "";
            this.orderEditControl_editing.OrderTime = "";
            this.orderEditControl_editing.ParentId = "";
            this.orderEditControl_editing.Price = "";
            this.orderEditControl_editing.Range = "";
            this.orderEditControl_editing.RecPath = "";
            this.orderEditControl_editing.RefID = "";
            this.orderEditControl_editing.Seller = "";
            this.orderEditControl_editing.SellerAddress = "";
            this.orderEditControl_editing.Size = new System.Drawing.Size(237, 212);
            this.orderEditControl_editing.Source = "";
            this.orderEditControl_editing.State = "";
            this.orderEditControl_editing.TabIndex = 0;
            this.orderEditControl_editing.TotalPrice = "";
            this.orderEditControl_editing.ContentChanged += new DigitalPlatform.ContentChangedEventHandler(this.orderEditControl_editing_ContentChanged);
            this.orderEditControl_editing.ControlKeyDown += new DigitalPlatform.ControlKeyEventHandler(this.orderEditControl_editing_ControlKeyDown);
            // 
            // checkBox_autoSearchDup
            // 
            this.checkBox_autoSearchDup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_autoSearchDup.AutoSize = true;
            this.checkBox_autoSearchDup.Checked = true;
            this.checkBox_autoSearchDup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_autoSearchDup.Location = new System.Drawing.Point(13, 353);
            this.checkBox_autoSearchDup.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBox_autoSearchDup.Name = "checkBox_autoSearchDup";
            this.checkBox_autoSearchDup.Size = new System.Drawing.Size(204, 19);
            this.checkBox_autoSearchDup.TabIndex = 0;
            this.checkBox_autoSearchDup.Text = "实时对参考ID进行查重(&R)";
            this.checkBox_autoSearchDup.UseVisualStyleBackColor = true;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(493, 382);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(412, 382);
            this.button_OK.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 0;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // OrderEditForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(580, 422);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Controls.Add(this.checkBox_autoSearchDup);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "OrderEditForm";
            this.ShowInTaskbar = false;
            this.Text = "采购信息";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OrderEditForm_FormClosed);
            this.Load += new System.EventHandler(this.OrderEditForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_existing.ResumeLayout(false);
            this.tableLayoutPanel_existing.PerformLayout();
            this.tableLayoutPanel_editing.ResumeLayout(false);
            this.tableLayoutPanel_editing.PerformLayout();
            this.panel_editing.ResumeLayout(false);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_existing;
        private System.Windows.Forms.Label label1;
        private OrderEditControl orderEditControl_existing;
        private System.Windows.Forms.Button button_existing_undoMaskDelete;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_editing;
        private System.Windows.Forms.Button button_editing_undoMaskDelete;
        private System.Windows.Forms.Label label_editing;
        private System.Windows.Forms.Panel panel_editing;
        private System.Windows.Forms.Button button_editing_nextRecord;
        private System.Windows.Forms.Button button_editing_prevRecord;
        private OrderEditControl orderEditControl_editing;
        private System.Windows.Forms.CheckBox checkBox_autoSearchDup;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}