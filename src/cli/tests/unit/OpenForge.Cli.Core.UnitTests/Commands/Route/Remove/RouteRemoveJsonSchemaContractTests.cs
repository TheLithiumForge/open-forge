using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Presentation.Route.Remove;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemoveJsonSchemaContractTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Remove JSON keeps the shared schema-v3 envelope and native data order"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void JsonKeepsEnvelopeAndNativeDataOrder()
    {
        using var document = JsonDocument.Parse(Render(CliDetail.Minimal));
        var root = document.RootElement;
        var data = root.GetProperty("data");
        var source = data.GetProperty("source");
        var detachedLink = Assert.Single(data.GetProperty("detachedLinks").EnumerateArray());

        AssertPropertyOrder(root,
            "schemaVersion", "command", "status", "detail", "filter", "workspace", "summary",
            "findings", "effects", "counts", "limitations", "data", "recovery", "next");
        AssertPropertyOrder(data, "mode", "subject", "source", "removed", "detachedLinks");
        AssertPropertyOrder(source, "id", "path");
        AssertPropertyOrder(detachedLink, "path", "location");
        AssertPropertyOrder(root.GetProperty("counts"), "filesRemoved", "sectionsUpdated", "linksDetached", "filesScanned", "errors");
        AssertPropertyOrder(root.GetProperty("effects")[0], "path", "kind", "action", "outcome", "reason", "owner");
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route remove", root.GetProperty("command").GetString());
        Assert.Equal("blocked", root.GetProperty("status").GetString());
        Assert.Equal("apply", data.GetProperty("mode").GetString());
        Assert.Equal("file", data.GetProperty("subject").GetString());
        Assert.Equal("3:5", detachedLink.GetProperty("location").GetString());
        Assert.False(detachedLink.TryGetProperty("before", out _));
        Assert.False(detachedLink.TryGetProperty("after", out _));
        Assert.False(root.GetProperty("effects")[0].TryGetProperty("before", out _));
        Assert.False(root.GetProperty("effects")[0].TryGetProperty("after", out _));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Remove JSON adds standard link text and full scan and effect evidence"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void JsonAddsStandardAndFullEvidence()
    {
        using var standard = JsonDocument.Parse(Render(CliDetail.Standard));
        var standardLink = Assert.Single(standard.RootElement.GetProperty("data").GetProperty("detachedLinks").EnumerateArray());
        AssertPropertyOrder(standardLink, "path", "location", "before", "after");
        Assert.Equal("See [Old guide](.agents/guidance/old%20guide.md#part).", standardLink.GetProperty("before").GetString());
        Assert.Equal("See Old guide.", standardLink.GetProperty("after").GetString());

        using var full = JsonDocument.Parse(Render(CliDetail.Full));
        var root = full.RootElement;
        var data = root.GetProperty("data");
        var effect = root.GetProperty("effects")[0];

        AssertPropertyOrder(data, "mode", "subject", "source", "removed", "detachedLinks", "scan");
        AssertPropertyOrder(data.GetProperty("scan"), "filesScanned", "occurrences");
        Assert.True(effect.TryGetProperty("before", out _));
        Assert.True(effect.TryGetProperty("after", out _));
        Assert.Equal(4, data.GetProperty("scan").GetProperty("filesScanned").GetInt32());
        Assert.Equal(1, data.GetProperty("scan").GetProperty("occurrences").GetInt32());
    }

    private static string Render(CliDetail detail)
    {
        var formation = RouteRemoveTestData.Formation() with
        {
            Findings =
            [
                new RouteRemoveFinding(
                    RouteRemoveFindingCode.ReferenceUnsafe,
                    CliSemanticStatus.Blocked,
                    RouteRemoveTestData.LeafPath,
                    "The incoming link cannot be detached safely.",
                    new SourceLocation(3, 5, 12, 49)),
            ],
        };
        var result = new RouteRemoveResult(
            formation,
            CliSemanticStatus.Blocked,
            null);
        var request = new CliPresentationRequest<RouteRemoveResult>(
            result,
            new CliPresentation(CliFormat.Json, detail, null));
        return CliRenderingStage.Render(request, RouteRemovePresentation.Rendering).PrimaryContent;
    }

    private static void AssertPropertyOrder(JsonElement element, params string[] expected)
        => Assert.Equal(expected, element.EnumerateObject().Select(property => property.Name));
}
