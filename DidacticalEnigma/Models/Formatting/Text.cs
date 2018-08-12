namespace DidacticalEnigma.Models
{
    public sealed class Text
    {
        public Text(string content, bool emphasis = false, FontSize fontSize = FontSize.Normal)
        {
            Content = content;
            Emphasis = emphasis;
            FontSize = fontSize;
        }

        public string Content { get; }
        public bool Emphasis { get; }

        public FontSize FontSize { get; }
    }
}