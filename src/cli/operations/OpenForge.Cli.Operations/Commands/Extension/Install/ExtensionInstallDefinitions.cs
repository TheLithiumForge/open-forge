using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Extension.Install;

internal static class ExtensionInstallDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "extension install";

    internal static readonly CliSyntaxDefinition InstallCommand = new(
        name: "install",
        description: "Install reviewed Extension packages into the selected Framework workspace.");

    internal static readonly CliOptionDefinition<string?> Source = new(
        name: "--source",
        description: "Read one exact local package or catalogue source.",
        arity: CliOptionArity.ExactlyOne,
        defaultValue: null,
        valueName: "package-or-catalogue-path");

    internal static readonly CliOptionDefinition<bool> All = new(
        "--all",
        "Select every package in the reviewed source.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> Force = new(
        "--force",
        "Replace eligible existing content during initial installation.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> Automatic = new(
        "--automatic",
        "Disable prompts; does not select packages or imply --force.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        "--dry-run",
        "Preview the complete installation without writing files.",
        CliOptionArity.None,
        false);

    internal static IReadOnlyList<ExtensionInstallFindingCode> FindingCodes { get; } =
        Array.AsReadOnly(Enum.GetValues<ExtensionInstallFindingCode>());

    internal static ExtensionInstallFindingCode ReadPermissionFinding(ExtensionPermissionFailure failure)
        => failure switch
        {
            ExtensionPermissionFailure.Required => ExtensionInstallFindingCode.PermissionRequired,
            ExtensionPermissionFailure.Declined => ExtensionInstallFindingCode.PermissionDeclined,
            ExtensionPermissionFailure.Invalid => ExtensionInstallFindingCode.PermissionsInvalid,
            ExtensionPermissionFailure.Unavailable => ExtensionInstallFindingCode.PermissionsUnavailable,
            ExtensionPermissionFailure.Changed => ExtensionInstallFindingCode.PermissionsChanged,
            ExtensionPermissionFailure.WriteFailed => ExtensionInstallFindingCode.PermissionWriteFailed,
            ExtensionPermissionFailure.Interrupted => ExtensionInstallFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(failure), failure, "The permission failure is not defined."),
        };

    internal static string ReadMachineName(ExtensionInstallFindingCode code)
        => code switch
        {
            ExtensionInstallFindingCode.InvalidInput => "extension-install.invalid-input",
            ExtensionInstallFindingCode.SelectionRequired => "extension-install.selection-required",
            ExtensionInstallFindingCode.InteractionEnded => "extension-install.interaction-ended",
            ExtensionInstallFindingCode.ConfirmationRequired => "extension-install.confirmation-required",
            ExtensionInstallFindingCode.SourceUnavailable => "extension-install.source-unavailable",
            ExtensionInstallFindingCode.SourceInvalid => "extension-install.source-invalid",
            ExtensionInstallFindingCode.FrameworkUnavailable => "extension-install.framework-unavailable",
            ExtensionInstallFindingCode.FrameworkUnsafe => "extension-install.framework-unsafe",
            ExtensionInstallFindingCode.LifecycleUnavailable => "extension-install.lifecycle-unavailable",
            ExtensionInstallFindingCode.LifecycleBlocked => "extension-install.lifecycle-blocked",
            ExtensionInstallFindingCode.ManagedDivergence => "extension-install.managed-divergence",
            ExtensionInstallFindingCode.PackageContentsChanged => "extension-install.package-contents-changed",
            ExtensionInstallFindingCode.InitialForceRequired => "extension-install.initial-force-required",
            ExtensionInstallFindingCode.OwnershipConflict => "extension-install.ownership-conflict",
            ExtensionInstallFindingCode.PermissionRequired => "extension-install.permission-required",
            ExtensionInstallFindingCode.PermissionDeclined => "extension-install.permission-declined",
            ExtensionInstallFindingCode.PermissionsInvalid => "extension-install.permissions-invalid",
            ExtensionInstallFindingCode.PermissionsUnavailable => "extension-install.permissions-unavailable",
            ExtensionInstallFindingCode.PermissionsChanged => "extension-install.permissions-changed",
            ExtensionInstallFindingCode.PermissionWriteFailed => "extension-install.permission-write-failed",
            ExtensionInstallFindingCode.TargetUnsafe => "extension-install.target-unsafe",
            ExtensionInstallFindingCode.ProjectionUnavailable => "extension-install.projection-unavailable",
            ExtensionInstallFindingCode.MetadataProjectionSkipped => "extension-install.metadata-projection-skipped",
            ExtensionInstallFindingCode.GeneratedRegionUnsafe => "extension-install.generated-region-unsafe",
            ExtensionInstallFindingCode.WorkspaceLockUnavailable => "extension-install.workspace-lock-unavailable",
            ExtensionInstallFindingCode.TargetChanged => "extension-install.target-changed",
            ExtensionInstallFindingCode.RecoveryConflict => "extension-install.recovery-conflict",
            ExtensionInstallFindingCode.RecoveryUnavailable => "extension-install.recovery-unavailable",
            ExtensionInstallFindingCode.LifecycleObservation => "extension-install.lifecycle-observation",
            ExtensionInstallFindingCode.PackageContentMissing => "extension-install.package-content-missing",
            ExtensionInstallFindingCode.RecoveryArtifactRetained => "extension-install.recovery-artifact-retained",
            ExtensionInstallFindingCode.WriteFailed => "extension-install.write-failed",
            ExtensionInstallFindingCode.TopologyVerificationFailed => "extension-install.topology-verification-failed",
            ExtensionInstallFindingCode.LifecyclePublicationFailed => "extension-install.lifecycle-publication-failed",
            ExtensionInstallFindingCode.VerificationFailed => "extension-install.verification-failed",
            ExtensionInstallFindingCode.RecoveryFailed => "extension-install.recovery-failed",
            ExtensionInstallFindingCode.OperationFailed => "extension-install.operation-failed",
            ExtensionInstallFindingCode.Interrupted => "extension-install.interrupted",
            ExtensionInstallFindingCode.SettingsInvalid => "extension-install.settings-invalid",
            ExtensionInstallFindingCode.SettingsUnavailable => "extension-install.settings-unavailable",
            ExtensionInstallFindingCode.RemovedExtension => "extension-install.removed-extension",
            ExtensionInstallFindingCode.PathExcluded => "extension-install.path-excluded",
            ExtensionInstallFindingCode.ExcludedAncestor => "extension-install.excluded-ancestor",
            _ => throw Undefined(nameof(code), code),
        };

    internal static CliSemanticStatus ReadStatus(ExtensionInstallFindingCode code)
        => code switch
        {
            ExtensionInstallFindingCode.InvalidInput
                or ExtensionInstallFindingCode.SelectionRequired
                or ExtensionInstallFindingCode.InteractionEnded
                or ExtensionInstallFindingCode.ConfirmationRequired => CliSemanticStatus.Invalid,
            ExtensionInstallFindingCode.SourceUnavailable
                or ExtensionInstallFindingCode.FrameworkUnavailable
                or ExtensionInstallFindingCode.LifecycleUnavailable
                or ExtensionInstallFindingCode.ProjectionUnavailable
                or ExtensionInstallFindingCode.RecoveryUnavailable
                or ExtensionInstallFindingCode.PermissionsUnavailable
                or ExtensionInstallFindingCode.SettingsUnavailable => CliSemanticStatus.Incomplete,
            ExtensionInstallFindingCode.SourceInvalid => CliSemanticStatus.Invalid,
            ExtensionInstallFindingCode.FrameworkUnsafe
                or ExtensionInstallFindingCode.LifecycleBlocked
                or ExtensionInstallFindingCode.ManagedDivergence
                or ExtensionInstallFindingCode.PackageContentsChanged
                or ExtensionInstallFindingCode.InitialForceRequired
                or ExtensionInstallFindingCode.OwnershipConflict
                or ExtensionInstallFindingCode.PermissionRequired
                or ExtensionInstallFindingCode.PermissionDeclined
                or ExtensionInstallFindingCode.PermissionsInvalid
                or ExtensionInstallFindingCode.PermissionsChanged
                or ExtensionInstallFindingCode.TargetUnsafe
                or ExtensionInstallFindingCode.GeneratedRegionUnsafe
                or ExtensionInstallFindingCode.WorkspaceLockUnavailable
                or ExtensionInstallFindingCode.TargetChanged
                or ExtensionInstallFindingCode.RecoveryConflict
                or ExtensionInstallFindingCode.SettingsInvalid
                or ExtensionInstallFindingCode.RemovedExtension
                or ExtensionInstallFindingCode.ExcludedAncestor => CliSemanticStatus.Blocked,
            ExtensionInstallFindingCode.PathExcluded => CliSemanticStatus.Complete,
            ExtensionInstallFindingCode.LifecycleObservation
                or ExtensionInstallFindingCode.MetadataProjectionSkipped
                or ExtensionInstallFindingCode.PackageContentMissing
                or ExtensionInstallFindingCode.RecoveryArtifactRetained
                => CliSemanticStatus.Attention,
            ExtensionInstallFindingCode.WriteFailed
                or ExtensionInstallFindingCode.TopologyVerificationFailed
                or ExtensionInstallFindingCode.LifecyclePublicationFailed
                or ExtensionInstallFindingCode.VerificationFailed
                or ExtensionInstallFindingCode.RecoveryFailed
                or ExtensionInstallFindingCode.OperationFailed
                or ExtensionInstallFindingCode.PermissionWriteFailed => CliSemanticStatus.Failed,
            ExtensionInstallFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw Undefined(nameof(code), code),
        };

    internal static string ReadMachineName(ExtensionInstallMode mode)
        => mode switch
        {
            ExtensionInstallMode.Apply => "apply",
            ExtensionInstallMode.DryRun => "dry-run",
            _ => throw Undefined(nameof(mode), mode),
        };

    internal static string ReadMachineName(ExtensionInstallSelectionKind value)
        => value switch
        {
            ExtensionInstallSelectionKind.ExplicitIds => "explicit-ids",
            ExtensionInstallSelectionKind.ExplicitAll => "explicit-all",
            ExtensionInstallSelectionKind.SinglePackageInference => "single-package-inference",
            ExtensionInstallSelectionKind.InteractiveIds => "interactive-ids",
            ExtensionInstallSelectionKind.InteractiveAll => "interactive-all",
            _ => throw Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionInstallSourceKind value)
        => value switch
        {
            ExtensionInstallSourceKind.Embedded => "embedded",
            ExtensionInstallSourceKind.Package => "package",
            ExtensionInstallSourceKind.Catalogue => "catalogue",
            _ => throw Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionInstallEffectKind value)
        => value switch
        {
            ExtensionInstallEffectKind.Directory => "directory",
            ExtensionInstallEffectKind.PackageFile => "package-file",
            ExtensionInstallEffectKind.GeneratedRegion => "generated-region",
            _ => throw Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionInstallEffectAction value)
        => value switch
        {
            ExtensionInstallEffectAction.Create => "create",
            ExtensionInstallEffectAction.Replace => "replace",
            _ => throw Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionInstallEffectOutcome value)
        => value switch
        {
            ExtensionInstallEffectOutcome.Planned => "planned",
            ExtensionInstallEffectOutcome.NotStarted => "not-started",
            ExtensionInstallEffectOutcome.Verified => "verified",
            ExtensionInstallEffectOutcome.VerificationFailed => "verification-failed",
            ExtensionInstallEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => throw Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionInstallEffectResidual value)
        => value switch
        {
            ExtensionInstallEffectResidual.None => "none",
            ExtensionInstallEffectResidual.Retained => "retained",
            ExtensionInstallEffectResidual.Unknown => "unknown",
            _ => throw Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionInstallGeneratedRegionState value)
        => value switch
        {
            ExtensionInstallGeneratedRegionState.Changed => "changed",
            ExtensionInstallGeneratedRegionState.Unchanged => "unchanged",
            _ => throw Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionInstallLifecycleAction value)
        => value switch
        {
            ExtensionInstallLifecycleAction.None => "none",
            ExtensionInstallLifecycleAction.Preserve => "preserve",
            ExtensionInstallLifecycleAction.Publish => "publish",
            _ => throw Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionInstallLifecycleOutcome value)
        => value switch
        {
            ExtensionInstallLifecycleOutcome.NotRequested => "not-requested",
            ExtensionInstallLifecycleOutcome.Planned => "planned",
            ExtensionInstallLifecycleOutcome.AlreadyCurrent => "already-current",
            ExtensionInstallLifecycleOutcome.NotStarted => "not-started",
            ExtensionInstallLifecycleOutcome.Verified => "verified",
            ExtensionInstallLifecycleOutcome.VerificationFailed => "verification-failed",
            ExtensionInstallLifecycleOutcome.CompletionUnknown => "completion-unknown",
            _ => throw Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionInstallRecoveryState value)
        => value switch
        {
            ExtensionInstallRecoveryState.NotRequired => "not-required",
            ExtensionInstallRecoveryState.NotCreated => "not-created",
            ExtensionInstallRecoveryState.Removed => "removed",
            ExtensionInstallRecoveryState.Retained => "retained",
            ExtensionInstallRecoveryState.Unknown => "unknown",
            _ => throw Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionInstallVerificationState value)
        => value switch
        {
            ExtensionInstallVerificationState.NotRequested => "not-requested",
            ExtensionInstallVerificationState.Planned => "planned",
            ExtensionInstallVerificationState.Verified => "verified",
            ExtensionInstallVerificationState.Failed => "failed",
            ExtensionInstallVerificationState.Unknown => "unknown",
            _ => throw Undefined(nameof(value), value),
        };

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<ExtensionInstallFinding> findings,
        ExtensionInstallRequest? request)
    {
        var divergence = findings.FirstOrDefault(finding =>
            finding.Code is ExtensionInstallFindingCode.ManagedDivergence
                or ExtensionInstallFindingCode.PackageContentsChanged);
        var force = findings.FirstOrDefault(finding =>
            finding.Code == ExtensionInstallFindingCode.InitialForceRequired);
        var selection = findings.FirstOrDefault(finding =>
            finding.Code == ExtensionInstallFindingCode.SelectionRequired);
        var confirmation = findings.FirstOrDefault(finding =>
            finding.Code == ExtensionInstallFindingCode.ConfirmationRequired);
        var removal = findings.FirstOrDefault(finding => finding.Code is
            ExtensionInstallFindingCode.RemovedExtension
                or ExtensionInstallFindingCode.PathExcluded
                or ExtensionInstallFindingCode.ExcludedAncestor);
        return status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Invalid => new CliNextAction(
                confirmation is not null
                    ? "open-forge extension install --automatic"
                    : selection is not null
                    ? "open-forge extension list"
                    : "open-forge extension install --help",
                confirmation is not null
                    ? "Rerun extension install with --automatic or use an interactive terminal to supply confirmation."
                    : selection is not null
                    ? "List the available Extensions, then rerun the request with an explicit selection."
                    : "Correct the named Extension Install input, then rerun the request."),
            CliSemanticStatus.Blocked when divergence is not null => new CliNextAction(
                $"open-forge extension update {divergence.Target}",
                "Use Extension Update to reconcile the managed package."),
            CliSemanticStatus.Blocked when force is not null => new CliNextAction(
                ForceCommand(request ?? throw new InvalidOperationException(
                    "An initial-force finding requires the normalized Extension Install request.")),
                "Rerun the same exact Extension Install request with force authority."),
            CliSemanticStatus.Blocked or CliSemanticStatus.Incomplete when findings.Any(finding =>
                finding.Code is ExtensionInstallFindingCode.SettingsInvalid
                    or ExtensionInstallFindingCode.SettingsUnavailable) => new CliNextAction(
                "Correct .agents/open-forge.json, then rerun extension install.",
                "Repair the workspace settings file before installing.")
                    { Kind = CliNextActionKind.Sentence },
            CliSemanticStatus.Blocked or CliSemanticStatus.Attention when removal is not null => ReadRemovalNextAction(removal.Code),
            CliSemanticStatus.Attention when findings.Any(finding =>
                finding.Code == ExtensionInstallFindingCode.RecoveryArtifactRetained) => new CliNextAction(
                    CommandLines.Cleanup,
                    "Review and remove the reported recovery artifact."),
            CliSemanticStatus.Blocked
                or CliSemanticStatus.Incomplete
                or CliSemanticStatus.Attention
                or CliSemanticStatus.Failed
                or CliSemanticStatus.Interrupted => null,
            _ => throw Undefined(nameof(status), status),
        };
    }

    private static CliNextAction ReadRemovalNextAction(ExtensionInstallFindingCode code)
    {
        var instruction = code switch
        {
            ExtensionInstallFindingCode.RemovedExtension
                => "Remove the selected Extension ID from removedExtensions in .agents/open-forge.json, then rerun extension install.",
            ExtensionInstallFindingCode.PathExcluded or ExtensionInstallFindingCode.ExcludedAncestor
                => "Remove the matching entry from removedCategories, removedFiles, or removedDirectories in .agents/open-forge.json, then rerun extension install.",
            _ => throw Undefined(nameof(code), code),
        };
        return new CliNextAction(
            instruction,
            "Workspace removal settings currently exclude the requested install.")
        { Kind = CliNextActionKind.Sentence };
    }

    private static string ForceCommand(ExtensionInstallRequest request)
    {
        var arguments = new List<string> { "open-forge", "extension", "install" };
        if (request.All)
        {
            arguments.Add("--all");
        }
        else
        {
            arguments.AddRange(request.RequestedIds);
        }

        if (request.SourcePath is { } sourcePath)
        {
            arguments.Add("--source");
            arguments.Add(QuoteArgument(sourcePath));
        }

        arguments.Add("--force");
        if (request.Automatic)
        {
            arguments.Add("--automatic");
        }

        if (request.Mode == ExtensionInstallMode.DryRun)
        {
            arguments.Add("--dry-run");
        }

        if (request.Workspace.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace)
        {
            arguments.Add("--workspace");
            arguments.Add(QuoteArgument(request.Workspace.LexicalRoot));
        }

        return string.Join(' ', arguments);
    }

    private static string QuoteArgument(string value)
        => $"'{value.Replace("'", "'\"'\"'", StringComparison.Ordinal)}'";

    private static ArgumentOutOfRangeException Undefined<T>(string name, T value)
        where T : struct, Enum
        => new(name, value, $"The Extension Install {name} value is not defined.");
}
