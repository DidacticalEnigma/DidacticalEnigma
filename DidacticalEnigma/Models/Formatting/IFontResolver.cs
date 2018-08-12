

using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace DidacticalEnigma.Models.Formatting
{
    public interface IFontResolver
    {
        FontFamily Resolve(string name);
    }

    class DefaultFontResolver : IFontResolver
    {
        private readonly string baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "dic\\KanjiStrokeOrders");

        private readonly Dictionary<string, FontFamily> cache = new Dictionary<string, FontFamily>();

        public FontFamily Resolve(string name)
        {
            if (name == null)
                return null;
            cache.TryGetValue(name, out var fontFamily);
            return fontFamily;
        }

        public DefaultFontResolver()
        {
            var kanjiFontFileName = "KanjiStrokeOrders_v4.002.ttf";
            var kanjiFontFamily = "KanjiStrokeOrders";
            cache["kanji"] = new FontFamily(new Uri(Path.Combine(baseDirectory, kanjiFontFileName)), kanjiFontFamily);
        }
    }
}
