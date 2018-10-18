using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Core.Utils;
using JDict;
using Radical = DidacticalEnigma.Core.Models.LanguageService.Radical;

namespace DidacticalEnigma.ViewModels
{
    public class KanjiRadicalLookupControlVM : INotifyPropertyChanged
    {
        public class RadicalVM : INotifyPropertyChanged
        {
            public CodePoint CodePoint { get; }

            public int StrokeCount { get; }

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

            public bool Highlighted => lookupVm.remapper.Comparer.Equals(CodePoint.ToString(), lookupVm.SearchText?.Trim() ?? "");

            private readonly KanjiRadicalLookupControlVM lookupVm;

            public RadicalVM(Radical radical, bool enabled, KanjiRadicalLookupControlVM lookupVm)
            {
                CodePoint = radical.CodePoint;
                StrokeCount = radical.StrokeCount;
                this.enabled = enabled;
                this.lookupVm = lookupVm;
                lookupVm.PropertyChanged += (sender, args) =>
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
        private readonly KanjiDict kanjiDict;
        private readonly RadicalRemapper remapper;

        public void SelectRadicals(IEnumerable<CodePoint> codePoints)
        {
            var codePointsList = codePoints.ToList();
            if(!codePointsList.Any())
            {
                foreach (var radical in Radicals)
                {
                    radical.Enabled = true;
                }
                kanji.Clear();
                return;
            }

            var lookup = service.LookupByRadicals(codePointsList).ToList();
            kanji.Clear();
            kanji.AddRange(lookup);
            var lookupHash = new HashSet<CodePoint>(lookup);
            foreach (var radical in Radicals)
            {
                var kanjiForRadical = service.LookupByRadicals(Enumerable.Repeat(radical.CodePoint, 1));
                radical.Enabled = lookupHash.IsIntersectionNonEmpty(kanjiForRadical);
            }

            OrderKanji();
        }

        private void OrderKanji()
        {
            sortedKanji.Clear();
            sortedKanji.AddRange(kanji.OrderBy(x => x, CurrentSortingCriterion.Comparer));
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

        private readonly ObservableBatchCollection<CodePoint> sortedKanji = new ObservableBatchCollection<CodePoint>();
        public IEnumerable<CodePoint> SortedKanji => sortedKanji;

        private readonly ObservableBatchCollection<CodePoint> kanji = new ObservableBatchCollection<CodePoint>();
        public IEnumerable<CodePoint> Kanji => kanji;

        private readonly ObservableBatchCollection<RadicalVM> radicals = new ObservableBatchCollection<RadicalVM>();
        public IEnumerable<RadicalVM> Radicals => radicals;

        public KanjiRadicalLookupControlVM(ILanguageService service, KanjiDict kanjiDict, RadicalRemapper remapper)
        {
            this.service = service;
            this.kanjiDict = kanjiDict;
            this.remapper = remapper;
            radicals.AddRange(service.AllRadicals().Select(r => new RadicalVM(r, enabled: true, this)));
            var tb = new TextBlock
            {
                FontSize = 24
            };
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
            sortingCriteria = new ObservableBatchCollection<SortingCriterion>
            {
                new SortingCriterion("Sort by stroke count", CompareBy(x => x.StrokeCount)),
                new SortingCriterion("Sort by frequency", CompareBy(x => x.FrequencyRating))
            };
            currentSortingCriterion = sortingCriteria[0];
        }

        private IComparer<CodePoint> CompareBy<T>(Func<KanjiEntry, T> f)
        {
            return Comparer<CodePoint>.Create((l, r) =>
            {
                var left = kanjiDict.Lookup(l.ToString()).Map(f);
                var right = kanjiDict.Lookup(r.ToString()).Map(f);
                return left.CompareTo(right);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class SortingCriterion
        {
            public string Description { get; }

            public IComparer<CodePoint> Comparer { get; }

            public SortingCriterion(string description, IComparer<CodePoint> comparer)
            {
                Description = description ?? throw new ArgumentNullException(nameof(description));
                Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            }

            public override string ToString()
            {
                return Description;
            }
        }

        private readonly ObservableBatchCollection<SortingCriterion> sortingCriteria;
        public IEnumerable<SortingCriterion> SortingCriteria => sortingCriteria;

        private SortingCriterion currentSortingCriterion;
        public SortingCriterion CurrentSortingCriterion
        {
            get => currentSortingCriterion;
            set
            {
                if (currentSortingCriterion == value)
                    return;

                currentSortingCriterion = value;
                OrderKanji();
                OnPropertyChanged();
            }
        }
    }
}
