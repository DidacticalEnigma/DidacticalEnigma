using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Utils;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class RadicalRemapper
    {
        private DualDictionary<int, int> remappingTable = new DualDictionary<int, int>(new Dictionary<int, int>
        {
            {'化', '⺅'},
            {'刈', '⺉'},
            {'込', '⻌'},
            {'汁', '氵'},
            {'初', '衤'},
            {'尚', '⺌'},
            {'買', '罒'},
            {'犯', '犭'},
            {'忙', '忄'},
            {'礼', '礻'},
            {'个', 131490},
            {'老', '⺹'},
            {'扎', '扌'},
            {'杰', '灬'},
            {'疔', '疒'},
            {'禹', '禸'},
            {'艾', '⺾'},
            //{'邦', '⻏'},
            //{'阡', '⻖'},
            // 并 none available - upside-down ハ
        });

        private string MapTo(string input)
        {
            return StringExt.FromCodePoints(input.AsCodePoints().Select(c => remappingTable.TryGetValue(c, out var value) ? value : c));
        }

        private string MapFrom(string input)
        {
            return StringExt.FromCodePoints(input.AsCodePoints().Select(c => remappingTable.TryGetKey(c, out var value) ? value : c));
        }

        private Kradfile kradfile;

        private Radkfile radkfile;

        public RadicalRemapper(Kradfile kradfile, Radkfile radkfile)
        {
            this.kradfile = kradfile;
            this.radkfile = radkfile;
            Comparer = new EqualityComparer(this);
        }

        private class EqualityComparer : IEqualityComparer<string>
        {
            private readonly RadicalRemapper remapper;

            public bool Equals(string x, string y)
            {
                return remapper.MapTo(x) == remapper.MapTo(y);
            }

            public int GetHashCode(string obj)
            {
                return remapper.MapTo(obj).GetHashCode();
            }

            public EqualityComparer(RadicalRemapper remapper)
            {
                this.remapper = remapper;
            }
        }

        public IEqualityComparer<string> Comparer { get; }

        public IEnumerable<string> LookupKanji(IEnumerable<string> radicals)
        {
            return radkfile.LookupMatching(radicals.Select(MapFrom));
            //.Select(MapTo);
        }

        public Option<IEnumerable<string>> LookupRadicals(string kanji)
        {
            return kradfile.LookupRadicals(MapFrom(kanji)).Map(radicals => radicals.Select(MapTo));
        }

        public IEnumerable<JDict.Radical> Radicals => radkfile.Radicals.Select(r => new JDict.Radical(
            remappingTable.TryGetValue(r.CodePoint, out var value) ? value : r.CodePoint,
            r.StrokeCount));
    }
}
