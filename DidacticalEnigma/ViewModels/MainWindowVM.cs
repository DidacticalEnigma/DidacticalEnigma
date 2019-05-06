using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using DidacticalEnigma.Core.Models;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Utils;
using Utility.Utils;

namespace DidacticalEnigma.ViewModels
{
    public interface ITextInsertCommand : ICommand
    {

    }

    public class TextInsertCommand : ITextInsertCommand
    {
        private Action<string> action;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string s)
                action(s);
            else if (parameter is CodePoint c)
                action(c.ToString());
            else
                throw new ArgumentException(nameof(parameter));
        }

        public event EventHandler CanExecuteChanged;

        public TextInsertCommand(Action<string> action)
        {
            this.action = action;
        }
    }

    public class MainWindowVM : INotifyPropertyChanged, IDisposable
    {
        private readonly ClipboardHook hook;

        private readonly Func<string> aboutTextProvider;

        public string AboutText => aboutTextProvider();

        public IReadOnlyList<UsageDataSourcePreviewVM> UsageDataSourceVMs { get; }

        private int usageDataSourceIndex = 0;
        public int UsageDataSourceIndex
        {
            get => usageDataSourceIndex;
            set
            {
                if (usageDataSourceIndex == value)
                    return;

                usageDataSourceIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UsageDataSourceVM));
            }
        }

        public UsageDataSourcePreviewVM UsageDataSourceVM => UsageDataSourceVMs[UsageDataSourceIndex];

        public KanjiRadicalLookupControlVM KanjiLookupVM { get; }

        public KanaBoardVM HiraganaBoard { get; }

        public KanaBoardVM KatakanaBoard { get; }

        private readonly int minUsageIndex = 1;

        private readonly int maxUsageIndex = 3;

        public MainWindowVM(
            ISentenceParser parser,
            KanaBoardVM hiraganaBoard,
            KanaBoardVM katakanaBoard,
            IEnumerable<UsageDataSourcePreviewVM> usageDataSourceVms,
            KanjiRadicalLookupControlVM kanjiLookupVm,
            IRelated related,
            IKanjiProperties kanjiProperties,
            IKanaProperties kanaProperties,
            IWebBrowser webBrowser,
            Func<string> aboutTextProvider,
            ITextInsertCommand insertText)
        {
            HiraganaBoard = hiraganaBoard;
            KatakanaBoard = katakanaBoard;
            InsertTextAtCaret = insertText;
            this.aboutTextProvider = aboutTextProvider;
            UsageDataSourceVMs = new ObservableBatchCollection<UsageDataSourcePreviewVM>(usageDataSourceVms);
            TextBuffers.Add(new TextBufferVM("Scratchpad", parser, kanjiProperties, kanaProperties, related));
            TextBuffers.Add(new TextBufferVM("Main", parser, kanjiProperties, kanaProperties, related));
            ClipboardTextBuffer = new TextBufferVM("Clipboard", parser, kanjiProperties, kanaProperties, related);
            TextBuffers.Add(ClipboardTextBuffer);
            KanjiLookupVM = kanjiLookupVm;
            hook = new ClipboardHook();
            hook.ClipboardChanged += SetContent;
            PlaceInClipboard = new RelayCommand((p) =>
            {
                var codePoint = (CodePoint)p;
                Clipboard.SetText(codePoint.ToString());
            });
            SearchWeb = new RelayCommand(query =>
            {
                if (CurrentTextBuffer == null)
                    return;
                if (SearchEngineIndex == -1)
                    return;

                var queryText = CurrentTextBuffer.SelectionInfo?.GetRequest().QueryText;
                if (queryText == null)
                    return;

                webBrowser.NavigateTo(SearchEngines[SearchEngineIndex].BuildSearch(queryText));
            });
            SwitchToTab = new RelayCommand(tab =>
            {
                switch ((string)tab)
                {
                    case "project":
                        TabIndex = 0;
                        break;
                    case "usage1":
                        TabIndex = 1;
                        break;
                    case "usage2":
                        TabIndex = 2;
                        break;
                    case "usage3":
                        TabIndex = 3;
                        break;
                    case "hiragana":
                        TabIndex = 4;
                        break;
                    case "kanji":
                        TabIndex = 5;
                        break;
                    case "katakana":
                        TabIndex = 6;
                        break;
                }
            });
            DataSourceForceRefresh = new RelayCommand(() =>
            {
                UsageDataSourceVM.Search(UsageDataSourceVM.Request, CancellationToken.None);
            });
        }

        private void SetContent(object sender, string e)
        {
            ClipboardTextBuffer.RawOutput = e;
            ClipboardTextBuffer.IssueMeCabSplit.Execute(null);
            var word = ClipboardTextBuffer.Lines.FirstOrDefault()?.Words?.FirstOrDefault();
            var codePoint = word?.CodePoints.FirstOrDefault();
            if (codePoint != null)
            {
                ClipboardTextBuffer.SelectionInfo = new SelectionInfoVM(codePoint, word,
                    () => string.Join("\n",
                        ClipboardTextBuffer.Lines.Select(l => string.Join(" ", l.Words.Select(w => w.StringForm)))));
            }
        }

        public ObservableBatchCollection<TextBufferVM> TextBuffers { get; } = new ObservableBatchCollection<TextBufferVM>();

        private TextBufferVM currentTextBuffer;

        private int searchEngineIndex = -1;

        public int SearchEngineIndex
        {
            get => searchEngineIndex;
            set
            {
                if (searchEngineIndex == value)
                    return;
                searchEngineIndex = value;
                OnPropertyChanged();
            }
        }

        public ObservableBatchCollection<SearchEngine> SearchEngines { get; } = new ObservableBatchCollection<SearchEngine>
        {
            new SearchEngine("https://duckduckgo.com/?q=", "site:japanese.stackexchange.com", literal: true, comment: "Search Japanese Stack Exchange"),
            new SearchEngine("https://duckduckgo.com/?q=", "site:maggiesensei.com", literal: true, comment: "Search Maggie Sensei website"),
            new SearchEngine("https://duckduckgo.com/?q=", "site:www.japanesewithanime.com", literal: true, comment: "Search Japanese with Anime blog"),
            new SearchEngine("https://duckduckgo.com/?q=", "とは", literal: true, comment: "What is...?"),
            new SearchEngine("https://duckduckgo.com/?q=", "意味", literal: true, comment: "Meaning...?"),
            new SearchEngine("https://duckduckgo.com/?q=", "英語", literal: true, comment: "English...?"),
            new SearchEngine(
                "http://www.nihongoresources.com/dictionaries/universal.html?type=sfx&query=",
                null,
                literal: false,
                comment: "nihongoresources.com SFX search"),
            new SearchEngine(
                "http://thejadednetwork.com/sfx/search/?submitSearch=Search+SFX&x=&keyword=",
                null,
                literal: false,
                comment: "The JADED Network SFX search"),

        };

        public TextBufferVM CurrentTextBuffer
        {
            get => currentTextBuffer;

            set
            {
                if (currentTextBuffer == value)
                    return;
                currentTextBuffer = value;
                OnPropertyChanged();
            }
        }

        public TextBufferVM ClipboardTextBuffer { get; }

        public RelayCommand PlaceInClipboard { get; }

        public RelayCommand SearchWeb { get; }

        public RelayCommand SwitchToTab { get; }

        public RelayCommand DataSourceForceRefresh { get; }

        public ICommand InsertTextAtCaret { get; }

        private int tabIndex = 1;
        public int TabIndex
        {
            get => tabIndex;
            set
            {
                if (value == tabIndex)
                    return;
                tabIndex = value;
                if (minUsageIndex <= value && value <= maxUsageIndex)
                    UsageDataSourceIndex = value - minUsageIndex;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            foreach (var usageDataSourcePreviewVm in UsageDataSourceVMs)
            {
                usageDataSourcePreviewVm.Dispose();
            }
            hook.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
