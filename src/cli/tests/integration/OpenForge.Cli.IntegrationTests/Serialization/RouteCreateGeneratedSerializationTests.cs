using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Create;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.IntegrationTests.Commands.Route.Create;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Serialization.Shared.Assertions;
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

    [Trait("Boundary", "Output")]
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
        var metadata = RouteCreatePresentation.Rendering.DataJsonTypeInfo;
        var presentation = new CliPresentationRequest<RouteCreateResult>(
            result,
            new CliPresentation(
                CliFormat.Json,
                CliDetail.Full, null));
        var selected = CliReportSelection.Select(
            result,
            new CliSelection(CliDetail.Full),
            RouteCreatePresentation.Rendering);

        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        Assert.Equal("RouteCreateData", metadata.Type.Name);
        Assert.Equal(JsonNamingPolicy.CamelCase, metadata.Options.PropertyNamingPolicy);
        Assert.False(metadata.Options.WriteIndented);

        var rendered = CommandOutputRenderers<RouteCreateResult>.Render(
            presentation,
            RouteCreatePresentation.Rendering);
        var expectedData = JsonSerializer.Serialize(selected.Report.Data, metadata);
        using var expectedDocument = JsonDocument.Parse(expectedData);
        using var renderedDocument = JsonDocument.Parse(rendered);
        var root = renderedDocument.RootElement;
        Assert.True(JsonElement.DeepEquals(
            expectedDocument.RootElement,
            root.GetProperty("data")));
        Assert.StartsWith("{\"schemaVersion\":3,", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("\"SchemaVersion\"", rendered, StringComparison.Ordinal);
        Schema3Assertions.Envelope(root, "route create", "completed", "full");
        AssertPropertyOrder(root.GetProperty("workspace"), "path", "selectedBy");
        var commandData = root.GetProperty("data");
        AssertPropertyOrder(commandData, "mode", "target", "listedIn", "template", "metadata", "content", "sections");
        AssertPropertyOrder(commandData.GetProperty("target"), "id", "path");
        AssertPropertyOrder(commandData.GetProperty("metadata"), "description", "responsibility", "tags");
        AssertPropertyOrder(commandData.GetProperty("sections")[0], "path", "before", "after", "verification");
        AssertPropertyOrder(root.GetProperty("recovery"), "path", "disposition");
        Assert.Equal(JsonValueKind.Null, root.GetProperty("recovery").GetProperty("path").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);

        var effects = root.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Equal(2, effects.Length);
        AssertEffectOrder(effects[0]);
        Assert.Equal(RouteCreateIntegrationWorkspace.TargetPath, effects[0].GetProperty("path").GetString());
        Assert.Equal("created", effects[0].GetProperty("action").GetString());
        Assert.Equal(JsonValueKind.Null, effects[0].GetProperty("before").ValueKind);
        Assert.Equal(Hash(ExpectedTargetDocument), effects[0].GetProperty("after").GetString());

        AssertEffectOrder(effects[1]);
        Assert.Equal(RouteCreateIntegrationWorkspace.ParentPath, effects[1].GetProperty("path").GetString());
        Assert.Equal("rewritten", effects[1].GetProperty("action").GetString());
        Assert.Equal(parentBeforeHash, effects[1].GetProperty("before").GetString());
        Assert.Equal(parentExpectedHash, effects[1].GetProperty("after").GetString());
    }

    private static void AssertEffectOrder(JsonElement effect)
        => AssertPropertyOrder(effect, "path", "kind", "action", "outcome", "reason", "owner", "before", "after");

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
