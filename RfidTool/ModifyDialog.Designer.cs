﻿
namespace RfidTool
{
    partial class ModifyDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyDialog));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_begin = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_exportUidPiiMap = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.listView_tags = new System.Windows.Forms.ListView();
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_errorInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_tu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_oi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_eas = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStripButton_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_begin,
            this.toolStripSeparator1,
            this.toolStripButton_exportUidPiiMap,
            this.toolStripButton_stop});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(951, 44);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_begin
            // 
            this.toolStripButton_begin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_begin.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_begin.Image")));
            this.toolStripButton_begin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_begin.Name = "toolStripButton_begin";
            this.toolStripButton_begin.Size = new System.Drawing.Size(58, 38);
            this.toolStripButton_begin.Text = "开始";
            this.toolStripButton_begin.ToolTipText = "开始修改";
            this.toolStripButton_begin.Click += new System.EventHandler(this.toolStripButton_begin_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 44);
            // 
            // toolStripButton_exportUidPiiMap
            // 
            this.toolStripButton_exportUidPiiMap.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_exportUidPiiMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_exportUidPiiMap.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_exportUidPiiMap.Image")));
            this.toolStripButton_exportUidPiiMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_exportUidPiiMap.Name = "toolStripButton_exportUidPiiMap";
            this.toolStripButton_exportUidPiiMap.Size = new System.Drawing.Size(142, 38);
            this.toolStripButton_exportUidPiiMap.Text = "导出对照关系";
            this.toolStripButton_exportUidPiiMap.Click += new System.EventHandler(this.toolStripButton_exportUidPiiMap_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 543);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(951, 37);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 27);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(832, 28);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // listView_tags
            // 
            this.listView_tags.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_uid,
            this.columnHeader_errorInfo,
            this.columnHeader_pii,
            this.columnHeader_tu,
            this.columnHeader_oi,
            this.columnHeader_eas});
            this.listView_tags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_tags.FullRowSelect = true;
            this.listView_tags.HideSelection = false;
            this.listView_tags.Location = new System.Drawing.Point(0, 44);
            this.listView_tags.Name = "listView_tags";
            this.listView_tags.Size = new System.Drawing.Size(951, 499);
            this.listView_tags.TabIndex = 2;
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
            // columnHeader_eas
            // 
            this.columnHeader_eas.Text = "EAS";
            this.columnHeader_eas.Width = 89;
            // 
            // toolStripButton_stop
            // 
            this.toolStripButton_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_stop.Enabled = false;
            this.toolStripButton_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_stop.Image")));
            this.toolStripButton_stop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_stop.Name = "toolStripButton_stop";
            this.toolStripButton_stop.Size = new System.Drawing.Size(58, 38);
            this.toolStripButton_stop.Text = "停止";
            this.toolStripButton_stop.ToolTipText = "停止修改";
            this.toolStripButton_stop.Click += new System.EventHandler(this.toolStripButton_stop_Click);
            // 
            // ModifyDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(951, 580);
            this.Controls.Add(this.listView_tags);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "ModifyDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "修改标签";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModifyDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ModifyDialog_FormClosed);
            this.Load += new System.EventHandler(this.ModifyDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ListView listView_tags;
        private System.Windows.Forms.ColumnHeader columnHeader_uid;
        private System.Windows.Forms.ColumnHeader columnHeader_errorInfo;
        private System.Windows.Forms.ColumnHeader columnHeader_pii;
        private System.Windows.Forms.ColumnHeader columnHeader_tu;
        private System.Windows.Forms.ColumnHeader columnHeader_oi;
        private System.Windows.Forms.ColumnHeader columnHeader_eas;
        private System.Windows.Forms.ToolStripButton toolStripButton_begin;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_exportUidPiiMap;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripButton toolStripButton_stop;
    }
}