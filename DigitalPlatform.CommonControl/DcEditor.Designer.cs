namespace DigitalPlatform.CommonControl
{
    partial class DcEditor
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            // 2015/7/21
            if (disposing)
            {
                this.ClearElements();
            }

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label_topleft = new System.Windows.Forms.Label();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.AutoScroll = true;
            this.tableLayoutPanel_main.AutoScrollMinSize = new System.Drawing.Size(200, 100);
            this.tableLayoutPanel_main.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel_main.ColumnCount = 5;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label3, 2, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label4, 3, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label5, 4, 0);
            this.tableLayoutPanel_main.Controls.Add(this.label_topleft, 0, 0);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(377, 222);
            this.tableLayoutPanel_main.TabIndex = 1;
            this.tableLayoutPanel_main.CellPaint += new System.Windows.Forms.TableLayoutCellPaintEventHandler(this.tableLayoutPanel_main_CellPaint);
            this.tableLayoutPanel_main.ForeColorChanged += new System.EventHandler(this.tableLayoutPanel_main_ForeColorChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(17, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "元素";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(52, 5);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "编码方案";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(113, 5);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "语种";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(148, 5);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "内容";
            // 
            // label_topleft
            // 
            this.label_topleft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_topleft.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_topleft.Location = new System.Drawing.Point(2, 5);
            this.label_topleft.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.label_topleft.Name = "label_topleft";
            this.label_topleft.Size = new System.Drawing.Size(11, 12);
            this.label_topleft.TabIndex = 5;
            this.label_topleft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_topleft_MouseUp);
            // 
            // DcEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "DcEditor";
            this.Size = new System.Drawing.Size(377, 222);
            this.Load += new System.EventHandler(this.DcEditor_Load);
            this.SizeChanged += new System.EventHandler(this.DcEditor_SizeChanged);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label_topleft;
    }
}
