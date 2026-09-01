using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class ExtensionCreateGeneratedSerializationTests
{
    [Fact(DisplayName = "Extension Create JSON document uses generated metadata with exact null and property coordinates"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public void DocumentUsesGeneratedMetadataWithExactCoordinates()
    {
        var document = new ExtensionCreateJsonDocument
        {
            SchemaVersion = 1,
            Command = "extension create",
            Status = "invalid",
            Workspace = null,
            Result = new ExtensionCreateJsonResult
            {
                Catalogue = null,
                Destination = null,
                Id = null,
                Manifest = null,
                Mode = "apply",
                IntendedEffects = [],
                AppliedEffects = [],
                Verification = new ExtensionCreateJsonVerification
                {
                    Catalogue = "not-started",
                    Destination = "not-started",
                    Manifest = "not-started",
                    Payload = "not-started",
                    Cause = "A stable ID is required.",
                },
                WorkspaceLifecycleChanged = false,
            },
            Next = null,
        };

        var json = JsonSerializer.Serialize(
            document,
            ExtensionCreateJsonContext.Default.ExtensionCreateJsonDocument);

        using var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        var result = root.GetProperty("result");
        Assert.Equal(
            ["catalogue", "destination", "id", "manifest", "mode", "intendedEffects", "appliedEffects", "verification", "workspaceLifecycleChanged"],
            result.EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Null, result.GetProperty("catalogue").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("destination").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("id").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("manifest").ValueKind);
        Assert.False(result.GetProperty("workspaceLifecycleChanged").GetBoolean());
    }

    [Fact(DisplayName = "Extension Create JSON renderer preserves exact generated projection order and result parity"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public void RendererPreservesGeneratedProjectionOrderAndParity()
    {
        var result = CompleteResult();
        var presentation = new CliPresentationRequest<ExtensionCreateResult>(
            result,
            new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal));

        var rendered = ExtensionCreateJsonRenderer.Render(presentation);
        var expected = JsonSerializer.Serialize(
            ExtensionCreateJsonProjection.Create(result),
            ExtensionCreateJsonContext.Default.ExtensionCreateJsonDocument);

        Assert.Equal(expected, rendered);
        using var parsed = JsonDocument.Parse(rendered);
        var commandResult = parsed.RootElement.GetProperty("result");
        Assert.Equal(
            ["catalogue", "destination", "id", "manifest", "mode", "intendedEffects", "appliedEffects", "verification", "workspaceLifecycleChanged"],
            commandResult.EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["name", "description", "version", "dependencies"],
            commandResult.GetProperty("manifest").EnumerateObject().Select(property => property.Name));
        Assert.Equal(["alpha", "zeta"], commandResult.GetProperty("manifest").GetProperty("dependencies").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(JsonValueKind.Null, parsed.RootElement.GetProperty("next").ValueKind);
    }

    private static ExtensionCreateResult CompleteResult()
        => new()
        {
            Status = CliSemanticStatus.Complete,
            Catalogue = "/catalogue",
            Destination = "/catalogue/development-toolkit",
            StableId = "development-toolkit",
            Manifest = new ExtensionCreateManifest
            {
                Id = "development-toolkit",
                Name = "Development Toolkit",
                Description = "Open Forge Extension package development-toolkit.",
                Version = "0.1.0",
                Dependencies = ["alpha", "zeta"],
            },
            Mode = ExtensionCreateMode.DryRun,
            IntendedEffects =
            [
                new ExtensionCreateEffect
                {
                    Kind = ExtensionCreateEffectKind.ManifestFile,
                    Path = "/catalogue/development-toolkit/extension.json",
                },
                new ExtensionCreateEffect
                {
                    Kind = ExtensionCreateEffectKind.PayloadAgentsDirectory,
                    Path = "/catalogue/development-toolkit/payload/.agents/",
                },
            ],
            AppliedEffects = [],
            Verification = new ExtensionCreateVerification
            {
                Catalogue = ExtensionCreateVerificationState.Verified,
                Destination = ExtensionCreateVerificationState.Planned,
                Manifest = ExtensionCreateVerificationState.Planned,
                Payload = ExtensionCreateVerificationState.Planned,
                Cause = null,
            },
            Findings = [],
            Next = null,
        };
}
