using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Utils;
using NUnit.Framework;

namespace JDict.Tests
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
    }
}
