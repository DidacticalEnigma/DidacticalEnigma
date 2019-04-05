using System;

namespace DidacticalEnigma.Core.Models.Project
{
    public class GlossNote : TranslatorNote, IEquatable<GlossNote>
    {
        public bool Equals(GlossNote other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && string.Equals(Foreign, other.Foreign);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GlossNote) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Foreign != null ? Foreign.GetHashCode() : 0);
            }
        }

        public static bool operator ==(GlossNote left, GlossNote right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GlossNote left, GlossNote right)
        {
            return !Equals(left, right);
        }

        public string Foreign { get; }

        public GlossNote(string foreign, string text) :
            base(text)
        {
            Foreign = foreign ?? throw new ArgumentNullException(nameof(foreign));
        }

        public override string ToString()
        {
            return $"{Foreign}: {Text}";
        }
    }
}