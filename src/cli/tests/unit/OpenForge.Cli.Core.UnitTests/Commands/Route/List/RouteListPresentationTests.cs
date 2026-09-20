using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.TestSupport;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Route.List;
using OpenForge.Cli.Core.Presentation.Route.List.Shared.Help;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List;

public sealed class RouteListPresentationTests
{
    [Fact(DisplayName = "Route list binding uses the default and accepted finite or all depths"), Trait("Feature", "route-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BindingUsesDefaultAndAcceptedDepths()
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var invocation = Invocation(RouteListContractTestData.Workspace());
        string[] omittedArguments = ["list", "memory"];
        string[] allArguments = ["list", "memory", "--depth=all"];

        var omitted = RouteListBinding.Bind(
            symbols.RouteGroup.Parse(omittedArguments),
            invocation,
            symbols);
        var all = RouteListBinding.Bind(
            symbols.RouteGroup.Parse(allArguments),
            invocation,
            symbols);

        Assert.Equal(1, Assert.IsType<RouteListRequest>(omitted.Request).RequestedDepth.Value);
        Assert.Equal(RouteListDepthKind.All, Assert.IsType<RouteListRequest>(all.Request).RequestedDepth.Kind);
        Assert.Equal("memory", Assert.IsType<RouteListRequest>(all.Request).SourceReference);
    }

    [Theory(DisplayName = "Route list binding maps invalid depth to one typed invalid result without a request"), InlineData("-1"), InlineData("+1"), InlineData("1.5"), InlineData("ALL"), InlineData("2147483648"), InlineData(""), Trait("Feature", "route-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void BindingMapsInvalidDepthToTypedResult(string depth)
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var invocation = Invocation(RouteListContractTestData.Workspace());
        string[] arguments = ["list", "memory", $"--depth={depth}"];
        var bound = RouteListBinding.Bind(
            symbols.RouteGroup.Parse(arguments),
            invocation,
            symbols);

        Assert.Null(bound.Request);
        var result = Assert.IsType<RouteListResult>(bound.InvalidResult);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteListFindingCode.InvalidDepth, Assert.Single(result.Findings).Code);
        Assert.Equal("route-list.invalid-depth", Assert.Single(result.Findings).MachineCode);
        Assert.Same(invocation.Workspace, result.Workspace);
    }

    [Fact(DisplayName = "Route list detail levels retain one immutable row order"), Trait("Feature", "route-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void TextDetailLevelsRetainTypedRowOrder()
    {
        var result = CompleteResult();
        var compact = CliRenderingStage.Render(
            Presentation(result, CliDetail.Minimal),
            RouteListPresentation.Rendering).PrimaryContent;
        var standard = CliRenderingStage.Render(
            Presentation(result, CliDetail.Standard),
            RouteListPresentation.Rendering).PrimaryContent;
        var full = CliRenderingStage.Render(
            Presentation(result, CliDetail.Full),
            RouteListPresentation.Rendering).PrimaryContent;

        Assert.DoesNotContain("Status:", compact, StringComparison.Ordinal);
        Assert.DoesNotContain("Coverage:", compact, StringComparison.Ordinal);
        Assert.DoesNotContain("Confirmed:", compact, StringComparison.Ordinal);
        Assert.Contains("  memory", compact, StringComparison.Ordinal);
        Assert.DoesNotContain(".agents/memory/_memory.md", compact, StringComparison.Ordinal);
        Assert.True(compact.IndexOf("memory", StringComparison.Ordinal) < compact.IndexOf("memory/child", StringComparison.Ordinal));
        Assert.Contains("Workspace: ", standard, StringComparison.Ordinal);
        Assert.Contains(".agents/memory/_memory.md", standard, StringComparison.Ordinal);
        Assert.Contains("#Memory", standard, StringComparison.Ordinal);
        Assert.DoesNotContain("Selection:", standard, StringComparison.Ordinal);
        Assert.Contains("Parent: memory; .agents/memory/_memory.md", full, StringComparison.Ordinal);
        Assert.Contains("Kind: file; direct children: none", full, StringComparison.Ordinal);
        Assert.Contains("Selected as: descendant", full, StringComparison.Ordinal);
        Assert.Contains("Overwrite: none", full, StringComparison.Ordinal);
        Assert.True(full.IndexOf("memory", StringComparison.Ordinal) < full.IndexOf("memory/child", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Route list JSON projection keeps the shared envelope and level data"), Trait("Feature", "route-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void JsonProjectionKeepsSharedEnvelopeAndLevelData()
    {
        var complete = CompleteResult();
        var json = CliRenderingStage.Render(
            Presentation(complete, CliDetail.Minimal, CliFormat.Json),
            RouteListPresentation.Rendering).PrimaryContent;
        var expandedJson = CliRenderingStage.Render(
            Presentation(complete, CliDetail.Standard, CliFormat.Json),
            RouteListPresentation.Rendering).PrimaryContent;
        Assert.True(JsonDetailComparison.RetainsData(json, expandedJson));
        using var document = JsonDocument.Parse(expandedJson);
        var root = document.RootElement;

        Assert.Equal(
            ["schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route list", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal(
            ["subject", "depth", "rows"],
            root.GetProperty("data").EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").GetProperty("subject").ValueKind);
        Assert.Equal(1, root.GetProperty("data").GetProperty("depth").GetInt32());
        var firstRow = root.GetProperty("data").GetProperty("rows")[0];
        Assert.Equal(
            ["id", "path", "description", "tags", "relativeDepth"],
            firstRow.EnumerateObject().Select(property => property.Name));
        Assert.Equal("Memory", firstRow.GetProperty("tags")[0].GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);

        var fullJson = CliRenderingStage.Render(
            Presentation(complete, CliDetail.Full, CliFormat.Json),
            RouteListPresentation.Rendering).PrimaryContent;
        using var fullDocument = JsonDocument.Parse(fullJson);
        var fullRow = fullDocument.RootElement.GetProperty("data").GetProperty("rows")[1];
        Assert.Equal(
            ["id", "path", "description", "tags", "relativeDepth", "parentId", "absoluteDepth", "kind", "directChildren", "selectedAs", "hasOverwrite"],
            fullRow.EnumerateObject().Select(property => property.Name));
        Assert.Equal("file", fullRow.GetProperty("kind").GetString());
        Assert.Equal("descendant", fullRow.GetProperty("selectedAs").GetString());
        Assert.Equal(JsonValueKind.Null, fullRow.GetProperty("directChildren").ValueKind);

        var all = CompleteResult(RouteListDepth.All);
        using var allDocument = JsonDocument.Parse(CliRenderingStage.Render(
            Presentation(all, CliDetail.Standard, CliFormat.Json),
            RouteListPresentation.Rendering).PrimaryContent);
        Assert.Equal("all", allDocument.RootElement.GetProperty("data").GetProperty("depth").GetString());
    }

    [Fact(DisplayName = "Route list JSON projection preserves null workspace and report findings"), Trait("Feature", "route-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void JsonProjectionPreservesNullWorkspaceAndReportFinding()
    {
        var result = RouteListResult.Create(
            CliSemanticStatus.Invalid,
            null,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.NotStarted(null),
            [],
            [new RouteListFinding(
                RouteListFindingCode.InvalidWorkspace,
                CliSemanticStatus.Invalid,
                "workspace",
                "The workspace is unavailable.")],
            new CliNextAction("open-forge route list --help", "Select a workspace."));

        using var document = JsonDocument.Parse(CliRenderingStage.Render(
            Presentation(result, CliDetail.Standard, CliFormat.Json),
            RouteListPresentation.Rendering).PrimaryContent);
        var root = document.RootElement;
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        var finding = Assert.Single(root.GetProperty("findings").EnumerateArray());
        Assert.Equal(
            ["severity", "code", "title", "message", "subject", "category", "resolution", "actions"],
            finding.EnumerateObject().Select(property => property.Name));
        Assert.Equal("route-list.invalid-workspace", finding.GetProperty("code").GetString());
        Assert.Equal("workspace", finding.GetProperty("subject").GetProperty("id").GetString());
        Assert.Equal(
            "Cannot use workspace as the workspace: it does not exist or cannot be read.",
            finding.GetProperty("message").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "Route list workspace selection failure forms one typed null-workspace result"), Trait("Feature", "route-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void WorkspaceFailureFormsTypedNullWorkspaceResult()
    {
        var input = new CliGlobalInput(
            "missing-workspace",
            1,
            CliFormat.Json,
            1,
            CliDetail.Standard,
            0, null,
            0,
            false,
            0,
            false,
            0);
        var invalidInput = new CliInvalidInput(
            "cli.workspace.invalid",
            CliInvalidInputSource.Workspace,
            ["The selected workspace is missing."]);
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        string[] arguments = ["list"];
        var context = new CliInvalidBindingInput(
            invalidInput,
            input,
            new CliProcessEnvironment(RouteListContractTestData.Workspace().LexicalRoot),
            new CliBindingParse(symbols.RouteGroup.Parse(arguments), arguments));
        var result = RouteListBinding.CreateInvalidResultFactory()(context);

        Assert.Null(result.Workspace);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteListFindingCode.InvalidWorkspace, Assert.Single(result.Findings).Code);
    }

    [Fact(DisplayName = "Route list diagnostics remain bounded and contain no row or source-content dump"), Trait("Feature", "route-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void DiagnosticsAreBoundedAndDoNotDumpRows()
    {
        var result = CompleteResult();
        var diagnostics = Assert.IsType<string>(CliRenderingStage.Render(
            Presentation(result, CliDetail.Debug),
            RouteListPresentation.Rendering).DiagnosticContent);

        Assert.InRange(diagnostics.Length, 1, CliPresentationDefinitions.MaximumDiagnosticLength);
        Assert.Contains("workspace.lexical=", diagnostics, StringComparison.Ordinal);
        Assert.Contains("workspace.lexical=", diagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("workspace.physical=", diagnostics, StringComparison.Ordinal);
        Assert.Contains("selection.kind=loader-roots", diagnostics, StringComparison.Ordinal);
        Assert.Contains("rows=2", diagnostics, StringComparison.Ordinal);
        Assert.Contains("findings=0", diagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("Memory description", diagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("memory/child", diagnostics, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route list help sections document grammar depth globals exits and accepted examples"), Trait("Feature", "route-list"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void HelpSectionsDocumentAcceptedSurface()
    {
        var help = RouteListHelpSections.CreateList();
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));

        Assert.Contains("open-forge route list [source-reference] [--depth=<non-negative-integer|all>]", text, StringComparison.Ordinal);
        Assert.Contains("exact source ID", text, StringComparison.Ordinal);
        Assert.Contains("The default depth is 1", text, StringComparison.Ordinal);
        Assert.Contains("--depth=all", text, StringComparison.Ordinal);
        Assert.Contains("--workspace <path>", text, StringComparison.Ordinal);
        Assert.Contains("route inspect — inspect", text, StringComparison.Ordinal);
        Assert.Contains("open-forge route list memory", text, StringComparison.Ordinal);
        Assert.Contains("open-forge route list .agents/memory/_memory.md", text, StringComparison.Ordinal);
        Assert.Contains("open-forge route list --depth=2 --format json", text, StringComparison.Ordinal);
    }

    private static CliInvocation Invocation(CliWorkspace workspace)
    {
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(CliFormat.Text, CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }

    private static CliPresentationRequest<RouteListResult> Presentation(
        RouteListResult result,
        CliDetail view,
        CliFormat format = CliFormat.Text)
    {
        return new CliPresentationRequest<RouteListResult>(
            result,
            new CliPresentation(format, view, null));
    }

    private static RouteListResult CompleteResult(RouteListDepth? depth = null)
    {
        var requestedDepth = depth ?? RouteListDepth.Default;
        var workspace = RouteListContractTestData.Workspace();
        var root = RouteListRow.Entrypoint(
            "memory",
            ".agents/memory/_memory.md",
            null,
            null,
            0,
            0,
            "Memory description",
            ["Memory"],
            1,
            new RouteListProvenance(
                RouteListSelectionProvenance.LoaderRoot,
                RouteListSourceProvenance.AuthoredEntrypoint,
                false));
        var child = RouteListRow.RoutedLeaf(
            "memory/child",
            ".agents/memory/child.md",
            "memory",
            ".agents/memory/_memory.md",
            1,
            1,
            "Child description",
            ["Child"],
            new RouteListProvenance(
                RouteListSelectionProvenance.Descendant,
                RouteListSourceProvenance.AuthoredLeaf,
                false));
        return RouteListResult.Create(
            CliSemanticStatus.Complete,
            workspace,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.Complete(
                requestedDepth,
                requestedDepth,
                1,
                2,
                ["Loader roots and requested descendants were confirmed."]),
            [root, child],
            [],
            null);
    }
}
