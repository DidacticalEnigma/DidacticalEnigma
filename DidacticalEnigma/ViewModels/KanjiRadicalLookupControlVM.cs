using DidacticalEnigma.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DidacticalEnigma.ViewModels
{
    public class KanjiRadicalLookupControlVM
    {
        private readonly ILanguageService service;

        public void SelectRadicals(IEnumerable<CodePoint> codePoints)
        {
            var lookup = service.LookupByRadicals(codePoints);
            Kanji.Clear();
            Kanji.AddRange(lookup);
        }

        public ICommand KanjiClick { get; }

        public double Width { get; }

        public double Height { get; }

        public ObservableBatchCollection<CodePoint> Kanji { get; } = new ObservableBatchCollection<CodePoint>();

        public ObservableBatchCollection<CodePoint> Radicals { get; } = new ObservableBatchCollection<CodePoint>();

        public KanjiRadicalLookupControlVM(ILanguageService service)
        {
            this.service = service;
            Radicals.AddRange(service.AllRadicals());
            var tb = new TextBlock();
            tb.FontSize = 24;
            foreach (var k in Radicals)
            {
                tb.Measure(new System.Windows.Size(100, 100));
                tb.Text = k.ToString();
                var size = tb.DesiredSize;
                Height = Math.Max(Height, size.Height);
                Width = Math.Max(Width, size.Width);
            }
            Height += 25;
            Width += 25;
            KanjiClick = new RelayCommand((p) =>
            {
                var codePoint = (CodePoint)p;
                Clipboard.SetText(codePoint.ToString());
            });
        }
    }
}
