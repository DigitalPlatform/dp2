using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Video.DirectShow;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using VL.OpenCV;

namespace TestOpenCvSharp
{
    // https://github.com/vvvv/VL.OpenCV/blob/main/src/VideoInInfo.cs
    // https://github.com/shimat/opencvsharp/issues/969
    // https://blog.csdn.net/nirendao/article/details/50429168
    // https://stackoverflow.com/questions/19258886/how-to-get-a-list-of-available-video-capture-devices
    // https://github.com/shimat/opencvsharp/issues/538
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        VideoCapture capture;
        Mat frame;
        Bitmap image;

        private void MenuItem_test_camera_Click(object sender, EventArgs e)
        {

        }

        int isCameraRunning = 0;

        private void CaptureCameraCallback()
        {
            frame = new Mat();
            capture = new VideoCapture();
            capture.Open(2);
            isCameraRunning = 1;
            while (isCameraRunning == 1)
            {
                capture.Read(frame);
                image = BitmapConverter.ToBitmap(frame);
                pictureBox1.Image = image;
                image = null;
            }
        }

        private void toolStripButton_start_Click(object sender, EventArgs e)
        {
            var formats1 = VideoInInfo.GetSupportedFormats(0);
            var formats2 = VideoInInfo.GetSupportedFormats(1);
            var formats3 = VideoInInfo.GetSupportedFormats(2);

            var list = VideoInInfo.EnumerateVideoDevices();

            var info = SelectCamera();

            frame = new Mat();
            capture = new VideoCapture();

            capture.Set(VideoCaptureProperties.FrameWidth, info.CaptureSize.Width);
            capture.Set(VideoCaptureProperties.FrameHeight, info.CaptureSize.Height);

            capture.Open(1);
            isCameraRunning = 1;
            _ = Task.Run(() =>
            {
                /*
                frame = new Mat();
                capture = new VideoCapture();
                capture.Open(2);
                */
                while (isCameraRunning == 1)
                {
                    capture.Read(frame);
                    image = BitmapConverter.ToBitmap(frame);
                    this.Invoke((Action)(() =>
                    {
                        pictureBox1.Image = image;
                    }));
                    image = null;
                }
            });
        }

        private void toolStripButton_stop_Click(object sender, EventArgs e)
        {
            isCameraRunning = 0;
            capture?.Release();
        }


        CameraInfo SelectCamera()
        {
            CameraInfo _cameraInfo = null;
            using (VideoCaptureDeviceForm form = new VideoCaptureDeviceForm())
            {
                if (_cameraInfo == null)
                    _cameraInfo = new CameraInfo();
                form.VideoDeviceMoniker = _cameraInfo.Moniker;
                form.CaptureSize = _cameraInfo.CaptureSize;
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // videoSource.VideoResolution = videoSource.VideoCapabilities[1];
                    // OpenVideoSource(videoSource);
                    _cameraInfo.Device = form.VideoDevice;
                    _cameraInfo.Moniker = form.VideoDeviceMoniker;
                    _cameraInfo.CaptureSize = form.CaptureSize;
                    return _cameraInfo;
                }

                return null;
            }
        }

        class CameraInfo
        {
            public VideoCaptureDevice Device { get; set; }
            public string Moniker { get; set; }
            public System.Drawing.Size CaptureSize { get; set; }
        }
    }
}
