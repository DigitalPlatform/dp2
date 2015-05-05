namespace dp2Circulation
{
    partial class DkywAmerceCardDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DkywAmerceCardDialog));
            this.label_cardInfo = new System.Windows.Forms.Label();
            this.label_thisPrice = new System.Windows.Forms.Label();
            this.button_writeCard = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // label_cardInfo
            // 
            this.label_cardInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_cardInfo.BackColor = System.Drawing.SystemColors.Info;
            this.label_cardInfo.Font = new System.Drawing.Font("Arial Black", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_cardInfo.Location = new System.Drawing.Point(10, 10);
            this.label_cardInfo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_cardInfo.Name = "label_cardInfo";
            this.label_cardInfo.Size = new System.Drawing.Size(277, 134);
            this.label_cardInfo.TabIndex = 0;
            this.label_cardInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_thisPrice
            // 
            this.label_thisPrice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_thisPrice.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_thisPrice.Font = new System.Drawing.Font("Arial Black", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_thisPrice.Location = new System.Drawing.Point(9, 154);
            this.label_thisPrice.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_thisPrice.Name = "label_thisPrice";
            this.label_thisPrice.Size = new System.Drawing.Size(278, 40);
            this.label_thisPrice.TabIndex = 1;
            this.label_thisPrice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_writeCard
            // 
            this.button_writeCard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_writeCard.Enabled = false;
            this.button_writeCard.Location = new System.Drawing.Point(170, 197);
            this.button_writeCard.Margin = new System.Windows.Forms.Padding(2);
            this.button_writeCard.Name = "button_writeCard";
            this.button_writeCard.Size = new System.Drawing.Size(56, 22);
            this.button_writeCard.TabIndex = 2;
            this.button_writeCard.Text = "扣款";
            this.button_writeCard.UseVisualStyleBackColor = true;
            this.button_writeCard.Click += new System.EventHandler(this.button_writeCard_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(230, 197);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // DkywAmerceCardDialog
            // 
            this.AcceptButton = this.button_writeCard;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(296, 229);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_writeCard);
            this.Controls.Add(this.label_thisPrice);
            this.Controls.Add(this.label_cardInfo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DkywAmerceCardDialog";
            this.ShowInTaskbar = false;
            this.Text = "(迪科远望)从IC卡扣款";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AmerceCardDialog_FormClosing);
            this.Load += new System.EventHandler(this.AmerceCardDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label_cardInfo;
        private System.Windows.Forms.Label label_thisPrice;
        private System.Windows.Forms.Button button_writeCard;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Timer timer1;
    }
}