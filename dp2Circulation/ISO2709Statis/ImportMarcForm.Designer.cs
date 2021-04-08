namespace dp2Circulation
{
    partial class ImportMarcForm
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
            this.button_next = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_source = new System.Windows.Forms.TabPage();
            this.tabPage_selectTarget = new System.Windows.Forms.TabPage();
            this.textBox_importRange = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_targetDbName = new DigitalPlatform.CommonControl.TabComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage_runImport = new System.Windows.Forms.TabPage();
            this.progressBar_records = new System.Windows.Forms.ProgressBar();
            this.webBrowser1_running = new System.Windows.Forms.WebBrowser();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_print = new System.Windows.Forms.Button();
            this.checkBox_overwriteByG01 = new System.Windows.Forms.CheckBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_selectTarget.SuspendLayout();
            this.tabPage_runImport.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Location = new System.Drawing.Point(532, 402);
            this.button_next.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(139, 38);
            this.button_next.TabIndex = 10;
            this.button_next.Text = "下一步(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_source);
            this.tabControl_main.Controls.Add(this.tabPage_selectTarget);
            this.tabControl_main.Controls.Add(this.tabPage_runImport);
            this.tabControl_main.Controls.Add(this.tabPage_print);
            this.tabControl_main.Location = new System.Drawing.Point(0, 17);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(671, 376);
            this.tabControl_main.TabIndex = 9;
            // 
            // tabPage_source
            // 
            this.tabPage_source.Location = new System.Drawing.Point(4, 31);
            this.tabPage_source.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_source.Name = "tabPage_source";
            this.tabPage_source.Size = new System.Drawing.Size(663, 341);
            this.tabPage_source.TabIndex = 4;
            this.tabPage_source.Text = "数据来源";
            this.tabPage_source.UseVisualStyleBackColor = true;
            // 
            // tabPage_selectTarget
            // 
            this.tabPage_selectTarget.Controls.Add(this.checkBox_overwriteByG01);
            this.tabPage_selectTarget.Controls.Add(this.textBox_importRange);
            this.tabPage_selectTarget.Controls.Add(this.label1);
            this.tabPage_selectTarget.Controls.Add(this.comboBox_targetDbName);
            this.tabPage_selectTarget.Controls.Add(this.label3);
            this.tabPage_selectTarget.Location = new System.Drawing.Point(4, 31);
            this.tabPage_selectTarget.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_selectTarget.Name = "tabPage_selectTarget";
            this.tabPage_selectTarget.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_selectTarget.Size = new System.Drawing.Size(663, 341);
            this.tabPage_selectTarget.TabIndex = 1;
            this.tabPage_selectTarget.Text = " 选定目标";
            this.tabPage_selectTarget.UseVisualStyleBackColor = true;
            // 
            // textBox_importRange
            // 
            this.textBox_importRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_importRange.Location = new System.Drawing.Point(220, 114);
            this.textBox_importRange.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_importRange.Name = "textBox_importRange";
            this.textBox_importRange.Size = new System.Drawing.Size(431, 31);
            this.textBox_importRange.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 118);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 21);
            this.label1.TabIndex = 2;
            this.label1.Text = "导入记录范围(&R):";
            // 
            // comboBox_targetDbName
            // 
            this.comboBox_targetDbName.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBox_targetDbName.FormattingEnabled = true;
            this.comboBox_targetDbName.Location = new System.Drawing.Point(220, 19);
            this.comboBox_targetDbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_targetDbName.Name = "comboBox_targetDbName";
            this.comboBox_targetDbName.Size = new System.Drawing.Size(334, 32);
            this.comboBox_targetDbName.TabIndex = 1;
            this.comboBox_targetDbName.DropDown += new System.EventHandler(this.comboBox_targetDbName_DropDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 22);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 21);
            this.label3.TabIndex = 0;
            this.label3.Text = "目标库(&D):";
            // 
            // tabPage_runImport
            // 
            this.tabPage_runImport.Controls.Add(this.progressBar_records);
            this.tabPage_runImport.Controls.Add(this.webBrowser1_running);
            this.tabPage_runImport.Location = new System.Drawing.Point(4, 31);
            this.tabPage_runImport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_runImport.Name = "tabPage_runImport";
            this.tabPage_runImport.Size = new System.Drawing.Size(663, 341);
            this.tabPage_runImport.TabIndex = 3;
            this.tabPage_runImport.Text = " 执行统计 ";
            this.tabPage_runImport.UseVisualStyleBackColor = true;
            // 
            // progressBar_records
            // 
            this.progressBar_records.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar_records.Location = new System.Drawing.Point(0, 296);
            this.progressBar_records.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.progressBar_records.Name = "progressBar_records";
            this.progressBar_records.Size = new System.Drawing.Size(653, 19);
            this.progressBar_records.TabIndex = 1;
            // 
            // webBrowser1_running
            // 
            this.webBrowser1_running.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1_running.Location = new System.Drawing.Point(4, 23);
            this.webBrowser1_running.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.webBrowser1_running.MinimumSize = new System.Drawing.Size(27, 28);
            this.webBrowser1_running.Name = "webBrowser1_running";
            this.webBrowser1_running.Size = new System.Drawing.Size(653, 264);
            this.webBrowser1_running.TabIndex = 0;
            // 
            // tabPage_print
            // 
            this.tabPage_print.Controls.Add(this.button_print);
            this.tabPage_print.Location = new System.Drawing.Point(4, 31);
            this.tabPage_print.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(663, 341);
            this.tabPage_print.TabIndex = 2;
            this.tabPage_print.Text = " 打印结果 ";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // button_print
            // 
            this.button_print.Location = new System.Drawing.Point(5, 24);
            this.button_print.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_print.Name = "button_print";
            this.button_print.Size = new System.Drawing.Size(293, 38);
            this.button_print.TabIndex = 0;
            this.button_print.Text = "打印统计结果(&P)";
            this.button_print.UseVisualStyleBackColor = true;
            // 
            // checkBox_overwriteByG01
            // 
            this.checkBox_overwriteByG01.AutoSize = true;
            this.checkBox_overwriteByG01.Location = new System.Drawing.Point(220, 58);
            this.checkBox_overwriteByG01.Name = "checkBox_overwriteByG01";
            this.checkBox_overwriteByG01.Size = new System.Drawing.Size(355, 25);
            this.checkBox_overwriteByG01.TabIndex = 4;
            this.checkBox_overwriteByG01.Text = "按照 -01 字段覆盖回原书目库(&O)";
            this.checkBox_overwriteByG01.UseVisualStyleBackColor = true;
            this.checkBox_overwriteByG01.CheckedChanged += new System.EventHandler(this.checkBox_overwriteByG01_CheckedChanged);
            // 
            // ImportMarcForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 458);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.tabControl_main);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ImportMarcForm";
            this.Text = "导入 MARC";
            this.Activated += new System.EventHandler(this.ImportMarcForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportMarcForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ImportMarcForm_FormClosed);
            this.Load += new System.EventHandler(this.ImportMarcForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_selectTarget.ResumeLayout(false);
            this.tabPage_selectTarget.PerformLayout();
            this.tabPage_runImport.ResumeLayout(false);
            this.tabPage_print.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_source;
        private System.Windows.Forms.TabPage tabPage_selectTarget;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPage_runImport;
        private System.Windows.Forms.ProgressBar progressBar_records;
        private System.Windows.Forms.WebBrowser webBrowser1_running;
        private System.Windows.Forms.TabPage tabPage_print;
        private System.Windows.Forms.Button button_print;
        private DigitalPlatform.CommonControl.TabComboBox comboBox_targetDbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_importRange;
        private System.Windows.Forms.CheckBox checkBox_overwriteByG01;
    }
}