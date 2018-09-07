using System;
using System.Globalization;
using System.Windows.Data;
using DidacticalEnigma.Core.Models.DataSources;
using DidacticalEnigma.Models;
using DidacticalEnigma.ViewModels;

namespace DidacticalEnigma.Converters
{
    [ValueConversion(typeof(SelectionInfoVM), typeof(Request))]
    class SelectionInfoToRequestConverter : IValueConverter
    {
        public static readonly SelectionInfoToRequestConverter Default = new SelectionInfoToRequestConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var selectionInfo = (SelectionInfoVM)value;
            return selectionInfo.GetRequest();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
