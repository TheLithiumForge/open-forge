using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
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
            if (oldPath is not null && newPath is not null
                && layout.TryGetSourceRelativePath(oldPath, out var oldRelative)
                && layout.TryGetSourceRelativePath(newPath, out var newRelative))
            {
                var oldCategory = oldRelative.Split('/')[0];
                var newCategory = newRelative.Split('/')[0];
                if (oldCategory == newCategory && oldRelative.Contains('/') && newRelative.Contains('/'))
                {
                    var prefix = layout.CategoryDirectory(oldCategory).Length + 1;
                    oldPath = oldPath[prefix..];
                    newPath = newPath[prefix..];
                }
            }
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
