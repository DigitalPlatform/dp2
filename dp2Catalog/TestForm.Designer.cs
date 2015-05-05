namespace dp2Catalog
{
    partial class TestForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_W3cDtfControl = new System.Windows.Forms.TabPage();
            this.button_setValue = new System.Windows.Forms.Button();
            this.textBox_w3cDtfString = new System.Windows.Forms.TextBox();
            this.button_getValue = new System.Windows.Forms.Button();
            this.w3cDtfControl1 = new DigitalPlatform.CommonControl.W3cDtfControl();
            this.button_value = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage_W3cDtfControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_W3cDtfControl);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(472, 271);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_W3cDtfControl
            // 
            this.tabPage_W3cDtfControl.Controls.Add(this.button_setValue);
            this.tabPage_W3cDtfControl.Controls.Add(this.textBox_w3cDtfString);
            this.tabPage_W3cDtfControl.Controls.Add(this.button_getValue);
            this.tabPage_W3cDtfControl.Controls.Add(this.w3cDtfControl1);
            this.tabPage_W3cDtfControl.Location = new System.Drawing.Point(4, 24);
            this.tabPage_W3cDtfControl.Name = "tabPage_W3cDtfControl";
            this.tabPage_W3cDtfControl.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_W3cDtfControl.Size = new System.Drawing.Size(464, 243);
            this.tabPage_W3cDtfControl.TabIndex = 0;
            this.tabPage_W3cDtfControl.Text = "W3cDtfControl";
            this.tabPage_W3cDtfControl.UseVisualStyleBackColor = true;
            // 
            // button_setValue
            // 
            this.button_setValue.Location = new System.Drawing.Point(8, 128);
            this.button_setValue.Name = "button_setValue";
            this.button_setValue.Size = new System.Drawing.Size(184, 28);
            this.button_setValue.TabIndex = 3;
            this.button_setValue.Text = "set value (向上)";
            this.button_setValue.UseVisualStyleBackColor = true;
            this.button_setValue.Click += new System.EventHandler(this.button_setValue_Click);
            // 
            // textBox_w3cDtfString
            // 
            this.textBox_w3cDtfString.Location = new System.Drawing.Point(8, 172);
            this.textBox_w3cDtfString.Name = "textBox_w3cDtfString";
            this.textBox_w3cDtfString.Size = new System.Drawing.Size(426, 25);
            this.textBox_w3cDtfString.TabIndex = 2;
            // 
            // button_getValue
            // 
            this.button_getValue.Location = new System.Drawing.Point(290, 128);
            this.button_getValue.Name = "button_getValue";
            this.button_getValue.Size = new System.Drawing.Size(144, 28);
            this.button_getValue.TabIndex = 1;
            this.button_getValue.Text = "get value (向下)";
            this.button_getValue.UseVisualStyleBackColor = true;
            this.button_getValue.Click += new System.EventHandler(this.button_getValue_Click);
            // 
            // w3cDtfControl1
            // 
            this.w3cDtfControl1.Location = new System.Drawing.Point(7, 86);
            this.w3cDtfControl1.Name = "w3cDtfControl1";
            this.w3cDtfControl1.Size = new System.Drawing.Size(427, 24);
            this.w3cDtfControl1.TabIndex = 0;
            this.w3cDtfControl1.ValueString = "";
            // 
            // button_value
            // 
            this.button_value.Location = new System.Drawing.Point(290, 128);
            this.button_value.Name = "button_value";
            this.button_value.Size = new System.Drawing.Size(102, 28);
            this.button_value.TabIndex = 1;
            this.button_value.Text = "value...";
            this.button_value.UseVisualStyleBackColor = true;
            this.button_value.Click += new System.EventHandler(this.button_getValue_Click);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 271);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TestForm";
            this.ShowInTaskbar = false;
            this.Text = "TestForm";
            this.tabControl1.ResumeLayout(false);
            this.tabPage_W3cDtfControl.ResumeLayout(false);
            this.tabPage_W3cDtfControl.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_W3cDtfControl;
        private DigitalPlatform.CommonControl.W3cDtfControl w3cDtfControl1;
        private System.Windows.Forms.Button button_getValue;
        private System.Windows.Forms.TextBox textBox_w3cDtfString;
        private System.Windows.Forms.Button button_value;
        private System.Windows.Forms.Button button_setValue;
    }
}