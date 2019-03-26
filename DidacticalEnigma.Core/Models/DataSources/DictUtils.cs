using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.LanguageService;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.DataSources
{
    static class DictUtils
    {
        public static (IEnumerable<T> entry, string word) GreedyLookup<T>(Func<string, IEnumerable<T>> lookup, IEnumerable<string> request, int backOffCountStart = 5)
        {
            int backOffCount = backOffCountStart;
            IEnumerable<T> found = null;
            string foundWord = null;
            string concatenatedWord = "";
            foreach(var word in request)
            {
                concatenatedWord += word;
                var newEntry = lookup(concatenatedWord);
                if(newEntry == null)
                {
                    backOffCount--;
                    if(backOffCount == 0)
                        break;
                }
                else
                {
                    found = newEntry;
                    foundWord = concatenatedWord;
                    backOffCount = backOffCountStart;
                }
            }

            return (found, foundWord);
        }

        public static Task<Option<RichFormatting>> Lookup<T>(
            Request request,
            Func<string, IEnumerable<T>> lookup,
            Func<Request, (IEnumerable<T> entry, string word)> greedyLookup,
            IKanaProperties kana)
        {
            IEnumerable<TextParagraph> Render(IEnumerable<T> entries)
            {
                var p = new TextParagraph();
                p.Content.Add(new Text(string.Join("\n\n", entries.Select(e => e.ToString()))));
                return EnumerableExt.OfSingle(p);
            }

            return Lookup(
                request,
                lookup,
                greedyLookup,
                kana,
                Render);
        }

        public static Task<Option<RichFormatting>> Lookup<T>(
            Request request,
            Func<string, IEnumerable<T>> lookup,
            Func<Request, (IEnumerable<T> entry, string word)> greedyLookup,
            IKanaProperties kana,
            Func<IEnumerable<T>, IEnumerable<Paragraph>> render)
        {
            var rawWord = request.Word.RawWord.Trim();
            var entry = lookup(rawWord);
            var rich = new RichFormatting();

            var (greedyEntry, greedyWord) = greedyLookup(request);
            if (greedyEntry != null && greedyWord != request.Word.RawWord)
            {
                rich.Paragraphs.Add(new TextParagraph(new[]
                {
                    new Text("The entries below are a result of the greedy lookup: "),
                    new Text(greedyWord, emphasis: true)
                }));
                foreach (var p in render(greedyEntry))
                {
                    rich.Paragraphs.Add(p);
                }
                
            }

            if (entry != null)
            {
                if (greedyEntry != null && greedyWord != request.Word.RawWord)
                {
                    rich.Paragraphs.Add(new TextParagraph(new[]
                    {
                        new Text("The entries below are a result of the regular lookup: "),
                        new Text(request.Word.RawWord, emphasis: true)
                    }));
                }
                foreach (var p in render(entry))
                {
                    rich.Paragraphs.Add(p);
                }
            }

            if (request.NotInflected != null && request.NotInflected != request.Word.RawWord)
            {
                entry = lookup(request.NotInflected);
                if (entry != null)
                {
                    rich.Paragraphs.Add(new TextParagraph(new[]
                    {
                        new Text("The entries below are a result of lookup on the base form: "),
                        new Text(request.NotInflected, emphasis: true)
                    }));
                    foreach (var p in render(entry))
                    {
                        rich.Paragraphs.Add(p);
                    }
                }
            }

            var rawWordHiragana = kana.ToHiragana(rawWord);
            if (rawWordHiragana != rawWord)
            {
                entry = lookup(rawWordHiragana);
                if (entry != null)
                {
                    rich.Paragraphs.Add(new TextParagraph(new[]
                    {
                        new Text("The entries below are a result of forced katakana -> hiragana conversion: "),
                        new Text($"{rawWord} -> {rawWordHiragana}", emphasis: true), 
                    }));
                    foreach (var p in render(entry))
                    {
                        rich.Paragraphs.Add(p);
                    }
                }
            }

            if (rich.Paragraphs.Count == 0)
                return Task.FromResult(Option.None<RichFormatting>());

            return Task.FromResult(Option.Some(rich));
        }
    }
}