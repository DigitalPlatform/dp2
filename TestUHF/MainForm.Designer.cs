namespace TestUHF
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.button_openReader = new System.Windows.Forms.Button();
            this.button_closeReader = new System.Windows.Forms.Button();
            this.button_inventory = new System.Windows.Forms.Button();
            this.textBox_result = new System.Windows.Forms.TextBox();
            this.button_readerData = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(893, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(893, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Location = new System.Drawing.Point(0, 523);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(893, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // button_openReader
            // 
            this.button_openReader.Location = new System.Drawing.Point(47, 69);
            this.button_openReader.Name = "button_openReader";
            this.button_openReader.Size = new System.Drawing.Size(177, 38);
            this.button_openReader.TabIndex = 3;
            this.button_openReader.Text = "Open Reader";
            this.button_openReader.UseVisualStyleBackColor = true;
            this.button_openReader.Click += new System.EventHandler(this.button_openReader_Click);
            // 
            // button_closeReader
            // 
            this.button_closeReader.Enabled = false;
            this.button_closeReader.Location = new System.Drawing.Point(239, 69);
            this.button_closeReader.Name = "button_closeReader";
            this.button_closeReader.Size = new System.Drawing.Size(177, 38);
            this.button_closeReader.TabIndex = 4;
            this.button_closeReader.Text = "Close Reader";
            this.button_closeReader.UseVisualStyleBackColor = true;
            this.button_closeReader.Click += new System.EventHandler(this.button_closeReader_Click);
            // 
            // button_inventory
            // 
            this.button_inventory.Location = new System.Drawing.Point(422, 69);
            this.button_inventory.Name = "button_inventory";
            this.button_inventory.Size = new System.Drawing.Size(177, 38);
            this.button_inventory.TabIndex = 5;
            this.button_inventory.Text = "Inventory";
            this.button_inventory.UseVisualStyleBackColor = true;
            this.button_inventory.Click += new System.EventHandler(this.button_inventory_Click);
            // 
            // textBox_result
            // 
            this.textBox_result.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_result.Location = new System.Drawing.Point(13, 124);
            this.textBox_result.Multiline = true;
            this.textBox_result.Name = "textBox_result";
            this.textBox_result.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_result.Size = new System.Drawing.Size(868, 377);
            this.textBox_result.TabIndex = 6;
            // 
            // button_readerData
            // 
            this.button_readerData.Location = new System.Drawing.Point(605, 69);
            this.button_readerData.Name = "button_readerData";
            this.button_readerData.Size = new System.Drawing.Size(177, 38);
            this.button_readerData.TabIndex = 7;
            this.button_readerData.Text = "Read";
            this.button_readerData.UseVisualStyleBackColor = true;
            this.button_readerData.Click += new System.EventHandler(this.button_readerData_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(893, 545);
            this.Controls.Add(this.button_readerData);
            this.Controls.Add(this.textBox_result);
            this.Controls.Add(this.button_inventory);
            this.Controls.Add(this.button_closeReader);
            this.Controls.Add(this.button_openReader);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Button button_openReader;
        private System.Windows.Forms.Button button_closeReader;
        private System.Windows.Forms.Button button_inventory;
        private System.Windows.Forms.TextBox textBox_result;
        private System.Windows.Forms.Button button_readerData;
    }
}

