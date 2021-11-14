using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DidacticalEnigma.Core.Models.LanguageService;
using Optional.Collections;
using Utility.Utils;
using Brushes = System.Windows.Media.Brushes;
using SystemColors = System.Windows.SystemColors;

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
            var resources = Application.Current.Resources;
            switch (b)
            {
                case PartOfSpeech.Noun:
                    return resources["NounTextHighlight"] ?? Brushes.DarkBlue;
                case PartOfSpeech.Verb:
                    return resources["VerbTextHighlight"] ?? Brushes.DarkRed;
                case PartOfSpeech.Particle:
                    return resources["ParticleTextHighlight"] ?? Brushes.RoyalBlue;
                default:
                    return resources["NoTextHighlight"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
