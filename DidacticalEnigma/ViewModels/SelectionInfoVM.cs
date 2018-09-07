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
        public CodePointVM Character { get; }

        public WordVM Word { get; }

        public string Text { get; }

        public SelectionInfoVM Clone(CodePointVM cpVm = null, WordVM word = null, string text = null)
        {
            cpVm = cpVm ?? this.Character;
            word = word ?? this.Word;
            text = text ?? this.Text;
            return new SelectionInfoVM(cpVm, word, text);
        }

        public SelectionInfoVM(CodePointVM cpVm, WordVM word, string text)
        {
            Character = cpVm;
            Word = word;
            Text = text;
        }

        public Request GetRequest()
        {
            var selectionInfo = this;
            return new Request(
                selectionInfo.Character.StringForm,
                selectionInfo.Word.StringForm,
                selectionInfo.Word.StringForm,
                selectionInfo.Word.WordInfo.EstimatedPartOfSpeech,
                selectionInfo.Word.WordInfo.NotInflected);
        }
    }
}
