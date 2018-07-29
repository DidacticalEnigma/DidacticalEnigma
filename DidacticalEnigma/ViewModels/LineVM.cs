using System.Collections.Generic;

namespace DidacticalEnigma.Models
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
