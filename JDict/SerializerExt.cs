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
            private readonly ISerializer<T> serializer;

            public OptionalSerializer(ISerializer<T> serializer)
            {
                this.serializer = serializer;
            }

            public Option<T> Deserialize(byte[] sourceBuffer, int sourceBufferOffset, int sourceBufferLength)
            {
                if (sourceBuffer[0] == 0)
                    return Option.Some(serializer.Deserialize(
                        sourceBuffer, 
                        sourceBufferOffset + 1,
                        sourceBufferLength - 1));
                else
                    return Option.None<T>();
            }

            public bool TrySerialize(Option<T> element, byte[] destinationBuffer, int destinationBufferOffset, int destinationBufferLength,
                out int actualSize)
            {
                if (destinationBufferLength < 1)
                {
                    actualSize = 0;
                    return false;
                }

                var (result, actualSizeResult) = element.Match(e =>
                    {
                        destinationBuffer[destinationBufferOffset] = 0;
                        var r = serializer.TrySerialize(
                            e,
                            destinationBuffer,
                            destinationBufferOffset + 1,
                            destinationBufferLength - 1,
                            out var a);
                        return (r, a+1);
                    },
                    () =>
                    {
                        destinationBuffer[destinationBufferOffset] = 1;
                        return (true, 1);
                    });
                actualSize = actualSizeResult;
                return result;
            }
        }

        public static ISerializer<Option<T>> ForOption<T>(ISerializer<T> serializer)
        {
            return new OptionalSerializer<T>(serializer);
        }
    }
}
