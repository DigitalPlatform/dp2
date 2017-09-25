namespace DigitalPlatform.CirculationClient
{
    partial class KernelResTree
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

            RemoveDownloader(null, true);

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KernelResTree));
            this.imageList_resIcon = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // imageList_resIcon
            // 
            this.imageList_resIcon.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_resIcon.ImageStream")));
            this.imageList_resIcon.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_resIcon.Images.SetKeyName(0, "database.bmp");
            this.imageList_resIcon.Images.SetKeyName(1, "searchfrom.bmp");
            this.imageList_resIcon.Images.SetKeyName(2, "");
            this.imageList_resIcon.Images.SetKeyName(3, "");
            this.imageList_resIcon.Images.SetKeyName(4, "");
            this.imageList_resIcon.Images.SetKeyName(5, "");
            // 
            // KernelResTree
            // 
            this.LineColor = System.Drawing.Color.Black;
            this.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.KernelResTree_AfterCheck);
            this.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.KernelResTree_AfterExpand);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.KernelResTree_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList_resIcon;
    }
}
