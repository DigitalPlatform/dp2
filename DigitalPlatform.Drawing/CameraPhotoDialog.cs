using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace DigitalPlatform.Drawing
{
    public partial class CameraPhotoDialog : Form
    {
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

        private DigitalPlatform.Drawing.QrRecognitionControl qrRecognitionControl1;

        public CameraPhotoDialog()
        {
            InitializeComponent();

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
        }

        void qrRecognitionControl1_FirstImageFilled(object sender, FirstImageFilledEventArgs e)
        {
            this.toolStripButton_getAndClose.Enabled = true;
        }

        private void CameraPhotoDialog_Load(object sender, EventArgs e)
        {
            // if (string.IsNullOrEmpty(m_strCurrentCamera) == false)
            this.qrRecognitionControl1.CurrentCamera = m_strCurrentCamera;

        }

        private void CameraPhotoDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_strCurrentCamera = this.qrRecognitionControl1.CurrentCamera;

            this.qrRecognitionControl1.EndCatch();
        }


        private void CameraPhotoDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        delegate void Delegate_EndCatch();
        internal void EndCatch()
        {
            Delegate_EndCatch d = new Delegate_EndCatch(_endCatch);
            this.BeginInvoke(d);
        }

        void _endCatch()
        {
            this.qrRecognitionControl1.EndCatch();
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
                return this.qrRecognitionControl1.Image;
            }
        }

        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            // this.qrRecognitionControl1.Size = this.panel1.Size;
            /*
            this.qrRecognitionControl1.Dock = DockStyle.None;
            this.qrRecognitionControl1.Dock = DockStyle.Fill;
             * */
            if (qrRecognitionControl1 != null)  // 可能控件还没有创建 2014/10/14
                qrRecognitionControl1.PerformAutoScale();
        }

        private void toolStripButton_getAndClose_Click(object sender, EventArgs e)
        {
            if (this.qrRecognitionControl1.Image == null)
            {
                MessageBox.Show(this, "尚未开始捕捉图像，请稍候重试");
                return;
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void toolStripButton_freeze_Click(object sender, EventArgs e)
        {
        }

        private void toolStripButton_ratate_Click(object sender, EventArgs e)
        {
            this.qrRecognitionControl1.RotateImage(RotateFlipType.Rotate90FlipNone);
        }

        private void toolStripButton_freeze_CheckedChanged(object sender, EventArgs e)
        {
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
        }

        delegate void _RefreshCameraDevList();

        /// <summary>
        /// 刷新摄像头设备列表
        /// </summary>
        void RefreshCameraDevList()
        {
            this.qrRecognitionControl1.RefreshDevList();
        }

        /// <summary>
        /// 窗口缺省过程函数
        /// </summary>
        /// <param name="m">消息</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == API.WM_DEVICECHANGE)
            {
                if (m.WParam.ToInt32() == API.DBT_DEVNODES_CHANGED)
                {
                    _RefreshCameraDevList d = new _RefreshCameraDevList(RefreshCameraDevList);
                    this.BeginInvoke(d);
                }
            }
            base.WndProc(ref m);
        }
    }
}
