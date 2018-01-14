namespace dp2Circulation
{
    internal partial class BindingControl
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

            this.DisposeFonts();

            if (Program.MainForm != null && Program.MainForm._imageManager != null)
                Program.MainForm._imageManager.GetObjectComplete -= _imageManager_GetObjectComplete;

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BindingControl));
            this.timer_dragScroll = new System.Windows.Forms.Timer(this.components);
            this.imageList_treeIcon = new System.Windows.Forms.ImageList(this.components);
            this.imageList_layout = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // timer_dragScroll
            // 
            this.timer_dragScroll.Tick += new System.EventHandler(this.timer_dragScroll_Tick);
            // 
            // imageList_treeIcon
            // 
            this.imageList_treeIcon.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_treeIcon.ImageStream")));
            this.imageList_treeIcon.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_treeIcon.Images.SetKeyName(0, "recieve_none.bmp");
            this.imageList_treeIcon.Images.SetKeyName(1, "recieve_not_complete.bmp");
            this.imageList_treeIcon.Images.SetKeyName(2, "recieve_complete.bmp");
            // 
            // imageList_layout
            // 
            this.imageList_layout.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_layout.ImageStream")));
            this.imageList_layout.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_layout.Images.SetKeyName(0, "layout_binding.bmp");
            this.imageList_layout.Images.SetKeyName(1, "layout_accepting.bmp");
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer_dragScroll;
        internal System.Windows.Forms.ImageList imageList_treeIcon;
        internal System.Windows.Forms.ImageList imageList_layout;
    }
}
