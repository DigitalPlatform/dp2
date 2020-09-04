using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

using DigitalPlatform;

namespace dp2Circulation
{
    // 2020/9/4
    /// <summary>
    /// 用于获得读者照片的对话框。从人脸中心获得视频流
    /// </summary>
    public partial class PatronPhotoDialog : Form
    {
        /*
        string m_strCurrentCamera = "";

        public string CurrentCamera
        {
            get
            {
                return m_strCurrentCamera;
            }
            set
            {
                m_strCurrentCamera = value;
            }
        }
        */

        // private DigitalPlatform.Drawing.QrRecognitionControl qrRecognitionControl1;

        public PatronPhotoDialog()
        {
            InitializeComponent();

            /*
            this.qrRecognitionControl1 = new DigitalPlatform.Drawing.QrRecognitionControl();
            this.qrRecognitionControl1.PhotoMode = true;

            // 
            // tabPage_camera
            // 
            this.panel1.Controls.Add(this.qrRecognitionControl1);
            // 
            // qrRecognitionControl1
            // 
            this.qrRecognitionControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.qrRecognitionControl1.Location = new System.Drawing.Point(0, 0);
            this.qrRecognitionControl1.Name = "qrRecognitionControl1";
            this.qrRecognitionControl1.Size = new System.Drawing.Size(98, 202);
            this.qrRecognitionControl1.TabIndex = 0;
            this.qrRecognitionControl1.BackColor = Color.DarkGray; //System.Drawing.SystemColors.Window;

            this.qrRecognitionControl1.FirstImageFilled += new FirstImageFilledEventHandler(qrRecognitionControl1_FirstImageFilled);
            */
        }

        /*
        void qrRecognitionControl1_FirstImageFilled(object sender, FirstImageFilledEventArgs e)
        {
            this.toolStripButton_getAndClose.Enabled = true;
        }
        */

        private void CameraPhotoDialog_Load(object sender, EventArgs e)
        {
            // this.qrRecognitionControl1.CurrentCamera = m_strCurrentCamera;

            BeginDisplayVideo();
        }

        private void CameraPhotoDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
            m_strCurrentCamera = this.qrRecognitionControl1.CurrentCamera;

            this.qrRecognitionControl1.EndCatch();
            */

            CancelDisplayVideo();
        }


        private void CameraPhotoDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public Image Image
        {
            get
            {
                return this.pictureBox1.Image;
                // return this.qrRecognitionControl1.Image;
            }
        }

        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            /*
            if (qrRecognitionControl1 != null)  // 可能控件还没有创建 2014/10/14
                qrRecognitionControl1.PerformAutoScale();
            */
        }

        private void toolStripButton_getAndClose_Click(object sender, EventArgs e)
        {
            /*
            if (this.qrRecognitionControl1.Image == null)
            {
                MessageBox.Show(this, "尚未开始捕捉图像，请稍候重试");
                return;
            }
            */
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void toolStripButton_ratate_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.Rotate90FlipNone);
        }

        private void toolStripButton_freeze_CheckedChanged(object sender, EventArgs e)
        {
            if (this.toolStripButton_freeze.Checked == true)
            {
                SetImageBorder(true);

                this.toolStripButton_ratate.Enabled = true;

                this.CancelDisplayVideo();
            }
            else
            {
                SetImageBorder(false);

                this.toolStripButton_ratate.Enabled = false;
                this.BeginDisplayVideo();
            }
            /*
            if (this.toolStripButton_freeze.Checked == true)
            {
                this.qrRecognitionControl1.SetImageBorder(true);

                this.toolStripButton_ratate.Enabled = true;

                this.qrRecognitionControl1.EndCatch();
                Application.DoEvents();
                Thread.Sleep(500);
                this.qrRecognitionControl1.DisplayImage(true);
            }
            else
            {
                this.qrRecognitionControl1.SetImageBorder(false);

                this.toolStripButton_ratate.Enabled = false;

                this.qrRecognitionControl1.DisplayText("重新开始捕捉");
                Application.DoEvents();
                Thread.Sleep(500);
                this.qrRecognitionControl1.StartCatch();
            }
            */
        }

        public void RotateImage(RotateFlipType flip_type)
        {
            Image image = this.pictureBox1.Image;
            image.RotateFlip(flip_type);

            pictureBox1.Width = image.Width;
            pictureBox1.Height = image.Height;
            DigitalPlatform.Drawing.ImageUtil.SetImage(pictureBox1, image);
        }

        public void SetImageBorder(bool bThick)
        {
            if (bThick == true)
                this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            else
                this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
        }

        // delegate void _RefreshCameraDevList();

        /// <summary>
        /// 刷新摄像头设备列表
        /// </summary>
        void RefreshCameraDevList()
        {
            // this.qrRecognitionControl1.RefreshDevList();
        }

        /// <summary>
        /// 窗口缺省过程函数
        /// </summary>
        /// <param name="m">消息</param>
        protected override void WndProc(ref Message m)
        {
            /*
            if (m.Msg == API.WM_DEVICECHANGE)
            {
                if (m.WParam.ToInt32() == API.DBT_DEVNODES_CHANGED)
                {
                    _RefreshCameraDevList d = new _RefreshCameraDevList(RefreshCameraDevList);
                    this.BeginInvoke(d);
                }
            }
            */
            base.WndProc(ref m);
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        Task _taskDisplayVideo = null;

        void CancelDisplayVideo()
        {
            if (_cancel != null)
            {
                _cancel.Cancel();
                _cancel.Dispose();
                _cancel = null;
            }
        }

        void BeginDisplayVideo()
        {
            CancelDisplayVideo();

            _cancel = new CancellationTokenSource();
            _taskDisplayVideo = Task.Run(() => {
                var result = DisplayVideo(Program.MainForm.FaceReaderUrl, _cancel.Token);
                if (_cancel != null && _cancel.IsCancellationRequested == false)
                    ShowMessageBox(result.ToString());
            });
        }

        void ShowMessageBox(string text)
        {
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(this, text);
            }));
        }

        NormalResult DisplayVideo(string url, CancellationToken token)
        {
            MyForm.FaceChannel channel = MyForm.StartFaceChannel(
    Program.MainForm.FaceReaderUrl,
    out string strError);
            if (channel == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            try
            {
                while (token.IsCancellationRequested == false)
                {
                    var result = channel.Object.GetImage("");
                    if (result.Value == -1)
                        return result;
                    using (MemoryStream stream = new MemoryStream(result.ImageData))
                    {
                        this.pictureBox1.Image = Image.FromStream(stream);
                    }
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = $"针对 {url} 的 GetImage() 请求失败: { ex.Message}";
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            finally
            {
                MyForm.EndFaceChannel(channel);
            }
        }

    }
}
