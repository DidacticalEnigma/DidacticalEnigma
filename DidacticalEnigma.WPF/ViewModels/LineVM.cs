using System.Collections.Generic;
using Utility.Utils;

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
