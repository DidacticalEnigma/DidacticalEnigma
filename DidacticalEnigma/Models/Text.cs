namespace DidacticalEnigma.Models
{
    public sealed class Text
    {
        public Text(string content, bool emphasis = false)
        {
            Content = content;
            Emphasis = emphasis;
        }

        public string Content { get; }
        public bool Emphasis { get; }
    }
}