using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace dp2SSL
{
    [ValueConversion(typeof(int), typeof(string))]
    public sealed class IntToAddConverter : IValueConverter
    {
        public string Direction { get; set; }

        public IntToAddConverter()
        {
            // set defaults
            Direction = "+";
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is int))
                return null;
            int v = (int)value;
            if (v == 0)
                return "";
            return Direction + v.ToString();
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
