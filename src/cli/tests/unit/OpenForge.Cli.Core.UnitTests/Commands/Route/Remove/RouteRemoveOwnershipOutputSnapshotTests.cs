using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Result;
using OpenForge.Cli.Core.Presentation.Route.Remove;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

[Trait("Feature", "route-remove-output"), Trait("Evidence", "Unit")]
public sealed class RouteRemoveOwnershipOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact]
    public void MinimalText()
    {
        var text = Render(CliFormat.Text, CliDetail.Minimal);

        Assert.StartsWith("Cannot remove guidance/old guide:", text, StringComparison.Ordinal);
        Assert.Contains("could not be read completely", text, StringComparison.Ordinal);
        Assert.Contains("No files were changed.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact]
    public void StandardText()
    {
        var text = Render(CliFormat.Text, CliDetail.Standard);

        Assert.StartsWith("Cannot remove guidance/old guide:", text, StringComparison.Ordinal);
        Assert.Contains("could not be read completely", text, StringComparison.Ordinal);
        Assert.Contains("No files were changed.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact]
    public void MinimalJson()
    {
        using var document = JsonDocument.Parse(Render(CliFormat.Json, CliDetail.Minimal));
        var root = document.RootElement;
        var data = root.GetProperty("data");

        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route remove", root.GetProperty("command").GetString());
        Assert.Equal("blocked", root.GetProperty("status").GetString());
        Assert.Equal("minimal", root.GetProperty("detail").GetString());
        Assert.Equal("file", data.GetProperty("subject").GetString());
        Assert.Empty(data.GetProperty("removed").EnumerateArray());
        Assert.Empty(data.GetProperty("detachedLinks").EnumerateArray());
        Assert.False(data.TryGetProperty("scan", out _));
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.Single(root.GetProperty("findings").EnumerateArray());
    }

    [Trait("Boundary", "Output")]
    [Fact]
    public void StandardJson()
    {
        using var document = JsonDocument.Parse(Render(CliFormat.Json, CliDetail.Standard));
        var root = document.RootElement;
        var finding = Assert.Single(root.GetProperty("findings").EnumerateArray());

        Assert.Equal("standard", root.GetProperty("detail").GetString());
        Assert.Equal("error", finding.GetProperty("severity").GetString());
        Assert.Equal("route-remove.ownership-unavailable", finding.GetProperty("code").GetString());
        Assert.Equal("Ownership record is unavailable", finding.GetProperty("title").GetString());
        Assert.DoesNotContain("lifecycle", finding.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    private static string Render(CliFormat format, CliDetail view)
    {
        var formation = RouteRemoveBoundary.Start(RouteRemoveTestData.Request(mode: RouteRemoveMode.DryRun)) with
        {
            Workspace = null,
            Source = RouteRemoveTestData.Source(),
            Subject = new RouteRemoveSubject { Kind = RouteRemoveSubjectKind.Leaf },
            Ownership = new RouteRemoveOwnership
            {
                State = RouteRemoveOwnershipState.NotEstablished,
                Framework = RouteRemoveOwnershipTrust.NotEstablished,
                Extensions = RouteRemoveOwnershipTrust.NotEstablished,
                Claims = [],
            },
        };
        var result = new RouteRemoveResultBuilder().Build(RouteRemoveBoundary.Stop(formation,
            RouteRemoveFindingCode.OwnershipUnavailable, CliSemanticStatus.Blocked,
            RouteRemoveTestData.LeafPath, "The ownership lock does not establish a complete inventory; no unmanaged state was inferred."));
        var request = new CliPresentationRequest<RouteRemoveResult>(result, new(format, view, null));
        return CliRenderingStage.Render(request, RouteRemovePresentation.Rendering).PrimaryContent;
    }
}
