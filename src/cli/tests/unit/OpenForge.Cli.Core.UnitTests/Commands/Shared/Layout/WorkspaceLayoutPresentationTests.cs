using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Result;
using OpenForge.Cli.Core.Commands.Shared.Layout;
using OpenForge.Cli.Core.Commands.Shared.Models.Layout;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Install.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Install.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Status.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Status.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.Layout;

[Trait("Feature", "workspace-layout"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
public sealed class WorkspaceLayoutPresentationTests
{
    private const string MainRoot = "docs/context";
    private const string CustomSnapshot = "custom-layout";
    private const string StatusSnapshot = "custom-status-layout";
    private const string UnavailableCause = "Fixture observation unavailable";
    private static readonly CliWorkspace Workspace = new(
        Path.GetFullPath(nameof(WorkspaceLayoutPresentationTests)),
        Path.GetFullPath(nameof(WorkspaceLayoutPresentationTests)), CliWorkspaceSelectionMethod.ExplicitWorkspace);

    [Fact(DisplayName = "Default and unavailable layouts have no optional output facts")]
    public void DefaultAndUnavailableLayoutsAreOmitted()
    {
        Assert.Null(WorkspaceLayoutProjection.Project(null));
        var facts = WorkspaceLayoutProjection.Project(WorkspaceLayout.Create(Workspace, WorkspaceLayout.DefaultMainRoot));
        Assert.Null(facts);
        var data = Select(facts);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(data, InstallDataJsonContext.Default.InstallData));
        Assert.False(document.RootElement.TryGetProperty(PublishedTokens.LayoutData.Layout, out _));
    }

    [Fact(DisplayName = "Install carries custom layout facts into structured and text output")]
    public void CustomLayoutIsProjectedAndRendered()
    {
        var facts = Assert.IsType<WorkspaceLayoutInfo>(WorkspaceLayoutProjection.Project(WorkspaceLayout.Create(Workspace, MainRoot)));
        var loader = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        Assert.Equal(MainRoot, facts.MainRoot);
        Assert.Equal(loader, facts.LoaderPath);
        var data = Select(facts);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(data, InstallDataJsonContext.Default.InstallData));
        var layout = document.RootElement.GetProperty(PublishedTokens.LayoutData.Layout);
        Assert.Equal(MainRoot, layout.GetProperty(PublishedTokens.LayoutData.MainRoot).GetString());
        Assert.Equal(loader, layout.GetProperty(PublishedTokens.LayoutData.LoaderPath).GetString());
        CommandOutputSnapshot.MatchSnapshot(
            InstallDataTextRenderer.Render(data, new(CliDetail.Standard, null), CliTextStyle.Plain).Content, CustomSnapshot);
    }

    private static OpenForge.Cli.Core.Presentation.Install.Models.InstallData Select(WorkspaceLayoutInfo? layout)
    {
        var result = new InstallResult(null, new InstallBindingInput(false, false, InstallMode.DryRun), [],
            facts: InstallResultFactsFactory.Empty() with { Layout = layout });
        return InstallReportSelector.Select(result, new CliSelection(CliDetail.Standard, null)).Data;
    }

    [Fact(DisplayName = "Status carries optional layout facts through its report and source-generated output")]
    public void StatusLayoutIsProjectedAndRendered()
    {
        var result = StatusResultBuilder.Event(null, StatusFindingCode.EntryUnavailable, null, UnavailableCause);
        var selection = new CliSelection(CliDetail.Standard, null);
        var absent = StatusReportSelector.Select(result, selection).Data;
        using var defaultDocument = JsonDocument.Parse(JsonSerializer.Serialize(absent, StatusDataJsonContext.Default.StatusData));
        Assert.False(defaultDocument.RootElement.TryGetProperty(PublishedTokens.LayoutData.Layout, out _));
        result = result with { Layout = new WorkspaceLayoutInfo(MainRoot, MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName) };
        var data = StatusReportSelector.Select(result, selection).Data;
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(data, StatusDataJsonContext.Default.StatusData));
        var layout = document.RootElement.GetProperty(PublishedTokens.LayoutData.Layout);
        Assert.Equal(MainRoot, layout.GetProperty(PublishedTokens.LayoutData.MainRoot).GetString());
        Assert.Equal(MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, layout.GetProperty(PublishedTokens.LayoutData.LoaderPath).GetString());
        CommandOutputSnapshot.MatchSnapshot(StatusDataTextRenderer.Render(data, selection, CliTextStyle.Plain).Content, StatusSnapshot);
    }
}
