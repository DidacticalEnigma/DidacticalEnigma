using System.Unicode;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class Kanji : CodePoint
    {
        internal Kanji(int s) :
            base(s)
        {
            
        }

        public override string ToDescriptionString()
        {
            var info = UnicodeInfo.GetCharInfo(Utf32);
            return ToLongString() + "\n" +
                   "Kun: " + info.JapaneseKunReading + "\n" +
                   "On: " + info.JapaneseOnReading + "\n" +
                   info.Definition + "\n";
        }
    }
}