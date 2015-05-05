namespace dp2Circulation
{
    partial class ReportColumnDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_name = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_align = new System.Windows.Forms.ComboBox();
            this.checkBox_sum = new System.Windows.Forms.CheckBox();
            this.textBox_cssClass = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_eval = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_dataType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(197, 253);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 12;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(116, 253);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 11;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "栏目名(&N):";
            // 
            // textBox_name
            // 
            this.textBox_name.Location = new System.Drawing.Point(116, 13);
            this.textBox_name.Name = "textBox_name";
            this.textBox_name.Size = new System.Drawing.Size(156, 21);
            this.textBox_name.TabIndex = 1;
            this.textBox_name.TextChanged += new System.EventHandler(this.textBox_name_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "对齐方式(&A):";
            // 
            // comboBox_align
            // 
            this.comboBox_align.FormattingEnabled = true;
            this.comboBox_align.Items.AddRange(new object[] {
            "left",
            "right",
            "center"});
            this.comboBox_align.Location = new System.Drawing.Point(116, 66);
            this.comboBox_align.Name = "comboBox_align";
            this.comboBox_align.Size = new System.Drawing.Size(156, 20);
            this.comboBox_align.TabIndex = 5;
            this.comboBox_align.SelectedIndexChanged += new System.EventHandler(this.comboBox_align_SelectedIndexChanged);
            // 
            // checkBox_sum
            // 
            this.checkBox_sum.AutoSize = true;
            this.checkBox_sum.Location = new System.Drawing.Point(12, 125);
            this.checkBox_sum.Name = "checkBox_sum";
            this.checkBox_sum.Size = new System.Drawing.Size(90, 16);
            this.checkBox_sum.TabIndex = 8;
            this.checkBox_sum.Text = "进行合计(&S)";
            this.checkBox_sum.UseVisualStyleBackColor = true;
            // 
            // textBox_cssClass
            // 
            this.textBox_cssClass.Location = new System.Drawing.Point(116, 92);
            this.textBox_cssClass.Name = "textBox_cssClass";
            this.textBox_cssClass.Size = new System.Drawing.Size(156, 21);
            this.textBox_cssClass.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "样式名(&C):";
            // 
            // textBox_eval
            // 
            this.textBox_eval.AcceptsReturn = true;
            this.textBox_eval.AcceptsTab = true;
            this.textBox_eval.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_eval.Location = new System.Drawing.Point(116, 154);
            this.textBox_eval.Multiline = true;
            this.textBox_eval.Name = "textBox_eval";
            this.textBox_eval.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_eval.Size = new System.Drawing.Size(156, 77);
            this.textBox_eval.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "脚本(&E):";
            // 
            // comboBox_dataType
            // 
            this.comboBox_dataType.FormattingEnabled = true;
            this.comboBox_dataType.Items.AddRange(new object[] {
            "Currency",
            "String",
            "Auto"});
            this.comboBox_dataType.Location = new System.Drawing.Point(116, 40);
            this.comboBox_dataType.Name = "comboBox_dataType";
            this.comboBox_dataType.Size = new System.Drawing.Size(156, 20);
            this.comboBox_dataType.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "数据类型(&T):";
            // 
            // ReportColumnDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(284, 288);
            this.Controls.Add(this.comboBox_dataType);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_eval);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_cssClass);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBox_sum);
            this.Controls.Add(this.comboBox_align);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_name);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "ReportColumnDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "一个栏目";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_name;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_align;
        private System.Windows.Forms.CheckBox checkBox_sum;
        private System.Windows.Forms.TextBox textBox_cssClass;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_eval;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_dataType;
        private System.Windows.Forms.Label label5;
    }
}