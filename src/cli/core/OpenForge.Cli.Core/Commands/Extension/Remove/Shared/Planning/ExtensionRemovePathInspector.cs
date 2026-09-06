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
        if (!path.Path.StartsWith(".agents/", StringComparison.Ordinal))
        {
            return ExtensionRemovePathObservation.Stop(
                path.Path,
                ExtensionRemoveFindingCode.TargetOutsideAgents,
                "Extension Remove targets must remain below the .agents directory.");
        }

        var selectedOwners = path.Owners.Where(selected.Contains).Order(StringComparer.Ordinal).ToArray();
        var remainingOwners = path.Owners.Where(owner => !selected.Contains(owner)).Order(StringComparer.Ordinal).ToArray();
        if (remainingOwners.Length > 0)
        {
            return ExtensionRemovePathObservation.Shared(path.Path, selectedOwners, remainingOwners);
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
            return ExtensionRemovePathObservation.Missing(path.Path, selectedOwners);
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return ExtensionRemovePathObservation.Stop(
                path.Path,
                resolution.State == PhysicalPathState.External
                    ? ExtensionRemoveFindingCode.TargetOutsideAgents
                    : ExtensionRemoveFindingCode.TargetUnsafe,
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
                FileStateSnapshot.File(logicalPath, physicalPath, bytes));
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

internal sealed record ExtensionRemovePathObservation
{
    internal required string Path { get; init; }

    internal required IReadOnlyList<string> SelectedOwnerIds { get; init; }

    internal required IReadOnlyList<string> RemainingOwnerIds { get; init; }

    internal required ExtensionRemovePathClassification Classification { get; init; }

    internal FileStateSnapshot? Snapshot { get; init; }

    internal ExtensionRemoveFinding? Boundary { get; init; }

    internal ExtensionRemovePathPlan ToPathPlan(ExtensionRemoveChangedContentPolicy policy)
        => new(
            Path,
            Classification,
            SelectedOwnerIds,
            RemainingOwnerIds,
            Classification switch
            {
                ExtensionRemovePathClassification.Shared => ExtensionRemovePathAction.RetainShared,
                ExtensionRemovePathClassification.UnchangedFinalOwner => ExtensionRemovePathAction.Delete,
                ExtensionRemovePathClassification.ChangedFinalOwner
                    when policy == ExtensionRemoveChangedContentPolicy.Delete => ExtensionRemovePathAction.Delete,
                ExtensionRemovePathClassification.ChangedFinalOwner => ExtensionRemovePathAction.KeepAsUnmanaged,
                ExtensionRemovePathClassification.Missing => ExtensionRemovePathAction.ReleaseOwnership,
                _ => throw new ArgumentOutOfRangeException(),
            });

    internal static ExtensionRemovePathObservation Shared(
        string path,
        IReadOnlyList<string> selected,
        IReadOnlyList<string> remaining)
        => Create(path, selected, remaining, ExtensionRemovePathClassification.Shared, snapshot: null);

    internal static ExtensionRemovePathObservation Missing(string path, IReadOnlyList<string> selected)
        => Create(path, selected, [], ExtensionRemovePathClassification.Missing, snapshot: null);

    internal static ExtensionRemovePathObservation Existing(
        string path,
        IReadOnlyList<string> selected,
        ExtensionRemovePathClassification classification,
        FileStateSnapshot snapshot)
        => Create(path, selected, [], classification, snapshot);

    internal static ExtensionRemovePathObservation Stop(
        string path,
        ExtensionRemoveFindingCode code,
        string cause)
        => new()
        {
            Path = path,
            SelectedOwnerIds = [],
            RemainingOwnerIds = [],
            Classification = ExtensionRemovePathClassification.Missing,
            Snapshot = null,
            Boundary = new ExtensionRemoveFinding(code, cause, path),
        };

    private static ExtensionRemovePathObservation Create(
        string path,
        IReadOnlyList<string> selected,
        IReadOnlyList<string> remaining,
        ExtensionRemovePathClassification classification,
        FileStateSnapshot? snapshot)
        => new()
        {
            Path = path,
            SelectedOwnerIds = selected,
            RemainingOwnerIds = remaining,
            Classification = classification,
            Snapshot = snapshot,
            Boundary = null,
        };
}
