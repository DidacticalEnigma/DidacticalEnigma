using NUnit.Framework;
using Utility.Utils;

namespace AutomatedTests
{
    [TestFixture]
    class StringExtTests
    {
        [Test]
        public void Split()
        {
            CollectionAssert.AreEqual(new[]{ "a", "", "c" }, StringExt.SplitWithQuotes("a,,c", ',', '"'));
            CollectionAssert.AreEqual(new[] { "", "a", "", "c", "" }, StringExt.SplitWithQuotes(",a,,c,", ',', '"'));
            CollectionAssert.AreEqual(new[] { "a", "bc,a,s", "c" }, StringExt.SplitWithQuotes("a,|bc,a,s|,c", ',', '|'));
        }

        [Test]
        public void Rotations()
        {
            CollectionAssert.AreEquivalent(new[]{"abc", "bca", "cab"}, StringExt.AllRotationsOf("abc"));
        }
    }
}
