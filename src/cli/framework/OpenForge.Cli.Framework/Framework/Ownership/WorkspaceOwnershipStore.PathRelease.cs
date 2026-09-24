using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Mutation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Ownership;

internal sealed partial class WorkspaceOwnershipStore
{
    // Task 50's explicit path-removal contract shares this release between route
    // and ordinary-file removal. Callers may release Library mappings only after
    // planning their exact owned link deletion through the link-specific boundary.
    internal OwnershipWritePlanResult PlanContentPathRelease(
        WorkspaceOwnershipRead current,
        ImmutableArray<string> paths,
        ImmutableArray<LibraryPathRelease> libraryPaths = default)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (paths.IsDefault)
        {
            throw new ArgumentException("Removal paths must be initialized.", nameof(paths));
        }
        foreach (var path in paths)
        {
            if (!PortableWorkspacePath.TryNormalize(path, out var normalized) || normalized != path)
            {
                throw new ArgumentException("Removal paths must be canonical workspace-relative paths.", nameof(paths));
            }
        }
        if (current.State is not (WorkspaceOwnershipReadState.Complete or WorkspaceOwnershipReadState.Absent)
            || current.Snapshot is not { } snapshot)
        {
            return OwnershipWritePlanResult.Skipped(
                current.Cause ?? "Ownership must be readable before content claims can be released.");
        }
        if (current.State == WorkspaceOwnershipReadState.Absent || paths.IsEmpty)
        {
            return OwnershipWritePlanResult.Unchanged();
        }

        var selected = paths.ToHashSet(StringComparer.Ordinal);
        var document = current.Document;
        var libraries = document.Libraries;
        if (!libraryPaths.IsDefaultOrEmpty)
        {
            foreach (var release in libraryPaths)
            {
                if (!document.Libraries.Any(library => library.Id == release.LibraryId
                    && library.Paths.Contains(release.SourceRelativePath, StringComparer.Ordinal)))
                {
                    throw new ArgumentException("A Library release must identify a recorded source-relative path.", nameof(libraryPaths));
                }
            }
            var released = libraryPaths.GroupBy(path => path.LibraryId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Select(path => path.SourceRelativePath).ToHashSet(StringComparer.Ordinal), StringComparer.Ordinal);
            libraries = [.. document.Libraries.Select(library => released.TryGetValue(library.Id, out var removed)
                ? library with { Paths = [.. library.Paths.Where(path => !removed.Contains(path))] }
                : library)];
        }
        var hasClaims = (document.Framework?.Paths.Any(selected.Contains) ?? false)
            || (document.Framework?.Regions.Any(region => selected.Contains(region.Path)) ?? false)
            || document.Extensions.Any(extension => extension.Paths.Any(selected.Contains)
                || extension.Regions.Any(region => selected.Contains(region.Path)))
            || libraries.Where((library, index) => library.Paths.Length != document.Libraries[index].Paths.Length).Any();
        if (!hasClaims)
        {
            return OwnershipWritePlanResult.Unchanged();
        }

        var intended = document with
        {
            Framework = document.Framework is { } framework
                ? framework with
                {
                    Paths = [.. framework.Paths.Where(path => !selected.Contains(path))],
                    Regions = [.. framework.Regions.Where(region => !selected.Contains(region.Path))],
                }
                : null,
            Extensions = [.. document.Extensions.Select(extension => extension with
            {
                Paths = [.. extension.Paths.Where(path => !selected.Contains(path))],
                Regions = [.. extension.Regions.Where(region => !selected.Contains(region.Path))],
            })],
            Libraries = libraries,
        };
        return OwnershipWritePlanResult.Planned(
            PlannedFileChange.Replace(snapshot.Expectation, WorkspaceOwnershipCodec.Write(intended)));
    }
}
