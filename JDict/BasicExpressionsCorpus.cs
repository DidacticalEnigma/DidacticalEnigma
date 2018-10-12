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
        private static Sentence SentenceFromLine(string line)
        {
            var components = line.Split('\t');
            var japaneseSentence = components[1].Trim();
            var englishSentence = components[2].Trim();
            return new Sentence(japaneseSentence, englishSentence);
        }

        private static IEnumerable<Sentence> Sentences(Func<TextReader> readerFactory)
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

        private static IAsyncEnumerable<Sentence> SentencesAsync(Func<TextReader> readerFactory)
        {
            return new AsyncEnumerable<Sentence>(async yield =>
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

        public IEnumerable<Sentence> AllSentences()
        {
            return Sentences(Reader);
        }

        public IAsyncEnumerable<Sentence> AllSentencesAsync()
        {
            return SentencesAsync(Reader);
        }

        public IEnumerable<Sentence> SearchByJapaneseText(string text)
        {
            return Sentences(Reader).Where(x => x.JapaneseSentence.Contains(text));
        }

        public IAsyncEnumerable<Sentence> SearchByJapaneseTextAsync(string text)
        {
            return SentencesAsync(Reader).Where(x => x.JapaneseSentence.Contains(text));
        }
    }
}
