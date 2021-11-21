using JDict;

namespace DidacticalEnigma.Models
{
    public class ReplRootModule
    {
        public ReplRootModule(
            JMDictLookup jmdict,
            JMNedictLookup jmnedict,
            ReplCorpora corpora)
        {
            this.Jmdict = jmdict;
            this.Jmnedict = jmnedict;
            this.Corpora = corpora;
        }

        public JMDictLookup Jmdict { get; }
        public JMNedictLookup Jmnedict { get; }
        public ReplCorpora Corpora { get; }
    }

    public class ReplCorpora
    {
        public ReplCorpora(
            BasicExpressionsCorpus bec,
            Tanaka tanaka,
            JESC jesc)
        {
            Bec = bec;
            Tanaka = tanaka;
            Jesc = jesc;
        }

        public BasicExpressionsCorpus Bec { get; }
        public Tanaka Tanaka { get; }
        public JESC Jesc { get; }
    }
}
