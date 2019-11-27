using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace dp2SSL
{
    [ValueConversion(typeof(Operator), typeof(string))]
    public sealed class OperatorConverter : IValueConverter
    {
        public string Style { get; set; }   // name/barcode/auto

        public OperatorConverter()
        {
            // set defaults
            Style = "auto";
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is Operator))
                return null;
            Operator person = (Operator)value;
            if (person == null)
                return "";
            if (Style == "barcode")
                return GetAccountName(person.PatronBarcode);
            else if (Style == "name")
                return person.PatronName;
            else
            {
                if (string.IsNullOrEmpty(person.PatronName) == false)
                    return person.PatronName;
                return GetAccountName(person.PatronBarcode);
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        static string GetAccountName(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            if (text[0] == '~')
                return text.Substring(1);
            return text;
        }
    }

}
