using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var actual = DidacticalEnigma.Utils.EnumerableExt.IntersperseSequencesWith(input, -1).ToList();
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
                DidacticalEnigma.Utils.EnumerableExt.IntersperseSequencesWith(input, -1).ToList());
        }
    }
}
