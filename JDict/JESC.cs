using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace JDict
{
    public class JESC
    {
        private IEnumerable<SentencePair> Sentences(Func<string, TextReader> readerFactory)
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
                    yield return new SentencePair(lineJp, lineEn);
                }
            }
        }

        private IAsyncEnumerable<SentencePair> SentencesAsync(Func<string, TextReader> readerFactory)
        {
            return new AsyncEnumerable<SentencePair>(async yield =>
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
                        await yield.ReturnAsync(new SentencePair(lineJp, lineEn));
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
            this.pathJp = Path.Combine(path, "ja.gz");
            this.pathEn = Path.Combine(path, "en.gz");
        }

        private TextReader Reader(string path)
        {
            return new StreamReader(
                new GZipStream(File.OpenRead(path), CompressionMode.Decompress),
                encoding);
        }

        public IEnumerable<SentencePair> AllSentences()
        {
            return Sentences(Reader);
        }

        public IAsyncEnumerable<SentencePair> AllSentencesAsync()
        {
            return SentencesAsync(Reader);
        }

        public IEnumerable<SentencePair> SearchByJapaneseText(string text)
        {
            return Sentences(Reader).Where(x => x.JapaneseSentence.Contains(text));
        }

        public IAsyncEnumerable<SentencePair> SearchByJapaneseTextAsync(string text)
        {
            return SentencesAsync(Reader).Where(x => x.JapaneseSentence.Contains(text));
        }
    }
}
