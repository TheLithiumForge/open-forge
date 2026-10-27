using System.Text.Json;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Status.Models;
using OpenForge.Cli.Core.Presentation.Status.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Status.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

[Trait("Boundary", "Processing"), Trait("Feature", "status-outside"), Trait("Evidence", "Unit")]
public sealed class StatusOutsidePresentationTests
{
    [Theory, InlineData(false), InlineData(true)]
    public void UnknownInventoryRetainsOneCanonicalTypedCauseWithOrWithoutAnExistingRouteFact(bool existingFact)
    {
        var seed = StatusAggregationObservationSeed.Create();
        var settings = new WorkspaceSettingsRead(WorkspaceSettingsReadState.Invalid, WorkspaceSettingsDocument.Empty,
            Path.Combine(seed.Layout!.Workspace.LexicalRoot, WorkspaceSettingsDefinitions.RelativePath), "Independent settings failure");
        var retained = new WorkspaceLayoutRead(WorkspaceLayoutReadState.Invalid, settings,
            WorkspaceOwnershipRead.Absent(settings.LogicalPath + ".lock"), null, [], []);
        var outside = new RouteOutsideRoutesObservation
        {
            LayoutRead = retained,
            RegistrationInventoryComplete = false,
            Routes = [],
            Layers = [],
            Total = seed.Routes.TotalAvailable,
            Startup = seed.Routes.CurrentStartup,
            Continuity = seed.Routes.Continuity,
        };
        var result = StatusResultBuilder.Build(StatusObservationSeeds.Request(), seed with
        {
            Layout = null,
            LayoutRead = retained,
            OutsideRoutes = outside,
            Routes = seed.Routes with
            {
                State = existingFact ? OperationalViewState.Incomplete : OperationalViewState.Complete,
                ContextUnavailablePath = existingFact ? WorkspaceSettingsDefinitions.RelativePath : null,
                ContextUnavailableCause = existingFact ? settings.Cause : null,
            },
        });
        var finding = Assert.Single(result.Findings, finding => finding.Code == StatusFindingCode.ContextInventoryIncomplete);
        Assert.Equal(WorkspaceSettingsDefinitions.RelativePath, finding.Subject);
        Assert.Equal(settings.Cause, finding.Cause);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Null(result.Facts.Context.OutsideRoutes);
        var data = Assert.IsType<StatusData>(StatusReportSelector.Select(result, new CliSelection(CliDetail.Full)).Data);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(data, StatusDataJsonContext.Default.StatusData));
        Assert.False(json.RootElement.TryGetProperty(PublishedTokens.StatusOutsideRoutes.Collection, out _));
    }

    [Theory]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3)]
    public void EveryDetailPreservesDeclarationOrderAndUnknownListing(int detailValue)
    {
        var rows = new StatusOutsideRoute[]
        {
            new("docs/z", StatusOutsideRouteState.Available, true),
            new("docs/a", StatusOutsideRouteState.Missing, false),
            new("docs/b", StatusOutsideRouteState.Unavailable, null),
            new("docs/c", StatusOutsideRouteState.Unsafe, null),
        };
        var seed = StatusResultSeeds.Representative(CliSemanticStatus.Complete);
        var result = seed with { Facts = seed.Facts with { Context = seed.Facts.Context with { OutsideRoutes = rows } } };
        var data = Assert.IsType<StatusData>(StatusReportSelector.Select(result, new CliSelection((CliDetail)detailValue)).Data);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(data, StatusDataJsonContext.Default.StatusData));
        var actual = json.RootElement.GetProperty(PublishedTokens.StatusOutsideRoutes.Collection).EnumerateArray().ToArray();
        Assert.Equal(rows.Select(row => row.Path), actual.Select(row => row.GetProperty(PublishedTokens.Json.Path).GetString()));
        Assert.Equal(new[] { PublishedTokens.StatusOutsideRoutes.Available, PublishedTokens.StatusOutsideRoutes.Missing,
                PublishedTokens.StatusOutsideRoutes.Unavailable, PublishedTokens.StatusOutsideRoutes.Unsafe },
            actual.Select(row => row.GetProperty(PublishedTokens.StatusOutsideRoutes.State).GetString()));
        Assert.True(actual[0].GetProperty(PublishedTokens.StatusOutsideRoutes.Listed).GetBoolean());
        Assert.False(actual[1].GetProperty(PublishedTokens.StatusOutsideRoutes.Listed).GetBoolean());
        Assert.All(actual.Skip(2), row => Assert.False(row.TryGetProperty(PublishedTokens.StatusOutsideRoutes.Listed, out _)));
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public void AbsentOrEmptyCollectionPreservesThePreviousDataShape(bool knownEmpty)
    {
        var seed = StatusResultSeeds.Representative();
        var result = seed with
        {
            Facts = seed.Facts with
            {
                Context = seed.Facts.Context with
                { OutsideRoutes = knownEmpty ? [] : null }
            }
        };
        var data = Assert.IsType<StatusData>(StatusReportSelector.Select(result, new CliSelection(CliDetail.Full)).Data);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(data, StatusDataJsonContext.Default.StatusData));
        Assert.False(json.RootElement.TryGetProperty(PublishedTokens.StatusOutsideRoutes.Collection, out _));
    }

    [Fact]
    public void OutsideStateVocabularyIsClosedAndRejectsUndefinedValues()
    {
        Assert.Equal(new[] { StatusOutsideRouteState.Available, StatusOutsideRouteState.Missing,
            StatusOutsideRouteState.Unsafe, StatusOutsideRouteState.Unavailable }, Enum.GetValues<StatusOutsideRouteState>());
        Assert.Throws<ArgumentOutOfRangeException>(() => StatusOutsideRouteStates.Read((StatusOutsideRouteState)int.MaxValue));
    }
}
