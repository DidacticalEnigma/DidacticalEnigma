using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JDict
{
    public class JESC
    {
        private IEnumerable<Sentence> Sentences(Func<string, TextReader> readerFactory)
        {
            using (var readerJp = readerFactory(pathJp))
            using (var readerEn = readerFactory(pathEn))
            {
                while (true)
                {
                    string lineJp;
                    if ((lineJp = readerJp.ReadLine()) == null)
                        break;
                    string lineEn;
                    if ((lineEn = readerEn.ReadLine()) == null)
                        break;
                    yield return new Sentence(lineJp, lineEn);
                }
            }
        }

        private IAsyncEnumerable<Sentence> SentencesAsync(Func<string, TextReader> readerFactory)
        {
            return new AsyncEnumerable<Sentence>(async yield =>
            {
                using (var readerJp = readerFactory(pathJp))
                using (var readerEn = readerFactory(pathEn))
                {
                    while (true)
                    {
                        string lineJp;
                        if ((lineJp = await readerJp.ReadLineAsync()) == null)
                            break;
                        string lineEn;
                        if ((lineEn = await readerEn.ReadLineAsync()) == null)
                            break;
                        await yield.ReturnAsync(new Sentence(lineJp, lineEn));
                    }
                }
            });
        }

        private readonly Encoding encoding;

        private readonly string pathJp;

        private readonly string pathEn;

        public JESC(string path, Encoding encoding)
        {
            this.encoding = encoding;
            this.pathJp = Path.Combine(path, "ja");
            this.pathEn = Path.Combine(path, "en");
        }

        public IEnumerable<Sentence> AllSentences()
        {
            return Sentences(path => new StreamReader(path, encoding));
        }

        public IAsyncEnumerable<Sentence> AllSentencesAsync()
        {
            return SentencesAsync(path => new StreamReader(path, encoding));
        }

        public IEnumerable<Sentence> SearchByJapaneseText(string text)
        {
            return Sentences(path => new StreamReader(path, encoding)).Where(x => x.JapaneseSentence.Contains(text));
        }

        public IAsyncEnumerable<Sentence> SearchByJapaneseTextAsync(string text)
        {
            return SentencesAsync(path => new StreamReader(path, encoding)).Where(x => x.JapaneseSentence.Contains(text));
        }
    }
}
