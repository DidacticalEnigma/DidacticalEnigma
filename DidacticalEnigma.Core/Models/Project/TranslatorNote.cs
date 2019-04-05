using System;

namespace DidacticalEnigma.Core.Models.Project
{
    public class TranslatorNote : IEquatable<TranslatorNote>
    {
        public bool Equals(TranslatorNote other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TranslatorNote) obj);
        }

        public override int GetHashCode()
        {
            return (Text != null ? Text.GetHashCode() : 0);
        }

        public static bool operator ==(TranslatorNote left, TranslatorNote right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TranslatorNote left, TranslatorNote right)
        {
            return !Equals(left, right);
        }

        public string Text { get; }

        public TranslatorNote(string text)
        {
            Text = text;
        }

        public override string ToString()
        {
            return $"{Text}";
        }
    }
}