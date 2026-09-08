using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal sealed class ExtensionRemovePathInspector(PhysicalPathResolver physicalPathResolver)
{
    private readonly FrameworkContentIdentity _contentIdentity = new();
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<ExtensionRemovePathObservation> ReadAsync(
        ExtensionRemoveRequest request,
        LifecycleExtensionPathV1 path,
        IReadOnlySet<string> selected,
        CancellationToken cancellationToken)
    {
        if (!ExtensionDestinationPolicy.IsAllowed(request.Workspace, path.Path)
            || !ExtensionDestinationInspector.HasOrdinaryAncestors(_physicalPathResolver, request.Workspace, path.Path, cancellationToken))
        {
            return ExtensionRemovePathObservation.Stop(
                path.Path,
                ExtensionRemoveFindingCode.TargetUnsafe,
                "The Extension destination is not an eligible workspace file.");
        }

        var libraryBoundary = await ExtensionRemoveLibraryBoundaryReader.ReadAsync(
            _physicalPathResolver, request, path.Path, cancellationToken).ConfigureAwait(false);
        if (ExtensionRemoveLibraryBoundaryPolicy.Classify(libraryBoundary) is { } libraryFinding)
        {
            return ExtensionRemovePathObservation.Stop(path.Path, libraryFinding.Code, libraryFinding.Cause)
                with
            { LibraryBoundary = libraryBoundary };
        }

        var selectedOwners = path.Owners.Where(selected.Contains).Order(StringComparer.Ordinal).ToArray();
        var remainingOwners = path.Owners.Where(owner => !selected.Contains(owner)).Order(StringComparer.Ordinal).ToArray();
        if (remainingOwners.Length > 0)
        {
            return ExtensionRemovePathObservation.Shared(path.Path, selectedOwners, remainingOwners) with { LibraryBoundary = libraryBoundary };
        }

        var logicalPath = Path.GetFullPath(Path.Combine(
            request.Workspace.LexicalRoot,
            path.Path.Replace('/', Path.DirectorySeparatorChar)));
        var resolution = _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            logicalPath);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return ExtensionRemovePathObservation.Missing(path.Path, selectedOwners) with { LibraryBoundary = libraryBoundary };
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return ExtensionRemovePathObservation.Stop(
                path.Path,
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
                    path.Path,
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
                    path.Path,
                    ExtensionRemoveFindingCode.TargetChanged,
                    "The managed Extension target changed physical identity during inspection.");
            }

            return ExtensionRemovePathObservation.Existing(
                path.Path,
                selectedOwners,
                ReadClassification(bytes, path),
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
                path.Path,
                ExtensionRemoveFindingCode.TargetUnsafe,
                $"The managed Extension target is unsafe or unavailable: {exception.Message}");
        }
    }

    private ExtensionRemovePathClassification ReadClassification(
        ReadOnlySpan<byte> bytes,
        LifecycleExtensionPathV1 path)
    {
        if (string.Equals(
                path.FingerprintKind,
                LifecycleSchema.SemanticFingerprintKind,
                StringComparison.Ordinal))
        {
            var facts = _contentIdentity.ReadSourceFingerprint(bytes);
            if (!facts.IsSemantic || facts.Sha256 is null)
            {
                return ExtensionRemovePathClassification.ChangedFinalOwner;
            }

            return Classify(facts.Sha256, path.BaselineFingerprint);
        }

        var fingerprint = _contentIdentity.ReadSourceFingerprint(bytes, path.FingerprintKind);
        return Classify(fingerprint, path.BaselineFingerprint);
    }

    private static ExtensionRemovePathClassification Classify(
        string currentFingerprint,
        string baselineFingerprint)
        => string.Equals(currentFingerprint, baselineFingerprint, StringComparison.Ordinal)
            ? ExtensionRemovePathClassification.UnchangedFinalOwner
            : ExtensionRemovePathClassification.ChangedFinalOwner;
}
