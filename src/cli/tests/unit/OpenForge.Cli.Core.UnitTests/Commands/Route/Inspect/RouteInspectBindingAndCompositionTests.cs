using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectBindingAndCompositionTests
{
    [Fact(DisplayName = "Manual Route graph composes List and Inspect with current group notes")]
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
        var section = Assert.Single(help.Sections);
        Assert.Equal("Command help", section.Heading);
        var text = string.Join(Environment.NewLine, help.Sections.Select(section => section.Body));
        Assert.DoesNotContain(" available", text, StringComparison.Ordinal);
        Assert.DoesNotContain("list", text, StringComparison.Ordinal);
        Assert.DoesNotContain("inspect", text, StringComparison.Ordinal);
        Assert.DoesNotContain("init", text, StringComparison.Ordinal);
        Assert.DoesNotContain("create", text, StringComparison.Ordinal);
        Assert.Contains("Use open-forge route <command> --help", text, StringComparison.Ordinal);
        Assert.DoesNotContain("update", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Planned but unavailable operation: remove.", text, StringComparison.Ordinal);
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
        Assert.True(request.AllowInteractiveSourceSelection);

        var json = RouteInspectBinding.Bind(
            route.Parse(["inspect", "one"]),
            Invocation(RouteInspectPresentationTestDataWorkspace(), CliOutputFormat.Json),
            symbols);
        var jsonRequest = Assert.IsType<RouteInspectRequest>(json.Request);
        Assert.False(jsonRequest.AllowInteractiveSourceSelection);

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
        var output = new StringWriter();
        var error = new StringWriter();

        string[] arguments = ["inspect"];
        var completion = await binding.InvokeAsync(
            new CliBindingParse(route.Parse(arguments), arguments),
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
        var output = new StringWriter();
        var error = new StringWriter();

        string[] arguments = ["inspect", "first", "second"];
        var completion = await binding.InvokeAsync(
            new CliBindingParse(route.Parse(arguments), arguments),
            Invocation(RouteInspectPresentationTestDataWorkspace()),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        Assert.Equal(0, operationCalls);
        Assert.Equal(4, completion.ExitCode);
    }

    [Theory(DisplayName = "Route Inspect terminal modes keep well-formed globals as no-ops before workspace and operation")]
    [InlineData("--help")]
    [InlineData("--version")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public async Task TerminalModesBypassWorkspaceAndOperationWithWellFormedGlobals(string terminalMode)
    {
        var bindingCalls = 0;
        var operationCalls = 0;
        var application = CreateApplication(() => bindingCalls++, () => operationCalls++);
        var missingWorkspace = Path.Combine(
            Path.GetTempPath(),
            $"open-forge-route-inspect-terminal-unit-{Guid.NewGuid():N}");
        using var output = new StringWriter();
        using var error = new StringWriter();

        var completion = await application.RunAsync(
            [
                "route",
                "inspect",
                "--workspace",
                missingWorkspace,
                "--json",
                "--view=compact",
                "--verbose",
                terminalMode,
            ],
            new CliProcessEnvironment(missingWorkspace),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliOutputTarget.StandardOutput, completion.PrimaryOutputTarget);
        Assert.Equal(0, bindingCalls);
        Assert.Equal(0, operationCalls);
        Assert.NotEmpty(output.ToString());
        Assert.Equal(string.Empty, error.ToString());
        Assert.False(Directory.Exists(missingWorkspace));
    }

    [Theory(DisplayName = "Route Inspect terminal modes reject source input before workspace selection and binding")]
    [InlineData("--help", "root")]
    [InlineData("--version", "root")]
    [InlineData("--help", "option-like")]
    [InlineData("--version", "option-like")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public async Task TerminalModesRejectSourceInputBeforeWorkspaceAndBinding(
        string terminalMode,
        string sourceKind)
    {
        var bindingCalls = 0;
        var operationCalls = 0;
        var application = CreateApplication(() => bindingCalls++, () => operationCalls++);
        var missingWorkspace = Path.Combine(
            Path.GetTempPath(),
            $"open-forge-route-inspect-terminal-conflict-unit-{Guid.NewGuid():N}");
        var arguments = new List<string>
        {
            "route",
            "inspect",
            "--workspace",
            missingWorkspace,
            "--json",
            "--view=compact",
            "--verbose",
            terminalMode,
        };
        switch (sourceKind)
        {
            case "root":
                arguments.Add("root");
                break;
            case "option-like":
                arguments.Add("--");
                arguments.Add("--view");
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(sourceKind),
                    sourceKind,
                    "The terminal source case is not defined.");
        }

        using var output = new StringWriter();
        using var error = new StringWriter();
        var completion = await application.RunAsync(
            arguments.ToArray(),
            new CliProcessEnvironment(missingWorkspace),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, completion.Status);
        Assert.Equal(4, completion.ExitCode);
        Assert.Equal(CliOutputTarget.StandardError, completion.PrimaryOutputTarget);
        Assert.Equal(0, bindingCalls);
        Assert.Equal(0, operationCalls);
        Assert.Equal(string.Empty, output.ToString());
        var diagnostic = Assert.Single(
            error.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
        Assert.InRange(diagnostic.Length, 1, 4096);
        Assert.False(Directory.Exists(missingWorkspace));
    }

    [Fact(DisplayName = "Route Inspect workspace failure retains the parser-owned requested source in a typed invalid result")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public async Task WorkspaceFailureRetainsRequestedSourceThroughBindingContext()
    {
        var route = new Command("route");
        var symbols = RouteInspectBinding.CreateSymbols(route);
        string[] arguments = ["inspect", "requested-source"];
        var parse = route.Parse(arguments);
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
        var bindingParse = new CliBindingParse(parse, arguments);
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
            new CliCommandBindingComponents<RouteInspectRequest, RouteInspectResult>
            {
                Help = CliHelpContent.Empty,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = (_, _) => throw new InvalidOperationException("The invalid path must not bind a request."),
                InvalidResultFactory = inputContext => fixedResult = contextualFactory(inputContext),
                Operation = (request, cancellationToken) =>
                {
                    operationCalls++;
                    return ValueTask.FromResult(RouteInspectPresentationTestData.CompleteResult());
                },
                Renderers = new CliRendererSet<RouteInspectResult>(
                    _ => "invalid",
                    _ => "{}"),
            });
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
        var components = new RouteInspectBindingComponents
        {
            Help = CliHelpContent.Empty,
            Operation = (request, cancellationToken) =>
            {
                operationCalls++;
                return ValueTask.FromResult(RouteInspectPresentationTestData.CompleteResult());
            },
            Renderers = new CliRendererSet<RouteInspectResult>(
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
            DiagnosticRenderer = presentation =>
            {
                diagnosticCalls++;
                return "bounded diagnostic";
            },
        };
        var binding = RouteInspectBinding.Close(symbols, components);
        string[] arguments = ["inspect", "root/item"];
        var parse = route.Parse(arguments);
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await binding.InvokeAsync(
            new CliBindingParse(parse, arguments),
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
        var symbols = RouteInspectBinding.CreateSymbols(route);
        return RouteInspectBinding.Close(
            symbols,
            new RouteInspectBindingComponents
            {
                Help = CliHelpContent.Empty,
                Operation = (request, cancellationToken) =>
                {
                    operationCall();
                    return ValueTask.FromResult(RouteInspectPresentationTestData.CompleteResult());
                },
                Renderers = new CliRendererSet<RouteInspectResult>(
                    _ => "human",
                    _ => "{}"),
                DiagnosticRenderer = null,
            });
    }

    private static CliCoreApplication CreateApplication(
        Action bindingCall,
        Action operationCall)
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteInspectBinding.CreateSymbols(route);
        var binding = new CliCommandBinding<RouteInspectRequest, RouteInspectResult>(
            symbols.InspectCommand,
            new CliCommandBindingComponents<RouteInspectRequest, RouteInspectResult>
            {
                Help = CliHelpContent.Empty,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = (parse, invocation) =>
                {
                    bindingCall();
                    return RouteInspectBinding.Bind(parse.Result, invocation, symbols);
                },
                InvalidResultFactory = RouteInspectBinding.CreateInvalidResultFactory(symbols),
                Operation = (request, cancellationToken) =>
                {
                    operationCall();
                    return ValueTask.FromResult(RouteInspectPresentationTestData.CompleteResult());
                },
                Renderers = new CliRendererSet<RouteInspectResult>(
                    _ => "human",
                    _ => "{}"),
            });
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(route, CliHelpContent.Empty)],
            [binding]);

        return new CliCoreApplication(
            new CliProcessIdentity("open-forge", "test"),
            tree,
            new CliWorkspaceSelector(new PhysicalPathResolver()));
    }

    private static CliInvocation Invocation(
        CliWorkspace workspace,
        CliOutputFormat outputFormat = CliOutputFormat.Human)
    {
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(outputFormat, CliView.Expanded, CliVerbosity.Normal),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }
}
