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
            Update = new RelayCommand(() =>
            {
                T.Clear();
                foreach (var word in SplitWords(tagger.Parse(s)))
                {
                    var wordVm = new WordVM(word, dictionary.Lookup(word));
                    foreach (var codePoint in word.AsCodePoints())
                    {
                        var cpVm = new CodePointVM(CodePoint.FromInt(codePoint), wordVm, Enumerable.Empty<CodePoint>());
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
