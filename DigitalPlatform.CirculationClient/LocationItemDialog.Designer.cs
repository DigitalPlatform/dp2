namespace DigitalPlatform.CirculationClient
{
    partial class LocationItemDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LocationItemDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_location = new System.Windows.Forms.TextBox();
            this.checkBox_canBorrow = new System.Windows.Forms.CheckBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_libraryCode = new System.Windows.Forms.ComboBox();
            this.checkBox_itemBarcodeNullable = new System.Windows.Forms.CheckBox();
            this.textBox_scriptCanBorrow = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_scriptCanReturn = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 18);
            this.label1.TabIndex = 2;
            this.label1.Text = "馆藏地名称(&N):";
            // 
            // textBox_location
            // 
            this.textBox_location.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_location.Location = new System.Drawing.Point(178, 54);
            this.textBox_location.Name = "textBox_location";
            this.textBox_location.Size = new System.Drawing.Size(372, 28);
            this.textBox_location.TabIndex = 3;
            // 
            // checkBox_canBorrow
            // 
            this.checkBox_canBorrow.AutoSize = true;
            this.checkBox_canBorrow.Location = new System.Drawing.Point(19, 154);
            this.checkBox_canBorrow.Name = "checkBox_canBorrow";
            this.checkBox_canBorrow.Size = new System.Drawing.Size(133, 22);
            this.checkBox_canBorrow.TabIndex = 5;
            this.checkBox_canBorrow.Text = "允许外借(&B)";
            this.checkBox_canBorrow.UseVisualStyleBackColor = true;
            this.checkBox_canBorrow.CheckedChanged += new System.EventHandler(this.checkBox_canBorrow_CheckedChanged);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(468, 489);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(84, 33);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(378, 489);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(84, 33);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "馆代码(&L):";
            // 
            // comboBox_libraryCode
            // 
            this.comboBox_libraryCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_libraryCode.FormattingEnabled = true;
            this.comboBox_libraryCode.Location = new System.Drawing.Point(178, 18);
            this.comboBox_libraryCode.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_libraryCode.Name = "comboBox_libraryCode";
            this.comboBox_libraryCode.Size = new System.Drawing.Size(372, 26);
            this.comboBox_libraryCode.TabIndex = 1;
            // 
            // checkBox_itemBarcodeNullable
            // 
            this.checkBox_itemBarcodeNullable.AutoSize = true;
            this.checkBox_itemBarcodeNullable.Location = new System.Drawing.Point(20, 111);
            this.checkBox_itemBarcodeNullable.Name = "checkBox_itemBarcodeNullable";
            this.checkBox_itemBarcodeNullable.Size = new System.Drawing.Size(187, 22);
            this.checkBox_itemBarcodeNullable.TabIndex = 4;
            this.checkBox_itemBarcodeNullable.Text = "册条码号可为空(&N)";
            this.checkBox_itemBarcodeNullable.UseVisualStyleBackColor = true;
            // 
            // textBox_scriptCanBorrow
            // 
            this.textBox_scriptCanBorrow.AcceptsReturn = true;
            this.textBox_scriptCanBorrow.AcceptsTab = true;
            this.textBox_scriptCanBorrow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_scriptCanBorrow.Location = new System.Drawing.Point(0, 0);
            this.textBox_scriptCanBorrow.Multiline = true;
            this.textBox_scriptCanBorrow.Name = "textBox_scriptCanBorrow";
            this.textBox_scriptCanBorrow.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_scriptCanBorrow.Size = new System.Drawing.Size(531, 135);
            this.textBox_scriptCanBorrow.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(476, 158);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 18);
            this.label3.TabIndex = 9;
            this.label3.Text = "脚本(&S):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-2, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(161, 18);
            this.label4.TabIndex = 11;
            this.label4.Text = "允许还回 脚本(&R):";
            // 
            // textBox_scriptCanReturn
            // 
            this.textBox_scriptCanReturn.AcceptsReturn = true;
            this.textBox_scriptCanReturn.AcceptsTab = true;
            this.textBox_scriptCanReturn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_scriptCanReturn.Location = new System.Drawing.Point(0, 24);
            this.textBox_scriptCanReturn.Multiline = true;
            this.textBox_scriptCanReturn.Name = "textBox_scriptCanReturn";
            this.textBox_scriptCanReturn.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_scriptCanReturn.Size = new System.Drawing.Size(531, 129);
            this.textBox_scriptCanReturn.TabIndex = 10;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(19, 187);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.textBox_scriptCanBorrow);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(531, 296);
            this.splitContainer1.SplitterDistance = 135;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 12;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.textBox_scriptCanReturn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(531, 153);
            this.panel1.TabIndex = 13;
            // 
            // LocationItemDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(568, 538);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBox_itemBarcodeNullable);
            this.Controls.Add(this.comboBox_libraryCode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_canBorrow);
            this.Controls.Add(this.textBox_location);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LocationItemDialog";
            this.ShowInTaskbar = false;
            this.Text = "馆藏地";
            this.Load += new System.EventHandler(this.LocationItemDialog_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_location;
        private System.Windows.Forms.CheckBox checkBox_canBorrow;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_libraryCode;
        private System.Windows.Forms.CheckBox checkBox_itemBarcodeNullable;
        private System.Windows.Forms.TextBox textBox_scriptCanBorrow;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_scriptCanReturn;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
    }
}