using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.Drawing
{
    public static class ImageUtil
    {
        // 2016/12/28
        // 设置 PictureBox 的 Image 成员，自动释放被替代的 Image，防止内存泄露
        public static void SetImage(PictureBox box, Image image)
        {
            Image old = box.Image;
            if (image != null && old == image)
            {
                box.Image = image;  // 可迫使刷新
                return;
            }

            Debug.Assert(image == null || old != image, "");
            box.Image = image;
            if (old != null)
                old.Dispose();
        }

        // return:
        //      null    出错。错误信息在 strError 中
        //      其它      返回的图像列表
        public static List<Image> GetImagesFromClipboard(out string strError)
        {
            strError = "";

            // 从剪贴板中取得图像对象
            List<Image> images = new List<Image>();
            IDataObject obj1 = Clipboard.GetDataObject();
            if (obj1.GetDataPresent(typeof(Bitmap)))
            {
                images.Add((Image)obj1.GetData(typeof(Bitmap)));
            }
            else if (obj1.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])obj1.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    try
                    {
                        images.Add(Image.FromFile(file));
                    }
                    catch (OutOfMemoryException)
                    {
                        strError = "当前 Windows 剪贴板中的某个文件不是图像文件";    // 。无法创建封面图像
                        return null;
                    }
                }
            }
            else
            {
                strError = "当前 Windows 剪贴板中没有图形对象"; // 。无法创建封面图像
                return null;
            }

            return images;
        }
    }

    public class ImageInfo
    {
        public Image Image { get; set; }
        public Image BackupImage { get; set; }
        public string ProcessCommand { get; set; }

        public void Dispose()
        {
            if (this.Image != null)
            {
                this.Image.Dispose();
                this.Image = null;
            }
            if (this.BackupImage != null)
            {
                this.BackupImage.Dispose();
                this.BackupImage = null;
            }
        }

        // 清除原始图像和处理指令
        public void ClearBackupImage()
        {
            if (this.BackupImage != null)
            {
                this.BackupImage.Dispose();
                this.BackupImage = null;
            }
            this.ProcessCommand = "";
        }
    }
}
