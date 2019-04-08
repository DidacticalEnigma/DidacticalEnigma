using System;
using JetBrains.Annotations;

namespace DidacticalEnigma.Core.Models.Project
{
    public abstract class ProjectFormatHandlerRegistration
    {
        public abstract bool TryOpen([NotNull] string path, out IProject project, out string failureReason);

        public string FormatName { get; }

        public string ExtensionFilter { get; }

        public bool IsDirectory { get; }

        protected ProjectFormatHandlerRegistration([NotNull] string formatName, [NotNull] string extensionFilter, bool isDirectory)
        {
            FormatName = formatName ?? throw new ArgumentNullException(nameof(formatName));
            ExtensionFilter = extensionFilter ?? throw new ArgumentNullException(nameof(extensionFilter));
            IsDirectory = isDirectory;
        }
    }
}