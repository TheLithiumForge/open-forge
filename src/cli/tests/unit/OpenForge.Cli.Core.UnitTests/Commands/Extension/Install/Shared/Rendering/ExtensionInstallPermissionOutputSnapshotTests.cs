using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Install;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

using TheLithium.Imprint;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Install.Shared.Rendering;

[Trait("Feature", "shared-permission-output"), Trait("Evidence", "Unit")]
public sealed class ExtensionInstallPermissionOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Install minimal text permission approval matches its reviewed snapshot")]
    public void MinimalText() => Render(CliFormat.Text, CliDetail.Minimal).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Install standard text permission approval matches its reviewed snapshot")]
    public void StandardText() => Render(CliFormat.Text, CliDetail.Standard).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Install minimal JSON permission approval matches its reviewed snapshot")]
    public void MinimalJson() => Render(CliFormat.Json, CliDetail.Minimal).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Install standard JSON permission approval matches its reviewed snapshot")]
    public void StandardJson() => Render(CliFormat.Json, CliDetail.Standard).AssertSnapshot();

    private static string Render(CliFormat format, CliDetail view)
    {
        var workspace = new CliWorkspace("extension-human-workspace", "extension-human-workspace", CliWorkspaceSelectionMethod.CurrentDirectory);
        var request = new ExtensionInstallRequest(workspace, ExtensionInstallMode.Apply, ["toolkit"], all: false, sourcePath: null, force: false, automatic: true, allowInteraction: false);
        var facts = new ExtensionInstallResultFacts
        {
            Permissions = new(
                ["docs/first.md", "docs/not-started.md"],
                ["docs/first.md", "docs/not-started.md"],
                WorkspacePermissionDecision.Approved, WorkspacePermissionAction.Replace, WorkspacePermissionOutcome.Verified),
            Selection = new(ExtensionInstallSelectionKind.ExplicitIds, ["toolkit"]),
            Source = null,
            Packages = [new("toolkit", "1.0.0", selectedRoot: true, dependencies: [])],
            Framework = null,
            Footprint = new(1, ["docs/first.md", "docs/not-started.md"], [".agents/unchanged-navigation.md"], []),
            Effects = [new("docs/first.md", "toolkit", ExtensionInstallEffectKind.PackageFile, ExtensionInstallEffectAction.Create,
                ExtensionInstallEffectOutcome.CompletionUnknown, ExtensionInstallEffectResidual.Unknown)],
            GeneratedNavigation = new([new(".agents/unchanged-navigation.md", ExtensionInstallGeneratedRegionState.Unchanged)]),
            Lifecycle = new(ExtensionInstallLifecycleAction.Publish, ExtensionInstallLifecycleOutcome.NotStarted),
            Recovery = new(ExtensionInstallRecoveryState.Retained, [".agents/open-forge.json", ".agents/open-forge.lock.json", ".agents/protected.md"], "/recovery/bundle"),
            Verification = new(ExtensionInstallVerificationState.Unknown, ExtensionInstallVerificationState.Unknown,
                ExtensionInstallVerificationState.NotRequested, ExtensionInstallVerificationState.NotRequested),
        };
        var result = new ExtensionInstallResult(request, facts, [new(ExtensionInstallFindingCode.Interrupted, "The caller interrupted installation.")]);
        var presentation = new CliPresentationRequest<ExtensionInstallResult>(result, new(format, view, null));
        var text = CliRenderingStage.Render(presentation, ExtensionInstallPresentation.Rendering).PrimaryContent;
        // Only the test workspace identity varies with the checkout and host. The shared helper
        // requires a path delimiter after the root, so a sibling such as `<root>-other` survives.
        return CommandOutputNormalization.ReplaceDelimitedPath(text, workspace.LexicalRoot, "<workspace>");
    }
}
