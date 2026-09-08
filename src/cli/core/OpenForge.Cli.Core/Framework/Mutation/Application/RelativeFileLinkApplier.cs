using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

// Link effects have a separate applier so ordinary file application can never
// accidentally acquire link-object semantics.
internal static class RelativeFileLinkApplier
{
    internal static async ValueTask<RelativeFileLinkReceipt> ApplyAsync(
        PhysicalPathResolver physicalPathResolver,
        WorkspaceLockLease lease,
        RelativeFileLinkApplicationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(input);
        var before = input.Expected;
        if (!lease.IsHeldFor(lease.Request.Workspace))
        {
            throw new ArgumentException(
                "Relative file-link application requires a live workspace lease.",
                nameof(lease));
        }

        var logicalPath = Path.GetFullPath(Path.Combine(
            lease.Request.Workspace.LexicalRoot,
            input.Effect.DestinationPath.Value.Replace('/', Path.DirectorySeparatorChar)));
        if (!PhysicalIdentityTracker.PathComparer.Equals(logicalPath, before.LogicalPath))
        {
            throw new ArgumentException(
                "The supplied link expectation must name the effect destination.",
                nameof(input));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return RelativeFileLinkReceipt.NotStarted(
                input.Effect,
                before,
                FilesystemNotStartedReason.Cancelled,
                "Relative file-link application was cancelled before any effect.");
        }

        if (input.RecoveryPreparation is not { } preparation
            || !preparation.MatchesRelativeFileLink(lease.Request, input.Effect))
        {
            return RelativeFileLinkReceipt.NotStarted(
                input.Effect,
                before,
                FilesystemNotStartedReason.ContractRejected,
                "A reversible link effect requires its exact verified recovery preparation.");
        }

        var validation = await RelativeFileLinkRevalidator.ValidateAsync(
            physicalPathResolver,
            lease,
            input.Effect,
            cancellationToken).ConfigureAwait(false);
        if (validation.State != Validation.Models.RelativeFileLinkValidationState.Matched)
        {
            return RelativeFileLinkReceipt.NotStarted(
                input.Effect,
                validation.Actual ?? before,
                validation.State switch
                {
                    Validation.Models.RelativeFileLinkValidationState.Cancelled =>
                        FilesystemNotStartedReason.Cancelled,
                    Validation.Models.RelativeFileLinkValidationState.Failed =>
                        FilesystemNotStartedReason.ApplicationFailed,
                    _ => FilesystemNotStartedReason.TargetChanged,
                },
                validation.Cause ?? "The destination leaf failed immediate no-follow revalidation.");
        }

        before = validation.Actual
            ?? throw new InvalidOperationException("A matched link validation requires an actual observation.");
        string physicalPath;
        try
        {
            if (!RelativeFileLinkRevalidator.TryResolvePhysicalLeaf(
                    physicalPathResolver,
                    lease.Request.Workspace,
                    logicalPath,
                    cancellationToken,
                    out physicalPath,
                    out var parentFailure,
                    out var parentCause))
            {
                return RelativeFileLinkReceipt.NotStarted(
                    input.Effect,
                    before,
                    parentFailure is null
                        ? FilesystemNotStartedReason.TargetChanged
                        : FilesystemNotStartedReason.ApplicationFailed,
                    parentCause);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RelativeFileLinkReceipt.NotStarted(
                input.Effect,
                before,
                FilesystemNotStartedReason.Cancelled,
                "Relative file-link application was cancelled before the effect began.");
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (input.Effect.Kind)
            {
                case RelativeFileLinkEffectKind.Create:
                    File.CreateSymbolicLink(physicalPath, input.Effect.RawRelativeTarget);
                    break;
                case RelativeFileLinkEffectKind.Delete:
                    File.Delete(physicalPath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(input),
                        input.Effect.Kind,
                        "The relative file-link effect kind is not defined.");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RelativeFileLinkReceipt.NotStarted(
                input.Effect,
                before,
                FilesystemNotStartedReason.Cancelled,
                "Relative file-link application was cancelled before the effect began.");
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or NotSupportedException
            or PlatformNotSupportedException)
        {
            return ObserveThrownEffect(
                physicalPathResolver,
                lease,
                input.Effect,
                before,
                logicalPath,
                cancellationToken);
        }

        NoFollowLeafObservation after;
        try
        {
            after = NoFollowLeafObserver.Observe(
                physicalPathResolver,
                lease.Request.Workspace,
                logicalPath,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RelativeFileLinkReceipt.CompletionUnknown(
                input.Effect,
                before,
                after: null,
                "Relative file-link verification was cancelled after the effect returned.");
        }

        return Matches(input.Effect.Intended, after)
            ? RelativeFileLinkReceipt.Verified(input.Effect, before, after)
            : RelativeFileLinkReceipt.VerificationFailed(
                input.Effect,
                before,
                after,
                "The applied link effect did not produce its exact intended no-follow state.");
    }

    private static RelativeFileLinkReceipt ObserveThrownEffect(
        PhysicalPathResolver resolver,
        WorkspaceLockLease lease,
        RelativeFileLinkEffect effect,
        NoFollowLeafObservation before,
        string logicalPath,
        CancellationToken cancellationToken)
    {
        NoFollowLeafObservation? after = null;
        try
        {
            after = NoFollowLeafObserver.Observe(
                resolver,
                lease.Request.Workspace,
                logicalPath,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }

        var cause = "The relative file-link effect failed at the managed filesystem boundary.";
        if (after is not null && Matches(effect.Intended, after))
        {
            return RelativeFileLinkReceipt.Verified(effect, before, after);
        }

        if (after == before)
        {
            return RelativeFileLinkReceipt.NotStarted(
                effect,
                before,
                FilesystemNotStartedReason.ApplicationFailed,
                cause);
        }

        return RelativeFileLinkReceipt.CompletionUnknown(
            effect,
            before,
            after,
            cause);
    }

    private static bool Matches(
        RelativeFileLinkState state,
        NoFollowLeafObservation observation)
        => state.State == observation.State
            && (state.Link is null || state.Link == observation.RelativeFileLink);
}
