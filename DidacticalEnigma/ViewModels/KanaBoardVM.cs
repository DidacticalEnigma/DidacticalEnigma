using DidacticalEnigma.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DidacticalEnigma.Utils;
using System.Windows.Input;
using System.Windows;

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

            private KanaBoardVM boardVM;

            public KanaVM(KanaBoardVM boardVM, CodePoint codePoint, string romaji, int x, int y)
            {
                this.boardVM = boardVM;
                CodePoint = codePoint;
                Romaji = romaji;
                X = x;
                Y = y;
            }
        }

        public ObservableBatchCollection<KanaVM> Kana { get; }

        public int Width { get; }

        public int Height { get; }

        public KanaBoardVM(string path, Encoding encoding, ILanguageService service)
        {
            Kana = new ObservableBatchCollection<KanaVM>();
            int x = 0;
            int y = 0;
            foreach (var lineColumn in File.ReadLines(path, encoding))
            {
                var components = lineColumn.Split(' ');
                if (components.Length > 1)
                    Kana.Add(new KanaVM(this, service.LookupCharacter(components[0]), components[1], x, y));
                else
                    Kana.Add(new KanaVM(this, null, null, x, y));

                x++;
                if(x == 5)
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
}
