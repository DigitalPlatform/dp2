﻿namespace dp2Circulation
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
            this.checkBox_lockTargetDbName = new System.Windows.Forms.CheckBox();
            this.checkBox_dontImportDupRecords = new System.Windows.Forms.CheckBox();
            this.tabComboBox_dupProject = new DigitalPlatform.CommonControl.TabComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_batchNo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_overwriteByG01 = new System.Windows.Forms.CheckBox();
            this.textBox_importRange = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_targetDbName = new DigitalPlatform.CommonControl.TabComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage_runImport = new System.Windows.Forms.TabPage();
            this.progressBar_records = new System.Windows.Forms.ProgressBar();
            this.webBrowser1_running = new System.Windows.Forms.WebBrowser();
            this.tabPage_print = new System.Windows.Forms.TabPage();
            this.button_print = new System.Windows.Forms.Button();
            this.textBox_source = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
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
            this.tabPage_selectTarget.Controls.Add(this.textBox_source);
            this.tabPage_selectTarget.Controls.Add(this.label5);
            this.tabPage_selectTarget.Controls.Add(this.checkBox_lockTargetDbName);
            this.tabPage_selectTarget.Controls.Add(this.checkBox_dontImportDupRecords);
            this.tabPage_selectTarget.Controls.Add(this.tabComboBox_dupProject);
            this.tabPage_selectTarget.Controls.Add(this.label4);
            this.tabPage_selectTarget.Controls.Add(this.textBox_batchNo);
            this.tabPage_selectTarget.Controls.Add(this.label2);
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
            // checkBox_lockTargetDbName
            // 
            this.checkBox_lockTargetDbName.AutoSize = true;
            this.checkBox_lockTargetDbName.Location = new System.Drawing.Point(561, 21);
            this.checkBox_lockTargetDbName.Name = "checkBox_lockTargetDbName";
            this.checkBox_lockTargetDbName.Size = new System.Drawing.Size(78, 25);
            this.checkBox_lockTargetDbName.TabIndex = 10;
            this.checkBox_lockTargetDbName.Text = "锁定";
            this.checkBox_lockTargetDbName.UseVisualStyleBackColor = true;
            this.checkBox_lockTargetDbName.CheckedChanged += new System.EventHandler(this.checkBox_lockTargetDbName_CheckedChanged);
            // 
            // checkBox_dontImportDupRecords
            // 
            this.checkBox_dontImportDupRecords.AutoSize = true;
            this.checkBox_dontImportDupRecords.Location = new System.Drawing.Point(220, 212);
            this.checkBox_dontImportDupRecords.Name = "checkBox_dontImportDupRecords";
            this.checkBox_dontImportDupRecords.Size = new System.Drawing.Size(162, 25);
            this.checkBox_dontImportDupRecords.TabIndex = 9;
            this.checkBox_dontImportDupRecords.Text = "重复的不导入";
            this.checkBox_dontImportDupRecords.UseVisualStyleBackColor = true;
            // 
            // tabComboBox_dupProject
            // 
            this.tabComboBox_dupProject.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.tabComboBox_dupProject.FormattingEnabled = true;
            this.tabComboBox_dupProject.Location = new System.Drawing.Point(220, 173);
            this.tabComboBox_dupProject.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabComboBox_dupProject.Name = "tabComboBox_dupProject";
            this.tabComboBox_dupProject.Size = new System.Drawing.Size(334, 32);
            this.tabComboBox_dupProject.TabIndex = 8;
            this.tabComboBox_dupProject.SelectedIndexChanged += new System.EventHandler(this.tabComboBox_dupProject_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 176);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 21);
            this.label4.TabIndex = 7;
            this.label4.Text = "查重方案(&U):";
            // 
            // textBox_batchNo
            // 
            this.textBox_batchNo.Location = new System.Drawing.Point(220, 261);
            this.textBox_batchNo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_batchNo.Name = "textBox_batchNo";
            this.textBox_batchNo.Size = new System.Drawing.Size(334, 31);
            this.textBox_batchNo.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 264);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 21);
            this.label2.TabIndex = 5;
            this.label2.Text = "编目批次号(&B):";
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
            this.label1.Location = new System.Drawing.Point(7, 117);
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
            this.comboBox_targetDbName.SelectedIndexChanged += new System.EventHandler(this.comboBox_targetDbName_SelectedIndexChanged);
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
            // textBox_source
            // 
            this.textBox_source.Location = new System.Drawing.Point(220, 298);
            this.textBox_source.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_source.Name = "textBox_source";
            this.textBox_source.Size = new System.Drawing.Size(334, 31);
            this.textBox_source.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 301);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 21);
            this.label5.TabIndex = 11;
            this.label5.Text = "来源(&S):";
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
        private System.Windows.Forms.TextBox textBox_batchNo;
        private System.Windows.Forms.Label label2;
        private DigitalPlatform.CommonControl.TabComboBox tabComboBox_dupProject;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBox_dontImportDupRecords;
        private System.Windows.Forms.CheckBox checkBox_lockTargetDbName;
        private System.Windows.Forms.TextBox textBox_source;
        private System.Windows.Forms.Label label5;
    }
}