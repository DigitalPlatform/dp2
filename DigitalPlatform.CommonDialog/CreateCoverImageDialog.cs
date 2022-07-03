using DigitalPlatform.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonDialog
{
    /// <summary>
    /// 用于自动创建三种尺寸的封面图像的对话框
    /// </summary>
    public partial class CreateCoverImageDialog : Form
    {
        // 注：这里面包含的 Image 对象，为 ImageInfo 赋予者自行管理生命周期，本对话框不负责释放
        public ImageInfo ImageInfo { get; set; }

#if NO
        public Image OriginImage = null;    // 初始图像。本对话框用它创建出三种不同尺寸的封面图像
#endif

        List<ImageType> _defaultTypes = new List<ImageType>();

        public CreateCoverImageDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.flowLayoutPanel1.Controls.Count == 0)
            {
                strError = "没有可用的封面图像";
                goto ERROR1;
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void CreateCoverImageDialog_Load(object sender, EventArgs e)
        {
            InitialDefaultTypes();

            string strMessage = "";
            CreateImages(out strMessage);
            if (string.IsNullOrEmpty(strMessage) == false)
            {
                int nWidth = GetLargeWidth();
                MessageBox.Show(this, strMessage + "\r\n\r\n建议改用宽度在 " + nWidth + " 像素以上的图像作为原始图像");
            }
        }

        private void CreateCoverImageDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // List<ImageType> _resultImages = new List<ImageType>();

        public List<ImageType> ResultImages
        {
            get
            {
#if NO
                List<ImageType> image_types = new List<ImageType>();
                foreach (PictureBox box in this.flowLayoutPanel1.Controls)
                {
                    ImageType type = new ImageType();
                    type.Image = box.Image;
                    type.Width = (int)box.Tag;
                    image_types.Add(type);
                }

                return image_types;
#endif
                return this._defaultTypes;
            }
        }

        // 根据原始的 Image，创建三个不同尺寸的 Image
        int CreateImages(out string strMessage)
        {
            strMessage = "";

            if (this.ImageInfo == null || this.ImageInfo.Image == null)
                return 0;

            int nMediumWidth = 0;
            // 获得 medium 尺寸的宽度
            foreach (ImageType type in _defaultTypes)
            {
                if (type.TypeName == "MediumImage")
                    nMediumWidth = type.Width;
            }

            List<string> not_create = new List<string>();

            foreach (ImageType type in _defaultTypes)
            {
                Image result = null;

                if (type.Width == -1)
                {
                    result = this.ImageInfo.BackupImage;

                    // 2022/7/1
                    if (result != null)
                        result = result.Clone() as Image;

                    type.ProcessCommand = this.ImageInfo.ProcessCommand;
                }
                else if (this.ImageInfo.Image.Width >= type.Width)
                {
                    result = CreateImage(this.ImageInfo.Image,
                        type.Width,
                        out bool created);
                    if (created == false)
                    {
                        result = result.Clone() as Image;
                        // result = CopyImage(result);    //复制图像 2022/7/1
                    }
                }
                else
                {
                    if (type.TypeName == "LargeImage" && nMediumWidth != 0
                        && this.ImageInfo.Image.Width > nMediumWidth)
                    {
                        result = new Bitmap(this.ImageInfo.Image);
                    }
                    else
                    {
                        not_create.Add(type.Width.ToString());
                        continue;
                    }
                }

                PictureBox box = new PictureBox();
                box.SizeMode = PictureBoxSizeMode.AutoSize;
                box.Image = result;
                box.Tag = result.Width;

                type.Image = result;

                this.flowLayoutPanel1.Controls.Add(box);
            }

            if (not_create.Count > 0)
            {
                strMessage = "下列宽度的图像版本没有创建: \r\n";
                foreach (string width in not_create)
                {
                    strMessage += width + "\r\n";
                }
            }

            return 0;
        }

        static Bitmap CopyImage(Image source)
        {
            Bitmap img = new Bitmap(source.Width, source.Height);
            using (Graphics gr = Graphics.FromImage(img))
            {
                gr.DrawImage(source, new Point(0, 0));
            }
            return img;
        }

        Image CreateImage(Image image,
            int nWidth,
            out bool created)
        {
            created = false;
            Image result = null;

            if (nWidth == -1)
                return image;

            int nOldWidth = image.Width;
            if (nOldWidth == nWidth)
                return image;

            result = new Bitmap(image);

            string strError = "";
            // 缩小图像
            // parameters:
            //		nNewWidth0	宽度(0表示不变化)
            //		nNewHeight0	高度
            //      bRatio  是否保持纵横比例
            // return:
            //      -1  出错
            //      0   没有必要缩放(objBitmap未处理)
            //      1   已经缩放
            int nRet = DigitalPlatform.Drawing.GraphicsUtil.ShrinkPic(ref result,
                nWidth,
                0,
                true,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);
            created = true;
            return result;
        }

        void ClearDefaultTypes()
        {
            foreach (ImageType type in _defaultTypes)
            {
                // 不要 Dispose() this.ImageInfo 管辖的两个图像对象
                if (this.ImageInfo != null)
                {
                    if (type.Image == this.ImageInfo.Image)
                        continue;
                    if (type.Image == this.ImageInfo.BackupImage)
                        continue;
                }

                if (type.Image != null)
                {
                    type.Image.Dispose();
                    type.Image = null;
                }
            }

            _defaultTypes.Clear();
        }

        // 三个尺寸的宽度
        // public static readonly int[] widths = new int[] { 61, 131, 408 };

        void InitialDefaultTypes()
        {
            ClearDefaultTypes();

            ImageType type = new ImageType();
            type.Width = 61;
            type.TypeName = "SmallImage";
            _defaultTypes.Add(type);

            type = new ImageType();
            type.Width = 131;
            type.TypeName = "MediumImage";
            _defaultTypes.Add(type);

            type = new ImageType();
            type.Width = 408;
            type.TypeName = "LargeImage";
            _defaultTypes.Add(type);

            if (this.ImageInfo != null && this.ImageInfo.BackupImage != null)
            {
                type = new ImageType();
                type.Width = -1;
                type.TypeName = "BackupImage";
                _defaultTypes.Add(type);
            }
        }

        ImageType GetImageType(string strTypeName)
        {
            // 获得 medium 尺寸的宽度
            foreach (ImageType type in _defaultTypes)
            {
                if (type.TypeName == strTypeName)
                    return type;
            }

            return null;
        }

        public int GetLargeWidth()
        {
            ImageType type = GetImageType("LargeImage");
            if (type != null)
                return type.Width;

            return 0;
        }
    }

    public class ImageType
    {
        public string TypeName = "";    // LargeImage / MediumImage / SmallImage / BackupImage
        public int Width = 0;
        public Image Image = null;
        public string ProcessCommand = "";    // 剪裁指令
    }
}
