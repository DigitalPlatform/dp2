namespace DigitalPlatform.CommonDialog
{
    partial class CalendarControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CalendarControl));
            this.timer_dragScroll = new System.Windows.Forms.Timer(this.components);
            this.imageList_stateIcons = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // timer_dragScroll
            // 
            this.timer_dragScroll.Tick += new System.EventHandler(this.timer_dragScroll_Tick);
            // 
            // imageList_stateIcons
            // 
            this.imageList_stateIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_stateIcons.ImageStream")));
            this.imageList_stateIcons.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_stateIcons.Images.SetKeyName(0, "working_day.bmp");
            this.imageList_stateIcons.Images.SetKeyName(1, "none_working_day.bmp");
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer_dragScroll;
        private System.Windows.Forms.ImageList imageList_stateIcons;
    }
}
