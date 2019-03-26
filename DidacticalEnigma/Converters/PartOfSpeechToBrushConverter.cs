using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DidacticalEnigma.Core.Models.LanguageService;

namespace DidacticalEnigma.Converters
{
    [ValueConversion(typeof(PartOfSpeech), typeof(SolidColorBrush))]
    public class PartOfSpeechToBrushConverter : IValueConverter
    {
        public static readonly PartOfSpeechToBrushConverter Default = new PartOfSpeechToBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var b = (PartOfSpeech)value;
            switch(b)
            {
                case PartOfSpeech.Noun:
                    return Brushes.DarkBlue;
                case PartOfSpeech.Verb:
                    return Brushes.DarkRed;
                case PartOfSpeech.Particle:
                    return Brushes.RoyalBlue;
                default:
                    return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
