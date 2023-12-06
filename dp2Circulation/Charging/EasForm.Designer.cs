namespace dp2Circulation.Charging
{
    partial class EasForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EasForm));
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_summary = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_reason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label_message = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label_number = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_clearAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_detailMode = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_numberMode = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.BackColor = System.Drawing.Color.Yellow;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_uid,
            this.columnHeader_pii,
            this.columnHeader_summary,
            this.columnHeader_reason});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(4, 4);
            this.listView1.Margin = new System.Windows.Forms.Padding(4);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(731, 247);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_uid
            // 
            this.columnHeader_uid.Text = "UID";
            this.columnHeader_uid.Width = 174;
            // 
            // columnHeader_pii
            // 
            this.columnHeader_pii.Text = "PII";
            this.columnHeader_pii.Width = 250;
            // 
            // columnHeader_summary
            // 
            this.columnHeader_summary.Text = "摘要";
            this.columnHeader_summary.Width = 600;
            // 
            // columnHeader_reason
            // 
            this.columnHeader_reason.Text = "动机";
            this.columnHeader_reason.Width = 200;
            // 
            // label_message
            // 
            this.label_message.AutoSize = true;
            this.label_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_message.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_message.Location = new System.Drawing.Point(4, 255);
            this.label_message.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(731, 55);
            this.label_message.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.listView1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label_message, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label_number, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 38);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(739, 456);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // label_number
            // 
            this.label_number.AutoSize = true;
            this.label_number.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_number.Font = new System.Drawing.Font("微软雅黑", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_number.Location = new System.Drawing.Point(4, 310);
            this.label_number.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_number.Name = "label_number";
            this.label_number.Size = new System.Drawing.Size(731, 146);
            this.label_number.TabIndex = 2;
            this.label_number.Text = "0";
            this.label_number.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label_number.Visible = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_clearAll,
            this.toolStripDropDownButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(739, 38);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_clearAll
            // 
            this.toolStripButton_clearAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_clearAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clearAll.Image")));
            this.toolStripButton_clearAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clearAll.Name = "toolStripButton_clearAll";
            this.toolStripButton_clearAll.Size = new System.Drawing.Size(100, 32);
            this.toolStripButton_clearAll.Text = "全部移除";
            this.toolStripButton_clearAll.Click += new System.EventHandler(this.toolStripButton_clearAll_Click);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_detailMode,
            this.ToolStripMenuItem_numberMode});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(117, 32);
            this.toolStripDropDownButton1.Text = "显示状态";
            // 
            // ToolStripMenuItem_detailMode
            // 
            this.ToolStripMenuItem_detailMode.Checked = true;
            this.ToolStripMenuItem_detailMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToolStripMenuItem_detailMode.Name = "ToolStripMenuItem_detailMode";
            this.ToolStripMenuItem_detailMode.Size = new System.Drawing.Size(171, 40);
            this.ToolStripMenuItem_detailMode.Text = "列表";
            this.ToolStripMenuItem_detailMode.Click += new System.EventHandler(this.ToolStripMenuItem_detailMode_Click);
            // 
            // ToolStripMenuItem_numberMode
            // 
            this.ToolStripMenuItem_numberMode.Name = "ToolStripMenuItem_numberMode";
            this.ToolStripMenuItem_numberMode.Size = new System.Drawing.Size(171, 40);
            this.ToolStripMenuItem_numberMode.Text = "数字";
            this.ToolStripMenuItem_numberMode.Click += new System.EventHandler(this.ToolStripMenuItem_numberMode_Click);
            // 
            // EasForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Yellow;
            this.ClientSize = new System.Drawing.Size(739, 494);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EasForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "请修正 EAS 状态";
            this.Activated += new System.EventHandler(this.EasForm_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EasForm_FormClosed);
            this.Load += new System.EventHandler(this.EasForm_Load);
            this.VisibleChanged += new System.EventHandler(this.EasForm_VisibleChanged);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ColumnHeader columnHeader_pii;
        private System.Windows.Forms.ColumnHeader columnHeader_summary;
        private System.Windows.Forms.ColumnHeader columnHeader_uid;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_clearAll;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_detailMode;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_numberMode;
        private System.Windows.Forms.Label label_number;
        private System.Windows.Forms.ColumnHeader columnHeader_reason;
    }
}