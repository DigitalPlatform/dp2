namespace dp2Circulation
{
    partial class OpacBrowseFormatDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpacBrowseFormatDialog));
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_type = new System.Windows.Forms.ComboBox();
            this.textBox_scriptFile = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.groupBox_formatName = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_virtualDatabaseName_newBlankLine = new System.Windows.Forms.Button();
            this.captionEditControl_formatName = new DigitalPlatform.CommonControl.CaptionEditControl();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_style = new System.Windows.Forms.TextBox();
            this.groupBox_formatName.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 298);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "����(&T):";
            // 
            // comboBox_type
            // 
            this.comboBox_type.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBox_type.FormattingEnabled = true;
            this.comboBox_type.Items.AddRange(new object[] {
            "biblio"});
            this.comboBox_type.Location = new System.Drawing.Point(18, 322);
            this.comboBox_type.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox_type.Name = "comboBox_type";
            this.comboBox_type.Size = new System.Drawing.Size(242, 29);
            this.comboBox_type.TabIndex = 3;
            // 
            // textBox_scriptFile
            // 
            this.textBox_scriptFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_scriptFile.Location = new System.Drawing.Point(17, 401);
            this.textBox_scriptFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_scriptFile.Name = "textBox_scriptFile";
            this.textBox_scriptFile.Size = new System.Drawing.Size(671, 31);
            this.textBox_scriptFile.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 374);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(159, 21);
            this.label3.TabIndex = 4;
            this.label3.Text = "�ű��ļ���(&S):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(587, 457);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(103, 38);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "ȡ��";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(477, 457);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(103, 38);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "ȷ��";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // groupBox_formatName
            // 
            this.groupBox_formatName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_formatName.Controls.Add(this.label5);
            this.groupBox_formatName.Controls.Add(this.button_virtualDatabaseName_newBlankLine);
            this.groupBox_formatName.Controls.Add(this.captionEditControl_formatName);
            this.groupBox_formatName.Location = new System.Drawing.Point(17, 18);
            this.groupBox_formatName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox_formatName.Name = "groupBox_formatName";
            this.groupBox_formatName.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox_formatName.Size = new System.Drawing.Size(675, 275);
            this.groupBox_formatName.TabIndex = 8;
            this.groupBox_formatName.TabStop = false;
            this.groupBox_formatName.Text = "��ʾ��ʽ��";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.BackColor = System.Drawing.SystemColors.Info;
            this.label5.Location = new System.Drawing.Point(18, 210);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.label5.Size = new System.Drawing.Size(631, 58);
            this.label5.TabIndex = 6;
            this.label5.Text = "ע����Ҫ����ʾ��ʽȡ���֡�����Ҫ��һ������(���Դ���Ϊzh)�����֡�";
            // 
            // button_virtualDatabaseName_newBlankLine
            // 
            this.button_virtualDatabaseName_newBlankLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_virtualDatabaseName_newBlankLine.Location = new System.Drawing.Point(22, 166);
            this.button_virtualDatabaseName_newBlankLine.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_virtualDatabaseName_newBlankLine.Name = "button_virtualDatabaseName_newBlankLine";
            this.button_virtualDatabaseName_newBlankLine.Size = new System.Drawing.Size(202, 38);
            this.button_virtualDatabaseName_newBlankLine.TabIndex = 5;
            this.button_virtualDatabaseName_newBlankLine.Text = "�����հ���(&N)";
            this.button_virtualDatabaseName_newBlankLine.UseVisualStyleBackColor = true;
            this.button_virtualDatabaseName_newBlankLine.Click += new System.EventHandler(this.button_virtualDatabaseName_newBlankLine_Click);
            // 
            // captionEditControl_formatName
            // 
            this.captionEditControl_formatName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.captionEditControl_formatName.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            this.captionEditControl_formatName.Changed = false;
            this.captionEditControl_formatName.Location = new System.Drawing.Point(22, 33);
            this.captionEditControl_formatName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.captionEditControl_formatName.Name = "captionEditControl_formatName";
            this.captionEditControl_formatName.Size = new System.Drawing.Size(627, 124);
            this.captionEditControl_formatName.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(319, 298);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 21);
            this.label1.TabIndex = 9;
            this.label1.Text = "����(&S):";
            // 
            // textBox_style
            // 
            this.textBox_style.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_style.Location = new System.Drawing.Point(323, 320);
            this.textBox_style.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_style.Name = "textBox_style";
            this.textBox_style.Size = new System.Drawing.Size(365, 31);
            this.textBox_style.TabIndex = 10;
            // 
            // OpacBrowseFormatDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(708, 513);
            this.Controls.Add(this.textBox_style);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox_formatName);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_scriptFile);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_type);
            this.Controls.Add(this.label2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "OpacBrowseFormatDialog";
            this.ShowInTaskbar = false;
            this.Text = "OPAC��¼��ʾ��ʽ";
            this.Load += new System.EventHandler(this.OpacBrowseFormatDialog_Load);
            this.groupBox_formatName.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_type;
        private System.Windows.Forms.TextBox textBox_scriptFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.GroupBox groupBox_formatName;
        private DigitalPlatform.CommonControl.CaptionEditControl captionEditControl_formatName;
        private System.Windows.Forms.Button button_virtualDatabaseName_newBlankLine;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_style;
    }
}