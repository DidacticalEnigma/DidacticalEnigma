using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DidacticalEnigma.Core.Models.Formatting;

namespace DidacticalEnigma.RestApi
{
    public class RichFormattingRenderer
    {
        public XmlDocument Render(RichFormatting document)
        {
            var xmlDocument = new XmlDocument();
            var root = xmlDocument.CreateElement("root");
            foreach (var paragraph in document.Paragraphs)
            {
                switch (paragraph)
                {
                    case LinkParagraph link:
                        {
                            var linkElement = xmlDocument.CreateElement("link");
                            linkElement.SetAttribute("target", link.Target.ToString());
                            linkElement.SetAttribute("text", link.DisplayText);

                            root.AppendChild(linkElement);
                        }
                        break;
                    case TextParagraph text:
                        {
                            var paragraphElement = xmlDocument.CreateElement("par");
                            foreach (var c in text.Content)
                            {
                                var spanElement = xmlDocument.CreateElement("span");
                                spanElement.InnerText = c.Content;
                                spanElement.SetAttribute("fontSize", c.FontSize.ToString());
                                spanElement.SetAttribute("fontName", c.FontName);
                                if (c.Emphasis)
                                {
                                    spanElement.SetAttribute("emphasis", "true");
                                }

                                paragraphElement.AppendChild(spanElement);
                            }
                            root.AppendChild(paragraphElement);
                        }
                        break;
                }
            }

            xmlDocument.AppendChild(root);
            return xmlDocument;
        }
    }
}
