using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Rendering;

public sealed class InstallHumanRendererTests
{
    [Fact(DisplayName = "Install compact output preserves the complete sparse result and final trimming"), Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void RendersCompleteSparseCompactOutput()
    {
        var root = Path.GetFullPath(Path.DirectorySeparatorChar.ToString());
        var workspacePath = Path.Combine(root, "install-rendering");
        var workspace = new CliWorkspace(
            lexicalRoot: workspacePath,
            physicalRoot: workspacePath,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new InstallRequest(workspace, InstallMode.Apply, force: false, automatic: false, allowsInteractiveConfirmation: false);
        var result = InstallResult.Create(request, findings: [], summary: new InstallOperationSummary
        {
            ManagementState = InstallManagementState.TrustedExact,
            PlannedDirectoryCount = 0,
            PlannedFileCount = 0,
            AppliedDirectoryCount = 0,
            AppliedTargetFileCount = 0,
            LifecyclePublished = false,
            RecoveryState = InstallRecoveryState.NotRequired,
            RecoveryResidualPath = null,
        });
        var expected = string.Join(Environment.NewLine,
            "Open Forge install",
            "Status: complete",
            $"Workspace: {root}install-rendering",
            "Selected by: --workspace",
            "Mode: apply; force: false; automatic: false",
            "Source: unavailable",
            "Installation state: matches the installed Framework",
            "Footprint: unavailable",
            "Effects: 0",
            "Findings: 0",
            "Lifecycle: preserve / already-current",
            "Recovery: not-required",
            "Verification: not-requested");

        var output = InstallHumanRenderer.Render(new CliPresentationRequest<InstallResult>(
            result,
            new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));

        Assert.Equal(expected, output);
    }

    [Theory(DisplayName = "Install human output preserves populated facts, escaping, retained recovery and next action"),
        InlineData(false), InlineData(true),
        Trait("Feature", "install-presentation"), Trait("Evidence", "Unit")]
    public void RendersCompletePopulatedHumanOutput(bool expanded)
    {
        const string inventoryFingerprint = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var root = Path.GetFullPath(Path.DirectorySeparatorChar.ToString());
        var workspacePath = Path.Combine(root, "workspace-\u2028\u2029-end");
        var recoveryPath = Path.Combine(root, "recovery-\u2028\u2029.zip");
        var workspace = new CliWorkspace(
            lexicalRoot: workspacePath,
            physicalRoot: workspacePath,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new InstallRequest(workspace, InstallMode.Apply, force: true, automatic: true, allowsInteractiveConfirmation: false);
        var summary = new InstallOperationSummary
        {
            ManagementState = InstallManagementState.EligibleInitialOccupant,
            PlannedDirectoryCount = 0,
            PlannedFileCount = 2,
            AppliedDirectoryCount = 0,
            AppliedTargetFileCount = 2,
            LifecyclePublished = true,
            RecoveryState = InstallRecoveryState.Retained,
            RecoveryResidualPath = recoveryPath,
        };
        var facts = new InstallResultFacts(new InstallResultFactsInput
        {
            Source = new InstallSource(inventoryFingerprint, assetCount: 2),
            Classification = InstallManagementClassification.EligibleInitialOccupant,
            Footprint = new InstallFootprint(payloadFiles: 1, managedRegions: 0, generatedRegions: 1),
            Effects =
            [
                new InstallEffect(new InstallEffectInput
                {
                    Path = ".agents/loader.md",
                    Kind = InstallEffectKind.File,
                    Action = InstallEffectAction.Replace,
                    SourceAssetPath = "framework/loader.md",
                    Outcome = InstallEffectOutcome.Verified,
                    Residual = InstallEffectResidual.None,
                }),
                new InstallEffect(new InstallEffectInput
                {
                    Path = ".agents/memory/_memory.md",
                    Kind = InstallEffectKind.GeneratedRegion,
                    Action = InstallEffectAction.Replace,
                    SourceAssetPath = null,
                    Outcome = InstallEffectOutcome.Verified,
                    Residual = InstallEffectResidual.None,
                }),
            ],
            Lifecycle = new InstallLifecycle(InstallLifecycleAction.Publish, InstallLifecycleOutcome.Verified),
            Recovery = new InstallRecovery(InstallResultRecoveryState.Retained, recoveryPath),
            Verification = new InstallVerification(InstallResultVerificationState.Verified),
        });
        var result = InstallResult.Create(request,
            findings: [new InstallFinding(
                InstallFindingCode.RecoveryArtifactRetained,
                cause: "Keep\r\n\t\"copy\"\\😀\ud800\u2028\u2029.",
                subject: "recovery\t\"copy\"\\\u2028\u2029.zip")],
            summary: summary,
            facts: facts);
        var escapedRoot = Path.DirectorySeparatorChar == '\\' ? root[..^1] + "\\\\" : "/";
        var expected = string.Join(Environment.NewLine,
            "Open Forge install",
            "Status: requires attention",
            $"Workspace: {root}workspace-\u2028\u2029-end",
            "Selected by: --workspace",
            "Mode: apply; force: true; automatic: true",
            "Source: embedded Framework; 2 assets",
            $"  Inventory fingerprint: {inventoryFingerprint}",
            "Installation state: existing content at installation paths",
            "Footprint: 1 payload files, 0 managed regions, 1 generated regions",
            "Effects: 2",
            "  .agents/loader.md: replace file; verified; residual: none; source: framework/loader.md",
            "  .agents/memory/_memory.md: replace generated-region; verified; residual: none",
            "Findings: 1",
            "REQUIRES ATTENTION: Keep\\u000d\\u000a\\u0009\\\"copy\\\"\\\\😀\\ud800\u2028\u2029. [install.recovery-artifact-retained]",
            "    Target: recovery\\u0009\\\"copy\\\"\\\\\u2028\u2029.zip",
            "Lifecycle: publish / verified",
            $"Recovery: retained / {escapedRoot}recovery-\u2028\u2029.zip",
            "Verification: verified",
            "Next: open-forge cleanup",
            "Review and remove the reported recovery artifact after confirming the verified Install result.");

        var output = InstallHumanRenderer.Render(new CliPresentationRequest<InstallResult>(
            result,
            new CliPresentation(CliOutputFormat.Human, expanded ? CliView.Expanded : CliView.Compact, CliVerbosity.Normal)));

        if (expanded)
        {
            Assert.Equal(expected, output);
        }
        else
        {
            Assert.Contains("Status: requires attention", output, StringComparison.Ordinal);
            Assert.Contains("Next: open-forge cleanup", output, StringComparison.Ordinal);
            Assert.DoesNotContain("inventory fingerprint:", output, StringComparison.OrdinalIgnoreCase);
            foreach (var effect in result.Facts.Effects)
            {
                Assert.Contains(effect.Path, output, StringComparison.Ordinal);
            }
        }
    }
}
