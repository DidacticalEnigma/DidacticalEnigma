using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticalEnigma.Models
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
