using DigitalPlatform.Text;
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
    public class StateToBackConverter : IValueConverter
    {
        public Color OpenColor { get; set; }
        public Color CloseColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "open")
                return new SolidColorBrush(OpenColor);

            return new SolidColorBrush(CloseColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class EntityStateToBackConverter : IValueConverter
    {
        public Color OverflowColor { get; set; }
        public Color OverdueColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = StringUtil.SplitList((string)value);
            foreach (string s in list)
            {
                if (s == "overflow")
                    return new SolidColorBrush(OverflowColor);
                if (s == "overdue")
                    return new SolidColorBrush(OverdueColor);
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
