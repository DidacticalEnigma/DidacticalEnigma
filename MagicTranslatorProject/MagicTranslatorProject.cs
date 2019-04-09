using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.Project;
using JetBrains.Annotations;
using MagicTranslatorProject.Json;
using Newtonsoft.Json;
using Optional;
using Utility.Utils;

namespace MagicTranslatorProject
{
    public class MagicTranslatorProject : IProject
    {
        private class MagicTranslatorProjectRegistration : ProjectFormatHandlerRegistration
        {
            public MagicTranslatorProjectRegistration() :
                base("Magic Translator", "*", true)
            {
            }

            public override bool TryOpen([NotNull] string path, out IProject project, out string failureReason)
            {
                try
                {
                    var metadata = Validate(path);
                    var p = new MagicTranslatorProject();
                    p.Init(metadata, path);
                    project = p;
                    failureReason = "";
                    return true;
                }
                catch (FileNotFoundException)
                {
                    failureReason = "This is not a valid Magic Translator project: metadata.json file is missing.";
                }
                catch (JsonSerializationException)
                {
                    failureReason = "This is not a valid Magic Translator project: the metadata.json is not valid.";
                }
                catch (InvalidDataException ex)
                {
                    failureReason = $"This is not a valid Magic Translator project: ${ex.Message}";
                }
                project = null;
                return false;
            }
        }

        public static ProjectFormatHandlerRegistration Registration { get; } = new MagicTranslatorProjectRegistration();

        public void Dispose()
        {

        }

        ITranslationContext IProject.Root => Root;

        public MangaContext Root { get; private set; }

        public void Refresh(bool fullRefresh = false)
        {
            if (fullRefresh)
            {
                foreach (var volume in Root.Children)
                {
                    foreach (var chapter in volume.Children)
                    {
                        foreach (var page in chapter.Children)
                        {
                            foreach (var capture in page.Children)
                            {
                                OnTranslationChanged(new TranslationChangedEventArgs(capture, capture.Translation, TranslationChangedReason.Unknown));
                            }
                        }
                    }
                }
            }
        }

        public event EventHandler<TranslationChangedEventArgs> TranslationChanged;

        private static MetadataJson Validate([NotNull] string path)
        {
            var text = File.ReadAllText(Path.Combine(path, "metadata.json"));
            var metadata = JsonConvert.DeserializeObject<MetadataJson>(text);
            if (!HasContinuousDigitPlaceholders(metadata.Structure.Volume))
            {
                throw new InvalidDataException("Volume path cannot have separated digits");
            }

            if (!HasContinuousDigitPlaceholders(metadata.Structure.Chapter))
            {
                throw new InvalidDataException("Chapter path cannot have separated digits");
            }

            if (!HasContinuousDigitPlaceholders(metadata.Structure.Page))
            {
                throw new InvalidDataException("Page path cannot have separated digits");
            }

            var matcher = new Regex(@"^.*?\{volume}.*?\{chapter}.*?\{page}.*?$");
            foreach (var p in new[]{ metadata.Structure.Raw, metadata.Structure.Translated, metadata.Structure.Save, metadata.Structure.Capture })
            {
                if(!matcher.IsMatch(p))
                    throw new InvalidDataException("The {volume} placeholder must be before {chapter} placeholder, which must be before {page} placeholder");
            }

            return metadata;
        }

        private static bool HasContinuousDigitPlaceholders([NotNull] string input)
        {
            return true;
        }

        private MagicTranslatorProject()
        {

        }

        private void Init([NotNull] MetadataJson json, [NotNull] string path)
        {
            Root = new MangaContext(json, path, new ProjectDirectoryListingProvider(json, path));
        }

        public MagicTranslatorProject([NotNull] string path)
        {
            var json = Validate(path);
            Init(json, path);
        }

        protected virtual void OnTranslationChanged([NotNull] TranslationChangedEventArgs e)
        {
            TranslationChanged?.Invoke(this, e);
        }
    }

    internal class ProjectDirectoryListingProvider
    {
        private MetadataJson metadata;

        public string GetCapturePath([NotNull] PageId page)
        {
            return FormatPath(
                Path.Combine(rootPath, metadata.Structure.Capture),
                page) + ".json";
        }

        public string GetRawPath([NotNull] PageId page)
        {
            return FormatPath(
                Path.Combine(rootPath, metadata.Structure.Raw),
                page);
        }

        private IEnumerable<int> Enumerate(int volume, int chapter, [NotNull] string group)
        {
            var components = ("/" + metadata.Structure.Raw)
                .Split('/', '\\');
            var rest = components.Zip(components.Skip(1), (current, next) => (current, next))
                .TakeWhile(c => !c.current.Contains("{" + group + "}"))
                .Select(c => c.next)
                .ToList();
            var matcher = new Regex(Regex.Escape(rest[rest.Count - 1])
                .Replace("\\{volume}", GetRegexComponent(metadata.Structure.Volume, "volume"))
                .Replace("\\{chapter}", GetRegexComponent(metadata.Structure.Chapter, "chapter"))
                .Replace("\\{page}", GetRegexComponent(metadata.Structure.Page, "page")));

            var rootVolumePath = Path.Combine(new[] { rootPath }
                .Concat(rest.Take(rest.Count - 1)
                    .Select(c => c.Replace("{volume}", FillPlaceholder(metadata.Structure.Volume, volume)))
                    .Select(c => c.Replace("{chapter}", FillPlaceholder(metadata.Structure.Chapter, chapter))))
                .ToArray());

            return new DirectoryInfo(rootVolumePath)
                .EnumerateFileSystemInfos()
                .Select(f => matcher.Match(f.Name))
                .Where(m => m.Success)
                .Select(m => int.Parse(m.Groups[group].Value))
                .Distinct()
                .OrderBy(x => x);
        }

        public IEnumerable<VolumeId> EnumerateVolumes()
        {
            return Enumerate(0, 0, "volume")
                .Select(x => new VolumeId(x));
        }

        public IEnumerable<ChapterId> EnumerateChapters([NotNull] VolumeId volume)
        {
            return Enumerate(volume.VolumeNumber, 0, "chapter")
                .Select(x => new ChapterId(volume, x));
        }

        public IEnumerable<PageId> EnumeratePages([NotNull] ChapterId chapter)
        {
            return Enumerate(chapter.Volume.VolumeNumber, chapter.ChapterNumber, "page")
                .Select(x => new PageId(chapter, x));
        }

        private string FormatPath([NotNull] string path, [NotNull] PageId pageId)
        {
            path = path
                .Replace("{volume}", FillPlaceholder(metadata.Structure.Volume, pageId.Chapter.Volume.VolumeNumber))
                .Replace("{chapter}", FillPlaceholder(metadata.Structure.Chapter, pageId.Chapter.ChapterNumber))
                .Replace("{page}", FillPlaceholder(metadata.Structure.Page, pageId.PageNumber));
            return path;
        }

        private static readonly Regex numberPlaceholder = new Regex("(#+)");

        private readonly string rootPath;

        private int LengthOfNumberPlaceholder([NotNull] string format)
        {
            return numberPlaceholder.Match(format).Groups[1].Value.Length;
        }

        private string GetRegexComponent([NotNull] string format, [NotNull] string groupName)
        {
            var length = LengthOfNumberPlaceholder(format);
            return numberPlaceholder.Replace(format, $"(?<{groupName}>[0-9]{{{length}}})");
        }

        private string FillPlaceholder([NotNull] string format, int value)
        {
            return numberPlaceholder.Replace(format, value.ToString().PadLeft(LengthOfNumberPlaceholder(format), '0'));
        }

        public ProjectDirectoryListingProvider([NotNull] MetadataJson metadata, [NotNull] string rootPath)
        {
            this.metadata = metadata;
            this.rootPath = rootPath;
        }

        public string GetCharactersPath()
        {
            return Path.Combine(rootPath, metadata.Structure.Characters ?? "character", "characters.json");
        }
    }

    internal class VolumeId : IEquatable<VolumeId>
    {
        public bool Equals(VolumeId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return VolumeNumber == other.VolumeNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VolumeId) obj);
        }

        public override int GetHashCode()
        {
            return VolumeNumber;
        }

        public static bool operator ==(VolumeId left, VolumeId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VolumeId left, VolumeId right)
        {
            return !Equals(left, right);
        }

        public int VolumeNumber { get; }

        public VolumeId(int volumeNumber)
        {
            VolumeNumber = volumeNumber;
        }
    }

    internal class ChapterId
    {
        [NotNull] public VolumeId Volume { get; }

        public int ChapterNumber { get; }

        public ChapterId([NotNull] VolumeId volume, int chapterNumber)
        {
            Volume = volume ?? throw new ArgumentNullException(nameof(volume));
            ChapterNumber = chapterNumber;
        }
    }

    internal class PageId : IEquatable<PageId>
    {
        public bool Equals(PageId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Chapter.Equals(other.Chapter) && PageNumber == other.PageNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PageId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Chapter.GetHashCode() * 397) ^ PageNumber;
            }
        }

        public static bool operator ==(PageId left, PageId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PageId left, PageId right)
        {
            return !Equals(left, right);
        }

        [NotNull] public ChapterId Chapter { get; }

        public int PageNumber { get; }

        public PageId([NotNull] ChapterId chapter, int pageNumber)
        {
            Chapter = chapter ?? throw new ArgumentNullException(nameof(chapter));
            PageNumber = pageNumber;
        }
    }

    internal class CaptureId
    {
        [NotNull] public PageId Page { get; }

        public int CaptureNumber { get; }

        public CaptureId([NotNull] PageId page, int captureNumber)
        {
            Page = page ?? throw new ArgumentNullException(nameof(page));
            CaptureNumber = captureNumber;
        }
    }
}