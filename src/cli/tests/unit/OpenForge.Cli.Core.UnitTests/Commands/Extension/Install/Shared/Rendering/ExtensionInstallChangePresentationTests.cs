using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Extension.Install;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Install.Shared.Rendering;

public sealed class ExtensionInstallChangePresentationTests
{
    [Theory(DisplayName = "Blocked and cancelled Extension Install never describes unstarted Entries effects as completed updates"), InlineData(false), InlineData(true)]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void BlockedEntriesEffectsHaveNoCompletedUpdateSummary(bool cancelled)
    {
        var workspace = new CliWorkspace("extension-human-workspace", "extension-human-workspace", CliWorkspaceSelectionMethod.CurrentDirectory);
        var request = new ExtensionInstallRequest(workspace, ExtensionInstallMode.Apply,
            ["toolkit"], all: false, sourcePath: null, force: false, automatic: true, allowInteraction: false);
        const string host = ".agents/patterns/_patterns.md";
        var facts = new ExtensionInstallResultFacts
        {
            Selection = new(ExtensionInstallSelectionKind.ExplicitIds, ["toolkit"]),
            Source = null,
            Packages = [new("toolkit", "1.0.0", selectedRoot: true, dependencies: [])],
            Framework = null,
            Footprint = new(0, [], [host], []),
            Effects = [new(host, null, ExtensionInstallEffectKind.GeneratedRegion, ExtensionInstallEffectAction.Replace,
                ExtensionInstallEffectOutcome.NotStarted, ExtensionInstallEffectResidual.None)],
            GeneratedNavigation = new([new(host, ExtensionInstallGeneratedRegionState.Changed)]),
            Lifecycle = new(ExtensionInstallLifecycleAction.Publish, ExtensionInstallLifecycleOutcome.NotStarted),
            Recovery = new(ExtensionInstallRecoveryState.NotCreated, [], null),
            Verification = new(ExtensionInstallVerificationState.Unknown, ExtensionInstallVerificationState.Unknown,
                ExtensionInstallVerificationState.Unknown, ExtensionInstallVerificationState.Unknown),
        };
        var result = new ExtensionInstallResult(request, facts,
            [new(cancelled ? ExtensionInstallFindingCode.Interrupted : ExtensionInstallFindingCode.TargetChanged, "Application did not start.", host)]);

        var report = ExtensionInstallPresentation.Rendering.Selector(result, new(CliDetail.Minimal));

        Assert.Equal(cancelled ? CliSemanticStatus.Interrupted : CliSemanticStatus.Blocked, report.Status);
        Assert.Empty(report.Data.TextDetails);
        Assert.Equal(CliEffectOutcome.NotStarted, Assert.Single(report.Effects).Outcome);
        Assert.Equal(host, Assert.Single(report.Data.TextRows).Path);
    }

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
