namespace dp2Circulation
{
    partial class OneActionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OneActionDialog));
            this.label_fieldName = new System.Windows.Forms.Label();
            this.comboBox_fieldName = new System.Windows.Forms.ComboBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton_inputTimeString = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_rfc1123Single = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_fieldValue = new System.Windows.Forms.ComboBox();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_fieldName
            // 
            this.label_fieldName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_fieldName.Location = new System.Drawing.Point(107, 36);
            this.label_fieldName.Margin = new System.Windows.Forms.Padding(0);
            this.label_fieldName.Name = "label_fieldName";
            this.label_fieldName.Size = new System.Drawing.Size(10, 20);
            this.label_fieldName.TabIndex = 3;
            // 
            // comboBox_fieldName
            // 
            this.comboBox_fieldName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_fieldName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_fieldName.FormattingEnabled = true;
            this.comboBox_fieldName.Location = new System.Drawing.Point(120, 11);
            this.comboBox_fieldName.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_fieldName.Name = "comboBox_fieldName";
            this.comboBox_fieldName.Size = new System.Drawing.Size(217, 20);
            this.comboBox_fieldName.TabIndex = 1;
            this.comboBox_fieldName.SelectedIndexChanged += new System.EventHandler(this.comboBox_fieldName_SelectedIndexChanged);
            this.comboBox_fieldName.SizeChanged += new System.EventHandler(this.comboBox_fieldName_SizeChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton_inputTimeString});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip1.Location = new System.Drawing.Point(309, 36);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.toolStrip1.Size = new System.Drawing.Size(30, 23);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton_inputTimeString
            // 
            this.toolStripDropDownButton_inputTimeString.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton_inputTimeString.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_rfc1123Single,
            this.toolStripSeparator1});
            this.toolStripDropDownButton_inputTimeString.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_inputTimeString.Image")));
            this.toolStripDropDownButton_inputTimeString.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_inputTimeString.Name = "toolStripDropDownButton_inputTimeString";
            this.toolStripDropDownButton_inputTimeString.Size = new System.Drawing.Size(29, 20);
            this.toolStripDropDownButton_inputTimeString.Text = "和时间有关的功能";
            // 
            // ToolStripMenuItem_rfc1123Single
            // 
            this.ToolStripMenuItem_rfc1123Single.Name = "ToolStripMenuItem_rfc1123Single";
            this.ToolStripMenuItem_rfc1123Single.Size = new System.Drawing.Size(171, 22);
            this.ToolStripMenuItem_rfc1123Single.Text = "RFC1123时间值...";
            this.ToolStripMenuItem_rfc1123Single.Click += new System.EventHandler(this.ToolStripMenuItem_rfc1123Single_Click_1);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(168, 6);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(264, 108);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(185, 108);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "字段值(&V):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "字段名(&N):";
            // 
            // comboBox_fieldValue
            // 
            this.comboBox_fieldValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_fieldValue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_fieldValue.FormattingEnabled = true;
            this.comboBox_fieldValue.Location = new System.Drawing.Point(120, 36);
            this.comboBox_fieldValue.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_fieldValue.Name = "comboBox_fieldValue";
            this.comboBox_fieldValue.Size = new System.Drawing.Size(182, 20);
            this.comboBox_fieldValue.TabIndex = 4;
            this.comboBox_fieldValue.DropDown += new System.EventHandler(this.comboBox_fieldValue_DropDown);
            this.comboBox_fieldValue.SizeChanged += new System.EventHandler(this.comboBox_fieldValue_SizeChanged);
            this.comboBox_fieldValue.TextChanged += new System.EventHandler(this.comboBox_fieldValue_TextChanged);
            // 
            // OneActionDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(350, 142);
            this.Controls.Add(this.comboBox_fieldValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label_fieldName);
            this.Controls.Add(this.comboBox_fieldName);
            this.Controls.Add(this.toolStrip1);
            this.Name = "OneActionDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "一个修改动作";
            this.Load += new System.EventHandler(this.OneActionDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_fieldName;
        private System.Windows.Forms.ComboBox comboBox_fieldName;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_inputTimeString;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_rfc1123Single;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_fieldValue;
    }
}