using System;
using System.Linq;
using DidacticalEnigma.Core.Models.Formatting;
using HtmlAgilityPack;
using Utility.Utils;

namespace DidacticalEnigma.Web
{
    public interface IRichFormattingHtmlRenderer
    {
        HtmlNode Render(HtmlDocument document, RichFormatting input);
    }

    public class RichFormattingHtmlRenderer : IRichFormattingHtmlRenderer
    {
        public HtmlNode Render(HtmlDocument document, RichFormatting input)
        {
            var div = document.CreateElement("div");
            foreach (var paragraph in input.Paragraphs)
            {
                switch (paragraph)
                {
                    case LinkParagraph link:
                        {
                            var p = document.CreateElement("p");
                            var a = document.CreateElement("a");
                            a.SetAttributeValue("href", link.Target.ToString());
                            a.AppendChild(document.CreateTextNode(HtmlDocument.HtmlEncode(link.DisplayText)));

                            p.AppendChild(a);
                            div.AppendChild(p);
                        }
                        break;
                    case TextParagraph text:
                        {
                            var p = document.CreateElement("p");
                            foreach (var c in text.Content)
                            {
                                HtmlNode node;
                                var span = document.CreateElement("span");
                                switch (c.FontSize)
                                {
                                    case FontSize.ExtraSmall:
                                        span.SetAttributeValue("style", "font-size: xx-small");
                                        break;
                                    case FontSize.Small:
                                        span.SetAttributeValue("style", "font-size: small");
                                        break;
                                    case FontSize.Medium:
                                        span.SetAttributeValue("style", "font-size: medium");
                                        break;
                                    case FontSize.Normal:
                                        break;
                                    case FontSize.Large:
                                        span.SetAttributeValue("style", "font-size: large");
                                        break;
                                    case FontSize.ExtraLarge:
                                        span.SetAttributeValue("style", "font-size: x-large");
                                        break;
                                    case FontSize.Humonguous:
                                        span.SetAttributeValue("style", "font-size: xx-large");
                                        break;
                                    default:
                                        throw new InvalidOperationException("not a valid enum value");
                                }

                                if (c.Emphasis)
                                {
                                    var b = document.CreateElement("b");
                                    b.AppendChild(span);
                                    node = b;
                                }
                                else
                                {
                                    node = span;
                                }

                                var textParagraphs = c.Content
                                    .Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None)
                                    .Intersperse(null)
                                    .Select(x => x != null ? document.CreateTextNode(HtmlDocument.HtmlEncode(x)) : document.CreateElement("br"));
                                foreach (var t in textParagraphs)
                                {
                                    span.AppendChild(t);
                                }
                                p.AppendChild(node);
                            }

                            div.AppendChild(p);
                        }
                        break;
                }
            }

            return div;
        }
    }
}