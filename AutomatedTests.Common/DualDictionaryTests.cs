using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Utility.Utils;

namespace AutomatedTests
{
    [TestFixture]
    class DualDictionaryTests
    {
        private DualDictionary<string, string> dict;

        DualDictionary<string, string> CreateDictionary(Action<Dictionary<string, string>> ops)
        {
            var src = new Dictionary<string, string>
            {
                {"hello", "world"},
                {"asdf", "lol"}
            };
            ops(src);
            var d = new DualDictionary<string, string>(src);
            return d;
        }

        [SetUp]
        public void SetUp()
        {
            dict = CreateDictionary(d => { });
        }

        [Test]
        public void Basic()
        {
            Assert.False(dict.Remove(new KeyValuePair<string, string>("hello", "www")));
            Assert.AreEqual("world", dict.Key["hello"]);
            Assert.AreEqual("hello", dict.Value["world"]);
        }

        [Test]
        public void Basic2()
        {
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                _ = dict.Key[null];
            });
            _ = Assert.Throws<ArgumentNullException>(() =>
            {
                _ = dict.Value[null];
            });
            dict.Key["a"] = "a";
            var expected = CreateDictionary(d => { d.Add("a", "a"); });
            CollectionAssert.AreEquivalent(expected, dict);
        }
    }
}
