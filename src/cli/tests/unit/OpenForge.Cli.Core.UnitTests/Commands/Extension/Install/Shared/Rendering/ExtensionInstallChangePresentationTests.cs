using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Extension.Install;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Install.Shared.Rendering;

public sealed class ExtensionInstallChangePresentationTests
{
    [Theory(DisplayName = "Extension Install replacement rows and reasons follow the actual recovery disposition")]
    [InlineData(true, (int)ExtensionInstallRecoveryState.NotCreated, "would be replaced")]
    [InlineData(false, (int)ExtensionInstallRecoveryState.Removed, "replaced")]
    [InlineData(false, (int)ExtensionInstallRecoveryState.Unknown, "replaced")]
    [InlineData(false, (int)ExtensionInstallRecoveryState.Retained, "replaced (your previous file is in the recovery bundle)")]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ReplacementRecoveryLabels(bool preview, int recoveryValue, string expected)
    {
        var state = (ExtensionInstallRecoveryState)recoveryValue;
        var workspace = new CliWorkspace("extension-human-workspace", "extension-human-workspace", CliWorkspaceSelectionMethod.CurrentDirectory);
        var request = new ExtensionInstallRequest(workspace, preview ? ExtensionInstallMode.DryRun : ExtensionInstallMode.Apply,
            ["toolkit"], all: false, sourcePath: null, force: true, automatic: true, allowInteraction: false);
        var facts = new ExtensionInstallResultFacts
        {
            Selection = new(ExtensionInstallSelectionKind.ExplicitIds, ["toolkit"]),
            Source = null,
            Packages = [new("toolkit", "1.0.0", selectedRoot: true, dependencies: [])],
            Framework = null,
            Footprint = new(0, [".agents/one.md"], [], []),
            Effects = [new(".agents/one.md", "toolkit", ExtensionInstallEffectKind.PackageFile, ExtensionInstallEffectAction.Replace,
                preview ? ExtensionInstallEffectOutcome.Planned : ExtensionInstallEffectOutcome.Verified, ExtensionInstallEffectResidual.Retained)],
            GeneratedNavigation = new([]),
            Lifecycle = new(ExtensionInstallLifecycleAction.Publish, preview ? ExtensionInstallLifecycleOutcome.Planned : ExtensionInstallLifecycleOutcome.Verified),
            Recovery = new(state, [".agents/one.md"], residualPath: state == ExtensionInstallRecoveryState.Retained
                ? Path.Combine(Path.GetTempPath(), "extension-install-recovery-fixture.zip") : null),
            Verification = new(ExtensionInstallVerificationState.NotRequested, ExtensionInstallVerificationState.NotRequested,
                ExtensionInstallVerificationState.NotRequested, ExtensionInstallVerificationState.NotRequested),
        };
        var report = ExtensionInstallPresentation.Rendering.Selector(new(request, facts, []), new(CliDetail.Minimal));
        Assert.Equal(expected, Assert.Single(report.Data.TextRows).Text);
        var reason = Assert.Single(report.Effects).Reason;
        if (state == ExtensionInstallRecoveryState.Retained)
            Assert.Equal("your previous file is in the recovery bundle", reason);
        else
            Assert.Null(reason);
    }
}
