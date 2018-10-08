using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DidacticalEnigma.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    class BooleanToLoadingTextConverter : IValueConverter
    {
        public static readonly BooleanToLoadingTextConverter Default = new BooleanToLoadingTextConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (bool)value;
            return b ? "Searching..." : " ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
