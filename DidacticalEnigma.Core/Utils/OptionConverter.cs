using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Optional;

namespace DidacticalEnigma.Core.Utils
{
    // WARNING: THIS FLATTENS NONEs
    internal class OptionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Option<>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var innerType = objectType.GetGenericArguments()?.FirstOrDefault() ?? throw new InvalidOperationException("No inner type found.");
            var noneMethod = MakeStaticGenericMethodInfo(nameof(None), innerType);
            var someMethod = MakeStaticGenericMethodInfo(nameof(Some), innerType);

            if (reader.TokenType == JsonToken.Null)
            {
                return noneMethod.Invoke(null, new object[] { });
            }

            var innerValue = serializer.Deserialize(reader, innerType);

            if (innerValue == null)
            {
                return noneMethod.Invoke(null, new object[] { });
            }

            return someMethod.Invoke(noneMethod, new[] { innerValue });
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var innerType = value.GetType()?.GetGenericArguments()?.FirstOrDefault() ?? throw new InvalidOperationException("No inner type found.");
            var hasValueMethod = MakeStaticGenericMethodInfo(nameof(HasValue), innerType);
            var getValueMethod = MakeStaticGenericMethodInfo(nameof(GetValue), innerType);

            var hasValue = (bool)hasValueMethod.Invoke(null, new[] { value });

            if (!hasValue)
            {
                writer.WriteNull();
                return;
            }

            var innerValue = getValueMethod.Invoke(null, new[] { value });
            serializer.Serialize(writer, innerValue);
        }

        private MethodInfo MakeStaticGenericMethodInfo(string name, params Type[] typeArguments)
        {
            return GetType()
                ?.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
                ?.MakeGenericMethod(typeArguments)
                ?? throw new InvalidOperationException($"Could not make generic MethodInfo for method '{name}'.");
        }

        private static bool HasValue<T>(Option<T> option) => option.HasValue;
        private static T GetValue<T>(Option<T> option) => option.ValueOr(default(T));
        private static Option<T> None<T>() => Option.None<T>();
        private static Option<T> Some<T>(T value) => Option.Some(value);
    }

}
