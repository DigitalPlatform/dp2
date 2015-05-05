namespace DigitalPlatform.CommonControl
{
    partial class OrderDesignControl2
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel_copies = new System.Windows.Forms.TableLayoutPanel();
            this.label_newlyArriveTotalCopy = new System.Windows.Forms.Label();
            this.label_newlyOrderTotalCopy = new System.Windows.Forms.Label();
            this.textBox_orderedTotalCopy = new System.Windows.Forms.TextBox();
            this.textBox_newlyOrderTotalCopy = new System.Windows.Forms.TextBox();
            this.label_orderedTotalCopy = new System.Windows.Forms.Label();
            this.button_newItem = new System.Windows.Forms.Button();
            this.label_arrivedTotalCopy = new System.Windows.Forms.Label();
            this.textBox_arrivedTotalCopy = new System.Windows.Forms.TextBox();
            this.textBox_newlyArriveTotalCopy = new System.Windows.Forms.TextBox();
            this.button_fullyAccept = new System.Windows.Forms.Button();
            this.panel_targetRecPath = new System.Windows.Forms.Panel();
            this.textBox_targetRecPath = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.dpTable1 = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn1 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn2 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn3 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_range = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_issueCount = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_copy = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_price = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_location = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn9 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn10 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn11 = new DigitalPlatform.CommonControl.DpColumn();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.dpColumn_color = new DigitalPlatform.CommonControl.DpColumn();
            this.tableLayoutPanel_copies.SuspendLayout();
            this.panel_targetRecPath.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_copies
            // 
            this.tableLayoutPanel_copies.AutoSize = true;
            this.tableLayoutPanel_copies.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel_copies.ColumnCount = 3;
            this.tableLayoutPanel_copies.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_copies.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_copies.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 146F));
            this.tableLayoutPanel_copies.Controls.Add(this.label_newlyArriveTotalCopy, 0, 3);
            this.tableLayoutPanel_copies.Controls.Add(this.label_newlyOrderTotalCopy, 0, 1);
            this.tableLayoutPanel_copies.Controls.Add(this.textBox_orderedTotalCopy, 1, 0);
            this.tableLayoutPanel_copies.Controls.Add(this.textBox_newlyOrderTotalCopy, 1, 1);
            this.tableLayoutPanel_copies.Controls.Add(this.label_orderedTotalCopy, 0, 0);
            this.tableLayoutPanel_copies.Controls.Add(this.button_newItem, 2, 1);
            this.tableLayoutPanel_copies.Controls.Add(this.label_arrivedTotalCopy, 0, 2);
            this.tableLayoutPanel_copies.Controls.Add(this.textBox_arrivedTotalCopy, 1, 2);
            this.tableLayoutPanel_copies.Controls.Add(this.textBox_newlyArriveTotalCopy, 1, 3);
            this.tableLayoutPanel_copies.Controls.Add(this.button_fullyAccept, 2, 3);
            this.tableLayoutPanel_copies.Controls.Add(this.panel_targetRecPath, 2, 2);
            this.tableLayoutPanel_copies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_copies.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_copies.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_copies.Name = "tableLayoutPanel_copies";
            this.tableLayoutPanel_copies.RowCount = 4;
            this.tableLayoutPanel_copies.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_copies.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_copies.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_copies.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_copies.Size = new System.Drawing.Size(295, 102);
            this.tableLayoutPanel_copies.TabIndex = 8;
            // 
            // label_newlyArriveTotalCopy
            // 
            this.label_newlyArriveTotalCopy.AutoSize = true;
            this.label_newlyArriveTotalCopy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_newlyArriveTotalCopy.Location = new System.Drawing.Point(2, 76);
            this.label_newlyArriveTotalCopy.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_newlyArriveTotalCopy.Name = "label_newlyArriveTotalCopy";
            this.label_newlyArriveTotalCopy.Size = new System.Drawing.Size(65, 26);
            this.label_newlyArriveTotalCopy.TabIndex = 9;
            this.label_newlyArriveTotalCopy.Text = "新验收(&R):";
            this.label_newlyArriveTotalCopy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_newlyArriveTotalCopy.Visible = false;
            // 
            // label_newlyOrderTotalCopy
            // 
            this.label_newlyOrderTotalCopy.AutoSize = true;
            this.label_newlyOrderTotalCopy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_newlyOrderTotalCopy.Location = new System.Drawing.Point(2, 25);
            this.label_newlyOrderTotalCopy.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_newlyOrderTotalCopy.Name = "label_newlyOrderTotalCopy";
            this.label_newlyOrderTotalCopy.Size = new System.Drawing.Size(65, 26);
            this.label_newlyOrderTotalCopy.TabIndex = 3;
            this.label_newlyOrderTotalCopy.Text = "新订购(&N):";
            this.label_newlyOrderTotalCopy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_orderedTotalCopy
            // 
            this.textBox_orderedTotalCopy.Location = new System.Drawing.Point(71, 2);
            this.textBox_orderedTotalCopy.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_orderedTotalCopy.Name = "textBox_orderedTotalCopy";
            this.textBox_orderedTotalCopy.ReadOnly = true;
            this.textBox_orderedTotalCopy.Size = new System.Drawing.Size(76, 21);
            this.textBox_orderedTotalCopy.TabIndex = 6;
            this.textBox_orderedTotalCopy.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_orderedTotalCopy.Visible = false;
            // 
            // textBox_newlyOrderTotalCopy
            // 
            this.textBox_newlyOrderTotalCopy.Location = new System.Drawing.Point(71, 27);
            this.textBox_newlyOrderTotalCopy.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_newlyOrderTotalCopy.Name = "textBox_newlyOrderTotalCopy";
            this.textBox_newlyOrderTotalCopy.Size = new System.Drawing.Size(76, 21);
            this.textBox_newlyOrderTotalCopy.TabIndex = 4;
            this.textBox_newlyOrderTotalCopy.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label_orderedTotalCopy
            // 
            this.label_orderedTotalCopy.AutoSize = true;
            this.label_orderedTotalCopy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_orderedTotalCopy.Location = new System.Drawing.Point(2, 0);
            this.label_orderedTotalCopy.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_orderedTotalCopy.Name = "label_orderedTotalCopy";
            this.label_orderedTotalCopy.Size = new System.Drawing.Size(65, 25);
            this.label_orderedTotalCopy.TabIndex = 5;
            this.label_orderedTotalCopy.Text = "已订购(&O):";
            this.label_orderedTotalCopy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_orderedTotalCopy.Visible = false;
            // 
            // button_newItem
            // 
            this.button_newItem.Location = new System.Drawing.Point(151, 27);
            this.button_newItem.Margin = new System.Windows.Forms.Padding(2);
            this.button_newItem.Name = "button_newItem";
            this.button_newItem.Size = new System.Drawing.Size(88, 22);
            this.button_newItem.TabIndex = 7;
            this.button_newItem.Text = "新增事项(&N)";
            this.button_newItem.UseVisualStyleBackColor = true;
            // 
            // label_arrivedTotalCopy
            // 
            this.label_arrivedTotalCopy.AutoSize = true;
            this.label_arrivedTotalCopy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_arrivedTotalCopy.Location = new System.Drawing.Point(2, 51);
            this.label_arrivedTotalCopy.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_arrivedTotalCopy.Name = "label_arrivedTotalCopy";
            this.label_arrivedTotalCopy.Size = new System.Drawing.Size(65, 25);
            this.label_arrivedTotalCopy.TabIndex = 8;
            this.label_arrivedTotalCopy.Text = "已验收(&A):";
            this.label_arrivedTotalCopy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label_arrivedTotalCopy.Visible = false;
            // 
            // textBox_arrivedTotalCopy
            // 
            this.textBox_arrivedTotalCopy.Location = new System.Drawing.Point(71, 53);
            this.textBox_arrivedTotalCopy.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_arrivedTotalCopy.Name = "textBox_arrivedTotalCopy";
            this.textBox_arrivedTotalCopy.ReadOnly = true;
            this.textBox_arrivedTotalCopy.Size = new System.Drawing.Size(76, 21);
            this.textBox_arrivedTotalCopy.TabIndex = 10;
            this.textBox_arrivedTotalCopy.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_arrivedTotalCopy.Visible = false;
            // 
            // textBox_newlyArriveTotalCopy
            // 
            this.textBox_newlyArriveTotalCopy.Location = new System.Drawing.Point(71, 78);
            this.textBox_newlyArriveTotalCopy.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_newlyArriveTotalCopy.Name = "textBox_newlyArriveTotalCopy";
            this.textBox_newlyArriveTotalCopy.Size = new System.Drawing.Size(76, 21);
            this.textBox_newlyArriveTotalCopy.TabIndex = 11;
            this.textBox_newlyArriveTotalCopy.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_newlyArriveTotalCopy.Visible = false;
            // 
            // button_fullyAccept
            // 
            this.button_fullyAccept.Location = new System.Drawing.Point(151, 78);
            this.button_fullyAccept.Margin = new System.Windows.Forms.Padding(2);
            this.button_fullyAccept.Name = "button_fullyAccept";
            this.button_fullyAccept.Size = new System.Drawing.Size(88, 22);
            this.button_fullyAccept.TabIndex = 12;
            this.button_fullyAccept.Text = "全部验收(&F)";
            this.button_fullyAccept.UseVisualStyleBackColor = true;
            this.button_fullyAccept.Visible = false;
            // 
            // panel_targetRecPath
            // 
            this.panel_targetRecPath.BackColor = System.Drawing.SystemColors.Info;
            this.panel_targetRecPath.Controls.Add(this.textBox_targetRecPath);
            this.panel_targetRecPath.Controls.Add(this.label9);
            this.panel_targetRecPath.ForeColor = System.Drawing.SystemColors.InfoText;
            this.panel_targetRecPath.Location = new System.Drawing.Point(151, 53);
            this.panel_targetRecPath.Margin = new System.Windows.Forms.Padding(2);
            this.panel_targetRecPath.Name = "panel_targetRecPath";
            this.panel_targetRecPath.Size = new System.Drawing.Size(142, 20);
            this.panel_targetRecPath.TabIndex = 13;
            this.panel_targetRecPath.Visible = false;
            // 
            // textBox_targetRecPath
            // 
            this.textBox_targetRecPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_targetRecPath.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_targetRecPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_targetRecPath.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_targetRecPath.Location = new System.Drawing.Point(63, 3);
            this.textBox_targetRecPath.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_targetRecPath.Name = "textBox_targetRecPath";
            this.textBox_targetRecPath.ReadOnly = true;
            this.textBox_targetRecPath.Size = new System.Drawing.Size(80, 14);
            this.textBox_targetRecPath.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.SystemColors.Info;
            this.label9.ForeColor = System.Drawing.SystemColors.InfoText;
            this.label9.Location = new System.Drawing.Point(2, 4);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(59, 12);
            this.label9.TabIndex = 0;
            this.label9.Text = "目标记录:";
            // 
            // dpTable1
            // 
            this.dpTable1.AutoDocCenter = true;
            this.dpTable1.Columns.Add(this.dpColumn_color);
            this.dpTable1.Columns.Add(this.dpColumn1);
            this.dpTable1.Columns.Add(this.dpColumn2);
            this.dpTable1.Columns.Add(this.dpColumn3);
            this.dpTable1.Columns.Add(this.dpColumn_range);
            this.dpTable1.Columns.Add(this.dpColumn_issueCount);
            this.dpTable1.Columns.Add(this.dpColumn_copy);
            this.dpTable1.Columns.Add(this.dpColumn_price);
            this.dpTable1.Columns.Add(this.dpColumn_location);
            this.dpTable1.Columns.Add(this.dpColumn9);
            this.dpTable1.Columns.Add(this.dpColumn10);
            this.dpTable1.Columns.Add(this.dpColumn11);
            this.dpTable1.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.dpTable1.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.dpTable1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dpTable1.DocumentBorderColor = System.Drawing.SystemColors.ControlDark;
            this.dpTable1.DocumentOrgX = ((long)(0));
            this.dpTable1.DocumentOrgY = ((long)(0));
            this.dpTable1.DocumentShadowColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dpTable1.FocusedItem = null;
            this.dpTable1.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable1.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable1.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable1.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable1.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable1.Location = new System.Drawing.Point(3, 105);
            this.dpTable1.Name = "dpTable1";
            this.dpTable1.Size = new System.Drawing.Size(289, 102);
            this.dpTable1.TabIndex = 9;
            this.dpTable1.Text = "dpTable1";
            // 
            // dpColumn1
            // 
            this.dpColumn1.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn1.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn1.Font = null;
            this.dpColumn1.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn1.Text = "书目号";
            // 
            // dpColumn2
            // 
            this.dpColumn2.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn2.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn2.Font = null;
            this.dpColumn2.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn2.Text = "渠道";
            // 
            // dpColumn3
            // 
            this.dpColumn3.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn3.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn3.Font = null;
            this.dpColumn3.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn3.Text = "经费来源";
            // 
            // dpColumn_range
            // 
            this.dpColumn_range.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_range.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_range.Font = null;
            this.dpColumn_range.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_range.Text = "预计出版时间";
            // 
            // dpColumn_issueCount
            // 
            this.dpColumn_issueCount.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_issueCount.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_issueCount.Font = null;
            this.dpColumn_issueCount.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_issueCount.Text = "期数";
            // 
            // dpColumn_copy
            // 
            this.dpColumn_copy.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_copy.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_copy.Font = null;
            this.dpColumn_copy.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_copy.Text = "复本数";
            // 
            // dpColumn_price
            // 
            this.dpColumn_price.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_price.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_price.Font = null;
            this.dpColumn_price.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_price.Text = "单价";
            // 
            // dpColumn_location
            // 
            this.dpColumn_location.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_location.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_location.Font = null;
            this.dpColumn_location.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_location.Text = "去向";
            // 
            // dpColumn9
            // 
            this.dpColumn9.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn9.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn9.Font = null;
            this.dpColumn9.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn9.Text = "类别";
            // 
            // dpColumn10
            // 
            this.dpColumn10.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn10.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn10.Font = null;
            this.dpColumn10.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn10.Text = "渠道地址";
            // 
            // dpColumn11
            // 
            this.dpColumn11.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn11.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn11.Font = null;
            this.dpColumn11.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn11.Text = "其他";
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.Controls.Add(this.tableLayoutPanel_copies, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.dpTable1, 0, 1);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(295, 210);
            this.tableLayoutPanel_main.TabIndex = 10;
            // 
            // dpColumn_color
            // 
            this.dpColumn_color.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_color.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_color.Font = null;
            this.dpColumn_color.ForeColor = System.Drawing.Color.Transparent;
            // 
            // OrderDesignControl2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Name = "OrderDesignControl2";
            this.Size = new System.Drawing.Size(295, 210);
            this.tableLayoutPanel_copies.ResumeLayout(false);
            this.tableLayoutPanel_copies.PerformLayout();
            this.panel_targetRecPath.ResumeLayout(false);
            this.panel_targetRecPath.PerformLayout();
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_copies;
        private System.Windows.Forms.Label label_newlyArriveTotalCopy;
        private System.Windows.Forms.Label label_newlyOrderTotalCopy;
        internal System.Windows.Forms.TextBox textBox_orderedTotalCopy;
        internal System.Windows.Forms.TextBox textBox_newlyOrderTotalCopy;
        private System.Windows.Forms.Label label_orderedTotalCopy;
        private System.Windows.Forms.Button button_newItem;
        private System.Windows.Forms.Label label_arrivedTotalCopy;
        internal System.Windows.Forms.TextBox textBox_arrivedTotalCopy;
        internal System.Windows.Forms.TextBox textBox_newlyArriveTotalCopy;
        private System.Windows.Forms.Button button_fullyAccept;
        private System.Windows.Forms.Panel panel_targetRecPath;
        private System.Windows.Forms.TextBox textBox_targetRecPath;
        private System.Windows.Forms.Label label9;
        internal DpTable dpTable1;
        private DpColumn dpColumn1;
        private DpColumn dpColumn2;
        private DpColumn dpColumn3;
        private DpColumn dpColumn_range;
        private DpColumn dpColumn_issueCount;
        private DpColumn dpColumn_copy;
        private DpColumn dpColumn_price;
        private DpColumn dpColumn_location;
        private DpColumn dpColumn9;
        private DpColumn dpColumn10;
        private DpColumn dpColumn11;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private DpColumn dpColumn_color;
    }
}
