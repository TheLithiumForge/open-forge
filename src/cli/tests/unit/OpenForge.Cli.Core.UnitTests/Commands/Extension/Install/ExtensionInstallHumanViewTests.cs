using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Install;

public sealed class ExtensionInstallHumanViewTests
{
    [Theory(DisplayName = "Extension Install views retain interrupted effects, unmatched planned paths and recovery without claiming completion"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void InterruptedEffectsAndUnmatchedPaths(int view)
    {
        var workspace = new CliWorkspace("extension-human-workspace", "extension-human-workspace", CliWorkspaceSelectionMethod.CurrentDirectory);
        var request = new ExtensionInstallRequest(workspace, ExtensionInstallMode.Apply, ["toolkit"], all: false, sourcePath: null, force: false, automatic: true, allowInteraction: false);
        var facts = new ExtensionInstallResultFacts
        {
            Selection = new(ExtensionInstallSelectionKind.ExplicitIds, ["toolkit"]),
            Source = null,
            Packages = [new("toolkit", selectedRoot: true, dependencies: [])],
            Framework = null,
            Footprint = new(1, [".agents/first.md", ".agents/not-started.md"], [".agents/unchanged-navigation.md"], []),
            Effects = [new(".agents/first.md", "toolkit", ExtensionInstallEffectKind.PackageFile, ExtensionInstallEffectAction.Create,
                ExtensionInstallEffectOutcome.CompletionUnknown, ExtensionInstallEffectResidual.Unknown)],
            GeneratedNavigation = new([new(".agents/unchanged-navigation.md", ExtensionInstallGeneratedRegionState.Unchanged)]),
            Lifecycle = new(ExtensionInstallLifecycleAction.Publish, ExtensionInstallLifecycleOutcome.NotStarted),
            Recovery = new(ExtensionInstallRecoveryState.Retained, [".agents/protected.md"], "/recovery/bundle"),
            Verification = new(ExtensionInstallVerificationState.Unknown, ExtensionInstallVerificationState.Unknown,
                ExtensionInstallVerificationState.NotRequested, ExtensionInstallVerificationState.NotRequested),
        };
        var result = new ExtensionInstallResult(request, facts, [new(ExtensionInstallFindingCode.Interrupted, "The caller interrupted installation.")]);
        var presentation = new CliPresentationRequest<ExtensionInstallResult>(result, new(CliOutputFormat.Human, (CliView)view, CliVerbosity.Normal));
        var before = ExtensionInstallJsonProjection.RenderJson(presentation);
        var rendered = ExtensionInstallPresentation.RenderHuman(presentation);
        Assert.Contains("Status: interrupted", rendered, StringComparison.Ordinal);
        Assert.Contains(".agents/first.md", rendered, StringComparison.Ordinal);
        Assert.Equal(1, rendered.ReplaceLineEndings("\n").Split('\n').Count(line => line == "  .agents/first.md"));
        Assert.Contains("completion-unknown; remaining state unknown", rendered, StringComparison.Ordinal);
        Assert.Contains(".agents/not-started.md", rendered, StringComparison.Ordinal);
        Assert.Contains("Planned package file.", rendered, StringComparison.Ordinal);
        Assert.Contains("Recovery: retained", rendered, StringComparison.Ordinal);
        Assert.Equal(view == (int)CliView.Expanded, rendered.Contains(".agents/unchanged-navigation.md", StringComparison.Ordinal));
        Assert.Equal(view == (int)CliView.Compact, rendered.Contains("Unchanged navigation paths summarized: 1", StringComparison.Ordinal));
        Assert.Contains("/recovery/bundle", rendered, StringComparison.Ordinal);
        Assert.Contains("Protected paths: .agents/protected.md", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("verified", rendered, StringComparison.Ordinal);
        Assert.Equal(before, ExtensionInstallJsonProjection.RenderJson(presentation));
    }
}
