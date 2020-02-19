namespace dp2ManageCenter
{
    partial class ConfigForm
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
            this.button_OK = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_general = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown_operlogChannelMax = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_backupChannelMax = new System.Windows.Forms.NumericUpDown();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_general.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_operlogChannelMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_backupChannelMax)).BeginInit();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(598, 404);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(92, 34);
            this.button_OK.TabIndex = 0;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(696, 404);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(92, 34);
            this.button_cancel.TabIndex = 1;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_general);
            this.tabControl_main.Controls.Add(this.tabPage2);
            this.tabControl_main.Location = new System.Drawing.Point(13, 13);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(775, 385);
            this.tabControl_main.TabIndex = 2;
            // 
            // tabPage_general
            // 
            this.tabPage_general.Controls.Add(this.label4);
            this.tabPage_general.Controls.Add(this.label3);
            this.tabPage_general.Controls.Add(this.label2);
            this.tabPage_general.Controls.Add(this.label1);
            this.tabPage_general.Controls.Add(this.numericUpDown_operlogChannelMax);
            this.tabPage_general.Controls.Add(this.numericUpDown_backupChannelMax);
            this.tabPage_general.Location = new System.Drawing.Point(4, 28);
            this.tabPage_general.Name = "tabPage_general";
            this.tabPage_general.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_general.Size = new System.Drawing.Size(767, 353);
            this.tabPage_general.TabIndex = 0;
            this.tabPage_general.Text = "一般特性";
            this.tabPage_general.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(197, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "日备份并发通道最大数:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(197, 18);
            this.label1.TabIndex = 2;
            this.label1.Text = "大备份并发通道最大数:";
            // 
            // numericUpDown_operlogChannelMax
            // 
            this.numericUpDown_operlogChannelMax.Location = new System.Drawing.Point(228, 75);
            this.numericUpDown_operlogChannelMax.Name = "numericUpDown_operlogChannelMax";
            this.numericUpDown_operlogChannelMax.Size = new System.Drawing.Size(120, 28);
            this.numericUpDown_operlogChannelMax.TabIndex = 1;
            // 
            // numericUpDown_backupChannelMax
            // 
            this.numericUpDown_backupChannelMax.Location = new System.Drawing.Point(228, 41);
            this.numericUpDown_backupChannelMax.Name = "numericUpDown_backupChannelMax";
            this.numericUpDown_backupChannelMax.Size = new System.Drawing.Size(120, 28);
            this.numericUpDown_backupChannelMax.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(767, 353);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(354, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(386, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "(注：对此参数的修改要在重启程序后才能生效)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(354, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(386, 18);
            this.label4.TabIndex = 5;
            this.label4.Text = "(注：对此参数的修改要在重启程序后才能生效)";
            // 
            // ConfigForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "ConfigForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "参数设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ConfigForm_FormClosed);
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_general.ResumeLayout(false);
            this.tabPage_general.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_operlogChannelMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_backupChannelMax)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_general;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.NumericUpDown numericUpDown_backupChannelMax;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown_operlogChannelMax;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
    }
}