using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Init;

internal static partial class RouteInitDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "route init";

    internal static readonly CliSyntaxDefinition InitCommand = new(
        "init",
        "Initialize every missing entrypoint in one exact route chain.");

    internal static readonly CliSyntaxDefinition RouteTarget = new(
        "route-target",
        "Select one exact route ID or .agents entrypoint path.");

    internal static readonly CliOptionDefinition<bool> Framework = new(
        "--framework",
        "Use the trusted embedded Framework topology and managed entrypoint assets.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<string?> Description = new(
        "--description",
        "Set the missing generic final target description.",
        CliOptionArity.ExactlyOne,
        null,
        "text");

    internal static readonly CliOptionDefinition<string?> Responsibility = new(
        "--responsibility",
        "Set or explicitly omit the missing generic final target responsibility.",
        CliOptionArity.ExactlyOne,
        null,
        "text");

    internal static readonly CliOptionDefinition<string[]> Tag = new(
        "--tag",
        "Add one ordered tag to the missing generic final target.",
        CliOptionArity.ExactlyOne,
        [],
        "tag");

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        "--dry-run",
        "Preview the complete initialization without writing files.",
        CliOptionArity.None,
        false);

    internal static readonly IReadOnlyList<RouteInitFindingCode> FindingCodes =
        Array.AsReadOnly(Enum.GetValues<RouteInitFindingCode>());

    internal static string ReadMachineName(RouteInitFindingCode code)
        => code switch
        {
            RouteInitFindingCode.InvalidInput => "route-init.invalid-input",
            RouteInitFindingCode.InvalidTarget => "route-init.invalid-target",
            RouteInitFindingCode.InvalidMetadata => "route-init.invalid-metadata",
            RouteInitFindingCode.WorkspaceUnavailable => "route-init.workspace-unavailable",
            RouteInitFindingCode.WorkspaceUnsafe => "route-init.workspace-unsafe",
            RouteInitFindingCode.TargetUnsafe => "route-init.target-unsafe",
            RouteInitFindingCode.RouteAmbiguous => "route-init.route-ambiguous",
            RouteInitFindingCode.IdentityCollision => "route-init.identity-collision",
            RouteInitFindingCode.LoaderUnsafe => "route-init.loader-unsafe",
            RouteInitFindingCode.FrameworkPayloadInvalid => "route-init.framework-payload-invalid",
            RouteInitFindingCode.FrameworkInstallRequired => "route-init.framework-install-required",
            RouteInitFindingCode.FrameworkUpdateRequired => "route-init.framework-update-required",
            RouteInitFindingCode.FrameworkAlignmentBlocked => "route-init.framework-alignment-blocked",
            RouteInitFindingCode.MetadataUnsafe => "route-init.metadata-unsafe",
            RouteInitFindingCode.GeneratedRegionUnsafe => "route-init.generated-region-unsafe",
            RouteInitFindingCode.LifecycleBlocked => "route-init.lifecycle-blocked",
            RouteInitFindingCode.WorkspaceLockUnavailable => "route-init.workspace-lock-unavailable",
            RouteInitFindingCode.TargetChanged => "route-init.target-changed",
            RouteInitFindingCode.RecoveryConflict => "route-init.recovery-conflict",
            RouteInitFindingCode.FrameworkPayloadUnavailable => "route-init.framework-payload-unavailable",
            RouteInitFindingCode.InspectionIncomplete => "route-init.inspection-incomplete",
            RouteInitFindingCode.MetadataIncomplete => "route-init.metadata-incomplete",
            RouteInitFindingCode.ProjectionIncomplete => "route-init.projection-incomplete",
            RouteInitFindingCode.LifecycleUnavailable => "route-init.lifecycle-unavailable",
            RouteInitFindingCode.RecoveryUnavailable => "route-init.recovery-unavailable",
            RouteInitFindingCode.NeedsAuthoring => "route-init.needs-authoring",
            RouteInitFindingCode.RecoveryArtifactRetained => "route-init.recovery-artifact-retained",
            RouteInitFindingCode.TargetChangedDuringApply => "route-init.target-changed-during-apply",
            RouteInitFindingCode.WriteFailed => "route-init.write-failed",
            RouteInitFindingCode.VerificationFailed => "route-init.verification-failed",
            RouteInitFindingCode.LifecyclePublicationFailed => "route-init.lifecycle-publication-failed",
            RouteInitFindingCode.RecoveryFailed => "route-init.recovery-failed",
            RouteInitFindingCode.OperationFailed => "route-init.operation-failed",
            RouteInitFindingCode.Interrupted => "route-init.interrupted",
            _ => Undefined(nameof(code), code),
        };

    internal static CliSemanticStatus ReadStatus(RouteInitFindingCode code)
        => code switch
        {
            RouteInitFindingCode.InvalidInput
                or RouteInitFindingCode.InvalidTarget
                or RouteInitFindingCode.InvalidMetadata => CliSemanticStatus.Invalid,
            RouteInitFindingCode.WorkspaceUnavailable
                or RouteInitFindingCode.WorkspaceUnsafe
                or RouteInitFindingCode.TargetUnsafe
                or RouteInitFindingCode.RouteAmbiguous
                or RouteInitFindingCode.IdentityCollision
                or RouteInitFindingCode.LoaderUnsafe
                or RouteInitFindingCode.FrameworkPayloadInvalid
                or RouteInitFindingCode.FrameworkInstallRequired
                or RouteInitFindingCode.FrameworkUpdateRequired
                or RouteInitFindingCode.FrameworkAlignmentBlocked
                or RouteInitFindingCode.MetadataUnsafe
                or RouteInitFindingCode.GeneratedRegionUnsafe
                or RouteInitFindingCode.LifecycleBlocked
                or RouteInitFindingCode.WorkspaceLockUnavailable
                or RouteInitFindingCode.TargetChanged
                or RouteInitFindingCode.RecoveryConflict => CliSemanticStatus.Blocked,
            RouteInitFindingCode.FrameworkPayloadUnavailable
                or RouteInitFindingCode.InspectionIncomplete
                or RouteInitFindingCode.MetadataIncomplete
                or RouteInitFindingCode.ProjectionIncomplete
                or RouteInitFindingCode.LifecycleUnavailable
                or RouteInitFindingCode.RecoveryUnavailable => CliSemanticStatus.Incomplete,
            RouteInitFindingCode.NeedsAuthoring => CliSemanticStatus.Complete,
            RouteInitFindingCode.RecoveryArtifactRetained => CliSemanticStatus.Attention,
            RouteInitFindingCode.TargetChangedDuringApply
                or RouteInitFindingCode.WriteFailed
                or RouteInitFindingCode.VerificationFailed
                or RouteInitFindingCode.LifecyclePublicationFailed
                or RouteInitFindingCode.RecoveryFailed
                or RouteInitFindingCode.OperationFailed => CliSemanticStatus.Failed,
            RouteInitFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Route Init finding code is not defined."),
        };

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<RouteInitFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        return status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Invalid => new CliNextAction(
                "open-forge route init --help",
                "Correct the named Route Init input, then rerun the request."),
            CliSemanticStatus.Blocked when Contains(findings, RouteInitFindingCode.FrameworkInstallRequired) => new CliNextAction(
                "open-forge install --dry-run",
                "Establish a trusted current Framework installation before rerunning Route Init in Framework mode."),
            CliSemanticStatus.Blocked when Contains(findings, RouteInitFindingCode.FrameworkUpdateRequired) => new CliNextAction(
                CommandLines.Update,
                "Update the installed Framework state to the running CLI's embedded inventory before rerunning Route Init."),
            CliSemanticStatus.Blocked when Contains(findings, RouteInitFindingCode.WorkspaceLockUnavailable)
                || Contains(findings, RouteInitFindingCode.TargetChanged) => new CliNextAction(
                    CommandLines.RouteInit,
                    "Wait for the blocking condition or inspect the changed target, then rerun Route Init from a fresh plan."),
            CliSemanticStatus.Blocked => new CliNextAction(
                CommandLines.Doctor,
                "Inspect the blocked workspace, route, identity, lifecycle, generated-region, or recovery boundary before rerunning Route Init."),
            CliSemanticStatus.Incomplete => new CliNextAction(
                CommandLines.Doctor,
                "Inspect the unavailable route, metadata, projection, lifecycle, or recovery facts before relying on this Route Init result."),
            CliSemanticStatus.Attention when Contains(findings, RouteInitFindingCode.RecoveryArtifactRetained) => new CliNextAction(
                CommandLines.Cleanup,
                "Review and remove the reported recovery artifact after confirming the verified Route Init result."),
            CliSemanticStatus.Attention => new CliNextAction(
                CommandLines.RouteUpdate,
                "Author each reported NeedsAuthoring entrypoint before relying on its description or tags."),
            CliSemanticStatus.Failed => new CliNextAction(
                "open-forge route init --detail debug",
                "Report the failure and retry the same Route Init request with bounded diagnostics."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                CommandLines.RouteInit,
                "Rerun the same Route Init request."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Route Init status is not defined."),
        };
    }

    private static bool Contains(
        IReadOnlyList<RouteInitFinding> findings,
        RouteInitFindingCode code)
        => findings.Any(finding => finding.Code == code);

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            name,
            value,
            $"The Route Init {name} value is not defined.");
}
