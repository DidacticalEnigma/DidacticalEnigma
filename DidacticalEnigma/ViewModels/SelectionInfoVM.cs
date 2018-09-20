using DidacticalEnigma.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.DataSources;

namespace DidacticalEnigma.ViewModels
{
    public class SelectionInfoVM
    {
        public Func<string> AllText { get; }

        public CodePointVM Character { get; }

        public WordVM Word { get; }

        public string Text { get; }

        public SelectionInfoVM Clone(CodePointVM cpVm = null, WordVM word = null, string text = null, Func<string> allText = null)
        {
            cpVm = cpVm ?? this.Character;
            word = word ?? this.Word;
            text = text ?? this.Text;
            allText = allText ?? this.AllText;
            return new SelectionInfoVM(cpVm, word, text, allText);
        }

        public SelectionInfoVM(CodePointVM cpVm, WordVM word, string text, Func<string> allText)
        {
            Character = cpVm;
            Word = word;
            Text = text;
            AllText = allText;
        }

        public Request GetRequest()
        {
            var selectionInfo = this;
            return new Request(
                selectionInfo.Character.StringForm,
                selectionInfo.Word.WordInfo,
                selectionInfo.Word.StringForm,
                selectionInfo.AllText,
                selectionInfo.Word.SubsequentWords?.Invoke().Select(w => w.StringForm));
        }
    }
}
