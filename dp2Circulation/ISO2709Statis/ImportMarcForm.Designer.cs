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
            this.comboBox_targetDbName = new DigitalPlatform.CommonControl.TabComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage_runImport = new System.Windows.Forms.TabPage();
            this.progressBar_records = new System.Windows.Forms.ProgressBar();
            this.webBrowser1_running = new System.Windows.Forms.WebBrowser();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_print = new System.Windows.Forms.Button();
            this.tabControl_main.SuspendLayout();
            this.tabPage_selectTarget.SuspendLayout();
            this.tabPage_runImport.SuspendLayout();
            this.tabPage_print.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Location = new System.Drawing.Point(435, 345);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(114, 33);
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
            this.tabControl_main.Location = new System.Drawing.Point(0, 15);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(549, 322);
            this.tabControl_main.TabIndex = 9;
            // 
            // tabPage_source
            // 
            this.tabPage_source.Location = new System.Drawing.Point(4, 28);
            this.tabPage_source.Name = "tabPage_source";
            this.tabPage_source.Size = new System.Drawing.Size(541, 290);
            this.tabPage_source.TabIndex = 4;
            this.tabPage_source.Text = "数据来源";
            this.tabPage_source.UseVisualStyleBackColor = true;
            // 
            // tabPage_selectTarget
            // 
            this.tabPage_selectTarget.Controls.Add(this.comboBox_targetDbName);
            this.tabPage_selectTarget.Controls.Add(this.label3);
            this.tabPage_selectTarget.Location = new System.Drawing.Point(4, 28);
            this.tabPage_selectTarget.Name = "tabPage_selectTarget";
            this.tabPage_selectTarget.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_selectTarget.Size = new System.Drawing.Size(541, 290);
            this.tabPage_selectTarget.TabIndex = 1;
            this.tabPage_selectTarget.Text = " 选定目标";
            this.tabPage_selectTarget.UseVisualStyleBackColor = true;
            // 
            // comboBox_targetDbName
            // 
            this.comboBox_targetDbName.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBox_targetDbName.FormattingEnabled = true;
            this.comboBox_targetDbName.Location = new System.Drawing.Point(143, 16);
            this.comboBox_targetDbName.Name = "comboBox_targetDbName";
            this.comboBox_targetDbName.Size = new System.Drawing.Size(274, 29);
            this.comboBox_targetDbName.TabIndex = 1;
            this.comboBox_targetDbName.DropDown += new System.EventHandler(this.comboBox_targetDbName_DropDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 18);
            this.label3.TabIndex = 0;
            this.label3.Text = "目标库(&D):";
            // 
            // tabPage_runImport
            // 
            this.tabPage_runImport.Controls.Add(this.progressBar_records);
            this.tabPage_runImport.Controls.Add(this.webBrowser1_running);
            this.tabPage_runImport.Location = new System.Drawing.Point(4, 28);
            this.tabPage_runImport.Name = "tabPage_runImport";
            this.tabPage_runImport.Size = new System.Drawing.Size(541, 290);
            this.tabPage_runImport.TabIndex = 3;
            this.tabPage_runImport.Text = " 执行统计 ";
            this.tabPage_runImport.UseVisualStyleBackColor = true;
            // 
            // progressBar_records
            // 
            this.progressBar_records.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar_records.Location = new System.Drawing.Point(0, 254);
            this.progressBar_records.Name = "progressBar_records";
            this.progressBar_records.Size = new System.Drawing.Size(534, 16);
            this.progressBar_records.TabIndex = 1;
            // 
            // webBrowser1_running
            // 
            this.webBrowser1_running.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1_running.Location = new System.Drawing.Point(3, 20);
            this.webBrowser1_running.MinimumSize = new System.Drawing.Size(22, 24);
            this.webBrowser1_running.Name = "webBrowser1_running";
            this.webBrowser1_running.Size = new System.Drawing.Size(534, 226);
            this.webBrowser1_running.TabIndex = 0;
            // 
            // tabPage_print
            // 
            this.tabPage_print.Controls.Add(this.button_print);
            this.tabPage_print.Location = new System.Drawing.Point(4, 28);
            this.tabPage_print.Name = "tabPage_print";
            this.tabPage_print.Size = new System.Drawing.Size(541, 290);
            this.tabPage_print.TabIndex = 2;
            this.tabPage_print.Text = " 打印结果 ";
            this.tabPage_print.UseVisualStyleBackColor = true;
            // 
            // button_print
            // 
            this.button_print.Location = new System.Drawing.Point(4, 21);
            this.button_print.Name = "button_print";
            this.button_print.Size = new System.Drawing.Size(240, 33);
            this.button_print.TabIndex = 0;
            this.button_print.Text = "打印统计结果(&P)";
            this.button_print.UseVisualStyleBackColor = true;
            // 
            // ImportMarcForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 393);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.tabControl_main);
            this.Name = "ImportMarcForm";
            this.Text = "导入 MARC";
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
    }
}