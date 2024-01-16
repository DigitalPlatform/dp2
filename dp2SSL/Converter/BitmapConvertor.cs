using DigitalPlatform.IO;
using DigitalPlatform.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace dp2SSL
{
    // 2023/12/15
    // 从字符串转换到 BitmapImage 对象
    public class StringToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string fileName = value as string;
            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(fileName);
                image.CacheOption = BitmapCacheOption.OnLoad;   // (注意这一句必须放在 .UriSource = ... 之后) 防止 WPF 一直锁定这个文件(即便 Image 都消失了还在锁定)
                image.EndInit();

                return image;
            }
            catch (Exception ex)
            {
                string error = "图像错误: " + ex.Message + " " + fileName;
                WpfClientInfo.WriteErrorLog(error);

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = BuildTextImage(error, 400);
                image.CacheOption = BitmapCacheOption.OnLoad;   // (注意这一句必须放在 .UriSource = ... 之后) 防止 WPF 一直锁定这个文件(即便 Image 都消失了还在锁定)
                image.EndInit();

                // 尝试删除这个图像文件，以便后面还有重试下载的机会
                // 集中通知处理
                _ = Task.Run(() =>
                {
                    App.TryDeleteBrokenImageFile(fileName);
                });
                return image;
            }

            /*
            // https://stackoverflow.com/questions/1684489/how-do-you-make-sure-wpf-releases-large-bitmapsource-from-memory
            BitmapImage image = new BitmapImage();
            using (var stream = new FileStream(value as string, FileMode.Open))
            {
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;   // 防止 WPF 一直锁定这个文件(即便 Image 都消失了还在锁定)
                image.EndInit();
            }
            return image;
            */
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }



        static Stream BuildTextImage(string strText,
    int nWidth = 400)
        {
            // 文字图片
            return DigitalPlatform.Drawing.ArtText.BuildArtText(
                strText,
                "Consolas", // "Microsoft YaHei",
                (float)16,
                System.Drawing.FontStyle.Bold,
            System.Drawing.Color.Red,
            System.Drawing.Color.Transparent,
            System.Drawing.Color.Gray,
            DigitalPlatform.Drawing.ArtEffect.None,
            System.Drawing.Imaging.ImageFormat.Png,
            nWidth);
        }

    }

}
