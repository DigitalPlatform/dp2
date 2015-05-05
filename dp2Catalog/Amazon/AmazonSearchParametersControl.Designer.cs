namespace dp2Catalog
{
    partial class AmazonSearchParametersControl
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
            this.tableLayoutPanel_content = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_content
            // 
            this.tableLayoutPanel_content.AutoSize = true;
            this.tableLayoutPanel_content.ColumnCount = 2;
            this.tableLayoutPanel_content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_content.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_content.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_content.Name = "tableLayoutPanel_content";
            this.tableLayoutPanel_content.RowCount = 1;
            this.tableLayoutPanel_content.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_content.Size = new System.Drawing.Size(320, 150);
            this.tableLayoutPanel_content.TabIndex = 0;
            this.tableLayoutPanel_content.CellPaint += new System.Windows.Forms.TableLayoutCellPaintEventHandler(this.tableLayoutPanel_content_CellPaint);
            // 
            // AmazonSearchParametersControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel_content);
            this.Name = "AmazonSearchParametersControl";
            this.Size = new System.Drawing.Size(320, 150);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_content;
    }
}
