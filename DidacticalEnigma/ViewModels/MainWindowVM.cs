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
        private MeCabTagger tagger;
        private EDict dictionary;
        private SimilarKana similar;
        private ClipboardHook hook;

        public MainWindowVM()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            tagger = MeCabTagger.Create(new MeCabParam
            {
                DicDir = Path.Combine(baseDir, @"dic\ipadic"),
                LatticeLevel = MeCabLatticeLevel.Zero,
                OutputFormatType = "wakati",
                AllMorphs = false,
                Partial = false
            });
            dictionary = new EDict(Path.Combine(baseDir, @"dic\edict2_utf8"), Encoding.UTF8);
            similar = SimilarKana.FromFile(Path.Combine(baseDir, @"dic\confused.txt"));
            Update = new RelayCommand(() =>
            {
                var parsed = tagger.Parse(input);
                RawOutput = string.Join("", SplitWords(parsed)
                        .Select(word => word.Contains("\r") || word.Contains("\n") ? "\n" : word));
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
            AnnotatedOutput.Clear();
            AnnotatedOutput.AddRange(SplitWords(unannotatedOutput)
                .Select(word => new WordVM(word, dictionary.Lookup(word)))
                .SelectMany(wordVm => wordVm.StringForm.Select(rawCp =>
                {
                    var cp = CodePoint.FromInt(rawCp);
                    return new CodePointVM(cp, wordVm, similar.FindSimilar(cp));
                })));
        }

        private string input = "";

        public ObservableBatchCollection<CodePointVM> AnnotatedOutput { get; } = new ObservableBatchCollection<CodePointVM>();

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
            tagger.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
