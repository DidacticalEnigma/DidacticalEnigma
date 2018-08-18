using DidacticalEnigma.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DidacticalEnigma.ViewModels
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
            return new Request(
                selectionInfo.Character.StringForm,
                selectionInfo.Word.StringForm,
                selectionInfo.Word.StringForm,
                selectionInfo.Word.WordInfo.EstimatedPartOfSpeech,
                selectionInfo.Word.WordInfo.NotInflected);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
