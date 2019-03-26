using System;
using System.Globalization;
using System.Windows.Data;
using DidacticalEnigma.ViewModels;

namespace DidacticalEnigma.Converters
{
    class SelectableDataSourceToStringConverter : IValueConverter
    {
        public static readonly SelectableDataSourceToStringConverter Default = new SelectableDataSourceToStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Selectable<DataSourceVM>))
                return value;
            var v = (Selectable<DataSourceVM>)value;
            return v.Entity.Descriptor.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
