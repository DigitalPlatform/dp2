using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace dp2SSL
{
    public class StateToBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "borrowed")
                return new SolidColorBrush(Colors.Transparent);

            if ((string)value == "onshelf")
                return new SolidColorBrush(Colors.DarkGreen);

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StateToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (StringUtil.IsInList("borrowed", (string)value))
                return FontAwesome.WPF.FontAwesomeIcon.AddressBook;

            // return FontAwesome.WPF.FontAwesomeIcon.HandPaperOutline;

            return FontAwesome.WPF.FontAwesomeIcon.Cube;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> results = new List<string>();
            var list = StringUtil.SplitList((string)value);
            foreach(string s in list)
            {
                if (s == "overflow")
                    results.Add("超额");
                if (s == "overdue")
                    results.Add("超期");
            }

            return StringUtil.MakePathList(results);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
