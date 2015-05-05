namespace DigitalPlatform.EasyMarc
{
    partial class EasyMarcControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EasyMarcControl));
            this.tableLayoutPanel_content = new System.Windows.Forms.TableLayoutPanel();
            this.imageList_expandIcons = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // tableLayoutPanel_content
            // 
            this.tableLayoutPanel_content.AutoSize = true;
            this.tableLayoutPanel_content.ColumnCount = 4;
            this.tableLayoutPanel_content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel_content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel_content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 4F));
            this.tableLayoutPanel_content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_content.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_content.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_content.Name = "tableLayoutPanel_content";
            this.tableLayoutPanel_content.RowCount = 2;
            this.tableLayoutPanel_content.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_content.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_content.Size = new System.Drawing.Size(233, 244);
            this.tableLayoutPanel_content.TabIndex = 1;
            this.tableLayoutPanel_content.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel_content_Paint);
            // 
            // imageList_expandIcons
            // 
            this.imageList_expandIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_expandIcons.ImageStream")));
            this.imageList_expandIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_expandIcons.Images.SetKeyName(0, "collapsed.ico");
            this.imageList_expandIcons.Images.SetKeyName(1, "expanded.ico");
            // 
            // EasyMarcControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tableLayoutPanel_content);
            this.Name = "EasyMarcControl";
            this.Size = new System.Drawing.Size(69, 27);
            this.SizeChanged += new System.EventHandler(this.EasyMarcControl_SizeChanged);
            this.Enter += new System.EventHandler(this.EasyMarcControl_Enter);
            this.Leave += new System.EventHandler(this.EasyMarcControl_Leave);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_content;
        private System.Windows.Forms.ImageList imageList_expandIcons;
    }
}
