using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DidacticalEnigma.Converters
{
    // , ConverterParameter={x:Static Brushes.Yellow}
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BooleanToBrushConverter : IValueConverter
    {
        public static readonly BooleanToBrushConverter Default = new BooleanToBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (bool)value;
            var brush = parameter is SolidColorBrush br ? br : Brushes.Yellow;
            //return b ? Brushes.Transparent : (Brush)parameter;
            return b ? brush : Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
