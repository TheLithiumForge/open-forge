using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Serialization.Shared.Assertions;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Create;
using OpenForge.Cli.Core.Presentation.Extension.Create.Models;
using OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class ExtensionCreateGeneratedSerializationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Create native data uses generated metadata with exact coordinates"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public void NativeDataUsesGeneratedMetadataWithExactCoordinates()
    {
        var data = new ExtensionCreateData
        {
            Mode = "apply",
            Id = null,
            Folder = null,
            PackagePath = null,
            ManifestPath = null,
            ContentPath = null,
            Manifest = null,
            ManifestContent = null,
        };

        var json = JsonSerializer.Serialize(
            data,
            ExtensionCreateDataJsonContext.Default.ExtensionCreateData);

        using var parsed = JsonDocument.Parse(json);
        Assert.Equal(
            ["mode", "id", "folder", "packagePath", "manifestPath", "contentPath"],
            parsed.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Null, parsed.RootElement.GetProperty("id").ValueKind);
        Assert.Equal(JsonValueKind.Null, parsed.RootElement.GetProperty("folder").ValueKind);
        Assert.Equal(JsonValueKind.Null, parsed.RootElement.GetProperty("packagePath").ValueKind);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Create native JSON renderer preserves generated data order and detail parity"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public void NativeRendererPreservesGeneratedDataOrderAndDetailParity()
    {
        var result = CompleteResult();
        var presentation = new CliPresentationRequest<ExtensionCreateResult>(
            result,
            new CliPresentation(CliFormat.Json, CliDetail.Standard, null));

        var rendered = CommandOutputRenderers<ExtensionCreateResult>.Render(presentation, ExtensionCreatePresentation.Rendering);
        using var parsed = JsonDocument.Parse(rendered);
        Schema3Assertions.Envelope(parsed.RootElement, "extension create", "completed", "standard");
        var commandResult = parsed.RootElement.GetProperty("data");
        Assert.Equal(
            ["mode", "id", "folder", "packagePath", "manifestPath", "contentPath", "manifest"],
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
                    Path = "/catalogue/development-toolkit/content/.agents/",
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
