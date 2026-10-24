using OpenForge.Cli.Core.Commands.Shared.Layout;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;

internal static class DoctorLayoutPresentation
{
    internal static DoctorFinding Project(WorkspaceLayout layout, DoctorFinding finding)
    {
        var advice = finding.Maintenance;
        if (advice is not null)
        {
            var oldPath = advice.OldPath;
            var newPath = advice.NewPath;
            if (oldPath is not null && newPath is not null)
                (oldPath, newPath) = WorkspaceLayoutPathDisplay.Rename(layout, oldPath, newPath);
            advice = advice with
            {
                DisplayOldPath = oldPath,
                DisplayNewPath = newPath,
                GuidanceDirectory = layout.CategoryDirectory(WorkspaceCategoryNames.Guidance),
                LoaderPath = layout.LoaderPath,
            };
        }
        return finding with
        {
            CustomMainRoot = layout.IsDefault ? null : layout.MainRoot,
            CustomLoaderPath = layout.IsDefault ? null : layout.LoaderPath,
            Maintenance = advice,
        };
    }
}
