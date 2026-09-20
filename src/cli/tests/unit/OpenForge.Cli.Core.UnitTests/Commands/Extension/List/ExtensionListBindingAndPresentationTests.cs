using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;
using OpenForge.Cli.Core.Commands.Extension.List;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Extension.List;
using OpenForge.Cli.Core.Presentation.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.List;

public sealed class ExtensionListBindingAndPresentationTests
{
    private static string WorkspaceRoot { get; } = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "open-forge-extension-list-workspace"));

    [Fact(DisplayName = "Extension List symbols compose one group leaf and exact local options"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
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

    [Fact(DisplayName = "Extension List JSON preserves the schema envelope and complete command graph"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void JsonPreservesSchemaEnvelopeAndGraph()
    {
        var result = CreateResult();
        var selected = CliReportSelection.Select(
            result,
            new CliSelection(CliDetail.Standard),
            ExtensionListPresentation.Rendering);
        var json = CliJsonRenderer.Render(selected, ExtensionListPresentation.Rendering.DataJsonTypeInfo);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("extension list", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("data");
        Assert.Single(commandResult.GetProperty("installed").EnumerateArray());
        Assert.Single(commandResult.GetProperty("available").EnumerateArray());
        Assert.Equal("embedded-catalogue", commandResult.GetProperty("source").GetProperty("kind").GetString());
        Assert.False(Assert.Single(commandResult.GetProperty("available").EnumerateArray()).TryGetProperty("installed", out _));
        Assert.Equal("open-forge extension install <id>", root.GetProperty("next").GetProperty("command").GetString());
    }

    [Fact(DisplayName = "Extension List JSON retains nullable full installed fields at their selected detail"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void JsonDataPresenceFollowsDetail()
    {
        var result = CreateResult();

        using var minimalDocument = JsonDocument.Parse(RenderJson(result, CliDetail.Minimal));
        var minimalRow = Assert.Single(minimalDocument.RootElement.GetProperty("data").GetProperty("installed").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, minimalRow.GetProperty("note").ValueKind);
        Assert.False(minimalRow.TryGetProperty("files", out _));
        Assert.False(minimalRow.TryGetProperty("recordedSource", out _));
        Assert.False(minimalRow.TryGetProperty("coverage", out _));

        using var fullDocument = JsonDocument.Parse(RenderJson(result, CliDetail.Full));
        var fullRow = Assert.Single(fullDocument.RootElement.GetProperty("data").GetProperty("installed").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, fullRow.GetProperty("note").ValueKind);
        Assert.Equal(JsonValueKind.Null, fullRow.GetProperty("recordedSource").ValueKind);
        Assert.Equal(2, fullRow.GetProperty("files").GetInt32());
        Assert.Equal("complete", fullRow.GetProperty("coverage").GetString());
    }

    [Fact(DisplayName = "Extension List human views retain section identity status and safe rows"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void HumanViewsRetainSectionsAndStatus()
    {
        var result = CreateResult();
        var compact = RenderText(result, CliDetail.Minimal);
        var expanded = RenderText(result, CliDetail.Standard);

        Assert.Contains("Installed", compact, StringComparison.Ordinal);
        Assert.Contains("toolkit", compact, StringComparison.Ordinal);
        Assert.Contains("A toolkit.", compact, StringComparison.Ordinal);
        Assert.Contains("Source: embedded catalogue", expanded, StringComparison.Ordinal);
        Assert.DoesNotContain("embedded-catalogue", expanded, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge extension install <id>", expanded, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Extension List maps cancelled source discovery to interrupted"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void CancelledDiscoveryStateIsInterrupted()
    {
        var request = new ExtensionListRequest
        {
            Workspace = new CliWorkspace(
                lexicalRoot: WorkspaceRoot,
                physicalRoot: WorkspaceRoot,
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            Selection = ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            ExplicitSource = null,
        };
        var source = new ExtensionSourceReadResult(
            state: ExtensionSourceReadState.Cancelled,
            kind: ExtensionSourceKind.EmbeddedCatalogue,
            identity: "embedded catalogue",
            packages: [],
            cause: "Source interrupted.");
        var lifecycle = WorkspaceOwnershipRead.Absent(Path.Combine(WorkspaceRoot, ".agents", "open-forge.lock.json"));

        var result = ExtensionListResultBuilder.Build(request, source, lifecycle);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionListFindingCode.Interrupted);
    }

    [Theory(DisplayName = "Extension List distinguishes a known empty installation from unavailable inventory"), Trait("Feature", "extension-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    [InlineData(ExtensionListCoverage.Complete, "none")]
    [InlineData(ExtensionListCoverage.Incomplete, "unavailable")]
    public void EmptyCoverageIsHonest(object coverageValue, string expected)
    {
        var coverage = (ExtensionListCoverage)coverageValue;
        var result = new ExtensionListResult(
            status: coverage == ExtensionListCoverage.Complete ? CliSemanticStatus.Complete : CliSemanticStatus.Incomplete,
            workspace: null,
            selection: ExtensionListSelection.Create(installedFlag: true, availableFlag: false),
            source: null,
            lifecycleTrust: coverage == ExtensionListCoverage.Complete ? ExtensionListOwnershipTrust.Trusted : ExtensionListOwnershipTrust.Incomplete,
            installedCoverage: coverage,
            availableCoverage: ExtensionListCoverage.NotRequested,
            installed: [], available: [], findings: [], next: null);
        foreach (var view in new[] { CliDetail.Minimal, CliDetail.Standard })
        {
            var text = RenderText(result, view);
            Assert.Contains(expected, text, StringComparison.Ordinal);
            Assert.DoesNotContain("Available", text, StringComparison.Ordinal);
            if (coverage == ExtensionListCoverage.Incomplete)
            {
                Assert.DoesNotContain("none", text, StringComparison.Ordinal);
            }
        }
    }

    private static ExtensionListResult CreateResult()
        => new(
            status: CliSemanticStatus.Complete,
            workspace: new CliWorkspace(
                lexicalRoot: WorkspaceRoot,
                physicalRoot: WorkspaceRoot,
                selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            selection: ExtensionListSelection.Create(installedFlag: true, availableFlag: true),
            source: new ExtensionListSource
            {
                Identity = "embedded catalogue",
                Kind = ExtensionListSourceKind.EmbeddedCatalogue,
                State = ExtensionListSourceState.Complete,
            },
            lifecycleTrust: ExtensionListOwnershipTrust.Trusted,
            installedCoverage: ExtensionListCoverage.Complete,
            availableCoverage: ExtensionListCoverage.Complete,
            installed:
            [
                new ExtensionListInstalledRow
                {
                    Id = "toolkit",
                    Version = "1.0.0",
                    Trust = ExtensionListOwnershipTrust.Trusted,
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
                    InstalledVersion = "1.0.0",
                },
            ],
            findings: [],
            next: null);

    private static string RenderText(ExtensionListResult result, CliDetail detail)
    {
        var selected = CliReportSelection.Select(result, new CliSelection(detail), ExtensionListPresentation.Rendering);
        return CliTextRenderer.Render(selected, CliTextStyle.Plain, ExtensionListPresentation.Rendering.DataTextRenderer).Content.TrimEnd();
    }

    private static string RenderJson(ExtensionListResult result, CliDetail detail)
    {
        var selected = CliReportSelection.Select(result, new CliSelection(detail), ExtensionListPresentation.Rendering);
        return CliJsonRenderer.Render(selected, ExtensionListPresentation.Rendering.DataJsonTypeInfo);
    }
}
