using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal sealed class ExtensionRemovePathInspector(PhysicalPathResolver physicalPathResolver)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<ExtensionRemovePathObservation> ReadAsync(
        ExtensionRemoveRequest request,
        string path,
        WorkspaceOwnershipDocument ownership,
        IReadOnlySet<string> selected,
        CancellationToken cancellationToken)
    {
        if (!ExtensionDestinationPolicy.IsAllowed(request.Workspace, path)
            || !ExtensionDestinationInspector.HasOrdinaryAncestors(_physicalPathResolver, request.Workspace, path, cancellationToken))
        {
            return ExtensionRemovePathObservation.Stop(
                path,
                ExtensionRemoveFindingCode.TargetUnsafe,
                "The Extension destination is not an eligible workspace file.");
        }

        var key = PortableWorkspacePath.CreatePortableKey(path);
        if (ownership.Extensions.SelectMany(extension => extension.Paths).Any(other =>
            other != path && PortableWorkspacePath.CreatePortableKey(other) == key))
        {
            return ExtensionRemovePathObservation.Stop(
                path,
                ExtensionRemoveFindingCode.OwnershipObservation,
                "The recorded Extension paths have a portable alias; no files were deleted.");
        }

        var libraryBoundary = await ExtensionRemoveLibraryBoundaryReader.ReadAsync(
            _physicalPathResolver, request, path, cancellationToken).ConfigureAwait(false);
        if (ExtensionRemoveLibraryBoundaryPolicy.Classify(libraryBoundary) is { } libraryFinding)
        {
            return ExtensionRemovePathObservation.Stop(path, libraryFinding.Code, libraryFinding.Cause)
                with
            { LibraryBoundary = libraryBoundary };
        }

        var selectedOwners = ownership.OwnersOf(path).Distinct(StringComparer.Ordinal).Where(selected.Contains).Order(StringComparer.Ordinal).ToArray();
        var remainingOwners = ownership.OwnersOf(path).Distinct(StringComparer.Ordinal).Where(owner => !selected.Contains(owner)).Order(StringComparer.Ordinal).ToArray();
        if (remainingOwners.Length > 0)
        {
            return ExtensionRemovePathObservation.Shared(path, selectedOwners, remainingOwners) with { LibraryBoundary = libraryBoundary };
        }

        var logicalPath = Path.GetFullPath(Path.Combine(
            request.Workspace.LexicalRoot,
            path.Replace('/', Path.DirectorySeparatorChar)));
        var resolution = _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            logicalPath);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return ExtensionRemovePathObservation.Missing(path, selectedOwners) with { LibraryBoundary = libraryBoundary };
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return ExtensionRemovePathObservation.Stop(
                path,
                ExtensionRemoveFindingCode.TargetUnsafe,
                resolution.Failure?.DirectCause
                    ?? "The managed Extension target boundary is unsafe or unavailable.");
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        try
        {
            var attributes = File.GetAttributes(physicalPath);
            if ((attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
            {
                return ExtensionRemovePathObservation.Stop(
                    path,
                    ExtensionRemoveFindingCode.TargetUnsafe,
                    "The managed Extension target is not an ordinary file.");
            }

            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
                .ConfigureAwait(false);
            var confirmed = _physicalPathResolver.ResolveCandidate(
                request.Workspace.LexicalRoot,
                request.Workspace.PhysicalRoot,
                logicalPath);
            if (confirmed.State != PhysicalPathState.Contained
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    physicalPath,
                    confirmed.GetContainedPhysicalPath()))
            {
                return ExtensionRemovePathObservation.Stop(
                    path,
                    ExtensionRemoveFindingCode.TargetChanged,
                    "The managed Extension target changed physical identity during inspection.");
            }

            return ExtensionRemovePathObservation.Existing(
                path,
                selectedOwners,
                ExtensionRemovePathClassification.FinalOwner,
                FileStateSnapshot.File(logicalPath, physicalPath, bytes)) with
            { LibraryBoundary = libraryBoundary };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is ArgumentException
            or IOException
            or InvalidDataException
            or UnauthorizedAccessException)
        {
            return ExtensionRemovePathObservation.Stop(
                path,
                ExtensionRemoveFindingCode.TargetUnsafe,
                $"The managed Extension target is unsafe or unavailable: {exception.Message}");
        }
    }

}
