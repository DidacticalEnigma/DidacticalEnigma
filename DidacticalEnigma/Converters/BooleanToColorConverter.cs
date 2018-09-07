using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DidacticalEnigma.Converters
{ 
    [ValueConversion(typeof(bool), typeof(Color))]
    public class BooleanToColorConverter : IValueConverter
    {
        public static readonly BooleanToColorConverter Default = new BooleanToColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var b = (bool)value;
            var brush = parameter is Color br ? br : Colors.Yellow;
            return b ? brush : Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
