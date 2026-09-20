using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;

internal enum RouteInitAppliedVerificationState
{
    Verified,
    Failed,
    Cancelled,
}

internal sealed record RouteInitAppliedVerification(
    RouteInitAppliedVerificationState State,
    string? Cause);

internal sealed class RouteInitAppliedVerifier(
    RouteInitPlanBuilder planBuilder,
    FileExpectationValidator expectationValidator)
{
    private readonly RouteInitPlanBuilder _planBuilder = planBuilder;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal async ValueTask<RouteInitAppliedVerification> VerifyAsync(
        RouteInitPlan original,
        WorkspaceLockLease lease,
        IReadOnlyList<DirectoryCreationReceipt> directoryReceipts,
        IReadOnlyList<FileChangeReceipt> fileReceipts,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(directoryReceipts);
        ArgumentNullException.ThrowIfNull(fileReceipts);
        if (!lease.IsHeldFor(original.Request.Workspace))
        {
            return Failed("The final Route Init verification does not hold the selected workspace lease.");
        }

        RouteInitAppliedVerification? postconditions;
        try
        {
            postconditions = await VerifyPostconditionsAsync(
                    original,
                    directoryReceipts,
                    fileReceipts,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception)
        {
            return Failed("Exact Route Init postcondition verification failed unexpectedly.");
        }

        if (postconditions is not null)
        {
            return postconditions;
        }

        var request = new RouteInitRequest(
            original.Request.Workspace,
            original.Request.RouteTarget,
            original.Request.Scaffold,
            original.Request.Mode,
            RouteInitMetadataInput.None);
        RouteInitPlanBuild fresh;
        try
        {
            fresh = await _planBuilder.BuildAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception)
        {
            return Failed("The final Route Init plan could not be rebuilt after application.");
        }

        if (fresh.Formation.Findings.Any(finding => finding.Code == RouteInitFindingCode.Interrupted))
        {
            return Cancelled();
        }

        if (fresh.Plan is not { } plan)
        {
            return Failed(
                fresh.Formation.Findings.FirstOrDefault()?.Cause
                    ?? "The final Route Init state could not form a complete verification plan.");
        }

        if (!plan.IsNoOp)
        {
            return Failed(
                $"The final Route Init state still requires {plan.DirectoryCreations.Length} directory, "
                    + $"{plan.FileChanges.Length} file changes.");
        }

        if (fresh.Formation.Plan is not
            {
                Completeness: RouteInitPlanCompleteness.Complete,
                Safety: RouteInitPlanSafety.Safe,
            }
            || fresh.Formation.Effects.Length != 0
            || fresh.Formation.Findings.Length != 0)
        {
            return Failed("The final Route Init state did not rebuild as one complete safe no-op.");
        }

        if (!MatchesRequest(original, plan)
            || !Equals(original.Preview.Target, fresh.Formation.Target)
            || !MatchesFramework(original.Preview.Framework, fresh.Formation.Framework)
            || !MatchesEntrypoints(original.Preview.Entrypoints, fresh.Formation.Entrypoints,
                original.Preview.Lifecycle.Action != RouteInitLifecycleAction.None)
            || !MatchesLifecycle(original.Preview, fresh.Formation))
        {
            return Failed("The final Route Init identity, topology, scaffold, or lifecycle facts changed after application.");
        }

        return new RouteInitAppliedVerification(RouteInitAppliedVerificationState.Verified, Cause: null);
    }

    private async ValueTask<RouteInitAppliedVerification?> VerifyPostconditionsAsync(
        RouteInitPlan plan,
        IReadOnlyList<DirectoryCreationReceipt> directoryReceipts,
        IReadOnlyList<FileChangeReceipt> fileReceipts,
        CancellationToken cancellationToken)
    {
        if (directoryReceipts.Count != plan.DirectoryCreations.Length
            || fileReceipts.Count != plan.FileChanges.Length)
        {
            return Failed("Final Route Init verification requires one verified receipt per planned effect.");
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
                })
            {
                return Failed("A planned Route Init directory has no exact verified receipt.");
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

        for (var index = 0; index < fileReceipts.Count; index++)
        {
            var receipt = fileReceipts[index];
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
                return Failed("A planned Route Init file has no exact intended verified receipt.");
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

    private async ValueTask<RouteInitAppliedVerification?> VerifyExpectationAsync(
        RouteInitPlan plan,
        FileExpectation expectation,
        CancellationToken cancellationToken)
    {
        var validation = await _expectationValidator.ValidateAsync(
                plan.Request.Workspace,
                expectation,
                cancellationToken)
            .ConfigureAwait(false);
        return ReadExpectationBoundary(validation.State, validation.Cause);
    }

    internal static RouteInitAppliedVerification? ReadExpectationBoundary(
        FileExpectationValidationState state,
        string? cause)
        => state switch
        {
            FileExpectationValidationState.Matched => null,
            FileExpectationValidationState.Mismatched
                or FileExpectationValidationState.Blocked
                or FileExpectationValidationState.Failed => Failed(
                    cause
                        ?? "An applied Route Init postcondition changed before final verification."),
            FileExpectationValidationState.Cancelled => Cancelled(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The file expectation validation state is not defined."),
        };

    private static bool Matches(PlannedFileChange expected, PlannedFileChange actual)
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
                or PlannedFileChangeKind.ReplaceGeneratedRegion => after.Kind == FileExpectationKind.File
                    && after.HasBytes
                    && after.Bytes.AsSpan().SequenceEqual(change.IntendedBytes.AsSpan()),
            PlannedFileChangeKind.Delete => after.Kind == FileExpectationKind.Missing,
            _ => throw new ArgumentOutOfRangeException(
                nameof(change),
                change.Kind,
                "The planned file change kind is not defined."),
        };

    private static bool MatchesRequest(RouteInitPlan expected, RouteInitPlan actual)
        => ReferenceEquals(expected.Request.Workspace, actual.Request.Workspace)
            && string.Equals(
                expected.Request.RouteTarget,
                actual.Request.RouteTarget,
                StringComparison.Ordinal)
            && expected.Request.Scaffold == actual.Request.Scaffold
            && expected.Request.Mode == actual.Request.Mode;

    private static bool MatchesFramework(
        RouteInitFramework? expected,
        RouteInitFramework? actual)
    {
        if (expected is null || actual is null)
        {
            return expected is null && actual is null;
        }

        return string.Equals(
                expected.InventoryFingerprint,
                actual.InventoryFingerprint,
                StringComparison.Ordinal)
            && expected.Segments.SequenceEqual(actual.Segments);
    }

    private static bool MatchesEntrypoints(
        IReadOnlyList<RouteInitEntrypoint> expected,
        IReadOnlyList<RouteInitEntrypoint> actual,
        bool verifyRecordedOwnership)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            var before = expected[index];
            var after = actual[index];
            if (!string.Equals(before.Id, after.Id, StringComparison.Ordinal)
                || !string.Equals(before.Path, after.Path, StringComparison.Ordinal)
                || before.Form != after.Form
                || after.Current != RouteInitEntrypointCurrent.Existing
                || verifyRecordedOwnership && (before.Ownership != after.Ownership
                    || !string.Equals(before.SourceAssetPath, after.SourceAssetPath, StringComparison.Ordinal))
                || after.Outcome != RouteInitEntrypointOutcome.Unchanged)
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesLifecycle(
        RouteInitResultFormation expected,
        RouteInitResultFormation actual)
        => expected.Lifecycle.Action == RouteInitLifecycleAction.None
            ? actual.Lifecycle == new RouteInitLifecycle(
                RouteInitLifecycleAction.None,
                RouteInitLifecycleOutcome.NotRequested)
            : actual.Lifecycle == new RouteInitLifecycle(
                RouteInitLifecycleAction.Preserve,
                RouteInitLifecycleOutcome.AlreadyCurrent);

    private static RouteInitAppliedVerification Cancelled()
        => new(RouteInitAppliedVerificationState.Cancelled, "Final Route Init verification was interrupted.");

    private static RouteInitAppliedVerification Failed(string cause)
        => new(RouteInitAppliedVerificationState.Failed, cause);
}
