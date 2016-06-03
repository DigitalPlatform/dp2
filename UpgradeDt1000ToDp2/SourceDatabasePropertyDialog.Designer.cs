namespace UpgradeDt1000ToDp2
{
    partial class SourceDatabasePropertyDialog
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
            this.comboBox_role = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_databaseName = new System.Windows.Forms.TextBox();
            this.label_bookOrSeries = new System.Windows.Forms.Label();
            this.comboBox_bookOrSeries = new System.Windows.Forms.ComboBox();
            this.comboBox_marcSyntax = new System.Windows.Forms.ComboBox();
            this.label_marcSyntax = new System.Windows.Forms.Label();
            this.checkBox_order = new System.Windows.Forms.CheckBox();
            this.groupBox_biblioDatabaseProperty = new System.Windows.Forms.GroupBox();
            this.checkBox_hasEntityDb = new System.Windows.Forms.CheckBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_circulation = new System.Windows.Forms.CheckBox();
            this.groupBox_biblioDatabaseProperty.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 79);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "角色(&R):";
            // 
            // comboBox_role
            // 
            this.comboBox_role.FormattingEnabled = true;
            this.comboBox_role.Items.AddRange(new object[] {
            "书目库",
            "读者库",
            "流通日志库",
            "辅助库",
            "规范库"});
            this.comboBox_role.Location = new System.Drawing.Point(10, 99);
            this.comboBox_role.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.comboBox_role.Name = "comboBox_role";
            this.comboBox_role.Size = new System.Drawing.Size(196, 25);
            this.comboBox_role.TabIndex = 3;
            this.comboBox_role.Text = "书目库";
            this.comboBox_role.TextChanged += new System.EventHandler(this.comboBox_role_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 10);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "数据库名(&N):";
            // 
            // textBox_databaseName
            // 
            this.textBox_databaseName.Location = new System.Drawing.Point(10, 31);
            this.textBox_databaseName.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_databaseName.Name = "textBox_databaseName";
            this.textBox_databaseName.Size = new System.Drawing.Size(244, 23);
            this.textBox_databaseName.TabIndex = 1;
            // 
            // label_bookOrSeries
            // 
            this.label_bookOrSeries.AutoSize = true;
            this.label_bookOrSeries.Location = new System.Drawing.Point(20, 35);
            this.label_bookOrSeries.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_bookOrSeries.Name = "label_bookOrSeries";
            this.label_bookOrSeries.Size = new System.Drawing.Size(64, 17);
            this.label_bookOrSeries.TabIndex = 0;
            this.label_bookOrSeries.Text = "图书/期刊:";
            // 
            // comboBox_bookOrSeries
            // 
            this.comboBox_bookOrSeries.FormattingEnabled = true;
            this.comboBox_bookOrSeries.Items.AddRange(new object[] {
            "图书",
            "期刊"});
            this.comboBox_bookOrSeries.Location = new System.Drawing.Point(23, 55);
            this.comboBox_bookOrSeries.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.comboBox_bookOrSeries.Name = "comboBox_bookOrSeries";
            this.comboBox_bookOrSeries.Size = new System.Drawing.Size(196, 25);
            this.comboBox_bookOrSeries.TabIndex = 1;
            this.comboBox_bookOrSeries.Text = "图书";
            // 
            // comboBox_marcSyntax
            // 
            this.comboBox_marcSyntax.FormattingEnabled = true;
            this.comboBox_marcSyntax.Items.AddRange(new object[] {
            "UNIMARC",
            "USMARC"});
            this.comboBox_marcSyntax.Location = new System.Drawing.Point(23, 122);
            this.comboBox_marcSyntax.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.comboBox_marcSyntax.Name = "comboBox_marcSyntax";
            this.comboBox_marcSyntax.Size = new System.Drawing.Size(196, 25);
            this.comboBox_marcSyntax.TabIndex = 3;
            this.comboBox_marcSyntax.Text = "UNIMARC";
            // 
            // label_marcSyntax
            // 
            this.label_marcSyntax.AutoSize = true;
            this.label_marcSyntax.Location = new System.Drawing.Point(20, 101);
            this.label_marcSyntax.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_marcSyntax.Name = "label_marcSyntax";
            this.label_marcSyntax.Size = new System.Drawing.Size(71, 17);
            this.label_marcSyntax.TabIndex = 2;
            this.label_marcSyntax.Text = "MARC格式:";
            // 
            // checkBox_order
            // 
            this.checkBox_order.AutoSize = true;
            this.checkBox_order.Location = new System.Drawing.Point(23, 194);
            this.checkBox_order.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_order.Name = "checkBox_order";
            this.checkBox_order.Size = new System.Drawing.Size(93, 21);
            this.checkBox_order.TabIndex = 4;
            this.checkBox_order.Text = "参与采购(&O)";
            this.checkBox_order.UseVisualStyleBackColor = true;
            // 
            // groupBox_biblioDatabaseProperty
            // 
            this.groupBox_biblioDatabaseProperty.Controls.Add(this.checkBox_hasEntityDb);
            this.groupBox_biblioDatabaseProperty.Controls.Add(this.comboBox_marcSyntax);
            this.groupBox_biblioDatabaseProperty.Controls.Add(this.checkBox_order);
            this.groupBox_biblioDatabaseProperty.Controls.Add(this.label_bookOrSeries);
            this.groupBox_biblioDatabaseProperty.Controls.Add(this.comboBox_bookOrSeries);
            this.groupBox_biblioDatabaseProperty.Controls.Add(this.label_marcSyntax);
            this.groupBox_biblioDatabaseProperty.Location = new System.Drawing.Point(10, 152);
            this.groupBox_biblioDatabaseProperty.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_biblioDatabaseProperty.Name = "groupBox_biblioDatabaseProperty";
            this.groupBox_biblioDatabaseProperty.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_biblioDatabaseProperty.Size = new System.Drawing.Size(244, 222);
            this.groupBox_biblioDatabaseProperty.TabIndex = 4;
            this.groupBox_biblioDatabaseProperty.TabStop = false;
            this.groupBox_biblioDatabaseProperty.Text = " 书目库特性 ";
            // 
            // checkBox_hasEntityDb
            // 
            this.checkBox_hasEntityDb.AutoSize = true;
            this.checkBox_hasEntityDb.Location = new System.Drawing.Point(23, 166);
            this.checkBox_hasEntityDb.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_hasEntityDb.Name = "checkBox_hasEntityDb";
            this.checkBox_hasEntityDb.Size = new System.Drawing.Size(90, 21);
            this.checkBox_hasEntityDb.TabIndex = 5;
            this.checkBox_hasEntityDb.Text = "包含实体(&E)";
            this.checkBox_hasEntityDb.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(118, 449);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(65, 31);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(189, 449);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(65, 31);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_circulation
            // 
            this.checkBox_circulation.AutoSize = true;
            this.checkBox_circulation.Location = new System.Drawing.Point(10, 394);
            this.checkBox_circulation.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_circulation.Name = "checkBox_circulation";
            this.checkBox_circulation.Size = new System.Drawing.Size(91, 21);
            this.checkBox_circulation.TabIndex = 7;
            this.checkBox_circulation.Text = "参与流通(&C)";
            this.checkBox_circulation.UseVisualStyleBackColor = true;
            this.checkBox_circulation.CheckedChanged += new System.EventHandler(this.checkBox_circulation_CheckedChanged);
            // 
            // SourceDatabasePropertyDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(265, 494);
            this.Controls.Add(this.checkBox_circulation);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox_biblioDatabaseProperty);
            this.Controls.Add(this.textBox_databaseName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_role);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SourceDatabasePropertyDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "设置数据库的类型";
            this.Load += new System.EventHandler(this.SourceDatabasePropertyDialog_Load);
            this.groupBox_biblioDatabaseProperty.ResumeLayout(false);
            this.groupBox_biblioDatabaseProperty.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_role;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_databaseName;
        private System.Windows.Forms.Label label_bookOrSeries;
        private System.Windows.Forms.ComboBox comboBox_bookOrSeries;
        private System.Windows.Forms.ComboBox comboBox_marcSyntax;
        private System.Windows.Forms.Label label_marcSyntax;
        private System.Windows.Forms.CheckBox checkBox_order;
        private System.Windows.Forms.GroupBox groupBox_biblioDatabaseProperty;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.CheckBox checkBox_circulation;
        private System.Windows.Forms.CheckBox checkBox_hasEntityDb;
    }
}