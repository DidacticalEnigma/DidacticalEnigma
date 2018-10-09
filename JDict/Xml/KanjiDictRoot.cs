
using System;
using System.Xml.Serialization;

namespace JDict.Internal.XmlModels
{
    [XmlRoot("kanjidic2")]
    public class KanjiDictRoot
    {
        [XmlElement("header")]
        public KanjiDictHeader Header { get; set; }

        [XmlElement("character")]
        public KanjiCharacter[] Characters { get; set; }
    }

    public class KanjiDictHeader
    {
        [XmlElement("file_version")]
        public string FileVersion { get; set; }

        [XmlElement("database_version")]
        public string DatabaseVersion { get; set; }

        [XmlElement("date_of_creation")]
        public DateTime DateOfCreation { get; set; }
    }

    public class KanjiCharacter
    {
        [XmlElement("literal")]
        public string Literal { get; set; }

        // CodePoint
        // 

        [XmlElement("radical")]
        public KanjiRadical Radical { get; set; }

        [XmlElement("misc")]
        public KanjiMisc Misc { get; set; }

        // dic_number?, query_code?, reading_meaning
    }

    public class KanjiRadical
    {
        [XmlElement("rad_value")]
        public int[] Value { get; set; }

        [XmlAttribute("rad_type")]
        public string Type { get; set; }
    }

    public class KanjiMisc
    {
        [XmlElement("grade")]
        public string Grade { get; set; }

        /// "If more than one, the first is considered the accepted count, while subsequent ones are common miscounts."
        [XmlElement("stroke_count")]
        public int[] StrokeCount { get; set; }

        // Variant
        //

        [XmlElement("freq")]
        public int FrequencyRating { get; set; }

        [XmlElement("rad_name")]
        public string[] RadicalName { get; set; }

        [XmlElement("jlpt")]
        public int JlptLevel { get; set; }
    }
}
