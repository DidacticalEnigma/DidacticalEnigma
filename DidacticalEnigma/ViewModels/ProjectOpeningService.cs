using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Project;
using Microsoft.WindowsAPICodePack.Dialogs;
using Optional;

namespace DidacticalEnigma.ViewModels
{
    internal interface IProjectOpeningService
    {
        Option<IProject, string> SelectOpen(ProjectFormatHandlerRegistration registration);
    }

    class ProjectOpeningService : IProjectOpeningService
    {
        public Option<IProject, string> SelectOpen(ProjectFormatHandlerRegistration registration)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = registration.IsDirectory;
            dialog.Filters.Add(new CommonFileDialogFilter($"{registration.FormatName} ({registration.ExtensionFilter})", registration.ExtensionFilter));
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                var filename = dialog.FileName;
                if (registration.TryOpen(filename, out var project, out string failureReason))
                {
                    return Option.Some<IProject, string>(project);
                }
                else
                {
                    return Option.None<IProject>().WithException(failureReason);
                }
            }
            return Option.None<IProject>().WithException("Cancelled");
        }
    }

    class ProjectCollection
    {

    }
}
