namespace dp2Circulation
{
    partial class BindingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BindingForm));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_editArea = new System.Windows.Forms.TableLayoutPanel();
            this.orderDesignControl1 = new DigitalPlatform.CommonControl.OrderDesignControl();
            this.toolStrip_editArea = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_closeTextArea = new System.Windows.Forms.ToolStripButton();
            this.checkBox_displayEditArea = new System.Windows.Forms.CheckBox();
            this.button_option = new System.Windows.Forms.Button();
            this.bindingControl1 = new dp2Circulation.BindingControl();
            this.entityEditControl1 = new dp2Circulation.EntityEditControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_editArea.SuspendLayout();
            this.toolStrip_editArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(419, 228);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(358, 228);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(9, 10);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.bindingControl1);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tableLayoutPanel_editArea);
            this.splitContainer_main.Size = new System.Drawing.Size(466, 213);
            this.splitContainer_main.SplitterDistance = 301;
            this.splitContainer_main.SplitterWidth = 6;
            this.splitContainer_main.TabIndex = 0;
            // 
            // tableLayoutPanel_editArea
            // 
            this.tableLayoutPanel_editArea.ColumnCount = 1;
            this.tableLayoutPanel_editArea.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_editArea.Controls.Add(this.entityEditControl1, 0, 1);
            this.tableLayoutPanel_editArea.Controls.Add(this.orderDesignControl1, 0, 2);
            this.tableLayoutPanel_editArea.Controls.Add(this.toolStrip_editArea, 0, 0);
            this.tableLayoutPanel_editArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_editArea.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_editArea.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_editArea.Name = "tableLayoutPanel_editArea";
            this.tableLayoutPanel_editArea.RowCount = 3;
            this.tableLayoutPanel_editArea.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel_editArea.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_editArea.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_editArea.Size = new System.Drawing.Size(159, 213);
            this.tableLayoutPanel_editArea.TabIndex = 0;
            // 
            // orderDesignControl1
            // 
            this.orderDesignControl1.AutoScroll = true;
            this.orderDesignControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.orderDesignControl1.Changed = true;
            this.orderDesignControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.orderDesignControl1.Location = new System.Drawing.Point(2, 109);
            this.orderDesignControl1.Margin = new System.Windows.Forms.Padding(2);
            this.orderDesignControl1.Name = "orderDesignControl1";
            this.orderDesignControl1.NewlyOrderTotalCopy = 0;
            this.orderDesignControl1.Size = new System.Drawing.Size(155, 102);
            this.orderDesignControl1.TabIndex = 1;
            this.orderDesignControl1.TargetRecPath = "";
            this.orderDesignControl1.Visible = false;
            this.orderDesignControl1.VisibleChanged += new System.EventHandler(this.orderDesignControl1_VisibleChanged);
            this.orderDesignControl1.Leave += new System.EventHandler(this.orderDesignControl1_Leave);
            // 
            // toolStrip_editArea
            // 
            this.toolStrip_editArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip_editArea.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_editArea.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_closeTextArea});
            this.toolStrip_editArea.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_editArea.Name = "toolStrip_editArea";
            this.toolStrip_editArea.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip_editArea.Size = new System.Drawing.Size(159, 16);
            this.toolStrip_editArea.TabIndex = 2;
            this.toolStrip_editArea.Text = "toolStrip1";
            // 
            // toolStripButton_closeTextArea
            // 
            this.toolStripButton_closeTextArea.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_closeTextArea.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_closeTextArea.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_closeTextArea.Image")));
            this.toolStripButton_closeTextArea.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_closeTextArea.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButton_closeTextArea.Name = "toolStripButton_closeTextArea";
            this.toolStripButton_closeTextArea.Size = new System.Drawing.Size(23, 13);
            this.toolStripButton_closeTextArea.Text = "隐藏编辑区域";
            this.toolStripButton_closeTextArea.Click += new System.EventHandler(this.toolStripButton_closeTextArea_Click);
            // 
            // checkBox_displayEditArea
            // 
            this.checkBox_displayEditArea.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_displayEditArea.AutoSize = true;
            this.checkBox_displayEditArea.Checked = true;
            this.checkBox_displayEditArea.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_displayEditArea.Location = new System.Drawing.Point(90, 227);
            this.checkBox_displayEditArea.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_displayEditArea.Name = "checkBox_displayEditArea";
            this.checkBox_displayEditArea.Size = new System.Drawing.Size(162, 16);
            this.checkBox_displayEditArea.TabIndex = 2;
            this.checkBox_displayEditArea.Text = "显示(单元格)编辑区域(&E)";
            this.checkBox_displayEditArea.UseVisualStyleBackColor = true;
            this.checkBox_displayEditArea.CheckedChanged += new System.EventHandler(this.checkBox_displayEditArea_CheckedChanged);
            // 
            // button_option
            // 
            this.button_option.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_option.Location = new System.Drawing.Point(10, 228);
            this.button_option.Margin = new System.Windows.Forms.Padding(2);
            this.button_option.Name = "button_option";
            this.button_option.Size = new System.Drawing.Size(76, 22);
            this.button_option.TabIndex = 1;
            this.button_option.Text = "选项(&O)...";
            this.button_option.UseVisualStyleBackColor = true;
            this.button_option.Click += new System.EventHandler(this.button_option1_Click);
            // 
            // bindingControl1
            // 
            this.bindingControl1.AcceptBatchNo = "";
            this.bindingControl1.AcceptBatchNoInputed = false;
            this.bindingControl1.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.bindingControl1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.bindingControl1.BiblioDbName = "";
            this.bindingControl1.Changed = false;
            this.bindingControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bindingControl1.DocumentOrgX = ((long)(0));
            this.bindingControl1.DocumentOrgY = ((long)(0));
            this.bindingControl1.FocusObject = null;
            this.bindingControl1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bindingControl1.HoverObject = null;
            this.bindingControl1.Location = new System.Drawing.Point(0, 0);
            this.bindingControl1.Margin = new System.Windows.Forms.Padding(2);
            this.bindingControl1.Name = "bindingControl1";
            this.bindingControl1.Size = new System.Drawing.Size(301, 213);
            this.bindingControl1.TabIndex = 0;
            this.bindingControl1.Text = "bindingControl1";
            this.bindingControl1.EditArea += new dp2Circulation.EditAreaEventHandler(this.bindingControl1_EditArea);
            this.bindingControl1.CellFocusChanged += new dp2Circulation.FocusChangedEventHandler(this.bindingControl1_CellFocusChanged);
            // 
            // entityEditControl1
            // 
            this.entityEditControl1.AccessNo = "";
            this.entityEditControl1.AutoScroll = true;
            this.entityEditControl1.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.entityEditControl1.Barcode = "";
            this.entityEditControl1.BatchNo = "";
            this.entityEditControl1.Binding = "";
            this.entityEditControl1.BindingCost = "";
            this.entityEditControl1.BookType = "";
            this.entityEditControl1.BorrowDate = "";
            this.entityEditControl1.Borrower = "";
            this.entityEditControl1.BorrowPeriod = "";
            this.entityEditControl1.Changed = false;
            this.entityEditControl1.Comment = "";
            this.entityEditControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityEditControl1.Initializing = true;
            this.entityEditControl1.Intact = "";
            this.entityEditControl1.Location = new System.Drawing.Point(0, 16);
            this.entityEditControl1.LocationString = "";
            this.entityEditControl1.Margin = new System.Windows.Forms.Padding(0);
            this.entityEditControl1.MemberBackColor = System.Drawing.Color.WhiteSmoke;
            this.entityEditControl1.MemberForeColor = System.Drawing.SystemColors.ControlText;
            this.entityEditControl1.MergeComment = "";
            this.entityEditControl1.MinimumSize = new System.Drawing.Size(75, 0);
            this.entityEditControl1.Name = "entityEditControl1";
            this.entityEditControl1.Operations = "";
            this.entityEditControl1.ParentId = "";
            this.entityEditControl1.Price = "";
            this.entityEditControl1.PublishTime = "";
            this.entityEditControl1.RecPath = "";
            this.entityEditControl1.RefID = "";
            this.entityEditControl1.RegisterNo = "";
            this.entityEditControl1.Seller = "";
            this.entityEditControl1.Size = new System.Drawing.Size(159, 91);
            this.entityEditControl1.Source = "";
            this.entityEditControl1.State = "";
            this.entityEditControl1.TabIndex = 0;
            this.entityEditControl1.Volume = "";
            this.entityEditControl1.PaintContent += new System.Windows.Forms.PaintEventHandler(this.entityEditControl1_PaintContent);
            this.entityEditControl1.ControlKeyDown += new DigitalPlatform.ControlKeyEventHandler(this.entityEditControl1_ControlKeyDown);
            this.entityEditControl1.VisibleChanged += new System.EventHandler(this.entityEditControl1_VisibleChanged);
            this.entityEditControl1.Leave += new System.EventHandler(this.entityEditControl1_Leave);
            // 
            // BindingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(484, 260);
            this.Controls.Add(this.button_option);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_displayEditArea);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "BindingForm";
            this.ShowInTaskbar = false;
            this.Text = "装订";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BindingForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BindingForm_FormClosed);
            this.Load += new System.EventHandler(this.BindingForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_editArea.ResumeLayout(false);
            this.tableLayoutPanel_editArea.PerformLayout();
            this.toolStrip_editArea.ResumeLayout(false);
            this.toolStrip_editArea.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private BindingControl bindingControl1;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.CheckBox checkBox_displayEditArea;
        private System.Windows.Forms.Button button_option;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_editArea;
        private DigitalPlatform.CommonControl.OrderDesignControl orderDesignControl1;
        private System.Windows.Forms.ToolStrip toolStrip_editArea;
        private System.Windows.Forms.ToolStripButton toolStripButton_closeTextArea;
        private EntityEditControl entityEditControl1;
    }
}