using DigitalPlatform.CommonControl;
namespace dp2Catalog
{
    partial class AmazonQueryControl
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label_searchIndex = new System.Windows.Forms.Label();
            this.comboBox_searchIndex = new DigitalPlatform.CommonControl.TabComboBox();
            this.label_searchParameters = new System.Windows.Forms.Label();
            this.amazonSearchParametersControl1 = new dp2Catalog.AmazonSearchParametersControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label_searchIndex, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBox_searchIndex, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label_searchParameters, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.amazonSearchParametersControl1, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(305, 40);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label_searchIndex
            // 
            this.label_searchIndex.AutoSize = true;
            this.label_searchIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_searchIndex.Location = new System.Drawing.Point(3, 0);
            this.label_searchIndex.Name = "label_searchIndex";
            this.label_searchIndex.Size = new System.Drawing.Size(53, 28);
            this.label_searchIndex.TabIndex = 0;
            this.label_searchIndex.Text = "检索类型";
            this.label_searchIndex.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBox_searchIndex
            // 
            this.comboBox_searchIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox_searchIndex.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBox_searchIndex.FormattingEnabled = true;
            this.comboBox_searchIndex.Location = new System.Drawing.Point(62, 3);
            this.comboBox_searchIndex.Name = "comboBox_searchIndex";
            this.comboBox_searchIndex.Size = new System.Drawing.Size(240, 22);
            this.comboBox_searchIndex.TabIndex = 1;
            this.comboBox_searchIndex.TextChanged += new System.EventHandler(this.comboBox_searchIndex_TextChanged);
            // 
            // label_searchParameters
            // 
            this.label_searchParameters.AutoSize = true;
            this.label_searchParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_searchParameters.Location = new System.Drawing.Point(3, 28);
            this.label_searchParameters.Name = "label_searchParameters";
            this.label_searchParameters.Size = new System.Drawing.Size(53, 12);
            this.label_searchParameters.TabIndex = 2;
            this.label_searchParameters.Text = "检索式";
            this.label_searchParameters.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // amazonSearchParametersControl1
            // 
            this.amazonSearchParametersControl1.AutoSize = true;
            this.amazonSearchParametersControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.amazonSearchParametersControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.amazonSearchParametersControl1.Location = new System.Drawing.Point(62, 31);
            this.amazonSearchParametersControl1.Name = "amazonSearchParametersControl1";
            this.amazonSearchParametersControl1.SearchIndex = "All";
            this.amazonSearchParametersControl1.Size = new System.Drawing.Size(240, 6);
            this.amazonSearchParametersControl1.TabIndex = 3;
            // 
            // AmazonQueryControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "AmazonQueryControl";
            this.Size = new System.Drawing.Size(308, 221);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label_searchIndex;
        private TabComboBox comboBox_searchIndex;
        private System.Windows.Forms.Label label_searchParameters;
        private AmazonSearchParametersControl amazonSearchParametersControl1;
    }
}
