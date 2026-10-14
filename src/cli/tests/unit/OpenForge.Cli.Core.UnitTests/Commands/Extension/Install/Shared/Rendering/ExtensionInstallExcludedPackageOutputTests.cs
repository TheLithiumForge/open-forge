using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Extension.Install;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Install.Shared.Rendering;

public sealed class ExtensionInstallExcludedPackageOutputTests
{
    [Theory(DisplayName = "Fully excluded multiple Extensions name their records categories counts and complete delivery steps")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void MultipleExcludedPackages(bool preview)
    {
        var workspace = new CliWorkspace("excluded-package-workspace", "excluded-package-workspace", CliWorkspaceSelectionMethod.CurrentDirectory);
        var request = new ExtensionInstallRequest(workspace, preview ? ExtensionInstallMode.DryRun : ExtensionInstallMode.Apply,
            ["first", "second"], all: false, sourcePath: null, force: false, automatic: true, allowInteraction: false);
        var facts = new ExtensionInstallResultFacts
        {
            Selection = new(ExtensionInstallSelectionKind.ExplicitIds, ["first", "second"]),
            Source = null,
            Packages = [new("first", "1.0.0", selectedRoot: true, dependencies: []), new("second", "1.0.0", selectedRoot: true, dependencies: [])],
            ExcludedPackages =
            [
                new(Id: "first", Name: "First", FileCount: 2, Categories: ["templates"], EntirePayloadExcluded: true),
                new(Id: "second", Name: "Second", FileCount: 1, Categories: ["guidance"], EntirePayloadExcluded: true),
            ],
            Framework = null,
            Footprint = null,
            Effects = [],
            GeneratedNavigation = null,
            Lifecycle = new(ExtensionInstallLifecycleAction.Publish,
                preview ? ExtensionInstallLifecycleOutcome.Planned : ExtensionInstallLifecycleOutcome.Verified),
            Recovery = new(ExtensionInstallRecoveryState.NotRequired, [], residualPath: null),
            Verification = new(ExtensionInstallVerificationState.NotRequested, ExtensionInstallVerificationState.NotRequested,
                ExtensionInstallVerificationState.NotRequested, ExtensionInstallVerificationState.NotRequested),
        };
        var result = new ExtensionInstallResult(request, facts, []);
        var output = CliRenderingStage.Render(new CliPresentationRequest<ExtensionInstallResult>(
            result, new(CliFormat.Text, CliDetail.Minimal, null)), ExtensionInstallPresentation.Rendering);
        CommandOutputSnapshot.MatchSnapshot(output.PrimaryContent, preview ? "preview" : "applied");
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Effects);
    }
}
