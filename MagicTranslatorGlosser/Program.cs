using System;
using System.IO;
using System.Linq;
using DidacticalEnigma.CLI.Common;

namespace MagicTranslatorGlosser
{
    class Program
    {
        static int Main(string[] args)
        {
            return EntryPoint.Main(new []{ Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Data"), "autoglosser" }.Concat(args).ToArray());
        }
    }
}
