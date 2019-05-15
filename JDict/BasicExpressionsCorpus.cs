using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace JDict
{
    public class BasicExpressionsCorpus
    {
        private static SentencePair SentenceFromLine(string line)
        {
            var components = line.Split('\t');
            var japaneseSentence = components[1].Trim();
            var englishSentence = components[2].Trim();
            return new SentencePair(japaneseSentence, englishSentence);
        }

        private static IEnumerable<SentencePair> Sentences(Func<TextReader> readerFactory)
        {
            using (var reader = readerFactory())
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return SentenceFromLine(line);
                }
            }
        }

        private static IAsyncEnumerable<SentencePair> SentencesAsync(Func<TextReader> readerFactory)
        {
            return new AsyncEnumerable<SentencePair>(async yield =>
            {
                using (var reader = readerFactory())
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        await yield.ReturnAsync(SentenceFromLine(line));
                    }
                }
            });
        }

        private readonly Encoding encoding;

        private readonly string path;

        public BasicExpressionsCorpus(string path, Encoding encoding)
        {
            this.encoding = encoding;
            this.path = path;
        }

        private TextReader Reader()
        {
            return new StreamReader(
                path,
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
