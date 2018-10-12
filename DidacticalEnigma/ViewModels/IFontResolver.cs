using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace DidacticalEnigma.ViewModels
{
    public interface IFontResolver
    {
        FontFamily Resolve(string name);
    }

    class DefaultFontResolver : IFontResolver
    {
        private readonly Dictionary<string, FontFamily> cache = new Dictionary<string, FontFamily>();

        public FontFamily Resolve(string name)
        {
            if (name == null)
                return null;
            cache.TryGetValue(name, out var fontFamily);
            return fontFamily;
        }

        public DefaultFontResolver(string baseDirectory)
        {
            var kanjiFontFileName = Directory.EnumerateFiles(baseDirectory, "*.ttf").First();
            var kanjiFontFamily = "./#KanjiStrokeOrders";
            cache["kanji"] = new FontFamily(new Uri(Path.Combine(baseDirectory, kanjiFontFileName)), kanjiFontFamily);
        }
    }
}
