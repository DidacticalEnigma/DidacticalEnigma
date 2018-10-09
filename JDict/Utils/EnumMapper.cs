using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Optional;
using Optional.Collections;

namespace JDict.Utils
{
    internal class EnumMapper<T>
        where T : Enum
    {
        public Option<T> FromDescription(string description)
        {
            return mapping.GetValueOrNone(description);
        }

        public string ToLongString(T value)
        {
            return inverseMapping
                .GetValueOrNone(value)
                .ValueOr(() => value.ToString());
        }

        // https://stackoverflow.com/questions/2650080/how-to-get-c-sharp-enum-description-from-value
        private static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    inherit: false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        private readonly Dictionary<string, T> mapping;

        private readonly Dictionary<T, string> inverseMapping;

        public EnumMapper()
        {
            mapping = Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToDictionary(e => GetEnumDescription(e), e => e);
            inverseMapping = mapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        }
    }
}
