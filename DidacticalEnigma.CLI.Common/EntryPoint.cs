using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Models.LanguageService;
using Gu.Inject;
using JDict;
using Newtonsoft.Json;
using NMeCab;
using Optional.Collections;

namespace DidacticalEnigma.CLI.Common
{
    public class EntryPoint
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                DisplayHelp();
            }
            switch (args[1])
            {
                case "help":
                case "-h":
                case "--help":
                default:
                    DisplayHelp();
                    break;
                case "autoglosser":
                    RunAutomaticGlossing(args[0], args[2]);
                    break;
            }
            return 0;
        }

        private static void RunAutomaticGlossing(string dataDir, string input)
        {
            var kana = new KanaProperties2(Path.Combine(dataDir, "character", "kana.txt"), Encoding.UTF8);
            using (var mecab = new MeCabIpadic(new MeCabParam { DicDir = Path.Combine(dataDir, "mecab", "ipadic"), UseMemoryMappedFile = true }))
            using (var dict = JMDictLookup.Create(Path.Combine(dataDir, "dictionaries", "JMdict_e.gz"), Path.Combine(dataDir, "dictionaries", "JMdict_e.cache")))
            {
                var glosser = new AutoGlosserNext(mecab, dict, kana);
                var glosses = glosser.Gloss(input);
                var jsonWriter = new JsonTextWriter(Console.Out);
                jsonWriter.WriteStartArray();
                foreach (var gloss in glosses)
                {
                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName("word");
                    jsonWriter.WriteValue(gloss.Foreign);
                    jsonWriter.WritePropertyName("definitions");
                    jsonWriter.WriteStartArray();
                    foreach (var glossCandidate in gloss.GlossCandidates)
                    {
                        jsonWriter.WriteValue(glossCandidate);
                    }
                    jsonWriter.WriteEndArray();
                    jsonWriter.WriteEndObject();
                }
                jsonWriter.WriteEndArray();
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("\tinit - pre-cache data sources");
            Console.WriteLine("\tautoglosser - provide glosses for japanese input");
        }
    }
}