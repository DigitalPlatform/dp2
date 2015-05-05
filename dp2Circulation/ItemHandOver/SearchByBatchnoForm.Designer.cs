namespace dp2Circulation
{
    partial class SearchByBatchnoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchByBatchnoForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label_location = new System.Windows.Forms.Label();
            this.comboBox_location = new System.Windows.Forms.ComboBox();
            this.button_search = new System.Windows.Forms.Button();
            this.comboBox_batchNo = new DigitalPlatform.CommonControl.TabComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "批次号(&B):";
            // 
            // label_location
            // 
            this.label_location.AutoSize = true;
            this.label_location.Location = new System.Drawing.Point(12, 48);
            this.label_location.Name = "label_location";
            this.label_location.Size = new System.Drawing.Size(99, 15);
            this.label_location.TabIndex = 2;
            this.label_location.Text = "馆藏地点(&L):";
            // 
            // comboBox_location
            // 
            this.comboBox_location.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_location.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_location.FormattingEnabled = true;
            this.comboBox_location.Location = new System.Drawing.Point(128, 45);
            this.comboBox_location.Name = "comboBox_location";
            this.comboBox_location.Size = new System.Drawing.Size(186, 23);
            this.comboBox_location.TabIndex = 3;
            this.comboBox_location.SizeChanged += new System.EventHandler(this.comboBox_location_SizeChanged);
            this.comboBox_location.DropDown += new System.EventHandler(this.comboBox_location_DropDown);
            // 
            // button_search
            // 
            this.button_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_search.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_search.Image = ((System.Drawing.Image)(resources.GetObject("button_search.Image")));
            this.button_search.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.button_search.Location = new System.Drawing.Point(320, 12);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(94, 28);
            this.button_search.TabIndex = 4;
            this.button_search.Text = "检索(&S)";
            this.button_search.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // comboBox_batchNo
            // 
            this.comboBox_batchNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_batchNo.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBox_batchNo.DropDownHeight = 300;
            this.comboBox_batchNo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_batchNo.FormattingEnabled = true;
            this.comboBox_batchNo.IntegralHeight = false;
            this.comboBox_batchNo.LeftFontStyle = System.Drawing.FontStyle.Bold;
            this.comboBox_batchNo.Location = new System.Drawing.Point(128, 12);
            this.comboBox_batchNo.Name = "comboBox_batchNo";
            this.comboBox_batchNo.RightFontStyle = System.Drawing.FontStyle.Italic;
            this.comboBox_batchNo.Size = new System.Drawing.Size(186, 26);
            this.comboBox_batchNo.TabIndex = 5;
            this.comboBox_batchNo.SizeChanged += new System.EventHandler(this.comboBox_batchNo_SizeChanged);
            this.comboBox_batchNo.DropDown += new System.EventHandler(this.comboBox_batchNo_DropDown);
            // 
            // SearchByBatchnoForm
            // 
            this.AcceptButton = this.button_search;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 137);
            this.Controls.Add(this.comboBox_batchNo);
            this.Controls.Add(this.button_search);
            this.Controls.Add(this.comboBox_location);
            this.Controls.Add(this.label_location);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SearchByBatchnoForm";
            this.ShowInTaskbar = false;
            this.Text = "根据批次号检索出册";
            this.Load += new System.EventHandler(this.SearchByBatchnoForm_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SearchByBatchnoForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_location;
        private System.Windows.Forms.ComboBox comboBox_location;
        private System.Windows.Forms.Button button_search;
        private DigitalPlatform.CommonControl.TabComboBox comboBox_batchNo;
    }
}