namespace dp2Circulation
{
    partial class GetMergeStyleDialog
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.groupBox_biblio = new System.Windows.Forms.GroupBox();
            this.radioButton_biblio_reserveTarget = new System.Windows.Forms.RadioButton();
            this.radioButton_biblio_reserveSource = new System.Windows.Forms.RadioButton();
            this.groupBox_subRecord = new System.Windows.Forms.GroupBox();
            this.radioButton_subrecord_target = new System.Windows.Forms.RadioButton();
            this.radioButton_subrecord_source = new System.Windows.Forms.RadioButton();
            this.radioButton_subrecord_combin = new System.Windows.Forms.RadioButton();
            this.textBox_messageText = new System.Windows.Forms.TextBox();
            this.groupBox_biblio.SuspendLayout();
            this.groupBox_subRecord.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.BackColor = System.Drawing.Color.Gray;
            this.panel1.Location = new System.Drawing.Point(13, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(307, 270);
            this.panel1.TabIndex = 0;
            this.panel1.SizeChanged += new System.EventHandler(this.panel1_SizeChanged);
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(473, 289);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(392, 289);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // groupBox_biblio
            // 
            this.groupBox_biblio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_biblio.BackColor = System.Drawing.SystemColors.Window;
            this.groupBox_biblio.Controls.Add(this.radioButton_biblio_reserveTarget);
            this.groupBox_biblio.Controls.Add(this.radioButton_biblio_reserveSource);
            this.groupBox_biblio.Location = new System.Drawing.Point(327, 120);
            this.groupBox_biblio.Name = "groupBox_biblio";
            this.groupBox_biblio.Size = new System.Drawing.Size(221, 55);
            this.groupBox_biblio.TabIndex = 5;
            this.groupBox_biblio.TabStop = false;
            this.groupBox_biblio.Text = "书目";
            // 
            // radioButton_biblio_reserveTarget
            // 
            this.radioButton_biblio_reserveTarget.AutoSize = true;
            this.radioButton_biblio_reserveTarget.Location = new System.Drawing.Point(119, 23);
            this.radioButton_biblio_reserveTarget.Name = "radioButton_biblio_reserveTarget";
            this.radioButton_biblio_reserveTarget.Size = new System.Drawing.Size(71, 16);
            this.radioButton_biblio_reserveTarget.TabIndex = 1;
            this.radioButton_biblio_reserveTarget.Text = "采用目标";
            this.radioButton_biblio_reserveTarget.UseVisualStyleBackColor = true;
            this.radioButton_biblio_reserveTarget.CheckedChanged += new System.EventHandler(this.radioButton_biblio_reserveTarget_CheckedChanged);
            // 
            // radioButton_biblio_reserveSource
            // 
            this.radioButton_biblio_reserveSource.AutoSize = true;
            this.radioButton_biblio_reserveSource.Checked = true;
            this.radioButton_biblio_reserveSource.Location = new System.Drawing.Point(6, 23);
            this.radioButton_biblio_reserveSource.Name = "radioButton_biblio_reserveSource";
            this.radioButton_biblio_reserveSource.Size = new System.Drawing.Size(59, 16);
            this.radioButton_biblio_reserveSource.TabIndex = 0;
            this.radioButton_biblio_reserveSource.TabStop = true;
            this.radioButton_biblio_reserveSource.Text = "采用源";
            this.radioButton_biblio_reserveSource.UseVisualStyleBackColor = true;
            this.radioButton_biblio_reserveSource.CheckedChanged += new System.EventHandler(this.radioButton_biblio_reserveSource_CheckedChanged);
            // 
            // groupBox_subRecord
            // 
            this.groupBox_subRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_subRecord.BackColor = System.Drawing.SystemColors.Window;
            this.groupBox_subRecord.Controls.Add(this.radioButton_subrecord_target);
            this.groupBox_subRecord.Controls.Add(this.radioButton_subrecord_source);
            this.groupBox_subRecord.Controls.Add(this.radioButton_subrecord_combin);
            this.groupBox_subRecord.Location = new System.Drawing.Point(327, 188);
            this.groupBox_subRecord.Name = "groupBox_subRecord";
            this.groupBox_subRecord.Size = new System.Drawing.Size(221, 95);
            this.groupBox_subRecord.TabIndex = 6;
            this.groupBox_subRecord.TabStop = false;
            this.groupBox_subRecord.Text = "子记录(册、订购、期、评注)";
            // 
            // radioButton_subrecord_target
            // 
            this.radioButton_subrecord_target.AutoSize = true;
            this.radioButton_subrecord_target.Location = new System.Drawing.Point(119, 69);
            this.radioButton_subrecord_target.Name = "radioButton_subrecord_target";
            this.radioButton_subrecord_target.Size = new System.Drawing.Size(71, 16);
            this.radioButton_subrecord_target.TabIndex = 3;
            this.radioButton_subrecord_target.Text = "采用目标";
            this.radioButton_subrecord_target.UseVisualStyleBackColor = true;
            this.radioButton_subrecord_target.CheckedChanged += new System.EventHandler(this.radioButton_subrecord_target_CheckedChanged);
            // 
            // radioButton_subrecord_source
            // 
            this.radioButton_subrecord_source.AutoSize = true;
            this.radioButton_subrecord_source.Location = new System.Drawing.Point(6, 47);
            this.radioButton_subrecord_source.Name = "radioButton_subrecord_source";
            this.radioButton_subrecord_source.Size = new System.Drawing.Size(59, 16);
            this.radioButton_subrecord_source.TabIndex = 2;
            this.radioButton_subrecord_source.Text = "采用源";
            this.radioButton_subrecord_source.UseVisualStyleBackColor = true;
            this.radioButton_subrecord_source.CheckedChanged += new System.EventHandler(this.radioButton_subrecord_source_CheckedChanged);
            // 
            // radioButton_subrecord_combin
            // 
            this.radioButton_subrecord_combin.AutoSize = true;
            this.radioButton_subrecord_combin.Checked = true;
            this.radioButton_subrecord_combin.Location = new System.Drawing.Point(54, 23);
            this.radioButton_subrecord_combin.Name = "radioButton_subrecord_combin";
            this.radioButton_subrecord_combin.Size = new System.Drawing.Size(95, 16);
            this.radioButton_subrecord_combin.TabIndex = 1;
            this.radioButton_subrecord_combin.TabStop = true;
            this.radioButton_subrecord_combin.Text = "合并源和目标";
            this.radioButton_subrecord_combin.UseVisualStyleBackColor = true;
            this.radioButton_subrecord_combin.CheckedChanged += new System.EventHandler(this.radioButton_subrecord_combin_CheckedChanged);
            // 
            // textBox_messageText
            // 
            this.textBox_messageText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_messageText.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_messageText.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_messageText.Location = new System.Drawing.Point(333, 13);
            this.textBox_messageText.Multiline = true;
            this.textBox_messageText.Name = "textBox_messageText";
            this.textBox_messageText.ReadOnly = true;
            this.textBox_messageText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_messageText.Size = new System.Drawing.Size(215, 82);
            this.textBox_messageText.TabIndex = 7;
            // 
            // GetMergeStyleDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(560, 324);
            this.Controls.Add(this.textBox_messageText);
            this.Controls.Add(this.groupBox_subRecord);
            this.Controls.Add(this.groupBox_biblio);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.panel1);
            this.Name = "GetMergeStyleDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "请指定合并方式";
            this.Load += new System.EventHandler(this.GetMergeStyleDialog_Load);
            this.groupBox_biblio.ResumeLayout(false);
            this.groupBox_biblio.PerformLayout();
            this.groupBox_subRecord.ResumeLayout(false);
            this.groupBox_subRecord.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.GroupBox groupBox_biblio;
        private System.Windows.Forms.RadioButton radioButton_biblio_reserveTarget;
        private System.Windows.Forms.RadioButton radioButton_biblio_reserveSource;
        private System.Windows.Forms.GroupBox groupBox_subRecord;
        private System.Windows.Forms.RadioButton radioButton_subrecord_target;
        private System.Windows.Forms.RadioButton radioButton_subrecord_source;
        private System.Windows.Forms.RadioButton radioButton_subrecord_combin;
        private System.Windows.Forms.TextBox textBox_messageText;
    }
}