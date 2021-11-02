using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Drawing;
using DigitalPlatform.IO;

namespace dp2Circulation.Charging
{
    public partial class PalmprintForm : Form
    {
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public bool Pause { get; set; }

        Task _task = null;

        volatile bool _isActivated = false;

        public PalmprintForm()
        {
            InitializeComponent();
        }

        private void PalmprintForm_Load(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                this.toolStripButton_restartTask.Enabled = false;
            }));
            try
            {
                if (_task == null)
                {
                    var token = _cancel.Token;
                    StartTask(token);
                }
            }
            finally
            {
                if (this.Created)
                {
                    this.Invoke((Action)(() =>
                    {
                        this.toolStripButton_restartTask.Enabled = true;
                    }));
                }
            }
        }

        void StartTask(CancellationToken token)
        {
            _task = Task.Factory.StartNew(async () =>
            {
                this.Invoke((Action)(() =>
                {
                    this.toolStripButton_restartTask.Visible = false;
                }));
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

                        if (_disableSendkey)
                        {
                            // TODO: 显示为掌纹图像上面叠加文字则更好
                            DisplayError("临时禁用发送", Color.DarkGray);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                            continue;
                        }

                        if (this.Pause == true || FingerprintManager.Pause == true)
                        {
                            DisplayError("暂停显示", Color.DarkGray);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                            continue;
                        }

                        var result = FingerprintManager.GetImage("wait:1000,rect");
                        if (result.Value == -1)
                        {
                            // 显示错误
                            /*
                            DisplayError(result.ErrorInfo + "\r\n掌纹图像显示已停止", Color.DarkRed);
                            return;
                            */
                            DisplayError(result.ErrorInfo, Color.DarkRed);
                            await Task.Delay(TimeSpan.FromSeconds(5), token);
                            continue;
                        }

                        if (result.ImageData == null)
                        {
                            Thread.Sleep(50);
                            continue;
                        }

                        var image = ReaderInfoForm.FromBytes(result.ImageData);
                        if (string.IsNullOrEmpty(result.Text) == false)
                            PaintLines(image, result.Text);
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
                    // 显示错误
                    DisplayError($"显示线程出现异常: {ex.Message}\r\n掌纹图像显示已停止", Color.DarkRed);
                }
                finally
                {
                    _task = null;
                    this.Invoke((Action)(() =>
                    {
                        this.toolStripButton_restartTask.Visible = true;
                    }));
                }
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);

        }

        public static void PaintLines(Image image, string text, float line_width = 4)
        {
            string[] values = text.Split(new char[] { ',' });
            List<int> rect = new List<int>();
            foreach (string v in values)
            {
                rect.Add(Convert.ToInt32(v));
            }
            Debug.Assert(rect.Count == 8);
            using (var g = Graphics.FromImage(image))
            using (var pen = new Pen(Color.GreenYellow, line_width))
            {
                Point[] PointArray = new Point[]{ new Point(rect[0], rect[1]),
                    new Point(rect[2], rect[3]),
                    new Point(rect[4], rect[5]),
                    new Point(rect[6], rect[7]),
                    new Point(rect[0], rect[1])};

                g.DrawLines(pen, PointArray);
            }
        }

        void DisplayError(string strError, Color backColor)
        {
            // 显示错误
            if (this.Created)
            {
                this.Invoke((Action)(() =>
                {
                    this.Image = BuildTextImage(
                    strError,
                    backColor,
                    32,
                    this.pictureBox1.Width);
                }));
            }
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
        }

        private void PalmprintForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel?.Cancel();
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

        private void toolStripButton_restartTask_Click(object sender, EventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                this.toolStripButton_restartTask.Enabled = false;
            }));
            try
            {
                if (_task == null
                    || !(_task.Status == TaskStatus.Running || _task.Status == TaskStatus.Created))
                {
                    if (_cancel != null && _cancel.IsCancellationRequested)
                    {
                        _cancel?.Dispose();
                        _cancel = new CancellationTokenSource();
                    }

                    var token = _cancel.Token;
                    StartTask(token);
                }
            }
            finally
            {
                if (this.Created)
                {
                    this.Invoke((Action)(() =>
                    {
                        this.toolStripButton_restartTask.Enabled = true;
                    }));
                }
            }
        }

        bool _disableSendkey = false;

        // 临时禁用发送 key 功能
        public bool DisableSendKey
        {
            get
            {
                return this._disableSendkey;
            }
            set
            {
                this._disableSendkey = value;
            }
        }

        private void PalmprintForm_Activated(object sender, EventArgs e)
        {
            _isActivated = true;
        }

        private void PalmprintForm_Deactivate(object sender, EventArgs e)
        {
            _isActivated = false;
        }

        public bool IsActivated
        {
            get
            {
                return this._isActivated;
            }
        }
    }
}
