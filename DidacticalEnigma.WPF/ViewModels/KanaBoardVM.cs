using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Models.LanguageService;
using Utility.Utils;

namespace DidacticalEnigma.ViewModels
{
    public class KanaBoardVM
    {
        public class KanaVM
        {
            public CodePoint CodePoint { get; }

            public string Kana => CodePoint?.ToString() ?? "";

            public string Romaji { get; }

            public int X { get; }

            public int Y { get; }

            public bool IsRegular => CodePoint != null;

            private readonly KanaBoardVM boardVM;

            public KanaVM(KanaBoardVM boardVM, CodePoint codePoint, string romaji, int x, int y)
            {
                this.boardVM = boardVM;
                CodePoint = codePoint;
                Romaji = romaji;
                X = x;
                Y = y;
            }
        }

        public ObservableBatchCollection<KanaVM> Kana { get; } = new ObservableBatchCollection<KanaVM>();

        public int Width { get; private set; }

        public int Height { get; private set; }

        public KanaBoardVM(string path, Encoding encoding)
        {
            Init(File.ReadLines(path, encoding).Select(lineColumn =>
            {
                var components = lineColumn.Split(' ');
                if (components.Length > 1)
                    return new KanaCharacter(components[0], components[1]);
                else
                    return null;
            }));
        }

        public KanaBoardVM(IEnumerable<KanaCharacter> kana)
        {
            Init(kana);
        }

        private void Init(IEnumerable<KanaCharacter> kana)
        {
            int x = 0;
            int y = 0;
            foreach (var k in kana)
            {
                if(k != null)
                    Kana.Add(new KanaVM(this, CodePoint.FromString(k.Kana), k.Romaji, x, y));
                else
                    Kana.Add(new KanaVM(this, null, null, x, y));

                x++;
                if (x == 5)
                {
                    x = 0;
                    y++;
                }
            }

            Width = x == 0 ? y : y + 1;
            Height = 5;

            var contents = Kana.OrderBy(k => k.X * Width + -k.Y).ToList();
            Kana.Clear();
            Kana.AddRange(contents);
        }
    }

    public class KanaCharacter
    {
        public string Kana { get; }

        public string Romaji { get; }

        public KanaCharacter(string kana, string romaji)
        {
            Kana = kana;
            Romaji = romaji;
        }
    }
}
