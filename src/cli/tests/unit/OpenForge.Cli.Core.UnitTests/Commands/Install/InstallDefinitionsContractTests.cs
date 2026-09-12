using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install;

public sealed class InstallDefinitionsContractTests
{
    [Fact(DisplayName = "Install definitions map every finding to its exact machine code and semantic status"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    public void FindingMappingsAreExact()
    {
        Assert.Equal(
        [
            ("install.invalid-input", CliSemanticStatus.Invalid),
            ("install.confirmation-required", CliSemanticStatus.Invalid),
            ("install.workspace-unavailable", CliSemanticStatus.Blocked),
            ("install.workspace-unsafe", CliSemanticStatus.Blocked),
            ("install.managed-divergence", CliSemanticStatus.Blocked),
            ("install.target-occupied", CliSemanticStatus.Blocked),
            ("install.ownership-conflict", CliSemanticStatus.Blocked),
            ("install.target-unsafe", CliSemanticStatus.Blocked),
            ("install.generated-region-unsafe", CliSemanticStatus.Blocked),
            ("install.lifecycle-blocked", CliSemanticStatus.Blocked),
            ("install.recovery-conflict", CliSemanticStatus.Blocked),
            ("install.payload-unavailable", CliSemanticStatus.Incomplete),
            ("install.payload-invalid", CliSemanticStatus.Blocked),
            ("install.lifecycle-unavailable", CliSemanticStatus.Incomplete),
            ("install.projection-unavailable", CliSemanticStatus.Incomplete),
            ("install.recovery-unavailable", CliSemanticStatus.Incomplete),
            ("install.recovery-artifact-retained", CliSemanticStatus.Attention),
            ("install.write-failed", CliSemanticStatus.Failed),
            ("install.verification-failed", CliSemanticStatus.Failed),
            ("install.lifecycle-publication-failed", CliSemanticStatus.Failed),
            ("install.recovery-failed", CliSemanticStatus.Failed),
            ("install.operation-failed", CliSemanticStatus.Failed),
            ("install.interrupted", CliSemanticStatus.Interrupted),
        ],
            InstallDefinitions.FindingCodes
                .Select(code => (
                    InstallDefinitions.ReadMachineName(code),
                    InstallDefinitions.ReadStatus(code))));
    }

    [Fact(DisplayName = "Install definitions expose every exact finite machine spelling"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    public void FiniteMappingsAreExact()
    {
        Assert.Equal(
            ["apply", "dry-run"],
            Enum.GetValues<InstallMode>().Select(InstallDefinitions.ReadMachineName));
        Assert.Equal(
            ["safe-absence", "trusted-exact", "managed-divergence", "eligible-initial-occupant"],
            Enum.GetValues<InstallManagementClassification>().Select(InstallDefinitions.ReadMachineName));
        Assert.Equal(
            ["directory", "file", "managed-region", "generated-region"],
            Enum.GetValues<InstallEffectKind>().Select(InstallDefinitions.ReadMachineName));
        Assert.Equal(
            ["create", "append", "replace"],
            Enum.GetValues<InstallEffectAction>().Select(InstallDefinitions.ReadMachineName));
        Assert.Equal(
            ["planned", "not-started", "verified", "verification-failed", "completion-unknown"],
            Enum.GetValues<InstallEffectOutcome>().Select(InstallDefinitions.ReadMachineName));
        Assert.Equal(
            ["none", "retained", "unknown"],
            Enum.GetValues<InstallEffectResidual>().Select(InstallDefinitions.ReadMachineName));
        Assert.Equal(
            ["none", "preserve", "publish"],
            Enum.GetValues<InstallLifecycleAction>().Select(InstallDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-requested", "planned", "already-current", "not-started", "verified", "verification-failed", "completion-unknown"],
            Enum.GetValues<InstallLifecycleOutcome>().Select(InstallDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-required", "not-created", "removed", "retained", "unknown"],
            Enum.GetValues<InstallResultRecoveryState>().Select(InstallDefinitions.ReadMachineName));
        Assert.Equal(
            ["not-requested", "verified", "failed", "unknown"],
            Enum.GetValues<InstallResultVerificationState>().Select(InstallDefinitions.ReadMachineName));
    }

    [Fact(DisplayName = "Install definitions reject every undefined enum value"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    public void UndefinedMappingsThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadMachineName((InstallFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadStatus((InstallFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadMachineName((InstallMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadMachineName((InstallManagementClassification)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadMachineName((InstallEffectKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadMachineName((InstallEffectAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadMachineName((InstallEffectOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadMachineName((InstallEffectResidual)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadMachineName((InstallLifecycleAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadMachineName((InstallLifecycleOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadMachineName((InstallResultRecoveryState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadMachineName((InstallResultVerificationState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InstallDefinitions.ReadNextAction(
                (CliSemanticStatus)int.MaxValue,
                [],
                new InstallBindingInput(false, false, InstallMode.Apply)));
    }

    [Fact(DisplayName = "Install next actions preserve the exact status and finding matrix"), Trait("Feature", "install-command"), Trait("Evidence", "Unit")]
    public void NextActionMatrixIsExact()
    {
        var input = new InstallBindingInput(false, false, InstallMode.Apply);
        var forceInput = new InstallBindingInput(true, false, InstallMode.Apply);

        Assert.Null(InstallDefinitions.ReadNextAction(CliSemanticStatus.Complete, [], input));
        Assert.Equal(
            ("open-forge install --automatic", "Rerun the same Install request with explicit automatic mode."),
            ReadNext(CliSemanticStatus.Invalid, [Finding(InstallFindingCode.ConfirmationRequired)], input));
        Assert.Equal(
            ("open-forge install --force --automatic", "Rerun the same Install request with explicit automatic mode."),
            ReadNext(CliSemanticStatus.Invalid, [Finding(InstallFindingCode.ConfirmationRequired)], forceInput));
        Assert.Equal(
            ("open-forge install --help", "Correct the named Install input, then rerun the request."),
            ReadNext(CliSemanticStatus.Invalid, [Finding(InstallFindingCode.InvalidInput)], input));
        Assert.Equal(
            ("open-forge update", "Update the existing managed Framework state from a fresh plan."),
            ReadNext(CliSemanticStatus.Blocked, [Finding(InstallFindingCode.ManagedDivergence)], input));
        Assert.Equal(
            ("open-forge doctor", "Inspect the blocked workspace, ownership, lifecycle, or safety boundary before rerunning Install."),
            ReadNext(CliSemanticStatus.Blocked, [Finding(InstallFindingCode.TargetUnsafe)], input));
        Assert.Equal(
            ("open-forge doctor", "Inspect the unavailable source, lifecycle, projection, or recovery facts before relying on Install."),
            ReadNext(CliSemanticStatus.Incomplete, [Finding(InstallFindingCode.PayloadUnavailable)], input));
        Assert.Equal(
            ("open-forge cleanup", "Review and remove the reported recovery artifact after confirming the verified Install result."),
            ReadNext(CliSemanticStatus.Attention, [Finding(InstallFindingCode.RecoveryArtifactRetained)], input));
        Assert.Equal(
            ("open-forge install --verbose", "Report the failure and retry the same Install request with bounded diagnostics."),
            ReadNext(CliSemanticStatus.Failed, [Finding(InstallFindingCode.OperationFailed)], input));
        Assert.Equal(
            ("open-forge install", "Rerun the same Install request."),
            ReadNext(CliSemanticStatus.Interrupted, [Finding(InstallFindingCode.Interrupted)], input));
    }

    private static (string Command, string Reason) ReadNext(
        CliSemanticStatus status,
        IReadOnlyList<InstallFinding> findings,
        InstallBindingInput input)
    {
        var next = Assert.IsType<CliNextAction>(InstallDefinitions.ReadNextAction(status, findings, input));
        return (next.Command, next.Reason);
    }

    private static InstallFinding Finding(InstallFindingCode code)
        => new(code, "The bounded Install condition was observed.");
}
