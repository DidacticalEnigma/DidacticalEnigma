using System;
using System.Globalization;
using System.Windows.Data;

namespace DidacticalEnigma.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    class BooleanNegationConverter : IValueConverter
    {
        public static readonly BooleanNegationConverter Default = new BooleanNegationConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (bool?) value;
            return !v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
