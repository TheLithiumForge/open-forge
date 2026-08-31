using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.IntegrationTests.Commands.Route.Create;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class RouteCreateGeneratedSerializationTests
{
    private const string ExpectedTargetDocument = "---\n"
        + "open-forge:\n"
        + "  description: Project overview\n"
        + "  tags: [Docs, Overview]\n"
        + "  responsibility: Explains the project\n"
        + "---\n";

    [Fact(DisplayName = "Route Create renderer uses command-local generated metadata for its populated ordered graph"), Trait("Feature", "route-create"), Trait("Evidence", "Integration")]
    public async Task RendererUsesCommandLocalGeneratedMetadataForPopulatedGraph()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-generated-serialization");
        workspace.SeedBase();
        var parentBeforeHash = Hash(workspace.ReadText(
            RouteCreateIntegrationWorkspace.ParentPath));
        var parentExpectedHash = Hash(ParentDocument(
            "- [Project overview](overview.md) - #Docs #Overview"));
        var result = await RouteCreateOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(RouteCreateMode.DryRun),
                TestContext.Current.CancellationToken);
        var presentation = new CliPresentationRequest<RouteCreateResult>(
            result,
            new CliPresentation(
                CliOutputFormat.Json,
                CliView.Expanded,
                CliVerbosity.Normal));
        var context = RouteCreateJsonContext.Default;
        var metadata = context.RouteCreateJsonDocument;

        Assert.Same(metadata, context.GetTypeInfo(typeof(RouteCreateJsonDocument)));
        Assert.Equal(typeof(RouteCreateJsonDocument), metadata.Type);
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        Assert.True(context.Options.WriteIndented);
        Assert.Equal(JsonNamingPolicy.CamelCase, context.Options.PropertyNamingPolicy);

        var rendered = RouteCreateJsonRenderer.Render(presentation);
        var expected = JsonSerializer.Serialize(
            RouteCreateJsonProjection.Create(result),
            metadata);

        Assert.Equal(expected, rendered);
        Assert.StartsWith("{\n  \"schemaVersion\": 1,", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("\"SchemaVersion\"", rendered, StringComparison.Ordinal);
        using var parsed = JsonDocument.Parse(rendered);
        var root = parsed.RootElement;
        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        AssertPropertyOrder(root.GetProperty("workspace"), "path", "selectedBy");
        var commandResult = root.GetProperty("result");
        AssertPropertyOrder(
            commandResult,
            "mode",
            "target",
            "parent",
            "metadata",
            "template",
            "plan",
            "effects",
            "unchangedPaths",
            "recovery",
            "verification",
            "findings");
        AssertPropertyOrder(commandResult.GetProperty("target"), "requested", "id", "path");
        AssertPropertyOrder(commandResult.GetProperty("parent"), "id", "path", "form");
        AssertPropertyOrder(commandResult.GetProperty("metadata"), "description", "responsibility", "tags");
        AssertPropertyOrder(commandResult.GetProperty("plan"), "completeness", "safety");
        AssertPropertyOrder(commandResult.GetProperty("recovery"), "state", "residualPath");
        Assert.Equal(JsonValueKind.Null, commandResult.GetProperty("template").ValueKind);
        Assert.Equal(
            JsonValueKind.Null,
            commandResult.GetProperty("recovery").GetProperty("residualPath").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);

        var effects = commandResult.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Equal(2, effects.Length);
        AssertEffectOrder(effects[0]);
        Assert.Equal(RouteCreateIntegrationWorkspace.TargetPath, effects[0].GetProperty("path").GetString());
        Assert.Equal("create", effects[0].GetProperty("action").GetString());
        var targetChange = effects[0].GetProperty("change");
        Assert.Equal(JsonValueKind.Null, targetChange.GetProperty("before").ValueKind);
        Assert.Equal(Hash(ExpectedTargetDocument), targetChange.GetProperty("expected").GetString());

        AssertEffectOrder(effects[1]);
        Assert.Equal(RouteCreateIntegrationWorkspace.ParentPath, effects[1].GetProperty("path").GetString());
        Assert.Equal("replace", effects[1].GetProperty("action").GetString());
        var parentChange = effects[1].GetProperty("change");
        Assert.Equal(JsonValueKind.String, parentChange.GetProperty("before").ValueKind);
        Assert.Equal(parentBeforeHash, parentChange.GetProperty("before").GetString());
        Assert.Equal(parentExpectedHash, parentChange.GetProperty("expected").GetString());
    }

    private static void AssertEffectOrder(JsonElement effect)
    {
        AssertPropertyOrder(effect, "path", "kind", "action", "change", "outcome", "residual");
        AssertPropertyOrder(effect.GetProperty("change"), "before", "expected");
    }

    private static string ParentDocument(string entry)
        => OpenForgeDocumentSeed.Metadata(
            description: "Project Alpha",
            tags: ["Project"],
            body: $"\n{OpenForgeDocumentSeed.GeneratedEntries(entry)}");

    private static string Hash(string contents)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(contents)));

    private static void AssertPropertyOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));
}
