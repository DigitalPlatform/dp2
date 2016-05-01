namespace dp2Circulation
{
    partial class ImportExportForm
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
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_source = new System.Windows.Forms.TabPage();
            this.button_getObjectDirectoryName = new System.Windows.Forms.Button();
            this.textBox_objectDirectoryName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_source_findFileName = new System.Windows.Forms.Button();
            this.textBox_source_fileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_subRecords_object = new System.Windows.Forms.CheckBox();
            this.checkBox_subRecords_comment = new System.Windows.Forms.CheckBox();
            this.checkBox_subRecords_issue = new System.Windows.Forms.CheckBox();
            this.checkBox_subRecords_order = new System.Windows.Forms.CheckBox();
            this.checkBox_subRecords_entity = new System.Windows.Forms.CheckBox();
            this.tabPage_target = new System.Windows.Forms.TabPage();
            this.comboBox_target_targetBiblioDbName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_next = new System.Windows.Forms.Button();
            this.tabPage_run = new System.Windows.Forms.TabPage();
            this.tabControl_main.SuspendLayout();
            this.tabPage_source.SuspendLayout();
            this.tabPage_target.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_source);
            this.tabControl_main.Controls.Add(this.tabPage_target);
            this.tabControl_main.Controls.Add(this.tabPage_run);
            this.tabControl_main.Location = new System.Drawing.Point(13, 13);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(443, 283);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_source
            // 
            this.tabPage_source.AutoScroll = true;
            this.tabPage_source.Controls.Add(this.button_getObjectDirectoryName);
            this.tabPage_source.Controls.Add(this.textBox_objectDirectoryName);
            this.tabPage_source.Controls.Add(this.label3);
            this.tabPage_source.Controls.Add(this.button_source_findFileName);
            this.tabPage_source.Controls.Add(this.textBox_source_fileName);
            this.tabPage_source.Controls.Add(this.label2);
            this.tabPage_source.Controls.Add(this.checkBox_subRecords_object);
            this.tabPage_source.Controls.Add(this.checkBox_subRecords_comment);
            this.tabPage_source.Controls.Add(this.checkBox_subRecords_issue);
            this.tabPage_source.Controls.Add(this.checkBox_subRecords_order);
            this.tabPage_source.Controls.Add(this.checkBox_subRecords_entity);
            this.tabPage_source.Location = new System.Drawing.Point(4, 22);
            this.tabPage_source.Name = "tabPage_source";
            this.tabPage_source.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_source.Size = new System.Drawing.Size(435, 257);
            this.tabPage_source.TabIndex = 1;
            this.tabPage_source.Text = "源文件";
            this.tabPage_source.UseVisualStyleBackColor = true;
            // 
            // button_getObjectDirectoryName
            // 
            this.button_getObjectDirectoryName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getObjectDirectoryName.Location = new System.Drawing.Point(389, 205);
            this.button_getObjectDirectoryName.Name = "button_getObjectDirectoryName";
            this.button_getObjectDirectoryName.Size = new System.Drawing.Size(39, 23);
            this.button_getObjectDirectoryName.TabIndex = 13;
            this.button_getObjectDirectoryName.Text = "...";
            this.button_getObjectDirectoryName.UseVisualStyleBackColor = true;
            this.button_getObjectDirectoryName.Click += new System.EventHandler(this.button_getObjectDirectoryName_Click);
            // 
            // textBox_objectDirectoryName
            // 
            this.textBox_objectDirectoryName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_objectDirectoryName.Location = new System.Drawing.Point(27, 207);
            this.textBox_objectDirectoryName.Name = "textBox_objectDirectoryName";
            this.textBox_objectDirectoryName.Size = new System.Drawing.Size(356, 21);
            this.textBox_objectDirectoryName.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 192);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "对象文件目录(&O):";
            // 
            // button_source_findFileName
            // 
            this.button_source_findFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_source_findFileName.Location = new System.Drawing.Point(389, 19);
            this.button_source_findFileName.Name = "button_source_findFileName";
            this.button_source_findFileName.Size = new System.Drawing.Size(39, 23);
            this.button_source_findFileName.TabIndex = 7;
            this.button_source_findFileName.Text = "...";
            this.button_source_findFileName.UseVisualStyleBackColor = true;
            this.button_source_findFileName.Click += new System.EventHandler(this.button_source_findFileName_Click);
            // 
            // textBox_source_fileName
            // 
            this.textBox_source_fileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_source_fileName.Location = new System.Drawing.Point(125, 21);
            this.textBox_source_fileName.Name = "textBox_source_fileName";
            this.textBox_source_fileName.Size = new System.Drawing.Size(258, 21);
            this.textBox_source_fileName.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "书目转储文件名(&D):";
            // 
            // checkBox_subRecords_object
            // 
            this.checkBox_subRecords_object.AutoSize = true;
            this.checkBox_subRecords_object.Checked = true;
            this.checkBox_subRecords_object.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_subRecords_object.Location = new System.Drawing.Point(8, 173);
            this.checkBox_subRecords_object.Name = "checkBox_subRecords_object";
            this.checkBox_subRecords_object.Size = new System.Drawing.Size(66, 16);
            this.checkBox_subRecords_object.TabIndex = 4;
            this.checkBox_subRecords_object.Text = "对象(&O)";
            this.checkBox_subRecords_object.UseVisualStyleBackColor = true;
            this.checkBox_subRecords_object.CheckedChanged += new System.EventHandler(this.checkBox_subRecords_object_CheckedChanged);
            // 
            // checkBox_subRecords_comment
            // 
            this.checkBox_subRecords_comment.AutoSize = true;
            this.checkBox_subRecords_comment.Checked = true;
            this.checkBox_subRecords_comment.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_subRecords_comment.Location = new System.Drawing.Point(8, 120);
            this.checkBox_subRecords_comment.Name = "checkBox_subRecords_comment";
            this.checkBox_subRecords_comment.Size = new System.Drawing.Size(66, 16);
            this.checkBox_subRecords_comment.TabIndex = 3;
            this.checkBox_subRecords_comment.Text = "评注(&C)";
            this.checkBox_subRecords_comment.UseVisualStyleBackColor = true;
            // 
            // checkBox_subRecords_issue
            // 
            this.checkBox_subRecords_issue.AutoSize = true;
            this.checkBox_subRecords_issue.Checked = true;
            this.checkBox_subRecords_issue.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_subRecords_issue.Location = new System.Drawing.Point(8, 98);
            this.checkBox_subRecords_issue.Name = "checkBox_subRecords_issue";
            this.checkBox_subRecords_issue.Size = new System.Drawing.Size(54, 16);
            this.checkBox_subRecords_issue.TabIndex = 2;
            this.checkBox_subRecords_issue.Text = "期(&I)";
            this.checkBox_subRecords_issue.UseVisualStyleBackColor = true;
            // 
            // checkBox_subRecords_order
            // 
            this.checkBox_subRecords_order.AutoSize = true;
            this.checkBox_subRecords_order.Checked = true;
            this.checkBox_subRecords_order.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_subRecords_order.Location = new System.Drawing.Point(8, 76);
            this.checkBox_subRecords_order.Name = "checkBox_subRecords_order";
            this.checkBox_subRecords_order.Size = new System.Drawing.Size(66, 16);
            this.checkBox_subRecords_order.TabIndex = 1;
            this.checkBox_subRecords_order.Text = "订购(&O)";
            this.checkBox_subRecords_order.UseVisualStyleBackColor = true;
            // 
            // checkBox_subRecords_entity
            // 
            this.checkBox_subRecords_entity.AutoSize = true;
            this.checkBox_subRecords_entity.Checked = true;
            this.checkBox_subRecords_entity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_subRecords_entity.Location = new System.Drawing.Point(8, 54);
            this.checkBox_subRecords_entity.Name = "checkBox_subRecords_entity";
            this.checkBox_subRecords_entity.Size = new System.Drawing.Size(54, 16);
            this.checkBox_subRecords_entity.TabIndex = 0;
            this.checkBox_subRecords_entity.Text = "册(&E)";
            this.checkBox_subRecords_entity.UseVisualStyleBackColor = true;
            // 
            // tabPage_target
            // 
            this.tabPage_target.AutoScroll = true;
            this.tabPage_target.Controls.Add(this.comboBox_target_targetBiblioDbName);
            this.tabPage_target.Controls.Add(this.label1);
            this.tabPage_target.Location = new System.Drawing.Point(4, 22);
            this.tabPage_target.Name = "tabPage_target";
            this.tabPage_target.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_target.Size = new System.Drawing.Size(435, 257);
            this.tabPage_target.TabIndex = 0;
            this.tabPage_target.Text = "目标库";
            this.tabPage_target.UseVisualStyleBackColor = true;
            // 
            // comboBox_target_targetBiblioDbName
            // 
            this.comboBox_target_targetBiblioDbName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_target_targetBiblioDbName.FormattingEnabled = true;
            this.comboBox_target_targetBiblioDbName.Location = new System.Drawing.Point(126, 61);
            this.comboBox_target_targetBiblioDbName.Name = "comboBox_target_targetBiblioDbName";
            this.comboBox_target_targetBiblioDbName.Size = new System.Drawing.Size(182, 20);
            this.comboBox_target_targetBiblioDbName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "目标书目库名(&B):";
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_next.Location = new System.Drawing.Point(369, 301);
            this.button_next.Margin = new System.Windows.Forms.Padding(2);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(83, 22);
            this.button_next.TabIndex = 2;
            this.button_next.Text = "下一步(&N)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // tabPage_run
            // 
            this.tabPage_run.Location = new System.Drawing.Point(4, 22);
            this.tabPage_run.Name = "tabPage_run";
            this.tabPage_run.Size = new System.Drawing.Size(435, 257);
            this.tabPage_run.TabIndex = 2;
            this.tabPage_run.Text = "导入";
            this.tabPage_run.UseVisualStyleBackColor = true;
            // 
            // ImportExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 334);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.tabControl_main);
            this.Name = "ImportExportForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "从书目转储文件导入";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportExportForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ImportExportForm_FormClosed);
            this.Load += new System.EventHandler(this.ImportExportForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_source.ResumeLayout(false);
            this.tabPage_source.PerformLayout();
            this.tabPage_target.ResumeLayout(false);
            this.tabPage_target.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_target;
        private System.Windows.Forms.TabPage tabPage_source;
        private System.Windows.Forms.ComboBox comboBox_target_targetBiblioDbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox_subRecords_object;
        private System.Windows.Forms.CheckBox checkBox_subRecords_comment;
        private System.Windows.Forms.CheckBox checkBox_subRecords_issue;
        private System.Windows.Forms.CheckBox checkBox_subRecords_order;
        private System.Windows.Forms.CheckBox checkBox_subRecords_entity;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Button button_source_findFileName;
        private System.Windows.Forms.TextBox textBox_source_fileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_getObjectDirectoryName;
        private System.Windows.Forms.TextBox textBox_objectDirectoryName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPage_run;
    }
}