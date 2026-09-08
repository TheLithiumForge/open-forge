using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Observation;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Application;

internal static class RelativeFileLinkRecoveryApplier
{
    internal static async ValueTask<RelativeFileLinkRecoveryResult> ApplyAsync(
        PhysicalPathResolver physicalPathResolver,
        WorkspaceLockLease lease,
        RecoveryBundlePreparation preparation,
        RecoveryEntry entry,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(entry);
        if (!lease.IsHeldFor(lease.Request.Workspace))
        {
            throw new ArgumentException("Explicit recovery requires a held workspace lease.", nameof(lease));
        }

        if (!preparation.MatchesWorkspace(lease.Request.Workspace) || !preparation.Entries.Contains(entry))
        {
            throw new ArgumentException("Explicit recovery requires the entry from its matching workspace preparation.", nameof(preparation));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var context = new RecoveryEntryComparisonContext(lease.Request.Workspace, entry);
        var observation = await RecoveryEntryObservationReader.ReadAsync(
            physicalPathResolver,
            context,
            cancellationToken).ConfigureAwait(false);
        var comparison = RecoveryEntryComparer.Compare(observation);
        return await ApplyComparedAsync(
            physicalPathResolver,
            lease,
            preparation,
            comparison,
            cancellationToken).ConfigureAwait(false);
    }

    // This explicit recovery path consumes the same observed comparison tested
    // in isolation. It must revalidate the held lease, preparation and current
    // final leaf before mutation. It is never automatic Library compensation.
    private static async ValueTask<RelativeFileLinkRecoveryResult> ApplyComparedAsync(
        PhysicalPathResolver physicalPathResolver,
        WorkspaceLockLease lease,
        RecoveryBundlePreparation preparation,
        RecoveryEntryComparison comparison,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(comparison);
        if (!lease.IsHeldFor(comparison.Input.Context.Workspace)
            || !preparation.MatchesWorkspace(lease.Request.Workspace)
            || !preparation.Entries.Contains(comparison.Input.Context.Entry))
        {
            throw new ArgumentException("The compared recovery entry must retain its held lease and matching preparation.", nameof(comparison));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var entry = comparison.Input.Context.Entry;
        if (entry.Kind is not (RecoveryEntryKind.RelativeFileLinkCreate
            or RecoveryEntryKind.RelativeFileLinkDelete))
        {
            throw new ArgumentException(
                "Relative file-link recovery requires a typed link entry.",
                nameof(comparison));
        }

        if (comparison.State != RecoveryBundleTargetComparisonState.Intended)
        {
            return RelativeFileLinkRecoveryResult.Classified(
                comparison.State is RecoveryBundleTargetComparisonState.Blocked
                    or RecoveryBundleTargetComparisonState.Unavailable
                    ? RelativeFileLinkRecoveryState.Blocked
                    : RelativeFileLinkRecoveryState.Mismatched,
                entry,
                comparison.Input.Leaf,
                after: null,
                comparison.Cause
                    ?? "The recovery target is not in the exact intended link state.",
                comparison.Input.Leaf.Failure);
        }

        var evidence = await RecoveryApplicationEvidence.ReadExactFinalAsync(
            lease.Request.Workspace,
            preparation,
            cancellationToken).ConfigureAwait(false);
        if (evidence.State != RecoveryBundleReadState.Valid)
        {
            return RelativeFileLinkRecoveryResult.Classified(
                RelativeFileLinkRecoveryState.Failed,
                entry,
                comparison.Input.Leaf,
                after: null,
                evidence.Cause ?? "The recovery final is unavailable.",
                evidence.Failure);
        }

        var inverse = Inverse(entry);
        var immediate = await RelativeFileLinkRevalidator.ValidateAsync(
            physicalPathResolver,
            lease,
            inverse,
            cancellationToken).ConfigureAwait(false);
        if (immediate.State != RelativeFileLinkValidationState.Matched)
        {
            return RelativeFileLinkRecoveryResult.Classified(
                immediate.State is RelativeFileLinkValidationState.Blocked
                    or RelativeFileLinkValidationState.Failed
                    ? RelativeFileLinkRecoveryState.Blocked
                    : RelativeFileLinkRecoveryState.Mismatched,
                entry,
                immediate.Actual ?? comparison.Input.Leaf,
                after: null,
                immediate.Cause
                    ?? "The recovery target changed before the inverse link effect.",
                immediate.Failure);
        }

        var before = immediate.Actual
            ?? throw new InvalidOperationException(
                "A matched link recovery validation requires an actual observation.");
        var logicalPath = Path.GetFullPath(Path.Combine(
            lease.Request.Workspace.LexicalRoot,
            entry.TargetPath.Replace('/', Path.DirectorySeparatorChar)));
        var physicalPath = Path.GetFullPath(Path.Combine(
            lease.Request.Workspace.PhysicalRoot,
            entry.TargetPath.Replace('/', Path.DirectorySeparatorChar)));
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (inverse.Kind)
            {
                case RelativeFileLinkEffectKind.Create:
                    File.CreateSymbolicLink(physicalPath, inverse.RawRelativeTarget);
                    break;
                case RelativeFileLinkEffectKind.Delete:
                    File.Delete(physicalPath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(comparison),
                        inverse.Kind,
                        "The inverse link effect kind is not defined.");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or NotSupportedException
            or PlatformNotSupportedException)
        {
            var failure = FilesystemFailure.FromException(
                exception switch
                {
                    UnauthorizedAccessException => FilesystemFailureKind.AccessDenied,
                    NotSupportedException or PlatformNotSupportedException =>
                        FilesystemFailureKind.Unsupported,
                    _ => FilesystemFailureKind.InputOutput,
                },
                exception);
            var afterFailure = TryObserve(
                physicalPathResolver,
                lease,
                logicalPath,
                cancellationToken);
            if (afterFailure is not null
                && MatchesPrior(entry, afterFailure))
            {
                return RelativeFileLinkRecoveryResult.Restored(
                    entry,
                    before,
                    afterFailure);
            }

            return RelativeFileLinkRecoveryResult.Classified(
                RelativeFileLinkRecoveryState.Failed,
                entry,
                before,
                afterFailure,
                failure.DirectCause,
                failure);
        }

        RecoveryEntryComparisonInput afterInput;
        try
        {
            afterInput = await RecoveryEntryObservationReader.ReadAsync(
                physicalPathResolver,
                comparison.Input.Context,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RelativeFileLinkRecoveryResult.Classified(
                RelativeFileLinkRecoveryState.Failed,
                entry,
                before,
                after: null,
                "Relative link recovery verification was cancelled after the inverse effect returned.");
        }

        var after = afterInput.Leaf;
        var afterComparison = RecoveryEntryComparer.Compare(afterInput);
        return afterComparison.State == RecoveryBundleTargetComparisonState.Prior
            ? RelativeFileLinkRecoveryResult.Restored(entry, before, after)
            : RelativeFileLinkRecoveryResult.Classified(
                afterComparison.State is RecoveryBundleTargetComparisonState.Blocked
                    or RecoveryBundleTargetComparisonState.Unavailable
                    ? RelativeFileLinkRecoveryState.Blocked
                    : RelativeFileLinkRecoveryState.Failed,
                entry,
                before,
                after,
                afterComparison.Cause
                    ?? "The inverse link effect did not restore the exact prior identity.",
                after.Failure);
    }

    private static RelativeFileLinkEffect Inverse(RecoveryEntry entry)
    {
        var path = CanonicalRelativePath.Create(entry.TargetPath);
        return entry.Kind switch
        {
            RecoveryEntryKind.RelativeFileLinkCreate => RelativeFileLinkEffect.Delete(
                path,
                entry.Intended.RelativeFileLink
                    ?? throw new InvalidOperationException(
                        "A link-create recovery entry requires intended link identity.")),
            RecoveryEntryKind.RelativeFileLinkDelete => RelativeFileLinkEffect.Create(
                path,
                entry.Prior.RelativeFileLink
                    ?? throw new InvalidOperationException(
                        "A link-delete recovery entry requires prior link identity.")),
            _ => throw new ArgumentOutOfRangeException(
                nameof(entry),
                entry.Kind,
                "The recovery entry is not a relative link effect."),
        };
    }

    private static bool MatchesPrior(
        RecoveryEntry entry,
        NoFollowLeafObservation observation)
        => entry.Prior.Kind switch
        {
            RecoveryEntryStateKind.Missing => observation.State == NoFollowLeafState.Missing,
            RecoveryEntryStateKind.RelativeFileLink =>
                observation.State == NoFollowLeafState.RelativeFileLink
                && observation.RelativeFileLink == entry.Prior.RelativeFileLink,
            RecoveryEntryStateKind.OrdinaryFile => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(entry),
                entry.Prior.Kind,
                "The recovery prior state kind is not defined."),
        };

    private static NoFollowLeafObservation? TryObserve(
        PhysicalPathResolver resolver,
        WorkspaceLockLease lease,
        string logicalPath,
        CancellationToken cancellationToken)
    {
        try
        {
            return NoFollowLeafObserver.Observe(
                resolver,
                lease.Request.Workspace,
                logicalPath,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }
}
