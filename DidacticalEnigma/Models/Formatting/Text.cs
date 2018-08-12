namespace DidacticalEnigma.Models
{
    public sealed class Text
    {
        public Text(string content, bool emphasis = false, FontSize fontSize = FontSize.Normal, string fontName = null)
        {
            Content = content;
            Emphasis = emphasis;
            FontSize = fontSize;
            FontName = fontName;
        }

        public string Content { get; }
        public bool Emphasis { get; }

        public string FontName { get; }

        public FontSize FontSize { get; }
    }
}