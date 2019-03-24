using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using Utility.Utils;
using Radical = DidacticalEnigma.Core.Models.LanguageService.Radical;

namespace DidacticalEnigma.ViewModels
{
    public class KanjiRadicalLookupControlVM : INotifyPropertyChanged, IDisposable
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

            public bool Highlighted => CodePoint.ToString() == lookupVm.SearchText?.Trim();

            private readonly KanjiRadicalLookupControlVM lookupVm;

            public RadicalVM(JDict.Radical radical, bool enabled, KanjiRadicalLookupControlVM lookupVm)
            {
                CodePoint = CodePoint.FromInt(radical.CodePoint);
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

        private IEnumerable<CodePoint> currentlySelected = Enumerable.Empty<CodePoint>();

        private Task task = Task.CompletedTask;
        private CancellationTokenSource addingTaskCancellationToken = null;

        public async void SetElements(IReadOnlyCollection<CodePoint> elements, Dispatcher dispatcher)
        {
            addingTaskCancellationToken?.Cancel();
            await task;

            addingTaskCancellationToken = new CancellationTokenSource();
            var token = addingTaskCancellationToken.Token;
            kanji.Clear();
            const int n = 20;
            kanji.AddRange(elements.Take(n));
            if (elements.Count <= n)
            {
                return;
            }

            var tcs = new TaskCompletionSource<bool>();
            task = tcs.Task;
            Task.Run(async () =>
            {
                try
                {
                    foreach (var chunk in elements.Skip(n).ChunkBy(400))
                    {
                        if (token.IsCancellationRequested)
                            break;
                        dispatcher.Invoke(() => { kanji.AddRange(chunk); },
                            DispatcherPriority.ApplicationIdle,
                            token);
                    }
                }
                finally
                {
                    tcs.SetResult(true);
                }
            });
        }

        public void SelectRadicals(IEnumerable<CodePoint> codePoints, Dispatcher dispatcher)
        {
            var codePointsList = codePoints.ToList();
            if (!codePointsList.Any())
            {
                foreach (var radical in Radicals)
                {
                    radical.Enabled = true;
                }

                SetElements(Array.Empty<CodePoint>(), dispatcher);
                return;
            }

            var lookup = this.lookup.SelectRadical(codePointsList);
            SetElements(lookup.Kanji, dispatcher);
            for (var i = 0; i < radicals.Count; i++)
            {
                if(codePointsList.Contains(radicals[i].CodePoint))
                    continue;

                radicals[i].Enabled = lookup.PossibleRadicals[i].Value;
            }

            currentlySelected = codePointsList;
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

        private readonly ObservableBatchCollection<CodePoint> kanji = new ObservableBatchCollection<CodePoint>();
        public IEnumerable<CodePoint> SortedKanji => kanji;

        private readonly ObservableBatchCollection<RadicalVM> radicals = new ObservableBatchCollection<RadicalVM>();
        public IEnumerable<RadicalVM> Radicals => radicals;

        public KanjiRadicalLookupControlVM(
            KanjiRadicalLookup lookup,
            IKanjiProperties kanjiProperties)
        {
            this.lookup = lookup;
            radicals.AddRange(kanjiProperties.Radicals.Select(r => new RadicalVM(r, enabled: true, this)));
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
            Height = Math.Max(Width, Height);
            Width = Math.Max(Width, Height);
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
        
        public IEnumerable<IKanjiOrdering> SortingCriteria => lookup.SortingCriteria;

        private readonly KanjiRadicalLookup lookup;

        public int CurrentKanjiOrderingIndex
        {
            get => lookup.SortingCriteria.SelectedIndex;
            set
            {
                if (lookup.SortingCriteria.SelectedIndex == value)
                    return;

                lookup.SortingCriteria.SelectedIndex = value;
                //SelectRadicals(currentlySelected);
                OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            addingTaskCancellationToken.Cancel();
        }
    }
}
