using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.Drawing
{
    public partial class CameraClipDialog : Form
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

        public CameraClipDialog()
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

            this.qrRecognitionControl1.FirstImageFilled += new EventHandler(qrRecognitionControl1_FirstImageFilled);
        }

        void qrRecognitionControl1_FirstImageFilled(object sender, EventArgs e)
        {
            // this.toolStripButton_getAndClose.Enabled = true;
            this.toolStrip1.Enabled = true;
        }

        private void CameraClipDialog_Load(object sender, EventArgs e)
        {
            this.qrRecognitionControl1.CurrentCamera = m_strCurrentCamera;

            tabControl_main_SelectedIndexChanged(this, e);
        }

        private void CameraClipDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_strCurrentCamera = this.qrRecognitionControl1.CurrentCamera;

            this.qrRecognitionControl1.EndCatch();
        }

        private void CameraClipDialog_FormClosed(object sender, FormClosedEventArgs e)
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

        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            if (qrRecognitionControl1 != null)  // 可能控件还没有创建 2014/10/14
                qrRecognitionControl1.PerformAutoScale();
        }

        private void toolStripButton_shoot_Click(object sender, EventArgs e)
        {
            this.qrRecognitionControl1.DisplayText("正在探测边沿 ...");
            Shoot();
            DetectEdge();
            this.qrRecognitionControl1.DisplayImage(true);
        }

        void Shoot()
        {
            Image temp = this.qrRecognitionControl1.Image;

            this.pictureBox_clip.Image = new Bitmap(temp);
            this.pictureBox_clip.InitialPoints(temp);

            this.tabControl_main.SelectedTab = this.tabPage_clip;
        }

        void DetectEdge()
        {
            if (this.pictureBox_clip.Image == null)
                return;

            Cursor old_cursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                double angle = 0;
                Rectangle rect;
                using (Bitmap bitmap = new Bitmap(this.pictureBox_clip.Image))
                {
                    // this.pictureBox1.Image = ImageUtil.AforgeAutoCrop(bitmap);

                    bool bRet = ImageUtil.GetSkewParam(bitmap,
                out angle,
                out rect);
                    if (bRet == false)
                    {
                        MessageBox.Show(this, "探测边框失败");
                        return;
                    }
                }

#if NO
            using (Bitmap bitmap = new Bitmap(this.pictureBox1.Image))
            {
                this.pictureBox1.Image = ImageUtil.Apply(bitmap,
                    angle,
                    rect);
            }
#endif

                List<Point> points = this.pictureBox_clip.ToPoints((float)angle, rect);
                this.pictureBox_clip.SetPoints(points);
            }
            finally
            {
                this.Cursor = old_cursor;
            }
        }

        private void toolStripButton_clip_output_Click(object sender, EventArgs e)
        {
            if (this.pictureBox_clip.Image == null)
            {
                MessageBox.Show(this, "没有可以输出的图像");
                return;
            }

            using (Bitmap bitmap = new Bitmap(this.pictureBox_clip.Image))
            {
                this.pictureBox_result.Image = ImageUtil.Clip(bitmap,
                    this.pictureBox_clip.GetCorners());
            }

            this.tabControl_main.SelectedTab = this.tabPage_result;
        }

        private void toolStripButton_clip_autoCorp_Click(object sender, EventArgs e)
        {
            DetectEdge();
        }

        private void toolStripButton_getAndClose_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_preview)
            {
                this.pictureBox_result.Image = this.qrRecognitionControl1.Image;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_clip)
            {
                using (Bitmap bitmap = new Bitmap(this.pictureBox_clip.Image))
                {
                    this.pictureBox_result.Image = ImageUtil.Clip(bitmap,
                        this.pictureBox_clip.GetCorners());
                }
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        public Image Image
        {
            get
            {
                return this.pictureBox_result.Image;
            }
            set
            {
                this.pictureBox_result.Image = value;
            }
        }

        private void toolStripButton_ratate_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_clip)
            {
                this.pictureBox_clip.RotateImage(RotateFlipType.Rotate90FlipNone);
            }
            if (this.tabControl_main.SelectedTab == this.tabPage_result)
            {
                Image image = this.pictureBox_result.Image;
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                this.pictureBox_result.Image = image;
            }
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_preview)
            {
                this.toolStripButton_ratate.Enabled = false;
                this.toolStripButton_clip_autoCorp.Enabled = false;
                this.toolStripButton_clip_output.Enabled = false;
                this.toolStripButton_paste.Enabled = false;
            }
            if (this.tabControl_main.SelectedTab == this.tabPage_clip)
            {
                this.toolStripButton_ratate.Enabled = true;
                this.toolStripButton_clip_autoCorp.Enabled = true;
                this.toolStripButton_clip_output.Enabled = true;
                this.toolStripButton_paste.Enabled = true;
            }
            if (this.tabControl_main.SelectedTab == this.tabPage_result)
            {
                this.toolStripButton_ratate.Enabled = true;
                this.toolStripButton_clip_autoCorp.Enabled = false;
                this.toolStripButton_clip_output.Enabled = false;
                this.toolStripButton_paste.Enabled = true;
            }

        }

        private void toolStripButton_copy_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.tabControl_main.SelectedTab == this.tabPage_preview)
            {
                if (this.qrRecognitionControl1.Image == null)
                {
                    strError = "图像为空，无法复制";
                    goto ERROR1;
                }
                Clipboard.SetImage(this.qrRecognitionControl1.Image);
            }
            if (this.tabControl_main.SelectedTab == this.tabPage_clip)
            {
                if (this.pictureBox_clip.Image == null)
                {
                    strError = "图像为空，无法复制";
                    goto ERROR1;
                }
                Clipboard.SetImage(this.pictureBox_clip.Image);
            }
            if (this.tabControl_main.SelectedTab == this.tabPage_result)
            {
                if (this.pictureBox_result.Image == null)
                {
                    strError = "图像为空，无法复制";
                    goto ERROR1;
                }
                Clipboard.SetImage(this.pictureBox_result.Image);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_paste_Click(object sender, EventArgs e)
        {
            string strError = "";

            Image image = null;
            IDataObject obj1 = Clipboard.GetDataObject();
            if (obj1.GetDataPresent(typeof(Bitmap)))
            {
                image = (Image)obj1.GetData(typeof(Bitmap));
            }
            else if (obj1.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])obj1.GetData(DataFormats.FileDrop);

                try
                {
                    image = Image.FromFile(files[0]);
                }
                catch (OutOfMemoryException)
                {
                    strError = "当前 Windows 剪贴板中的第一个文件不是图像文件。无法进行粘贴";
                    goto ERROR1;
                }
            }
            else
            {
                strError = "当前 Windows 剪贴板中没有图形对象。无法进行粘贴";
                goto ERROR1;
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_clip)
            {
                this.pictureBox_clip.Image = image;
                this.pictureBox_clip.InitialPoints(image);
            }
            if (this.tabControl_main.SelectedTab == this.tabPage_result)
            {
                this.pictureBox_result.Image = image;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
