using System.Collections.Generic;
using JDict;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public static class JMDictUtils
    {
        public static (IEnumerable<JMDictEntry> entry, string word) GreedyLookup(this JMDict jmDict, IEnumerable<string> request, int backOffCountStart = 5)
        {
            int backOffCount = backOffCountStart;
            IEnumerable<JMDictEntry> found = null;
            string foundWord = null;
            string concatenatedWord = "";
            foreach(var word in request)
            {
                concatenatedWord += word;
                var newEntry = jmDict.Lookup(concatenatedWord);
                if(newEntry == null)
                {
                    backOffCount--;
                    if(backOffCount == 0)
                        break;
                }
                else
                {
                    found = newEntry;
                    foundWord = concatenatedWord;
                    backOffCount = backOffCountStart;
                }
            }

            return (found, foundWord);
        }
    }
}