using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Presentation;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.IntegrationTests.Commands.Install;

namespace OpenForge.Cli.IntegrationTests.Serialization;

public sealed class InstallGeneratedSerializationTests
{
    [Fact(DisplayName = "Install JSON document uses generated metadata with exact required null coordinates and order"), Trait("Feature", "install-presentation"), Trait("Evidence", "Integration")]
    public void DocumentUsesGeneratedMetadataWithExactCoordinates()
    {
        var document = new InstallJsonDocument
        {
            SchemaVersion = 1,
            Command = "install",
            Status = "invalid",
            Workspace = null,
            Result = new InstallJsonResult
            {
                Mode = "apply",
                Force = false,
                Automatic = false,
                Source = null,
                Classification = null,
                Footprint = null,
                Effects = [],
                Lifecycle = new InstallJsonLifecycle
                {
                    Action = "none",
                    Outcome = "not-requested",
                },
                Recovery = new InstallJsonRecovery
                {
                    State = "not-required",
                    ResidualPath = null,
                },
                Verification = "not-requested",
                Findings =
                [
                    new InstallJsonFinding
                    {
                        Code = "install.invalid-input",
                        Target = null,
                        Cause = "The representative input is invalid.",
                    },
                ],
            },
            Next = null,
        };

        var json = JsonSerializer.Serialize(
            document,
            InstallJsonContext.Default.InstallJsonDocument);

        using var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        var result = root.GetProperty("result");
        Assert.Equal(
            ["mode", "force", "automatic", "source", "classification", "footprint", "effects", "lifecycle", "recovery", "verification", "findings"],
            result.EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Null, result.GetProperty("source").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("classification").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("footprint").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("recovery").GetProperty("residualPath").ValueKind);
        Assert.Equal(JsonValueKind.Null, result.GetProperty("findings")[0].GetProperty("target").ValueKind);
    }

    [Fact(DisplayName = "Install JSON renderer preserves generated projection order and complete operation parity"), Trait("Feature", "install-presentation"), Trait("Evidence", "Integration")]
    public async Task RendererPreservesGeneratedProjectionOrderAndParity()
    {
        using var workspace = InstallOperationWorkspace.Create("install-generated-serialization");
        using var standardInput = new StringReader(string.Empty);
        using var promptOutput = new StringWriter();
        var result = await InstallOperationFactory.Create(
                new CliInteractiveSession(
                    standardInput,
                    promptOutput,
                    canPrompt: false),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(
                    mode: InstallMode.DryRun,
                    automatic: true),
                TestContext.Current.CancellationToken);
        var presentation = new CliPresentationRequest<InstallResult>(
            result,
            new CliPresentation(
                CliOutputFormat.Json,
                CliView.Expanded,
                CliVerbosity.Normal));

        var rendered = InstallJsonRenderer.Render(presentation);
        var expected = JsonSerializer.Serialize(
            InstallJsonProjection.Create(result),
            InstallJsonContext.Default.InstallJsonDocument);

        Assert.Equal(expected, rendered);
        Assert.Equal(string.Empty, promptOutput.ToString());
        using var parsed = JsonDocument.Parse(rendered);
        var root = parsed.RootElement;
        Assert.Equal("install", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("result");
        Assert.Equal(
            ["mode", "force", "automatic", "source", "classification", "footprint", "effects", "lifecycle", "recovery", "verification", "findings"],
            commandResult.EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", commandResult.GetProperty("mode").GetString());
        Assert.Equal("safe-absence", commandResult.GetProperty("classification").GetString());
        Assert.NotEqual(JsonValueKind.Null, commandResult.GetProperty("source").ValueKind);
        Assert.NotEqual(JsonValueKind.Null, commandResult.GetProperty("footprint").ValueKind);
        Assert.NotEmpty(commandResult.GetProperty("effects").EnumerateArray());
        Assert.Empty(commandResult.GetProperty("findings").EnumerateArray());
    }
}
