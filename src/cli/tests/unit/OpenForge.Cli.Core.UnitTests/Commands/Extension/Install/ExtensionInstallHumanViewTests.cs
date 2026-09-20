using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Install;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Install;

public sealed class ExtensionInstallHumanViewTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Extension Install views retain interrupted effects, unmatched planned paths and recovery without claiming completion"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    public void InterruptedEffectsAndUnmatchedPaths(int view)
    {
        var workspace = new CliWorkspace("extension-human-workspace", "extension-human-workspace", CliWorkspaceSelectionMethod.CurrentDirectory);
        var request = new ExtensionInstallRequest(workspace, ExtensionInstallMode.Apply, ["toolkit"], all: false, sourcePath: null, force: false, automatic: true, allowInteraction: false);
        var facts = new ExtensionInstallResultFacts
        {
            Selection = new(ExtensionInstallSelectionKind.ExplicitIds, ["toolkit"]),
            Source = null,
            Packages = [new("toolkit", "1.0.0", selectedRoot: true, dependencies: [])],
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
        var presentation = new CliPresentationRequest<ExtensionInstallResult>(result, new(CliFormat.Text, (CliDetail)view, null));
        var before = CliRenderingStage.Render(presentation with
        {
            Presentation = presentation.Presentation with { Format = CliFormat.Json },
        }, ExtensionInstallPresentation.Rendering).PrimaryContent;
        var rendered = CliRenderingStage.Render(presentation, ExtensionInstallPresentation.Rendering).PrimaryContent;
        Assert.Contains("Extension install was cancelled. Nothing was changed.", rendered, StringComparison.Ordinal);
        Assert.Contains(".agents/first.md", rendered, StringComparison.Ordinal);
        Assert.Contains("final state unknown", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("Generated navigation observed for installation.", rendered, StringComparison.Ordinal);
        Assert.Contains("Workspace:", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain(".agents/not-started.md", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("verified", rendered, StringComparison.Ordinal);
        var after = CliRenderingStage.Render(presentation with
        {
            Presentation = presentation.Presentation with { Format = CliFormat.Json },
        }, ExtensionInstallPresentation.Rendering).PrimaryContent;
        Assert.Equal(before, after);
    }

    [Fact(DisplayName = "Extension Install minimal OwnershipConflict output names the exact protected target and preserves its cause"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
    public void MinimalOwnershipConflictNamesExactTargetAndCause()
    {
        var workspace = new CliWorkspace("extension-human-workspace", "extension-human-workspace", CliWorkspaceSelectionMethod.CurrentDirectory);
        var request = new ExtensionInstallRequest(workspace, ExtensionInstallMode.Apply, ["toolkit"], all: false, sourcePath: null, force: true, automatic: true, allowInteraction: false);
        const string target = ".agents/shared/source.md";
        const string cause = "The Extension target is inside the source root of a registered workspace Library.";
        var facts = new ExtensionInstallResultFacts
        {
            Selection = new(ExtensionInstallSelectionKind.ExplicitIds, ["toolkit"]),
            Source = null,
            Packages = [new("toolkit", "1.0.0", selectedRoot: true, dependencies: [])],
            Framework = null,
            Footprint = new(1, [target], [], []),
            Effects = [],
            GeneratedNavigation = new([]),
            Lifecycle = new(ExtensionInstallLifecycleAction.None, ExtensionInstallLifecycleOutcome.NotRequested),
            Recovery = new(ExtensionInstallRecoveryState.NotRequired, [], residualPath: null),
            Verification = new(
                ExtensionInstallVerificationState.NotRequested,
                ExtensionInstallVerificationState.NotRequested,
                ExtensionInstallVerificationState.NotRequested,
                ExtensionInstallVerificationState.NotRequested),
        };
        var result = new ExtensionInstallResult(
            request,
            facts,
            [new(ExtensionInstallFindingCode.OwnershipConflict, cause, target)]);
        var rendered = CliRenderingStage.Render(
            new CliPresentationRequest<ExtensionInstallResult>(
                result,
                new(CliFormat.Text, CliDetail.Minimal, null)),
            ExtensionInstallPresentation.Rendering).PrimaryContent;

        Assert.Contains(cause, rendered, StringComparison.Ordinal);
        Assert.Contains(target, rendered, StringComparison.Ordinal);
    }
}
