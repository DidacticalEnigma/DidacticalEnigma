using DidacticalEnigma.Models;
using DidacticalEnigma.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DidacticalEnigma.ViewModels
{
    public class KanjiRadicalLookupControlVM : INotifyPropertyChanged
    {
        public class RadicalVM : INotifyPropertyChanged
        {
            public CodePoint CodePoint { get; }

            // dumb workaround for the name showing with the same color as disabled
            public string Name => CodePoint.ToString() == "｜" ? "|" : CodePoint.ToString();

            public Visibility Visible
            {
                get
                {
                    if (enabled)
                        return Visibility.Visible;
                    if (lookupVm.HideNonMatchingRadicals)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            private bool selected;
            public bool Selected
            {
                get => selected;
                set
                {
                    if (selected == value)
                        return;

                    selected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Visible));
                }
            }

            private bool enabled;
            public bool Enabled
            {
                get => enabled;
                set
                {
                    if (enabled == value)
                        return;

                    enabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Visible));
                }
            }

            public bool Highlighted => CodePoint.ToString() == lookupVm.SearchText?.Trim();

            private readonly KanjiRadicalLookupControlVM lookupVm;

            public RadicalVM(CodePoint codePoint, bool enabled, KanjiRadicalLookupControlVM lookupVm)
            {
                CodePoint = codePoint;
                Enabled = enabled;
                this.lookupVm = lookupVm;
                this.lookupVm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(HideNonMatchingRadicals))
                    {
                        OnPropertyChanged(nameof(Visible));
                    }

                    if (args.PropertyName == nameof(SearchText))
                    {
                        OnPropertyChanged(nameof(Highlighted));
                    }
                };
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly ILanguageService service;

        public void SelectRadicals(IEnumerable<CodePoint> codePoints)
        {
            if(!codePoints.Any())
            {
                foreach (var radical in Radicals)
                {
                    radical.Enabled = true;
                }
                Kanji.Clear();
                return;
            }

            var lookup = service.LookupByRadicals(codePoints);
            Kanji.Clear();
            Kanji.AddRange(lookup);
            var lookupHash = new HashSet<CodePoint>(lookup);
            foreach (var radical in Radicals)
            {
                var kanjiForRadical = service.LookupByRadicals(Enumerable.Repeat(radical.CodePoint, 1));
                radical.Enabled = lookupHash.IsIntersectionNonEmpty(kanjiForRadical);
            }
        }

        private bool hideNonMatchingRadicals = false;

        public bool HideNonMatchingRadicals
        {
            get => hideNonMatchingRadicals;
            set
            {
                if (hideNonMatchingRadicals == value)
                    return;
                hideNonMatchingRadicals = value;
                OnPropertyChanged();
            }
        }

        private string searchText;

        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText == value)
                    return;
                searchText = value;
                OnPropertyChanged();
            }
        }

        public ICommand KanjiClick { get; }

        public double Width { get; }

        public double Height { get; }

        public ObservableBatchCollection<CodePoint> Kanji { get; } = new ObservableBatchCollection<CodePoint>();

        public ObservableBatchCollection<RadicalVM> Radicals { get; } = new ObservableBatchCollection<RadicalVM>();

        public KanjiRadicalLookupControlVM(ILanguageService service)
        {
            this.service = service;
            Radicals.AddRange(service.AllRadicals().Select(r => new RadicalVM(r, true, this)));
            var tb = new TextBlock();
            tb.FontSize = 24;
            foreach (var k in Radicals)
            {
                tb.Measure(new System.Windows.Size(100, 100));
                tb.Text = k.CodePoint.ToString();
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
