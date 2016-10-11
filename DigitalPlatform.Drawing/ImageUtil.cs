using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.Drawing
{
    public static class ImageUtil
    {
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
}
