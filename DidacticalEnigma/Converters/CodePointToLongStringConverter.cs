using System;
using System.Globalization;
using System.Windows.Data;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Models;

namespace DidacticalEnigma.Converters
{
    [ValueConversion(typeof(CodePoint), typeof(string))]
    public class CodePointToLongStringConverter : IValueConverter
    {
        public static readonly CodePointToLongStringConverter Default = new CodePointToLongStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var cp = (CodePoint)value;
            return cp.ToLongString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
