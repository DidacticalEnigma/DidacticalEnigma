using NMeCab;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DidacticalEnigma
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        MeCabTagger tagger;

        public MainWindowVM()
        {
            tagger = MeCabTagger.Create(new MeCabParam
            {
                DicDir = @"C:\Users\IEUser\Desktop\NMECAB\dic\ipadic"
            });
            tagger.LatticeLevel = MeCabLatticeLevel.Zero;
            tagger.OutPutFormatType = "wakati";
            tagger.AllMorphs = false;
            tagger.Partial = false;
        }

        private string s = "";

        public string T
        {
            get => tagger.Parse(S);
        }
        public string S
        {
            get => s;

            set
            {
                s = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(T));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
