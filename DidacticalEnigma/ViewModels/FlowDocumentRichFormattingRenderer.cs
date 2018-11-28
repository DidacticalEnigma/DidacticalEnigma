using System;
using System.Windows;
using System.Windows.Documents;
using DidacticalEnigma.Core.Models.Formatting;

namespace DidacticalEnigma.ViewModels
{
    static class FlowDocumentRichFormattingRenderer
    {
        public static FlowDocument Render(IFontResolver fontResolver, RichFormatting document)
        {
            var flow = new FlowDocument
            {
                PagePadding = new Thickness(4),
                TextAlignment = TextAlignment.Left
            };
            foreach (var paragraph in document.Paragraphs)
            {
                switch (paragraph)
                {
                    case TextParagraph text:
                        {
                            var p = new System.Windows.Documents.Paragraph();
                            foreach (var c in text.Content)
                            {
                                Inline i;
                                var r = new Run(c.Content);
                                switch (c.FontSize)
                                {
                                    case FontSize.ExtraSmall:
                                        break;
                                    case FontSize.Small:
                                        r.FontSize = 10;
                                        break;
                                    case FontSize.Normal:
                                        r.FontSize = 16;
                                        break;
                                    case FontSize.Large:
                                        r.FontSize = 24;
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
