using System;
using System.Globalization;
using System.Windows.Data;

namespace DidacticalEnigma.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    class BooleanToLoadingTextConverter : IValueConverter
    {
        public static readonly BooleanToLoadingTextConverter Default = new BooleanToLoadingTextConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var b = (bool)value;
            return b ? "Searching..." : " ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
