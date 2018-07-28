using System;
using System.Xml.Serialization;

// I have to make these classes public because otherwise XML serialization complains
// But they aren't really for public use
namespace JDict.Internal.XmlModels
{
    
    [XmlRoot("JMdict")]
    public class JdicRoot
    {
        [XmlElement("entry")]
        public JdicEntry[] Entries { get; set; }
    }

    public class JdicEntry
    {
        [XmlElement("ent_seq")]
        public long Number { get; set; }

        [XmlElement("k_ele")]
        public KanjiElement[] KanjiElements { get; set; }

        [XmlElement("r_ele")]
        public ReadingElement[] ReadingElements { get; set; }

        [XmlElement("sense")]
        public Sense[] Senses { get; set; }
    }

    public class Sense
    {
        [XmlElement("stagk")]
        public string[] Stagk { get; set; }

        [XmlElement("stagr")]
        public string[] Stagkr { get; set; }

        [XmlElement("pos")]
        public string[] PartOfSpeech { get; set; }

        [XmlElement("xref")]
        public string[] CrossRef { get; set; }

        [XmlElement("ant")]
        public string[] Antonym { get; set; }

        [XmlElement("field")]
        public string[] Field { get; set; }

        [XmlElement("misc")]
        public string[] Misc { get; set; }

        [XmlElement("s_inf")]
        public string[] Information { get; set; }

        [XmlElement("lsource")]
        public string[] LoanWordSource { get; set; }

        [XmlElement("dial")]
        public string[] Dialect { get; set; }

        [XmlElement("gloss")]
        public Gloss[] Glosses { get; set; }

    }

    public class LoanSource
    {
        [XmlAttribute("xml:lang")]
        public string Lang { get; set; }

        [XmlAttribute("ls_type")]
        public string LoanType { get; set; }

        [XmlAttribute("ls_wasei")]
        public string Wasei { get; set; }
    }

    public class Gloss
    {
        [XmlAttribute("xml:lang")]
        public string Lang { get; set; }

        [XmlAttribute("g_gend")]
        public string Gender { get; set; }

        [XmlAttribute("g_type")]
        public string Type { get; set; }
    }

    public class ReadingElement
    {
        [XmlElement("reb")]
        public string Reb { get; set; }

        [XmlElement("re_nokanji")]
        public string NoKanji { get; set; }

        [XmlElement("re_restr")]
        public string[] Restr { get; set; }

        [XmlElement("re_inf")]
        public string[] Inf { get; set; }

        [XmlElement("re_pri")]
        public string[] Pri { get; set; }
    }

    public class KanjiElement
    {
        [XmlElement("keb")]
        public string Key { get; set; }

        [XmlElement("ke_inf")]
        public string[] Inf { get; set; }

        [XmlElement("ke_pri")]
        public string[] Pri { get; set; }
    }   
}
