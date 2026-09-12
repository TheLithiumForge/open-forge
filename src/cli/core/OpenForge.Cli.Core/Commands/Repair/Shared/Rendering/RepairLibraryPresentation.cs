using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Presentation;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Rendering;

internal static class RepairLibraryPresentation
{
    internal static RepairJsonLibraryProposal Proposal(RepairLibraryRecoveryProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        var projected = DoctorLibraryRecoveryPresentation.Project(proposal.Evidence);
        return new RepairJsonLibraryProposal
        {
            LibraryId = projected.LibraryId,
            BundlePath = projected.BundlePath,
            Attribution = projected.Attribution,
            EntryOrdinal = projected.EntryOrdinal,
            LogicalPath = projected.LogicalPath,
            EntryKind = projected.EntryKind,
            CurrentRecordState = projected.CurrentRecordState,
            VerifiedPriorRecord = projected.VerifiedPriorRecord,
            Prior = projected.Prior,
            Intended = projected.Intended,
            Observed = projected.Observed,
            Comparison = projected.Comparison,
        };
    }

    internal static RepairJsonSelectedLibrary Selected(RepairSelectedLibraryRecovery selected)
        => new()
        {
            Proposal = Proposal(selected.Proposal),
            Origins = [.. selected.Origins.Select(RepairDefinitions.ReadMachineName)],
        };

    internal static RepairJsonLibraryStep Step(RepairLibraryRecoveryStep step)
        => new()
        {
            Ordinal = step.Ordinal,
            Selection = Selected(step.Selection),
            Dependencies = [.. step.Dependency.Domains.Select(RepairDefinitions.ReadMachineName)],
            Verification = [.. step.Verification.Kinds.Select(RepairDefinitions.ReadMachineName)],
            HasEffect = step.Effect is not null,
            Outcome = RepairDefinitions.ReadMachineName(step.Outcome),
        };

    internal static RepairJsonLibraryExecution Execution(RepairLibraryExecution execution)
    {
        ArgumentNullException.ThrowIfNull(execution);
        return new RepairJsonLibraryExecution
        {
            Receipts = [.. execution.LibraryReceipts.Select(Receipt)],
            ForwardBundlePath = execution.ForwardPreparation?.BundlePath,
            ForwardCleanupState = execution.ForwardCleanup is { } cleanup
                ? DeletionState(cleanup.State)
                : null,
            ForwardResidualPaths = execution.ForwardCleanup is { } deletion
                ? deletion.ResidualPath is { } residualPath ? [residualPath] : []
                : null,
            Cancellation = execution.Cancellation is { } cancellation
                ? new RepairJsonLibraryInterruption(
                    Stage(cancellation.Stage),
                    cancellation.EffectOrdinal,
                    Cause: null)
                : null,
            UnexpectedFailure = execution.UnexpectedFailure is { } failure
                ? new RepairJsonLibraryInterruption(
                    Stage(failure.Stage),
                    failure.EffectOrdinal,
                    failure.Cause)
                : null,
        };
    }

    internal static void Append(StringBuilder builder, RepairResult result)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(result);
        var selection = result.Selection?.Libraries;
        builder.AppendLine("Library residual recovery:");
        builder.AppendLine($"  Selected: {selection?.Selected.Length ?? 0}");
        builder.AppendLine($"  Unselected: {selection?.Unselected.Length ?? 0}");
        if (result.LibraryExecution is { } execution)
        {
            builder.AppendLine($"  Receipts: {execution.LibraryReceipts.Length}");
            builder.AppendLine($"  Forward recovery: {execution.ForwardPreparation?.BundlePath ?? "not required"}");
            builder.AppendLine($"  Forward cleanup: {(execution.ForwardCleanup is { } cleanup ? DeletionState(cleanup.State) : "not requested")}");
            foreach (var receipt in execution.LibraryReceipts)
            {
                var projection = Receipt(receipt);
                builder.AppendLine($"  {projection.Proposal.LibraryId}: {projection.Proposal.EntryKind} {projection.Verification}");
                builder.AppendLine($"    original residual: {projection.OriginalBundlePath}");
            }
        }
    }

    private static RepairJsonLibraryReceipt Receipt(RepairLibraryRecoveryReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        return receipt.Kind switch
        {
            RepairLibraryRecoveryKind.Ordinary when receipt.Ordinary is { } ordinary => new RepairJsonLibraryReceipt
            {
                Proposal = Proposal(receipt.Effect.Selection.Proposal),
                OriginalBundlePath = receipt.OriginalResidual.BundlePath,
                Effect = Effect(ordinary.Effect),
                Verification = Verification(ordinary.Verification),
                After = ordinary.After?.Observed is { } observed ? State(observed) : null,
                AfterComparison = ordinary.After is { } after ? Comparison(after.State) : null,
                Cause = ordinary.Cause,
            },
            RepairLibraryRecoveryKind.RelativeFileLink when receipt.RelativeFileLink is { } link => new RepairJsonLibraryReceipt
            {
                Proposal = Proposal(receipt.Effect.Selection.Proposal),
                OriginalBundlePath = receipt.OriginalResidual.BundlePath,
                Effect = link.State == RelativeFileLinkRecoveryState.Restored ? "applied" : "not-started",
                Verification = LinkVerification(link.State),
                After = LinkAfter(link.After),
                AfterComparison = link.State == RelativeFileLinkRecoveryState.Restored ? "prior" : null,
                Cause = link.Cause,
            },
            RepairLibraryRecoveryKind.Ordinary or RepairLibraryRecoveryKind.RelativeFileLink =>
                throw new ArgumentException("A Library recovery receipt requires its typed result.", nameof(receipt)),
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.Kind, "The Library recovery kind is not defined."),
        };
    }

    private static RecoveryBundleManifestStateV1? LinkAfter(NoFollowLeafObservation? after)
        => after?.State switch
        {
            NoFollowLeafState.Missing => State(RecoveryEntryState.Missing),
            NoFollowLeafState.RelativeFileLink when after.RelativeFileLink is { } link =>
                State(RecoveryEntryState.RelativeLink(link)),
            null => null,
            _ => null,
        };

    private static RecoveryBundleManifestStateV1 State(RecoveryEntryState state)
        => state.Kind switch
        {
            RecoveryEntryStateKind.Missing => new RecoveryBundleManifestStateV1
            {
                Kind = "missing",
                Length = null,
                Sha256 = null,
                LinkKind = null,
                RawRelativeTarget = null,
            },
            RecoveryEntryStateKind.OrdinaryFile when state.OrdinaryFile is { } ordinary => new RecoveryBundleManifestStateV1
            {
                Kind = "ordinary-file",
                Length = ordinary.Length,
                Sha256 = ordinary.Sha256,
                LinkKind = null,
                RawRelativeTarget = null,
            },
            RecoveryEntryStateKind.RelativeFileLink when state.RelativeFileLink is { } link => new RecoveryBundleManifestStateV1
            {
                Kind = "relative-file-link",
                Length = null,
                Sha256 = null,
                LinkKind = LinkKind(link.LinkKind),
                RawRelativeTarget = link.RawRelativeTarget,
            },
            RecoveryEntryStateKind.OrdinaryFile or RecoveryEntryStateKind.RelativeFileLink =>
                throw new ArgumentException("A recovery state requires its typed identity.", nameof(state)),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state.Kind, "The recovery state kind is not defined."),
        };

    private static string Effect(FilesystemEffectState state)
        => state switch
        {
            FilesystemEffectState.NotStarted => "not-started",
            FilesystemEffectState.Applied => "applied",
            FilesystemEffectState.Unknown => "unknown",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The filesystem effect state is not defined."),
        };

    private static string Verification(FilesystemVerificationState state)
        => state switch
        {
            FilesystemVerificationState.NotStarted => "not-started",
            FilesystemVerificationState.Verified => "verified",
            FilesystemVerificationState.Failed => "failed",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The filesystem verification state is not defined."),
        };

    private static string LinkVerification(RelativeFileLinkRecoveryState state)
        => state switch
        {
            RelativeFileLinkRecoveryState.Restored => "verified",
            RelativeFileLinkRecoveryState.Mismatched or RelativeFileLinkRecoveryState.Blocked
                or RelativeFileLinkRecoveryState.Failed => "failed",
            RelativeFileLinkRecoveryState.Cancelled => "not-started",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The relative link recovery state is not defined."),
        };

    private static string Comparison(RecoveryBundleTargetComparisonState state)
        => state switch
        {
            RecoveryBundleTargetComparisonState.Prior => "prior",
            RecoveryBundleTargetComparisonState.Intended => "intended",
            RecoveryBundleTargetComparisonState.Third => "third",
            RecoveryBundleTargetComparisonState.Unavailable => "unavailable",
            RecoveryBundleTargetComparisonState.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The recovery comparison state is not defined."),
        };

    private static string Stage(RepairLibraryExecutionStage stage)
        => stage switch
        {
            RepairLibraryExecutionStage.Lease => "lease",
            RepairLibraryExecutionStage.Revalidation => "revalidation",
            RepairLibraryExecutionStage.ForwardPreparation => "forward-preparation",
            RepairLibraryExecutionStage.Effect => "effect",
            RepairLibraryExecutionStage.Verification => "verification",
            RepairLibraryExecutionStage.ForwardCleanup => "forward-cleanup",
            RepairLibraryExecutionStage.PostDiagnosis => "post-diagnosis",
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, "The Repair Library execution stage is not defined."),
        };

    private static string DeletionState(RecoveryBundleDeletionState state)
        => state switch
        {
            RecoveryBundleDeletionState.Deleted => "removed",
            RecoveryBundleDeletionState.Failed => "failed",
            RecoveryBundleDeletionState.Blocked => "blocked",
            RecoveryBundleDeletionState.Cancelled => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The recovery deletion state is not defined."),
        };

    private static string LinkKind(NoFollowLinkKind kind)
        => kind switch
        {
            NoFollowLinkKind.SymbolicLink => "relative-file-symbolic-link",
            NoFollowLinkKind.Junction or NoFollowLinkKind.Other => throw new ArgumentOutOfRangeException(
                nameof(kind), kind, "A recovery relative-file-link state requires a symbolic link."),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The link kind is not defined."),
        };
}
