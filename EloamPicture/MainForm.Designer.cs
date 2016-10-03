namespace EloamPicture
{
    partial class MainForm
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_message = new System.Windows.Forms.ToolStripStatusLabel();
            this.selectMode = new System.Windows.Forms.ComboBox();
            this.selectDevice = new System.Windows.Forms.ComboBox();
            this.selectResolution = new System.Windows.Forms.ComboBox();
            this.pictureSavePath = new System.Windows.Forms.TextBox();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_preview = new System.Windows.Forms.TabPage();
            this.toolStrip_preview = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_preview_start = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_preview_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_preview_shoot = new System.Windows.Forms.ToolStripButton();
            this.eloamView = new AxeloamComLib.AxEloamView();
            this.tabPage_setting = new System.Windows.Forms.TabPage();
            this.rectify = new System.Windows.Forms.CheckBox();
            this.removeGround = new System.Windows.Forms.CheckBox();
            this.tabPage_clip = new System.Windows.Forms.TabPage();
            this.toolStrip_clip = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_clip_shoot = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_clip_autoCorp = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_clip_output = new System.Windows.Forms.ToolStripButton();
            this.tabPage_result = new System.Windows.Forms.TabPage();
            this.pictureBox_result = new System.Windows.Forms.PictureBox();
            this.toolStrip_result = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_clip_Rotate = new System.Windows.Forms.ToolStripButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button_setting_openOutputFolder = new System.Windows.Forms.Button();
            this.pictureBox_clip = new EloamPicture.ClipControl();
            this.statusStrip1.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_preview.SuspendLayout();
            this.toolStrip_preview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eloamView)).BeginInit();
            this.tabPage_setting.SuspendLayout();
            this.tabPage_clip.SuspendLayout();
            this.toolStrip_clip.SuspendLayout();
            this.tabPage_result.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_result)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_clip)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(566, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(566, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_message});
            this.statusStrip1.Location = new System.Drawing.Point(0, 343);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(566, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_message
            // 
            this.toolStripStatusLabel_message.Name = "toolStripStatusLabel_message";
            this.toolStripStatusLabel_message.Size = new System.Drawing.Size(0, 17);
            // 
            // selectMode
            // 
            this.selectMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectMode.FormattingEnabled = true;
            this.selectMode.Location = new System.Drawing.Point(79, 50);
            this.selectMode.Name = "selectMode";
            this.selectMode.Size = new System.Drawing.Size(179, 20);
            this.selectMode.TabIndex = 10;
            this.selectMode.SelectedIndexChanged += new System.EventHandler(this.selectMode_SelectedIndexChanged);
            // 
            // selectDevice
            // 
            this.selectDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectDevice.FormattingEnabled = true;
            this.selectDevice.Location = new System.Drawing.Point(79, 24);
            this.selectDevice.Name = "selectDevice";
            this.selectDevice.Size = new System.Drawing.Size(179, 20);
            this.selectDevice.TabIndex = 10;
            this.selectDevice.SelectedIndexChanged += new System.EventHandler(this.selectDevice_SelectedIndexChanged);
            // 
            // selectResolution
            // 
            this.selectResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectResolution.FormattingEnabled = true;
            this.selectResolution.Location = new System.Drawing.Point(79, 76);
            this.selectResolution.Name = "selectResolution";
            this.selectResolution.Size = new System.Drawing.Size(179, 20);
            this.selectResolution.TabIndex = 10;
            // 
            // pictureSavePath
            // 
            this.pictureSavePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureSavePath.Location = new System.Drawing.Point(79, 126);
            this.pictureSavePath.Name = "pictureSavePath";
            this.pictureSavePath.ReadOnly = true;
            this.pictureSavePath.Size = new System.Drawing.Size(473, 21);
            this.pictureSavePath.TabIndex = 22;
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_setting);
            this.tabControl_main.Controls.Add(this.tabPage_preview);
            this.tabControl_main.Controls.Add(this.tabPage_clip);
            this.tabControl_main.Controls.Add(this.tabPage_result);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 49);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(566, 294);
            this.tabControl_main.TabIndex = 2;
            // 
            // tabPage_preview
            // 
            this.tabPage_preview.Controls.Add(this.toolStrip_preview);
            this.tabPage_preview.Controls.Add(this.eloamView);
            this.tabPage_preview.Location = new System.Drawing.Point(4, 22);
            this.tabPage_preview.Name = "tabPage_preview";
            this.tabPage_preview.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_preview.Size = new System.Drawing.Size(558, 268);
            this.tabPage_preview.TabIndex = 0;
            this.tabPage_preview.Text = "预览";
            this.tabPage_preview.UseVisualStyleBackColor = true;
            // 
            // toolStrip_preview
            // 
            this.toolStrip_preview.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolStrip_preview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_preview_start,
            this.toolStripButton_preview_stop,
            this.toolStripSeparator1,
            this.toolStripButton_preview_shoot});
            this.toolStrip_preview.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStrip_preview.Location = new System.Drawing.Point(3, 3);
            this.toolStrip_preview.Name = "toolStrip_preview";
            this.toolStrip_preview.Size = new System.Drawing.Size(47, 262);
            this.toolStrip_preview.TabIndex = 0;
            this.toolStrip_preview.Text = "toolStrip2";
            // 
            // toolStripButton_preview_start
            // 
            this.toolStripButton_preview_start.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_preview_start.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_preview_start.Image")));
            this.toolStripButton_preview_start.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_preview_start.Name = "toolStripButton_preview_start";
            this.toolStripButton_preview_start.Size = new System.Drawing.Size(44, 21);
            this.toolStripButton_preview_start.Text = "启动";
            this.toolStripButton_preview_start.Click += new System.EventHandler(this.toolStripButton_preview_start_Click);
            // 
            // toolStripButton_preview_stop
            // 
            this.toolStripButton_preview_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_preview_stop.Enabled = false;
            this.toolStripButton_preview_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_preview_stop.Image")));
            this.toolStripButton_preview_stop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_preview_stop.Name = "toolStripButton_preview_stop";
            this.toolStripButton_preview_stop.Size = new System.Drawing.Size(44, 21);
            this.toolStripButton_preview_stop.Text = "停止";
            this.toolStripButton_preview_stop.Click += new System.EventHandler(this.toolStripButton_preview_stop_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(44, 6);
            // 
            // toolStripButton_preview_shoot
            // 
            this.toolStripButton_preview_shoot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_preview_shoot.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripButton_preview_shoot.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_preview_shoot.Image")));
            this.toolStripButton_preview_shoot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_preview_shoot.Name = "toolStripButton_preview_shoot";
            this.toolStripButton_preview_shoot.Size = new System.Drawing.Size(44, 26);
            this.toolStripButton_preview_shoot.Text = "取图";
            this.toolStripButton_preview_shoot.Click += new System.EventHandler(this.toolStripButton_preview_shoot_Click);
            // 
            // eloamView
            // 
            this.eloamView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.eloamView.Enabled = true;
            this.eloamView.Location = new System.Drawing.Point(32, 3);
            this.eloamView.Name = "eloamView";
            this.eloamView.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("eloamView.OcxState")));
            this.eloamView.Size = new System.Drawing.Size(523, 262);
            this.eloamView.TabIndex = 1;
            // 
            // tabPage_setting
            // 
            this.tabPage_setting.Controls.Add(this.button_setting_openOutputFolder);
            this.tabPage_setting.Controls.Add(this.label4);
            this.tabPage_setting.Controls.Add(this.pictureSavePath);
            this.tabPage_setting.Controls.Add(this.label3);
            this.tabPage_setting.Controls.Add(this.selectResolution);
            this.tabPage_setting.Controls.Add(this.label2);
            this.tabPage_setting.Controls.Add(this.selectMode);
            this.tabPage_setting.Controls.Add(this.label1);
            this.tabPage_setting.Controls.Add(this.selectDevice);
            this.tabPage_setting.Controls.Add(this.rectify);
            this.tabPage_setting.Controls.Add(this.removeGround);
            this.tabPage_setting.Location = new System.Drawing.Point(4, 22);
            this.tabPage_setting.Name = "tabPage_setting";
            this.tabPage_setting.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_setting.Size = new System.Drawing.Size(558, 268);
            this.tabPage_setting.TabIndex = 1;
            this.tabPage_setting.Text = "设置";
            this.tabPage_setting.UseVisualStyleBackColor = true;
            // 
            // rectify
            // 
            this.rectify.AutoSize = true;
            this.rectify.Location = new System.Drawing.Point(373, 26);
            this.rectify.Name = "rectify";
            this.rectify.Size = new System.Drawing.Size(72, 16);
            this.rectify.TabIndex = 30;
            this.rectify.Text = "纠偏裁边";
            this.rectify.UseVisualStyleBackColor = true;
            this.rectify.Visible = false;
            this.rectify.CheckedChanged += new System.EventHandler(this.rectify_CheckedChanged);
            // 
            // removeGround
            // 
            this.removeGround.AutoSize = true;
            this.removeGround.Location = new System.Drawing.Point(373, 48);
            this.removeGround.Name = "removeGround";
            this.removeGround.Size = new System.Drawing.Size(60, 16);
            this.removeGround.TabIndex = 31;
            this.removeGround.Text = "去底色";
            this.removeGround.UseVisualStyleBackColor = true;
            this.removeGround.Visible = false;
            // 
            // tabPage_clip
            // 
            this.tabPage_clip.Controls.Add(this.toolStrip_clip);
            this.tabPage_clip.Controls.Add(this.pictureBox_clip);
            this.tabPage_clip.Location = new System.Drawing.Point(4, 22);
            this.tabPage_clip.Name = "tabPage_clip";
            this.tabPage_clip.Size = new System.Drawing.Size(558, 268);
            this.tabPage_clip.TabIndex = 2;
            this.tabPage_clip.Text = "裁切";
            this.tabPage_clip.UseVisualStyleBackColor = true;
            // 
            // toolStrip_clip
            // 
            this.toolStrip_clip.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolStrip_clip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_clip_shoot,
            this.toolStripButton_clip_autoCorp,
            this.toolStripButton_clip_output,
            this.toolStripSeparator2,
            this.toolStripButton_clip_Rotate});
            this.toolStrip_clip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_clip.Name = "toolStrip_clip";
            this.toolStrip_clip.Size = new System.Drawing.Size(47, 268);
            this.toolStrip_clip.TabIndex = 1;
            this.toolStrip_clip.Text = "toolStrip2";
            // 
            // toolStripButton_clip_shoot
            // 
            this.toolStripButton_clip_shoot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_clip_shoot.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clip_shoot.Image")));
            this.toolStripButton_clip_shoot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clip_shoot.Name = "toolStripButton_clip_shoot";
            this.toolStripButton_clip_shoot.Size = new System.Drawing.Size(44, 21);
            this.toolStripButton_clip_shoot.Text = "取图";
            this.toolStripButton_clip_shoot.Click += new System.EventHandler(this.toolStripButton_clip_shoot_Click);
            // 
            // toolStripButton_clip_autoCorp
            // 
            this.toolStripButton_clip_autoCorp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_clip_autoCorp.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clip_autoCorp.Image")));
            this.toolStripButton_clip_autoCorp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clip_autoCorp.Name = "toolStripButton_clip_autoCorp";
            this.toolStripButton_clip_autoCorp.Size = new System.Drawing.Size(44, 21);
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
            this.toolStripButton_clip_output.Size = new System.Drawing.Size(44, 26);
            this.toolStripButton_clip_output.Text = "输出";
            this.toolStripButton_clip_output.Click += new System.EventHandler(this.toolStripButton_clip_output_Click);
            // 
            // tabPage_result
            // 
            this.tabPage_result.Controls.Add(this.toolStrip_result);
            this.tabPage_result.Controls.Add(this.pictureBox_result);
            this.tabPage_result.Location = new System.Drawing.Point(4, 22);
            this.tabPage_result.Name = "tabPage_result";
            this.tabPage_result.Size = new System.Drawing.Size(558, 268);
            this.tabPage_result.TabIndex = 3;
            this.tabPage_result.Text = "结果";
            this.tabPage_result.UseVisualStyleBackColor = true;
            // 
            // pictureBox_result
            // 
            this.pictureBox_result.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_result.Location = new System.Drawing.Point(29, 3);
            this.pictureBox_result.Name = "pictureBox_result";
            this.pictureBox_result.Size = new System.Drawing.Size(529, 266);
            this.pictureBox_result.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_result.TabIndex = 0;
            this.pictureBox_result.TabStop = false;
            // 
            // toolStrip_result
            // 
            this.toolStrip_result.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolStrip_result.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_result.Name = "toolStrip_result";
            this.toolStrip_result.Size = new System.Drawing.Size(26, 268);
            this.toolStrip_result.TabIndex = 1;
            this.toolStrip_result.Text = "toolStrip2";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(44, 6);
            // 
            // toolStripButton_clip_Rotate
            // 
            this.toolStripButton_clip_Rotate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_clip_Rotate.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clip_Rotate.Image")));
            this.toolStripButton_clip_Rotate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clip_Rotate.Name = "toolStripButton_clip_Rotate";
            this.toolStripButton_clip_Rotate.Size = new System.Drawing.Size(44, 20);
            this.toolStripButton_clip_Rotate.Text = "顺时针旋转 90 度";
            this.toolStripButton_clip_Rotate.Click += new System.EventHandler(this.toolStripButton_clip_Rotate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 32;
            this.label1.Text = "设备名:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 33;
            this.label2.Text = "视频模式:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 34;
            this.label3.Text = "分辨率:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 35;
            this.label4.Text = "输出路径:";
            // 
            // button_setting_openOutputFolder
            // 
            this.button_setting_openOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_setting_openOutputFolder.Location = new System.Drawing.Point(415, 153);
            this.button_setting_openOutputFolder.Name = "button_setting_openOutputFolder";
            this.button_setting_openOutputFolder.Size = new System.Drawing.Size(137, 23);
            this.button_setting_openOutputFolder.TabIndex = 36;
            this.button_setting_openOutputFolder.Text = "打开文件夹 ...";
            this.button_setting_openOutputFolder.UseVisualStyleBackColor = true;
            this.button_setting_openOutputFolder.Click += new System.EventHandler(this.button_setting_openOutputFolder_Click);
            // 
            // pictureBox_clip
            // 
            this.pictureBox_clip.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_clip.Location = new System.Drawing.Point(50, 0);
            this.pictureBox_clip.Name = "pictureBox_clip";
            this.pictureBox_clip.Size = new System.Drawing.Size(508, 268);
            this.pictureBox_clip.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_clip.TabIndex = 0;
            this.pictureBox_clip.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 365);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "EloamPicture -- Eloam图像扫描";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_preview.ResumeLayout(false);
            this.tabPage_preview.PerformLayout();
            this.toolStrip_preview.ResumeLayout(false);
            this.toolStrip_preview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eloamView)).EndInit();
            this.tabPage_setting.ResumeLayout(false);
            this.tabPage_setting.PerformLayout();
            this.tabPage_clip.ResumeLayout(false);
            this.tabPage_clip.PerformLayout();
            this.toolStrip_clip.ResumeLayout(false);
            this.toolStrip_clip.PerformLayout();
            this.tabPage_result.ResumeLayout(false);
            this.tabPage_result.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_result)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_clip)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_message;
        private System.Windows.Forms.ComboBox selectMode;
        private System.Windows.Forms.ComboBox selectDevice;
        private System.Windows.Forms.ComboBox selectResolution;
        private AxeloamComLib.AxEloamView eloamView;
        private System.Windows.Forms.TextBox pictureSavePath;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_preview;
        private System.Windows.Forms.TabPage tabPage_setting;
        private System.Windows.Forms.CheckBox rectify;
        private System.Windows.Forms.CheckBox removeGround;
        private System.Windows.Forms.TabPage tabPage_clip;
        private ClipControl pictureBox_clip;
        private System.Windows.Forms.ToolStrip toolStrip_preview;
        private System.Windows.Forms.ToolStripButton toolStripButton_preview_start;
        private System.Windows.Forms.ToolStripButton toolStripButton_preview_stop;
        private System.Windows.Forms.ToolStrip toolStrip_clip;
        private System.Windows.Forms.ToolStripButton toolStripButton_clip_shoot;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_preview_shoot;
        private System.Windows.Forms.ToolStripButton toolStripButton_clip_autoCorp;
        private System.Windows.Forms.ToolStripButton toolStripButton_clip_output;
        private System.Windows.Forms.TabPage tabPage_result;
        private System.Windows.Forms.PictureBox pictureBox_result;
        private System.Windows.Forms.ToolStrip toolStrip_result;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_clip_Rotate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_setting_openOutputFolder;
    }
}

