using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace EpwingRemapper.Core
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var config = new Configuration();
                using (var e = (args as IEnumerable<string>).GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        switch (e.Current?.Trim())
                        {
                            case "-o":
                            case "--output":
                                ReadOutputDir(config, e);
                                break;
                            case "-i":
                            case "--bitmap-json-data":
                                ReadInputBitmapJson(config, e);
                                break;
                            case "-c2t":
                            case "--capture2text-path":
                                ReadCapture2TextCliExecutablePath(config, e);
                                break;
                            case "-h":
                            case "--help":
                                DisplayHelp();
                                return 0;
                        }
                    }
                }

                var _ = ProcessFonts<Rgb24>(config, (img, id) =>
                {
                    var targetDir = Directory
                        .CreateDirectory(Path.Combine(config.OutputDirectory, id.SubBookIndex.ToString())).FullName;
                    using (var targetPng =
                        File.OpenWrite(Path.Combine(targetDir,
                            $"{id.Kind}_{id.Code}_{img.Width}x{img.Height}.png")))
                    {
                        img.SaveAsPng(targetPng);
                    }

                    return null;
                });
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }

            void ReadOutputDir(Configuration cfg, IEnumerator<string> e)
            {
                if (!e.MoveNext())
                    throw new ArgumentException();

                cfg.OutputDirectory = e.Current;
            }

            void ReadCapture2TextCliExecutablePath(Configuration cfg, IEnumerator<string> e)
            {
                if (!e.MoveNext())
                    throw new ArgumentException();

                cfg.Capture2TextCliExecutablePath = e.Current;
            }

            void ReadInputBitmapJson(Configuration cfg, IEnumerator<string> e)
            {
                if (!e.MoveNext())
                    throw new ArgumentException();

                cfg.InputBitmapJson = e.Current;
            }

            void DisplayHelp()
            {
                Console.WriteLine("Usage: program -c2t path-to-your-capture2text-executable -i exported-bitmap-data -o output-directory");
            }
        }

        public static Dictionary<CodeIdentifier, List<KeyValuePair<Size, string>>> ProcessFonts<TPixel>(Configuration config, CharacterRecognizer<TPixel> recognizer)
            where TPixel : struct, IPixel<TPixel>
        {
            config.Validate();

            var result = new Dictionary<CodeIdentifier, List<KeyValuePair<Size, string>>>();
            using (var jsonFile = File.OpenText(config.InputBitmapJson))
            using (var jsonReader = new JsonTextReader(jsonFile))
            {
                var serializer = new JsonSerializer();
                var root = serializer.Deserialize<Root>(jsonReader);
                foreach (var (subbook, subbookindex) in root.Subbooks.Select((s, i) => (s, i)))
                {
                    foreach (var font in subbook.Fonts)
                    {
                        var narrow = font.Narrow.AllGlyphs
                            .Select(g => (CharacterKind.Narrow, g, (width: font.Narrow.Width, height: font.Narrow.Height)));
                        var wide = font.Wide.AllGlyphs
                            .Select(g => (CharacterKind.Wide, g, (width: font.Wide.Width, height: font.Wide.Height)));
                        foreach (var (kind, glyph, (width, height)) in narrow.Concat(wide))
                        {
                            var expanded = glyph.BitmapData.SelectMany(p => new[]
                            {
                                (p & 128) == 0 ? NamedColors<TPixel>.White : NamedColors<TPixel>.Black,
                                (p & 64) == 0 ? NamedColors<TPixel>.White : NamedColors<TPixel>.Black,
                                (p & 32) == 0 ? NamedColors<TPixel>.White : NamedColors<TPixel>.Black,
                                (p & 16) == 0 ? NamedColors<TPixel>.White : NamedColors<TPixel>.Black,
                                (p & 8) == 0 ? NamedColors<TPixel>.White : NamedColors<TPixel>.Black,
                                (p & 4) == 0 ? NamedColors<TPixel>.White : NamedColors<TPixel>.Black,
                                (p & 2) == 0 ? NamedColors<TPixel>.White : NamedColors<TPixel>.Black,
                                (p & 1) == 0 ? NamedColors<TPixel>.White : NamedColors<TPixel>.Black,
                            }).ToArray();
                            var img = Image.LoadPixelData(expanded, width, height);
                            var recognitionAttempt = recognizer(img, new  CodeIdentifier(kind, glyph.Code, subbookindex));
                            var value = GetOrAdd(result, new CodeIdentifier(kind, glyph.Code, subbookindex), () => new List<KeyValuePair<Size, string>>());
                            value.Add(new KeyValuePair<Size, string>(new Size(width, height), recognitionAttempt));
                        }
                    }
                }
            }
            return result;
        }

        private static TValue GetOrAdd<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key, Func<TValue> Valuefactory)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                value = Valuefactory();
                dict[key] = value;
                return value;
            }
        }
    }

    public enum CharacterKind
    {
        Narrow,
        Wide
    }

    public delegate string CharacterRecognizer<TPixel>(
        Image<TPixel> img,
        CodeIdentifier id)
        where TPixel : struct, IPixel<TPixel>;

    public class Configuration
    {
        // MANDATORY
        public string Capture2TextCliExecutablePath { get; set; }

        // MANDATORY
        public string OutputDirectory { get; set; }

        // MANDATORY
        public string InputBitmapJson { get; set; }

        internal void Validate()
        {
            if (Capture2TextCliExecutablePath == null)
                throw new ArgumentException("this argument is mandatory", nameof(Capture2TextCliExecutablePath));

            if (OutputDirectory == null)
                throw new ArgumentException("this argument is mandatory", nameof(OutputDirectory));

            if (InputBitmapJson == null)
                throw new ArgumentException("this argument is mandatory", nameof(InputBitmapJson));

        }

    }

    internal class Root
    {
        public string CharCode { get; set; }

        public string DiscCode { get; set; }

        public IEnumerable<SubBook> Subbooks { get; set; }
    }

    internal class SubBook
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("copyright")]
        public string Copyright { get; set; }

        [JsonProperty("fonts")]
        public IEnumerable<Font> Fonts { get; set; }
    }

    internal class Font
    {
        [JsonProperty("narrow")]
        public Glyphs Narrow { get; set; }

        [JsonProperty("wide")]
        public Glyphs Wide { get; set; }
    }

    internal class Glyphs
    {
        [JsonProperty("glyphs")]
        public List<Glyph> AllGlyphs { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    internal class Glyph
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("bitmap")]
        public byte[] BitmapData { get; set; }

        [JsonProperty("translatedCodePoint")]
        public int? CodePoint { get; set; }


    }
}
