namespace DigitalPlatform.Marc
{
    partial class DeleteFieldDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeleteFieldDlg));
            this.label_message = new System.Windows.Forms.Label();
            this.button_yes = new System.Windows.Forms.Button();
            this.button_no = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_message
            // 
            this.label_message.AutoEllipsis = true;
            this.label_message.Location = new System.Drawing.Point(16, 16);
            this.label_message.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(357, 65);
            this.label_message.TabIndex = 0;
            this.label_message.Text = "确实要删除当前字段?";
            // 
            // button_yes
            // 
            this.button_yes.Location = new System.Drawing.Point(165, 105);
            this.button_yes.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_yes.Name = "button_yes";
            this.button_yes.Size = new System.Drawing.Size(100, 29);
            this.button_yes.TabIndex = 1;
            this.button_yes.Text = "是(&Y)";
            this.button_yes.UseVisualStyleBackColor = true;
            this.button_yes.Click += new System.EventHandler(this.button_yes_Click);
            // 
            // button_no
            // 
            this.button_no.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_no.Location = new System.Drawing.Point(273, 105);
            this.button_no.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_no.Name = "button_no";
            this.button_no.Size = new System.Drawing.Size(100, 29);
            this.button_no.TabIndex = 2;
            this.button_no.Text = "否(&N)";
            this.button_no.UseVisualStyleBackColor = true;
            this.button_no.Click += new System.EventHandler(this.button_no_Click);
            // 
            // DeleteFieldDlg
            // 
            this.AcceptButton = this.button_yes;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_no;
            this.ClientSize = new System.Drawing.Size(389, 149);
            this.Controls.Add(this.button_no);
            this.Controls.Add(this.button_yes);
            this.Controls.Add(this.label_message);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeleteFieldDlg";
            this.ShowInTaskbar = false;
            this.Text = "删除字段";
            this.Load += new System.EventHandler(this.DeleteFieldDlg_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_yes;
        private System.Windows.Forms.Button button_no;
    }
}