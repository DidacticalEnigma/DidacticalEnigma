using System;
using System.Xml.Serialization;

namespace JDict.Xml
{
    
    [XmlRoot("JMdict")]
    public class JdicRoot
    {
        [XmlElement("entry")]
        public JdicEntry[] Entries { get; set; }
    }

    [XmlRoot("entry")]
    public class JdicEntry
    {
        [XmlElement("ent_seq")]
        public long Number { get; set; }

        [XmlElement("k_ele")]
        public KanjiElement[] KanjiElements { get; set; } = Array.Empty<KanjiElement>();

        [XmlElement("r_ele")]
        public ReadingElement[] ReadingElements { get; set; } = Array.Empty<ReadingElement>();

        [XmlElement("sense")]
        public Sense[] Senses { get; set; } = Array.Empty<Sense>();
    }

    public class Sense
    {
        [XmlElement("stagk")]
        public string[] Stagk { get; set; } = Array.Empty<string>();

        [XmlElement("stagr")]
        public string[] Stagkr { get; set; } = Array.Empty<string>();

        [XmlElement("pos")]
        public string[] PartOfSpeech { get; set; } = Array.Empty<string>();

        [XmlElement("xref")]
        public string[] CrossRef { get; set; } = Array.Empty<string>();

        [XmlElement("ant")]
        public string[] Antonym { get; set; } = Array.Empty<string>();

        [XmlElement("field")]
        public string[] Field { get; set; } = Array.Empty<string>();

        [XmlElement("misc")]
        public string[] Misc { get; set; } = Array.Empty<string>();

        [XmlElement("s_inf")]
        public string[] Information { get; set; } = Array.Empty<string>();

        [XmlElement("lsource")]
        public LoanSource[] LoanWordSource { get; set; } = Array.Empty<LoanSource>();

        [XmlElement("dial")]
        public string[] Dialect { get; set; } = Array.Empty<string>();

        [XmlElement("gloss")]
        public Gloss[] Glosses { get; set; } = Array.Empty<Gloss>();

    }

    public class LoanSource
    {
        [XmlAttribute("xml:lang")]
        public string Lang { get; set; }

        [XmlAttribute("ls_type")]
        public string LoanType { get; set; }

        [XmlAttribute("ls_wasei")]
        public string Wasei { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    public class Gloss
    {
        [XmlAttribute("xml:lang")]
        public string Lang { get; set; }

        [XmlAttribute("g_gend")]
        public string Gender { get; set; }

        [XmlAttribute("g_type")]
        public string Type { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    public class ReadingElement
    {
        [XmlElement("reb")]
        public string Reb { get; set; }

        [XmlElement("re_nokanji")]
        public string NoKanji { get; set; }

        [XmlElement("re_restr")]
        public string[] Restr { get; set; } = Array.Empty<string>();

        [XmlElement("re_inf")]
        public string[] Inf { get; set; } = Array.Empty<string>();

        [XmlElement("re_pri")]
        public string[] Pri { get; set; } = Array.Empty<string>();
    }

    public class KanjiElement
    {
        [XmlElement("keb")]
        public string Key { get; set; }

        [XmlElement("ke_inf")]
        public string[] Inf { get; set; } = Array.Empty<string>();

        [XmlElement("ke_pri")]
        public string[] Pri { get; set; } = Array.Empty<string>();
    }   
}
