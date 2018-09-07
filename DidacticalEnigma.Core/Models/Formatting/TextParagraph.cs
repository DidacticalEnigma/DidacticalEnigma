using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Core.Models.Formatting
{
    public class TextParagraph : Paragraph
    {
        public ICollection<Text> Content { get; }

        public TextParagraph()
            : this(Enumerable.Empty<Text>())
        {

        }

        public TextParagraph(IEnumerable<Text> content)
        {
            Content = new List<Text>(content);
        }
    }
}