using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace JDict
{
    public class EpWing
    {
        public EpWing(string path, string cache, Func<string, string> codeReplacer)
        {
            using (var reader = File.OpenText(path))
            {
                Init(reader, codeReplacer);
            }
        }

        public EpWing(string path, string cache) :
            this(path, cache, code => throw new InvalidDataException("The input was assumed to be untranslated-code free, yet it wasn't"))
        {

        }

        private void Init(TextReader rawReader, Func<string, string> codeReplacer)
        {
            using (var reader = new JsonTextReader(rawReader))
            {
                if (!reader.Read() || reader.TokenType != JsonToken.StartObject)
                {
                    throw new InvalidDataException();
                }

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndObject)
                        break;
                    if (reader.TokenType != JsonToken.PropertyName)
                        throw new InvalidDataException();

                    switch ((string)reader.Value)
                    {
                        case "charCode":
                            reader.Skip();
                            break;
                        case "discCode":
                            reader.Skip();
                            break;
                        case "subbooks":
                            if (!reader.Read() || reader.TokenType != JsonToken.StartArray)
                            {
                                throw new InvalidDataException();
                            }
                            ReadSubbooks(reader, codeReplacer);
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
            }
        }

        private void ReadSubbooks(JsonReader reader, Func<string, string> codeReplacer)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                    break;
                if (reader.TokenType != JsonToken.StartObject)
                    throw new InvalidDataException();

                ReadSubbook(reader, codeReplacer);

            }
        }

        private void ReadSubbook(
            JsonReader reader,
            Func<string, string> codeReplacer)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;
                if (reader.TokenType != JsonToken.PropertyName)
                    throw new InvalidDataException();

                var d = new Dictionary<string, List<string>>();
                switch ((string)reader.Value)
                {
                    case "title":
                        reader.Skip();
                        break;
                    case "copyright":
                        reader.Skip();
                        break;
                    case "entries":
                        if (!reader.Read() || reader.TokenType != JsonToken.StartArray)
                        {
                            throw new InvalidDataException();
                        }
                        ReadEntries(reader, d, codeReplacer);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
                subbooks.Add(d);
            }
        }

        private void ReadEntries(
            JsonReader reader,
            Dictionary<string, List<string>> d,
            Func<string, string> codeReplacer)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                    break;
                if (reader.TokenType != JsonToken.StartObject)
                    throw new InvalidDataException();

                ReadEntry(reader, d, codeReplacer);
            }
        }

        private void ReadEntry(
            JsonReader reader,
            Dictionary<string, List<string>> d,
            Func<string, string> codeReplacer)
        {
            string heading = null;
            string text = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    throw new InvalidDataException();
                }

                switch ((string) reader.Value)
                {
                    case "heading":
                        if (!reader.Read() || reader.TokenType != JsonToken.String)
                            throw new InvalidDataException();
                        heading = (string)reader.Value;
                        break;
                    case "text":
                        if (!reader.Read() || reader.TokenType != JsonToken.String)
                            throw new InvalidDataException();
                        text = (string)reader.Value;
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
            if(text == null || heading == null)
                throw new InvalidDataException();

            text = Remap(text, codeReplacer);
            heading = Remap(text, codeReplacer);

            var entries = GetOrAdd(d, heading, () => new List<string>());
            entries.Add(text);
        }

        private static TValue GetOrAdd<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key, Func<TValue> Valuefactory)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                value = Valuefactory();
                dict[key] = value;
                return value;
            }
        }

        private static readonly Regex remappingRegex = new Regex(@"(\{\{.*?\}\})");

        private string Remap(string input, Func<string, string> remapper)
        {
            return remappingRegex.Replace(input, match => remapper(match.Value));
        }

        private readonly List<Dictionary<string, List<string>>> subbooks = new List<Dictionary<string, List<string>>>();
    }

    public class EpWingEntry
    {
        public string Key { get; }

        public string Text { get; }

        public EpWingEntry(string key, string text)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}
