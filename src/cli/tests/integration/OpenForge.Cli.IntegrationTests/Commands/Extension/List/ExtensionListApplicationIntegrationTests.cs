using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.List;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.IntegrationTests.Framework.Extensions;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.List;

public sealed class ExtensionListApplicationIntegrationTests
{
    [Theory(DisplayName = "Extension List honours cancellation at ingress for every source selection"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration")]
    [InlineData("default")]
    [InlineData("embedded-only")]
    [InlineData("explicit-source")]
    public async Task OperationCancellationIsAlwaysInterrupted(string scenario)
    {
        using var workspace = TemporaryWorkspace.Create("extension-list-cancel-workspace");
        using var source = TemporaryWorkspace.Create("extension-list-cancel-source");
        var request = new ExtensionListRequest
        {
            Workspace = new CliWorkspace(
                lexicalRoot: workspace.Path,
                physicalRoot: workspace.Path,
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            Selection = scenario == "embedded-only"
                ? ExtensionListSelection.Create(installedFlag: false, availableFlag: true)
                : ExtensionListSelection.Create(installedFlag: false, availableFlag: false),
            ExplicitSource = scenario == "explicit-source" ? source.Path : null,
        };
        var beforeWorkspace = workspace.SnapshotHashes();
        var beforeSource = source.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await ExtensionListOperationFactory.Create().ExecuteAsync(request, cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(ExtensionListFindingCode.Interrupted, Assert.Single(result.Findings).Code);
        Assert.Equal(beforeWorkspace, workspace.SnapshotHashes());
        Assert.Equal(beforeSource, source.SnapshotHashes());
    }

    [Theory(DisplayName = "Extension List pipeline presents terminal typed events on stderr with exact exits"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration")]
    [InlineData((int)CliSemanticStatus.Failed, (int)ExtensionListFindingCode.OperationFailed, 1, "failed")]
    [InlineData((int)CliSemanticStatus.Interrupted, (int)ExtensionListFindingCode.Interrupted, 130, "interrupted")]
    public async Task TerminalTypedEventsPreserveHumanPresentationPolicy(
        int statusValue,
        int findingCodeValue,
        int expectedExitCode,
        string expectedStatus)
    {
        using var workspace = TemporaryWorkspace.Create("extension-list-terminal-event");
        var before = workspace.SnapshotHashes();
        var request = new ExtensionListRequest
        {
            Workspace = new CliWorkspace(
                lexicalRoot: workspace.Path,
                physicalRoot: workspace.Path,
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            Selection = ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            ExplicitSource = null,
        };
        var result = ExtensionListResultBuilder.Event(
            request,
            (CliSemanticStatus)statusValue,
            (ExtensionListFindingCode)findingCodeValue,
            $"The {expectedStatus} terminal event was selected.");
        using var standardOutput = new StringWriter(CultureInfo.InvariantCulture);
        using var standardError = new StringWriter(CultureInfo.InvariantCulture);
        var pipeline = new CliCommandPipeline<ExtensionListRequest, ExtensionListResult>(
            // Unexpected failures have no deterministic safe System.IO trigger; this typed seam
            // executes the production renderer, output, and completion stages without a test hook.
            (_, _) => ValueTask.FromResult(result),
            new CliRendererSet<ExtensionListResult>(
                ExtensionListHumanRenderer.Render,
                ExtensionListJsonRenderer.Render));

        var completion = await pipeline.ExecuteAsync(
            request,
            new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        var error = standardError.ToString();

        Assert.Equal(expectedExitCode, completion.ExitCode);
        Assert.Equal(CliOutputTarget.StandardError, completion.PrimaryOutputTarget);
        Assert.Equal(string.Empty, standardOutput.ToString());
        Assert.Contains($"Status: {expectedStatus}", error, StringComparison.Ordinal);
        Assert.Contains(
            ExtensionListDefinitions.ReadFindingCode((ExtensionListFindingCode)findingCodeValue),
            error,
            StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Composed root Extension group and List help expose the complete read-only boundary"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration")]
    public async Task ComposedHelpExposesGroupAndLeaf()
    {
        using var workspace = TemporaryWorkspace.Create("extension-list-help");

        var root = await CliHostCapture.RunAsync([], workspace.Path);
        var group = await CliHostCapture.RunAsync(["extension"], workspace.Path);
        var leaf = await CliHostCapture.RunAsync(["extension", "list", "--help"], workspace.Path);

        Assert.Equal(0, root.ExitCode);
        Assert.Equal(0, group.ExitCode);
        Assert.Equal(0, leaf.ExitCode);
        Assert.Contains("open-forge extension --help", root.Output, StringComparison.Ordinal);
        Assert.Contains("list", group.Output, StringComparison.Ordinal);
        Assert.Contains("open-forge extension list", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("Results and streams", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("writes no payload", leaf.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Composed Extension List reports embedded and trusted installed facts with unchanged bytes"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration")]
    public async Task ComposedListReportsTrustedAndAvailableFactsWithoutWrites()
    {
        using var workspace = TemporaryWorkspace.Create("extension-list-complete");
        workspace.WriteText(
            ".agents/open-forge.lifecycle.json",
            ExtensionSourceAndLifecycleIntegrationTests.Lifecycle(
                workspace.Path,
                """
                [{
                  "id": "development-toolkit",
                  "version": "0.1.0",
                  "source": "embedded catalogue",
                  "dependencies": [],
                  "paths": [".agents/workflows/architecture.md"]
                }]
                """,
                """
                [{
                  "path": ".agents/workflows/architecture.md",
                  "owners": ["development-toolkit"],
                  "baselineFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                  "fingerprintKind": "semantic"
                }]
                """));
        var before = workspace.SnapshotHashes();

        var human = await CliHostCapture.RunAsync(["extension", "list", "--workspace", workspace.Path], workspace.Path);
        var json = await CliHostCapture.RunAsync(["extension", "list", "--workspace", workspace.Path, "--json"], workspace.Path);
        var verbose = await CliHostCapture.RunAsync(["extension", "list", "--workspace", workspace.Path, "--json", "--verbose"], workspace.Path);

        Assert.Equal(0, human.ExitCode);
        Assert.Equal(string.Empty, human.Error);
        Assert.Contains("Installed: coverage complete; record trusted", human.Output, StringComparison.Ordinal);
        Assert.Contains("Available: coverage complete", human.Output, StringComparison.Ordinal);
        Assert.Equal(0, json.ExitCode);
        Assert.Equal(string.Empty, json.Error);
        using var document = JsonDocument.Parse(json.Output);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        Assert.Single(document.RootElement.GetProperty("result").GetProperty("installed").EnumerateArray());
        Assert.Equal(ExtensionCatalogueSource.PackageIds,
            document.RootElement.GetProperty("result").GetProperty("available").EnumerateArray()
                .Select(package => package.GetProperty("id").GetString()));
        Assert.Equal(json.Output, verbose.Output);
        Assert.Contains("status=complete", verbose.Error, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Composed Extension List JSON preserves typed facts while ignoring opaque framework duplicates"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration")]
    public async Task ComposedJsonIgnoresOpaqueFrameworkDuplicateProperties()
    {
        using var workspace = TemporaryWorkspace.Create("extension-list-opaque-framework");
        var lifecycle = ExtensionSourceAndLifecycleIntegrationTests.Lifecycle(
            workspace.Path,
            """
            [{
              "id": "development-toolkit",
              "version": "0.1.0",
              "source": "embedded catalogue",
              "dependencies": [],
              "paths": []
            }]
            """,
            "[]").Replace(
                "\"framework\": null",
                "\"framework\": { \"settings\": { \"enabled\": true, \"enabled\": false }, \"settings\": null }",
                StringComparison.Ordinal);
        workspace.WriteText(".agents/open-forge.lifecycle.json", lifecycle);
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["extension", "list", "--workspace", workspace.Path, "--installed", "--json"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var commandResult = document.RootElement.GetProperty("result");
        Assert.Equal("trusted", commandResult.GetProperty("coverage").GetProperty("lifecycleTrust").GetString());
        Assert.Equal(
            "development-toolkit",
            Assert.Single(commandResult.GetProperty("installed").EnumerateArray()).GetProperty("id").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Composed Extension List preserves available-only coverage when lifecycle is missing"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration")]
    public async Task AvailableOnlyDoesNotInferOrRequireLifecycle()
    {
        using var workspace = TemporaryWorkspace.Create("extension-list-available");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["extension", "list", "--workspace", workspace.Path, "--available", "--json"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var commandResult = document.RootElement.GetProperty("result");
        Assert.Equal("not-requested", commandResult.GetProperty("coverage").GetProperty("installed").GetString());
        Assert.Equal(JsonValueKind.Null, commandResult.GetProperty("coverage").GetProperty("lifecycleTrust").ValueKind);
        Assert.Equal(ExtensionCatalogueSource.PackageIds,
            commandResult.GetProperty("available").EnumerateArray()
                .Select(package => package.GetProperty("id").GetString()));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Composed Extension List preserves safe untrusted installed rows with incomplete coverage"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration")]
    public async Task ComposedListPreservesSafeUntrustedInstalledFacts()
    {
        using var workspace = TemporaryWorkspace.Create("extension-list-untrusted");
        var lifecycle = ExtensionSourceAndLifecycleIntegrationTests.Lifecycle(
            workspace.Path,
            """
            [{
              "id": "development-toolkit",
              "version": "0.1.0",
              "source": "embedded catalogue",
              "dependencies": [],
              "paths": [".agents/workflows/architecture.md"]
            }]
            """,
            """
            [{
              "path": ".agents/workflows/architecture.md",
              "owners": ["development-toolkit"],
              "baselineFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
              "fingerprintKind": "semantic"
            }]
            """).Replace("\"coverage\": \"complete\"", "\"coverage\": \"incomplete\"", StringComparison.Ordinal);
        workspace.WriteText(".agents/open-forge.lifecycle.json", lifecycle);

        var result = await CliHostCapture.RunAsync(
            ["extension", "list", "--workspace", workspace.Path, "--installed", "--json"],
            workspace.Path);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var commandResult = document.RootElement.GetProperty("result");
        Assert.Equal("untrusted", commandResult.GetProperty("coverage").GetProperty("lifecycleTrust").GetString());
        Assert.Equal("development-toolkit", Assert.Single(commandResult.GetProperty("installed").EnumerateArray()).GetProperty("id").GetString());
    }
}
