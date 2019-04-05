using System;
using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Core.Models.Project
{
    public class TranslationChangedEventArgs : EventArgs
    {
        public IEnumerable<ITranslation> Changed { get; }

        public TranslationChangedEventArgs(IEnumerable<ITranslation> changed)
        {
            Changed = changed.ToList().AsReadOnly();
        }
    }
}