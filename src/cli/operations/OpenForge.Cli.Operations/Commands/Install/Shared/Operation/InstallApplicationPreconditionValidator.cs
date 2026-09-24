using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Operation;

internal sealed class InstallApplicationPreconditionValidator(
    MutationRevalidator mutationRevalidator,
    InstallIntendedStateBuilder intendedStateBuilder)
{
    private readonly MutationRevalidator _mutationRevalidator = mutationRevalidator;
    private readonly InstallIntendedStateBuilder _intendedStateBuilder = intendedStateBuilder;

    internal async ValueTask<InstallApplicationPreconditionResult> ValidateUnderLeaseAsync(
        WorkspaceLockLease lease,
        InstallPlan plan,
        CancellationToken cancellationToken)
    {
        var current = await _intendedStateBuilder.BuildAsync(
                plan.Request,
                plan.Payload,
                cancellationToken)
            .ConfigureAwait(false);
        var intendedBoundary = ReadIntendedBoundary(current);
        if (intendedBoundary is not null)
        {
            return intendedBoundary;
        }

        var intendedState = current.IntendedState
            ?? throw new InvalidOperationException(
                "A complete Install intended-state revalidation requires its state.");

        if (!IntendedStateEquals(plan.IntendedState, intendedState))
        {
            return InstallApplicationPreconditionResult.Boundary(
                MutationValidationResult.Blocked(
                    "Volatile authored source or topology facts changed after Install planning."),
                InstallFindingCode.TargetUnsafe);
        }

        var recovery = await RecoveryBundleCatalogue.ReadAsync(
                plan.Request.Workspace,
                cancellationToken)
            .ConfigureAwait(false);
        var recoveryBoundary = ReadRecoveryBoundary(recovery);
        if (recoveryBoundary is not null)
        {
            return recoveryBoundary;
        }

        var validation = await _mutationRevalidator.ValidateAsync(
                lease,
                plan.DirectoryCreations,
                plan.FileChanges,
                cancellationToken)
            .ConfigureAwait(false);
        return validation.State switch
        {
            MutationValidationState.Valid => InstallApplicationPreconditionResult.Valid(validation),
            MutationValidationState.Cancelled => InstallApplicationPreconditionResult.Boundary(
                validation,
                InstallFindingCode.Interrupted),
            MutationValidationState.Mismatched
                or MutationValidationState.Blocked
                or MutationValidationState.Failed => InstallApplicationPreconditionResult.Boundary(
                    validation,
                    InstallFindingCode.TargetUnsafe),
            _ => throw InvalidValidationState(validation.State),
        };
    }

    private static ArgumentOutOfRangeException InvalidValidationState(
        MutationValidationState state)
        => new(
            nameof(state),
            state,
            "The mutation validation state is not defined.");

    private static InstallApplicationPreconditionResult? ReadRecoveryBoundary(
        RecoveryBundleCatalogueResult recovery)
    {
        return recovery.State switch
        {
            RecoveryBundleCatalogueState.Available when recovery.Candidates.Length == 0 => null,
            RecoveryBundleCatalogueState.Available => InstallApplicationPreconditionResult.Boundary(
                MutationValidationResult.Blocked(
                    "Recognized Framework recovery residuals appeared after Install planning."),
                InstallFindingCode.RecoveryConflict),
            RecoveryBundleCatalogueState.Unavailable => InstallApplicationPreconditionResult.Boundary(
                MutationValidationResult.Blocked(
                    recovery.Cause
                        ?? "Framework recovery residual facts are unavailable for Install revalidation."),
                InstallFindingCode.RecoveryUnavailable),
            RecoveryBundleCatalogueState.Cancelled => InstallApplicationPreconditionResult.Boundary(
                MutationValidationResult.Cancelled(),
                InstallFindingCode.Interrupted),
            _ => throw new ArgumentOutOfRangeException(
                nameof(recovery),
                recovery.State,
                "The recovery catalogue state is not defined."),
        };
    }

    private static InstallApplicationPreconditionResult? ReadIntendedBoundary(
        InstallIntendedStateBuild current)
    {
        return current.State switch
        {
            InstallIntendedStateBuildState.Complete => null,
            InstallIntendedStateBuildState.Incomplete => InstallApplicationPreconditionResult.Boundary(
                MutationValidationResult.Blocked(
                    current.Cause
                        ?? "Volatile authored source and topology facts are unavailable for Install revalidation."),
                current.FindingCode ?? InstallFindingCode.ProjectionUnavailable),
            InstallIntendedStateBuildState.Blocked => InstallApplicationPreconditionResult.Boundary(
                MutationValidationResult.Blocked(
                    current.Cause
                        ?? "Volatile authored source or topology facts are unsafe for Install revalidation."),
                current.FindingCode ?? InstallFindingCode.GeneratedRegionUnsafe),
            InstallIntendedStateBuildState.Cancelled => InstallApplicationPreconditionResult.Boundary(
                MutationValidationResult.Cancelled(),
                current.FindingCode ?? InstallFindingCode.Interrupted),
            _ => throw new ArgumentOutOfRangeException(
                nameof(current),
                current.State,
                "The intended state build state is not defined."),
        };
    }

    private static bool IntendedStateEquals(
        InstallIntendedState planned,
        InstallIntendedState current)
        => DictionaryEquals(planned.TargetBytes, current.TargetBytes)
            && DictionaryEquals(planned.ManagedBlockBytes, current.ManagedBlockBytes)
            && planned.GeneratedRegionPaths.SetEquals(current.GeneratedRegionPaths)
            && planned.ProjectionInputs.SequenceEqual(current.ProjectionInputs);

    private static bool DictionaryEquals(
        IReadOnlyDictionary<string, byte[]> planned,
        IReadOnlyDictionary<string, byte[]> current)
    {
        if (planned.Count != current.Count)
        {
            return false;
        }

        foreach (var pair in planned)
        {
            if (!current.TryGetValue(pair.Key, out var bytes)
                || !pair.Value.AsSpan().SequenceEqual(bytes))
            {
                return false;
            }
        }

        return true;
    }
}
