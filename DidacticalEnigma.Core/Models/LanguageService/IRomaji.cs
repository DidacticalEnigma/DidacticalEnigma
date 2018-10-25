using DidacticalEnigma.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IRomaji
    {
        string ToRomaji(string input);
    }

    public class ModifiedHepburn : IRomaji
    {
        private readonly IMorphologicalAnalyzer<IEntry> mecab;
        private readonly IKanaProperties props;

        public string ToRomaji(string input)
        {
            var words = mecab.ParseToEntries(input)
                .Where(entry => entry.IsRegular)
                .Select(entry => entry.Pronunciation);

            bool first = true;
            var sb = new StringBuilder();
            foreach (var word in words)
            {
                if (!first)
                    sb.Append(" ");
                first = false;
                for (int i = 0; i < word.Length; ++i)
                {
                    var c = word[i];
                    var n = i+1 < word.Length ? word[i+1] : '\0';

                    if (c == 'ー' && i > 0)
                    {
                        var lastLetter = sb[sb.Length - 1];
                        if (lastLetter == 'o')
                            lastLetter = 'u';
                        sb.Append(lastLetter);
                        continue;
                    }

                    if ((c == 'ん' || c == 'ン')
                        && "あいうえおアイウエオ".Contains(n.ToString()))
                    {
                        sb.Append(props.LookupRomaji(c.ToString()) ?? c.ToString());
                        sb.Append('\'');
                        sb.Append(props.LookupRomaji(n.ToString()) ?? n.ToString());
                        ++i;
                        continue;
                    }

                    var romaji = props.LookupRomaji(new string(new[] {c, n}));
                    if (romaji != null)
                    {
                        sb.Append(romaji);
                        i++;
                    }
                    else
                    {
                        sb.Append(props.LookupRomaji(c.ToString()) ?? c.ToString());
                    }
                }
            }

            return sb.ToString();
        }

        public ModifiedHepburn(IMorphologicalAnalyzer<IEntry> mecab, IKanaProperties props)
        {
            this.mecab = mecab;
            this.props = props;
        }
    }
}
