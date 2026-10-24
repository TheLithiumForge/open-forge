using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Extension.Update.Shared.Wording;

[Trait("Feature", "extension-update"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
public sealed class ExtensionUpdateWordingTests
{
    private const string CustomMainRoot = "handbook/context";
    private const string ReportedTarget = "reported-recovery-location";

    [Theory(DisplayName = "Extension Update recovery labels prefer the target then the observed context folder and keep unknown layouts neutral")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, "default")]
    [InlineData(CustomMainRoot, false, "custom")]
    [InlineData(null, false, "unknown")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, "default-target")]
    [InlineData(CustomMainRoot, true, "custom-target")]
    [InlineData(null, true, "unknown-target")]
    public void RecoveryUnavailableUsesObservedContextFolder(string? mainRoot, bool hasTarget, string scenario)
    {
        var workspacePath = Path.GetFullPath("extension-recovery-workspace");
        var workspace = new CliWorkspace(workspacePath, workspacePath, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = mainRoot is null ? null : WorkspaceLayout.Create(workspace, mainRoot);
        var finding = new ExtensionUpdateFinding(
            ExtensionUpdateFindingCode.RecoveryUnavailable,
            "Recovery inspection was unavailable.",
            hasTarget ? ReportedTarget : null)
        {
            ContextFolder = layout?.MainRoot,
        };

        var label = ExtensionUpdateWording.FindingMessage(finding, "toolkit", sourcePath: null, recoveryPath: null);

        CommandOutputSnapshot.MatchSnapshot(label, scenario);
    }
}
