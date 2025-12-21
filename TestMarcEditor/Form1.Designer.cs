namespace TestMarcEditor
{
    partial class Form1
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.marcEditor1 = new DigitalPlatform.Marc.MarcEditor();
            this.marcControl1 = new LibraryStudio.Forms.MarcControl();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(16, 5, 0, 5);
            this.menuStrip1.Size = new System.Drawing.Size(2133, 72);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(120, 50);
            this.MenuItem_file.Text = "文件";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Location = new System.Drawing.Point(0, 72);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.toolStrip1.Size = new System.Drawing.Size(2133, 75);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Location = new System.Drawing.Point(0, 1178);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(3, 0, 37, 0);
            this.statusStrip1.Size = new System.Drawing.Size(2133, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(1339, 552);
            this.textBox1.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(260, 62);
            this.textBox1.TabIndex = 4;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(1413, 357);
            this.textBox2.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(260, 62);
            this.textBox2.TabIndex = 8;
            // 
            // marcEditor1
            // 
            this.marcEditor1.AutoScroll = true;
            this.marcEditor1.AutoScrollMinSize = new System.Drawing.Size(1099, 86);
            this.marcEditor1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.marcEditor1.ClientBoundsWidth = 0;
            this.marcEditor1.Content = "????????????????????????";
            this.marcEditor1.ContentBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor1.ContentTextColor = System.Drawing.SystemColors.WindowText;
            this.marcEditor1.CurrentImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.marcEditor1.FocusedField = null;
            this.marcEditor1.FocusedFieldIndex = 0;
            this.marcEditor1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.marcEditor1.HorzGridColor = System.Drawing.Color.LightGray;
            this.marcEditor1.IndicatorBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor1.IndicatorBackColorDisabled = System.Drawing.SystemColors.Control;
            this.marcEditor1.IndicatorTextColor = System.Drawing.Color.Green;
            this.marcEditor1.Lang = "zh";
            this.marcEditor1.Location = new System.Drawing.Point(0, 152);
            this.marcEditor1.Marc = "????????????????????????";
            this.marcEditor1.MarcDefDom = null;
            this.marcEditor1.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.marcEditor1.Name = "marcEditor1";
            this.marcEditor1.NameBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor1.NameCaptionBackColor = System.Drawing.SystemColors.Info;
            this.marcEditor1.NameCaptionTextColor = System.Drawing.SystemColors.InfoText;
            this.marcEditor1.NameTextColor = System.Drawing.Color.Blue;
            this.marcEditor1.ReadOnly = false;
            this.marcEditor1.SelectedBackColor = System.Drawing.SystemColors.Highlight;
            this.marcEditor1.SelectedTextColor = System.Drawing.SystemColors.HighlightText;
            this.marcEditor1.Size = new System.Drawing.Size(1099, 896);
            this.marcEditor1.TabIndex = 3;
            this.marcEditor1.UiState = "{\"FieldNameCaptionWidth\":100}";
            this.marcEditor1.VertGridColor = System.Drawing.Color.LightGray;
            this.marcEditor1.GetConfigFile += new DigitalPlatform.Marc.GetConfigFileEventHandle(this.marcEditor1_GetConfigFile);
            this.marcEditor1.GetConfigDom += new DigitalPlatform.Marc.GetConfigDomEventHandle(this.marcEditor1_GetConfigDom);
            // 
            // marcControl1
            // 
            this.marcControl1.AutoScroll = true;
            this.marcControl1.AutoScrollMinSize = new System.Drawing.Size(744, 55);
            this.marcControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.marcControl1.Changed = false;
            this.marcControl1.ClientBoundsWidth = 0;
            this.marcControl1.Content = "";
            this.marcControl1.Location = new System.Drawing.Point(1316, 741);
            this.marcControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.marcControl1.Name = "marcControl1";
            this.marcControl1.ReadOnly = false;
            this.marcControl1.Size = new System.Drawing.Size(744, 406);
            this.marcControl1.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(288F, 288F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(2133, 1200);
            this.Controls.Add(this.marcControl1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.marcEditor1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private DigitalPlatform.Marc.MarcEditor marcEditor1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private LibraryStudio.Forms.MarcControl marcControl1;
    }
}

