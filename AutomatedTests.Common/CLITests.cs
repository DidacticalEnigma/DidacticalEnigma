using System;
using System.IO;
using DidacticalEnigma.CLI.Common;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class CLITests
    {
        [Test]
        public void T()
        {
            var previous = Console.Out;
            try
            {
                var textWriter = new StringWriter();
                Console.SetOut(textWriter);
                var exitCode = EntryPoint.Main(new[] {"autoglosser", TestDataPaths.BaseDir, "セーラー服"});
                Assert.AreEqual(0, exitCode);
                Assert.AreEqual("[{\"word\":\"セーラー服\",\"definitions\":[\"sailor suit/middy uniform\"]}]\r\n", textWriter.ToString());
            }
            finally
            {
                Console.SetOut(previous);
            }
        }
    }
}
