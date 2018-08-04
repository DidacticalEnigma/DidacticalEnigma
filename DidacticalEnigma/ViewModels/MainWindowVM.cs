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
using System.Security.Cryptography;
using System.Diagnostics;

namespace DidacticalEnigma
{

    public class MainWindowVM : INotifyPropertyChanged, IDisposable
    {
        private readonly ILanguageService lang;

        private ClipboardHook hook;

        public string AboutText => File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"dic\about.txt"), Encoding.UTF8);

        public MainWindowVM()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            {
                
            }
            var kanjidict = JDict.KanjiDict.Create(Path.Combine(baseDir, @"dic\kanjidic2.xml"));
            var jdict = JDict.JMDict.Create(Path.Combine(baseDir, @"dic\JMdict_e"));
            var kradfile = new Kradfile(Path.Combine(baseDir, @"dic\kradfile1_plus_2_utf8"), Encoding.UTF8);
            var radkfile = new Radkfile(Path.Combine(baseDir, @"dic\radkfile1_plus_2_utf8"), Encoding.UTF8);
            lang = new LanguageService(
                new MeCabParam
                {
                    DicDir = Path.Combine(baseDir, @"dic\ipadic"),
                },
                SimilarKana.FromFile(Path.Combine(baseDir, @"dic\confused.txt")),
                jdict,
                kradfile,
                radkfile);
            Update = new RelayCommand(() =>
            {
                SetAnnotations(Input);
            }, () =>
            {
                return string.IsNullOrEmpty(RawOutput);
            });
            Reset = new RelayCommand(() =>
            {
                RawOutput = "";
                SetAnnotations("");
            }, () =>
            {
                return !string.IsNullOrEmpty(RawOutput);
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
            Lines.AddRange(
                lang.BreakIntoSentences(unannotatedOutput)
                    .Select(sentence => new LineVM(sentence.Select(word => new WordVM(word, lang)))));
            RawOutput = string.Join(
                "\n",
                Lines.Select(
                    line => string.Join(
                        " ",
                        line.Words.Select(word => word.StringForm))));
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
                Reset.OnExecuteChanged();
                SetAnnotations(value);
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
        public RelayCommand Reset { get; }

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
