#if NO
namespace dp2Circulation
{
    partial class BiblioRegisterControl
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

            this.ClearList();

            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BiblioRegisterControl));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.easyMarcControl1 = new DigitalPlatform.EasyMarc.EasyMarcControl();
            this.label_summary = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_selectBiblio = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_viewDetail = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_viewSummary = new System.Windows.Forms.ToolStripButton();
            this.dpTable_browseLines = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn_no = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_recPath = new DigitalPlatform.CommonControl.DpColumn();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.splitter_line = new DigitalPlatform.EasyMarc.TransparentSplitter();
            this.textBox_biblioBarcode = new System.Windows.Forms.TextBox();
            this.imageList_progress = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 341F));
            this.tableLayoutPanel1.Controls.Add(this.easyMarcControl1, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label_summary, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.dpTable_browseLines, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.splitter_line, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBox_biblioBarcode, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(573, 141);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // easyMarcControl1
            // 
            this.easyMarcControl1.AutoScroll = true;
            this.easyMarcControl1.CaptionWidth = 116;
            this.easyMarcControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.easyMarcControl1.HideIndicator = true;
            this.easyMarcControl1.IncludeNumber = false;
            this.easyMarcControl1.Location = new System.Drawing.Point(144, 23);
            this.easyMarcControl1.MarcDefDom = null;
            this.easyMarcControl1.Name = "easyMarcControl1";
            this.easyMarcControl1.Size = new System.Drawing.Size(79, 94);
            this.easyMarcControl1.TabIndex = 2;
            this.easyMarcControl1.Visible = false;
            this.easyMarcControl1.TextChanged += new System.EventHandler(this.easyMarcControl1_TextChanged);
            this.easyMarcControl1.GetConfigDom += new DigitalPlatform.Marc.GetConfigDomEventHandle(this.easyMarcControl1_GetConfigDom);
            // 
            // label_summary
            // 
            this.label_summary.AutoSize = true;
            this.label_summary.BackColor = System.Drawing.Color.Transparent;
            this.label_summary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_summary.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.label_summary.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label_summary.Location = new System.Drawing.Point(229, 20);
            this.label_summary.Name = "label_summary";
            this.label_summary.Size = new System.Drawing.Size(1, 100);
            this.label_summary.TabIndex = 3;
            this.label_summary.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBox1.Location = new System.Drawing.Point(3, 23);
            this.pictureBox1.MaximumSize = new System.Drawing.Size(200, 300);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(111, 64);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_selectBiblio,
            this.toolStripButton_viewDetail,
            this.toolStripButton_viewSummary});
            this.toolStrip1.Location = new System.Drawing.Point(117, 20);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(24, 100);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_selectBiblio
            // 
            this.toolStripButton_selectBiblio.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_selectBiblio.Enabled = false;
            this.toolStripButton_selectBiblio.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.toolStripButton_selectBiblio.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_selectBiblio.Image")));
            this.toolStripButton_selectBiblio.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton_selectBiblio.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_selectBiblio.Name = "toolStripButton_selectBiblio";
            this.toolStripButton_selectBiblio.Size = new System.Drawing.Size(21, 4);
            this.toolStripButton_selectBiblio.ToolTipText = "选择书目";
            this.toolStripButton_selectBiblio.Click += new System.EventHandler(this.toolStripButton_selectBiblio_Click);
            // 
            // toolStripButton_viewDetail
            // 
            this.toolStripButton_viewDetail.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_viewDetail.Enabled = false;
            this.toolStripButton_viewDetail.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_viewDetail.Image")));
            this.toolStripButton_viewDetail.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButton_viewDetail.Name = "toolStripButton_viewDetail";
            this.toolStripButton_viewDetail.Size = new System.Drawing.Size(21, 20);
            this.toolStripButton_viewDetail.Text = "书目";
            this.toolStripButton_viewDetail.Click += new System.EventHandler(this.toolStripButton_viewDetail_Click);
            // 
            // toolStripButton_viewSummary
            // 
            this.toolStripButton_viewSummary.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_viewSummary.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_viewSummary.Image")));
            this.toolStripButton_viewSummary.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButton_viewSummary.Name = "toolStripButton_viewSummary";
            this.toolStripButton_viewSummary.Size = new System.Drawing.Size(21, 20);
            this.toolStripButton_viewSummary.Text = "摘要";
            this.toolStripButton_viewSummary.Click += new System.EventHandler(this.toolStripButton_viewSummary_Click);
            // 
            // dpTable_browseLines
            // 
            this.dpTable_browseLines.AutoDocCenter = true;
            this.dpTable_browseLines.BackColor = System.Drawing.SystemColors.Window;
            this.dpTable_browseLines.Columns.Add(this.dpColumn_no);
            this.dpTable_browseLines.Columns.Add(this.dpColumn_recPath);
            this.dpTable_browseLines.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.dpTable_browseLines.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.dpTable_browseLines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dpTable_browseLines.DocumentBorderColor = System.Drawing.SystemColors.Window;
            this.dpTable_browseLines.DocumentOrgX = ((long)(0));
            this.dpTable_browseLines.DocumentOrgY = ((long)(0));
            this.dpTable_browseLines.DocumentShadowColor = System.Drawing.SystemColors.Window;
            this.dpTable_browseLines.FocusedItem = null;
            this.dpTable_browseLines.FullRowSelect = true;
            this.dpTable_browseLines.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable_browseLines.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable_browseLines.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable_browseLines.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable_browseLines.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable_browseLines.Location = new System.Drawing.Point(235, 23);
            this.dpTable_browseLines.Name = "dpTable_browseLines";
            this.dpTable_browseLines.Size = new System.Drawing.Size(335, 94);
            this.dpTable_browseLines.TabIndex = 6;
            this.dpTable_browseLines.Text = "dpTable1";
            this.dpTable_browseLines.SelectionChanged += new System.EventHandler(this.dpTable_browseLines_SelectionChanged);
            this.dpTable_browseLines.DoubleClick += new System.EventHandler(this.dpTable_browseLines_DoubleClick);
            // 
            // dpColumn_no
            // 
            this.dpColumn_no.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_no.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_no.Font = null;
            this.dpColumn_no.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_no.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_no.Text = "序号";
            this.dpColumn_no.Width = 50;
            // 
            // dpColumn_recPath
            // 
            this.dpColumn_recPath.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_recPath.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_recPath.Font = null;
            this.dpColumn_recPath.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_recPath.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_recPath.Text = "书目记录路径";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 131);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(111, 0);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // splitter_line
            // 
            this.splitter_line.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter_line.Location = new System.Drawing.Point(0, 120);
            this.splitter_line.Margin = new System.Windows.Forms.Padding(0);
            this.splitter_line.Name = "splitter_line";
            this.splitter_line.Size = new System.Drawing.Size(117, 8);
            this.splitter_line.TabIndex = 7;
            this.splitter_line.TabStop = false;
            this.splitter_line.MouseDown += new System.Windows.Forms.MouseEventHandler(this.splitter_line_MouseDown);
            this.splitter_line.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitter_line_MouseUp);
            // 
            // textBox_biblioBarcode
            // 
            this.textBox_biblioBarcode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_biblioBarcode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_biblioBarcode.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.textBox_biblioBarcode.Location = new System.Drawing.Point(3, 3);
            this.textBox_biblioBarcode.Name = "textBox_biblioBarcode";
            this.textBox_biblioBarcode.ReadOnly = true;
            this.textBox_biblioBarcode.Size = new System.Drawing.Size(111, 22);
            this.textBox_biblioBarcode.TabIndex = 8;
            // 
            // imageList_progress
            // 
            this.imageList_progress.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_progress.ImageStream")));
            this.imageList_progress.TransparentColor = System.Drawing.Color.White;
            this.imageList_progress.Images.SetKeyName(0, "process_32.png");
            this.imageList_progress.Images.SetKeyName(1, "action_success_24.png");
            this.imageList_progress.Images.SetKeyName(2, "dialog_error_24.png");
            this.imageList_progress.Images.SetKeyName(3, "progress_information.bmp");
            this.imageList_progress.Images.SetKeyName(4, "circle_24.png");
            // 
            // BiblioRegisterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanel1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Name = "BiblioRegisterControl";
            this.Size = new System.Drawing.Size(573, 141);
            this.SizeChanged += new System.EventHandler(this.BiblioRegisterControl_SizeChanged);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private DigitalPlatform.EasyMarc.EasyMarcControl easyMarcControl1;
        private System.Windows.Forms.Label label_summary;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_selectBiblio;
        private DigitalPlatform.CommonControl.DpTable dpTable_browseLines;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_recPath;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_no;
        private System.Windows.Forms.ToolStripButton toolStripButton_viewDetail;
        private System.Windows.Forms.ToolStripButton toolStripButton_viewSummary;
        private DigitalPlatform.EasyMarc.TransparentSplitter splitter_line;
        private System.Windows.Forms.TextBox textBox_biblioBarcode;
        private System.Windows.Forms.ImageList imageList_progress;
    }
}
#endif
