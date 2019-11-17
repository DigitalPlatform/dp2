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
        public string Style { get; set; }

        public OperatorConverter()
        {
            // set defaults
            Style = "name";
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
                return person.PatronBarcode;
            return person.PatronName;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
