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

namespace DidacticalEnigma
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        private MeCabTagger tagger;
        private EDict dictionary;
        private SimilarKana similar;

        public MainWindowVM()
        {
            tagger = MeCabTagger.Create(new MeCabParam
            {
                DicDir = @"C:\Users\IEUser\Desktop\DidacticalEnigma\DidacticalEnigma\dic\ipadic",
                LatticeLevel = MeCabLatticeLevel.Zero,
                OutputFormatType = "wakati",
                AllMorphs = false,
                Partial = false
            });
            dictionary = new EDict(@"C:\Users\IEUser\Desktop\DidacticalEnigma\DidacticalEnigma\dic\edict2_utf8", Encoding.UTF8);
            similar = SimilarKana.FromFile(@"C:\Users\IEUser\Desktop\DidacticalEnigma\DidacticalEnigma\dic\confused.txt");
            Update = new RelayCommand(() =>
            {
                T.Clear();
                var parsed = tagger.Parse(s);
                foreach (var word in SplitWords(parsed))
                {
                    var wordTrimmed = word.Contains("\r") || word.Contains("\n") ? "\n" : word;
                    wordTrimmed = word.Trim(' ') == "" ? "          " : wordTrimmed;
                    var wordVm = new WordVM(wordTrimmed, dictionary.Lookup(wordTrimmed));
                    foreach (var codePoint in wordTrimmed.AsCodePoints())
                    {
                        var cp = CodePoint.FromInt(codePoint);
                        var cpVm = new CodePointVM(cp, wordVm, similar.FindSimilar(cp));
                        T.Add(cpVm);
                    }
                }
            });
        }

        private string s = "";

        public ObservableCollection<CodePointVM> T { get; } = new ObservableCollection<CodePointVM>();
        public string S
        {
            get => s;

            set
            {
                s = value;
                OnPropertyChanged();
            }
        }

        public ICommand Update { get; }

        private IEnumerable<string> SplitWords(string input)
        {
            input = input.Trim();
            int start = 0;
            int end = 0;
            bool current = false;
            for(int i = 0; i < input.Length; ++i)
            {
                if(char.IsWhiteSpace(input[i]) == current)
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
            if(start != end)
            {
                yield return input.Substring(start, end - start);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
