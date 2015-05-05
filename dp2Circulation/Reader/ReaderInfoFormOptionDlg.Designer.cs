namespace dp2Circulation
{
    partial class ReaderInfoFormOptionDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReaderInfoFormOptionDlg));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_newReaderDefault = new System.Windows.Forms.TabPage();
            this.splitContainer_newReaderDefault = new System.Windows.Forms.SplitContainer();
            this.textBox_newReaderDefaulComment = new System.Windows.Forms.TextBox();
            this.readerEditControl_newReaderDefault = new dp2Circulation.ReaderEditControl();
            this.tabPage_cardPhoto = new System.Windows.Forms.TabPage();
            this.textBox_cardPhoto_maxWidth = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_idCard = new System.Windows.Forms.TabPage();
            this.groupBox_idCardFieldsSelection = new System.Windows.Forms.GroupBox();
            this.button_idCard_clearAll = new System.Windows.Forms.Button();
            this.button_idCard_selectAll = new System.Windows.Forms.Button();
            this.checkBox_idCard_selectPhoto = new System.Windows.Forms.CheckBox();
            this.checkBox_idCard_selectValidateRange = new System.Windows.Forms.CheckBox();
            this.checkBox_idCard_selectAgency = new System.Windows.Forms.CheckBox();
            this.checkBox_idCard_selectIdCardNumber = new System.Windows.Forms.CheckBox();
            this.checkBox_idCard_selectAddress = new System.Windows.Forms.CheckBox();
            this.checkBox_idCard_selectDateOfBirth = new System.Windows.Forms.CheckBox();
            this.checkBox_idCard_selectNation = new System.Windows.Forms.CheckBox();
            this.checkBox_idCard_selectGender = new System.Windows.Forms.CheckBox();
            this.checkBox_idCard_selectName = new System.Windows.Forms.CheckBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_newReaderDefault.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_newReaderDefault)).BeginInit();
            this.splitContainer_newReaderDefault.Panel1.SuspendLayout();
            this.splitContainer_newReaderDefault.Panel2.SuspendLayout();
            this.splitContainer_newReaderDefault.SuspendLayout();
            this.tabPage_cardPhoto.SuspendLayout();
            this.tabPage_idCard.SuspendLayout();
            this.groupBox_idCardFieldsSelection.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(224, 291);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(145, 291);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 0;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_newReaderDefault);
            this.tabControl_main.Controls.Add(this.tabPage_cardPhoto);
            this.tabControl_main.Controls.Add(this.tabPage_idCard);
            this.tabControl_main.Location = new System.Drawing.Point(11, 11);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(288, 276);
            this.tabControl_main.TabIndex = 3;
            // 
            // tabPage_newReaderDefault
            // 
            this.tabPage_newReaderDefault.Controls.Add(this.splitContainer_newReaderDefault);
            this.tabPage_newReaderDefault.Location = new System.Drawing.Point(4, 22);
            this.tabPage_newReaderDefault.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_newReaderDefault.Name = "tabPage_newReaderDefault";
            this.tabPage_newReaderDefault.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_newReaderDefault.Size = new System.Drawing.Size(280, 250);
            this.tabPage_newReaderDefault.TabIndex = 0;
            this.tabPage_newReaderDefault.Text = " 读者信息缺省值 ";
            this.tabPage_newReaderDefault.UseVisualStyleBackColor = true;
            // 
            // splitContainer_newReaderDefault
            // 
            this.splitContainer_newReaderDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_newReaderDefault.Location = new System.Drawing.Point(2, 2);
            this.splitContainer_newReaderDefault.Name = "splitContainer_newReaderDefault";
            this.splitContainer_newReaderDefault.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_newReaderDefault.Panel1
            // 
            this.splitContainer_newReaderDefault.Panel1.Controls.Add(this.textBox_newReaderDefaulComment);
            // 
            // splitContainer_newReaderDefault.Panel2
            // 
            this.splitContainer_newReaderDefault.Panel2.Controls.Add(this.readerEditControl_newReaderDefault);
            this.splitContainer_newReaderDefault.Size = new System.Drawing.Size(276, 246);
            this.splitContainer_newReaderDefault.SplitterDistance = 63;
            this.splitContainer_newReaderDefault.SplitterWidth = 8;
            this.splitContainer_newReaderDefault.TabIndex = 1;
            // 
            // textBox_newReaderDefaulComment
            // 
            this.textBox_newReaderDefaulComment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_newReaderDefaulComment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_newReaderDefaulComment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_newReaderDefaulComment.Location = new System.Drawing.Point(0, 0);
            this.textBox_newReaderDefaulComment.Multiline = true;
            this.textBox_newReaderDefaulComment.Name = "textBox_newReaderDefaulComment";
            this.textBox_newReaderDefaulComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_newReaderDefaulComment.Size = new System.Drawing.Size(276, 63);
            this.textBox_newReaderDefaulComment.TabIndex = 0;
            this.textBox_newReaderDefaulComment.Text = "当读者信息窗装载空白记录的时候，可以使用服务器中读者库下的cfgs/template配置文件中的模板记录，或者这里定义的缺省值，一共两种方式";
            // 
            // readerEditControl_newReaderDefault
            // 
            this.readerEditControl_newReaderDefault.Address = "";
            this.readerEditControl_newReaderDefault.Barcode = "";
            this.readerEditControl_newReaderDefault.CardNumber = "";
            this.readerEditControl_newReaderDefault.Changed = false;
            this.readerEditControl_newReaderDefault.Comment = "";
            this.readerEditControl_newReaderDefault.CreateDate = "Wed, 04 Oct 2006 00:00:00 +0800";
            this.readerEditControl_newReaderDefault.DateOfBirth = "Wed, 04 Oct 2006 00:00:00 +0800";
            this.readerEditControl_newReaderDefault.Department = "";
            this.readerEditControl_newReaderDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.readerEditControl_newReaderDefault.Email = "";
            this.readerEditControl_newReaderDefault.ExpireDate = "Wed, 04 Oct 2006 00:00:00 +0800";
            this.readerEditControl_newReaderDefault.Foregift = "";
            this.readerEditControl_newReaderDefault.Gender = "";
            this.readerEditControl_newReaderDefault.HireExpireDate = "";
            this.readerEditControl_newReaderDefault.HirePeriod = "";
            this.readerEditControl_newReaderDefault.IdCardNumber = "";
            this.readerEditControl_newReaderDefault.Initializing = true;
            this.readerEditControl_newReaderDefault.Location = new System.Drawing.Point(0, 0);
            this.readerEditControl_newReaderDefault.Margin = new System.Windows.Forms.Padding(2);
            this.readerEditControl_newReaderDefault.Name = "readerEditControl_newReaderDefault";
            this.readerEditControl_newReaderDefault.NameString = "";
            this.readerEditControl_newReaderDefault.Post = "";
            this.readerEditControl_newReaderDefault.ReaderType = "";
            this.readerEditControl_newReaderDefault.RecPath = "";
            this.readerEditControl_newReaderDefault.Size = new System.Drawing.Size(276, 175);
            this.readerEditControl_newReaderDefault.State = "";
            this.readerEditControl_newReaderDefault.TabIndex = 0;
            this.readerEditControl_newReaderDefault.Tel = "";
            this.readerEditControl_newReaderDefault.GetLibraryCode += new dp2Circulation.GetLibraryCodeEventHandler(this.readerEditControl_newReaderDefault_GetLibraryCode);
            // 
            // tabPage_cardPhoto
            // 
            this.tabPage_cardPhoto.AutoScroll = true;
            this.tabPage_cardPhoto.Controls.Add(this.textBox_cardPhoto_maxWidth);
            this.tabPage_cardPhoto.Controls.Add(this.label1);
            this.tabPage_cardPhoto.Location = new System.Drawing.Point(4, 22);
            this.tabPage_cardPhoto.Name = "tabPage_cardPhoto";
            this.tabPage_cardPhoto.Size = new System.Drawing.Size(280, 250);
            this.tabPage_cardPhoto.TabIndex = 1;
            this.tabPage_cardPhoto.Text = "证件照";
            this.tabPage_cardPhoto.UseVisualStyleBackColor = true;
            // 
            // textBox_cardPhoto_maxWidth
            // 
            this.textBox_cardPhoto_maxWidth.Location = new System.Drawing.Point(181, 18);
            this.textBox_cardPhoto_maxWidth.Name = "textBox_cardPhoto_maxWidth";
            this.textBox_cardPhoto_maxWidth.Size = new System.Drawing.Size(89, 21);
            this.textBox_cardPhoto_maxWidth.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(173, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "自动限定图片宽度[像素数](&W):";
            // 
            // tabPage_idCard
            // 
            this.tabPage_idCard.AutoScroll = true;
            this.tabPage_idCard.Controls.Add(this.groupBox_idCardFieldsSelection);
            this.tabPage_idCard.Location = new System.Drawing.Point(4, 22);
            this.tabPage_idCard.Name = "tabPage_idCard";
            this.tabPage_idCard.Size = new System.Drawing.Size(280, 250);
            this.tabPage_idCard.TabIndex = 2;
            this.tabPage_idCard.Text = "身份证信息";
            this.tabPage_idCard.UseVisualStyleBackColor = true;
            // 
            // groupBox_idCardFieldsSelection
            // 
            this.groupBox_idCardFieldsSelection.Controls.Add(this.button_idCard_clearAll);
            this.groupBox_idCardFieldsSelection.Controls.Add(this.button_idCard_selectAll);
            this.groupBox_idCardFieldsSelection.Controls.Add(this.checkBox_idCard_selectPhoto);
            this.groupBox_idCardFieldsSelection.Controls.Add(this.checkBox_idCard_selectValidateRange);
            this.groupBox_idCardFieldsSelection.Controls.Add(this.checkBox_idCard_selectAgency);
            this.groupBox_idCardFieldsSelection.Controls.Add(this.checkBox_idCard_selectIdCardNumber);
            this.groupBox_idCardFieldsSelection.Controls.Add(this.checkBox_idCard_selectAddress);
            this.groupBox_idCardFieldsSelection.Controls.Add(this.checkBox_idCard_selectDateOfBirth);
            this.groupBox_idCardFieldsSelection.Controls.Add(this.checkBox_idCard_selectNation);
            this.groupBox_idCardFieldsSelection.Controls.Add(this.checkBox_idCard_selectGender);
            this.groupBox_idCardFieldsSelection.Controls.Add(this.checkBox_idCard_selectName);
            this.groupBox_idCardFieldsSelection.Location = new System.Drawing.Point(3, 19);
            this.groupBox_idCardFieldsSelection.Name = "groupBox_idCardFieldsSelection";
            this.groupBox_idCardFieldsSelection.Size = new System.Drawing.Size(255, 191);
            this.groupBox_idCardFieldsSelection.TabIndex = 0;
            this.groupBox_idCardFieldsSelection.TabStop = false;
            this.groupBox_idCardFieldsSelection.Text = "导入身份证时选用下列字段";
            // 
            // button_idCard_clearAll
            // 
            this.button_idCard_clearAll.Location = new System.Drawing.Point(103, 149);
            this.button_idCard_clearAll.Name = "button_idCard_clearAll";
            this.button_idCard_clearAll.Size = new System.Drawing.Size(75, 23);
            this.button_idCard_clearAll.TabIndex = 10;
            this.button_idCard_clearAll.Text = "全清除";
            this.button_idCard_clearAll.UseVisualStyleBackColor = true;
            this.button_idCard_clearAll.Click += new System.EventHandler(this.button_idCard_clearAll_Click_1);
            // 
            // button_idCard_selectAll
            // 
            this.button_idCard_selectAll.Location = new System.Drawing.Point(23, 149);
            this.button_idCard_selectAll.Margin = new System.Windows.Forms.Padding(2);
            this.button_idCard_selectAll.Name = "button_idCard_selectAll";
            this.button_idCard_selectAll.Size = new System.Drawing.Size(75, 23);
            this.button_idCard_selectAll.TabIndex = 9;
            this.button_idCard_selectAll.Text = "全选";
            this.button_idCard_selectAll.UseVisualStyleBackColor = true;
            this.button_idCard_selectAll.Click += new System.EventHandler(this.button_idCard_selectAll_Click);
            // 
            // checkBox_idCard_selectPhoto
            // 
            this.checkBox_idCard_selectPhoto.AutoSize = true;
            this.checkBox_idCard_selectPhoto.Location = new System.Drawing.Point(142, 121);
            this.checkBox_idCard_selectPhoto.Name = "checkBox_idCard_selectPhoto";
            this.checkBox_idCard_selectPhoto.Size = new System.Drawing.Size(66, 16);
            this.checkBox_idCard_selectPhoto.TabIndex = 8;
            this.checkBox_idCard_selectPhoto.Text = "照片(&P)";
            this.checkBox_idCard_selectPhoto.UseVisualStyleBackColor = true;
            // 
            // checkBox_idCard_selectValidateRange
            // 
            this.checkBox_idCard_selectValidateRange.AutoSize = true;
            this.checkBox_idCard_selectValidateRange.Location = new System.Drawing.Point(142, 77);
            this.checkBox_idCard_selectValidateRange.Name = "checkBox_idCard_selectValidateRange";
            this.checkBox_idCard_selectValidateRange.Size = new System.Drawing.Size(90, 16);
            this.checkBox_idCard_selectValidateRange.TabIndex = 7;
            this.checkBox_idCard_selectValidateRange.Text = "有效期限(&V)";
            this.checkBox_idCard_selectValidateRange.UseVisualStyleBackColor = true;
            // 
            // checkBox_idCard_selectAgency
            // 
            this.checkBox_idCard_selectAgency.AutoSize = true;
            this.checkBox_idCard_selectAgency.Location = new System.Drawing.Point(142, 55);
            this.checkBox_idCard_selectAgency.Name = "checkBox_idCard_selectAgency";
            this.checkBox_idCard_selectAgency.Size = new System.Drawing.Size(90, 16);
            this.checkBox_idCard_selectAgency.TabIndex = 6;
            this.checkBox_idCard_selectAgency.Text = "签发机关(&A)";
            this.checkBox_idCard_selectAgency.UseVisualStyleBackColor = true;
            // 
            // checkBox_idCard_selectIdCardNumber
            // 
            this.checkBox_idCard_selectIdCardNumber.AutoSize = true;
            this.checkBox_idCard_selectIdCardNumber.Location = new System.Drawing.Point(142, 33);
            this.checkBox_idCard_selectIdCardNumber.Name = "checkBox_idCard_selectIdCardNumber";
            this.checkBox_idCard_selectIdCardNumber.Size = new System.Drawing.Size(90, 16);
            this.checkBox_idCard_selectIdCardNumber.TabIndex = 5;
            this.checkBox_idCard_selectIdCardNumber.Text = "身份证号(&U)";
            this.checkBox_idCard_selectIdCardNumber.UseVisualStyleBackColor = true;
            // 
            // checkBox_idCard_selectAddress
            // 
            this.checkBox_idCard_selectAddress.AutoSize = true;
            this.checkBox_idCard_selectAddress.Location = new System.Drawing.Point(23, 121);
            this.checkBox_idCard_selectAddress.Name = "checkBox_idCard_selectAddress";
            this.checkBox_idCard_selectAddress.Size = new System.Drawing.Size(66, 16);
            this.checkBox_idCard_selectAddress.TabIndex = 4;
            this.checkBox_idCard_selectAddress.Text = "住址(&A)";
            this.checkBox_idCard_selectAddress.UseVisualStyleBackColor = true;
            // 
            // checkBox_idCard_selectDateOfBirth
            // 
            this.checkBox_idCard_selectDateOfBirth.AutoSize = true;
            this.checkBox_idCard_selectDateOfBirth.Location = new System.Drawing.Point(23, 99);
            this.checkBox_idCard_selectDateOfBirth.Name = "checkBox_idCard_selectDateOfBirth";
            this.checkBox_idCard_selectDateOfBirth.Size = new System.Drawing.Size(90, 16);
            this.checkBox_idCard_selectDateOfBirth.TabIndex = 3;
            this.checkBox_idCard_selectDateOfBirth.Text = "出生日期(&B)";
            this.checkBox_idCard_selectDateOfBirth.UseVisualStyleBackColor = true;
            // 
            // checkBox_idCard_selectNation
            // 
            this.checkBox_idCard_selectNation.AutoSize = true;
            this.checkBox_idCard_selectNation.Location = new System.Drawing.Point(23, 77);
            this.checkBox_idCard_selectNation.Name = "checkBox_idCard_selectNation";
            this.checkBox_idCard_selectNation.Size = new System.Drawing.Size(66, 16);
            this.checkBox_idCard_selectNation.TabIndex = 2;
            this.checkBox_idCard_selectNation.Text = "民族(&A)";
            this.checkBox_idCard_selectNation.UseVisualStyleBackColor = true;
            // 
            // checkBox_idCard_selectGender
            // 
            this.checkBox_idCard_selectGender.AutoSize = true;
            this.checkBox_idCard_selectGender.Location = new System.Drawing.Point(23, 55);
            this.checkBox_idCard_selectGender.Name = "checkBox_idCard_selectGender";
            this.checkBox_idCard_selectGender.Size = new System.Drawing.Size(66, 16);
            this.checkBox_idCard_selectGender.TabIndex = 1;
            this.checkBox_idCard_selectGender.Text = "性别(&G)";
            this.checkBox_idCard_selectGender.UseVisualStyleBackColor = true;
            // 
            // checkBox_idCard_selectName
            // 
            this.checkBox_idCard_selectName.AutoSize = true;
            this.checkBox_idCard_selectName.Location = new System.Drawing.Point(23, 33);
            this.checkBox_idCard_selectName.Name = "checkBox_idCard_selectName";
            this.checkBox_idCard_selectName.Size = new System.Drawing.Size(66, 16);
            this.checkBox_idCard_selectName.TabIndex = 0;
            this.checkBox_idCard_selectName.Text = "姓名(&N)";
            this.checkBox_idCard_selectName.UseVisualStyleBackColor = true;
            // 
            // ReaderInfoFormOptionDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(310, 325);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ReaderInfoFormOptionDlg";
            this.ShowInTaskbar = false;
            this.Text = "选项";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ReaderInfoFormOptionDlg_FormClosed);
            this.Load += new System.EventHandler(this.ReaderInfoFormOptionDlg_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_newReaderDefault.ResumeLayout(false);
            this.splitContainer_newReaderDefault.Panel1.ResumeLayout(false);
            this.splitContainer_newReaderDefault.Panel1.PerformLayout();
            this.splitContainer_newReaderDefault.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_newReaderDefault)).EndInit();
            this.splitContainer_newReaderDefault.ResumeLayout(false);
            this.tabPage_cardPhoto.ResumeLayout(false);
            this.tabPage_cardPhoto.PerformLayout();
            this.tabPage_idCard.ResumeLayout(false);
            this.groupBox_idCardFieldsSelection.ResumeLayout(false);
            this.groupBox_idCardFieldsSelection.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_newReaderDefault;
        private ReaderEditControl readerEditControl_newReaderDefault;
        private System.Windows.Forms.SplitContainer splitContainer_newReaderDefault;
        private System.Windows.Forms.TextBox textBox_newReaderDefaulComment;
        private System.Windows.Forms.TabPage tabPage_cardPhoto;
        private System.Windows.Forms.TextBox textBox_cardPhoto_maxWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage_idCard;
        private System.Windows.Forms.GroupBox groupBox_idCardFieldsSelection;
        private System.Windows.Forms.CheckBox checkBox_idCard_selectValidateRange;
        private System.Windows.Forms.CheckBox checkBox_idCard_selectAgency;
        private System.Windows.Forms.CheckBox checkBox_idCard_selectIdCardNumber;
        private System.Windows.Forms.CheckBox checkBox_idCard_selectAddress;
        private System.Windows.Forms.CheckBox checkBox_idCard_selectDateOfBirth;
        private System.Windows.Forms.CheckBox checkBox_idCard_selectNation;
        private System.Windows.Forms.CheckBox checkBox_idCard_selectGender;
        private System.Windows.Forms.CheckBox checkBox_idCard_selectName;
        private System.Windows.Forms.CheckBox checkBox_idCard_selectPhoto;
        private System.Windows.Forms.Button button_idCard_selectAll;
        private System.Windows.Forms.Button button_idCard_clearAll;
    }
}