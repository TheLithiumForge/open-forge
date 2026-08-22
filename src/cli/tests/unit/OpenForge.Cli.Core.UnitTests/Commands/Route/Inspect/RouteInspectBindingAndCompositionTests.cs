using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectBindingAndCompositionTests
{
    [Fact(DisplayName = "Route family composes one group with exact List and Inspect children and shared help")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void RouteFamilyComposesOneGroupWithExactChildren()
    {
        var route = RouteBinding.CreateGroup();
        var list = RouteListBinding.CreateSymbols(route);
        var inspect = RouteInspectBinding.CreateSymbols(route);

        Assert.Same(route, list.RouteGroup);
        Assert.Same(route, inspect.RouteGroup);
        Assert.Equal(["list", "inspect"], route.Subcommands.Select(command => command.Name));
        Assert.Same(list.ListCommand, route.Subcommands[0]);
        Assert.Same(inspect.InspectCommand, route.Subcommands[1]);

        var help = RouteHelpSections.CreateGroup();
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));
        Assert.Contains("list     available", text, StringComparison.Ordinal);
        Assert.Contains("inspect  available", text, StringComparison.Ordinal);
        Assert.Contains("init     unavailable", text, StringComparison.Ordinal);
        Assert.Contains("remove   unavailable", text, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Inspect owns one parser argument with omission and typed semantic cardinality")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void InspectArgumentUsesParserOwnedValuesAndTypedCardinality()
    {
        var route = new Command("route");
        var symbols = RouteInspectBinding.CreateSymbols(route);
        var invocation = Invocation(RouteInspectPresentationTestDataWorkspace());

        Assert.Equal(ArgumentArity.ZeroOrMore, symbols.SourceReferences.Arity);
        Assert.Equal(typeof(string[]), symbols.SourceReferences.ValueType);

        var missing = RouteInspectBinding.Bind(
            route.Parse(["inspect"]),
            invocation,
            symbols);
        var missingResult = Assert.IsType<RouteInspectResult>(missing.InvalidResult);
        Assert.Null(missing.Request);
        Assert.Equal(CliSemanticStatus.Invalid, missingResult.Status);
        Assert.Equal(RouteInspectConditionCode.MissingSource, Assert.Single(missingResult.Conditions).Code);

        var multiple = RouteInspectBinding.Bind(
            route.Parse(["inspect", "first", "second"]),
            invocation,
            symbols);
        var multipleResult = Assert.IsType<RouteInspectResult>(multiple.InvalidResult);
        Assert.Null(multiple.Request);
        Assert.Equal(CliSemanticStatus.Invalid, multipleResult.Status);
        Assert.Equal(RouteInspectConditionCode.MultipleSources, Assert.Single(multipleResult.Conditions).Code);

        var one = RouteInspectBinding.Bind(
            route.Parse(["inspect", "one"]),
            invocation,
            symbols);
        var request = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation.RouteInspectRequest>(one.Request);
        Assert.Null(one.InvalidResult);
        Assert.Equal("one", request.SourceReference);

        var optionLike = RouteInspectBinding.Bind(
            route.Parse(["inspect", "--", "--view"]),
            invocation,
            symbols);
        var optionLikeRequest = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation.RouteInspectRequest>(optionLike.Request);
        Assert.Equal("--view", optionLikeRequest.SourceReference);
    }

    [Fact(DisplayName = "Route Inspect missing source operand bypasses the closed operation")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public async Task MissingOperandDoesNotInvokeOperation()
    {
        var route = new Command("route");
        var operationCalls = 0;
        var binding = CreateCountingBinding(route, () => operationCalls++);
        var arguments = new[] { "route", "inspect" };
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await binding.InvokeAsync(
            new CliBindingParse(route.Parse(["inspect"]), arguments),
            Invocation(RouteInspectPresentationTestDataWorkspace()),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        Assert.Equal(0, operationCalls);
        Assert.Equal(4, completion.ExitCode);
    }

    [Fact(DisplayName = "Route Inspect multiple source operands bypass the closed operation")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public async Task MultipleOperandsDoNotInvokeOperation()
    {
        var route = new Command("route");
        var operationCalls = 0;
        var binding = CreateCountingBinding(route, () => operationCalls++);
        var arguments = new[] { "route", "inspect", "first", "second" };
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await binding.InvokeAsync(
            new CliBindingParse(route.Parse(["inspect", "first", "second"]), arguments),
            Invocation(RouteInspectPresentationTestDataWorkspace()),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        Assert.Equal(0, operationCalls);
        Assert.Equal(4, completion.ExitCode);
    }

    [Fact(DisplayName = "Route Inspect workspace failure retains the parser-owned requested source in a typed invalid result")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public async Task WorkspaceFailureRetainsRequestedSourceThroughBindingContext()
    {
        var route = new Command("route");
        var symbols = RouteInspectBinding.CreateSymbols(route);
        var parse = route.Parse(["inspect", "requested-source"]);
        var input = new CliGlobalInput(
            null,
            0,
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
        var bindingParse = new CliBindingParse(
            parse,
            ["route", "inspect", "requested-source", "--json"]);
        var context = new CliInvalidBindingInput(
            invalidInput,
            input,
            new CliProcessEnvironment(Path.GetTempPath()),
            bindingParse);

        var contextualFactory = RouteInspectBinding.CreateInvalidResultFactory(symbols);
        var operationCalls = 0;
        RouteInspectResult? fixedResult = null;
        var binding = new CliCommandBinding<RouteInspectRequest, RouteInspectResult>(
            symbols.InspectCommand,
            CliHelpContent.Empty,
            CliWorkspaceRequirement.Required,
            (_, _) => throw new InvalidOperationException("The invalid path must not bind a request."),
            inputContext => fixedResult = contextualFactory(inputContext),
            (request, cancellationToken) =>
            {
                operationCalls++;
                return ValueTask.FromResult(RouteInspectPresentationTestData.CompleteResult());
            },
            new CliRendererSet<RouteInspectResult>(
                _ => "invalid",
                _ => "{}"));
        using var output = new StringWriter();
        using var error = new StringWriter();

        await binding.PresentInvalidAsync(
            context,
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        var result = Assert.IsType<RouteInspectResult>(fixedResult);
        Assert.Equal(0, operationCalls);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal("requested-source", result.Selection.RequestedReference);
        Assert.Equal(RouteInspectConditionCode.InvalidWorkspace, Assert.Single(result.Conditions).Code);
        Assert.Null(result.Identity);
        Assert.Null(result.Profile);
        Assert.NotNull(result.Next);
    }

    [Theory(DisplayName = "Closed Route Inspect binding invokes one counting operation and selected cached renderer per invocation")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    [InlineData((int)CliOutputFormat.Human, (int)CliVerbosity.Normal, "human", false)]
    [InlineData((int)CliOutputFormat.Json, (int)CliVerbosity.Normal, "json", false)]
    [InlineData((int)CliOutputFormat.Human, (int)CliVerbosity.Verbose, "human", true)]
    public async Task ClosedBindingInvokesOperationOnceAndRendersCachedResult(
        int formatValue,
        int verbosityValue,
        string expectedOutput,
        bool expectsDiagnostic)
    {
        var route = new Command("route");
        var symbols = RouteInspectBinding.CreateSymbols(route);
        var operationCalls = 0;
        var rendererCalls = 0;
        var diagnosticCalls = 0;
        var components = new RouteInspectBindingComponents(
            CliHelpContent.Empty,
            (request, cancellationToken) =>
            {
                operationCalls++;
                return ValueTask.FromResult(RouteInspectPresentationTestData.CompleteResult());
            },
            new CliRendererSet<RouteInspectResult>(
                presentation =>
                {
                    rendererCalls++;
                    return "human";
                },
                presentation =>
                {
                    rendererCalls++;
                    return "json";
                }),
            presentation =>
            {
                diagnosticCalls++;
                return "bounded diagnostic";
            });
        var binding = RouteInspectBinding.Close(symbols, components);
        var parse = route.Parse(["inspect", "root/item"]);
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await binding.InvokeAsync(
            new CliBindingParse(parse, ["route", "inspect", "root/item"]),
            new CliInvocation(
                new CliProcessIdentity("open-forge", "test"),
                new CliPresentation(
                    (CliOutputFormat)formatValue,
                    CliView.Expanded,
                    (CliVerbosity)verbosityValue),
                CliTerminalMode.None,
                new CliWorkspaceRequest(null, Path.GetTempPath()),
                RouteInspectPresentationTestDataWorkspace()),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        Assert.Equal(1, operationCalls);
        Assert.Equal(1, rendererCalls);
        Assert.Equal(expectsDiagnostic ? 1 : 0, diagnosticCalls);
        Assert.Equal(expectedOutput + Environment.NewLine, output.ToString());
        Assert.Equal(expectsDiagnostic ? "bounded diagnostic" + Environment.NewLine : string.Empty, error.ToString());
        Assert.Equal(0, completion.ExitCode);
    }

    private static CliWorkspace RouteInspectPresentationTestDataWorkspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "route-inspect-presentation-unit"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static CliCommandBinding<RouteInspectRequest, RouteInspectResult> CreateCountingBinding(
        Command route,
        Action operationCall)
    {
        ArgumentNullException.ThrowIfNull(operationCall);
        var symbols = RouteInspectBinding.CreateSymbols(route);
        return RouteInspectBinding.Close(
            symbols,
            new RouteInspectBindingComponents(
                CliHelpContent.Empty,
                (request, cancellationToken) =>
                {
                    operationCall();
                    return ValueTask.FromResult(RouteInspectPresentationTestData.CompleteResult());
                },
                new CliRendererSet<RouteInspectResult>(
                    _ => "human",
                    _ => "{}"),
                null));
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
}
