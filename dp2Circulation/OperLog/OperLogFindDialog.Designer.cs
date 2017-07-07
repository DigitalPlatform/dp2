namespace dp2Circulation
{
    partial class OperLogFindDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OperLogFindDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_recPathList = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkedComboBox_filter = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.checkedComboBox_operations = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "操作类型";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(308, 229);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(389, 229);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "过滤方式";
            // 
            // textBox_recPathList
            // 
            this.textBox_recPathList.AcceptsReturn = true;
            this.textBox_recPathList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_recPathList.Location = new System.Drawing.Point(12, 96);
            this.textBox_recPathList.MaxLength = 0;
            this.textBox_recPathList.Multiline = true;
            this.textBox_recPathList.Name = "textBox_recPathList";
            this.textBox_recPathList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_recPathList.Size = new System.Drawing.Size(451, 127);
            this.textBox_recPathList.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "相关记录路径(每行一个):";
            // 
            // checkedComboBox_filter
            // 
            this.checkedComboBox_filter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_filter.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_filter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_filter.Location = new System.Drawing.Point(88, 43);
            this.checkedComboBox_filter.Margin = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_filter.Name = "checkedComboBox_filter";
            this.checkedComboBox_filter.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_filter.Size = new System.Drawing.Size(375, 22);
            this.checkedComboBox_filter.TabIndex = 5;
            this.checkedComboBox_filter.DropDown += new System.EventHandler(this.checkedComboBox_filter_DropDown);
            this.checkedComboBox_filter.TextChanged += new System.EventHandler(this.checkedComboBox_filter_TextChanged);
            // 
            // checkedComboBox_operations
            // 
            this.checkedComboBox_operations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_operations.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_operations.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_operations.Location = new System.Drawing.Point(88, 13);
            this.checkedComboBox_operations.Margin = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_operations.Name = "checkedComboBox_operations";
            this.checkedComboBox_operations.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_operations.Size = new System.Drawing.Size(375, 22);
            this.checkedComboBox_operations.TabIndex = 1;
            this.checkedComboBox_operations.DropDown += new System.EventHandler(this.checkedComboBox_operations_DropDown);
            // 
            // OperLogFindDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(476, 264);
            this.Controls.Add(this.textBox_recPathList);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkedComboBox_filter);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkedComboBox_operations);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OperLogFindDialog";
            this.ShowInTaskbar = false;
            this.Text = "查找日志记录";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_operations;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_filter;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_recPathList;
        private System.Windows.Forms.Label label3;
    }
}