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
            if (value is BasicCharacter basic)
            {
                serializer.Serialize(writer, basic.Kind);
            }
            else if (value is NamedCharacter named)
            {
                serializer.Serialize(writer, nameMapping.Inverse[named.Name]);
            }
            else
            {
                throw new JsonSerializationException();
            }
        }

        public override CharacterType ReadJson(JsonReader reader, Type objectType, CharacterType existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var o = serializer.Deserialize(reader);
            if (o is long i)
            {
                return new NamedCharacter(nameMapping[i]);
            }
            else if(o is string s)
            {
                return new BasicCharacter(s);
            }
            else
            {
                throw new JsonSerializationException();
            }
        }

        public CharacterTypeConverter(IReadOnlyDualDictionary<long, string> nameMapping)
        {
            this.nameMapping = nameMapping;
        }
    }
}