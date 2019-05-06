using System.Linq;
using AutomatedTests;
using DidacticalEnigma.Core.Models;
using NUnit.Framework;

[TestFixture]
public class SentenceParserTests
{


    [Test]
    public void Test()
    {
        var parser = PartialWordLookupTests.Configure(TestDataPaths.BaseDir).Get<SentenceParser>();
        Assert.IsNotNull(parser.BreakIntoSentences("試着").SelectMany(x => x).First().Reading);
    }
}