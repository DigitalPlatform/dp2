using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace dp2SSL
{
    // https://social.msdn.microsoft.com/Forums/vstudio/en-US/8b0c8f35-48c0-4d52-9c02-6e04aa75a27f/dotted-border?forum=wpf
    public class ThicknessMaxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Thickness thickness = (Thickness)value;
            double horizontalMax = Math.Max(thickness.Left, thickness.Right);
            double verticalMax = Math.Max(thickness.Top, thickness.Bottom);
            return Math.Max(horizontalMax, verticalMax);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
