using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace JDict.Tests
{
    class DependentOnKenkyuushaAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets { get; private set; }

        public void AfterTest(ITest test)
        {
            // no-op
        }

        public void BeforeTest(ITest test)
        {
            if (!File.Exists(TestDataPaths.Kenkyusha5))
                Assert.Ignore();

            byte[] hash;
            using (var file = File.OpenRead(TestDataPaths.Kenkyusha5))
            using (var sha = new SHA256Managed())
            {
                hash = sha.ComputeHash(file);
            }
            Assert.AreEqual(
                TestDataPaths.Kenkyusha5Hash.ToUpperInvariant(),
                BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant());
        }
    }
}
