using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal static class ExtensionRemoveLibraryBoundaryPolicy
{
    internal static ExtensionRemoveFinding? Classify(ExtensionRemoveLibraryBoundary boundary)
    {
        ArgumentNullException.ThrowIfNull(boundary);

        var leafFinding = boundary.Leaf.State switch
        {
            NoFollowLeafState.Missing or NoFollowLeafState.OrdinaryFile => null,
            NoFollowLeafState.Inaccessible or NoFollowLeafState.Unknown => new ExtensionRemoveFinding(
                ExtensionRemoveFindingCode.ProjectionUnavailable,
                boundary.Leaf.Failure?.DirectCause
                    ?? "The Extension target could not be observed without following its final component.",
                boundary.Path),
            NoFollowLeafState.Directory
                or NoFollowLeafState.RelativeFileLink
                or NoFollowLeafState.Link
                or NoFollowLeafState.ReparsePoint
                or NoFollowLeafState.Special => new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.TargetUnsafe,
                    "The Extension target is not an ordinary non-link file or a safely missing leaf.",
                    boundary.Path),
            _ => throw new ArgumentOutOfRangeException(
                nameof(boundary),
                boundary.Leaf.State,
                "The no-follow target state is not defined."),
        };
        if (leafFinding is not null)
        {
            return leafFinding;
        }

        var key = PortableWorkspacePath.CreatePortableKey(boundary.Path);
        var document = boundary.Ownership.Document;
        string[] libraryPaths;
        try
        {
            libraryPaths = document.Libraries.SelectMany(library => library.Paths.Select(path =>
                LibraryPathIdentity.Map(
                    WorkspaceRelativeDirectory.Create(library.SourceRoot),
                    LibraryDestinationRoot.Create(library.DestinationRoot),
                    SourceRelativeEligiblePath.Create(path)).DestinationPath.Value)).ToArray();
        }
        catch (ArgumentException)
        {
            return new ExtensionRemoveFinding(
                ExtensionRemoveFindingCode.OwnershipObservation,
                "The recorded Library mappings cannot be interpreted safely; no files were deleted.",
                boundary.Path);
        }

        if (libraryPaths.Concat(document.Framework?.Paths ?? [])
            .Any(path => PortableWorkspacePath.CreatePortableKey(path) == key))
        {
            return new ExtensionRemoveFinding(
                ExtensionRemoveFindingCode.OwnershipConflict,
                "The Extension target is owned by the Framework or a workspace Library.",
                boundary.Path);
        }

        return null;
    }

    internal static bool Matches(ExtensionRemoveLibraryBoundary expected, ExtensionRemoveLibraryBoundary observed)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(observed);
        if (!string.Equals(expected.Path, observed.Path, StringComparison.Ordinal) || expected.Request.Workspace != observed.Request.Workspace)
        {
            throw new ArgumentException("Library boundary revalidation requires the same workspace target.", nameof(observed));
        }

        return expected.Ownership.State == observed.Ownership.State
            && SnapshotMatches(expected.Ownership.Snapshot, observed.Ownership.Snapshot)
            && expected.Leaf.State == observed.Leaf.State
            && Equals(expected.Leaf.RelativeFileLink, observed.Leaf.RelativeFileLink)
            && Equals(expected.Leaf.Link, observed.Leaf.Link)
            && expected.Leaf.Failure?.Kind == observed.Leaf.Failure?.Kind;
    }

    private static bool SnapshotMatches(FileStateSnapshot? expected, FileStateSnapshot? observed)
        => expected is null
            ? observed is null
            : observed is not null
                && expected.HasBytes == observed.HasBytes
                && expected.Expectation == observed.Expectation
                && expected.Bytes.AsSpan().SequenceEqual(observed.Bytes.AsSpan());
}
