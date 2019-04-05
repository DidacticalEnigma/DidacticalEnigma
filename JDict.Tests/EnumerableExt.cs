using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Utility.Utils;

namespace JDict.Tests
{
    [TestFixture]
    class EnumerableExt
    {
        [Test]
        public void Lexicographical()
        {
            Assert.True("".LexicographicalCompare("") == 0);
            Assert.True("".LexicographicalCompare("a") < 0);
            Assert.True("a".LexicographicalCompare("") > 0);
            Assert.True("hello".LexicographicalCompare("world") < 0);
            Assert.True("hello".LexicographicalCompare("hellow") < 0);
            Assert.True("hellow".LexicographicalCompare("hellox") < 0);
        }

        [Test]
        public void Intersperse()
        {
            var input = new[]
            {
                new int[] { 1, 2 },
                new int[] { 5, 6 },
                new int[] { },
                new int[] { 2 }
            };
            var expected = new[] { 1, 2, -1, 5, 6, -1, 2 };
            var actual = Utility.Utils.EnumerableExt.IntersperseSequencesWith(input, -1).ToList();
            CollectionAssert.AreEqual(
                expected,
                actual);

            input = new int[][]
            {
                new int[]{},
                new int[]{1},
            };
            CollectionAssert.AreEqual(
                new[] { 1 },
                Utility.Utils.EnumerableExt.IntersperseSequencesWith(input, -1).ToList());
        }

        private static int[][] GroupConsecutive(int[] input)
        {
            return input.GroupConsecutive(x => x).Select(x => x.ToArray()).ToArray();
        }

        [Test]
        public void GroupConsecutive()
        {
            CollectionAssert.AreEqual(new int[][] { }, GroupConsecutive(new int[0]));
            CollectionAssert.AreEqual(new int[][] { new[] { 1 } }, GroupConsecutive(new int[] { 1 }));
            CollectionAssert.AreEqual(new int[][] { new[] { 1, 1 } }, GroupConsecutive(new int[] { 1, 1 }));
            CollectionAssert.AreEqual(new int[][] { new[] { 1, 1, 1 } }, GroupConsecutive(new int[] { 1, 1, 1 }));
            CollectionAssert.AreEqual(new int[][] { new[] { 1, 1 }, new[] { 3 } }, GroupConsecutive(new int[] { 1, 1, 3 }));
            CollectionAssert.AreEqual(new int[][] { new[] { 1 }, new[] { 3 }, new[] { 1 } }, GroupConsecutive(new int[] { 1, 3, 1 }));
        }

        [Test]
        public void InvertMapping()
        {
            var dict = new Dictionary<string, IEnumerable<int>>
            {
                { "hello",  new []{1,2} },
                { "world",  new []{1,3} }
            };
            CollectionAssert.AreEquivalent(new Dictionary<int, IEnumerable<string>>
            {
                {1, new []{ "hello", "world" } },
                {2, new []{ "hello" } },
                {3, new []{ "world" } }
            }, dict.InvertMappingToSequence());
        }
    }
}
