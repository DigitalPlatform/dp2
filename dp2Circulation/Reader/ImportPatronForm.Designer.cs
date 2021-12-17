
namespace dp2Circulation
{
    partial class ImportPatronForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_patronXmlFileName = new System.Windows.Forms.TextBox();
            this.button_getFileName = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_appendMode = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_targetDbName = new System.Windows.Forms.ComboBox();
            this.button_begin = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.checkBox_refreshRefID = new System.Windows.Forms.CheckBox();
            this.checkBox_restoreMode = new System.Windows.Forms.CheckBox();
            this.button_getObjectDirectoryName = new System.Windows.Forms.Button();
            this.textBox_objectDirectoryName = new System.Windows.Forms.TextBox();
            this.label_objectDirectoryName = new System.Windows.Forms.Label();
            this.checkBox_object = new System.Windows.Forms.CheckBox();
            this.checkBox_autoPostfix = new System.Windows.Forms.CheckBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(181, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "读者 XML 文件名:";
            // 
            // textBox_patronXmlFileName
            // 
            this.textBox_patronXmlFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_patronXmlFileName.Location = new System.Drawing.Point(17, 38);
            this.textBox_patronXmlFileName.Name = "textBox_patronXmlFileName";
            this.textBox_patronXmlFileName.Size = new System.Drawing.Size(694, 31);
            this.textBox_patronXmlFileName.TabIndex = 1;
            this.textBox_patronXmlFileName.TextChanged += new System.EventHandler(this.textBox_patronXmlFileName_TextChanged);
            // 
            // button_getFileName
            // 
            this.button_getFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getFileName.Location = new System.Drawing.Point(717, 33);
            this.button_getFileName.Name = "button_getFileName";
            this.button_getFileName.Size = new System.Drawing.Size(71, 36);
            this.button_getFileName.TabIndex = 2;
            this.button_getFileName.Text = "...";
            this.button_getFileName.UseVisualStyleBackColor = true;
            this.button_getFileName.Click += new System.EventHandler(this.button_getFileName_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 198);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 21);
            this.label2.TabIndex = 7;
            this.label2.Text = "导入方式:";
            // 
            // comboBox_appendMode
            // 
            this.comboBox_appendMode.FormattingEnabled = true;
            this.comboBox_appendMode.Items.AddRange(new object[] {
            "追加",
            "覆盖到原有路径",
            "覆盖到原有ID"});
            this.comboBox_appendMode.Location = new System.Drawing.Point(131, 195);
            this.comboBox_appendMode.Name = "comboBox_appendMode";
            this.comboBox_appendMode.Size = new System.Drawing.Size(302, 29);
            this.comboBox_appendMode.TabIndex = 8;
            this.comboBox_appendMode.SelectedIndexChanged += new System.EventHandler(this.comboBox_appendMode_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 303);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 21);
            this.label3.TabIndex = 11;
            this.label3.Text = "目标库:";
            // 
            // comboBox_targetDbName
            // 
            this.comboBox_targetDbName.FormattingEnabled = true;
            this.comboBox_targetDbName.Location = new System.Drawing.Point(131, 300);
            this.comboBox_targetDbName.Name = "comboBox_targetDbName";
            this.comboBox_targetDbName.Size = new System.Drawing.Size(302, 29);
            this.comboBox_targetDbName.TabIndex = 12;
            this.comboBox_targetDbName.DropDown += new System.EventHandler(this.comboBox_targetDbName_DropDown);
            this.comboBox_targetDbName.SelectedIndexChanged += new System.EventHandler(this.comboBox_targetDbName_SelectedIndexChanged);
            // 
            // button_begin
            // 
            this.button_begin.Location = new System.Drawing.Point(131, 382);
            this.button_begin.Name = "button_begin";
            this.button_begin.Size = new System.Drawing.Size(141, 43);
            this.button_begin.TabIndex = 14;
            this.button_begin.Text = "开始导入";
            this.button_begin.UseVisualStyleBackColor = true;
            this.button_begin.Click += new System.EventHandler(this.button_begin_Click);
            // 
            // button_stop
            // 
            this.button_stop.Location = new System.Drawing.Point(278, 382);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(141, 43);
            this.button_stop.TabIndex = 15;
            this.button_stop.Text = "停止";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 453);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(818, 37);
            this.statusStrip1.TabIndex = 16;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(300, 27);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(27, 28);
            this.toolStripStatusLabel1.Text = "...";
            // 
            // checkBox_refreshRefID
            // 
            this.checkBox_refreshRefID.AutoSize = true;
            this.checkBox_refreshRefID.Location = new System.Drawing.Point(131, 230);
            this.checkBox_refreshRefID.Name = "checkBox_refreshRefID";
            this.checkBox_refreshRefID.Size = new System.Drawing.Size(219, 25);
            this.checkBox_refreshRefID.TabIndex = 9;
            this.checkBox_refreshRefID.Text = "更新 RefID (慎用)";
            this.checkBox_refreshRefID.UseVisualStyleBackColor = true;
            // 
            // checkBox_restoreMode
            // 
            this.checkBox_restoreMode.AutoSize = true;
            this.checkBox_restoreMode.Location = new System.Drawing.Point(131, 335);
            this.checkBox_restoreMode.Name = "checkBox_restoreMode";
            this.checkBox_restoreMode.Size = new System.Drawing.Size(405, 25);
            this.checkBox_restoreMode.TabIndex = 13;
            this.checkBox_restoreMode.Text = "恢复模式 (危险，会强制写入借阅信息)";
            this.checkBox_restoreMode.UseVisualStyleBackColor = true;
            // 
            // button_getObjectDirectoryName
            // 
            this.button_getObjectDirectoryName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getObjectDirectoryName.Location = new System.Drawing.Point(717, 129);
            this.button_getObjectDirectoryName.Margin = new System.Windows.Forms.Padding(5);
            this.button_getObjectDirectoryName.Name = "button_getObjectDirectoryName";
            this.button_getObjectDirectoryName.Size = new System.Drawing.Size(71, 36);
            this.button_getObjectDirectoryName.TabIndex = 6;
            this.button_getObjectDirectoryName.Text = "...";
            this.button_getObjectDirectoryName.UseVisualStyleBackColor = true;
            this.button_getObjectDirectoryName.Click += new System.EventHandler(this.button_getObjectDirectoryName_Click);
            // 
            // textBox_objectDirectoryName
            // 
            this.textBox_objectDirectoryName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_objectDirectoryName.Location = new System.Drawing.Point(56, 134);
            this.textBox_objectDirectoryName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_objectDirectoryName.Name = "textBox_objectDirectoryName";
            this.textBox_objectDirectoryName.ReadOnly = true;
            this.textBox_objectDirectoryName.Size = new System.Drawing.Size(655, 31);
            this.textBox_objectDirectoryName.TabIndex = 5;
            // 
            // label_objectDirectoryName
            // 
            this.label_objectDirectoryName.AutoSize = true;
            this.label_objectDirectoryName.Location = new System.Drawing.Point(53, 108);
            this.label_objectDirectoryName.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label_objectDirectoryName.Name = "label_objectDirectoryName";
            this.label_objectDirectoryName.Size = new System.Drawing.Size(180, 21);
            this.label_objectDirectoryName.TabIndex = 4;
            this.label_objectDirectoryName.Text = "对象文件目录(&O):";
            // 
            // checkBox_object
            // 
            this.checkBox_object.AutoSize = true;
            this.checkBox_object.Checked = true;
            this.checkBox_object.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_object.Location = new System.Drawing.Point(17, 77);
            this.checkBox_object.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_object.Name = "checkBox_object";
            this.checkBox_object.Size = new System.Drawing.Size(111, 25);
            this.checkBox_object.TabIndex = 3;
            this.checkBox_object.Text = "对象(&O)";
            this.checkBox_object.UseVisualStyleBackColor = true;
            this.checkBox_object.CheckedChanged += new System.EventHandler(this.checkBox_object_CheckedChanged);
            // 
            // checkBox_autoPostfix
            // 
            this.checkBox_autoPostfix.AutoSize = true;
            this.checkBox_autoPostfix.Location = new System.Drawing.Point(131, 261);
            this.checkBox_autoPostfix.Name = "checkBox_autoPostfix";
            this.checkBox_autoPostfix.Size = new System.Drawing.Size(372, 25);
            this.checkBox_autoPostfix.TabIndex = 10;
            this.checkBox_autoPostfix.Text = "遭遇条码号重复时自动添加随机后缀";
            this.checkBox_autoPostfix.UseVisualStyleBackColor = true;
            // 
            // ImportPatronForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(818, 490);
            this.Controls.Add(this.checkBox_autoPostfix);
            this.Controls.Add(this.button_getObjectDirectoryName);
            this.Controls.Add(this.textBox_objectDirectoryName);
            this.Controls.Add(this.label_objectDirectoryName);
            this.Controls.Add(this.checkBox_object);
            this.Controls.Add(this.checkBox_restoreMode);
            this.Controls.Add(this.checkBox_refreshRefID);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_begin);
            this.Controls.Add(this.comboBox_targetDbName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_appendMode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_getFileName);
            this.Controls.Add(this.textBox_patronXmlFileName);
            this.Controls.Add(this.label1);
            this.Name = "ImportPatronForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "从读者 XML 文件导入";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportPatronForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ImportPatronForm_FormClosed);
            this.Load += new System.EventHandler(this.ImportPatronForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_patronXmlFileName;
        private System.Windows.Forms.Button button_getFileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_appendMode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_targetDbName;
        private System.Windows.Forms.Button button_begin;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.CheckBox checkBox_refreshRefID;
        private System.Windows.Forms.CheckBox checkBox_restoreMode;
        private System.Windows.Forms.Button button_getObjectDirectoryName;
        private System.Windows.Forms.TextBox textBox_objectDirectoryName;
        private System.Windows.Forms.Label label_objectDirectoryName;
        private System.Windows.Forms.CheckBox checkBox_object;
        private System.Windows.Forms.CheckBox checkBox_autoPostfix;
    }
}