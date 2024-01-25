using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Runtime.CompilerServices;

using DigitalPlatform.IO;
using DigitalPlatform.WPF;
using DigitalPlatform;

namespace dp2SSL
{
    /// <summary>
    /// FingerprintVideoControl.xaml 的交互逻辑
    /// </summary>
    public partial class FingerprintVideoControl : UserControl
    {
        public FingerprintVideoControl()
        {
            InitializeComponent();
        }

        public void Hide()
        {
            App.Invoke(() =>
            {
                this.Visibility = Visibility.Collapsed;
            });
        }

        public void Show()
        {
            App.Invoke(() =>
            {
                this.Visibility = Visibility.Visible;
            });
        }

        public void StartDisplayFingerprint(
            CancellationToken token)
        {
            this.Show();

            Image image_control = this.photo;
            var lines_control = this.lines;
            var caption = PageBorrow.GetFingerprintCaption();

            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        if (string.IsNullOrEmpty(FingerprintManager.Url))
                        {
                            DisplayError($"尚未启用{caption}识别功能", System.Drawing.Color.DarkGray);
                            await Task.Delay(TimeSpan.FromSeconds(5), token);
                            continue;
                        }

                        /*
                        if (_disableSendkey)
                        {
                            // TODO: 显示为掌纹图像上面叠加文字则更好
                            DisplayError("临时禁用发送", Color.DarkGray);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                            continue;
                        }


                        */
                        if (/*this.Pause == true || */ FingerprintManager.Pause == true)
                        {
                            DisplayError("暂停显示", System.Drawing.Color.DarkGray);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                            continue;
                        }

                        var result = FingerprintManager.GetImage("wait:1000,rect");
                        if (result.Value == -1)
                        {
                            // 显示错误
                            DisplayError(result.ErrorInfo, System.Drawing.Color.DarkRed);
                            await Task.Delay(TimeSpan.FromSeconds(5), token);
                            continue;
                        }

                        if (result.ImageData == null)
                        {
                            Thread.Sleep(50);
                            continue;
                        }

                        PaintBytes(
                            result.ImageData,
                            out double width,
                            out double height);
                        if (lines_control != null
                            /*&& string.IsNullOrEmpty(result.Text) == false*/)
                            PaintLines(
                                result.Text,
                                width,
                                height);
                    }
                }
                catch (Exception ex)
                {
                    // 写入错误日志
                    WpfClientInfo.WriteErrorLog($"显示{caption}图像出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    // 显示错误
                    DisplayError($"显示线程出现异常: {ex.Message}\r\n{caption}图像显示已停止", System.Drawing.Color.DarkRed);
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);

        }

        public void PaintBytes(
            byte[] bytes,
            out double width,
            out double height)
        {
            Image control = this.photo;

            double w = 0;
            double h = 0;
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                App.Invoke(() =>
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = stream;
                    image.CacheOption = BitmapCacheOption.OnLoad;   // (注意这一句必须放在 .UriSource = ... 之后) 防止 WPF 一直锁定这个文件(即便 Image 都消失了还在锁定)
                    image.EndInit();

                    control.Source = image;

                    w = control.ActualWidth / image.Width;
                    h = control.ActualHeight / image.Height;
                });
            }
            width = w;
            height = h;
        }

        void DisplayError(
            string strError,
            System.Drawing.Color backColor)
        {
            Image control = this.photo;

            App.Invoke(() =>
            {
                BitmapImage image = new BitmapImage();

                image.BeginInit();
                image.StreamSource = StringToBitmapConverter.BuildTextImage(strError, backColor, 300);
                image.CacheOption = BitmapCacheOption.OnLoad;   // (注意这一句必须放在 .UriSource = ... 之后) 防止 WPF 一直锁定这个文件(即便 Image 都消失了还在锁定)
                image.EndInit();
                control.Source = image;
            });
        }

        public void PaintLines(
            string text,
            double scale_x,
            double scale_y,
            float line_width = 4)
        {
            var control = this.lines;

            if (string.IsNullOrEmpty(text))
            {
                App.Invoke(() =>
                {
                    if (control.Points == null
                        || control.Points.Count != 0)
                        control.Points = new PointCollection();
                });
                return;
            }
            string[] values = text.Split(new char[] { ',' });
            List<int> rect = new List<int>();
            foreach (string v in values)
            {
                rect.Add(Convert.ToInt32(v));
            }
            if (rect.Count != 8)
                throw new ArgumentException("应该是 8 个数字");
            Debug.Assert(rect.Count == 8);

            App.Invoke(() =>
            {
                var transform = control.RenderTransform as ScaleTransform;
                transform.ScaleX = scale_x;
                transform.ScaleY = scale_y;

                control.Points = new PointCollection
                {
                new Point(rect[0], rect[1]),
                new Point(rect[2], rect[3]),
                new Point(rect[4], rect[5]),
                new Point(rect[6], rect[7]),
                new Point(rect[0], rect[1])
                };
            });
        }

        public void SetQuality(string text)
        {
            App.Invoke(new Action(() =>
            {
                this.quality.Text = text;
            }));
        }
    }
}
