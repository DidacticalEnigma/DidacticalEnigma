using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace JDict
{
    public class Tanaka
    {
        private static Sentence SentenceFromLine(string line)
        {
            var components = line.Split('\t');
            var japaneseSentence = components[0].Remove(0, 3);
            var englishSentence = components[1].Remove(components[1].IndexOf("#ID", StringComparison.InvariantCulture));
            return new Sentence(japaneseSentence, englishSentence);
        }

        private static IEnumerable<Sentence> Sentences(Func<TextReader> readerFactory)
        {
            using (var reader = readerFactory())
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("B:"))
                        continue;

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
                        if (line.StartsWith("B:"))
                            continue;

                        await yield.ReturnAsync(SentenceFromLine(line));
                    }
                }
            });
        }

        private readonly Encoding encoding;

        private readonly string path;
        
        public Tanaka(string path, Encoding encoding)
        {
            this.encoding = encoding;
            this.path = path;
        }

        private TextReader Reader()
        {
            return new StreamReader(
                new GZipStream(File.OpenRead(path), CompressionMode.Decompress),
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
