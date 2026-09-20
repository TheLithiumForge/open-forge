using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Index;

internal static class IndexDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "index";

    internal static readonly CliSyntaxDefinition IndexCommand = new(
        CommandIdentity,
        "Rebuild generated Entries for selected routes.");

    internal static readonly CliSyntaxDefinition Sources = new(
        "source-reference",
        "Select zero or more source IDs or exact .agents/... paths.");

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        "--dry-run",
        "Preview all generated Entries changes without writing files.",
        CliOptionArity.None,
        false,
        null);

    internal static readonly IReadOnlyList<IndexFindingCode> FindingCodes =
        Array.AsReadOnly(Enum.GetValues<IndexFindingCode>());

    internal static IndexFindingDefinition Read(IndexFindingCode code)
    {
        return code switch
        {
            IndexFindingCode.InvalidInput => Definition(code, "index.invalid-input", CliSemanticStatus.Invalid),
            IndexFindingCode.InvalidSource => Definition(code, "index.invalid-source", CliSemanticStatus.Invalid),
            IndexFindingCode.WorkspaceUnavailable => Definition(code, "index.workspace-unavailable", CliSemanticStatus.Blocked),
            IndexFindingCode.WorkspaceUnsafe => Definition(code, "index.workspace-unsafe", CliSemanticStatus.Blocked),
            IndexFindingCode.SourceAmbiguous => Definition(code, "index.source-ambiguous", CliSemanticStatus.Blocked),
            IndexFindingCode.SourceUnsafe => Definition(code, "index.source-unsafe", CliSemanticStatus.Blocked),
            IndexFindingCode.TopologyAmbiguous => Definition(code, "index.topology-ambiguous", CliSemanticStatus.Blocked),
            IndexFindingCode.TargetUnexposed => Definition(code, "index.target-unexposed", CliSemanticStatus.Blocked),
            IndexFindingCode.TargetUnsafe => Definition(code, "index.target-unsafe", CliSemanticStatus.Blocked),
            IndexFindingCode.MetadataUnsafe => Definition(code, "index.metadata-unsafe", CliSemanticStatus.Blocked),
            IndexFindingCode.MetadataOptional => Definition(code, "index.metadata-optional", CliSemanticStatus.Attention),
            IndexFindingCode.GeneratedRegionUnsafe => Definition(code, "index.generated-region-unsafe", CliSemanticStatus.Blocked),
            IndexFindingCode.WorkspaceLockUnavailable => Definition(code, "index.workspace-lock-unavailable", CliSemanticStatus.Blocked),
            IndexFindingCode.TargetChanged => Definition(code, "index.target-changed", CliSemanticStatus.Blocked),
            IndexFindingCode.RecoveryConflict => Definition(code, "index.recovery-conflict", CliSemanticStatus.Blocked),
            IndexFindingCode.DiscoveryIncomplete => Definition(code, "index.discovery-incomplete", CliSemanticStatus.Incomplete),
            IndexFindingCode.MetadataIncomplete => Definition(code, "index.metadata-incomplete", CliSemanticStatus.Incomplete),
            IndexFindingCode.MetadataSkipped => Definition(code, "index.metadata-skipped", CliSemanticStatus.Incomplete),
            IndexFindingCode.ProjectionIncomplete => Definition(code, "index.projection-incomplete", CliSemanticStatus.Incomplete),
            IndexFindingCode.RecoveryUnavailable => Definition(code, "index.recovery-unavailable", CliSemanticStatus.Incomplete),
            IndexFindingCode.RecoveryArtifactRetained => Definition(code, "index.recovery-artifact-retained", CliSemanticStatus.Attention),
            IndexFindingCode.TargetChangedDuringApply => Definition(code, "index.target-changed-during-apply", CliSemanticStatus.Failed),
            IndexFindingCode.WriteFailed => Definition(code, "index.write-failed", CliSemanticStatus.Failed),
            IndexFindingCode.VerificationFailed => Definition(code, "index.verification-failed", CliSemanticStatus.Failed),
            IndexFindingCode.RecoveryFailed => Definition(code, "index.recovery-failed", CliSemanticStatus.Failed),
            IndexFindingCode.OperationFailed => Definition(code, "index.operation-failed", CliSemanticStatus.Failed),
            IndexFindingCode.Interrupted => Definition(code, "index.interrupted", CliSemanticStatus.Interrupted),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Index finding code is not defined."),
        };
    }

    internal static string ReadMachineName(IndexFindingCode code) => Read(code).MachineName;

    internal static string ReadMachineName(IndexMode mode)
        => mode switch
        {
            IndexMode.Apply => "apply",
            IndexMode.DryRun => "dry-run",
            _ => throw Undefined(nameof(mode), mode),
        };

    internal static string ReadMachineName(IndexSelectionOrigin origin)
        => origin switch
        {
            IndexSelectionOrigin.AutomaticLoader => "automatic-loader",
            IndexSelectionOrigin.ExplicitSources => "explicit-sources",
            _ => throw Undefined(nameof(origin), origin),
        };

    internal static string ReadMachineName(IndexSelectionScope scope)
        => scope switch
        {
            IndexSelectionScope.NotEstablished => "not-established",
            IndexSelectionScope.Rooted => "rooted",
            IndexSelectionScope.Detached => "detached",
            IndexSelectionScope.Mixed => "mixed",
            _ => throw Undefined(nameof(scope), scope),
        };

    internal static string ReadMachineName(IndexLogicalSourceScope scope)
        => scope switch
        {
            IndexLogicalSourceScope.Rooted => "rooted",
            IndexLogicalSourceScope.Detached => "detached",
            _ => throw Undefined(nameof(scope), scope),
        };

    internal static string ReadMachineName(IndexRegionAction action)
        => action switch
        {
            IndexRegionAction.NotEstablished => "not-established",
            IndexRegionAction.Unchanged => "unchanged",
            IndexRegionAction.Update => "update",
            _ => throw Undefined(nameof(action), action),
        };

    internal static string ReadMachineName(IndexRegionOutcome outcome)
        => outcome switch
        {
            IndexRegionOutcome.NotEstablished => "not-established",
            IndexRegionOutcome.AlreadyCurrent => "already-current",
            IndexRegionOutcome.NotRequested => "not-requested",
            IndexRegionOutcome.NotStarted => "not-started",
            IndexRegionOutcome.Applied => "applied",
            IndexRegionOutcome.Verified => "verified",
            IndexRegionOutcome.Unknown => "unknown",
            _ => throw Undefined(nameof(outcome), outcome),
        };

    internal static string ReadMachineName(IndexRecoveryState state)
        => state switch
        {
            IndexRecoveryState.NotRequired => "not-required",
            IndexRecoveryState.NotCreated => "not-created",
            IndexRecoveryState.Removed => "removed",
            IndexRecoveryState.Retained => "retained",
            IndexRecoveryState.Unknown => "unknown",
            _ => throw Undefined(nameof(state), state),
        };

    internal static CliSemanticStatus ReadStatus(IndexFindingCode code) => Read(code).Status;

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<IndexFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        if (status == CliSemanticStatus.Complete)
        {
            return null;
        }

        return status switch
        {
            CliSemanticStatus.Invalid => new CliNextAction(
                "open-forge index --help",
                "Correct the named Index input, then rerun the request."),
            CliSemanticStatus.Blocked when FirstForStatus(findings, status) == IndexFindingCode.SourceAmbiguous => new CliNextAction(
                CommandLines.Index,
                "Replace every ambiguous source with one listed exact path, then rerun the same Index request."),
            CliSemanticStatus.Blocked when FirstForStatus(findings, status) == IndexFindingCode.WorkspaceLockUnavailable => new CliNextAction(
                CommandLines.Index,
                "Wait for the workspace lock to become available or inspect lock availability, then rerun Index from a fresh plan."),
            CliSemanticStatus.Blocked when FirstForStatus(findings, status) == IndexFindingCode.TargetChanged => new CliNextAction(
                CommandLines.Index,
                "Inspect the changed target, then rerun Index from a fresh plan."),
            CliSemanticStatus.Blocked => new CliNextAction(
                CommandLines.Doctor,
                "Inspect the blocked workspace, topology, metadata, generated-region, or recovery boundary before rerunning Index."),
            CliSemanticStatus.Incomplete => new CliNextAction(
                CommandLines.Doctor,
                "Inspect the unavailable discovery, metadata, projection, or recovery facts before relying on this Index result."),
            CliSemanticStatus.Attention when HasOnlyOptionalFindings(findings) => null,
            CliSemanticStatus.Attention => new CliNextAction(
                CommandLines.Cleanup,
                "Review and remove the reported recovery artifact after confirming the verified Index result."),
            CliSemanticStatus.Failed => new CliNextAction(
                "open-forge index --detail debug",
                "Report the failure and retry the same Index request with bounded diagnostics."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                CommandLines.Index,
                "Rerun the same Index request."),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Index status is not defined."),
        };
    }

    internal static bool HasOnlyOptionalFindings(IReadOnlyList<IndexFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        return findings.Count > 0
            && findings.All(finding => finding.Code == IndexFindingCode.MetadataOptional);
    }

    private static IndexFindingCode? FirstForStatus(
        IReadOnlyList<IndexFinding> findings,
        CliSemanticStatus status)
        => findings.FirstOrDefault(finding => finding.Status == status)?.Code;

    private static IndexFindingDefinition Definition(
        IndexFindingCode code,
        string machineName,
        CliSemanticStatus status)
        => new(code, machineName, status);

    private static ArgumentOutOfRangeException Undefined<T>(string name, T value)
        where T : struct, Enum
        => new(name, value, $"The Index {name} value is not defined.");
}

internal sealed record IndexFindingDefinition(
    IndexFindingCode Code,
    string MachineName,
    CliSemanticStatus Status);
