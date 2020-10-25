using System;
using System.Linq;
using DidacticalEnigma.Core.Models.DataSources;

namespace DidacticalEnigma.ViewModels
{
    public class SelectionInfoVM
    {
        public Func<string> AllText { get; }

        public CodePointVM Character { get; }

        public WordVM Word { get; }

        public SelectionInfoVM Clone(CodePointVM cpVm = null, WordVM word = null, Func<string> allText = null)
        {
            cpVm = cpVm ?? Character;
            word = word ?? Word;
            allText = allText ?? AllText;
            return new SelectionInfoVM(cpVm, word, allText);
        }

        public SelectionInfoVM(CodePointVM cpVm, WordVM word, Func<string> allText)
        {
            Character = cpVm;
            Word = word;
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
