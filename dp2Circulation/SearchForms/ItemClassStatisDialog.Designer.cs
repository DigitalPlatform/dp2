namespace dp2Circulation
{
    partial class ItemClassStatisDialog
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
            this.label2 = new System.Windows.Forms.Label();
            this.button_getOutputExcelFileName = new System.Windows.Forms.Button();
            this.textBox_outputExcelFileName = new System.Windows.Forms.TextBox();
            this.comboBox_classType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_price = new System.Windows.Forms.CheckBox();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_general = new System.Windows.Forms.TabPage();
            this.tabPage_property = new System.Windows.Forms.TabPage();
            this.label_classTitle = new System.Windows.Forms.Label();
            this.textBox_classTitle = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_setCommonValue = new System.Windows.Forms.Button();
            this.tabControl_main.SuspendLayout();
            this.tabPage_general.SuspendLayout();
            this.tabPage_property.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Excel 文件名(&F):";
            // 
            // button_getOutputExcelFileName
            // 
            this.button_getOutputExcelFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getOutputExcelFileName.Location = new System.Drawing.Point(325, 24);
            this.button_getOutputExcelFileName.Name = "button_getOutputExcelFileName";
            this.button_getOutputExcelFileName.Size = new System.Drawing.Size(44, 23);
            this.button_getOutputExcelFileName.TabIndex = 5;
            this.button_getOutputExcelFileName.Text = "...";
            this.button_getOutputExcelFileName.UseVisualStyleBackColor = true;
            this.button_getOutputExcelFileName.Click += new System.EventHandler(this.button_getOutputExcelFileName_Click);
            // 
            // textBox_outputExcelFileName
            // 
            this.textBox_outputExcelFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_outputExcelFileName.Location = new System.Drawing.Point(6, 26);
            this.textBox_outputExcelFileName.Name = "textBox_outputExcelFileName";
            this.textBox_outputExcelFileName.Size = new System.Drawing.Size(313, 21);
            this.textBox_outputExcelFileName.TabIndex = 4;
            // 
            // comboBox_classType
            // 
            this.comboBox_classType.FormattingEnabled = true;
            this.comboBox_classType.Items.AddRange(new object[] {
            "中图法",
            "科图法",
            "人大法",
            "石头汤分类法",
            "DDC",
            "UDC",
            "LCC",
            "其它"});
            this.comboBox_classType.Location = new System.Drawing.Point(6, 83);
            this.comboBox_classType.Name = "comboBox_classType";
            this.comboBox_classType.Size = new System.Drawing.Size(201, 20);
            this.comboBox_classType.TabIndex = 6;
            this.comboBox_classType.SelectedIndexChanged += new System.EventHandler(this.comboBox_classType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "分类法(&C):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(320, 282);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 9;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(241, 282);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 8;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_price
            // 
            this.checkBox_price.AutoSize = true;
            this.checkBox_price.Location = new System.Drawing.Point(6, 138);
            this.checkBox_price.Name = "checkBox_price";
            this.checkBox_price.Size = new System.Drawing.Size(78, 16);
            this.checkBox_price.TabIndex = 10;
            this.checkBox_price.Text = "价格列(&P)";
            this.checkBox_price.UseVisualStyleBackColor = true;
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_general);
            this.tabControl_main.Controls.Add(this.tabPage_property);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(383, 265);
            this.tabControl_main.TabIndex = 11;
            // 
            // tabPage_general
            // 
            this.tabPage_general.AutoScroll = true;
            this.tabPage_general.Controls.Add(this.checkBox_price);
            this.tabPage_general.Controls.Add(this.comboBox_classType);
            this.tabPage_general.Controls.Add(this.label1);
            this.tabPage_general.Controls.Add(this.label2);
            this.tabPage_general.Controls.Add(this.textBox_outputExcelFileName);
            this.tabPage_general.Controls.Add(this.button_getOutputExcelFileName);
            this.tabPage_general.Location = new System.Drawing.Point(4, 22);
            this.tabPage_general.Name = "tabPage_general";
            this.tabPage_general.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_general.Size = new System.Drawing.Size(375, 239);
            this.tabPage_general.TabIndex = 0;
            this.tabPage_general.Text = "开始";
            this.tabPage_general.UseVisualStyleBackColor = true;
            // 
            // tabPage_property
            // 
            this.tabPage_property.Controls.Add(this.button_setCommonValue);
            this.tabPage_property.Controls.Add(this.label3);
            this.tabPage_property.Controls.Add(this.label_classTitle);
            this.tabPage_property.Controls.Add(this.textBox_classTitle);
            this.tabPage_property.Location = new System.Drawing.Point(4, 22);
            this.tabPage_property.Name = "tabPage_property";
            this.tabPage_property.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_property.Size = new System.Drawing.Size(375, 239);
            this.tabPage_property.TabIndex = 1;
            this.tabPage_property.Text = "特性";
            this.tabPage_property.UseVisualStyleBackColor = true;
            // 
            // label_classTitle
            // 
            this.label_classTitle.AutoSize = true;
            this.label_classTitle.Location = new System.Drawing.Point(6, 16);
            this.label_classTitle.Name = "label_classTitle";
            this.label_classTitle.Size = new System.Drawing.Size(71, 12);
            this.label_classTitle.TabIndex = 5;
            this.label_classTitle.Text = "类目栏内容:";
            // 
            // textBox_classTitle
            // 
            this.textBox_classTitle.AcceptsReturn = true;
            this.textBox_classTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_classTitle.Location = new System.Drawing.Point(8, 31);
            this.textBox_classTitle.Multiline = true;
            this.textBox_classTitle.Name = "textBox_classTitle";
            this.textBox_classTitle.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_classTitle.Size = new System.Drawing.Size(361, 173);
            this.textBox_classTitle.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(292, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "注: 每行一个";
            // 
            // button_setCommonValue
            // 
            this.button_setCommonValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_setCommonValue.Location = new System.Drawing.Point(254, 210);
            this.button_setCommonValue.Name = "button_setCommonValue";
            this.button_setCommonValue.Size = new System.Drawing.Size(115, 23);
            this.button_setCommonValue.TabIndex = 8;
            this.button_setCommonValue.Text = "设为常用值";
            this.button_setCommonValue.UseVisualStyleBackColor = true;
            this.button_setCommonValue.Click += new System.EventHandler(this.button_setCommonValue_Click);
            // 
            // ItemClassStatisDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(406, 316);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "ItemClassStatisDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "册分类统计";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ItemClassStatisDialog_FormClosed);
            this.Load += new System.EventHandler(this.ItemClassStatisDialog_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_general.ResumeLayout(false);
            this.tabPage_general.PerformLayout();
            this.tabPage_property.ResumeLayout(false);
            this.tabPage_property.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_getOutputExcelFileName;
        private System.Windows.Forms.TextBox textBox_outputExcelFileName;
        private System.Windows.Forms.ComboBox comboBox_classType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_price;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_general;
        private System.Windows.Forms.TabPage tabPage_property;
        private System.Windows.Forms.Label label_classTitle;
        private System.Windows.Forms.TextBox textBox_classTitle;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_setCommonValue;
    }
}