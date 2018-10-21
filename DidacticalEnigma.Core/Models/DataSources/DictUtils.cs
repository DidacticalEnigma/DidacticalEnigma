using System;
using System.Collections.Generic;
using JDict;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public static class DictUtils
    {
        public static (IEnumerable<T> entry, string word) GreedyLookup<T>(Func<string, IEnumerable<T>> lookup, IEnumerable<string> request, int backOffCountStart = 5)
        {
            int backOffCount = backOffCountStart;
            IEnumerable<T> found = null;
            string foundWord = null;
            string concatenatedWord = "";
            foreach(var word in request)
            {
                concatenatedWord += word;
                var newEntry = lookup(concatenatedWord);
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