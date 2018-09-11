using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using DidacticalEnigma.Core.Models;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Core.Utils;
using DidacticalEnigma.Models;
using DidacticalEnigma.Utils;
using JDict;
using NMeCab;

namespace DidacticalEnigma.ViewModels
{
    public class MainWindowVM : INotifyPropertyChanged, IDisposable
    {
        private readonly ILanguageService lang;

        private readonly ClipboardHook hook;

        public string AboutText => File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"dic\about.txt"), Encoding.UTF8);

        public UsageDataSourcePreviewVM UsageDataSourceVM { get; }

        public KanjiRadicalLookupControlVM KanjiLookupVM { get; }

        public KanaBoardVM HiraganaBoard { get; }

        public KanaBoardVM KatakanaBoard { get; }

        public MainWindowVM()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            {
                
            }
            var kanjidict = JDict.KanjiDict.Create(Path.Combine(baseDir, @"dic\kanjidic2.xml"));
            var kradfile = new Kradfile(Path.Combine(baseDir, @"dic\kradfile1_plus_2_utf8"), Encoding.UTF8);
            var radkfile = new Radkfile(Path.Combine(baseDir, @"dic\radkfile1_plus_2_utf8"), Encoding.UTF8);
            var kanaProperties = new KanaProperties(
                Path.Combine(baseDir, @"dic\hiragana_romaji.txt"),
                Path.Combine(baseDir, @"dic\katakana_romaji.txt"),
                Path.Combine(baseDir, @"dic\kana_related.txt"),
                Encoding.UTF8);
            lang = new LanguageService(
                new MeCab(new MeCabParam
                {
                    DicDir = Path.Combine(baseDir, @"dic\ipadic"),
                }),
                EasilyConfusedKana.FromFile(Path.Combine(baseDir, @"dic\confused.txt")),
                kradfile,
                radkfile,
                kanjidict,
                kanaProperties);
            HiraganaBoard = new KanaBoardVM(Path.Combine(baseDir, @"dic\hiragana_romaji.txt"), Encoding.UTF8, lang);
            KatakanaBoard = new KanaBoardVM(Path.Combine(baseDir, @"dic\katakana_romaji.txt"), Encoding.UTF8, lang);
            this.jmdict = JDict.JMDict.Create(Path.Combine(baseDir, "dic", "JMdict_e"));
            var wordFrequency = new FrequencyList(Path.Combine(baseDir, @"dic\word_form_frequency_list.txt"), Encoding.UTF8);
            UsageDataSourceVM = new UsageDataSourcePreviewVM(lang, Path.Combine(baseDir, "dic"), jmdict, wordFrequency);
            TextBuffers.Add(new TextBufferVM("Scratchpad", lang));
            TextBuffers.Add(new TextBufferVM("Main", lang));
            ClipboardTextBuffer = new TextBufferVM("Clipboard", lang);
            KanjiLookupVM = new KanjiRadicalLookupControlVM(lang);
            hook = new ClipboardHook();
            hook.ClipboardChanged += SetContent;
            PlaceInClipboard = new RelayCommand((p) =>
            {
                var codePoint = (CodePoint)p;
                Clipboard.SetText(codePoint.ToString());
            });
            SendToCurrent = new RelayCommand(() =>
            {
                if (CurrentTextBuffer == null)
                    return;
                CurrentTextBuffer.RawOutput = ClipboardTextBuffer.RawOutput;
            });
            SendToScratchpad = new RelayCommand(() =>
            {
                TextBuffers[0].RawOutput = ClipboardTextBuffer.RawOutput;
            });
            SearchWeb = new RelayCommand(query =>
            {
                if(CurrentTextBuffer == null)
                    return;
                if (SearchEngineIndex == -1)
                    return;

                LaunchWebBrowserAt(SearchEngines[SearchEngineIndex].BuildSearch(CurrentTextBuffer.SelectionInfo.GetRequest().QueryText));
            });
            SwitchToTab = new RelayCommand(tab =>
            {
                switch((string)tab)
                {
                    case "project":
                        TabIndex = 0;
                        break;
                    case "usage1":
                        TabIndex = 1;
                        break;
                    case "hiragana":
                        TabIndex = 3;
                        break;
                    case "kanji":
                        TabIndex = 4;
                        break;
                    case "katakana":
                        TabIndex = 5;
                        break;;

                }
            });
        }

        private static void LaunchWebBrowserAt(string url)
        {
            using (Process.Start(url))
            {

            }
        }

        private void SetContent(object sender, string e)
        {
            ClipboardTextBuffer.RawOutput = e;
        }

        public ObservableBatchCollection<TextBufferVM> TextBuffers { get; } = new ObservableBatchCollection<TextBufferVM>();

        private TextBufferVM currentTextBuffer;

        private readonly JMDict jmdict;

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

        public RelayCommand SendToScratchpad { get; }

        public RelayCommand SendToCurrent { get; }

        public RelayCommand SearchWeb { get; }

        public RelayCommand SwitchToTab { get; }

        private int tabIndex = 1;

        public int TabIndex
        {
            get => tabIndex;
            set
            {
                if (value == tabIndex)
                    return;
                tabIndex = value;
                OnPropertyChanged();
            }
        }

        private IEnumerable<string> SplitWords(string input)
        {
            input = input.Trim();
            int start = 0;
            int end = 0;
            bool current = false;
            for (int i = 0; i < input.Length; ++i)
            {
                if (char.IsWhiteSpace(input[i]) == current)
                {
                    ++end;
                }
                else
                {
                    current = !current;
                    yield return input.Substring(start, end - start);
                    start = end;
                    ++end;
                }
            }
            if (start != end)
            {
                yield return input.Substring(start, end - start);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            UsageDataSourceVM.Dispose();
            lang.Dispose();
            jmdict.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
