using System;

namespace EpwingRemapper.Core
{
    public class CodeIdentifier : IEquatable<CodeIdentifier>
    {
        public CharacterKind Kind { get; }

        public int Code { get; }

        public int SubBookIndex { get; }

        public CodeIdentifier(CharacterKind kind, int code, int subBookIndex)
        {
            Kind = kind;
            Code = code;
            SubBookIndex = subBookIndex;
        }

        public bool Equals(CodeIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Kind == other.Kind && Code == other.Code && SubBookIndex == other.SubBookIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CodeIdentifier) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Kind;
                hashCode = (hashCode * 397) ^ Code;
                hashCode = (hashCode * 397) ^ SubBookIndex;
                return hashCode;
            }
        }

        public static bool operator ==(CodeIdentifier left, CodeIdentifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CodeIdentifier left, CodeIdentifier right)
        {
            return !Equals(left, right);
        }
    }
}