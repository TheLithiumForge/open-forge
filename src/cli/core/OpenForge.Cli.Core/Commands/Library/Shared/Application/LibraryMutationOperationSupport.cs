using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Permissions;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Application;

internal static class LibraryMutationOperationSupport
{
    internal static ImmutableArray<LibraryMappingObservation> ObserveMappings(
        PhysicalPathResolver resolver,
        LibraryMappingSetRequest request,
        CancellationToken cancellationToken)
        => [.. request.Paths.Select(path => LibraryMappingObserver.Observe(resolver,
            new LibraryMappingObservationRequest
            {
                Workspace = request.Workspace,
                Mapping = LibraryPathIdentity.Map(request.SourceRoot, request.DestinationRoot, path),
            }, cancellationToken))];

    internal static ImmutableArray<CanonicalRelativePath> ReadAncestors(
        CliWorkspace workspace,
        IEnumerable<string> destinationPaths,
        IEnumerable<PlannedFileChange> generatedRegions)
    {
        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in destinationPaths.Concat(generatedRegions.Select(change =>
                     Path.GetRelativePath(workspace.LexicalRoot, change.LogicalPath)
                         .Replace(Path.DirectorySeparatorChar, '/')
                         .Replace(Path.AltDirectorySeparatorChar, '/'))))
        {
            var segments = path.Split('/');
            for (var length = 1; length < segments.Length; length++)
            {
                var ancestor = string.Join('/', segments.AsSpan(0, length).ToArray());
                if (ancestor != WorkspacePermissionDefinitions.ImplicitDirectoryPath)
                {
                    paths.Add(ancestor);
                }
            }
        }

        return [.. paths.Order(StringComparer.Ordinal).Select(CanonicalRelativePath.Create)];
    }

    internal static async ValueTask<RecoveryBundlePreparationResult> PrepareRecoveryAsync(
        LibraryRecoveryPreparationRequest request,
        CancellationToken cancellationToken)
    {
        var targets = new List<RecoveryBundleTarget>();
        if (request.Permissions?.RecoveryTarget is { } permission)
        {
            targets.Add(permission);
        }
        foreach (var link in request.Links)
        {
            var observation = request.Mappings.SingleOrDefault(mapping =>
                mapping.Mapping.DestinationPath.Value == link.DestinationPath.Value)
                ?? throw new InvalidOperationException("A Library link effect requires its exact no-follow preflight observation.");
            targets.Add(RecoveryBundleTarget.Create(link, observation.Leaf));
        }
        foreach (var change in request.GeneratedRegions)
        {
            targets.Add(RecoveryBundleTarget.Create(change,
                await ReadSnapshotAsync(change, cancellationToken).ConfigureAwait(false)));
        }
        if (request.RecordChange is { } recordChange)
        {
            var before = request.Record.Snapshot
                ?? throw new InvalidOperationException("A Library record effect requires its exact prior snapshot.");
            targets.Add(recordChange.Kind == PlannedFileChangeKind.Create
                ? RecoveryBundleTarget.CreateReversible(recordChange, before)
                : RecoveryBundleTarget.Create(recordChange, before));
        }
        return await RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(request.Lease.Request.Workspace, request.Command,
                RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, request.Operation, request.Lease.Request.Workspace),
                request.Lease.Request.OperationId, targets), cancellationToken).ConfigureAwait(false);
    }

    internal static async ValueTask<RecoveryBundleDeletionResult?> CleanupRecoveryAsync(
        WorkspaceLockLease lease,
        RecoveryBundlePreparation? preparation,
        CancellationToken cancellationToken)
    {
        if (preparation is null)
        {
            return null;
        }

        var catalogue = await RecoveryBundleCatalogue.ReadAsync(
            lease.Request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (catalogue.State != RecoveryBundleCatalogueState.Available)
        {
            return catalogue.State == RecoveryBundleCatalogueState.Cancelled
                ? RecoveryBundleDeletionResult.CancelledUnknown()
                : RecoveryBundleDeletionResult.BlockedUnknown(
                    catalogue.Cause ?? "The prepared Library recovery bundle could not be re-enumerated.",
                    preparation.BundlePath);
        }

        var candidate = catalogue.Candidates.SingleOrDefault(value => Matches(value, preparation));
        return candidate is null
            ? RecoveryBundleDeletionResult.BlockedUnknown(
                "The prepared Library recovery bundle no longer has one exact recognized identity.",
                preparation.BundlePath)
            : await RecoveryBundleDeletionGuard.DeleteAsync(
                lease,
                candidate,
                cancellationToken).ConfigureAwait(false);
    }

    internal static LibraryExecutionEvidence Empty(
        LibraryCancellationFact? cancellation = null,
        LibraryUnexpectedFailureFact? unexpected = null,
        RecoveryBundlePreparation? preparation = null)
        => new()
        {
            Permission = null,
            Directories = [],
            Links = [],
            GeneratedRegions = [],
            Record = null,
            RecoveryPreparation = preparation,
            RecoveryCleanup = null,
            Cancellation = cancellation,
            UnexpectedFailure = unexpected,
            SourceEffectScope = null,
            RecordPublicationOrder = LibraryRecordPublicationOrder.NotObserved,
        };

    internal static LibraryExecutionEvidence WithCleanup(
        LibraryExecutionEvidence evidence,
        RecoveryBundleDeletionResult? cleanup)
        => evidence with { RecoveryCleanup = cleanup };

    internal static bool PlansMatch(LibraryMutationEffects expected, LibraryMutationEffects actual)
        => expected.Directories.Select(DirectoryKey).SequenceEqual(actual.Directories.Select(DirectoryKey))
            && expected.Links.Select(LinkKey).SequenceEqual(actual.Links.Select(LinkKey))
            && expected.GeneratedRegions.Select(ChangeKey).SequenceEqual(actual.GeneratedRegions.Select(ChangeKey))
            && ChangeKey(expected.RecordChange) == ChangeKey(actual.RecordChange);

    private static async ValueTask<FileStateSnapshot> ReadSnapshotAsync(
        PlannedFileChange change,
        CancellationToken cancellationToken)
    {
        if (change.Expectation.Kind != FileExpectationKind.File
            || change.Expectation.PhysicalPath is not { } physicalPath)
        {
            throw new InvalidOperationException("A generated-region recovery target requires an ordinary-file expectation.");
        }

        var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
        var snapshot = FileStateSnapshot.File(change.LogicalPath, physicalPath, bytes);
        if (snapshot.Expectation != change.Expectation)
        {
            throw new InvalidOperationException("A generated-region target changed before recovery preparation.");
        }

        return snapshot;
    }

    private static bool Matches(
        RecoveryBundleCandidateSnapshot candidate,
        RecoveryBundlePreparation preparation)
        => candidate.Kind == RecoveryBundleCandidateKind.Final
            && candidate.Integrity == RecoveryBundleIntegrity.Verified
            && candidate.Verified is { } verified
            && PhysicalIdentityTracker.PathComparer.Equals(candidate.Path, preparation.BundlePath)
            && PhysicalIdentityTracker.PathComparer.Equals(
                verified.WorkspacePhysicalPath,
                preparation.WorkspacePhysicalPath)
            && string.Equals(verified.WorkspaceKey, preparation.WorkspaceKey, StringComparison.Ordinal)
            && string.Equals(verified.Command, preparation.Command, StringComparison.Ordinal)
            && verified.OperationId == preparation.OperationId
            && verified.Entries.SequenceEqual(preparation.Entries);

    private static string DirectoryKey(PlannedDirectoryCreation creation)
        => $"{creation.LogicalPath}\u001f{creation.Expectation.Kind}";

    private static string LinkKey(RelativeFileLinkEffect effect)
        => $"{effect.DestinationPath.Value}\u001f{effect.Kind}\u001f{effect.RawRelativeTarget}\u001f{effect.Expected.State}";

    private static string? ChangeKey(PlannedFileChange? change)
        => change is null
            ? null
            : $"{change.LogicalPath}\u001f{change.Kind}\u001f{change.Expectation.Kind}\u001f{change.Expectation.ContentHash}\u001f{FileExpectation.Hash(change.IntendedBytes.AsSpan())}";
}
