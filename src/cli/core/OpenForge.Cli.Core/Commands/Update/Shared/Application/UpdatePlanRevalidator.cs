using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Application;

internal sealed class UpdatePlanRevalidator(
    UpdatePlanBuilder planBuilder,
    MutationRevalidator mutationRevalidator)
{
    private readonly UpdatePlanBuilder _planBuilder = planBuilder;
    private readonly MutationRevalidator _mutationRevalidator = mutationRevalidator;

    internal async ValueTask<UpdatePlanRevalidation> RevalidateAsync(
        UpdatePlanExecution expected,
        WorkspaceLockLease lease,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(lease);
        if (!lease.IsHeldFor(expected.Build.Preview.Workspace
                ?? throw new InvalidOperationException(
                    "A complete Update execution requires its selected workspace.")))
        {
            return Failed("Update revalidation does not hold the selected workspace lease.");
        }

        var changes = Changes(expected);
        IReadOnlyList<FileExpectationValidationResult> checks = [];
        if (changes.Count != 0)
        {
            MutationValidationResult validation;
            try
            {
                validation = await _mutationRevalidator
                    .ValidateAsync(lease, changes, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Interrupted();
            }
            catch (Exception)
            {
                return Failed("Update target expectations could not be revalidated under the lease.");
            }

            if (validation.State != MutationValidationState.Valid)
            {
                return validation.State == MutationValidationState.Cancelled
                    ? Interrupted()
                    : Changed(validation.Cause
                        ?? "An Update target changed or became unsafe before application.");
            }

            checks = validation.Checks;
        }

        UpdatePlanResolution current;
        try
        {
            current = await _planBuilder
                .BuildExecutionAsync(expected.Request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
        }
        catch (Exception)
        {
            return Failed("The complete Update plan could not be rebuilt under the workspace lease.");
        }

        if (current.Execution is not { } actual || !Matches(expected, actual))
        {
            return Changed(
                current.Build.Preview.Findings.FirstOrDefault()?.Cause
                    ?? "The complete Update plan changed before application.");
        }

        return new UpdatePlanRevalidation(
            UpdatePlanRevalidationState.Exact,
            Cause: null,
            checks);
    }

    private static IReadOnlyList<PlannedFileChange> Changes(UpdatePlanExecution execution)
        => execution.Effects
            .Select(effect => effect.FileChange)
            .Concat(execution.LifecycleChange is { } lifecycle ? [lifecycle] : [])
            .ToArray();

    private static bool Matches(UpdatePlanExecution expected, UpdatePlanExecution actual)
        => SourceMatches(expected, actual)
            && expected.Build.Preview.Mode == actual.Build.Preview.Mode
            && expected.Build.Preview.Force == actual.Build.Preview.Force
            && expected.Build.Preview.Prune == actual.Build.Preview.Prune
            && expected.Build.Preview.Automatic == actual.Build.Preview.Automatic
            && expected.Observations.Count == actual.Observations.Count
            && expected.Observations.Zip(actual.Observations).All(pair =>
                ComparisonMatches(pair.First.Comparison, pair.Second.Comparison)
                && SnapshotMatches(pair.First.Snapshot, pair.Second.Snapshot)
                && BytesMatch(pair.First.IntendedDocumentBytes, pair.Second.IntendedDocumentBytes))
            && expected.ProjectionInputs.Count == actual.ProjectionInputs.Count
            && expected.ProjectionInputs.Zip(actual.ProjectionInputs).All(pair =>
                SnapshotMatches(pair.First, pair.Second))
            && expected.Build.Plan is { } expectedPlan
            && actual.Build.Plan is { } actualPlan
            && expectedPlan.Authority == actualPlan.Authority
            && expectedPlan.Decisions.Count == actualPlan.Decisions.Count
            && expectedPlan.Decisions.Zip(actualPlan.Decisions).All(pair =>
                pair.First.Disposition == pair.Second.Disposition
                && ComparisonMatches(pair.First.Comparison, pair.Second.Comparison))
            && expected.Effects.Count == actual.Effects.Count
            && expected.Effects.Zip(actual.Effects).All(pair =>
                EffectMatches(pair.First.ResultEffect, pair.Second.ResultEffect)
                && ChangeMatches(pair.First.FileChange, pair.Second.FileChange))
            && ChangeMatches(expected.LifecycleChange, actual.LifecycleChange);

    private static bool SourceMatches(UpdatePlanExecution expected, UpdatePlanExecution actual)
        => expected.Payload.InventoryFingerprint == actual.Payload.InventoryFingerprint
            && expected.Payload.Assets.Length == actual.Payload.Assets.Length
            && expected.Payload.Assets.Zip(actual.Payload.Assets).All(pair =>
                pair.First.Path == pair.Second.Path
                && pair.First.Bytes.AsSpan().SequenceEqual(pair.Second.Bytes.AsSpan()));

    private static bool ComparisonMatches(UpdateComparison expected, UpdateComparison actual)
        => expected.RelativePath == actual.RelativePath
            && expected.Kind == actual.Kind
            && expected.RegionIdentity == actual.RegionIdentity
            && expected.SourceAssetPath == actual.SourceAssetPath
            && expected.SourceAssetPresentInCurrentInventory
                == actual.SourceAssetPresentInCurrentInventory
            && expected.FingerprintKind == actual.FingerprintKind
            && expected.BaselineFingerprint == actual.BaselineFingerprint
            && expected.CurrentFingerprint == actual.CurrentFingerprint
            && expected.IntendedFingerprint == actual.IntendedFingerprint
            && expected.CurrentState == actual.CurrentState
            && expected.IntendedState == actual.IntendedState
            && expected.RetirementEligibility == actual.RetirementEligibility
            && ByteFactsMatch(expected.CurrentBytes, actual.CurrentBytes)
            && ByteFactsMatch(expected.IntendedBytes, actual.IntendedBytes);

    private static bool ByteFactsMatch(
        UpdateComparisonByteFacts expected,
        UpdateComparisonByteFacts actual)
        => expected.Sha256 == actual.Sha256
            && (expected.ExactBytes, actual.ExactBytes) switch
            {
                (null, null) => true,
                ({ } left, { } right) => left.AsSpan().SequenceEqual(right.AsSpan()),
                _ => false,
            };

    private static bool SnapshotMatches(FileStateSnapshot expected, FileStateSnapshot actual)
        => expected.Expectation == actual.Expectation
            && expected.HasBytes == actual.HasBytes
            && expected.Bytes.AsSpan().SequenceEqual(actual.Bytes.AsSpan());

    private static bool EffectMatches(UpdatePhysicalEffect expected, UpdatePhysicalEffect actual)
        => expected.Path == actual.Path
            && expected.Action == actual.Action
            && expected.Outcome == actual.Outcome
            && expected.Residual == actual.Residual
            && expected.Changes.SequenceEqual(actual.Changes);

    private static bool ChangeMatches(PlannedFileChange? expected, PlannedFileChange? actual)
        => (expected, actual) switch
        {
            (null, null) => true,
            ({ } left, { } right) => left.Kind == right.Kind
                && left.Expectation == right.Expectation
                && left.IntendedBytes.AsSpan().SequenceEqual(right.IntendedBytes.AsSpan()),
            _ => false,
        };

    private static bool BytesMatch(byte[]? expected, byte[]? actual)
        => (expected, actual) switch
        {
            (null, null) => true,
            ({ } left, { } right) => left.AsSpan().SequenceEqual(right.AsSpan()),
            _ => false,
        };

    private static UpdatePlanRevalidation Changed(string cause)
        => new(UpdatePlanRevalidationState.Changed, cause, []);

    private static UpdatePlanRevalidation Failed(string cause)
        => new(UpdatePlanRevalidationState.Failed, cause, []);

    private static UpdatePlanRevalidation Interrupted()
        => new(
            UpdatePlanRevalidationState.Interrupted,
            "Update plan revalidation was interrupted.",
            []);
}
