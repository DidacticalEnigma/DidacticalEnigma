using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DidacticalEnigma.Core.Models.Project
{
    public class TranslatedText : TranslatorNote, IEquatable<TranslatedText>
    {
        public string Author { get; }

        public TranslatedText(string author, string text) :
            base(text)
        {
            Author = author ?? throw new ArgumentNullException(nameof(author));
        }

        public bool Equals(TranslatedText other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && string.Equals(Author, other.Author);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TranslatedText) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Author != null ? Author.GetHashCode() : 0);
            }
        }

        public static bool operator ==(TranslatedText left, TranslatedText right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TranslatedText left, TranslatedText right)
        {
            return !Equals(left, right);
        }
    }
}