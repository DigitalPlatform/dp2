namespace dp2Circulation
{
    partial class ReportDefForm
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
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_title = new System.Windows.Forms.TabPage();
            this.textBox_title_typeName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_title_comment = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_title_title = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_columns = new System.Windows.Forms.TabPage();
            this.textBox_columns_sortStyle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.listView_columns = new System.Windows.Forms.ListView();
            this.columnHeader_title = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_align = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_sum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_css = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_eval = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage_css = new System.Windows.Forms.TabPage();
            this.textBox_css_content = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage_property = new System.Windows.Forms.TabPage();
            this.checkedComboBox_property_createFreq = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox_property_fresh = new System.Windows.Forms.CheckBox();
            this.columnHeader_dataType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl_main.SuspendLayout();
            this.tabPage_title.SuspendLayout();
            this.tabPage_columns.SuspendLayout();
            this.tabPage_css.SuspendLayout();
            this.tabPage_property.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(340, 262);
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
            this.button_OK.Location = new System.Drawing.Point(259, 262);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_title);
            this.tabControl_main.Controls.Add(this.tabPage_columns);
            this.tabControl_main.Controls.Add(this.tabPage_css);
            this.tabControl_main.Controls.Add(this.tabPage_property);
            this.tabControl_main.Location = new System.Drawing.Point(13, 13);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(402, 243);
            this.tabControl_main.TabIndex = 5;
            // 
            // tabPage_title
            // 
            this.tabPage_title.AutoScroll = true;
            this.tabPage_title.Controls.Add(this.textBox_title_typeName);
            this.tabPage_title.Controls.Add(this.label5);
            this.tabPage_title.Controls.Add(this.textBox_title_comment);
            this.tabPage_title.Controls.Add(this.label2);
            this.tabPage_title.Controls.Add(this.textBox_title_title);
            this.tabPage_title.Controls.Add(this.label1);
            this.tabPage_title.Location = new System.Drawing.Point(4, 22);
            this.tabPage_title.Name = "tabPage_title";
            this.tabPage_title.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_title.Size = new System.Drawing.Size(394, 217);
            this.tabPage_title.TabIndex = 0;
            this.tabPage_title.Text = "标题";
            this.tabPage_title.UseVisualStyleBackColor = true;
            // 
            // textBox_title_typeName
            // 
            this.textBox_title_typeName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_title_typeName.Location = new System.Drawing.Point(71, 6);
            this.textBox_title_typeName.Name = "textBox_title_typeName";
            this.textBox_title_typeName.Size = new System.Drawing.Size(317, 21);
            this.textBox_title_typeName.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "类型(&T):";
            // 
            // textBox_title_comment
            // 
            this.textBox_title_comment.AcceptsReturn = true;
            this.textBox_title_comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_title_comment.HideSelection = false;
            this.textBox_title_comment.Location = new System.Drawing.Point(9, 145);
            this.textBox_title_comment.Multiline = true;
            this.textBox_title_comment.Name = "textBox_title_comment";
            this.textBox_title_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_title_comment.Size = new System.Drawing.Size(379, 62);
            this.textBox_title_comment.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 130);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "注释(&C):";
            // 
            // textBox_title_title
            // 
            this.textBox_title_title.AcceptsReturn = true;
            this.textBox_title_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_title_title.HideSelection = false;
            this.textBox_title_title.Location = new System.Drawing.Point(9, 57);
            this.textBox_title_title.Multiline = true;
            this.textBox_title_title.Name = "textBox_title_title";
            this.textBox_title_title.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_title_title.Size = new System.Drawing.Size(379, 62);
            this.textBox_title_title.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "标题文字(&T):";
            // 
            // tabPage_columns
            // 
            this.tabPage_columns.Controls.Add(this.textBox_columns_sortStyle);
            this.tabPage_columns.Controls.Add(this.label4);
            this.tabPage_columns.Controls.Add(this.listView_columns);
            this.tabPage_columns.Location = new System.Drawing.Point(4, 22);
            this.tabPage_columns.Name = "tabPage_columns";
            this.tabPage_columns.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_columns.Size = new System.Drawing.Size(394, 217);
            this.tabPage_columns.TabIndex = 1;
            this.tabPage_columns.Text = "栏目";
            this.tabPage_columns.UseVisualStyleBackColor = true;
            // 
            // textBox_columns_sortStyle
            // 
            this.textBox_columns_sortStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_columns_sortStyle.Location = new System.Drawing.Point(87, 190);
            this.textBox_columns_sortStyle.Name = "textBox_columns_sortStyle";
            this.textBox_columns_sortStyle.Size = new System.Drawing.Size(301, 21);
            this.textBox_columns_sortStyle.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1, 193);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 1;
            this.label4.Text = "排序方式(&S):";
            // 
            // listView_columns
            // 
            this.listView_columns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_columns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_title,
            this.columnHeader_dataType,
            this.columnHeader_align,
            this.columnHeader_sum,
            this.columnHeader_css,
            this.columnHeader_eval});
            this.listView_columns.FullRowSelect = true;
            this.listView_columns.HideSelection = false;
            this.listView_columns.Location = new System.Drawing.Point(3, 3);
            this.listView_columns.Name = "listView_columns";
            this.listView_columns.Size = new System.Drawing.Size(388, 181);
            this.listView_columns.TabIndex = 0;
            this.listView_columns.UseCompatibleStateImageBehavior = false;
            this.listView_columns.View = System.Windows.Forms.View.Details;
            this.listView_columns.DoubleClick += new System.EventHandler(this.listView_columns_DoubleClick);
            this.listView_columns.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_columns_MouseUp);
            // 
            // columnHeader_title
            // 
            this.columnHeader_title.Text = "标题";
            this.columnHeader_title.Width = 187;
            // 
            // columnHeader_align
            // 
            this.columnHeader_align.Text = "对齐方式";
            this.columnHeader_align.Width = 70;
            // 
            // columnHeader_sum
            // 
            this.columnHeader_sum.Text = "进行合计";
            this.columnHeader_sum.Width = 74;
            // 
            // columnHeader_css
            // 
            this.columnHeader_css.Text = "样式名";
            this.columnHeader_css.Width = 100;
            // 
            // columnHeader_eval
            // 
            this.columnHeader_eval.Text = "脚本";
            this.columnHeader_eval.Width = 100;
            // 
            // tabPage_css
            // 
            this.tabPage_css.Controls.Add(this.textBox_css_content);
            this.tabPage_css.Controls.Add(this.label3);
            this.tabPage_css.Location = new System.Drawing.Point(4, 22);
            this.tabPage_css.Name = "tabPage_css";
            this.tabPage_css.Size = new System.Drawing.Size(394, 217);
            this.tabPage_css.TabIndex = 2;
            this.tabPage_css.Text = "CSS";
            this.tabPage_css.UseVisualStyleBackColor = true;
            // 
            // textBox_css_content
            // 
            this.textBox_css_content.AcceptsReturn = true;
            this.textBox_css_content.AcceptsTab = true;
            this.textBox_css_content.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_css_content.HideSelection = false;
            this.textBox_css_content.Location = new System.Drawing.Point(3, 25);
            this.textBox_css_content.MaxLength = 0;
            this.textBox_css_content.Multiline = true;
            this.textBox_css_content.Name = "textBox_css_content";
            this.textBox_css_content.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_css_content.Size = new System.Drawing.Size(388, 175);
            this.textBox_css_content.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "CSS 内容(&C):";
            // 
            // tabPage_property
            // 
            this.tabPage_property.Controls.Add(this.checkedComboBox_property_createFreq);
            this.tabPage_property.Controls.Add(this.label6);
            this.tabPage_property.Controls.Add(this.checkBox_property_fresh);
            this.tabPage_property.Location = new System.Drawing.Point(4, 22);
            this.tabPage_property.Name = "tabPage_property";
            this.tabPage_property.Size = new System.Drawing.Size(394, 217);
            this.tabPage_property.TabIndex = 3;
            this.tabPage_property.Text = "属性";
            this.tabPage_property.UseVisualStyleBackColor = true;
            // 
            // checkedComboBox_property_createFreq
            // 
            this.checkedComboBox_property_createFreq.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_property_createFreq.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checkedComboBox_property_createFreq.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_property_createFreq.Location = new System.Drawing.Point(138, 16);
            this.checkedComboBox_property_createFreq.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_property_createFreq.Name = "checkedComboBox_property_createFreq";
            this.checkedComboBox_property_createFreq.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_property_createFreq.Size = new System.Drawing.Size(179, 24);
            this.checkedComboBox_property_createFreq.TabIndex = 18;
            this.checkedComboBox_property_createFreq.DropDown += new System.EventHandler(this.checkedComboBox_property_createFreq_DropDown);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(113, 12);
            this.label6.TabIndex = 17;
            this.label6.Text = "推荐的创建频率(&F):";
            // 
            // checkBox_property_fresh
            // 
            this.checkBox_property_fresh.AutoSize = true;
            this.checkBox_property_fresh.Location = new System.Drawing.Point(11, 70);
            this.checkBox_property_fresh.Name = "checkBox_property_fresh";
            this.checkBox_property_fresh.Size = new System.Drawing.Size(90, 16);
            this.checkBox_property_fresh.TabIndex = 0;
            this.checkBox_property_fresh.Text = "时间敏感(&F)";
            this.checkBox_property_fresh.UseVisualStyleBackColor = true;
            // 
            // columnHeader_dataType
            // 
            this.columnHeader_dataType.Text = "数据类型";
            this.columnHeader_dataType.Width = 80;
            // 
            // ReportDefForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(427, 297);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "ReportDefForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "报表配置文件";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReportDefForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ReportDefForm_FormClosed);
            this.Load += new System.EventHandler(this.ReportDefForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_title.ResumeLayout(false);
            this.tabPage_title.PerformLayout();
            this.tabPage_columns.ResumeLayout(false);
            this.tabPage_columns.PerformLayout();
            this.tabPage_css.ResumeLayout(false);
            this.tabPage_css.PerformLayout();
            this.tabPage_property.ResumeLayout(false);
            this.tabPage_property.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_title;
        private System.Windows.Forms.TabPage tabPage_columns;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_title_title;
        private System.Windows.Forms.TextBox textBox_title_comment;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listView_columns;
        private System.Windows.Forms.ColumnHeader columnHeader_title;
        private System.Windows.Forms.ColumnHeader columnHeader_align;
        private System.Windows.Forms.ColumnHeader columnHeader_sum;
        private System.Windows.Forms.TabPage tabPage_css;
        private System.Windows.Forms.TextBox textBox_css_content;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ColumnHeader columnHeader_css;
        private System.Windows.Forms.TextBox textBox_columns_sortStyle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_title_typeName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabPage tabPage_property;
        private System.Windows.Forms.CheckBox checkBox_property_fresh;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_property_createFreq;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ColumnHeader columnHeader_eval;
        private System.Windows.Forms.ColumnHeader columnHeader_dataType;
    }
}