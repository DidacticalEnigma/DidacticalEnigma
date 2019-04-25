using System;
using System.Xml.Serialization;

namespace JDict.Xml
{
    [XmlRoot("kanjidic2")]
    public class KanjiDictRoot
    {
        [XmlElement("header")]
        public KanjiDictHeader Header { get; set; }

        [XmlElement("character")]
        public KanjiCharacter[] Characters { get; set; } = Array.Empty<KanjiCharacter>();
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

        [XmlElement("codepoint")]
        public KanjiCodePoints CodePoints { get; set; }

        [XmlElement("radical")]
        public KanjiRadical Radical { get; set; }

        [XmlElement("misc")]
        public KanjiMisc Misc { get; set; }

        [XmlElement("dic_number")]
        public KanjiDictionaryNumbers DictionaryReferences { get; set; }

        [XmlElement("query_code")]
        public KanjiQueryCodes QueryCodes { get; set; }

        [XmlElement("reading_meaning")]
        public KanjiReadingMeaning ReadingsAndMeanings { get; set; }
    }

    public class KanjiDictionaryNumbers
    {
        [XmlElement("dic_ref")]
        public KanjiDictionaryReference[] References { get; set; } = Array.Empty<KanjiDictionaryReference>();
    }

    public class KanjiDictionaryReference
    {
        [XmlAttribute("dr_type")]
        public string Type { get; set; }

        [XmlAttribute("m_vol")]
        public int Volume { get; set; }

        [XmlAttribute("m_page")]
        public int Page { get; set; }

        [XmlText]
        public string Info { get; set; }
    }

    public class KanjiCodePoints
    {
        [XmlElement("cp_value")]
        public KanjiCodePoint[] CodePoints { get; set; } = Array.Empty<KanjiCodePoint>();
    }

    public class KanjiCodePoint
    {
        [XmlAttribute("cp_type")]
        public string Type { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class KanjiQueryCodes
    {
        [XmlElement("q_code")]
        public KanjiQueryCode[] Codes { get; set; } = Array.Empty<KanjiQueryCode>();
    }

    public class KanjiQueryCode
    {
        [XmlAttribute("qc_type")]
        public string Type { get; set; }

        [XmlAttribute("skip_misclass")]
        public string MisclassificationType { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class KanjiReadingMeaning
    {
        [XmlElement("rmgroup")]
        public KanjiRmGroup[] Groups { get; set; } = Array.Empty<KanjiRmGroup>();

        [XmlElement("nanori")]
        public KanjiNanori[] Nanori { get; set; } = Array.Empty<KanjiNanori>();
    }

    public class KanjiRmGroup
    {
        [XmlElement("reading")]
        public KanjiReading[] Readings { get; set; } = Array.Empty<KanjiReading>();

        [XmlElement("meaning")]
        public KanjiMeaning[] Meanings { get; set; } = Array.Empty<KanjiMeaning>();
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
        public int[] Value { get; set; } = Array.Empty<int>();

        [XmlAttribute("rad_type")]
        public string Type { get; set; }
    }

    public class KanjiMisc
    {
        [XmlElement("grade")]
        public string Grade { get; set; }

        /// "If more than one, the first is considered the accepted count, while subsequent ones are common miscounts."
        [XmlElement("stroke_count")]
        public int[] StrokeCount { get; set; } = Array.Empty<int>();

        // Variant
        [XmlElement("variant")]
        public KanjiVariant[] Variants { get; set; } = Array.Empty<KanjiVariant>();

        [XmlElement("freq")]
        public int FrequencyRating { get; set; }

        [XmlElement("rad_name")]
        public string[] RadicalName { get; set; } = Array.Empty<string>();

        [XmlElement("jlpt")]
        public int JlptLevel { get; set; }
    }

    public class KanjiVariant
    {
        [XmlAttribute("var_type")]
        public string Type { get; set; }

        [XmlText]
        public string Data { get; set; }
    }
}
