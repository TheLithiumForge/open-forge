using OpenForge.Cli.Core.Presentation.Route.Create;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

public sealed class RouteCreatePresentationContractTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Create selector preserves the accepted typed data"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void SelectorPreservesAcceptedTypedData()
    {
        var selected = CliReportSelection.Select(
            RouteCreateTestData.Result(),
            new CliSelection(CliDetail.Standard),
            RouteCreatePresentation.Rendering);
        var report = selected.Report;
        var data = report.Data;

        Assert.Equal("route create", report.Command);
        Assert.Equal(CliSemanticStatus.Complete, report.Status);
        Assert.Equal("apply", data.Mode);
        Assert.Equal(RouteCreateTestData.TargetId, data.Target.Id);
        Assert.Equal(RouteCreateTestData.TargetPath, data.Target.Path);
        Assert.Equal(RouteCreateTestData.ParentPath, data.ListedIn);
        Assert.Equal("Project overview", data.Metadata?.Description);
        Assert.Equal("Explains the project", data.Metadata?.Responsibility);
        Assert.Equal(["Docs", "Overview"], data.Metadata?.Tags);
        Assert.Null(data.Template);
        var effect = Assert.Single(report.Effects);
        Assert.Equal(RouteCreateTestData.TargetPath, effect.Path);
        Assert.Equal(CliEffectKind.File, effect.Kind);
        Assert.Equal(CliEffectAction.Created, effect.Action);
        Assert.Equal(CliEffectOutcome.Done, effect.Outcome);
        var fileCount = report.Counts.Single(count => count.Name == "filesCreated");
        Assert.Equal(1, fileCount.Value);
        Assert.Empty(report.Findings);
        Assert.Null(report.Next);
    }
}
