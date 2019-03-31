using System;
using System.Collections.Async;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class CustomNotesDataSource : IDataSource
    {
        private readonly string notesFilePath;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("AF9401B8-958E-4F31-8673-9B64C8A5F2CD"),
            "Custom notes",
            "The information in this source is provided by you, dear user.",
            null);

        public void Dispose()
        {

        }

        public async Task<Option<RichFormatting>> Answer(Request request, CancellationToken token)
        {
            try
            {
                var rich = new RichFormatting();
                var paragraphs = ReadParagraphs();
                if (paragraphs != null)
                {
                    await paragraphs.Where(paragraph => paragraph.Contains(request.QueryText)).ForEachAsync(paragraph =>
                    {
                        var text = new TextParagraph(
                            StringExt.HighlightWords(paragraph, request.QueryText)
                                .Select(p => new Text(p.text, emphasis: p.highlight)));
                        rich.Paragraphs.Add(text);
                    }, token);
                    return Option.Some(rich);
                }
            }
            catch (FileNotFoundException)
            {
                var text = "This data source looks for a custom_notes.txt file in the data directory. Currently no such file exists.";
                var rich = new RichFormatting(
                    EnumerableExt.OfSingle(
                        new TextParagraph(
                            EnumerableExt.OfSingle(
                                new Text(text)))));
                return Option.Some(rich);
            }

            return Option.None<RichFormatting>();
        }

        private IAsyncEnumerable<string> ReadParagraphs()
        {
            return new AsyncEnumerable<string>(async yield =>
            {
                using (var reader = new StreamReader(notesFilePath))
                {
                    var sb = new StringBuilder();
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (line == "" && sb.Length != 0)
                        {
                            await yield.ReturnAsync(sb.ToString());
                            sb.Clear();
                            continue;
                        }

                        sb.AppendLine(line);
                    }

                    if (sb.Length != 0)
                    {
                        await yield.ReturnAsync(sb.ToString());
                    }
                }
            });
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public CustomNotesDataSource(string notesFilePath)
        {
            this.notesFilePath = notesFilePath;
        }
    }
}