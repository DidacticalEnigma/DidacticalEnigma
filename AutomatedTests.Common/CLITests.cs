using System;
using System.IO;
using DidacticalEnigma.CLI.Common;
using NUnit.Framework;

namespace AutomatedTests
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
                var exitCode = EntryPoint.Main(new[] { TestDataPaths.BaseDir, "autoglosser", "セーラー服" });
                Assert.AreEqual(0, exitCode);
                Assert.AreEqual("[{\"word\":\"セーラー服\",\"definitions\":[\"sailor suit/middy uniform\"]}]", textWriter.ToString().Trim());
            }
            finally
            {
                Console.SetOut(previous);
            }
        }
    }
}
