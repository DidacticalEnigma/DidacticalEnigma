using System;

namespace DidacticalEnigma.Core.Models.Formatting
{
    public class LinkParagraph : Paragraph
    {
        public Uri Target { get; }

        public string DisplayText { get; }

        public LinkParagraph(Uri target, string displayText)
        {
            Target = target;
            DisplayText = displayText;
        }
    }
}
