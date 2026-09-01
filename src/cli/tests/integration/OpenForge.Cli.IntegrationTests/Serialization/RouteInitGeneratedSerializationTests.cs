using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class RouteInitGeneratedSerializationTests
{
    [Fact(DisplayName = "Route Init JSON document uses the registered source-generated presentation graph"), Trait("Feature", "route-init-presentation"), Trait("Evidence", "Integration")]
    public void DocumentUsesRegisteredSourceGeneratedPresentationGraph()
    {
        var document = new RouteInitJsonDocument
        {
            SchemaVersion = 1,
            Command = "route init",
            Status = "complete",
            Workspace = null,
            Result = new RouteInitJsonResult
            {
                Mode = "dry-run",
                Scaffold = "generic",
                Target = new RouteInitJsonTarget
                {
                    Requested = "memory/project-alpha/documents",
                    Id = "memory/project-alpha/documents",
                    Path = ".agents/memory/project-alpha/documents/_documents.md",
                },
                Plan = new RouteInitJsonPlan
                {
                    Completeness = "complete",
                    Safety = "safe",
                },
                Framework = null,
                Entrypoints = [],
                Effects = [],
                UnchangedPaths = [],
                Lifecycle = new RouteInitJsonLifecycle
                {
                    Action = "none",
                    Outcome = "not-requested",
                },
                Recovery = new RouteInitJsonRecovery
                {
                    State = "not-created",
                    ResidualPath = null,
                },
                Verification = "not-requested",
                Findings = [],
            },
            Next = null,
        };

        var metadata = RouteInitJsonContext.Default.RouteInitJsonDocument;
        Assert.Equal(typeof(RouteInitJsonDocument), metadata.Type);
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);

        var json = JsonSerializer.Serialize(document, metadata);

        using var parsed = JsonDocument.Parse(json);
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            parsed.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal("route init", parsed.RootElement.GetProperty("command").GetString());
        Assert.Equal("complete", parsed.RootElement.GetProperty("status").GetString());
        var result = parsed.RootElement.GetProperty("result");
        Assert.Equal(
            ["mode", "scaffold", "target", "plan", "framework", "entrypoints", "effects", "unchangedPaths", "lifecycle", "recovery", "verification", "findings"],
            result.EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal("generic", result.GetProperty("scaffold").GetString());
        Assert.Equal(
            ".agents/memory/project-alpha/documents/_documents.md",
            result.GetProperty("target").GetProperty("path").GetString());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("framework").ValueKind);
    }
}
