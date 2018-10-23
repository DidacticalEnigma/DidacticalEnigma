using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JDict.Json
{
    // {"title":"研究社　新和英大辞典　第５版","format":3,"revision":"wadai1","sequenced":true}
    class YomichanDictionaryVersion
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("format")]
        public int Format { get; set; }

        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("sequenced")]
        public bool Sequenced { get; set; }
    }

    class YomichanDictionaryInfo : DbDictVersion
    {
        public string Title { get; set; }

        public string Revision { get; set; }
    }

    [JsonConverter(typeof(YomichanDictionaryEntryConverter))]
    class YomichanDictionaryEntry
    {
        public long Id { get; set; }

        public string Expression { get; set; }

        public string Reading { get; set; }

        public string DefinitionTags { get; set; }

        public string Rules { get; set; }

        public int Score { get; set; }

        public IEnumerable<string> Glossary { get; set; }

        public int Sequence { get; set; }

        public string TermTags { get; set; }
    }

    class YomichanDictionaryEntryConverter : JsonConverter<YomichanDictionaryEntry>
    {
        public override void WriteJson(JsonWriter writer, YomichanDictionaryEntry value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new object[]
            {
                value.Expression,
                value.Reading,
                value.DefinitionTags,
                value.Rules,
                value.Score,
                value.Glossary,
                value.Sequence,
                value.TermTags
            });
        }

        public override YomichanDictionaryEntry ReadJson(
            JsonReader reader,
            Type objectType,
            YomichanDictionaryEntry existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var arr = serializer.Deserialize<JArray>(reader);
            var newValue = new YomichanDictionaryEntry
            {
                Expression = arr[0].Value<string>(),
                Reading = arr[1].Value<string>(),
                DefinitionTags = arr[2].Value<string>(),
                Rules = arr[3].Value<string>(),
                Score = arr[4].Value<int>(),
                Glossary = arr[5].Values<string>().ToList(),
                Sequence = arr[6].Value<int>(),
                TermTags = arr[7].Value<string>()
            };
            return newValue;
        }
    }
}
