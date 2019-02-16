using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace dp2SSL
{
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "red")
                return new SolidColorBrush(Colors.DarkRed);
            if ((string)value == "green")
                return new SolidColorBrush(Colors.DarkGreen);
            if ((string)value == "yellow")
                return new SolidColorBrush(Colors.DarkOrange);

            return new SolidColorBrush(Colors.DarkRed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
