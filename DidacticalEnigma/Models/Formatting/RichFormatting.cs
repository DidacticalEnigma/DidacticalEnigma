using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

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

        public virtual FlowDocument Render()
        {
            var flow = new FlowDocument();
            foreach(var paragraph in Paragraphs)
            {
                switch(paragraph)
                {
                    case TextParagraph text:
                        {
                            var p = new System.Windows.Documents.Paragraph();
                            foreach(var c in text.Content)
                            {
                                Inline i;
                                if(c.Emphasis)
                                {
                                    i = new Bold(new Run(c.Content));
                                }
                                else
                                {
                                    i = new Run(c.Content);
                                }
                                p.Inlines.Add(i);
                            }
                            flow.Blocks.Add(p);
                        }
                        break;
                }
            }
            return flow;
        }
    }
}
