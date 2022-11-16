using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

// using AForge.Video;
using Accord.Video;
using ZXing;
using Accord.Vision.Motion;

namespace DigitalPlatform.Drawing
{
    public partial class QrRecognitionControl : UserControl
    {
        public event CatchedEventHandler Catched = null;

        public event FirstImageFilledEventHandler FirstImageFilled = null;

        private struct Device
        {
            public int Index;
            public string Name;
            public string Moniker;
            public override string ToString()
            {
                return Name;
            }
        }

        private CameraDevices camDevices;
        private Bitmap _currentBitmapForDecoding;
        private readonly Thread decodingThread;
        private Result currentResult;
        private readonly Pen resultRectPen;

        MotionDetector motionDetector = null;

        public QrRecognitionControl()
        {
            InitializeComponent();

            // test
            // throw new Exception("test exception");

            camDevices = new CameraDevices();

            decodingThread = new Thread(DecodeBarcode);

            resultRectPen = new Pen(Color.Green, 10);

            motionDetector = GetDefaultMotionDetector();
        }

        public void DisposeResource(bool disposing)
        {
            if (disposing)
            {
                // 2018/10/23
                if (_currentBitmapForDecoding != null)
                    _currentBitmapForDecoding.Dispose();

                if (resultRectPen != null)
                    resultRectPen.Dispose();

                decodingThread?.Abort();
                if (camDevices.Current != null)
                {
                    if (camDevices.Current.IsRunning)
                    {
                        camDevices.Current.SignalToStop();
                    }
                    camDevices.Current.NewFrame -= Current_NewFrame;
                    camDevices.Current.VideoSourceError -= Current_VideoSourceError;
                }
            }
        }

        public bool EnableMotionDetect
        {
            get
            {
                return motionDetector != null;
            }
            set
            {
                if (value == true)
                {
                    motionDetector = GetDefaultMotionDetector();
                }
                else
                {
                    motionDetector = null;
                }
            }
        }

        public bool PhotoMode = false;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.PhotoMode == false)
                decodingThread?.Start();
            ////
            LoadDevicesToCombobox();
        }

        public void RefreshDevList()
        {
            CameraDevices temp = new CameraDevices();
            if (this.camDevices.IsEqual(temp) == false)
            {
#if NO
                if (this.camDevices != null && camDevices.Current != null)
                {
                    if (camDevices.Current.IsRunning)
                    {
                        camDevices.Current.SignalToStop();
                    }
                    camDevices.Current.NewFrame -= Current_NewFrame;
                    // this.camDevices.Current.WaitForStop();
                    WaitForStop();
                }
#endif
                EndCatch();

                camDevices = new CameraDevices();
                LoadDevicesToCombobox();
            }
        }

        private void LoadDevicesToCombobox()
        {
            cmbDevice.Items.Clear();
            this.m_strSelectedCameraName = "";

            List<string> names = new List<string>();
            for (var index = 0; index < camDevices.Devices.Count; index++)
            {
                names.Add(camDevices.Devices[index].Name);
            }

            ChangeNames(ref names);

            for (var index = 0; index < camDevices.Devices.Count; index++)
            {
                // cmbDevice.Items.Add(new Device { Index = index, Name = camDevices.Devices[index].Name, Moniker = camDevices.Devices[index].MonikerString });
                cmbDevice.Items.Add(new Device { Index = index, Name = names[index], Moniker = camDevices.Devices[index].MonikerString });
            }
        }

        // 为字符串列表中重复的名字加上后缀编号
        static void ChangeNames(ref List<string> names)
        {
            List<int> numbers = new List<int>();
            // 先把数字数组填充 0
            for (int i = 0; i < names.Count; i++)
            {
                numbers.Add(0); // 0 表示没有数字。1 开始才是要用的数字
            }

            // number 中元素 -1 表示处理过了
            while (true)
            {
                string first = "";
                int nStart = 2;
                int nCount = 0;
                for (int i = 0; i < numbers.Count; i++)
                {
                    if (numbers[i] > 0 || numbers[i] == -1)
                        continue;

                    nCount++;
                    if (string.IsNullOrEmpty(first) == true)
                    {
                        first = names[i];
                        numbers[i] = -1;
                        continue;
                    }

                    if (names[i] == first)
                    {
                        numbers[names.IndexOf(first)] = 1;
                        numbers[i] = nStart++;
                    }
                }
                if (nCount == 0)
                    break;
            }

            for (int i = 0; i < numbers.Count; i++)
            {
                if (numbers[i] > 0)
                    names[i] = names[i] + " " + numbers[i].ToString();
            }
        }

        public string CurrentCamera
        {
            get
            {
                if (this.camDevices == null)
                    return "";
                if (this.camDevices.Current == null)
                    return "";
                return this.camDevices.CurrentCameraMonier;
            }
            set
            {
                try
                {

                    if (cmbDevice.Items.Count > 0)
                        SelectListItem(value);
                    else
                    {
                        this.m_strSelectedCameraName = value;
                        StartCatch();
                    }
                }
                catch
                {
                    this.m_strSelectedCameraName = "";
                }
            }
        }

#if NO
        public int SelectedCameraIndex
        {
            get
            {
                if (cmbDevice.SelectedItem == null)
                    return -1;
                return ((Device)(cmbDevice.SelectedItem)).Index;
            }
        }
#endif

        string m_strSelectedCameraName = "";
        bool m_bInCatch = false;
        public bool StartCatch()
        {
            this.LastText = "";
            if (camDevices.Current != null)
            {
                if (camDevices.Current.IsRunning)
                {
                    camDevices.Current.SignalToStop();
                }
                camDevices.Current.NewFrame -= Current_NewFrame;
                camDevices.Current.VideoSourceError -= Current_VideoSourceError;

#if NO
                DateTime start = DateTime.Now;
                while (camDevices.Current.IsRunning)
                {
                    Thread.Sleep(100);

                    // 等待超过两秒就跳出循环。这时后面的启动有可能在信号中断之前发生，也就是说摄像头不能成功启动
                    if (DateTime.Now - start > new TimeSpan(0,0,2))
                        break;
                }
#endif
                // camDevices.Current.WaitForStop();
                WaitForStop();
            }

            if (string.IsNullOrEmpty(this.m_strSelectedCameraName) == true)
            {
                this.label_message.Text = "尚未指定摄像头";
                this.label_message.Visible = true;

                this.pictureBox1.Visible = false;
                OnFirstImageFilled(true);
                return false;
            }
            else
            {
                camDevices.SelectCamera(m_strSelectedCameraName);
                this.label_message.Visible = false;
            }

            // camDevices.SelectCamera(((Device)(cmbDevice.SelectedItem)).Index);
            camDevices.Current.NewFrame += Current_NewFrame;
            camDevices.Current.VideoSourceError += Current_VideoSourceError;
            // testing
            // camDevices.Current.VideoResolution = camDevices.Current.VideoCapabilities[camDevices.Current.VideoCapabilities.Length - 1];
            camDevices.Current.Start();
            m_bInCatch = true;

            this.pictureBox1.Visible = true;
            this.progressBar1.Visible = true;
            return true;
        }

        private void Current_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            string error = eventArgs.Description;
        }

        /// <summary>
        /// 显示或隐藏图像
        /// </summary>
        /// <param name="bDisplay">显示或隐藏图像。如果为 true 表示显示图像，隐藏文字显示；否则隐藏图像，显示文字</param>
        public void DisplayImage(bool bDisplay = true)
        {
            this.label_message.Visible = !bDisplay;
            this.pictureBox1.Visible = bDisplay;
        }

        // 2019/6/6
        public void DisplayCameraList(bool display)
        {
            this.panel_camera.Visible = display;
        }

        /// <summary>
        /// 显示文字
        /// </summary>
        /// <param name="strText">文字内容</param>
        public void DisplayText(string strText)
        {
            this.label_message.Text = strText;
            this.label_message.Visible = true;
            this.pictureBox1.Visible = false;
        }

        public void RotateImage(RotateFlipType flip_type)
        {
            Image image = this.pictureBox1.Image;
            if (image == null)
                return;
            image.RotateFlip(flip_type);

            pictureBox1.Width = image.Width;
            pictureBox1.Height = image.Height;
            ImageUtil.SetImage(pictureBox1, image); // 2016/12/28
        }

        public void SetImageBorder(bool bThick)
        {
            if (bThick == true)
                this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            else
                this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
        }

        void WaitForStop()
        {
            if (camDevices.Current == null)
                return;

            this.camDevices.Current.WaitForStop();

            // 注：不是太明白当初为啥要用这一段代码 2019/6/13
#if NO
            DateTime start = DateTime.Now;
            while (camDevices.Current.IsRunning)
            {
                Thread.Sleep(100);

                // 等待超过两秒就跳出循环。这时后面的启动有可能在信号中断之前发生，也就是说摄像头不能成功启动
                if (DateTime.Now - start > new TimeSpan(0, 0, 2))
                    break;
            }
#endif
        }

        public bool InCatch
        {
            get
            {
                return this.m_bInCatch;
            }
        }

        public void EndCatch()
        {
            m_bInCatch = false;
            if (camDevices.Current != null)
            {
                if (camDevices.Current.IsRunning)
                {
                    camDevices.Current.SignalToStop();
                }
                camDevices.Current.NewFrame -= Current_NewFrame;
                camDevices.Current.VideoSourceError -= Current_VideoSourceError;

                // 2014/2/28
                WaitForStop();

                this.label_message.Text = "已停止捕捉";
                this.label_message.Visible = true;

                this.pictureBox1.Visible = false;
                this.progressBar1.Value = 0;
                this.progressBar1.Visible = false;
            }
        }

        void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (currentResult == null)
                return;

            return;

            if (currentResult.ResultPoints != null && currentResult.ResultPoints.Length > 0)
            {
                var resultPoints = currentResult.ResultPoints;
                var rect = new Rectangle((int)resultPoints[0].X, (int)resultPoints[0].Y, 1, 1);
                foreach (var point in resultPoints)
                {
                    if (point.X < rect.Left)
                        rect = new Rectangle((int)point.X, rect.Y, rect.Width + rect.X - (int)point.X, rect.Height);
                    if (point.X > rect.Right)
                        rect = new Rectangle(rect.X, rect.Y, rect.Width + (int)point.X - rect.X, rect.Height);
                    if (point.Y < rect.Top)
                        rect = new Rectangle(rect.X, (int)point.Y, rect.Width, rect.Height + rect.Y - (int)point.Y);
                    if (point.Y > rect.Bottom)
                        rect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height + (int)point.Y - rect.Y);
                }

                using (var g = pictureBox1.CreateGraphics())
                {
#if NO
                    if (_frameWidth != 0 && _frameHeight != 0)
                        g.ScaleTransform((float)pictureBox1.Width / (float)_frameWidth,
                            (float)pictureBox1.Height / (float)_frameHeight);
#endif
                    g.DrawRectangle(resultRectPen, rect);
                }

#if NO
                using (Bitmap bitmap = (Bitmap)pictureBox1.Image)
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.DrawRectangle(resultRectPen, rect);
                    }
                }
#endif

            }
        }

        void DisplayMotionLevel(float level)
        {
            try
            {
                this.progressBar1.Value = (int)((float)100 * level);
            }
            catch
            {

            }
        }

        static float AWAKE_LEVEL = 0.3F;

        private void DecodeBarcode()
        {
            var reader = new BarcodeReader();
            while (true)
            {
                if (_currentBitmapForDecoding != null)
                {
                    var result = reader.Decode(_currentBitmapForDecoding);
                    if (result != null)
                    {
                        Invoke(new Action<Result>(ShowResult), result);
                    }
                    else
                        this.Invoke(new Action<Color>(PaintColor), Color.Yellow);

                    if (motionDetector != null
                        && (_iFrameCount % 2) == 0)
                    {
                        motionLevel = motionDetector.ProcessFrame(_currentBitmapForDecoding);
                        // Debug.WriteLine("level=" + motionLevel.ToString());
                        if (motionLevel > AWAKE_LEVEL)
                        {
                            BeginInvoke(new Action(RefreshLastText));
                        }
                        BeginInvoke(new Action<float>(DisplayMotionLevel), new object[] { motionLevel });
                    }
                    _iFrameCount++;

                    _currentBitmapForDecoding.Dispose();
                    _currentBitmapForDecoding = null;
                }
                int nInterval = 1000;
                if (motionLevel > AWAKE_LEVEL)
                    nInterval = 200;    // 200

#if NO
                if (motionLevel > 0.5)
                    nInterval = 200;    // 200
#endif

                Thread.Sleep(nInterval);
            }
        }

        // 最近一次捕捉到的文字
        string m_strLastText = "";
        // 最近一次捕捉到文字的时间。用于超时以后清空 m_strLastText
        DateTime m_lastTextTime = DateTime.Now;

        public string LastText
        {
            get
            {
                return this.m_strLastText;
            }
            set
            {
                this.m_strLastText = value;
                this.m_lastTextTime = DateTime.Now;
            }
        }

        // 看看是否超时，如果超时则清空 m_strLastText
        void RefreshLastText()
        {
            try
            {
                DateTime now = DateTime.Now;
                if (string.IsNullOrEmpty(this.m_strLastText) == false
                    && now - this.m_lastTextTime > new TimeSpan(0, 0, 5))
                    this.LastText = "";
            }
            catch
            {

            }
        }

        int _colorPosIndex = 0;

        void PaintColor(Color color)
        {
            int nCellHeight = Math.Max(10, pictureBox1.Height / 20);
            int nMax = pictureBox1.Width / nCellHeight;
            if (_colorPosIndex >= nMax)
                _colorPosIndex = 0;
            using (var g = pictureBox1.CreateGraphics())
            {
                using (SolidBrush brush = new SolidBrush(color))
                {
                    if (color == Color.Green)
                        g.FillRectangle(brush, 0, 0, pictureBox1.Width, nCellHeight);
                    else
                        g.FillRectangle(brush, nCellHeight * _colorPosIndex, 0, nCellHeight, nCellHeight);
                }
            }

            _colorPosIndex++;
        }

        private void ShowResult(Result result)
        {
            currentResult = result;

            // RefreshLastText();

            if (this.Catched != null && m_bInCatch == true)
            {
                if (result.Text == m_strLastText)
                {
                    // 重复的捕捉，被忽略。图像瞬间绘制红色
                    PaintColor(Color.Red);
                    motionLevel = 0F;
                }
                else
                {
                    // 捕捉成功。图像瞬间绘制绿色
                    PaintColor(Color.Green);

                    CatchedEventArgs e = new CatchedEventArgs();
                    e.BarcodeFormat = result.BarcodeFormat;
                    e.Text = result.Text;
                    this.Catched(this, e);
                    this.LastText = result.Text;
                    motionLevel = 0F;
                }
            }

        }

        int _inNewFrame = 0;

        // int _sourceFrameCount = 0;

        private void Current_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (IsDisposed)
            {
                return;
            }

            _inNewFrame++;
            try
            {
                if (_currentBitmapForDecoding == null)
                {
                    _currentBitmapForDecoding = (Bitmap)eventArgs.Frame.Clone();
                }
                //if ((_sourceFrameCount % 2) == 1)
                if (this.IsHandleCreated)   // 2019/6/2
                    BeginInvoke(new Action<Bitmap>(ShowFrame), eventArgs.Frame.Clone());
                //_sourceFrameCount++;
                /*
                    if (motionLevel < 0.5)
                        Thread.Sleep(100);
                 * */
                // Application.DoEvents();
            }
            catch (ObjectDisposedException)
            {
                // not sure, why....
                int i = 0;
                i++;
            }

            _inNewFrame--;
        }

        bool _bFirstImageFilled = false;
        //int _frameWidth = 0;
        //int _frameHeight = 0;
        int _iFrameCount = 0;
        float motionLevel = 0F;

        // 注意，本函数以内，负责释放 frame
        private void ShowFrame(Bitmap frame)
        {
            try
            {
                int _frameWidth = frame.Width;
                int _frameHeight = frame.Height;
                if (pictureBox1.Width < _frameWidth)
                    pictureBox1.Width = _frameWidth;

                if (pictureBox1.Height < _frameHeight)
                    pictureBox1.Height = _frameHeight;

                // pictureBox1.Image = frame;
                // 2018/10/23
                ImageUtil.SetImage(pictureBox1, frame);
                frame = null;
#if NO
            if (_bFirstImageFilled == false)
            {
                if (this.FirstImageFilled != null)
                    this.FirstImageFilled(this, new EventArgs());
                _bFirstImageFilled = true;
            }
#endif
                OnFirstImageFilled(false);

#if NO
            if ((_iFrameCount % 20) == 0)
            {
                motionLevel = motionDetector.ProcessFrame(frame);
                // Debug.WriteLine("level=" + motionLevel.ToString());
                if (motionLevel > 0.3F)
                {
                    BeginInvoke(new Action(RefreshLastText));
                }
            }
            _iFrameCount++;
#endif
            }
            catch
            {

            }
            finally
            {
                if (frame != null)
                    frame.Dispose();
            }
        }

        void OnFirstImageFilled(bool bError)
        {
            if (_bFirstImageFilled == false)
            {
                if (this.FirstImageFilled != null)
                    this.FirstImageFilled(this, new FirstImageFilledEventArgs(bError));
                _bFirstImageFilled = true;
            }
        }

        // Play around with this function to tweak results.
        public static MotionDetector GetDefaultMotionDetector()
        {
            IMotionDetector detector = null;
            // AForge.Vision.Motion.IMotionProcessing processor = null;
            MotionDetector motionDetector = null;

            detector = new TwoFramesDifferenceDetector()
            {
                DifferenceThreshold = 15,
                SuppressNoise = true
            };

            //detector = new AForge.Vision.Motion.CustomFrameDifferenceDetector()
            //{
            //  DifferenceThreshold = 15,
            //  KeepObjectsEdges = true,
            //  SuppressNoise = true
            //};

            //processor = new AForge.Vision.Motion.GridMotionAreaProcessing()
            //{
            //  HighlightColor = System.Drawing.Color.Red,
            //  HighlightMotionGrid = true,
            //  GridWidth = 100,
            //  GridHeight = 100,
            //  MotionAmountToHighlight = 100F
            //};

            /*
            processor = new AForge.Vision.Motion.MotionAreaHighlighting()
            {
                HighlightColor = System.Drawing.Color.Red,
            };
             * */

            motionDetector = new MotionDetector(detector);
            return (motionDetector);
        }
#if NO
        // Play around with this function to tweak results.
        public static AForge.Vision.Motion.MotionDetector GetDefaultMotionDetector()
        {
            AForge.Vision.Motion.IMotionDetector detector = null;
            AForge.Vision.Motion.IMotionProcessing processor = null;
            AForge.Vision.Motion.MotionDetector motionDetector = null;

            //detector = new AForge.Vision.Motion.TwoFramesDifferenceDetector()
            //{
            //  DifferenceThreshold = 15,
            //  SuppressNoise = true
            //};

            //detector = new AForge.Vision.Motion.CustomFrameDifferenceDetector()
            //{
            //  DifferenceThreshold = 15,
            //  KeepObjectsEdges = true,
            //  SuppressNoise = true
            //};

            detector = new AForge.Vision.Motion.SimpleBackgroundModelingDetector()
            {
                DifferenceThreshold = 10,
                FramesPerBackgroundUpdate = 10,
                KeepObjectsEdges = true,
                MillisecondsPerBackgroundUpdate = 0,
                SuppressNoise = true
            };

            //processor = new AForge.Vision.Motion.GridMotionAreaProcessing()
            //{
            //  HighlightColor = System.Drawing.Color.Red,
            //  HighlightMotionGrid = true,
            //  GridWidth = 100,
            //  GridHeight = 100,
            //  MotionAmountToHighlight = 100F
            //};

            processor = new AForge.Vision.Motion.BlobCountingObjectsProcessing()
            {
                HighlightColor = System.Drawing.Color.Red,
                HighlightMotionRegions = true,
                MinObjectsHeight = 10,
                MinObjectsWidth = 10
            };

            motionDetector = new AForge.Vision.Motion.MotionDetector(detector, processor);

            return (motionDetector);
        }
#endif

        public Image Image
        {
            get
            {
                return this.pictureBox1.Image;
            }
            set
            {
                ImageUtil.SetImage(this.pictureBox1, value);    // 2016/12/28
            }
        }

        private void cmbDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDevice.SelectedItem == null)
            {
                this.EndCatch();
                this.m_strSelectedCameraName = "";
                return;
            }
            this.m_strSelectedCameraName = ((Device)(cmbDevice.SelectedItem)).Moniker;
            StartCatch();
        }

        bool SelectListItem(string strMoniker)
        {
            foreach (Device device in cmbDevice.Items)
            {
                if (device.Moniker == strMoniker)
                {
                    cmbDevice.SelectedItem = device;
                    return true;
                }
            }

            cmbDevice.SelectedItem = null;
            this.camDevices.SelectCamera("");

            // string strTemp = this.camDevices.CurrentCameraMonier;

            {
                this.label_message.Text = "尚未指定摄像头";
                this.label_message.Visible = true;

                this.pictureBox1.Visible = false;
                OnFirstImageFilled(true);
            }
            return false;
        }
    }

    public delegate void CatchedEventHandler(object sender,
        CatchedEventArgs e);

    /// <summary>
    /// 捕捉完成的参数
    /// </summary>
    public class CatchedEventArgs : EventArgs
    {
        public BarcodeFormat BarcodeFormat = 0; // [out]
        public string Text = "";    // [out]
    }

    public delegate void FirstImageFilledEventHandler(object sender,
    FirstImageFilledEventArgs e);

    /// <summary>
    /// 第一帧图像到来的参数
    /// </summary>
    public class FirstImageFilledEventArgs : EventArgs
    {
        public bool Error = false;  // 是否 因为发生错误而第一帧图像不会到来

        public FirstImageFilledEventArgs(bool bError)
        {
            this.Error = bError;
        }
    }
}
