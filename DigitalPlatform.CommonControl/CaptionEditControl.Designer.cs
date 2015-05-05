namespace DigitalPlatform.CommonControl
{
    partial class CaptionEditControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.label_topleft = new System.Windows.Forms.Label();
            this.label_language = new System.Windows.Forms.Label();
            this.label_text = new System.Windows.Forms.Label();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel_main.ColumnCount = 3;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.Controls.Add(this.label_topleft, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label_language, 1, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label_text, 2, 0);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(208, 120);
            this.tableLayoutPanel_main.TabIndex = 0;
            // 
            // label_topleft
            // 
            this.label_topleft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_topleft.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_topleft.Location = new System.Drawing.Point(2, 5);
            this.label_topleft.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.label_topleft.Name = "label_topleft";
            this.label_topleft.Size = new System.Drawing.Size(10, 12);
            this.label_topleft.TabIndex = 7;
            this.label_topleft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_topleft_MouseUp);
            // 
            // label_language
            // 
            this.label_language.AutoSize = true;
            this.label_language.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_language.Location = new System.Drawing.Point(16, 5);
            this.label_language.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.label_language.Name = "label_language";
            this.label_language.Size = new System.Drawing.Size(57, 12);
            this.label_language.TabIndex = 8;
            this.label_language.Text = "语言代码";
            // 
            // label_text
            // 
            this.label_text.AutoSize = true;
            this.label_text.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_text.Location = new System.Drawing.Point(77, 5);
            this.label_text.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.label_text.Name = "label_text";
            this.label_text.Size = new System.Drawing.Size(44, 12);
            this.label_text.TabIndex = 9;
            this.label_text.Text = "文字值";
            // 
            // CaptionEditControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "CaptionEditControl";
            this.Size = new System.Drawing.Size(208, 120);
            this.Enter += new System.EventHandler(this.CaptionEditControl_Enter);
            this.Leave += new System.EventHandler(this.CaptionEditControl_Leave);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.Label label_topleft;
        private System.Windows.Forms.Label label_language;
        private System.Windows.Forms.Label label_text;
    }
}
