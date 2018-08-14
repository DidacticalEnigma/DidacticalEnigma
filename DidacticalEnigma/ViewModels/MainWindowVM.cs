using NMeCab;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JDict;
using System.Text;
using System.Collections.ObjectModel;
using DidacticalEnigma.Models;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Unicode;
using DidacticalEnigma.ViewModels;

namespace DidacticalEnigma
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
                new MeCabParam
                {
                    DicDir = Path.Combine(baseDir, @"dic\ipadic"),
                },
                EasilyConfusedKana.FromFile(Path.Combine(baseDir, @"dic\confused.txt")),
                kradfile,
                radkfile,
                kanjidict,
                kanaProperties);
            HiraganaBoard = new KanaBoardVM(Path.Combine(baseDir, @"dic\hiragana_romaji.txt"), Encoding.UTF8, lang);
            KatakanaBoard = new KanaBoardVM(Path.Combine(baseDir, @"dic\katakana_romaji.txt"), Encoding.UTF8, lang);
            UsageDataSourceVM = new UsageDataSourcePreviewVM(lang, Path.Combine(baseDir, "dic"));
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
        }

        private void SetContent(object sender, string e)
        {
            ClipboardTextBuffer.RawOutput = e;
        }

        public ObservableBatchCollection<TextBufferVM> TextBuffers { get; } = new ObservableBatchCollection<TextBufferVM>();

        private TextBufferVM currentTextBuffer;
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
