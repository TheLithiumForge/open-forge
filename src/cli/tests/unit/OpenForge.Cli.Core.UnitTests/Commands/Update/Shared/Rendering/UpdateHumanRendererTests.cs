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
        var result = SparseResult([]);
        var expected = string.Join(Environment.NewLine,
            "The managed Framework is up to date.",
            "Status: complete",
            "Workspace: unavailable",
            "Selected by: unavailable",
            "Mode: apply; force: false; prune: false; automatic: false",
            "Source: unavailable",
            "Effects: 0",
            "Generated navigation: unavailable",
            "Lifecycle: trust=not-requested / coverage=not-requested / action=none / outcome=not-requested",
            "Recovery: not-required / residual=unavailable",
            "Verification: not-requested");

        var output = UpdateHumanRenderer.Render(new CliPresentationRequest<UpdateResult>(
            result,
            new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));

        Assert.Equal(expected, output);
    }

    [Theory(DisplayName = "Update human output preserves populated facts, escaping, dry-run, retained recovery and next action"),
        InlineData(false), InlineData(true),
        Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void RendersCompletePopulatedHumanOutput(bool expanded)
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
        var expected = string.Join(Environment.NewLine,
            "The managed Framework update requires attention.",
            "Status: requires attention",
            $"Workspace: {root}workspace-\u2028\u2029-end",
            "Selected by: --workspace",
            "Mode: dry-run; force: true; prune: true; automatic: true",
            "Source: embedded Framework; 2 assets",
            $"  ID: framework; version: v1\\u0009\\\"x\\\"\\\\😀\\ud800\u2028\u2029-end; inventory fingerprint: {baselineFingerprint}",
            "REQUIRES ATTENTION: Keep\\u000d\\u000a\\u0009\\\"copy\\\"\\\\😀\\ud800\u2028\u2029. [update.recovery-artifact-retained]",
            "  recovery\\u0009\\\"copy\\\"\\\\\u2028\u2029.zip",
            "Effects: 1",
            "  docs/index.md: replace; planned; residual: none",
            "    managed-region: replace / region=entries-\u2028\u2029-end / source=framework/source-\u2028\u2029.md",
            "    Comparison: managed-region / region=entries-\u2028\u2029-end; current: baseline-equivalent; intended: changed; retirement: not-applicable",
            "    Source: framework/source-\u2028\u2029.md / present=true",
            $"    Fingerprints: policy=open-forge-markdown-v1 / baseline={baselineFingerprint} / current={baselineFingerprint} / intended={intendedFingerprint}",
            "Generated navigation: complete",
            "  .agents/memory/_memory.md: changed",
            "Lifecycle: trust=trusted / coverage=complete / action=publish / outcome=planned",
            "Recovery: retained / residual=recovery-\u2028\u2029.zip",
            "  Protected: docs/index.md",
            "  Protected: .agents/open-forge.lifecycle.json",
            "Verification: not-requested",
            "No files changed (--dry-run).",
            "Next: open-forge cleanup",
            "Review and remove the reported recovery artifact after confirming the verified Update result.");

        var output = UpdateHumanRenderer.Render(new CliPresentationRequest<UpdateResult>(
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
            foreach (var effect in result.Effects)
            {
                Assert.Contains(effect.Path, output, StringComparison.Ordinal);
            }
        }
    }
    [Theory(DisplayName = "Update explains each preserved-state kind without changing the producer's JSON cause"),
        InlineData((int)UpdateFindingCode.ManagedDivergence, "Local changes were kept", false),
        InlineData((int)UpdateFindingCode.ManagedDivergence, "Local changes were kept", true),
        InlineData((int)UpdateFindingCode.ManagedTargetMissing, "The managed path is missing", false),
        InlineData((int)UpdateFindingCode.ManagedTargetMissing, "The managed path is missing", true),
        InlineData((int)UpdateFindingCode.RetiredContentPreserved, "Update did not remove it", false),
        InlineData((int)UpdateFindingCode.RetiredContentPreserved, "Update did not remove it", true),
        Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void PreservedStateExplanationFollowsItsTypedKind(int code, string explanation, bool expanded)
    {
        const string cause = "Update preserved trusted managed divergence without the required explicit authority.";
        var result = SparseResult([new UpdateFinding((UpdateFindingCode)code, "docs/guide.md", cause)]);
        var presentation = new CliPresentationRequest<UpdateResult>(result,
            new CliPresentation(CliOutputFormat.Human, expanded ? CliView.Expanded : CliView.Compact, CliVerbosity.Normal));
        var before = UpdateJsonRenderer.Render(presentation);

        var text = UpdateHumanRenderer.Render(presentation);

        Assert.Contains(explanation, text, StringComparison.Ordinal);
        Assert.Contains("docs/guide.md", text, StringComparison.Ordinal);
        Assert.Contains(cause, before, StringComparison.Ordinal);
        Assert.Equal(before, UpdateJsonRenderer.Render(presentation));
    }

    private static UpdateResult SparseResult(IReadOnlyList<UpdateFinding> findings)
    {
        return new UpdateResult(new UpdateResultFormation
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
            Findings = findings,
        });
    }

}
