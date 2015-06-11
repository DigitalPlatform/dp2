namespace dp2Circulation
{
    partial class MarcDefaultDialog
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
            this.tabPage_unimarc = new System.Windows.Forms.TabPage();
            this.tabPage_marc21 = new System.Windows.Forms.TabPage();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.textBox_unimarc_default = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_unimarc_importantFields = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_marc21_importantFields = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_marc21_default = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_unimarc.SuspendLayout();
            this.tabPage_marc21.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_unimarc);
            this.tabControl_main.Controls.Add(this.tabPage_marc21);
            this.tabControl_main.Location = new System.Drawing.Point(13, 13);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(409, 243);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_unimarc
            // 
            this.tabPage_unimarc.AutoScroll = true;
            this.tabPage_unimarc.Controls.Add(this.textBox_unimarc_importantFields);
            this.tabPage_unimarc.Controls.Add(this.label2);
            this.tabPage_unimarc.Controls.Add(this.textBox_unimarc_default);
            this.tabPage_unimarc.Controls.Add(this.label1);
            this.tabPage_unimarc.Location = new System.Drawing.Point(4, 22);
            this.tabPage_unimarc.Name = "tabPage_unimarc";
            this.tabPage_unimarc.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_unimarc.Size = new System.Drawing.Size(401, 217);
            this.tabPage_unimarc.TabIndex = 0;
            this.tabPage_unimarc.Text = "UNIMARC";
            this.tabPage_unimarc.UseVisualStyleBackColor = true;
            // 
            // tabPage_marc21
            // 
            this.tabPage_marc21.AutoScroll = true;
            this.tabPage_marc21.Controls.Add(this.textBox_marc21_importantFields);
            this.tabPage_marc21.Controls.Add(this.label3);
            this.tabPage_marc21.Controls.Add(this.textBox_marc21_default);
            this.tabPage_marc21.Controls.Add(this.label4);
            this.tabPage_marc21.Location = new System.Drawing.Point(4, 22);
            this.tabPage_marc21.Name = "tabPage_marc21";
            this.tabPage_marc21.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_marc21.Size = new System.Drawing.Size(401, 217);
            this.tabPage_marc21.TabIndex = 1;
            this.tabPage_marc21.Text = "MARC21";
            this.tabPage_marc21.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(266, 262);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(347, 262);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_cancel.TabIndex = 2;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // textBox_unimarc_default
            // 
            this.textBox_unimarc_default.AcceptsReturn = true;
            this.textBox_unimarc_default.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_unimarc_default.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_unimarc_default.HideSelection = false;
            this.textBox_unimarc_default.Location = new System.Drawing.Point(6, 27);
            this.textBox_unimarc_default.Multiline = true;
            this.textBox_unimarc_default.Name = "textBox_unimarc_default";
            this.textBox_unimarc_default.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_unimarc_default.Size = new System.Drawing.Size(372, 126);
            this.textBox_unimarc_default.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(173, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "记录初始值[每行一个字段](&D):";
            // 
            // textBox_unimarc_importantFields
            // 
            this.textBox_unimarc_importantFields.AcceptsReturn = true;
            this.textBox_unimarc_importantFields.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_unimarc_importantFields.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_unimarc_importantFields.HideSelection = false;
            this.textBox_unimarc_importantFields.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_unimarc_importantFields.Location = new System.Drawing.Point(6, 185);
            this.textBox_unimarc_importantFields.Multiline = true;
            this.textBox_unimarc_importantFields.Name = "textBox_unimarc_importantFields";
            this.textBox_unimarc_importantFields.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_unimarc_importantFields.Size = new System.Drawing.Size(372, 126);
            this.textBox_unimarc_importantFields.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 170);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "显示字段[每行一个字段名](&I):";
            // 
            // textBox_marc21_importantFields
            // 
            this.textBox_marc21_importantFields.AcceptsReturn = true;
            this.textBox_marc21_importantFields.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_marc21_importantFields.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_marc21_importantFields.HideSelection = false;
            this.textBox_marc21_importantFields.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_marc21_importantFields.Location = new System.Drawing.Point(6, 185);
            this.textBox_marc21_importantFields.Multiline = true;
            this.textBox_marc21_importantFields.Name = "textBox_marc21_importantFields";
            this.textBox_marc21_importantFields.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_marc21_importantFields.Size = new System.Drawing.Size(372, 126);
            this.textBox_marc21_importantFields.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 170);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(173, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "显示字段[每行一个字段名](&I):";
            // 
            // textBox_marc21_default
            // 
            this.textBox_marc21_default.AcceptsReturn = true;
            this.textBox_marc21_default.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_marc21_default.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_marc21_default.HideSelection = false;
            this.textBox_marc21_default.Location = new System.Drawing.Point(6, 27);
            this.textBox_marc21_default.Multiline = true;
            this.textBox_marc21_default.Name = "textBox_marc21_default";
            this.textBox_marc21_default.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_marc21_default.Size = new System.Drawing.Size(372, 126);
            this.textBox_marc21_default.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(173, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "记录初始值[每行一个字段](&D):";
            // 
            // MarcDefaultDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(434, 297);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl_main);
            this.Name = "MarcDefaultDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "书目记录缺省值";
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_unimarc.ResumeLayout(false);
            this.tabPage_unimarc.PerformLayout();
            this.tabPage_marc21.ResumeLayout(false);
            this.tabPage_marc21.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_unimarc;
        private System.Windows.Forms.TabPage tabPage_marc21;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.TextBox textBox_unimarc_default;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_unimarc_importantFields;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_marc21_importantFields;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_marc21_default;
        private System.Windows.Forms.Label label4;
    }
}