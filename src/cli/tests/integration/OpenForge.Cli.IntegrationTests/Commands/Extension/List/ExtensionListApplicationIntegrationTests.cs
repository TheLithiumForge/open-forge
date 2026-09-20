using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.List;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Extension.List;
using OpenForge.Cli.Core.Presentation.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.List;

public sealed class ExtensionListApplicationIntegrationTests
{
    [Trait("Boundary", "OS")]
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

    [Trait("Boundary", "Output")]
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
        var pipeline = new CliReportPipeline<ExtensionListRequest, ExtensionListResult, ExtensionListData>(
            // Unexpected failures have no deterministic safe System.IO trigger; this typed seam
            // executes the production renderer, output, and completion stages without a test hook.
            (_, _) => ValueTask.FromResult(result),
            ExtensionListPresentation.Rendering);

        var completion = await pipeline.ExecuteAsync(
            request,
            new CliPresentation(CliFormat.Text, CliDetail.Full, null),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        var error = standardError.ToString();

        Assert.Equal(expectedExitCode, completion.ExitCode);
        Assert.Equal(CliOutputTarget.StandardError, completion.PrimaryOutputTarget);
        Assert.Equal(string.Empty, standardOutput.ToString());
        Assert.Contains(
            expectedStatus == "failed"
                ? "Extension list stopped because of an unexpected error: The failed terminal event was selected."
                : "Extension list was cancelled.",
            error,
            StringComparison.Ordinal);
        Assert.Contains(
            ExtensionListDefinitions.ReadFindingCode((ExtensionListFindingCode)findingCodeValue),
            error,
            StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
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

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Extension List reports embedded and trusted installed facts with unchanged bytes"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration")]
    public async Task ComposedListReportsTrustedAndAvailableFactsWithoutWrites()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-list-complete");
        await workspace.SeedFrameworkAsync();
        var installed = await workspace.RunAsync(["extension", "install", "development-toolkit", "--automatic"]);
        Assert.Equal(0, installed.ExitCode);
        var before = workspace.Snapshot();

        var human = await workspace.RunAsync(["extension", "list"]);
        var json = await workspace.RunAsync(["extension", "list", "--format", "json"]);
        var verbose = await workspace.RunAsync(["extension", "list", "--format", "json", "--detail", "debug"]);

        Assert.Equal(0, human.ExitCode);
        Assert.Equal(string.Empty, human.StandardError);
        Assert.Contains("Installed", human.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("development-toolkit", human.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(0, json.ExitCode);
        Assert.Equal(string.Empty, json.StandardError);
        using var document = JsonDocument.Parse(json.StandardOutput);
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("data").GetProperty("installed").EnumerateArray(),
            package => package.GetProperty("id").GetString() == "development-toolkit");
        Assert.Equal(ExtensionCatalogueSource.PackageIds,
            document.RootElement.GetProperty("data").GetProperty("available").EnumerateArray()
                .Select(package => package.GetProperty("id").GetString()));
        using var verboseDocument = JsonDocument.Parse(verbose.StandardOutput);
        var minimalData = document.RootElement.GetProperty("data");
        var verboseData = verboseDocument.RootElement.GetProperty("data");
        Assert.Equal(
            minimalData.GetProperty("installed").EnumerateArray().Select(package => package.GetProperty("id").GetString()),
            verboseData.GetProperty("installed").EnumerateArray().Select(package => package.GetProperty("id").GetString()));
        Assert.Equal(
            minimalData.GetProperty("available").EnumerateArray().Select(package => package.GetProperty("id").GetString()),
            verboseData.GetProperty("available").EnumerateArray().Select(package => package.GetProperty("id").GetString()));
        Assert.All(
            verboseData.GetProperty("installed").EnumerateArray(),
            package =>
            {
                Assert.True(package.TryGetProperty("files", out _));
                Assert.True(package.TryGetProperty("recordedSource", out _));
                Assert.True(package.TryGetProperty("coverage", out _));
            });
        Assert.Contains("status=completed", verbose.StandardError, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Extension List reads lock ownership and ignores malformed leftover state"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration")]
    public async Task ComposedJsonIgnoresMalformedLeftoverState()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-list-opaque-framework");
        await workspace.SeedFrameworkAsync();
        var installed = await workspace.RunAsync(["extension", "install", "development-toolkit", "--automatic"]);
        Assert.Equal(0, installed.ExitCode);
        workspace.CreateOccupant(ExtensionInstallIntegrationWorkspace.LifecyclePath, "{ obsolete and malformed }");
        var before = workspace.Snapshot();

        var result = await workspace.RunAsync(["extension", "list", "--installed", "--format", "json", "--detail", "full"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        var commandResult = document.RootElement.GetProperty("data");
        Assert.Contains(
            commandResult.GetProperty("installed").EnumerateArray(),
            package => package.GetProperty("id").GetString() == "development-toolkit");
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Extension List preserves available-only coverage when lifecycle is missing"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration")]
    public async Task AvailableOnlyDoesNotInferOrRequireLifecycle()
    {
        using var workspace = TemporaryWorkspace.Create("extension-list-available");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["extension", "list", "--workspace", workspace.Path, "--available", "--format", "json"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var commandResult = document.RootElement.GetProperty("data");
        Assert.Empty(commandResult.GetProperty("installed").EnumerateArray());
        Assert.Equal(ExtensionCatalogueSource.PackageIds,
            commandResult.GetProperty("available").EnumerateArray()
                .Select(package => package.GetProperty("id").GetString()));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Composed Extension List reports unavailable ownership without claiming an empty installation"), Trait("Feature", "extension-list"), Trait("Evidence", "Integration")]
    [InlineData("missing")]
    [InlineData("invalid")]
    [InlineData("nonordinary")]
    [InlineData("duplicate")]
    public async Task UnavailableOwnershipIsInformational(string state)
    {
        using var workspace = TemporaryWorkspace.Create("extension-list-unknown-ownership");
        workspace.WriteText(".agents/workflows/architecture.md", "Matching files do not establish ownership.");
        if (state == "invalid") workspace.WriteText(".agents/open-forge.lock.json", "{");
        if (state == "nonordinary") workspace.CreateDirectory(".agents/open-forge.lock.json");
        if (state == "duplicate") workspace.WriteText(".agents/open-forge.lock.json", """
            {"extensions":[{"id":"toolkit"},{"id":"toolkit"}]}
            """);
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(
            ["extension", "list", "--workspace", workspace.Path, "--installed", "--format", "json", "--detail", "full"], workspace.Path);
        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var facts = document.RootElement.GetProperty("data");
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        Assert.Empty(facts.GetProperty("installed").EnumerateArray());
        var finding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("extension-list.ownership-observation", finding.GetProperty("code").GetString());
        Assert.Equal("info", finding.GetProperty("severity").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
