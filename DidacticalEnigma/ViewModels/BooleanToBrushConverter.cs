using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DidacticalEnigma
{
    // , ConverterParameter={x:Static Brushes.Yellow}
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BooleanToBrushConverter : IValueConverter
    {
        public static readonly BooleanToVisibilityConverter Default = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (bool)value;
            //return b ? Brushes.Transparent : (Brush)parameter;
            return b ? Brushes.Yellow : Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
