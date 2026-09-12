using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.List;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.List;

public sealed class ExtensionListBindingAndPresentationTests
{
    [Fact(DisplayName = "Extension List symbols compose one group leaf and exact local options"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit")]
    public void SymbolsComposeExactGrammar()
    {
        var group = ExtensionBinding.CreateGroup();
        var symbols = ExtensionListBinding.CreateSymbols(group);
        var parse = group.Parse("list --installed --installed --available --source catalogue");

        Assert.Empty(parse.Errors);
        Assert.Same(symbols.ListCommand, parse.CommandResult.Command);
        Assert.True(parse.GetValue(symbols.Installed));
        Assert.True(parse.GetValue(symbols.Available));
        Assert.Equal("catalogue", parse.GetValue(symbols.Source));
        Assert.Equal(3, symbols.ListCommand.Options.Count);
    }

    [Fact(DisplayName = "Extension List JSON preserves the schema envelope and complete command graph"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit")]
    public void JsonPreservesSchemaEnvelopeAndGraph()
    {
        var result = CreateResult();
        var json = ExtensionListJsonRenderer.Render(
            new CliPresentationRequest<ExtensionListResult>(
                result,
                new CliPresentation(CliOutputFormat.Json, CliView.Compact, CliVerbosity.Normal)));

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("extension list", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("result");
        Assert.Single(commandResult.GetProperty("installed").EnumerateArray());
        Assert.Single(commandResult.GetProperty("available").EnumerateArray());
        Assert.Equal("trusted", commandResult.GetProperty("coverage").GetProperty("lifecycleTrust").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "Extension List human views retain section identity status and safe rows"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit")]
    public void HumanViewsRetainSectionsAndStatus()
    {
        var result = CreateResult();
        var compact = ExtensionListHumanRenderer.Render(
            new CliPresentationRequest<ExtensionListResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));
        var expanded = ExtensionListHumanRenderer.Render(
            new CliPresentationRequest<ExtensionListResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal)));

        Assert.Equal(
            """
            Extension list: installed=1; available=1; source=available; status=complete
            Installed: toolkit; trusted
            Available: toolkit 1.0.0
            """,
            compact);
        Assert.Equal(
            """
            Open Forge extension list
            Workspace: /tmp/workspace
            Source: embedded catalogue; embedded-catalogue; available
            Installed (coverage: complete; trust: trusted)
            - toolkit 1.0.0; trusted; 2 managed paths; source available
            Available (coverage: complete)
            - toolkit 1.0.0; 1 package; 0 dependencies
            Status: complete
            """,
            expanded);
    }

    [Theory(DisplayName = "Extension List maps every cancelled discovery state to interrupted"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit")]
    [InlineData("source")]
    [InlineData("lifecycle")]
    public void CancelledDiscoveryStateIsInterrupted(string stage)
    {
        var request = new ExtensionListRequest
        {
            Workspace = new CliWorkspace(
                lexicalRoot: "/tmp/workspace",
                physicalRoot: "/tmp/workspace",
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            Selection = ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            ExplicitSource = null,
        };
        var source = new ExtensionSourceReadResult(
            state: stage == "source" ? ExtensionSourceReadState.Cancelled : ExtensionSourceReadState.Complete,
            kind: ExtensionSourceKind.EmbeddedCatalogue,
            identity: "embedded catalogue",
            packages: [],
            cause: stage == "source" ? "Source interrupted." : null);
        var lifecycle = new LifecycleReadResult(
            state: stage == "lifecycle" ? LifecycleReadState.Cancelled : LifecycleReadState.Complete,
            trust: stage == "lifecycle" ? LifecycleExtensionTrust.Incomplete : LifecycleExtensionTrust.Absent,
            packages: [],
            cause: stage == "lifecycle" ? "Lifecycle interrupted." : null);

        var result = ExtensionListResultBuilder.Build(request, source, lifecycle);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionListFindingCode.Interrupted);
    }

    [Theory(DisplayName = "Extension List human presentation executes every status stream and exit policy"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Complete, "complete", 0, (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Failed, "failed", 1, (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Attention, "requires attention", 2, (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Incomplete, "incomplete", 3, (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Invalid, "invalid", 4, (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Blocked, "blocked", 5, (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Interrupted, "interrupted", 130, (int)CliOutputTarget.StandardError)]
    public async Task HumanPresentationExecutesCompleteStatusMatrix(
        int statusValue,
        string humanStatus,
        int expectedExitCode,
        int targetValue)
    {
        var status = (CliSemanticStatus)statusValue;
        var target = (CliOutputTarget)targetValue;
        var result = status == CliSemanticStatus.Complete
            ? CreateResult()
            : ExtensionListResultBuilder.Event(
                CreateRequest(),
                status,
                ReadFindingCode(status),
                $"The {humanStatus} presentation path was selected.");
        using var standardOutput = new StringWriter(CultureInfo.InvariantCulture);
        using var standardError = new StringWriter(CultureInfo.InvariantCulture);
        var pipeline = new CliCommandPipeline<ExtensionListRequest, ExtensionListResult>(
            (_, _) => ValueTask.FromResult(result),
            new CliRendererSet<ExtensionListResult>(
                ExtensionListHumanRenderer.Render,
                ExtensionListJsonRenderer.Render));
        var presentation = new CliPresentation(
            CliOutputFormat.Human,
            CliView.Expanded,
            CliVerbosity.Normal);

        var completion = await pipeline.ExecuteAsync(
            CreateRequest(),
            presentation,
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        var rendered = $"{ExtensionListHumanRenderer.Render(new CliPresentationRequest<ExtensionListResult>(result, presentation))}{Environment.NewLine}";
        Assert.Equal(status, completion.Status);
        Assert.Equal(expectedExitCode, completion.ExitCode);
        Assert.Equal(target, completion.PrimaryOutputTarget);
        Assert.Equal(target == CliOutputTarget.StandardOutput ? rendered : string.Empty, standardOutput.ToString());
        Assert.Equal(target == CliOutputTarget.StandardError ? rendered : string.Empty, standardError.ToString());
        Assert.Contains($"Status: {humanStatus}", rendered, StringComparison.Ordinal);
    }

    private static ExtensionListFindingCode ReadFindingCode(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Failed => ExtensionListFindingCode.OperationFailed,
            CliSemanticStatus.Attention => ExtensionListFindingCode.SourceUnavailable,
            CliSemanticStatus.Incomplete => ExtensionListFindingCode.LifecycleUnavailable,
            CliSemanticStatus.Invalid => ExtensionListFindingCode.InvalidInput,
            CliSemanticStatus.Blocked => ExtensionListFindingCode.SourceBlocked,
            CliSemanticStatus.Interrupted => ExtensionListFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The event finding status is not defined."),
        };

    private static ExtensionListRequest CreateRequest()
        => new()
        {
            Workspace = new CliWorkspace(
                lexicalRoot: "/tmp/workspace",
                physicalRoot: "/tmp/workspace",
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            Selection = ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            ExplicitSource = null,
        };

    private static ExtensionListResult CreateResult()
        => new(
            status: CliSemanticStatus.Complete,
            workspace: new CliWorkspace(
                lexicalRoot: "/tmp/workspace",
                physicalRoot: "/tmp/workspace",
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            selection: ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            source: new ExtensionListSource
            {
                Identity = "embedded catalogue",
                Kind = ExtensionSourceKind.EmbeddedCatalogue,
                State = ExtensionSourceReadState.Complete,
            },
            lifecycleTrust: LifecycleExtensionTrust.Trusted,
            installedCoverage: ExtensionListCoverage.Complete,
            availableCoverage: ExtensionListCoverage.Complete,
            installed:
            [
                new ExtensionListInstalledRow
                {
                    Id = "toolkit",
                    Version = "1.0.0",
                    Trust = LifecycleExtensionTrust.Trusted,
                    ManagedPathCount = 2,
                    SourceAvailable = true,
                },
            ],
            available:
            [
                new ExtensionListAvailableRow
                {
                    Id = "toolkit",
                    Name = "Toolkit",
                    Description = "A toolkit.",
                    Version = "1.0.0",
                    PackageCount = 1,
                    DependencyCount = 0,
                },
            ],
            findings: [],
            next: null);
}
