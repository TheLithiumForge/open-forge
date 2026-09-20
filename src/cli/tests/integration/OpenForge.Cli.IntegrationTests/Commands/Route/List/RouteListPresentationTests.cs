using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Route.List;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List;

public sealed class RouteListPresentationTests
{
    [Fact(DisplayName = "Route list invalid depth binding bypasses the operation pipeline"), Trait("Feature", "route-list"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task InvalidDepthBypassesOperation()
    {
        var symbols = RouteListBinding.CreateSymbols(RouteBinding.CreateGroup());
        var operationCalls = 0;
        var fallback = CompleteResult();
        var binding = OpenForge.Cli.Core.Shell.Composition.CliReportBinding.Close(RouteListBinding.CreateRequestBinding(
            symbols,
            new RouteListBindingComponents
            {
                Help = CliHelpContent.Empty,
                Operation = (request, cancellationToken) =>
                {
                    operationCalls++;
                    return ValueTask.FromResult(fallback);
                },
            }), RouteListPresentation.Rendering);
        var output = new StringWriter();
        var error = new StringWriter();
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(symbols.RouteGroup, CliHelpContent.Empty)],
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
        Assert.Contains("Cannot list routes: --depth must be a whole number or all.", error.ToString(), StringComparison.Ordinal);
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
