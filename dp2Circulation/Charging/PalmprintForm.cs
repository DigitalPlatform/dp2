using DigitalPlatform;
using DigitalPlatform.Drawing;
using DigitalPlatform.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Circulation.Charging
{
    public partial class PalmprintForm : Form
    {
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public bool Pause { get; set; }

        public PalmprintForm()
        {
            InitializeComponent();
        }

        private void PalmprintForm_Load(object sender, EventArgs e)
        {
            var token = _cancel.Token;

            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    while (token.IsCancellationRequested == false)
                    {
                        if (string.IsNullOrEmpty(FingerprintManager.Url))
                        {
                            DisplayError("尚未启用掌纹识别功能", Color.DarkGray);
                            await Task.Delay(TimeSpan.FromSeconds(5), token);
                            continue;
                        }

                        if (this.Pause == true || FingerprintManager.Pause == true)
                        {
                            DisplayError("暂停显示", Color.DarkGray);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                            continue;
                        }

                        var result = FingerprintManager.GetImage("wait:1000");
                        if (result.Value == -1)
                        {
                            // 显示错误
                            DisplayError(result.ErrorInfo + "\r\n掌纹图像显示已停止", Color.DarkRed);
                            return;
                        }

                        if (result.ImageData == null)
                        {
                            Thread.Sleep(50);
                            continue;
                        }

                        var image = ReaderInfoForm.FromBytes(result.ImageData);
                        this.Invoke(new Action(() =>
                        {
                            this.Image = image;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    // 写入错误日志
                    MainForm.WriteErrorLog($"显示掌纹图像出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        void DisplayError(string strError, Color backColor)
        {
            // 显示错误
            this.Invoke((Action)(() =>
            {
                this.Image = BuildTextImage(
                strError,
                backColor,
                32,
                this.pictureBox1.Width);
            }));
        }

        public static Bitmap BuildTextImage(string strText,
            Color backColor,
    float fFontSize = 64,
    int nWidth = 400)
        {
            // 文字图片
            return ArtText.BuildArtText(
                strText,
                "Microsoft YaHei",  // "Consolas", // 
                fFontSize,  // (float)16,
                FontStyle.Regular,  // .Bold,
                Color.White,
                backColor,  // Color.DarkRed,
                Color.Gray,
                ArtEffect.None,
                nWidth);
        }

        private void PalmprintForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel?.Cancel();
        }

        private void PalmprintForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel?.Dispose();
        }

        public Image Image
        {
            get
            {
                return this.pictureBox1.Image;
            }
            set
            {
                ImageUtil.SetImage(this.pictureBox1, value);
            }
        }

        public string MessageText
        {
            get
            {
                return this.toolStripStatusLabel1.Text;
            }
            set
            {
                this.toolStripStatusLabel1.Text = value;
            }
        }

    }
}
