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

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(value as string);
            image.CacheOption = BitmapCacheOption.OnLoad;   // (注意这一句必须放在 .UriSource = ... 之后) 防止 WPF 一直锁定这个文件(即便 Image 都消失了还在锁定)
            image.EndInit();

            return image;

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
    }

}
