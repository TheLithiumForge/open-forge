using OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;

internal sealed class RouteCreateAppliedVerifier(
    RouteCreatePlanBuilder planBuilder,
    FileExpectationValidator expectationValidator)
{
    private readonly RouteCreatePlanBuilder _planBuilder = planBuilder;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal ValueTask<RouteCreateAppliedVerification> VerifyAsync(
        RouteCreatePlan plan,
        WorkspaceLockLease lease,
        IReadOnlyList<FileChangeReceipt> receipts,
        CancellationToken cancellationToken)
        => VerifyAsync(plan, lease, Array.Empty<DirectoryCreationReceipt>(), receipts, cancellationToken);

    internal async ValueTask<RouteCreateAppliedVerification> VerifyAsync(
        RouteCreatePlan plan,
        WorkspaceLockLease lease,
        IReadOnlyList<DirectoryCreationReceipt> directoryReceipts,
        IReadOnlyList<FileChangeReceipt> receipts,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(directoryReceipts);
        ArgumentNullException.ThrowIfNull(receipts);
        if (!lease.IsHeldFor(plan.Request.Workspace))
        {
            return Failed(
                "The final Route Create verification does not hold the selected workspace lease.");
        }

        RouteCreateAppliedVerification? postconditionBoundary;
        try
        {
            postconditionBoundary = await VerifyPostconditionsAsync(
                    plan,
                    directoryReceipts,
                    receipts,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception)
        {
            return Failed(
                "Exact Route Create postcondition verification failed unexpectedly.");
        }

        if (postconditionBoundary is not null)
        {
            return postconditionBoundary;
        }

        RouteCreatePlanBuild fresh;
        try
        {
            fresh = await _planBuilder.BuildAsync(plan.Request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception)
        {
            return Failed(
                "The final Route Create plan could not be rebuilt after application.");
        }

        if (fresh.Formation.Findings.Any(finding =>
                finding.Code == RouteCreateFindingCode.Interrupted))
        {
            return Cancelled();
        }

        if (fresh.Plan is not { } rebuilt)
        {
            return Failed(
                fresh.Formation.Findings.FirstOrDefault()?.Cause
                    ?? "The final Route Create state could not form a complete verification plan.");
        }

        if (!rebuilt.IsNoOp)
        {
            var cause = rebuilt.DirectoryCreations.Length == 0
                ? $"The final Route Create state still requires {rebuilt.FileChanges.Length} file changes."
                : $"The final Route Create state still requires {rebuilt.DirectoryCreations.Length} directory "
                    + $"and {rebuilt.FileChanges.Length} file changes.";
            return Failed(cause);
        }

        if (fresh.Formation.Plan is not
            {
                Completeness: RouteCreatePlanCompleteness.Complete,
                Safety: RouteCreatePlanSafety.Safe,
            }
            || fresh.Formation.Effects.Length != 0
            || !MatchesOptionalFindings(plan.Preview.Findings, fresh.Formation.Findings))
        {
            return Failed(
                "The final Route Create state did not rebuild as one complete safe no-op with the original optional finding set.");
        }

        if (!MatchesFacts(plan.Preview, fresh.Formation))
        {
            return Failed(
                "The final Route Create target, parent, metadata, or Template facts changed after application.");
        }

        return new RouteCreateAppliedVerification
        {
            State = RouteCreateAppliedVerificationState.Verified,
            Cause = null,
        };
    }

    private async ValueTask<RouteCreateAppliedVerification?> VerifyPostconditionsAsync(
        RouteCreatePlan plan,
        IReadOnlyList<DirectoryCreationReceipt> directoryReceipts,
        IReadOnlyList<FileChangeReceipt> receipts,
        CancellationToken cancellationToken)
    {
        if (directoryReceipts.Count != plan.DirectoryCreations.Length
            || receipts.Count != plan.FileChanges.Length)
        {
            return Failed(
                "Final Route Create verification requires one verified receipt per planned effect.");
        }

        for (var index = 0; index < directoryReceipts.Count; index++)
        {
            var receipt = directoryReceipts[index];
            if (receipt.Creation.Expectation != plan.DirectoryCreations[index].Expectation
                || receipt is not
                {
                    EffectState: FilesystemEffectState.Applied,
                    VerificationState: FilesystemVerificationState.Verified,
                    After: { } after,
                }
                || after.Kind != FileExpectationKind.Directory
                || after.PhysicalPath is not { } afterPhysicalPath
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    afterPhysicalPath,
                    receipt.IntendedPhysicalPath))
            {
                return Failed(
                    "A planned Route Create directory has no exact intended verified receipt.");
            }

            var directoryBoundary = await VerifyExpectationAsync(
                    plan,
                    after.Expectation,
                    cancellationToken)
                .ConfigureAwait(false);
            if (directoryBoundary is not null)
            {
                return directoryBoundary;
            }
        }

        for (var index = 0; index < receipts.Count; index++)
        {
            var receipt = receipts[index];
            var change = plan.FileChanges[index];
            if (!Matches(change, receipt.Change)
                || receipt is not
                {
                    EffectState: FilesystemEffectState.Applied,
                    VerificationState: FilesystemVerificationState.Verified,
                    After: { } after,
                }
                || !MatchesIntended(change, after))
            {
                return Failed(
                    "A planned Route Create file has no exact intended verified receipt.");
            }

            var boundary = await VerifyExpectationAsync(
                    plan,
                    after.Expectation,
                    cancellationToken)
                .ConfigureAwait(false);
            if (boundary is not null)
            {
                return boundary;
            }
        }

        return null;
    }

    private async ValueTask<RouteCreateAppliedVerification?> VerifyExpectationAsync(
        RouteCreatePlan plan,
        FileExpectation expectation,
        CancellationToken cancellationToken)
    {
        var validation = await _expectationValidator.ValidateAsync(
                plan.Request.Workspace,
                expectation,
                cancellationToken)
            .ConfigureAwait(false);
        return validation.State switch
        {
            FileExpectationValidationState.Matched => null,
            FileExpectationValidationState.Cancelled => Cancelled(),
            FileExpectationValidationState.Mismatched
                or FileExpectationValidationState.Blocked
                or FileExpectationValidationState.Failed => Failed(
                    validation.Cause
                        ?? "An applied Route Create postcondition changed before final verification."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(validation),
                validation.State,
                "The file expectation validation state is not defined."),
        };
    }

    private static bool Matches(
        PlannedFileChange expected,
        PlannedFileChange actual)
        => expected.Kind == actual.Kind
            && expected.Expectation == actual.Expectation
            && expected.IntendedBytes.AsSpan().SequenceEqual(actual.IntendedBytes.AsSpan());

    private static bool MatchesIntended(
        PlannedFileChange change,
        FileStateSnapshot after)
        => change.Kind switch
        {
            PlannedFileChangeKind.Create
                or PlannedFileChangeKind.Replace
                or PlannedFileChangeKind.ReplaceGeneratedRegion =>
                after.Kind == FileExpectationKind.File
                    && after.HasBytes
                    && after.Bytes.AsSpan().SequenceEqual(change.IntendedBytes.AsSpan()),
            PlannedFileChangeKind.Delete => after.Kind == FileExpectationKind.Missing,
            _ => throw new ArgumentOutOfRangeException(
                nameof(change),
                change.Kind,
                "The planned file change kind is not defined."),
        };

    private static bool MatchesFacts(
        RouteCreateResultFormation expected,
        RouteCreateResultFormation actual)
        => Equals(expected.Target, actual.Target)
            && Equals(expected.Parent, actual.Parent)
            && string.Equals(
                expected.Metadata.Description,
                actual.Metadata.Description,
                StringComparison.Ordinal)
            && string.Equals(
                expected.Metadata.Responsibility,
                actual.Metadata.Responsibility,
                StringComparison.Ordinal)
            && expected.Metadata.Tags.SequenceEqual(actual.Metadata.Tags)
            && Equals(expected.Template, actual.Template);

    private static bool MatchesOptionalFindings(
        IReadOnlyList<RouteCreateFinding> expected,
        IReadOnlyList<RouteCreateFinding> actual)
    {
        if (expected.Any(finding => finding.Code != RouteCreateFindingCode.OptionalMetadata)
            || actual.Any(finding => finding.Code != RouteCreateFindingCode.OptionalMetadata)
            || expected.Count != actual.Count)
        {
            return false;
        }

        return expected.Zip(actual).All(pair =>
            pair.First.Code == pair.Second.Code
            && string.Equals(pair.First.Target, pair.Second.Target, StringComparison.Ordinal)
            && string.Equals(pair.First.Cause, pair.Second.Cause, StringComparison.Ordinal));
    }

    private static RouteCreateAppliedVerification Cancelled()
        => new()
        {
            State = RouteCreateAppliedVerificationState.Cancelled,
            Cause = "Final Route Create verification was interrupted.",
        };

    private static RouteCreateAppliedVerification Failed(string cause)
        => new()
        {
            State = RouteCreateAppliedVerificationState.Failed,
            Cause = cause,
        };
}
