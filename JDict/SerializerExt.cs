using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Optional;
using TinyIndex;

[assembly: InternalsVisibleTo("JDict.Tests")]

namespace JDict
{
    internal static class SerializerExt
    {
        private class OptionalSerializer<T> : ISerializer<Option<T>>
        {
            private static bool TryGetMaybe<U>(Option<U> input, out U output)
            {
                output = Optional.Unsafe.OptionUnsafeExtensions.ValueOrDefault(input);
                return input.HasValue;
            }

            private readonly ISerializer<T> serializer;

            public OptionalSerializer(ISerializer<T> serializer)
            {
                this.serializer = serializer;
            }

            public Option<T> Deserialize(ReadOnlySpan<byte> input)
            {
                if (input[0] == 0)
                    return Option.Some(serializer.Deserialize(input.Slice(1)));
                else
                    return Option.None<T>();
            }

            public bool TrySerialize(Option<T> element, Span<byte> output, out int actualSize)
            {
                if (output.Length < 1)
                {
                    actualSize = 0;
                    return false;
                }

                if (TryGetMaybe(element, out var e))
                {
                    output[0] = 0;
                    var r = serializer.TrySerialize(
                        e,
                        output.Slice(1),
                        out var a);
                    actualSize = a + 1;
                    return r;
                }
                else
                {
                    output[0] = byte.MaxValue;
                    actualSize = 1;
                    return true;
                }
            }
        }

        public static ISerializer<Option<T>> ForOption<T>(ISerializer<T> serializer)
        {
            return new OptionalSerializer<T>(serializer);
        }
    }
}
