using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using DidacticalEnigma.Models.Formatting;

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

        public virtual FlowDocument Render(IFontResolver fontResolver)
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
                                var r = new Run(c.Content);
                                switch (c.FontSize)
                                {
                                    case FontSize.ExtraSmall:
                                        break;
                                    case FontSize.Small:
                                        break;
                                    case FontSize.Normal:
                                        // do nothing
                                        break;
                                    case FontSize.Large:
                                        break;
                                    case FontSize.ExtraLarge:
                                        break;
                                    case FontSize.Humonguous:
                                        r.FontSize = 120;
                                        break;
                                    default:
                                        throw new InvalidOperationException("not a valid enum value");
                                }

                                if (fontResolver.Resolve(c.FontName) is System.Windows.Media.FontFamily f)
                                {
                                    r.FontFamily = f;
                                }

                                if (c.Emphasis)
                                {
                                    i = new Bold(r);
                                }
                                else
                                {
                                    i = r;
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
