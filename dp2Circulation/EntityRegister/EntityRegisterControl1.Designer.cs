namespace dp2Circulation
{
    partial class EntityRegisterControlOld
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
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.easyMarcControl1 = new DigitalPlatform.EasyMarc.EasyMarcControl();
            this.splitContainer_right = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel_entities = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_right)).BeginInit();
            this.splitContainer_right.Panel2.SuspendLayout();
            this.splitContainer_right.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.easyMarcControl1);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.splitContainer_right);
            this.splitContainer_main.Size = new System.Drawing.Size(406, 300);
            this.splitContainer_main.SplitterDistance = 181;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 1;
            // 
            // easyMarcControl1
            // 
            this.easyMarcControl1.AutoScroll = true;
            this.easyMarcControl1.CaptionWidth = 116;
            this.easyMarcControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.easyMarcControl1.HideIndicator = true;
            this.easyMarcControl1.IncludeNumber = false;
            this.easyMarcControl1.Location = new System.Drawing.Point(0, 0);
            this.easyMarcControl1.MarcDefDom = null;
            this.easyMarcControl1.Name = "easyMarcControl1";
            this.easyMarcControl1.Size = new System.Drawing.Size(181, 300);
            this.easyMarcControl1.TabIndex = 0;
            // 
            // splitContainer_right
            // 
            this.splitContainer_right.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_right.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_right.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_right.Name = "splitContainer_right";
            this.splitContainer_right.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_right.Panel2
            // 
            this.splitContainer_right.Panel2.Controls.Add(this.flowLayoutPanel_entities);
            this.splitContainer_right.Size = new System.Drawing.Size(217, 300);
            this.splitContainer_right.SplitterDistance = 111;
            this.splitContainer_right.TabIndex = 0;
            // 
            // flowLayoutPanel_entities
            // 
            this.flowLayoutPanel_entities.AutoScroll = true;
            this.flowLayoutPanel_entities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_entities.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel_entities.Name = "flowLayoutPanel_entities";
            this.flowLayoutPanel_entities.Size = new System.Drawing.Size(217, 185);
            this.flowLayoutPanel_entities.TabIndex = 0;
            // 
            // EntityRegisterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer_main);
            this.Name = "EntityRegisterControl";
            this.Size = new System.Drawing.Size(406, 300);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.splitContainer_right.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_right)).EndInit();
            this.splitContainer_right.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main;
        private DigitalPlatform.EasyMarc.EasyMarcControl easyMarcControl1;
        private System.Windows.Forms.SplitContainer splitContainer_right;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_entities;
    }
}
