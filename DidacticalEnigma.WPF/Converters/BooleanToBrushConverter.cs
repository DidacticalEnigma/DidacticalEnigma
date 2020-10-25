using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DidacticalEnigma.Converters
{
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BooleanToBrushConverter : IValueConverter
    {
        public static readonly BooleanToBrushConverter Default = new BooleanToBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var b = (bool)value;
            var brush = parameter is SolidColorBrush br ? br : Brushes.Yellow;
            return b ? brush : Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
