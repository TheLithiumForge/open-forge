using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;

internal static class LibraryMappingObserver
{
    internal static LibraryMappingObservation Observe(
        PhysicalPathResolver resolver,
        LibraryMappingObservationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Workspace);
        ArgumentNullException.ThrowIfNull(request.Mapping);
        cancellationToken.ThrowIfCancellationRequested();
        var logicalPath = Path.GetFullPath(Path.Combine(
            request.Workspace.LexicalRoot,
            request.Mapping.DestinationPath.Value.Replace('/', Path.DirectorySeparatorChar)));
        var parentPath = Path.GetDirectoryName(logicalPath)
            ?? throw new InvalidOperationException("A Library mapping destination requires a parent.");
        var parent = LibraryDirectoryBoundaryObserver.Observe(
            request.Workspace,
            parentPath,
            cancellationToken);
        if (parent.State == NoFollowLeafState.Missing)
        {
            return LibraryMappingObservation.Create(
                request.Mapping,
                NoFollowLeafObservation.Missing(logicalPath),
                LibraryMappingObservationState.Missing);
        }

        if (parent.State != NoFollowLeafState.Directory)
        {
            var unavailable = parent.State is NoFollowLeafState.Inaccessible
                or NoFollowLeafState.Unknown;
            var leaf = NoFollowLeafObservation.Classified(
                logicalPath,
                NoFollowLeafState.Unknown,
                parent.Failure
                    ?? new FilesystemFailure(
                        FilesystemFailureKind.Unsupported,
                        "The Library destination parent is not a real ordinary directory."));
            return LibraryMappingObservation.Create(
                request.Mapping,
                leaf,
                unavailable
                    ? LibraryMappingObservationState.Unavailable
                    : LibraryMappingObservationState.Blocked,
                "The Library destination parent is not a real ordinary directory.");
        }

        var observed = NoFollowLeafObserver.Observe(
            resolver,
            request.Workspace,
            logicalPath,
            cancellationToken);
        var expected = RelativeFileLinkIdentity.Create(
            NoFollowLinkKind.SymbolicLink,
            request.Mapping.ExpectedRelativeLink.Value);
        return observed.State switch
        {
            NoFollowLeafState.Missing => LibraryMappingObservation.Create(
                request.Mapping,
                observed,
                LibraryMappingObservationState.Missing),
            NoFollowLeafState.RelativeFileLink => LibraryMappingObservation.Create(
                request.Mapping,
                observed,
                observed.RelativeFileLink == expected
                    ? LibraryMappingObservationState.Current
                    : LibraryMappingObservationState.Changed),
            NoFollowLeafState.OrdinaryFile => LibraryMappingObservation.Create(
                request.Mapping,
                observed,
                LibraryMappingObservationState.Changed),
            NoFollowLeafState.Inaccessible or NoFollowLeafState.Unknown =>
                LibraryMappingObservation.Create(
                    request.Mapping,
                    observed,
                    LibraryMappingObservationState.Unavailable,
                    observed.Failure?.DirectCause
                        ?? "The Library destination leaf is unavailable."),
            NoFollowLeafState.Directory
                or NoFollowLeafState.Link
                or NoFollowLeafState.ReparsePoint
                or NoFollowLeafState.Special => LibraryMappingObservation.Create(
                    request.Mapping,
                    observed,
                    LibraryMappingObservationState.Blocked,
                    "The Library destination is occupied by an unsafe object kind."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(request),
                observed.State,
                "The no-follow leaf state is not defined."),
        };
    }
}
