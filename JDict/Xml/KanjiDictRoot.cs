
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

        // dic_number?, query_code?

        [XmlElement("reading_meaning")]
        public KanjiReadingMeaning ReadingsAndMeanings { get; set; }
    }

    public class KanjiReadingMeaning
    {
        [XmlElement("rmgroup")]
        public KanjiRmGroup[] Groups { get; set; }

        [XmlElement("nanori")]
        public KanjiNanori[] Nanori { get; set; }
    }

    public class KanjiRmGroup
    {
        [XmlElement("reading")]
        public KanjiReading[] Readings { get; set; }

        [XmlElement("meaning")]
        public KanjiMeaning[] Meanings { get; set; }
    }

    public class KanjiReading
    {
        [XmlAttribute("r_type")]
        public string ReadingType { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class KanjiMeaning
    {
        [XmlAttribute("m_lang")]
        public string Language { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class KanjiNanori
    {
        [XmlText]
        public string Value { get; set; }
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
