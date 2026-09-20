using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.IntegrationTests.Commands.Install;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Serialization.Shared.Assertions;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class InstallGeneratedSerializationTests
{
    // 12 directories (including .agents), 12 payload files, 2 host regions, and the ownership record.
    private const int FreshInstallEffectCount = 27;

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install JSON uses generated native data metadata with the complete envelope"), Trait("Feature", "install-presentation"), Trait("Evidence", "Integration")]
    public async Task NativeDataUsesGeneratedMetadataWithCompleteEnvelope()
    {
        using var workspace = InstallOperationWorkspace.Create("install-generated-serialization");
        var result = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Confirmation(),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(automatic: true),
                TestContext.Current.CancellationToken);

        var rendered = Render(result, CliDetail.Full);
        using var parsed = JsonDocument.Parse(rendered);
        var root = parsed.RootElement;
        Schema3Assertions.Envelope(root, "install", "completed", "full");
        Assert.Equal(workspace.PhysicalPath, root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal("explicit-workspace", root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Empty(root.GetProperty("findings").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);

        var data = root.GetProperty("data");
        Assert.Equal(
            ["mode", "force", "automatic", "classification", "footprint", "lockPath", "effects", "source", "lifecycle", "verification"],
            data.EnumerateObject().Select(property => property.Name));
        Assert.Equal("apply", data.GetProperty("mode").GetString());
        Assert.False(data.GetProperty("force").GetBoolean());
        Assert.True(data.GetProperty("automatic").GetBoolean());
        Assert.Equal("safe-absence", data.GetProperty("classification").GetString());
        Assert.Equal(InstallDefinitions.OwnershipRecordPath, data.GetProperty("lockPath").GetString());
        Assert.Equal(InstallOperationWorkspace.EmbeddedPayloadPaths.Count, data.GetProperty("footprint").GetProperty("files").GetInt32());
        Assert.Equal(11, data.GetProperty("footprint").GetProperty("directories").GetInt32());
        Assert.Equal(2, data.GetProperty("footprint").GetProperty("sections").GetInt32());
        Assert.Equal(FreshInstallEffectCount, data.GetProperty("effects").GetArrayLength());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("source").GetProperty("inventoryFingerprint").GetString()));
        Assert.Equal("publish", data.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("verified", data.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal("verified", data.GetProperty("verification").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install generated native data preserves a pure dry-run projection and complete effects"), Trait("Feature", "install-presentation"), Trait("Evidence", "Integration")]
    public async Task NativeDataPreservesDryRunProjectionAndCompleteEffects()
    {
        using var workspace = InstallOperationWorkspace.Create("install-generated-dry-run");
        var result = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Unavailable(),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(
                    mode: InstallMode.DryRun,
                    automatic: true),
                TestContext.Current.CancellationToken);

        var rendered = Render(result, CliDetail.Standard);
        using var parsed = JsonDocument.Parse(rendered);
        var root = parsed.RootElement;
        Schema3Assertions.Envelope(root, "install", "completed", "standard");
        var data = root.GetProperty("data");
        Assert.Equal("dry-run", data.GetProperty("mode").GetString());
        Assert.True(data.GetProperty("automatic").GetBoolean());
        Assert.False(data.TryGetProperty("effects", out _));
        Assert.Equal(FreshInstallEffectCount, root.GetProperty("effects").GetArrayLength());
        Assert.Equal("planned", root.GetProperty("effects")[0].GetProperty("outcome").GetString());
        Assert.False(data.TryGetProperty("source", out _));
        Assert.False(data.TryGetProperty("lifecycle", out _));
        Assert.False(data.TryGetProperty("verification", out _));
        Assert.Equal("not-required", root.GetProperty("recovery").GetProperty("disposition").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    private static string Render(InstallResult result, CliDetail detail)
        => CommandOutputRenderers<InstallResult>.Render(
            new CliPresentationRequest<InstallResult>(
                result,
                new CliPresentation(CliFormat.Json, detail, null)),
            InstallPresentation.Rendering);
}
