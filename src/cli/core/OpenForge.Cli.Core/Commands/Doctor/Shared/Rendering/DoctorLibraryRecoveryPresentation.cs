using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorLibraryRecoveryPresentation
{
    internal static DoctorJsonLibraryRecovery Project(LibraryResidualEvidence proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        var verified = proposal.Residual.Candidate.Verified
            ?? throw new ArgumentException("A Library recovery projection requires an exact verified final.", nameof(proposal));
        var entry = proposal.Entry.Input.Context.Entry;
        return new DoctorJsonLibraryRecovery
        {
            LibraryId = proposal.LibraryId.Value,
            BundlePath = proposal.Residual.Candidate.Path,
            Attribution = RecoveryBundleAttributionCodec.Serialize(verified.Attribution),
            EntryOrdinal = entry.Ordinal,
            LogicalPath = proposal.Entry.Input.Context.LogicalPath,
            EntryKind = EntryKind(entry.Kind),
            CurrentRecordState = RecordState(proposal.CurrentRecord.State),
            VerifiedPriorRecord = proposal.VerifiedPriorRecord is not null,
            Prior = State(entry.Prior),
            Intended = State(entry.Intended),
            Observed = proposal.Entry.Observed is { } observed ? State(observed) : null,
            Comparison = Comparison(proposal.Entry.State),
        };
    }

    internal static string StateIdentity(RecoveryEntryState state)
        => JsonSerializer.Serialize(
            State(state),
            RecoveryBundleJsonContext.Default.RecoveryBundleManifestStateV1);

    internal static void Append(StringBuilder builder, LibraryResidualEvidence proposal)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(proposal);
        var projection = Project(proposal);
        builder.AppendLine($"      library: {DoctorHumanRenderer.Text(projection.LibraryId)}");
        builder.AppendLine($"      residual: {DoctorHumanRenderer.Text(projection.BundlePath)}");
        builder.AppendLine($"      entry: {projection.EntryOrdinal} {projection.EntryKind} {DoctorHumanRenderer.Text(projection.LogicalPath)}");
        builder.AppendLine($"      comparison: {projection.Comparison}");
    }

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
            RecoveryEntryStateKind.OrdinaryFile => new RecoveryBundleManifestStateV1
            {
                Kind = "ordinary-file",
                Length = state.OrdinaryFile!.Length,
                Sha256 = state.OrdinaryFile.Sha256,
                LinkKind = null,
                RawRelativeTarget = null,
            },
            RecoveryEntryStateKind.RelativeFileLink => new RecoveryBundleManifestStateV1
            {
                Kind = "relative-file-link",
                Length = null,
                Sha256 = null,
                LinkKind = LinkKind(state.RelativeFileLink!.LinkKind),
                RawRelativeTarget = state.RelativeFileLink.RawRelativeTarget,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(state), state.Kind, "The recovery state kind is not defined."),
        };

    private static string EntryKind(RecoveryEntryKind kind)
        => kind switch
        {
            RecoveryEntryKind.OrdinaryCreate => "ordinary-create",
            RecoveryEntryKind.OrdinaryReplace => "ordinary-replace",
            RecoveryEntryKind.OrdinaryReplaceGeneratedRegion => "ordinary-replace-generated-region",
            RecoveryEntryKind.OrdinaryDelete => "ordinary-delete",
            RecoveryEntryKind.RelativeFileLinkCreate => "relative-file-link-create",
            RecoveryEntryKind.RelativeFileLinkDelete => "relative-file-link-delete",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The recovery entry kind is not defined."),
        };

    private static string RecordState(LibrariesRecordReadState state)
        => state switch
        {
            LibrariesRecordReadState.Missing => "missing",
            LibrariesRecordReadState.Complete => "complete",
            LibrariesRecordReadState.Malformed => "malformed",
            LibrariesRecordReadState.Unavailable => "unavailable",
            LibrariesRecordReadState.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library record state is not defined."),
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

    private static string LinkKind(NoFollowLinkKind kind)
        => kind switch
        {
            NoFollowLinkKind.SymbolicLink => "relative-file-symbolic-link",
            NoFollowLinkKind.Junction or NoFollowLinkKind.Other => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "A recovery relative-file-link state requires a symbolic link."),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The link kind is not defined."),
        };
}
