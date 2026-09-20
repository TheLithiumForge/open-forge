using System.Text.Json;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Rendering;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class RouteInspectGeneratedSerializationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Inspect data uses generated metadata for the complete native data graph")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public void InspectDataUsesGeneratedMetadata()
    {
        var data = new RouteInspectData
        {
            Id = "root",
            Path = ".agents/root/_root.md",
            Kind = "entrypoint",
            EntrypointForm = "canonical",
            OverwritePath = ".agents/root/_root.overwrite.md",
            Belongs = new RouteInspectDataBelongs
            {
                RouteChain = ["root"],
                Parent = null,
                DirectChildren = new RouteInspectDataCounts { Files = 1, Entrypoints = 2 },
                Descendants = new RouteInspectDataCounts { Files = 3, Entrypoints = 4 },
            },
            Read = new RouteInspectDataRead
            {
                AtStart = true,
                AutomaticallyWhen = ["the Loader is read"],
                MayReadAgain = false,
            },
            Size = new RouteInspectDataSize
            {
                Own = new RouteInspectDataMeasurement { Files = 1, Bytes = 4, Tokens = 1 },
                Adds = new RouteInspectDataMeasurement { Files = 1, Bytes = 4, Tokens = 1 },
                LoadNow = null,
            },
            Axioms = new RouteInspectDataAxioms
            {
                InheritedFrom = ["loader"],
                Local = "substantive local Axioms",
            },
            Tags = ["Guidance", "Team"],
            Selected = new RouteInspectDataSelected
            {
                Closure = new RouteInspectDataMeasurement { Files = 2, Bytes = 8, Tokens = 2 },
                StartupOverlap = new RouteInspectDataMeasurement { Files = 1, Bytes = 4, Tokens = 1 },
            },
            Selection = new RouteInspectDataSelection
            {
                Kind = "source-id",
                Method = "automatic-id",
                Requested = "root",
            },
            Layers =
            [
                new RouteInspectDataLayer { Path = ".agents/root/_root.md", Kind = "base" },
                new RouteInspectDataLayer { Path = ".agents/root/_root.overwrite.md", Kind = "overwrite" },
            ],
            StatusReason = "all applicable route facts are available",
        };

        var metadata = RouteInspectDataJsonContext.Default.RouteInspectData;
        Assert.Equal(typeof(RouteInspectData), metadata.Type);
        var json = JsonSerializer.Serialize(data, metadata);

        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(
            ["id", "path", "kind", "entrypointForm", "overwritePath", "belongs", "read", "size", "axioms", "tags", "selected", "selection", "layers", "statusReason"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal("root", root.GetProperty("id").GetString());
        Assert.Equal(".agents/root/_root.overwrite.md", root.GetProperty("overwritePath").GetString());
        Assert.Equal(["Guidance", "Team"], root.GetProperty("tags").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(JsonValueKind.Null, root.GetProperty("size").GetProperty("loadNow").ValueKind);
        Assert.Equal("automatic-id", root.GetProperty("selection").GetProperty("method").GetString());
        Assert.Equal(
            [".agents/root/_root.md", ".agents/root/_root.overwrite.md"],
            root.GetProperty("layers").EnumerateArray().Select(layer => layer.GetProperty("path").GetString()));
    }
}
