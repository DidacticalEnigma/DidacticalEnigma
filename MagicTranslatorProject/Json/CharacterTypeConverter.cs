using System;
using Newtonsoft.Json;
using Utility.Utils;

namespace MagicTranslatorProject
{
    public class CharacterTypeConverter : JsonConverter<CharacterType>
    {
        private IReadOnlyDualDictionary<long, string> nameMapping;

        public override void WriteJson(JsonWriter writer, CharacterType value, JsonSerializer serializer)
        {
            switch (value)
            {
                case BasicCharacter basic:
                    serializer.Serialize(writer, basic.Kind);
                    break;
                case NamedCharacter named:
                    serializer.Serialize(writer, nameMapping.Inverse[named.Name]);
                    break;
                default:
                    throw new JsonSerializationException();
            }
        }

        public override CharacterType ReadJson(JsonReader reader, Type objectType, CharacterType existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var o = serializer.Deserialize(reader);
            switch (o)
            {
                case long i:
                    return new NamedCharacter(nameMapping[i]);
                case string s:
                    return new BasicCharacter(s);
                default:
                    throw new JsonSerializationException();
            }
        }

        public CharacterTypeConverter(IReadOnlyDualDictionary<long, string> nameMapping)
        {
            this.nameMapping = nameMapping;
        }
    }
}