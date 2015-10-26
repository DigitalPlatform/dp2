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
            this.ToolStripMenuItem_readerRights = new System.Windows.Forms.ToolStripMenuItem();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_fieldValue = new System.Windows.Forms.ComboBox();
            this.checkedComboBox_stateRemove = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label_remove = new System.Windows.Forms.Label();
            this.checkedComboBox_stateAdd = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label_add = new System.Windows.Forms.Label();
            this.ToolStripMenuItem_objectRights = new System.Windows.Forms.ToolStripMenuItem();
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
            this.toolStrip1.Location = new System.Drawing.Point(278, 36);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.toolStrip1.Size = new System.Drawing.Size(61, 23);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton_inputTimeString
            // 
            this.toolStripDropDownButton_inputTimeString.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton_inputTimeString.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_rfc1123Single,
            this.toolStripSeparator1,
            this.ToolStripMenuItem_readerRights,
            this.ToolStripMenuItem_objectRights});
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
            // ToolStripMenuItem_readerRights
            // 
            this.ToolStripMenuItem_readerRights.Name = "ToolStripMenuItem_readerRights";
            this.ToolStripMenuItem_readerRights.Size = new System.Drawing.Size(171, 22);
            this.ToolStripMenuItem_readerRights.Text = "读者权限值...";
            this.ToolStripMenuItem_readerRights.Click += new System.EventHandler(this.ToolStripMenuItem_readerRights_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(264, 150);
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
            this.button_OK.Location = new System.Drawing.Point(185, 150);
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
            // checkedComboBox_stateRemove
            // 
            this.checkedComboBox_stateRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_stateRemove.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_stateRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_stateRemove.Location = new System.Drawing.Point(185, 81);
            this.checkedComboBox_stateRemove.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_stateRemove.Name = "checkedComboBox_stateRemove";
            this.checkedComboBox_stateRemove.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_stateRemove.Size = new System.Drawing.Size(152, 22);
            this.checkedComboBox_stateRemove.TabIndex = 17;
            this.checkedComboBox_stateRemove.DropDown += new System.EventHandler(this.checkedComboBox_stateRemove_DropDown);
            this.checkedComboBox_stateRemove.TextChanged += new System.EventHandler(this.checkedComboBox_stateRemove_TextChanged);
            // 
            // label_remove
            // 
            this.label_remove.AutoSize = true;
            this.label_remove.Location = new System.Drawing.Point(121, 83);
            this.label_remove.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_remove.Name = "label_remove";
            this.label_remove.Size = new System.Drawing.Size(41, 12);
            this.label_remove.TabIndex = 16;
            this.label_remove.Text = "减(&R):";
            // 
            // checkedComboBox_stateAdd
            // 
            this.checkedComboBox_stateAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_stateAdd.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_stateAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_stateAdd.Location = new System.Drawing.Point(185, 59);
            this.checkedComboBox_stateAdd.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_stateAdd.Name = "checkedComboBox_stateAdd";
            this.checkedComboBox_stateAdd.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_stateAdd.Size = new System.Drawing.Size(152, 22);
            this.checkedComboBox_stateAdd.TabIndex = 15;
            this.checkedComboBox_stateAdd.DropDown += new System.EventHandler(this.checkedComboBox_stateAdd_DropDown);
            this.checkedComboBox_stateAdd.TextChanged += new System.EventHandler(this.checkedComboBox_stateAdd_TextChanged);
            // 
            // label_add
            // 
            this.label_add.AutoSize = true;
            this.label_add.Location = new System.Drawing.Point(121, 61);
            this.label_add.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_add.Name = "label_add";
            this.label_add.Size = new System.Drawing.Size(41, 12);
            this.label_add.TabIndex = 14;
            this.label_add.Text = "增(&A):";
            // 
            // ToolStripMenuItem_objectRights
            // 
            this.ToolStripMenuItem_objectRights.Name = "ToolStripMenuItem_objectRights";
            this.ToolStripMenuItem_objectRights.Size = new System.Drawing.Size(171, 22);
            this.ToolStripMenuItem_objectRights.Text = "对象权限值...";
            this.ToolStripMenuItem_objectRights.Click += new System.EventHandler(this.ToolStripMenuItem_objectRights_Click);
            // 
            // OneActionDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(350, 184);
            this.Controls.Add(this.checkedComboBox_stateRemove);
            this.Controls.Add(this.label_remove);
            this.Controls.Add(this.checkedComboBox_stateAdd);
            this.Controls.Add(this.label_add);
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
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_stateRemove;
        private System.Windows.Forms.Label label_remove;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_stateAdd;
        private System.Windows.Forms.Label label_add;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_readerRights;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_objectRights;
    }
}