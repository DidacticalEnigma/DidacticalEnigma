namespace MagicTranslatorProject
{
    public class NamedCharacter : CharacterType
    {
        public string Name { get; }

        public NamedCharacter(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return $"Character: {Name}";
        }
    }

    public class BasicCharacter : CharacterType
    {
        public string Kind { get; }

        public BasicCharacter(string kind)
        {
            this.Kind = kind;
        }

        public override string ToString()
        {
            switch (Kind)
            {
                case "unknown":
                    return "N/A";
                case "chapter-title":
                    return "Chapter Title";
                case "narrator":
                    return "Narrator";
                case "sfx":
                    return "SFX";
                default:
                    return Kind;
            }
        }
    }

    public abstract class CharacterType
    {
        public abstract override string ToString();
    }
}