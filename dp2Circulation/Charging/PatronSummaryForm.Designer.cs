namespace dp2Circulation
{
    partial class PatronSummaryForm
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
            this.components = new System.ComponentModel.Container();
            this.dpTable1 = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn_content = new DigitalPlatform.CommonControl.DpColumn();
            this.label_comment = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // dpTable1
            // 
            this.dpTable1.AutoDocCenter = false;
            this.dpTable1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dpTable1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dpTable1.Columns.Add(this.dpColumn_content);
            this.dpTable1.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.dpTable1.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.dpTable1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dpTable1.DocumentBorderColor = System.Drawing.Color.Transparent;
            this.dpTable1.DocumentOrgX = ((long)(0));
            this.dpTable1.DocumentOrgY = ((long)(0));
            this.dpTable1.DocumentShadowColor = System.Drawing.Color.Transparent;
            this.dpTable1.FocusedItem = null;
            this.dpTable1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.dpTable1.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable1.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable1.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable1.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable1.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable1.LineDistance = 10;
            this.dpTable1.Location = new System.Drawing.Point(0, 0);
            this.dpTable1.Name = "dpTable1";
            this.dpTable1.Padding = new System.Windows.Forms.Padding(4);
            this.dpTable1.Size = new System.Drawing.Size(317, 264);
            this.dpTable1.TabIndex = 0;
            this.dpTable1.Text = "dpTable1";
            this.dpTable1.PaintRegion += new DigitalPlatform.CommonControl.PaintRegionEventHandler(this.dpTable1_PaintRegion);
            this.dpTable1.SizeChanged += new System.EventHandler(this.dpTable1_SizeChanged);
            // 
            // dpColumn_content
            // 
            this.dpColumn_content.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_content.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_content.Font = null;
            this.dpColumn_content.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_content.Text = "内容";
            // 
            // label_comment
            // 
            this.label_comment.BackColor = System.Drawing.SystemColors.Info;
            this.label_comment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_comment.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.label_comment.Location = new System.Drawing.Point(0, 0);
            this.label_comment.Name = "label_comment";
            this.label_comment.Size = new System.Drawing.Size(317, 264);
            this.label_comment.TabIndex = 1;
            this.label_comment.Text = "label1";
            this.label_comment.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_comment.Click += new System.EventHandler(this.label_comment_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 4000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // PatronSummaryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(317, 264);
            this.Controls.Add(this.label_comment);
            this.Controls.Add(this.dpTable1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PatronSummaryForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "读者摘要";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PatronSummaryForm_FormClosed);
            this.Load += new System.EventHandler(this.PatronSummaryForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.CommonControl.DpTable dpTable1;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_content;
        private System.Windows.Forms.Label label_comment;
        private System.Windows.Forms.Timer timer1;
    }
}