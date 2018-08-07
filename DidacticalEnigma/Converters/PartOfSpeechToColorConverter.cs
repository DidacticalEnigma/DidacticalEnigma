using DidacticalEnigma.Utils;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DidacticalEnigma.Models;

namespace DidacticalEnigma
{
    [ValueConversion(typeof(PartOfSpeech), typeof(SolidColorBrush))]
    public class PartOfSpeechToColorConverter : IValueConverter
    {
        public static readonly PartOfSpeechToColorConverter Default = new PartOfSpeechToColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
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
