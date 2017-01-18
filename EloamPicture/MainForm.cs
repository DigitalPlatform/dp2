using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

using eloamComLib;
using EloamPicture.Properties;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform;
using DigitalPlatform.Drawing;

namespace EloamPicture
{
    public partial class MainForm : Form
    {
        public ApplicationInfo AppInfo = null;  // new ApplicationInfo("dp2circulation.xml");

        private EloamGlobal global;
        private EloamVideo video;
        private int timer_value;

        private EloamImage globalTempImage;

        public EloamMemory m_pTemplate;
        public EloamMemory m_pFeature;

        bool hasClickOcrList = false;
        //定义系统计时器
        private System.Timers.Timer timer;

        //设备列表
        private List<EloamDevice> deviceList;

        public MainForm()
        {
            InitializeComponent();

            global = new EloamGlobal();
            deviceList = new List<EloamDevice>();

            m_pFeature = null;
            m_pTemplate = null;

            FormInit();

            Init();
        }

        private void toolStripButton_start_Click(object sender, EventArgs e)
        {
        }

        private void toolStripButton_stop_Click(object sender, EventArgs e)
        {
        }

        static uint RGB(uint r, uint g, uint b)
        {
            return (((b << 16) | (g << 8)) | r);
        }

        public void FormInit()
        {
            //传入设备状态改变事件
            global.DevChange += DevChangeEventHandler;
#if NO
            //传入移动监测事件
            global.MoveDetec += MoveDetecEventHandler;
#endif
            //视频播放事件
            global.Arrival += ArrivalEventHandler;

#if NO
            //二代证
            global.IdCard += IdCardEventHandler;
            //Ocr识别事件
            global.Ocr += OcrEventHandler;
#endif

            //初始化设备
            global.InitDevs();

#if NO
            if (!global.InitIdCard())
            {
                button_StartIdCardRead.Enabled = false;
                button_StopReadIDCard.Enabled = false;
            }

            if (global.InitBarcode())
            {
                barcode.Enabled = true;
            }
            else
            {
                barcode.Enabled = false;
            }
            if (global.InitOcr())
            {
                ocr.Enabled = true;
                ocrList.Enabled = true;
            }
#endif
        }

        void FormEnd()
        {
            // closeVideo_Click(new object(), new EventArgs());
            toolStripButton_stop_Click(new object(), new EventArgs());

            int count = deviceList.Count;
            if (count != 0)
            {
                for (int i = 0; i < count; i++)
                {
                    deviceList[i].Destroy();
                }
            }
            deviceList.Clear();

            global.DeinitBarcode();

            if (null != m_pTemplate)
            {
                m_pTemplate.Destroy();
                m_pTemplate = null;
            }
            if (null != m_pFeature)
            {
                m_pFeature.Destroy();
                m_pFeature = null;
            }

            global.DeinitBiokey();
            global.DeinitIdCard();
            global.DeinitDevs();
            global.DeinitOcr();

            //传出设备状态改变事件
            global.DevChange -= DevChangeEventHandler;
#if NO
            //传出移动监测事件
            global.MoveDetec -= MoveDetecEventHandler;
#endif
            //传出视频播放事件
            global.Arrival -= ArrivalEventHandler;
#if NO
            //传出二代证
            global.IdCard -= IdCardEventHandler;
            //传出Ocr识别事件
            global.Ocr -= OcrEventHandler;
#endif
        }

        //设备状态改变事件响应
        public void DevChangeEventHandler(int type, int idx, int dbt)
        {
            if (1 == type)
            {
                if (1 == dbt)//设备到达
                {
                    EloamDevice tempDevice = (EloamDevice)global.CreateDevice(1, idx);
                    deviceList.Add(tempDevice);

                    selectDevice.Items.Add(tempDevice.GetFriendlyName());
                    if (-1 == selectDevice.SelectedIndex)
                    {
                        selectDevice.SelectedIndex = 0;//改变所选设备
                    }

                }
                else if (2 == dbt)//设备丢失
                {
                    EloamDevice tempDevice = deviceList[idx];
                    if (null != video)
                    {
                        EloamDevice tempDevice2 = (EloamDevice)video.GetDevice();
                        if (tempDevice == tempDevice2)
                        {
                            // closeVideo_Click(new object(), new EventArgs());

                            toolStripButton_stop_Click(new object(), new EventArgs());
                        }
                    }

                    deviceList[idx].Destroy();
                    deviceList.RemoveAt(idx);
                    selectDevice.Items.RemoveAt(idx);
                    if (-1 == selectDevice.SelectedIndex)
                    {
                        if (0 != deviceList.Count)
                        {
                            selectDevice.SelectedIndex = 0;
                        }
                        else
                        {
                            selectDevice.Items.Clear();
                            selectMode.Items.Clear();
                            selectResolution.Items.Clear();
                        }
                    }
                }
            }
        }

        //视频播放事件
        private void ArrivalEventHandler(object pVideo, int id)
        {
#if NO
            if (0 == id)//视频第一帧
            {
                openVideo.Enabled = false;
                closeVideo.Enabled = true;
                turnLeft.Enabled = true;
                turnRight.Enabled = true;
                exchangeLeftRight.Enabled = true;
                exchangeUpDown.Enabled = true;
                openProperty.Enabled = true;

                rectify.Enabled = true;
                removeGround.Enabled = true;
                autoShoot.Enabled = true;

                openTimer.Enabled = true;
                edit_Timer.Enabled = true;

                compoundShoot.Enabled = true;

                pictureSavePath.Enabled = true;
                shoot.Enabled = true;

                barcode.Enabled = true;
            }
#endif
        }


        public void Reset()
        {
            Init();
        }

        public void Init()
        {
            selectDevice.Enabled = true;
            selectResolution.Enabled = true;
            selectMode.Enabled = true;

#if NO
            openVideo.Enabled = true;
            closeVideo.Enabled = false;
            turnLeft.Enabled = false;
            turnRight.Enabled = false;
            exchangeLeftRight.Enabled = false;
            exchangeUpDown.Enabled = false;
            openProperty.Enabled = false;

            rectify.Enabled = false;
            rectify.Checked = false;
            removeGround.Enabled = false;
            removeGround.Checked = false;
            autoShoot.Enabled = false;
            autoShoot.Checked = false;

            openTimer.Enabled = false;
            openTimer.Checked = false;
            edit_Timer.Enabled = false;

            compoundShoot.Enabled = false;
            compoundShoot.Checked = false;

            pictureSavePath.Enabled = false;
            shoot.Enabled = false;
#endif

            timer_value = 5;

#if NO
            edit_Timer.Text = timer_value.ToString();
            pictureSavePath.Text = "D:";

            barcode.Enabled = false;
#endif
        }

#if NO
        void LoadSettings()
        {
            this.pictureSavePath.Text = Settings.Default.picture_directory;
        }

        void SaveSettings()
        {
            Settings.Default.picture_directory = this.pictureSavePath.Text;

            Settings.Default.Save();
        }
#endif

        public string UserDir { get; set; }

        public string OutputDir { get; set; }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // LoadSettings();

            this.UserDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "eloamPicture_v1");
            PathUtil.CreateDirIfNeed(this.UserDir);

            this.OutputDir = Path.Combine(this.UserDir, "pic");
            PathUtil.CreateDirIfNeed(this.OutputDir);
            this.pictureSavePath.Text = this.OutputDir;

            this.AppInfo = new ApplicationInfo(Path.Combine(this.UserDir, "eloamPicture.xml"));

            AppInfo.LoadFormStates(this,
    "mainformstate",
    FormWindowState.Maximized);

            this.BeginInvoke(new Action<object, EventArgs>(toolStripButton_preview_start_Click), this, new EventArgs());
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormEnd();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // SaveSettings();

            if (this.AppInfo != null)
            {
                AppInfo.SaveFormStates(this, "mainformstate");

                this.AppInfo.Save();
            }
        }

        private void selectMode_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void selectDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = selectDevice.SelectedIndex;//记录当前所选设备

            selectMode.Items.Clear();

            if (-1 != idx)
            {
                EloamDevice tempDevice = deviceList[idx];

                //加载模式列表
                int subtype = tempDevice.GetSubtype();
                if (0 != (subtype & 1))
                {
                    selectMode.Items.Add("YUY2");
                }
                if (0 != (subtype & 2))
                {
                    selectMode.Items.Add("MJPG");
                }
                if (0 != (subtype & 4))
                {
                    selectMode.Items.Add("UYVY");
                }

                //若为辅摄像头，优先选择MJPG方式
                if (1 != tempDevice.GetEloamType() && 0 != (subtype & 2))
                {
                    selectMode.SelectedIndex = 1;
                }
                else
                {
                    selectMode.SelectedIndex = 0;
                }

                this.toolStripButton_preview_start.Enabled = true;
            }
            else
            {
                this.toolStripButton_preview_start.Enabled = false;
            }


        }

        void Shoot()
        {
#if NO
            if (null == video)
            {
                return;
            }

            EloamView tempView = (EloamView)eloamView.GetView();
            EloamImage tempImage = (EloamImage)video.CreateImage(0, tempView);

            if (null != tempImage)
            {

                DateTime dateTime = DateTime.Now;
                string time = DateTime.Now.ToString(
                    "yyyyMMdd_HHmmss_fff", DateTimeFormatInfo.InvariantInfo);
                string filename = pictureSavePath.Text + "\\Manual_" + time + ".jpg";

                {
                    if (tempImage.Save(filename, 0))
                    {
                        eloamView.PlayCaptureEffect();
                        // eloamThumbnail.Add(filename);
                    }
                    else
                    {
                        MessageBox.Show("保存失败，请检查保存路径设置是否正确!");
                    }

                }

            }
#endif
            if (this.toolStripButton_preview_start.Enabled == true)
                toolStripButton_preview_start_Click(this, new EventArgs());

            if (null == video)
            {
                return;
            }

            EloamView tempView = (EloamView)eloamView.GetView();
            EloamImage tempImage = (EloamImage)video.CreateImage(0, tempView);

            if (null != tempImage)
            {

                DateTime dateTime = DateTime.Now;
                string time = DateTime.Now.ToString(
                    "yyyyMMdd_HHmmss_fff", DateTimeFormatInfo.InvariantInfo);
                string filename = pictureSavePath.Text + "\\Manual_" + time + ".jpg";

                {
                    if (tempImage.Save(filename, 0))
                    {
                        eloamView.PlayCaptureEffect();
                        // eloamThumbnail.Add(filename);
                    }
                    else
                    {
                        MessageBox.Show("保存失败，请检查保存路径设置是否正确!");
                    }

                    Image temp = Image.FromFile(filename);
                    ImageUtil.SetImage(this.pictureBox_clip, new Bitmap(temp)); // 2016/12/28
                    this.pictureBox_clip.InitialPoints(temp);
                    temp.Dispose();
                    File.Delete(filename);
                }

                this.tabControl_main.SelectedTab = this.tabPage_clip;
            }
        }

        private void rectify_CheckedChanged(object sender, EventArgs e)
        {
            if (null != video)
            {
                if (rectify.Checked)
                {
                    video.EnableDeskew(0);
                }
                else
                {
                    video.DisableDeskew();
                }
            }
        }

        private void toolStripButton_preview_start_Click(object sender, EventArgs e)
        {
            int devIdx = selectDevice.SelectedIndex;
            string curModeString = selectMode.SelectedItem.ToString();
            int modeIdx = (curModeString == "YUY2" ? 1 :
                            (curModeString == "MJPG" ? 2 :
                                (curModeString == "UYVY" ? 4 :
                                    -1)));
            int resIdx = selectResolution.SelectedIndex;

            if (-1 != devIdx)
            {
                if (null != video)
                {
                    video.Destroy();
                    video = null;
                }

                EloamDevice tempDevice = deviceList[devIdx];
                video = (EloamVideo)tempDevice.CreateVideo(resIdx, modeIdx);

                if (null != video)
                {
                    eloamView.SelectVideo(video);
                    eloamView.SetText("打开视频中，请等待...", RGB(255, 255, 255));

                    selectDevice.Enabled = false;
                    selectResolution.Enabled = false;
                    selectMode.Enabled = false;

                    //openVideo.Enabled = false;
                    //closeVideo.Enabled = true;
                }
            }

            this.toolStripButton_preview_start.Enabled = false;
            this.toolStripButton_preview_stop.Enabled = true;
        }

        private void toolStripButton_preview_stop_Click(object sender, EventArgs e)
        {
            if (null != video)
            {
                eloamView.SetText(null, 0);
                video.Destroy();
                video = null;
            }

            eloamView.SetText(null, 0);

            Reset();

            this.toolStripButton_preview_start.Enabled = true;
            this.toolStripButton_preview_stop.Enabled = false;
        }

        // 取图，并自动探测边界
        private void toolStripButton_preview_shoot_Click(object sender, EventArgs e)
        {
            Shoot();
            DetectEdge();
        }

        // 取图，但并不自动探测边界
        private void toolStripButton_clip_shoot_Click(object sender, EventArgs e)
        {
            Shoot();
        }

        void DetectEdge()
        {
            if (this.pictureBox_clip.Image == null)
                return;

            double angle = 0;
            Rectangle rect;
            using (Bitmap bitmap = new Bitmap(this.pictureBox_clip.Image))
            {
                // this.pictureBox1.Image = ImageUtil.AforgeAutoCrop(bitmap);
                DetectBorderParam param = new DetectBorderParam(bitmap);

                bool bRet = AForgeImageUtil.GetSkewParam(bitmap,
                    param,
                    out angle,
                    out rect);
                if (bRet == false)
                {
                    MessageBox.Show(this, "fail");
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

        // 自动探测边沿
        private void toolStripButton_clip_autoCorp_Click(object sender, EventArgs e)
        {
            DetectEdge();
        }

        // 输出
        private void toolStripButton_clip_output_Click(object sender, EventArgs e)
        {
            if (this.pictureBox_clip.Image == null)
            {
                MessageBox.Show(this, "没有可以输出的图像");
                return;
            }

            using (Bitmap bitmap = new Bitmap(this.pictureBox_clip.Image))
            {
                ImageUtil.SetImage(this.pictureBox_result, AForgeImageUtil.Clip(bitmap,
                    this.pictureBox_clip.GetCorners()));    // 2016/12/28
            }

            this.tabControl_main.SelectedTab = this.tabPage_result;

            string strFileName = this.GetNewOutputFileName();
            if (string.IsNullOrEmpty(strFileName) == false)
            {
                this.pictureBox_result.Image.Save(strFileName);

                this.SetStatusMessage("成功创建图像文件 " + strFileName);
            }
            else
                this.SetStatusMessage("尚未指定输出目录");
        }

        void SetStatusMessage(string strText)
        {
            this.toolStripStatusLabel_message.Text = strText;
        }

        private void toolStripButton_clip_Rotate_Click(object sender, EventArgs e)
        {
            this.pictureBox_clip.RotateImage(RotateFlipType.Rotate90FlipNone);
        }

        string GetNewOutputFileName()
        {
            if (string.IsNullOrEmpty(this.pictureSavePath.Text))
                return "";

            string strDate = DateTime.Now.ToString("yyyyMMdd");
            for (int index = 1; ; index++)
            {
                string strNumber = index.ToString();
                string strFileName = Path.Combine(this.OutputDir, strDate + "_" + strNumber + ".png");
                if (File.Exists(strFileName) == false)
                    return strFileName;
            }
        }

        // 打开输出文件夹
        private void button_setting_openOutputFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.OutputDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

    }
}
