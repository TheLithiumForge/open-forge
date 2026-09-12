using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
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

        if (boundary.Record.State == LibrariesRecordReadState.Complete && boundary.Record.Record is not { })
        {
            throw new InvalidOperationException("A complete Library record observation requires its document.");
        }
        if (boundary.Record.Record is { } record && record.Libraries.Any(library =>
                LibraryPathIdentity.Mappings(library).Any(mapping => PortableWorkspacePath.CreatePortableKey(mapping.DestinationPath.Value) == PortableWorkspacePath.CreatePortableKey(boundary.Path))))
        {
            return new ExtensionRemoveFinding(
                ExtensionRemoveFindingCode.OwnershipConflict,
                "The Extension target is owned by a registered workspace Library.",
                boundary.Path);
        }

        return boundary.Record.State switch
        {
            LibrariesRecordReadState.Missing or LibrariesRecordReadState.Complete => null,
            LibrariesRecordReadState.Unavailable => new ExtensionRemoveFinding(
                ExtensionRemoveFindingCode.ProjectionUnavailable,
                boundary.Record.Cause
                    ?? "Library ownership could not be observed for the Extension target.",
                boundary.Path),
            LibrariesRecordReadState.Malformed or LibrariesRecordReadState.Blocked => new ExtensionRemoveFinding(
                ExtensionRemoveFindingCode.OwnershipConflict,
                boundary.Record.Cause
                    ?? "Library ownership is unsafe or ambiguous for the Extension target.",
                boundary.Path),
            _ => throw new ArgumentOutOfRangeException(
                nameof(boundary),
                boundary.Record.State,
                "The Library record state is not defined."),
        };
    }

    internal static bool Matches(ExtensionRemoveLibraryBoundary expected, ExtensionRemoveLibraryBoundary observed)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(observed);
        if (!string.Equals(expected.Path, observed.Path, StringComparison.Ordinal) || expected.Request.Workspace != observed.Request.Workspace)
        {
            throw new ArgumentException("Library boundary revalidation requires the same workspace target.", nameof(observed));
        }

        return expected.Record.State == observed.Record.State
            && SnapshotMatches(expected.Record.Snapshot, observed.Record.Snapshot)
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
