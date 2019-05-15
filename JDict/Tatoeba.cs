
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Optional;
using Utility.Utils;

namespace JDict
{
    public static class Tatoeba
    {
        public static IEnumerable<TatoebaSentence> ParseSentences(string path)
        {
            using (var reader = File.OpenText(path))
            {
                foreach (var tatoebaSentence in ParseSentences(reader))
                {
                    yield return tatoebaSentence;
                }
            }
        }

        public static IEnumerable<TatoebaSentence> ParseSentences(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return ParseSentence(line);
            }
        }

        private static TatoebaSentence ParseSentence(string sentenceLine)
        {
            var components = sentenceLine.Split('\t');
            return new TatoebaSentence(
                long.Parse(components[0]),
                components[1],
                components[2]);
        }

        public static IEnumerable<TatoebaSentenceDetailed> ParseSentencesDetailed(string path)
        {
            using (var reader = File.OpenText(path))
            {
                foreach (var tatoebaSentence in ParseSentencesDetailed(reader))
                {
                    yield return tatoebaSentence;
                }
            }
        }

        public static IEnumerable<TatoebaSentenceDetailed> ParseSentencesDetailed(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return ParseSentenceDetailed(line);
            }
        }

        private static Option<DateTime> ParseTimeOpt(string time)
        {
            return time == "\\N" || time == "0000-00-00 00:00:00"
                ? ParseTime(time).Some()
                : Option.None<DateTime>();
        }

        private static DateTime ParseTime(string time)
        {
            return DateTime.ParseExact(
                time,
                "yyyy-MM-dd hh:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal);
        }

        private static Editability ParseEditability(string input)
        {
            switch (input)
            {
                case "anyone": return Editability.Anyone;
                case "creator": return Editability.Creator;
                case "no_one": return Editability.NoOne;
            }

            throw new ArgumentException($"'{input}' is not valid input value", nameof(input));
        }

        private static TatoebaSentenceDetailed ParseSentenceDetailed(string sentenceLine)
        {
            var components = sentenceLine.Split('\t');
            return new TatoebaSentenceDetailed(
                long.Parse(components[0]),
                components[1],
                components[2],
                components[3],
                ParseTimeOpt(components[4]),
                ParseTimeOpt(components[5]));
        }

        public static IEnumerable<TatoebaLink> ParseLinks(string path)
        {
            using (var reader = File.OpenText(path))
            {
                foreach (var tatoebaLink in ParseLinks(reader))
                {
                    yield return tatoebaLink;
                }
            }
        }

        public static IEnumerable<TatoebaLink> ParseLinks(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return ParseLink(line);
            }
        }

        private static TatoebaLink ParseLink(string line)
        {
            var components = line.Split('\t');
            return new TatoebaLink(
                long.Parse(components[0]),
                long.Parse(components[1]));
        }

        public static IEnumerable<TatoebaTag> ParseTags(string path)
        {
            using (var reader = File.OpenText(path))
            {
                foreach (var tatoebaTag in ParseTags(reader))
                {
                    yield return tatoebaTag;
                }
            }
        }

        public static IEnumerable<TatoebaTag> ParseTags(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return ParseTag(line);
            }
        }

        private static TatoebaTag ParseTag(string line)
        {
            var components = line.Split('\t');
            return new TatoebaTag(
                long.Parse(components[0]),
                components[1]);
        }

        public static IEnumerable<TatoebaList> ParseLists(string path)
        {
            using (var reader = File.OpenText(path))
            {
                foreach (var tatoebaList in ParseLists(reader))
                {
                    yield return tatoebaList;
                }
            }
        }

        public static IEnumerable<TatoebaList> ParseLists(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return ParseList(line);
            }
        }

        private static TatoebaList ParseList(string line)
        {
            var components = line.Split('\t');
            return new TatoebaList(
                long.Parse(components[0]),
                components[1],
                ParseTime(components[2]),
                ParseTime(components[3]),
                components[4],
                ParseEditability(components[5]));
        }

        public static IEnumerable<TatoebaListSentenceLink> ParseListSentenceLinks(string path)
        {
            using (var reader = File.OpenText(path))
            {
                foreach (var tatoebaListSentenceLink in ParseListSentenceLinks(reader))
                {
                    yield return tatoebaListSentenceLink;
                }
            }
        }

        public static IEnumerable<TatoebaListSentenceLink> ParseListSentenceLinks(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return ParseListSentenceLink(line);
            }
        }

        private static TatoebaListSentenceLink ParseListSentenceLink(string line)
        {
            var components = line.Split('\t');
            return new TatoebaListSentenceLink(
                long.Parse(components[0]),
                long.Parse(components[1]));
        }

        public static IEnumerable<TatoebaJapaneseIndex> ParseJapaneseIndices(string path)
        {
            using (var reader = File.OpenText(path))
            {
                foreach (var japaneseIndex in ParseJapaneseIndices(reader))
                {
                    yield return japaneseIndex;
                }
            }
        }

        public static IEnumerable<TatoebaJapaneseIndex> ParseJapaneseIndices(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return ParseJapaneseIndex(line);
            }
        }

        private static TatoebaJapaneseIndex ParseJapaneseIndex(string line)
        {
            var components = line.Split('\t');
            return new TatoebaJapaneseIndex(
                long.Parse(components[0]),
                long.Parse(components[1]),
                components[2]);
        }

        public static IEnumerable<TatoebaSentenceAudio> ParseSentenceAudio(string path)
        {
            using (var reader = File.OpenText(path))
            {
                foreach (var sentenceAudio in ParseSentenceAudio(reader))
                {
                    yield return sentenceAudio;
                }
            }
        }

        public static IEnumerable<TatoebaSentenceAudio> ParseSentenceAudio(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return ParseSentenceAudioLine(line);
            }
        }

        private static TatoebaSentenceAudio ParseSentenceAudioLine(string line)
        {
            var components = line.Split('\t');
            return new TatoebaSentenceAudio(
                long.Parse(components[0]),
                components[1],
                components[2] == "" || components[2] == "\\N" ? License.Of("tatoeba.org use only") : License.Of(components[2]),
                components[3] == "" || components[3] == "\\N" ? Option.None<Uri>() : new Uri(components[3]).Some());
        }
        
        public static IEnumerable<TatoebaUserSkill> ParseUserSkills(string path)
        {
            using (var reader = File.OpenText(path))
            {
                foreach (var userSkill in ParseUserSkills(reader))
                {
                    yield return userSkill;
                }
            }
        }

        public static IEnumerable<TatoebaUserSkill> ParseUserSkills(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return ParseUserSkill(line);
            }
        }

        private static TatoebaUserSkill ParseUserSkill(string line)
        {
            var components = line.Split('\t');
            return new TatoebaUserSkill(
                components[0],
                components[1] == "\\N" ? Option.None<int>() : int.Parse(components[1]).Some(),
                components[2],
                components[3]);
        }
        
        public static IEnumerable<TatoebaUserSentenceRating> ParseUserSentenceRatings(string path)
        {
            using (var reader = File.OpenText(path))
            {
                foreach (var rating in ParseUserSentenceRatings(reader))
                {
                    yield return rating;
                }
            }
        }

        public static IEnumerable<TatoebaUserSentenceRating> ParseUserSentenceRatings(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return ParseUserSentenceRating(line);
            }
        }

        private static TatoebaUserSentenceRating ParseUserSentenceRating(string line)
        {
            var components = line.Split('\t');
            return new TatoebaUserSentenceRating(
                components[0],
                components[1],
                long.Parse(components[2]),
                EnumExt.ParseNumericExact<Rating>(components[3]),
                ParseTime(components[4]),
                ParseTime(components[5]));
        }
    }

    public enum Editability
    {
        NoOne,
        Anyone,
        Creator
    }

    public enum Rating
    {
        // undecided or unsure
        Undecided = 0,
        // OK
        Ok = 1,
        NotOk = -1
    }

    public class TatoebaUserSentenceRating
    {
        public string Username { get; }

        public string Language { get; }

        public long SentenceId { get; }

        public Rating Rating { get; }

        public DateTime DateAdded { get; }

        public DateTime DateModified { get; }

        public TatoebaUserSentenceRating(
            string username,
            string language,
            long sentenceId,
            Rating rating,
            DateTime dateAdded,
            DateTime dateModified)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Language = language ?? throw new ArgumentNullException(nameof(language));
            SentenceId = sentenceId;
            Rating = rating;
            DateAdded = dateAdded;
            DateModified = dateModified;
        }
    }

    public class TatoebaUserSkill
    {
        public string Language { get; }

        public Option<int> SkillLevel { get; }

        public string Username { get; }

        public string Details { get; }

        public TatoebaUserSkill(string language, Option<int> skillLevel, string username, string details)
        {
            Language = language ?? throw new ArgumentNullException(nameof(language));
            SkillLevel = skillLevel;
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Details = details ?? throw new ArgumentNullException(nameof(details));
        }
    }

    public class License : IEquatable<License>
    {
        public string Identifier { get; }

        public override string ToString()
        {
            return Identifier;
        }

        private License(string identifier)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }

        public License Create(string identifier)
        {
            return new License(identifier);
        }

        public bool Equals(License other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Identifier, other.Identifier);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((License) obj);
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        public static bool operator ==(License left, License right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(License left, License right)
        {
            return !Equals(left, right);
        }

        // Attribution 4.0 International (CC BY 4.0) 
        // https://creativecommons.org/licenses/by/4.0/
        public static readonly License CcBy40 = new License("CC BY 4.0");

        // Attribution-NonCommercial 4.0 International (CC BY-NC 4.0) 
        // https://creativecommons.org/licenses/by-nc/4.0/
        public static readonly License CcByNc40 = new License("CC BY-NC 4.0");

        // Attribution-NonCommercial-NoDerivs 3.0 Unported (CC BY-NC-ND 3.0) 
        // https://creativecommons.org/licenses/by-nc-nd/3.0/
        public static readonly License CcByNcNd30 = new License("CC BY-NC-ND 3.0");

        // Attribution-ShareAlike 4.0 International (CC BY-SA 4.0) 
        // https://creativecommons.org/licenses/by-sa/4.0/
        public static readonly License CcBySa40 = new License("CC BY-SA 4.0");

        public static License Of(string s)
        {
            return new License(s);
        }
    }

    public class TatoebaSentenceAudio
    {
        public long SentenceId { get; }

        public string Username { get; }

        public License License { get; }

        public Option<Uri> AttributionUrl { get; }

        public TatoebaSentenceAudio(long sentenceId, string username, License license, Option<Uri> attributionUrl)
        {
            SentenceId = sentenceId;
            Username = username ?? throw new ArgumentNullException(nameof(username));
            License = license ?? throw new ArgumentNullException(nameof(license));
            AttributionUrl = attributionUrl;
        }
    }

    public class TatoebaJapaneseIndex
    {
        public long JapaneseSentenceId { get; }

        public long EnglishSentenceId { get; }

        public string Index { get; }

        public TatoebaJapaneseIndex(long japaneseSentenceId, long englishSentenceId, string index)
        {
            JapaneseSentenceId = japaneseSentenceId;
            EnglishSentenceId = englishSentenceId;
            Index = index ?? throw new ArgumentNullException(nameof(index));
        }
    }

    public class TatoebaListSentenceLink
    {
        public long ListId { get; }

        public long SentenceId { get; }

        public TatoebaListSentenceLink(long listId, long sentenceId)
        {
            ListId = listId;
            SentenceId = sentenceId;
        }
    }

    public class TatoebaList
    {
        public long ListId { get; }

        public string Username { get; }

        public DateTime DateCreated { get; }

        public DateTime DateModified { get; }

        public string ListName { get; }

        public Editability EditableBy { get; }

        public TatoebaList(
            long listId,
            string username,
            DateTime dateCreated,
            DateTime dateModified,
            string listName,
            Editability editableBy)
        {
            ListId = listId;
            Username = username ?? throw new ArgumentNullException(nameof(username));
            DateCreated = dateCreated;
            DateModified = dateModified;
            ListName = listName ?? throw new ArgumentNullException(nameof(listName));
            EditableBy = editableBy;
        }
    }

    public class TatoebaTag
    {
        public long SentenceId { get; }

        public string TagName { get; }

        public TatoebaTag(long sentenceId, string tagName)
        {
            SentenceId = sentenceId;
            TagName = tagName ?? throw new ArgumentNullException(nameof(tagName));
        }
    }

    public class TatoebaLink
    {
        public long SentenceId { get; }

        public long TranslationId { get; }

        public TatoebaLink(long sentenceId, long translationId)
        {
            SentenceId = sentenceId;
            TranslationId = translationId;
        }

        public KeyValuePair<long, long> AsKeyValuePair()
        {
            return new KeyValuePair<long, long>(SentenceId, TranslationId);
        }
    }

    public class TatoebaSentence
    {
        public long SentenceId { get; }

        public string Language { get; }

        public string Text { get; }

        public TatoebaSentence(long sentenceId, string language, string text)
        {
            SentenceId = sentenceId;
            Language = language ?? throw new ArgumentNullException(nameof(language));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }

    public class TatoebaSentenceDetailed
    {
        public long SentenceId { get; }

        public string Language { get; }

        public string Text { get; }

        public string Username { get; }

        public Option<DateTime> DateAdded { get; }

        public Option<DateTime> DateModified { get; }

        public TatoebaSentenceDetailed(
            long sentenceId,
            string language,
            string text,
            string username,
            Option<DateTime> dateAdded = default,
            Option<DateTime> dateModified = default)
        {
            SentenceId = sentenceId;
            Language = language ?? throw new ArgumentNullException(nameof(language));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            DateAdded = dateAdded;
            DateModified = dateModified;
        }

        public TatoebaSentence AsSentence()
        {
            return new TatoebaSentence(SentenceId, Language, Text);
        }
    }
}
