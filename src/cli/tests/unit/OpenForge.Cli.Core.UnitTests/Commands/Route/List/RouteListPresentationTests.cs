using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List;

public sealed class RouteListPresentationTests
{
    [Fact(DisplayName = "Route list binding uses the default and accepted finite or all depths"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
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

    [Theory(DisplayName = "Route list binding maps invalid depth to one typed invalid result without a request"), InlineData("-1"), InlineData("+1"), InlineData("1.5"), InlineData("ALL"), InlineData("2147483648"), InlineData(""), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
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

    [Fact(DisplayName = "Route list invalid depth binding bypasses the operation pipeline"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public async Task InvalidDepthBypassesOperation()
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var operationCalls = 0;
        var fallback = CompleteResult();
        var binding = RouteListBinding.Close(
            symbols,
            new RouteListBindingComponents
            {
                Help = CliHelpContent.Empty,
                Operation = (request, cancellationToken) =>
                {
                    operationCalls++;
                    return ValueTask.FromResult(fallback);
                },
                Renderers = new CliRendererSet<RouteListResult>(
                    RouteListHumanRenderer.Render,
                    RouteListJsonRenderer.Render),
            });
        var output = new StringWriter();
        var error = new StringWriter();
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(symbols.RouteGroup, CliHelpContent.Empty, symbols.DelimiterPolicies)],
            [binding]);
        var parse = tree.Parse(["route", "list", "--depth=-1"]);
        var completion = await binding.InvokeAsync(
            new CliBindingParse(parse.Result, parse.OriginalArguments),
            Invocation(RouteListContractTestData.Workspace()),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        Assert.Equal(0, operationCalls);
        Assert.Equal(4, completion.ExitCode);
        Assert.Empty(output.ToString());
        Assert.Contains("route-list.invalid-depth", error.ToString(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route list compact and expanded renderers retain one immutable row order"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void HumanViewsRetainTypedRowOrder()
    {
        var result = CompleteResult();
        var compact = RouteListHumanRenderer.Render(Presentation(result, CliView.Compact));
        var expanded = RouteListHumanRenderer.Render(Presentation(result, CliView.Expanded));

        Assert.StartsWith("result=complete  coverage=complete", compact, StringComparison.Ordinal);
        Assert.Contains("coverage-evidence=", compact, StringComparison.Ordinal);
        Assert.Contains("memory  .agents/memory/_memory.md", compact, StringComparison.Ordinal);
        Assert.True(compact.IndexOf("memory  ", StringComparison.Ordinal) < compact.IndexOf("memory/child", StringComparison.Ordinal));
        Assert.StartsWith(
            $"Open Forge route list{Environment.NewLine}Result: complete{Environment.NewLine}Coverage: complete",
            expanded,
            StringComparison.Ordinal);
        Assert.Contains("Coverage evidence:", expanded, StringComparison.Ordinal);
        Assert.Contains("Parent ID: memory", expanded, StringComparison.Ordinal);
        Assert.Contains("Provenance: descendant; authored topology leaf", expanded, StringComparison.Ordinal);
        Assert.True(expanded.IndexOf("ID: memory", StringComparison.Ordinal) < expanded.IndexOf("ID: memory/child", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Route list JSON projection keeps its envelope, scalar depth forms, and nullable facts"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void JsonProjectionKeepsEnvelopeScalarsAndNulls()
    {
        var complete = CompleteResult();
        var json = RouteListJsonRenderer.Render(Presentation(complete, CliView.Compact, CliOutputFormat.Json));
        var expandedJson = RouteListJsonRenderer.Render(Presentation(complete, CliView.Expanded, CliOutputFormat.Json));
        Assert.Equal(json, expandedJson);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route list", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(
            ["selection", "requestedDepth", "effectiveDepth", "coverage", "findings", "rows"],
            root.GetProperty("result").EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["kind", "attemptedId", "attemptedPath", "resolvedId", "resolvedPath"],
            root.GetProperty("result").GetProperty("selection").EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["state", "selectedRootCount", "confirmedRowCount", "confirmations", "unresolvedBoundaries"],
            root.GetProperty("result").GetProperty("coverage").EnumerateObject().Select(property => property.Name));
        Assert.Equal(1, root.GetProperty("result").GetProperty("requestedDepth").GetInt32());
        Assert.Equal(1, root.GetProperty("result").GetProperty("effectiveDepth").GetInt32());
        var firstRow = root.GetProperty("result").GetProperty("rows")[0];
        Assert.Equal(
            ["id", "path", "parentId", "parentPath", "absoluteDepth", "relativeDepth", "kind", "description", "tags", "directChildCount", "provenance"],
            firstRow.EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Number, root.GetProperty("result").GetProperty("rows")[1].GetProperty("absoluteDepth").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("result").GetProperty("rows")[1].GetProperty("directChildCount").ValueKind);
        Assert.Equal("Memory", firstRow.GetProperty("tags")[0].GetString());
        Assert.True(root.GetProperty("result").GetProperty("coverage").TryGetProperty("confirmations", out _));
        Assert.Equal("loader-root", firstRow.GetProperty("provenance").GetProperty("selection").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);

        var all = CompleteResult(RouteListDepth.All);
        using var allDocument = JsonDocument.Parse(RouteListJsonRenderer.Render(Presentation(all, CliView.Expanded, CliOutputFormat.Json)));
        Assert.Equal("all", allDocument.RootElement.GetProperty("result").GetProperty("requestedDepth").GetString());
    }

    [Fact(DisplayName = "Route list JSON projection preserves null workspace and MachineCode findings"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesNullWorkspaceAndFindingCode()
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

        using var document = JsonDocument.Parse(RouteListJsonRenderer.Render(Presentation(result, CliView.Expanded, CliOutputFormat.Json)));
        var root = document.RootElement;
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        var finding = Assert.Single(root.GetProperty("result").GetProperty("findings").EnumerateArray());
        Assert.Equal(
            ["code", "status", "subject", "cause", "candidatePaths"],
            finding.EnumerateObject().Select(property => property.Name));
        Assert.Equal("route-list.invalid-workspace", finding.GetProperty("code").GetString());
        Assert.Equal("workspace", finding.GetProperty("subject").GetString());
        Assert.Equal("The workspace is unavailable.", finding.GetProperty("cause").GetString());
        Assert.Equal("Select a workspace.", root.GetProperty("next").GetProperty("reason").GetString());
    }

    [Fact(DisplayName = "Route list workspace selection failure forms one typed null-workspace result"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void WorkspaceFailureFormsTypedNullWorkspaceResult()
    {
        var input = new CliGlobalInput(
            "missing-workspace",
            1,
            CliOutputFormat.Json,
            1,
            CliView.Expanded,
            0,
            CliVerbosity.Normal,
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

    [Fact(DisplayName = "Route list diagnostics remain bounded and contain no row or source-content dump"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DiagnosticsAreBoundedAndDoNotDumpRows()
    {
        var result = CompleteResult();
        var diagnostics = Assert.IsType<string>(RouteListDiagnosticRenderer.Render(Presentation(result, CliView.Expanded)));

        Assert.InRange(diagnostics.Length, 1, CliRenderingStage.MaximumDiagnosticLength);
        Assert.Contains("workspace.lexical=", diagnostics, StringComparison.Ordinal);
        Assert.Contains("workspace.physical=", diagnostics, StringComparison.Ordinal);
        Assert.Contains("selection.kind=loader-roots", diagnostics, StringComparison.Ordinal);
        Assert.Contains("rows=2", diagnostics, StringComparison.Ordinal);
        Assert.Contains("findings=0", diagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("Memory description", diagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("memory/child", diagnostics, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route list help sections document grammar depth globals exits and accepted examples"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void HelpSectionsDocumentAcceptedSurface()
    {
        var help = RouteListHelpSections.CreateList();
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));

        Assert.Contains("open-forge route list [source-reference] [--depth=<non-negative-integer|all>]", text, StringComparison.Ordinal);
        Assert.Contains("exact source ID", text, StringComparison.Ordinal);
        Assert.Contains("The default depth is 1", text, StringComparison.Ordinal);
        Assert.Contains("--depth=all", text, StringComparison.Ordinal);
        Assert.Contains("--workspace <path>", text, StringComparison.Ordinal);
        Assert.Contains("JSON always writes one result envelope to stdout", text, StringComparison.Ordinal);
        Assert.Contains("route inspect — inspect", text, StringComparison.Ordinal);
        Assert.Contains("open-forge route list memory", text, StringComparison.Ordinal);
        Assert.Contains("open-forge route list .agents/memory/_memory.md", text, StringComparison.Ordinal);
        Assert.Contains("open-forge route list --depth=2 --json", text, StringComparison.Ordinal);
    }

    private static CliInvocation Invocation(CliWorkspace workspace)
    {
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }

    private static CliPresentationRequest<RouteListResult> Presentation(
        RouteListResult result,
        CliView view,
        CliOutputFormat format = CliOutputFormat.Human)
    {
        return new CliPresentationRequest<RouteListResult>(
            result,
            new CliPresentation(format, view, CliVerbosity.Normal));
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
