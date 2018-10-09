using System;
using System.Linq;

namespace JDict
{
    internal class DbDictVersion : IEquatable<DbDictVersion>
    {
        public long Id { get; set; }

        public int DbVersion { get; set; }

        public long OriginalFileSize { get; set; }

        public byte[] OriginalFileHash { get; set; }

        public bool Equals(DbDictVersion other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DbVersion.Equals(other.DbVersion) &&
                   OriginalFileSize == other.OriginalFileSize &&
                   OriginalFileHash?.SequenceEqual(other.OriginalFileHash ?? Enumerable.Empty<byte>()) == true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DbDictVersion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DbVersion.GetHashCode();
                hashCode = (hashCode * 397) ^ OriginalFileSize.GetHashCode();
                hashCode = (hashCode * 397) ^ (OriginalFileHash != null ? Hash(OriginalFileHash) : 0);
                return hashCode;
            }

            int Hash(byte[] h)
            {
                int x = 0;
                unchecked
                {   
                    foreach (var b in h)
                    {
                        x = x * 33 + b;
                    }
                }
                return x;
            }
        }

        public static bool operator ==(DbDictVersion left, DbDictVersion right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DbDictVersion left, DbDictVersion right)
        {
            return !Equals(left, right);
        }
    }
}