using System.Xml.Serialization;

namespace JDict.Xml
{
    [XmlRoot("JMnedict")]
    public class JMNedictRoot
    {
        [XmlElement("entry")]
        public NeEntry[] Entries { get; set; }
    }

    [XmlRoot("entry")]
    public class NeEntry
    {
        [XmlElement("ent_seq")]
        public long SequenceNumber { get; set; }

        [XmlElement("k_ele")]
        public KanjiElement[] KanjiElements { get; set; }

        [XmlElement("r_ele")]
        public ReadingElement[] ReadingElements { get; set; }

        [XmlElement("trans")]
        public NeTranslationalEquivalent[] TranslationalEquivalents { get; set; }
    }

    public class NeTranslationalEquivalent
    {
        [XmlElement("name_type")]
        public string[] Types { get; set; }

        [XmlElement("xref")]
        public string[] CrossReferences { get; set; }

        [XmlElement("trans_det")]
        public NeTranslation[] Translation { get; set; }
    }

    public class NeTranslation
    {
        [XmlAttribute("xml:lang")]
        public string Lang { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}