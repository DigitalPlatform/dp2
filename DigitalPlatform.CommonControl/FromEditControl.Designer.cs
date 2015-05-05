namespace DigitalPlatform.CommonControl
{
    partial class FromEditControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label_topleft = new System.Windows.Forms.Label();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.AutoScroll = true;
            this.tableLayoutPanel_main.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel_main.ColumnCount = 3;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label_topleft, 0, 0);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(336, 150);
            this.tableLayoutPanel_main.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(23, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "风格";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(68, 6);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "显示文字";
            // 
            // label_topleft
            // 
            this.label_topleft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_topleft.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_topleft.Location = new System.Drawing.Point(3, 6);
            this.label_topleft.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.label_topleft.Name = "label_topleft";
            this.label_topleft.Size = new System.Drawing.Size(14, 15);
            this.label_topleft.TabIndex = 6;
            this.label_topleft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_topleft_MouseUp);
            // 
            // FromEditControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Name = "FromEditControl";
            this.Size = new System.Drawing.Size(336, 150);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_topleft;
    }
}
