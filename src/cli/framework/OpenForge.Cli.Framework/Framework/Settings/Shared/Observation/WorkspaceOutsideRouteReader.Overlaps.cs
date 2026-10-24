using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Observation;

internal sealed partial class WorkspaceOutsideRouteReader
{
    private void ApplyOverlaps(ImmutableArray<WorkspaceOutsideRouteRead>.Builder reads, CancellationToken cancellationToken)
    {
        if (reads.Count == 0) return;
        var reserved = new[] { layout.MainRoot, WorkspaceLayout.DefaultMainRoot, WorkspaceRuntimePaths.DirectoryName, WorkspaceLayoutNames.GitMetadataDirectoryName }
            .Distinct(StringComparer.Ordinal)
            .Select(relative => (Relative: relative, Resolution: Resolve(relative)))
            .ToArray();
        for (var index = 0; index < reads.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (reads[index].PhysicalDirectory is not { } physical) continue;
            foreach (var (relative, resolution) in reserved)
            {
                var reservedPhysical = resolution.ResolvedPhysicalPath;
                if (resolution.State == PhysicalPathState.Missing)
                    reservedPhysical = Path.Combine(layout.Workspace.PhysicalRoot, relative.Replace('/', Path.DirectorySeparatorChar));
                if (reservedPhysical is not null && Overlaps(physical, reservedPhysical))
                    reads[index] = Overlap(reads[index], relative);
            }
            var physicalRelative = Path.GetRelativePath(layout.Workspace.PhysicalRoot, physical);
            if (physicalRelative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Contains(WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparer.OrdinalIgnoreCase))
                reads[index] = Overlap(reads[index], WorkspaceLayoutNames.GitMetadataDirectoryName);
            for (var other = 0; other < index; other++)
            {
                if (reads[other].PhysicalDirectory is not { } otherPhysical || !Overlaps(physical, otherPhysical)) continue;
                reads[index] = Overlap(reads[index], reads[other].Descriptor.DirectoryPath);
                reads[other] = Overlap(reads[other], reads[index].Descriptor.DirectoryPath);
            }
        }
    }

    private static bool Overlaps(string left, string right)
        => PhysicalContainment.Contains(PortableWorkspacePath.CreatePortableKey(left), PortableWorkspacePath.CreatePortableKey(right))
            || PhysicalContainment.Contains(PortableWorkspacePath.CreatePortableKey(right), PortableWorkspacePath.CreatePortableKey(left));

    private static WorkspaceOutsideRouteRead Overlap(WorkspaceOutsideRouteRead read, string related)
    {
        if (read.Issues.Any(issue => issue.Code == WorkspaceLayoutIssueCode.OutsideOverlap && issue.RelatedPaths.Contains(related, StringComparer.Ordinal)))
            return read;
        return read with
        {
            State = WorkspaceOutsideRouteReadState.Unsafe,
            Issues = [.. read.Issues, new(WorkspaceLayoutIssueCode.OutsideOverlap, read.Descriptor.DirectoryPath,
                "The resolved registration overlaps reserved storage or another registered directory.", [related])],
        };
    }
}
