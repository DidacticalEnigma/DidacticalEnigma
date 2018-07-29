using DidacticalEnigma.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DidacticalEnigma
{
    [ValueConversion(typeof(CodePoint), typeof(string))]
    public class CodePointToLongStringConverter : IValueConverter
    {
        public static readonly CodePointToLongStringConverter Default = new CodePointToLongStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cp = (CodePoint)value;
            return cp.ToLongString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
