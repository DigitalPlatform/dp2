namespace dp2Catalog
{
    partial class ZTargetControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZTargetControl));
            this.imageList_resIcon = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // imageList_resIcon
            // 
            this.imageList_resIcon.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_resIcon.ImageStream")));
            this.imageList_resIcon.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_resIcon.Images.SetKeyName(0, "folder.bmp");
            this.imageList_resIcon.Images.SetKeyName(1, "offline_computer.bmp");
            this.imageList_resIcon.Images.SetKeyName(2, "database.bmp");
            this.imageList_resIcon.Images.SetKeyName(3, "computer.bmp");
            this.imageList_resIcon.Images.SetKeyName(4, "database_notinall.bmp");
            // 
            // ZTargetControl
            // 
            this.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.ZTargetControl_AfterCheck);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ZTargetControl_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ZTargetControl_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList_resIcon;
    }
}
