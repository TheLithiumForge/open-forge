using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Update;

public sealed class ExtensionUpdateResultContractTests
{
    [Fact(DisplayName = "Extension Update result snapshots typed facts and orders findings deterministically"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void ResultSnapshotsFactsAndOrdersFindings()
    {
        var package = new ExtensionUpdatePackage("toolkit", selectedRoot: true, dependencies: []);
        var comparison = new ExtensionUpdateComparison(
            ".agents/toolkit.md",
            "toolkit",
            ExtensionUpdateComparisonTargetKind.PackageFile,
            region: null,
            sourceAssetPath: "toolkit/content/.agents/toolkit.md",
            ExtensionUpdateComparisonFingerprintKind.OpenForgeMarkdownV1,
            baselineFingerprint: Fingerprint('a'),
            currentFingerprint: Fingerprint('a'),
            intendedFingerprint: Fingerprint('b'),
            ExtensionUpdateComparisonCurrentState.BaselineEquivalent,
            ExtensionUpdateComparisonIntendedState.Changed,
            ExtensionUpdateRetirementEligibility.NotApplicable);
        var changes = new ExtensionUpdateLogicalChange(
            ExtensionUpdateComparisonTargetKind.PackageFile,
            ExtensionUpdateChangeAction.Replace,
            region: null,
            sourceAssetPath: "toolkit/content/.agents/toolkit.md");
        var effect = new ExtensionUpdateEffect(
            ".agents/toolkit.md",
            "toolkit",
            ExtensionUpdateEffectKind.PackageFile,
            ExtensionUpdateEffectAction.Replace,
            [changes],
            ExtensionUpdateEffectOutcome.Planned,
            ExtensionUpdateEffectResidual.None);
        var facts = new ExtensionUpdateResultFacts
        {
            Selection = new ExtensionUpdateSelection(
                ExtensionUpdateSelectionKind.ExplicitIds,
                ["toolkit"]),
            Source = new ExtensionUpdateSource(
                ExtensionUpdateSourceKind.Catalogue,
                "/source",
                "catalogue-identity",
                packageCount: 1),
            Packages = [package],
            Comparisons = [comparison],
            GeneratedNavigation = new ExtensionUpdateGeneratedNavigation(
            [
                new ExtensionUpdateGeneratedRegion(
                    ".agents/_index.md",
                    ExtensionUpdateGeneratedRegionState.Changed),
                new ExtensionUpdateGeneratedRegion(".agents/unchanged-navigation.md", ExtensionUpdateGeneratedRegionState.Unchanged),
            ]),
            Effects = [effect],
            Lifecycle = new ExtensionUpdateLifecycle(
                ExtensionUpdateLifecycleTrust.Trusted,
                ExtensionUpdateLifecycleCoverage.Complete,
                ExtensionUpdateLifecycleAction.Publish,
                ExtensionUpdateLifecycleOutcome.Planned),
            Recovery = new ExtensionUpdateRecovery(
                ExtensionUpdateRecoveryState.NotCreated,
                [".agents/toolkit.md"],
                residualPath: null),
            Verification = new ExtensionUpdateVerification(
                ExtensionUpdateVerificationState.Planned,
                ExtensionUpdateVerificationState.Planned,
                ExtensionUpdateVerificationState.Planned),
        };
        var formation = new ExtensionUpdateResultFormation
        {
            Workspace = Workspace(),
            Mode = ExtensionUpdateMode.DryRun,
            Force = false,
            Prune = false,
            Automatic = true,
            Facts = facts,
            Findings =
            [
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.ManagedDivergence,
                    "Current bytes differ.",
                    ".agents/toolkit.md"),
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.TargetUnsafe,
                    "Target is unsafe.",
                    ".agents/unsafe.md"),
            ],
        };

        var result = new ExtensionUpdateResult(formation);

        Assert.Equal("extension update", result.Command);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Same(formation.Workspace, result.Workspace);
        Assert.Equal(ExtensionUpdateMode.DryRun, result.Mode);
        Assert.True(result.Automatic);
        Assert.Same(package, Assert.Single(result.Packages));
        Assert.Same(comparison, Assert.Single(result.Comparisons));
        Assert.Same(effect, Assert.Single(result.Effects));
        Assert.Equal(
            [ExtensionUpdateFindingCode.ManagedDivergence, ExtensionUpdateFindingCode.TargetUnsafe],
            result.Findings.Select(finding => finding.Code));
        Assert.Null(result.Next);
        foreach (var view in new[] { CliView.Compact, CliView.Expanded })
        {
            var request = new CliPresentationRequest<ExtensionUpdateResult>(result, new(CliOutputFormat.Human, view, CliVerbosity.Normal));
            var jsonBefore = ExtensionUpdateJsonProjection.RenderJson(request);
            var rendered = ExtensionUpdatePresentation.RenderHuman(request);
            Assert.Contains("Status: blocked", rendered, StringComparison.Ordinal);
            Assert.Contains(".agents/unsafe.md", rendered, StringComparison.Ordinal);
            Assert.Contains(".agents/_index.md", rendered, StringComparison.Ordinal);
            Assert.Equal(view == CliView.Expanded, rendered.Contains(".agents/unchanged-navigation.md", StringComparison.Ordinal));
            Assert.Equal(view == CliView.Compact, rendered.Contains("Unchanged navigation paths summarized: 1", StringComparison.Ordinal));
            Assert.Contains("planned", rendered, StringComparison.Ordinal);
            Assert.DoesNotContain("verified", rendered, StringComparison.Ordinal);
            Assert.Contains("Protected paths: .agents/toolkit.md", rendered, StringComparison.Ordinal);
            Assert.Equal(1, rendered.ReplaceLineEndings("\n").Split('\n').Count(line => line == "  .agents/toolkit.md"));
            Assert.Equal(jsonBefore, ExtensionUpdateJsonProjection.RenderJson(request));
        }

    }

    [Fact(DisplayName = "Extension Update empty result initializes every typed safety fact without effects"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void EmptyResultInitializesEveryTypedSafetyFact()
    {
        var result = ExtensionUpdateResult.Empty(
            Workspace(),
            ExtensionUpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            findings:
            [
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.InvalidInput,
                    "Invalid selection."),
            ]);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Empty(result.Packages);
        Assert.Empty(result.Comparisons);
        Assert.Empty(result.Effects);
        Assert.Null(result.Selection);
        Assert.Null(result.Source);
        Assert.Equal(ExtensionUpdateLifecycleAction.None, result.Lifecycle.Action);
        Assert.Equal(ExtensionUpdateRecoveryState.NotRequired, result.Recovery.State);
        Assert.Equal(ExtensionUpdateVerificationState.NotRequested, result.Verification.Targets);
        Assert.NotNull(result.Next);
    }

    private static CliWorkspace Workspace()
        => new("extension-update-result-workspace", "extension-update-result-workspace", CliWorkspaceSelectionMethod.CurrentDirectory);

    private static string Fingerprint(char value)
        => new(value, 64);
}
