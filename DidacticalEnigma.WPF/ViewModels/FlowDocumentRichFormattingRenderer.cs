using System;
using System.Windows;
using System.Windows.Documents;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Utils;

namespace DidacticalEnigma.ViewModels
{
    public interface IFlowDocumentRichFormattingRenderer
    {
        FlowDocument Render(RichFormatting document);
    }

    public class FlowDocumentRichFormattingRenderer : IFlowDocumentRichFormattingRenderer
    {
        public FlowDocumentRichFormattingRenderer(IFontResolver fontResolver, IWebBrowser webBrowser)
        {
            this.webBrowser = webBrowser;
            this.fontResolver = fontResolver;
        }

        private IWebBrowser webBrowser;

        private IFontResolver fontResolver;

        public FlowDocument Render(RichFormatting document)
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
                    case LinkParagraph link:
                        {
                            var p = new System.Windows.Documents.Paragraph();
                            var hlink = new Hyperlink(new Run(link.DisplayText))
                            {
                                NavigateUri = link.Target
                            };
                            hlink.RequestNavigate += (sender, args) =>
                            {
                                webBrowser.NavigateTo(args.Uri);
                                args.Handled = true;
                            };
                            p.Inlines.Add(hlink);
                            flow.Blocks.Add(p);
                        }
                        break;
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
                                        r.FontSize = 9;
                                        break;
                                    case FontSize.Small:
                                        r.FontSize = 12;
                                        break;
                                    case FontSize.Medium:
                                        r.FontSize = 14;
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
