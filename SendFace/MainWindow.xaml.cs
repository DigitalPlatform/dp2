using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.Face;
using DigitalPlatform.Interfaces;
using DigitalPlatform.WPF;
using SendFace.Properties;

namespace SendFace
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource _cancelRefresh = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;

            _errorTable = new ErrorTable((s) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(s) == false)
                        this.error.Visibility = Visibility.Visible;
                    else
                        this.error.Visibility = Visibility.Collapsed;
                    this.error.Text = s;
                }));
            });
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            string value = Settings.Default.windowPosition;
            if (string.IsNullOrEmpty(value) == false)
                _position = value;
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            InterceptMouse.MouseClick -= InterceptMouse_MouseClick;

            _cancelRefresh.Cancel();

            Settings.Default.windowPosition = _position;
            Settings.Default.Save();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FaceManager.Base.Name = "人脸中心";
            FaceManager.Url = "ipc://FaceChannel/FaceServer";
            FaceManager.SetError += FaceManager_SetError; ;
            FaceManager.Start(_cancelRefresh.Token);

            InterceptMouse.MouseClick += InterceptMouse_MouseClick;

            SetWindowPos(_position);

            // 后台自动检查更新
            var task = Task.Run(() =>
            {
                try
                {
                    NormalResult result = WpfClientInfo.InstallUpdateSync();
                    if (result.Value == -1)
                        SetError("update", "自动更新出错: " + result.ErrorInfo);
                    else if (result.Value == 1)
                    {
                        SetError("update", result.ErrorInfo);
                        // Updated?.Invoke(this, new UpdatedEventArgs { Message = result.ErrorInfo });
                    }
                    else if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                        SetError("update", result.ErrorInfo);
                }
                catch(Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"自动后台更新出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });
        }

        private void InterceptMouse_MouseClick(object sender, MouseClickEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Rect rect = new Rect(this.Left, this.Top, this.Width, this.Height);
                Point pt = RealPixelsToWpf(this, e.Location);
                // message.Content = $"x={e.Location.X},y={e.Location.Y}";
                if (rect.Contains(pt))
                {
                    // System.Windows.Forms.SendKeys.SendWait($"x={e.Location.X},y={e.Location.Y}\r\n");
                    OnClicked();
                    e.Handled = true;
                }
            }));
        }

        static Point RealPixelsToWpf(Window w, Point p)
        {
            var t = PresentationSource.FromVisual(w).CompositionTarget.TransformFromDevice;
            return t.Transform(p);
        }

        void OnClicked()
        {
            if (this.cancelButton.Visibility == Visibility.Visible)
            {
                // 停止识别
                Task.Run(() =>
                {
                    CancelButton_Click(this, new RoutedEventArgs());
                });

            }
            else
            {
                // 开始识别
                Task.Run(() =>
                {
                    recognitionFace();
                });
            }
        }

        private void FaceManager_SetError(object sender, DigitalPlatform.IO.SetErrorEventArgs e)
        {
            SetError("face", e.Error);
        }

        ErrorTable _errorTable = null;

        public void SetError(string type, string error)
        {
            /*
            if (type == "face" && error != null)
            {
                Debug.Assert(false, "");
            }
            */

            _errorTable.SetError(type, error);
        }

        public void ClearErrors(string type)
        {
            _errorTable.SetError(type, "");
        }

        async void recognitionFace()
        {
            EnterMode("video");

            RecognitionFaceResult result = null;

            _stopVideo = false;
            var task = Task.Run(() =>
            {
                DisplayVideo();
            });
            try
            {
                result = await RecognitionFace("");
                if (result.Value == -1)
                {
                    if (result.ErrorCode != "cancelled")
                        SetError("face", result.ErrorInfo);
                    return;
                }

                SetError("face", null);
            }
            finally
            {
                FaceManager.CancelRecognitionFace();
                _stopVideo = true;

                EnterMode("standby");
            }

            System.Windows.Forms.SendKeys.SendWait(result.Patron + "\r");
        }

        async Task<RecognitionFaceResult> RecognitionFace(string style)
        {
            // EnableControls(false);
            try
            {
                return await Task.Run<RecognitionFaceResult>(() =>
                {
                    var result = FaceManager.GetState("camera");
                    if (result.Value == -1)
                        return new RecognitionFaceResult
                        {
                            Value = -1,
                            ErrorInfo = result.ErrorInfo,
                            ErrorCode = result.ErrorCode
                        };
                    return FaceManager.RecognitionFace("");
                });
            }
            finally
            {
                // EnableControls(true);
            }
        }


        // parameters:
        //      mode    "standby" "video"
        public void EnterMode(string mode)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {

                if (mode == "standby")
                {
                    this.photo.Visibility = Visibility.Collapsed;
                    this.inputFace.Visibility = Visibility.Visible;
                    this.cancelButton.Visibility = Visibility.Collapsed;
                    this.Width = 100;
                    // this.Height = 100;
                    this.Background = new SolidColorBrush(Colors.Transparent);

                    SetWindowPos(_position);
                }
                if (mode == "video")
                {
                    this.photo.Width = 300;
                    this.photo.Height = 300;
                    this.photo.Visibility = Visibility.Visible;
                    this.inputFace.Visibility = Visibility.Collapsed;
                    this.cancelButton.Visibility = Visibility.Visible;
                    this.Width = 400;
                    // this.Height = 400;
                    this.Background = new SolidColorBrush(Colors.DarkGray);

                    SetWindowPos(_position);

                }
            }));
        }

        string _position = "right_bottom";

        void SetWindowPos(string position)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;

            if (position == "right_bottom")
            {
                this.Left = desktopWorkingArea.Right - this.Width;
                this.Top = desktopWorkingArea.Bottom - this.Height;
            }
            else if (position == "left_bottom")
            {
                this.Left = 0;
                this.Top = desktopWorkingArea.Bottom - this.Height;
            }
            else if (position == "right_top")
            {
                this.Left = desktopWorkingArea.Right - this.Width;
                this.Top = 0;
            }
            else if (position == "left_top")
            {
                this.Left = 0;
                this.Top = 0;
            }
            else
                throw new ArgumentException($"未知的 position '{position}'");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // EnterMode("standby");

            FaceManager.CancelRecognitionFace();
            _stopVideo = true;
        }

        bool _stopVideo = false;

        void DisplayVideo()
        {
            while (_stopVideo == false)
            {
                var result = FaceManager.GetImage("");
                if (result.ImageData == null)
                {
                    Thread.Sleep(500);
                    continue;
                }
                MemoryStream stream = new MemoryStream(result.ImageData);
                try
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        SetPhoto(stream);
                    }));
                    stream = null;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
        }

        public void SetPhoto(Stream stream)
        {
            if (stream == null)
            {
                this.photo.Source = null;
                return;
            }
            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = stream;
            imageSource.EndInit();
            this.photo.Source = imageSource;
        }

        private void InputFace_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu cm = this.FindResource("menu") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Menu_leftTop_Click(object sender, RoutedEventArgs e)
        {
            _position = "left_top";
            SetWindowPos(_position);
        }

        private void Menu_rightTop_Click(object sender, RoutedEventArgs e)
        {
            _position = "right_top";
            SetWindowPos(_position);
        }

        private void Menu_leftBottom_Click(object sender, RoutedEventArgs e)
        {
            _position = "left_bottom";
            SetWindowPos(_position);
        }

        private void Menu_rightBottom_Click(object sender, RoutedEventArgs e)
        {
            _position = "right_bottom";
            SetWindowPos(_position);
        }
    }
}
