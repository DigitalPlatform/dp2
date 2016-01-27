namespace dp2Circulation
{
    partial class BookshelfForm
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

            if (this.Channel != null)
                this.Channel.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BookshelfForm));
            this.label1 = new System.Windows.Forms.Label();
            this.tabComboBox_batchNo = new DigitalPlatform.CommonControl.TabComboBox();
            this.button_create = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "编目批次号(&B):";
            // 
            // tabComboBox_batchNo
            // 
            this.tabComboBox_batchNo.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.tabComboBox_batchNo.FormattingEnabled = true;
            this.tabComboBox_batchNo.Location = new System.Drawing.Point(133, 13);
            this.tabComboBox_batchNo.Name = "tabComboBox_batchNo";
            this.tabComboBox_batchNo.Size = new System.Drawing.Size(208, 26);
            this.tabComboBox_batchNo.TabIndex = 1;
            // 
            // button_create
            // 
            this.button_create.Location = new System.Drawing.Point(348, 13);
            this.button_create.Name = "button_create";
            this.button_create.Size = new System.Drawing.Size(99, 28);
            this.button_create.TabIndex = 2;
            this.button_create.Text = "创建(&C)";
            this.button_create.UseVisualStyleBackColor = true;
            this.button_create.Click += new System.EventHandler(this.button_create_Click);
            // 
            // BookshelfForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 267);
            this.Controls.Add(this.button_create);
            this.Controls.Add(this.tabComboBox_batchNo);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BookshelfForm";
            this.Text = "新书通报";
            this.Load += new System.EventHandler(this.BookshelfForm_Load);
            this.Activated += new System.EventHandler(this.BookshelfForm_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BookshelfForm_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BookshelfForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private DigitalPlatform.CommonControl.TabComboBox tabComboBox_batchNo;
        private System.Windows.Forms.Button button_create;
    }
}