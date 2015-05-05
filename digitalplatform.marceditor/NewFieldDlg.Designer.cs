namespace DigitalPlatform.Marc
{
    partial class NewFieldDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewFieldDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_fieldName = new System.Windows.Forms.TextBox();
            this.listView_fieldNameList = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_fieldName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.radioButton_insertBefore = new System.Windows.Forms.RadioButton();
            this.radioButton_insertAfter = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_autoComplete = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 198);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "字段名(&N):";
            // 
            // textBox_fieldName
            // 
            this.textBox_fieldName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_fieldName.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_fieldName.Location = new System.Drawing.Point(81, 192);
            this.textBox_fieldName.MaxLength = 3;
            this.textBox_fieldName.Name = "textBox_fieldName";
            this.textBox_fieldName.Size = new System.Drawing.Size(100, 26);
            this.textBox_fieldName.TabIndex = 0;
            this.textBox_fieldName.TextChanged += new System.EventHandler(this.textBox_fieldName_TextChanged);
            this.textBox_fieldName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_fieldName_KeyPress);
            this.textBox_fieldName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox_fieldName_KeyUp);
            // 
            // listView_fieldNameList
            // 
            this.listView_fieldNameList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_fieldNameList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_fieldName,
            this.columnHeader_comment});
            this.listView_fieldNameList.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listView_fieldNameList.FullRowSelect = true;
            this.listView_fieldNameList.HideSelection = false;
            this.listView_fieldNameList.Location = new System.Drawing.Point(10, 10);
            this.listView_fieldNameList.Name = "listView_fieldNameList";
            this.listView_fieldNameList.Size = new System.Drawing.Size(229, 176);
            this.listView_fieldNameList.TabIndex = 5;
            this.listView_fieldNameList.UseCompatibleStateImageBehavior = false;
            this.listView_fieldNameList.View = System.Windows.Forms.View.Details;
            this.listView_fieldNameList.SelectedIndexChanged += new System.EventHandler(this.listView_fieldNameList_SelectedIndexChanged);
            this.listView_fieldNameList.DoubleClick += new System.EventHandler(this.listView_fieldNameList_DoubleClick);
            // 
            // columnHeader_fieldName
            // 
            this.columnHeader_fieldName.Text = "字段名";
            this.columnHeader_fieldName.Width = 72;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "注释";
            this.columnHeader_comment.Width = 227;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(82, 323);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 22);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(163, 323);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 22);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // radioButton_insertBefore
            // 
            this.radioButton_insertBefore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_insertBefore.AutoSize = true;
            this.radioButton_insertBefore.Location = new System.Drawing.Point(71, 24);
            this.radioButton_insertBefore.Name = "radioButton_insertBefore";
            this.radioButton_insertBefore.Size = new System.Drawing.Size(49, 13);
            this.radioButton_insertBefore.TabIndex = 0;
            this.radioButton_insertBefore.Text = "前插(&B)";
            this.radioButton_insertBefore.UseVisualStyleBackColor = true;
            // 
            // radioButton_insertAfter
            // 
            this.radioButton_insertAfter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_insertAfter.AutoSize = true;
            this.radioButton_insertAfter.Checked = true;
            this.radioButton_insertAfter.Location = new System.Drawing.Point(71, 48);
            this.radioButton_insertAfter.Name = "radioButton_insertAfter";
            this.radioButton_insertAfter.Size = new System.Drawing.Size(49, 13);
            this.radioButton_insertAfter.TabIndex = 1;
            this.radioButton_insertAfter.TabStop = true;
            this.radioButton_insertAfter.Text = "后插(&A)";
            this.radioButton_insertAfter.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.radioButton_insertAfter);
            this.groupBox1.Controls.Add(this.radioButton_insertBefore);
            this.groupBox1.Location = new System.Drawing.Point(10, 244);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(228, 73);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 插入方式 ";
            // 
            // checkBox_autoComplete
            // 
            this.checkBox_autoComplete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_autoComplete.AutoSize = true;
            this.checkBox_autoComplete.Checked = true;
            this.checkBox_autoComplete.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_autoComplete.Location = new System.Drawing.Point(81, 221);
            this.checkBox_autoComplete.Name = "checkBox_autoComplete";
            this.checkBox_autoComplete.Size = new System.Drawing.Size(90, 16);
            this.checkBox_autoComplete.TabIndex = 1;
            this.checkBox_autoComplete.Text = "自动结束(&C)";
            this.checkBox_autoComplete.UseVisualStyleBackColor = true;
            // 
            // NewFieldDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(248, 356);
            this.Controls.Add(this.checkBox_autoComplete);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_fieldNameList);
            this.Controls.Add(this.textBox_fieldName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NewFieldDlg";
            this.ShowInTaskbar = false;
            this.Text = "新字段";
            this.Load += new System.EventHandler(this.NewFieldDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_fieldName;
        private DigitalPlatform.GUI.ListViewNF listView_fieldNameList;
        private System.Windows.Forms.ColumnHeader columnHeader_fieldName;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.RadioButton radioButton_insertBefore;
        private System.Windows.Forms.RadioButton radioButton_insertAfter;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_autoComplete;
    }
}