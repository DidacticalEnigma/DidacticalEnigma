using System.Collections.Generic;
using DidacticalEnigma.Core.Utils;

namespace DidacticalEnigma.ViewModels
{
    public class LineVM
    {
        public ObservableBatchCollection<WordVM> Words { get; }

        public LineVM(IEnumerable<WordVM> words)
        {
            Words = new ObservableBatchCollection<WordVM>(words);
        }
    }
}
