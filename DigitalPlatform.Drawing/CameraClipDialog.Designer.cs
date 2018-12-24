namespace DigitalPlatform.Drawing
{
    partial class CameraClipDialog
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

            if (this.qrRecognitionControl1 != null)
                this.qrRecognitionControl1.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CameraClipDialog));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel_null = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton_cancel = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_getAndClose = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_shoot = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_clip_autoCorp = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_clip_output = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_ratate = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_copy = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_paste = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_selectAll = new System.Windows.Forms.ToolStripButton();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_preview = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabPage_clip = new System.Windows.Forms.TabPage();
            this.pictureBox_clip = new DigitalPlatform.Drawing.ClipControl();
            this.tabPage_result = new System.Windows.Forms.TabPage();
            this.pictureBox_result = new System.Windows.Forms.PictureBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_autoDetectEdge = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_preview.SuspendLayout();
            this.tabPage_clip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_clip)).BeginInit();
            this.tabPage_result.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_result)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Right;
            this.toolStrip1.Enabled = false;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel_null,
            this.toolStripButton_cancel,
            this.toolStripButton_getAndClose,
            this.toolStripButton_shoot,
            this.toolStripSeparator1,
            this.toolStripButton_clip_autoCorp,
            this.toolStripButton_clip_output,
            this.toolStripSeparator2,
            this.toolStripButton_ratate,
            this.toolStripButton_copy,
            this.toolStripButton_paste,
            this.toolStripButton_selectAll,
            this.toolStripSeparator3,
            this.toolStripButton_autoDetectEdge});
            this.toolStrip1.Location = new System.Drawing.Point(736, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip1.Size = new System.Drawing.Size(88, 460);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel_null
            // 
            this.toolStripLabel_null.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel_null.Name = "toolStripLabel_null";
            this.toolStripLabel_null.Size = new System.Drawing.Size(83, 24);
            this.toolStripLabel_null.Text = " ";
            // 
            // toolStripButton_cancel
            // 
            this.toolStripButton_cancel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_cancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_cancel.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.toolStripButton_cancel.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_cancel.Image")));
            this.toolStripButton_cancel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_cancel.Name = "toolStripButton_cancel";
            this.toolStripButton_cancel.Size = new System.Drawing.Size(83, 28);
            this.toolStripButton_cancel.Text = "取消";
            this.toolStripButton_cancel.Click += new System.EventHandler(this.toolStripButton_cancel_Click);
            // 
            // toolStripButton_getAndClose
            // 
            this.toolStripButton_getAndClose.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_getAndClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_getAndClose.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.toolStripButton_getAndClose.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_getAndClose.Image")));
            this.toolStripButton_getAndClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_getAndClose.Name = "toolStripButton_getAndClose";
            this.toolStripButton_getAndClose.Size = new System.Drawing.Size(83, 35);
            this.toolStripButton_getAndClose.Text = "确定";
            this.toolStripButton_getAndClose.ToolTipText = "获取图像，自动关闭对话框";
            this.toolStripButton_getAndClose.Click += new System.EventHandler(this.toolStripButton_getAndClose_Click);
            // 
            // toolStripButton_shoot
            // 
            this.toolStripButton_shoot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_shoot.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripButton_shoot.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_shoot.Image")));
            this.toolStripButton_shoot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_shoot.Name = "toolStripButton_shoot";
            this.toolStripButton_shoot.Size = new System.Drawing.Size(83, 35);
            this.toolStripButton_shoot.Text = "截图";
            this.toolStripButton_shoot.Click += new System.EventHandler(this.toolStripButton_shoot_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(83, 6);
            // 
            // toolStripButton_clip_autoCorp
            // 
            this.toolStripButton_clip_autoCorp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_clip_autoCorp.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clip_autoCorp.Image")));
            this.toolStripButton_clip_autoCorp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clip_autoCorp.Name = "toolStripButton_clip_autoCorp";
            this.toolStripButton_clip_autoCorp.Size = new System.Drawing.Size(83, 28);
            this.toolStripButton_clip_autoCorp.Text = "探边";
            this.toolStripButton_clip_autoCorp.Click += new System.EventHandler(this.toolStripButton_clip_autoCorp_Click);
            // 
            // toolStripButton_clip_output
            // 
            this.toolStripButton_clip_output.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_clip_output.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripButton_clip_output.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clip_output.Image")));
            this.toolStripButton_clip_output.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clip_output.Name = "toolStripButton_clip_output";
            this.toolStripButton_clip_output.Size = new System.Drawing.Size(83, 35);
            this.toolStripButton_clip_output.Text = "输出";
            this.toolStripButton_clip_output.Click += new System.EventHandler(this.toolStripButton_clip_output_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(83, 6);
            // 
            // toolStripButton_ratate
            // 
            this.toolStripButton_ratate.Enabled = false;
            this.toolStripButton_ratate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_ratate.Image")));
            this.toolStripButton_ratate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_ratate.Name = "toolStripButton_ratate";
            this.toolStripButton_ratate.Size = new System.Drawing.Size(83, 28);
            this.toolStripButton_ratate.Text = "旋转";
            this.toolStripButton_ratate.ToolTipText = "顺指针旋转 90 度 (冻结时可用)";
            this.toolStripButton_ratate.Click += new System.EventHandler(this.toolStripButton_ratate_Click);
            // 
            // toolStripButton_copy
            // 
            this.toolStripButton_copy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_copy.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_copy.Image")));
            this.toolStripButton_copy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_copy.Name = "toolStripButton_copy";
            this.toolStripButton_copy.Size = new System.Drawing.Size(83, 28);
            this.toolStripButton_copy.Text = "复制";
            this.toolStripButton_copy.Click += new System.EventHandler(this.toolStripButton_copy_Click);
            // 
            // toolStripButton_paste
            // 
            this.toolStripButton_paste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_paste.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_paste.Image")));
            this.toolStripButton_paste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_paste.Name = "toolStripButton_paste";
            this.toolStripButton_paste.Size = new System.Drawing.Size(83, 28);
            this.toolStripButton_paste.Text = "粘贴";
            this.toolStripButton_paste.Click += new System.EventHandler(this.toolStripButton_paste_Click);
            // 
            // toolStripButton_selectAll
            // 
            this.toolStripButton_selectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_selectAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_selectAll.Image")));
            this.toolStripButton_selectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_selectAll.Name = "toolStripButton_selectAll";
            this.toolStripButton_selectAll.Size = new System.Drawing.Size(83, 28);
            this.toolStripButton_selectAll.Text = "全选";
            this.toolStripButton_selectAll.Click += new System.EventHandler(this.toolStripButton_selectAll_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_preview);
            this.tabControl_main.Controls.Add(this.tabPage_clip);
            this.tabControl_main.Controls.Add(this.tabPage_result);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(736, 460);
            this.tabControl_main.TabIndex = 5;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_preview
            // 
            this.tabPage_preview.Controls.Add(this.panel1);
            this.tabPage_preview.Location = new System.Drawing.Point(4, 28);
            this.tabPage_preview.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_preview.Name = "tabPage_preview";
            this.tabPage_preview.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_preview.Size = new System.Drawing.Size(728, 428);
            this.tabPage_preview.TabIndex = 0;
            this.tabPage_preview.Text = "预览";
            this.tabPage_preview.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(4, 4);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(720, 420);
            this.panel1.TabIndex = 1;
            this.panel1.SizeChanged += new System.EventHandler(this.panel1_SizeChanged);
            // 
            // tabPage_clip
            // 
            this.tabPage_clip.Controls.Add(this.pictureBox_clip);
            this.tabPage_clip.Location = new System.Drawing.Point(4, 28);
            this.tabPage_clip.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_clip.Name = "tabPage_clip";
            this.tabPage_clip.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_clip.Size = new System.Drawing.Size(736, 428);
            this.tabPage_clip.TabIndex = 1;
            this.tabPage_clip.Text = "裁切";
            this.tabPage_clip.UseVisualStyleBackColor = true;
            // 
            // pictureBox_clip
            // 
            this.pictureBox_clip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_clip.Location = new System.Drawing.Point(4, 4);
            this.pictureBox_clip.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox_clip.Name = "pictureBox_clip";
            this.pictureBox_clip.Size = new System.Drawing.Size(728, 420);
            this.pictureBox_clip.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_clip.TabIndex = 1;
            this.pictureBox_clip.TabStop = false;
            // 
            // tabPage_result
            // 
            this.tabPage_result.Controls.Add(this.pictureBox_result);
            this.tabPage_result.Location = new System.Drawing.Point(4, 28);
            this.tabPage_result.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_result.Name = "tabPage_result";
            this.tabPage_result.Size = new System.Drawing.Size(736, 428);
            this.tabPage_result.TabIndex = 2;
            this.tabPage_result.Text = "结果";
            this.tabPage_result.UseVisualStyleBackColor = true;
            // 
            // pictureBox_result
            // 
            this.pictureBox_result.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_result.Location = new System.Drawing.Point(0, 0);
            this.pictureBox_result.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox_result.Name = "pictureBox_result";
            this.pictureBox_result.Size = new System.Drawing.Size(736, 428);
            this.pictureBox_result.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_result.TabIndex = 1;
            this.pictureBox_result.TabStop = false;
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(83, 6);
            // 
            // toolStripButton_autoDetectEdge
            // 
            this.toolStripButton_autoDetectEdge.Checked = true;
            this.toolStripButton_autoDetectEdge.CheckOnClick = true;
            this.toolStripButton_autoDetectEdge.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton_autoDetectEdge.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_autoDetectEdge.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_autoDetectEdge.Image")));
            this.toolStripButton_autoDetectEdge.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_autoDetectEdge.Name = "toolStripButton_autoDetectEdge";
            this.toolStripButton_autoDetectEdge.Size = new System.Drawing.Size(83, 28);
            this.toolStripButton_autoDetectEdge.Text = "自动探边";
            // 
            // CameraClipDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 460);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.toolStrip1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "CameraClipDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "从摄像头获取图像";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CameraClipDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CameraClipDialog_FormClosed);
            this.Load += new System.EventHandler(this.CameraClipDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_preview.ResumeLayout(false);
            this.tabPage_clip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_clip)).EndInit();
            this.tabPage_result.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_result)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_getAndClose;
        private System.Windows.Forms.ToolStripButton toolStripButton_shoot;
        private System.Windows.Forms.ToolStripButton toolStripButton_ratate;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_preview;
        private System.Windows.Forms.TabPage tabPage_clip;
        private System.Windows.Forms.TabPage tabPage_result;
        private System.Windows.Forms.Panel panel1;
        private ClipControl pictureBox_clip;
        private System.Windows.Forms.ToolStripButton toolStripButton_clip_output;
        private System.Windows.Forms.ToolStripButton toolStripButton_clip_autoCorp;
        private System.Windows.Forms.PictureBox pictureBox_result;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_copy;
        private System.Windows.Forms.ToolStripButton toolStripButton_paste;
        private System.Windows.Forms.ToolStripButton toolStripButton_cancel;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_null;
        private System.Windows.Forms.ToolStripButton toolStripButton_selectAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton_autoDetectEdge;
    }
}