using System;

namespace DidacticalEnigma.Core.Models.Project
{
    public abstract class ProjectFormatHandlerRegistration
    {
        public abstract bool IsValid(string path);

        public abstract IProject Open(string path);

        public string FormatName { get; }

        public string ExtensionFilter { get; }

        public bool IsDirectory { get; }

        protected ProjectFormatHandlerRegistration(string formatName, string extensionFilter, bool isDirectory)
        {
            FormatName = formatName ?? throw new ArgumentNullException(nameof(formatName));
            ExtensionFilter = extensionFilter ?? throw new ArgumentNullException(nameof(extensionFilter));
            IsDirectory = isDirectory;
        }
    }
}