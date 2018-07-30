using NMeCab;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JDict;
using System.Text;
using System.Collections.ObjectModel;
using DidacticalEnigma.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System;
using System.IO;

namespace DidacticalEnigma
{

    public class MainWindowVM : INotifyPropertyChanged, IDisposable
    {
        private readonly ILanguageService lang;

        private ClipboardHook hook;

        public MainWindowVM()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var kanjidict = JDict.KanjiDict.Create(Path.Combine(baseDir, @"dic\kanjidic2.xml"));
            var jdict = JDict.JMDict.Create(Path.Combine(baseDir, @"dic\JMdict_e"));
            lang = new LanguageService(
                new MeCabParam
                {
                    DicDir = Path.Combine(baseDir, @"dic\ipadic"),
                },
                SimilarKana.FromFile(Path.Combine(baseDir, @"dic\confused.txt")),
                jdict);
            Update = new RelayCommand(() =>
            {
                RawOutput = string.Join("\n", lang.BreakIntoSentences(Input)
                    .Select(sentence => string.Join(" ", sentence.Select(word => word.RawWord))));
            }, () =>
            {
                return string.IsNullOrEmpty(RawOutput);
            });
            PlaceInClipboard = new RelayCommand((p) =>
            {
                var codePoint = (CodePoint)p;
                Clipboard.SetText(codePoint.ToString());
            });
            hook = new ClipboardHook();
            hook.ClipboardChanged += SetContent;
        }

        private void SetContent(object sender, string e)
        {
            Input = e;
        }

        private void SetAnnotations(string unannotatedOutput)
        {
            Lines.Clear();
            Lines.AddRange(unannotatedOutput
                .Split('\n')
                .Select(line =>
                    new LineVM(line
                        .Split(' ')
                        .Select(word => new WordVM(word, lang)))));
        }

        private string input = "";

        public ObservableBatchCollection<LineVM> Lines { get; } = new ObservableBatchCollection<LineVM>();

        private string rawOutput = "";
        public string RawOutput
        {
            get => rawOutput;
            set
            {
                if (rawOutput == value)
                    return;
                rawOutput = value;
                OnPropertyChanged();
                Update.OnExecuteChanged();
                SetAnnotations(RawOutput);
            }
        }
        public string Input
        {
            get => input;

            set
            {
                input = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand PlaceInClipboard { get; }

        public RelayCommand Update { get; }

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
            hook.Dispose();
            lang.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
