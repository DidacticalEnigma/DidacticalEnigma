using System;
using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Core.Models.Formatting
{
    public class RichFormatting
    {
        public ICollection<Paragraph> Paragraphs { get; }

        public RichFormatting()
            : this(Enumerable.Empty<Paragraph>())
        {

        }

        public RichFormatting(IEnumerable<Paragraph> paragraphs)
        {
            Paragraphs = new List<Paragraph>(paragraphs);
        }
    }
}
