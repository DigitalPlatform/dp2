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

using DigitalPlatform.Core;
using DigitalPlatform.Face;
using DigitalPlatform.Interfaces;


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

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _cancelRefresh.Cancel();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FaceManager.Base.Name = "人脸中心";
            FaceManager.Url = "ipc://FaceChannel/FaceServer";
            FaceManager.SetError += FaceManager_SetError; ;
            FaceManager.Start(_cancelRefresh.Token);
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


        private async void InputFace_Click(object sender, RoutedEventArgs e)
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

            // SendKeys.SendWait(result.Patron);
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
            if (mode == "standby")
            {
                this.photo.Visibility = Visibility.Collapsed;
                this.inputFace.Visibility = Visibility.Visible;
                this.cancelButton.Visibility = Visibility.Collapsed;
                this.Width = 100;
                this.Height = 100;
            }
            if (mode == "video")
            {
                this.photo.Width = 300;
                this.photo.Height = 300;
                this.photo.Visibility = Visibility.Visible;
                this.inputFace.Visibility = Visibility.Collapsed;
                this.cancelButton.Visibility = Visibility.Visible;
                this.Width = 400;
                this.Height = 400;
            }
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
    }
}
