using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Accord.Controls;
using Accord.Video;
using Accord.Video.DirectShow;

namespace TestAccord
{
    public partial class Form1 : Form
    {
        VideoSourcePlayer videoSourcePlayer = null;

        public Form1()
        {
            InitializeComponent();

            videoSourcePlayer = new VideoSourcePlayer();
            videoSourcePlayer.Size = new Size(100, 100);
            videoSourcePlayer.Dock = DockStyle.Fill;
            videoSourcePlayer.KeepAspectRatio = true;
            videoSourcePlayer.NewFrame += VideoSourcePlayer_NewFrame;
            this.Controls.Add(videoSourcePlayer);
        }

        private void VideoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            DateTime now = DateTime.Now;
            using (Graphics g = Graphics.FromImage(image))

            // paint current time
            using (SolidBrush brush = new SolidBrush(Color.Red))
            {
                g.DrawString(now.ToString(), this.Font, brush, new PointF(5, 5));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCurrentVideoSource();
        }

        private void ToolStripMenuItem_openCamera_Click(object sender, EventArgs e)
        {
            VideoCaptureDeviceForm form = new VideoCaptureDeviceForm();

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // create video source
                VideoCaptureDevice videoSource = form.VideoDevice;

                // open it
                OpenVideoSource(videoSource);
            }
        }

        // Open video source
        private void OpenVideoSource(IVideoSource source)

        {
            // set busy cursor
            this.Cursor = Cursors.WaitCursor;


            // stop current video source
            CloseCurrentVideoSource();

            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();

            // reset stop watch
            // stopWatch = null;

            // start timer
            // timer.Start();
            this.Cursor = Cursors.Default;
        }

        // Close video source if it is running
        private void CloseCurrentVideoSource()
        {
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();
                videoSourcePlayer.VideoSource = null;
            }
        }
    }
}
