namespace dp2Catalog
{
    partial class ZQueryControl
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
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.AutoScroll = true;
            this.tableLayoutPanel_main.AutoScrollMinSize = new System.Drawing.Size(300, 100);
            this.tableLayoutPanel_main.ColumnCount = 3;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label3, 2, 0);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 1;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(345, 310);
            this.tableLayoutPanel_main.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Âß¼­Ëã·û";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(59, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.MinimumSize = new System.Drawing.Size(75, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "¼ìË÷´Ê";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(290, 0);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "¼ìË÷Í¾¾¶";
            // 
            // ZQueryControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "ZQueryControl";
            this.Size = new System.Drawing.Size(345, 310);
            this.Load += new System.EventHandler(this.QueryControl_Load);
            this.SizeChanged += new System.EventHandler(this.QueryControl_SizeChanged);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}
