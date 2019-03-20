using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Optional;
using TinyIndex;

namespace JDict.Tests
{
    [TestFixture]
    class SerializerExtTests
    {
        [Test]
        public void AdvancedTests()
        {
            var serializer = Serializer.ForComposite()
                .With(SerializerExt.ForOption(Serializer.ForEnum<FileShare>()))
                .With(SerializerExt.ForOption(Serializer.ForEnum<FileShare>()))
                .Create();

            var original = new object[] {Option.None<FileShare>(), FileShare.Delete.Some()};
            var buffer = new byte[16384];
            var result = serializer.TrySerialize(original, buffer.AsSpan(), out var actualSize);
            var resurrected = serializer.Deserialize(buffer.AsSpan().Slice(0, actualSize));
            CollectionAssert.AreEqual(original, resurrected);
        }

        [Test]
        public void OptionalTest()
        {
            var serializer = SerializerExt.ForOption(Serializer.ForInt());
            var buffer = new byte[6];
            for (int i = 0; i < buffer.Length; ++i)
            {
                {
                    var original = 1000042.Some();
                    bool result = serializer.TrySerialize(original, buffer.AsSpan().Slice(0, i), out var actualSize);
                    if (i < sizeof(int) + 1)
                    {
                        Assert.False(result);
                    }
                    else
                    {
                        Assert.True(result);
                        Assert.AreEqual(sizeof(int) + 1, actualSize);
                        var resurrected = serializer.Deserialize(buffer.AsSpan().Slice(0, i));
                        Assert.AreEqual(original, resurrected);
                    }
                }
                {
                    var original = 1000042.None();
                    bool result = serializer.TrySerialize(original, buffer.AsSpan().Slice(0, i), out var actualSize);
                    if (i < 1)
                    {
                        Assert.False(result);
                    }
                    else
                    {
                        Assert.True(result);
                        Assert.AreEqual(1, actualSize);
                        var resurrected = serializer.Deserialize(buffer.AsSpan().Slice(0, i));
                        Assert.AreEqual(original, resurrected);
                    }
                }
            }
        }
    }
}
