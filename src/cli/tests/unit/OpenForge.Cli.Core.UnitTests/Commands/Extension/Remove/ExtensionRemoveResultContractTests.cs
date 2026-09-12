using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveResultContractTests
{
    [Fact(DisplayName = "Extension Remove result snapshots typed facts and orders findings by stable identity"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void ResultSnapshotsFactsAndOrdersFindings()
    {
        var package = new ExtensionRemovePackageFact(
            "toolkit",
            selectedForRemoval: true,
            ["library"]);
        var dependencyPlan = new ExtensionRemoveDependencyPlan(
            [
                package,
                new ExtensionRemovePackageFact("library", selectedForRemoval: false, []),
            ],
            ["toolkit"],
            [],
            ["library"]);
        var selection = new ExtensionRemoveSelection(
            ExtensionRemoveSelectionKind.ExplicitIds,
            ["toolkit"]);
        var path = new ExtensionRemovePathPlan(
            ".agents/toolkit.md",
            ExtensionRemovePathClassification.UnchangedFinalOwner,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.Delete);
        var effect = new ExtensionRemoveEffect(
            ".agents/toolkit.md",
            "toolkit",
            ExtensionRemoveEffectKind.PackageFile,
            ExtensionRemoveEffectAction.Delete,
            ExtensionRemoveEffectOutcome.Planned,
            ExtensionRemoveEffectResidual.None);
        var facts = new ExtensionRemoveResultFacts
        {
            Selection = selection,
            Dependencies = dependencyPlan,
            Paths = [path],
            GeneratedNavigation = new ExtensionRemoveGeneratedNavigation(
            [
                new ExtensionRemoveGeneratedRegion(
                    ".agents/_index.md",
                    ExtensionRemoveGeneratedRegionState.Changed),
                new ExtensionRemoveGeneratedRegion(".agents/unchanged-navigation.md", ExtensionRemoveGeneratedRegionState.Unchanged),
            ]),
            Effects = [effect],
            Lifecycle = new ExtensionRemoveLifecycle(
                ExtensionRemoveLifecycleTrust.Trusted,
                ExtensionRemoveLifecycleCoverage.Complete,
                ExtensionRemoveLifecycleAction.Publish,
                ExtensionRemoveLifecycleOutcome.Planned),
            Recovery = new ExtensionRemoveRecovery(
                ExtensionRemoveRecoveryState.NotCreated,
                [".agents/toolkit.md"],
                residualPath: null),
            Verification = new ExtensionRemoveVerification(
                ExtensionRemoveVerificationState.Planned,
                ExtensionRemoveVerificationState.Planned,
                ExtensionRemoveVerificationState.Planned),
            PackageSourceUnchanged = true,
        };
        var findingInput = new List<ExtensionRemoveFinding>
        {
            new(
                ExtensionRemoveFindingCode.TargetUnsafe,
                "The target is reserved.",
                ".agents/unsafe.md"),
            new(
                ExtensionRemoveFindingCode.ManagedDivergence,
                "The current content differs.",
                ".agents/toolkit.md"),
        };
        var result = new ExtensionRemoveResult(new ExtensionRemoveResultFormation
        {
            Workspace = Workspace(),
            Mode = ExtensionRemoveMode.DryRun,
            Prune = false,
            Automatic = true,
            Facts = facts,
            Findings = findingInput,
        });

        findingInput.Reverse();

        Assert.Equal("extension remove", result.Command);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Same(facts.Selection, result.Selection);
        Assert.Same(facts.Dependencies, result.Dependencies);
        Assert.Same(path, Assert.Single(result.Paths));
        Assert.Same(effect, Assert.Single(result.Effects));
        Assert.NotSame(facts.Paths, result.Paths);
        Assert.NotSame(facts.Effects, result.Effects);
        Assert.Equal(
            [
                ExtensionRemoveFindingCode.ManagedDivergence,
                ExtensionRemoveFindingCode.TargetUnsafe,
            ],
            result.Findings.Select(finding => finding.Code));
        Assert.True(result.PackageSourceUnchanged);
        Assert.Null(result.Next);
        foreach (var view in new[] { CliView.Compact, CliView.Expanded })
        {
            var request = new CliPresentationRequest<ExtensionRemoveResult>(result, new(CliOutputFormat.Human, view, CliVerbosity.Normal));
            var jsonBefore = ExtensionRemoveJsonProjection.RenderJson(request);
            var rendered = ExtensionRemovePresentation.RenderHuman(request);
            Assert.Contains("Status: blocked", rendered, StringComparison.Ordinal);
            Assert.Contains(".agents/unsafe.md", rendered, StringComparison.Ordinal);
            Assert.Contains(".agents/_index.md", rendered, StringComparison.Ordinal);
            Assert.Equal(view == CliView.Expanded, rendered.Contains(".agents/unchanged-navigation.md", StringComparison.Ordinal));
            Assert.Equal(view == CliView.Compact, rendered.Contains("Unchanged navigation paths summarized: 1", StringComparison.Ordinal));
            Assert.Contains("planned", rendered, StringComparison.Ordinal);
            Assert.DoesNotContain("verified", rendered, StringComparison.Ordinal);
            Assert.Contains("Protected paths: .agents/toolkit.md", rendered, StringComparison.Ordinal);
            Assert.Equal(1, rendered.ReplaceLineEndings("\n").Split('\n').Count(line => line == "  .agents/toolkit.md"));
            Assert.Equal(jsonBefore, ExtensionRemoveJsonProjection.RenderJson(request));
        }

    }

    [Fact(DisplayName = "Extension Remove empty result initializes every typed safety fact and next action"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void EmptyResultInitializesTypedFacts()
    {
        var workspace = Workspace();
        var result = ExtensionRemoveResult.Empty(
            workspace,
            ExtensionRemoveMode.Apply,
            prune: false,
            automatic: true,
            findings:
            [
                new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.InvalidInput,
                    "Correct the selected input."),
            ]);

        Assert.Equal("extension remove", result.Command);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Same(workspace, result.Workspace);
        Assert.Empty(result.Paths);
        Assert.Empty(result.Effects);
        Assert.Null(result.Selection);
        Assert.Null(result.Dependencies);
        Assert.Null(result.GeneratedNavigation);
        Assert.Equal(ExtensionRemoveLifecycleTrust.NotRequested, result.Lifecycle.Trust);
        Assert.Equal(ExtensionRemoveLifecycleCoverage.NotRequested, result.Lifecycle.Coverage);
        Assert.Equal(ExtensionRemoveLifecycleAction.None, result.Lifecycle.Action);
        Assert.Equal(ExtensionRemoveRecoveryState.NotRequired, result.Recovery.State);
        Assert.Equal(ExtensionRemoveVerificationState.NotRequested, result.Verification.Targets);
        Assert.True(result.PackageSourceUnchanged);
        Assert.Equal(
            "open-forge extension remove --help",
            Assert.IsType<CliNextAction>(result.Next).Command);
    }

    [Fact(DisplayName = "Extension Remove findings derive status and reject undefined or blank facts"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void FindingsValidateTypedFacts()
    {
        var attention = new ExtensionRemoveFinding(
            ExtensionRemoveFindingCode.ManagedDivergence,
            "Current bytes differ.",
            ".agents/toolkit.md");
        var blocked = new ExtensionRemoveFinding(
            ExtensionRemoveFindingCode.TargetUnsafe,
            "Target is reserved.");

        Assert.Equal(CliSemanticStatus.Attention, attention.Status);
        Assert.Equal(".agents/toolkit.md", attention.Target);
        Assert.Equal(CliSemanticStatus.Blocked, blocked.Status);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionRemoveFinding(
            (ExtensionRemoveFindingCode)int.MaxValue,
            "Invalid code."));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveFinding(
            ExtensionRemoveFindingCode.InvalidInput,
            " "));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveFinding(
            ExtensionRemoveFindingCode.InvalidInput,
            "Invalid target.",
            string.Empty));
    }

    [Fact(DisplayName = "Extension Remove recovery facts require a residual path only for retained bundles"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void RecoveryFactsRequireRetainedResidualCertainty()
    {
        var retained = new ExtensionRemoveRecovery(
            ExtensionRemoveRecoveryState.Retained,
            [".agents/toolkit.md"],
            "recovery/bundle.zip");
        var removed = new ExtensionRemoveRecovery(
            ExtensionRemoveRecoveryState.Removed,
            [".agents/toolkit.md"],
            residualPath: null);

        Assert.Equal("recovery/bundle.zip", retained.ResidualPath);
        Assert.Equal(ExtensionRemoveRecoveryState.Removed, removed.State);
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveRecovery(
            ExtensionRemoveRecoveryState.Retained,
            [],
            residualPath: null));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveRecovery(
            ExtensionRemoveRecoveryState.Removed,
            [],
            "recovery/bundle.zip"));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveRecovery(
            ExtensionRemoveRecoveryState.Removed,
            [".agents/toolkit.md", ".agents/toolkit.md"],
            residualPath: null));
    }

    private static CliWorkspace Workspace()
        => new(
            "extension-remove-result-workspace",
            "extension-remove-result-workspace",
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
