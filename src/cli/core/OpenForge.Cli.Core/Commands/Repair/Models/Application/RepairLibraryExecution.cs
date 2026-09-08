using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Application;

internal sealed record RepairLibraryExecution
{
    public required ImmutableArray<FileChangeReceipt> ReferenceReceipts { get; init; }
    public required ImmutableArray<RepairLibraryRecoveryReceipt> LibraryReceipts { get; init; }
    public required RecoveryBundlePreparation? ForwardPreparation { get; init; }
    public required RecoveryBundleDeletionResult? ForwardCleanup { get; init; }
    public required DoctorDiagnosisRead? PostDiagnosis { get; init; }
    public RepairPostVerification? ReferencePostVerification { get; init; }
    public required RepairLibraryCancellation? Cancellation { get; init; }
    public required RepairLibraryUnexpectedFailure? UnexpectedFailure { get; init; }
}

internal enum RepairLibraryRecoveryKind
{
    Ordinary,
    RelativeFileLink,
}

internal sealed record RepairLibraryRecoveryReceipt
{
    internal RepairLibraryRecoveryReceipt(
        RepairLibraryRecoveryKind kind,
        RepairLibraryRecoveryEffect effect,
        RecoveryBundlePreparation originalResidual,
        OrdinaryFileRecoveryResult? ordinary,
        RelativeFileLinkRecoveryResult? relativeFileLink)
    {
        ArgumentNullException.ThrowIfNull(effect);
        ArgumentNullException.ThrowIfNull(originalResidual);
        var coherent = kind switch
        {
            RepairLibraryRecoveryKind.Ordinary => ordinary is not null && relativeFileLink is null
                && effect.Entry.Kind is RecoveryEntryKind.OrdinaryCreate or RecoveryEntryKind.OrdinaryReplace
                    or RecoveryEntryKind.OrdinaryReplaceGeneratedRegion or RecoveryEntryKind.OrdinaryDelete,
            RepairLibraryRecoveryKind.RelativeFileLink => ordinary is null && relativeFileLink is not null
                && effect.Entry.Kind is RecoveryEntryKind.RelativeFileLinkCreate or RecoveryEntryKind.RelativeFileLinkDelete,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Library recovery receipt kind is not defined."),
        };
        var evidence = effect.Selection.Proposal.Evidence;
        var verified = evidence.Residual.Candidate.Verified
            ?? throw new ArgumentException("A Library receipt requires its selected verified final.", nameof(effect));
        var context = evidence.Entry.Input.Context;
        if (!string.Equals(originalResidual.BundlePath, verified.BundlePath, StringComparison.Ordinal)
            || !string.Equals(originalResidual.WorkspaceKey, verified.WorkspaceKey, StringComparison.Ordinal)
            || !PhysicalIdentityTracker.PathComparer.Equals(originalResidual.WorkspacePhysicalPath, verified.WorkspacePhysicalPath)
            || !originalResidual.MatchesWorkspace(context.Workspace)
            || originalResidual.OperationId != verified.OperationId
            || !string.Equals(originalResidual.Command, verified.Command, StringComparison.Ordinal)
            || originalResidual.Attribution != verified.Attribution
            || !originalResidual.Entries.SequenceEqual(verified.Entries)
            || !originalResidual.Entries.Contains(effect.Entry))
        {
            throw new ArgumentException("A Library receipt must retain its exact selected original residual preparation.", nameof(originalResidual));
        }

        if (!coherent
            || ordinary is not null && (ordinary.Before.Input.Context != context
                || ordinary.After is { } after && after.Input.Context != context)
            || relativeFileLink is not null && (relativeFileLink.Entry != effect.Entry
                || !PhysicalIdentityTracker.PathComparer.Equals(relativeFileLink.Current.LogicalPath, context.LogicalPath)
                || relativeFileLink.After is { } afterLink
                    && !PhysicalIdentityTracker.PathComparer.Equals(afterLink.LogicalPath, context.LogicalPath)))
        {
            throw new ArgumentException("A Library receipt requires matching recovery result contexts for its selected entry.", nameof(effect));
        }

        Kind = kind;
        Effect = effect;
        OriginalResidual = originalResidual;
        Ordinary = ordinary;
        RelativeFileLink = relativeFileLink;
    }

    internal RepairLibraryRecoveryKind Kind { get; }
    internal RepairLibraryRecoveryEffect Effect { get; }
    internal RecoveryBundlePreparation OriginalResidual { get; }
    internal OrdinaryFileRecoveryResult? Ordinary { get; }
    internal RelativeFileLinkRecoveryResult? RelativeFileLink { get; }
}

internal sealed record RepairLibraryPreparedApplication
{
    public required RepairPlan Plan { get; init; }
    public required RepairMutationServices Services { get; init; }
    public required WorkspaceLockLease Lease { get; init; }
    public required RecoveryBundlePreparation? ForwardPreparation { get; init; }
    public required ImmutableArray<FileExpectation> ReferenceTargets { get; init; }
    public required ImmutableArray<FileExpectationValidationResult> ReferenceChecks { get; init; }
    public required ImmutableArray<RecoveryBundlePreparation> OriginalPreparations { get; init; }
}

internal enum RepairAtomicEffectKind
{
    Reference,
    LibraryRecovery,
}

internal sealed record RepairAtomicEffect
{
    internal RepairAtomicEffect(int ordinal, RepairAtomicEffectKind kind, RepairEffect? reference, RepairLibraryRecoveryEffect? libraryRecovery)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(ordinal);
        var coherent = kind switch
        {
            RepairAtomicEffectKind.Reference => reference is not null && libraryRecovery is null,
            RepairAtomicEffectKind.LibraryRecovery => reference is null && libraryRecovery is not null,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The atomic Repair effect kind is not defined."),
        };
        if (!coherent)
        {
            throw new ArgumentException("An atomic Repair effect requires exactly its selected typed payload.", nameof(kind));
        }

        Ordinal = ordinal;
        Kind = kind;
        Reference = reference;
        LibraryRecovery = libraryRecovery;
    }

    internal int Ordinal { get; }
    internal RepairAtomicEffectKind Kind { get; }
    internal RepairEffect? Reference { get; }
    internal RepairLibraryRecoveryEffect? LibraryRecovery { get; }
}

internal enum RepairLibraryExecutionStage
{
    Lease,
    Revalidation,
    ForwardPreparation,
    Effect,
    Verification,
    ForwardCleanup,
    PostDiagnosis,
}

internal sealed record RepairLibraryCancellation(RepairLibraryExecutionStage Stage, int? EffectOrdinal);

internal sealed record RepairLibraryUnexpectedFailure(RepairLibraryExecutionStage Stage, int? EffectOrdinal, string Cause);
