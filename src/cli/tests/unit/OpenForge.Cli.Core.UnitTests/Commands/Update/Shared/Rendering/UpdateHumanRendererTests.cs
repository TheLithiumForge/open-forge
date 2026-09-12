using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Update.Shared.Rendering;

public sealed class UpdateHumanRendererTests
{
    [Fact(DisplayName = "Update compact output preserves the complete sparse result and final trimming"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void RendersCompleteSparseCompactOutput()
    {
        var result = new UpdateResult(new UpdateResultFormation
        {
            Workspace = null,
            Mode = UpdateMode.Apply,
            Force = false,
            Prune = false,
            Automatic = false,
            Source = null,
            Comparisons = [],
            GeneratedNavigation = null,
            Effects = [],
            Lifecycle = new UpdateLifecycle
            {
                Trust = UpdateLifecycleTrust.NotRequested,
                Coverage = UpdateLifecycleCoverage.NotRequested,
                Action = UpdateLifecycleAction.None,
                Outcome = UpdateLifecycleOutcome.NotRequested,
            },
            Recovery = new UpdateRecovery
            {
                State = UpdateRecoveryState.NotRequired,
                ProtectedPaths = [],
                ResidualPath = null,
            },
            Verification = UpdateVerificationState.NotRequested,
            Findings = [],
        });
        var expected = string.Join(Environment.NewLine,
            "The managed Framework is up to date.",
            "Workspace: unavailable",
            "Selected by: unavailable",
            "Flags: mode=apply, force=false, prune=false, automatic=false",
            "Status: complete",
            "Source: unavailable",
            "Comparisons: 0",
            "Generated navigation: unavailable",
            "Effects: 0",
            "Lifecycle: trust=not-requested / coverage=not-requested / action=none / outcome=not-requested",
            "Recovery: not-required / residual=unavailable",
            "Findings: 0",
            "Verification: not-requested");

        var output = UpdateHumanRenderer.Render(new CliPresentationRequest<UpdateResult>(
            result,
            new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));

        Assert.Equal(expected, output);
    }

    [Fact(DisplayName = "Update expanded output preserves populated facts, escaping, dry-run, retained recovery and next action"),
        Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void RendersCompletePopulatedExpandedOutput()
    {
        const string baselineFingerprint = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        const string intendedFingerprint = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        var root = Path.GetFullPath(Path.DirectorySeparatorChar.ToString());
        var workspacePath = Path.Combine(root, "workspace-\u2028\u2029-end");
        var workspace = new CliWorkspace(
            lexicalRoot: workspacePath,
            physicalRoot: workspacePath,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var result = new UpdateResult(new UpdateResultFormation
        {
            Workspace = workspace,
            Mode = UpdateMode.DryRun,
            Force = true,
            Prune = true,
            Automatic = true,
            Source = new UpdateSource
            {
                Id = "framework",
                Version = "v1\t\"x\"\\😀\ud800\u2028\u2029-end",
                InventoryFingerprint = baselineFingerprint,
                AssetCount = 2,
            },
            Comparisons =
            [
                new UpdateComparison
                {
                    RelativePath = "docs/index.md",
                    Kind = UpdateComparisonTargetKind.ManagedRegion,
                    RegionIdentity = "entries-\u2028\u2029-end",
                    SourceAssetPath = "framework/source-\u2028\u2029.md",
                    SourceAssetPresentInCurrentInventory = true,
                    FingerprintKind = UpdateComparisonFingerprintKind.OpenForgeMarkdownV1,
                    BaselineFingerprint = baselineFingerprint,
                    CurrentFingerprint = baselineFingerprint,
                    IntendedFingerprint = intendedFingerprint,
                    CurrentState = UpdateComparisonCurrentState.BaselineEquivalent,
                    IntendedState = UpdateComparisonIntendedState.Changed,
                    RetirementEligibility = UpdateRetirementEligibility.NotApplicable,
                    CurrentBytes = new UpdateComparisonByteFacts
                    {
                        ExactBytes = [1, 2, 3],
                        Sha256 = "039058c6f2c0cb492c533b0a4d14ef77cc0f78abccced5287d84a1a2011cfb81",
                    },
                    IntendedBytes = new UpdateComparisonByteFacts
                    {
                        ExactBytes = [4, 5, 6],
                        Sha256 = "787c798e39a5bc1910355bae6d0cd87a36b2e10fd0202a83e3bb6b005da83472",
                    },
                },
            ],
            GeneratedNavigation = new UpdateGeneratedNavigation
            {
                Coverage = UpdateGeneratedNavigationCoverage.Complete,
                Regions =
                [
                    new UpdateGeneratedNavigationRegion
                    {
                        Path = ".agents/memory/_memory.md",
                        State = UpdateGeneratedNavigationRegionState.Changed,
                    },
                ],
            },
            Effects =
            [
                new UpdatePhysicalEffect(
                    path: "docs/index.md",
                    action: UpdatePhysicalEffectAction.Replace,
                    changes: [new UpdateLogicalChange(
                        kind: UpdateComparisonTargetKind.ManagedRegion,
                        action: UpdateLogicalChangeAction.Replace,
                        region: "entries-\u2028\u2029-end",
                        sourceAssetPath: "framework/source-\u2028\u2029.md")],
                    outcome: UpdatePhysicalEffectOutcome.Planned,
                    residual: UpdatePhysicalEffectResidual.None),
            ],
            Lifecycle = new UpdateLifecycle
            {
                Trust = UpdateLifecycleTrust.Trusted,
                Coverage = UpdateLifecycleCoverage.Complete,
                Action = UpdateLifecycleAction.Publish,
                Outcome = UpdateLifecycleOutcome.Planned,
            },
            Recovery = new UpdateRecovery
            {
                State = UpdateRecoveryState.Retained,
                ProtectedPaths = [".agents/open-forge.lifecycle.json", "docs/index.md"],
                ResidualPath = "recovery-\u2028\u2029.zip",
            },
            Verification = UpdateVerificationState.NotRequested,
            Findings = [new UpdateFinding(
                UpdateFindingCode.RecoveryArtifactRetained,
                target: "recovery\t\"copy\"\\\u2028\u2029.zip",
                cause: "Keep\r\n\t\"copy\"\\😀\ud800\u2028\u2029.")],
        });
        var escapedRoot = Path.DirectorySeparatorChar == '\\' ? root[..^1] + "\\\\" : "/";
        var expected = string.Join(Environment.NewLine,
            "The managed Framework update requires attention.",
            $"Workspace: {escapedRoot}workspace-\u2028\u2029-end",
            "Selected by: --workspace",
            "Flags: mode=dry-run, force=true, prune=true, automatic=true",
            "Status: requires attention",
            $"Source: framework / version=v1\\u0009\\\"x\\\"\\\\😀\\ud800\u2028\u2029-end / inventory={baselineFingerprint} / assets=2",
            "Comparisons: 1",
            "  docs/index.md: managed-region / region=entries-\u2028\u2029-end / current=baseline-equivalent / intended=changed / retirement=not-applicable",
            "    Source: framework/source-\u2028\u2029.md / present=true",
            $"    Fingerprints: policy=open-forge-markdown-v1 / baseline={baselineFingerprint} / current={baselineFingerprint} / intended={intendedFingerprint}",
            "Generated navigation: complete",
            "  .agents/memory/_memory.md: changed",
            "Effects: 1",
            "  docs/index.md: replace / planned / residual=none",
            "    managed-region: replace / region=entries-\u2028\u2029-end / source=framework/source-\u2028\u2029.md",
            "Lifecycle: trust=trusted / coverage=complete / action=publish / outcome=planned",
            "Recovery: retained / residual=recovery-\u2028\u2029.zip",
            "  Protected: docs/index.md",
            "  Protected: .agents/open-forge.lifecycle.json",
            "Findings: 1",
            "  update.recovery-artifact-retained / target=recovery\\u0009\\\"copy\\\"\\\\\u2028\u2029.zip / Keep\\u000d\\u000a\\u0009\\\"copy\\\"\\\\😀\\ud800\u2028\u2029.",
            "Verification: not-requested",
            "No files changed (--dry-run).",
            "Next: open-forge cleanup — Review and remove the reported recovery artifact after confirming the verified Update result.");

        var output = UpdateHumanRenderer.Render(new CliPresentationRequest<UpdateResult>(
            result,
            new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal)));

        Assert.Equal(expected, output);
    }
}
