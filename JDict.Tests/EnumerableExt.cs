using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Utils;

namespace JDict.Tests
{
    [TestFixture]
    class EnumerableExt
    {
        [Test]
        public void Intersperse()
        {
            var input = new int[][]{
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
