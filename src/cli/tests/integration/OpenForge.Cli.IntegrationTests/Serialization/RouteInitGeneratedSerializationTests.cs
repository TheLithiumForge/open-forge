using System.Text.Json;
using OpenForge.Cli.Core.Presentation.Route.Init.Models;
using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Rendering;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class RouteInitGeneratedSerializationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Init JSON data uses its registered source-generated presentation graph"),
     Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public void DataUsesRegisteredSourceGeneratedPresentationGraph()
    {
        var data = new RouteInitData
        {
            Mode = "dry-run",
            Target = new RouteInitDataTarget
            {
                Id = "memory/project-alpha/documents",
                Path = ".agents/memory/project-alpha/documents/_documents.md",
            },
            Scaffold = "generic",
            Entrypoints =
            [
                new RouteInitDataEntrypoint
                {
                    Path = ".agents/memory/project-alpha/documents/_documents.md",
                    Outcome = "planned",
                    NeedsAuthoring = true,
                },
            ],
            ListedIn = [],
        };

        var metadata = RouteInitDataJsonContext.Default.RouteInitData;
        Assert.Equal(typeof(RouteInitData), metadata.Type);
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);

        var json = JsonSerializer.Serialize(data, metadata);

        using var parsed = JsonDocument.Parse(json);
        Assert.Equal(
            ["mode", "target", "scaffold", "entrypoints", "listedIn"],
            parsed.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", parsed.RootElement.GetProperty("mode").GetString());
        Assert.Equal("generic", parsed.RootElement.GetProperty("scaffold").GetString());
        Assert.True(parsed.RootElement.GetProperty("entrypoints")[0].GetProperty("needsAuthoring").GetBoolean());
    }
}
