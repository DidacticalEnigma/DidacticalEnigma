using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.Utils
{
    public static class EnumExt
    {
        public static TEnum Parse<TEnum>(string input)
            where TEnum : struct, Enum
        {
            return (TEnum)Enum.Parse(typeof(TEnum), input);
        }

        private static object ParseNumericImpl<TEnum>(string input)
            where TEnum : struct, Enum
        {
            var underlying = Enum.GetUnderlyingType(typeof(TEnum));

            if (underlying == typeof(byte))
            {
                return byte.Parse(input);
            }

            if (underlying == typeof(sbyte))
            {
                return sbyte.Parse(input);
            }

            if (underlying == typeof(short))
            {
                return short.Parse(input);
            }

            if (underlying == typeof(ushort))
            {
                return ushort.Parse(input);
            }

            if (underlying == typeof(int))
            {
                return int.Parse(input);
            }

            if (underlying == typeof(uint))
            {
                return uint.Parse(input);
            }

            if (underlying == typeof(long))
            {
                return long.Parse(input);
            }

            if (underlying == typeof(ulong))
            {
                return ulong.Parse(input);
            }

            throw new ArgumentException(nameof(underlying));
        }

        public static TEnum ParseNumeric<TEnum>(string input)
            where TEnum : struct, Enum
        {
            var obj = ParseNumericImpl<TEnum>(input);
            return (TEnum)Enum.ToObject(typeof(TEnum), obj);
        }

        public static TEnum ParseNumericExact<TEnum>(string input)
            where TEnum : struct, Enum
        {
            var obj = ParseNumericImpl<TEnum>(input);
            if(!Enum.IsDefined(typeof(TEnum), obj))
                throw new ArgumentException("not a valid enum value", nameof(input));
            return (TEnum)Enum.ToObject(typeof(TEnum), obj);
        }
    }
}
