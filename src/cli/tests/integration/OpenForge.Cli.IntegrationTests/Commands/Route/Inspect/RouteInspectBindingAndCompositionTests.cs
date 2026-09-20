using System.CommandLine;
using System.Text.Json;
using OpenForge.Cli.Core.UnitTests.Commands.Shared.Presentation;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
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
    [Fact(DisplayName = "Route Inspect missing source operand bypasses the closed operation")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
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
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
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
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
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
                "--format=json",
                "--detail=minimal",
                "--detail-filter=all",
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
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
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
            "--format=json",
            "--detail=minimal",
            "--detail-filter=all",
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
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task WorkspaceFailureRetainsRequestedSourceThroughBindingContext()
    {
        var route = new Command("route");
        var symbols = RouteInspectBinding.CreateSymbols(route);
        string[] arguments = ["inspect", "requested-source"];
        var parse = route.Parse(arguments);
        var input = new CliGlobalInput(
            null,
            0,
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
        var bindingParse = new CliBindingParse(parse, arguments);
        var context = new CliInvalidBindingInput(
            invalidInput,
            input,
            new CliProcessEnvironment(Path.GetTempPath()),
            bindingParse);

        var contextualFactory = RouteInspectBinding.CreateInvalidResultFactory(symbols);
        var operationCalls = 0;
        RouteInspectResult? fixedResult = null;
        var binding = CliReportBinding.Close(
            new CliRequestBinding<RouteInspectRequest, RouteInspectResult>
            {
                Command = symbols.InspectCommand,
                Help = CliHelpContent.Empty,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = (_, _) => throw new InvalidOperationException("The invalid path must not bind a request."),
                InvalidResultFactory = inputContext => fixedResult = contextualFactory(inputContext),
                Operation = (request, cancellationToken) =>
                {
                    operationCalls++;
                    return ValueTask.FromResult(RouteInspectPresentationTestData.CompleteResult());
                },
            }, CommandBindingTestRendering.Create<RouteInspectResult>("invalid"));
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
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    [InlineData((int)CliFormat.Text, (int)CliDetail.Standard, "human", false)]
    [InlineData((int)CliFormat.Json, (int)CliDetail.Standard, "json", false)]
    [InlineData((int)CliFormat.Text, (int)CliDetail.Debug, "human", true)]
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
        var components = new RouteInspectBindingComponents
        {
            Help = CliHelpContent.Empty,
            Operation = (request, cancellationToken) =>
            {
                operationCalls++;
                return ValueTask.FromResult(RouteInspectPresentationTestData.CompleteResult());
            },
        };
        var binding = CliReportBinding.Close(RouteInspectBinding.CreateRequestBinding(symbols, components),
            CommandBindingTestRendering.Create<RouteInspectResult>(selected: _ => { rendererCalls++; }, diagnostics: ["bounded diagnostic"]));
        string[] arguments = ["inspect", "root/item"];
        var parse = route.Parse(arguments);
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await binding.InvokeAsync(
            new CliBindingParse(parse, arguments),
            new CliInvocation(
                new CliProcessIdentity("open-forge", "test"),
                new CliPresentation(
                    (CliFormat)formatValue,
                    (CliDetail)verbosityValue,
                    null),
                CliTerminalMode.None,
                new CliWorkspaceRequest(null, Path.GetTempPath()),
                RouteInspectPresentationTestDataWorkspace()),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        Assert.Equal(1, operationCalls);
        Assert.Equal(1, rendererCalls);
        if (expectedOutput == "json")
        {
            using var document = JsonDocument.Parse(output.ToString());
            Assert.Equal("json", document.RootElement.GetProperty("data").GetProperty("marker").GetString());
        }
        else
        {
            Assert.Equal(expectedOutput + Environment.NewLine, output.ToString());
        }
        Assert.Equal(expectsDiagnostic ? "bounded diagnostic" + Environment.NewLine : string.Empty, error.ToString());
        Assert.Equal(0, completion.ExitCode);
    }

    private static CliWorkspace RouteInspectPresentationTestDataWorkspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "route-inspect-presentation-unit"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static ICliCommandBinding CreateCountingBinding(
        Command route,
        Action operationCall)
    {
        var symbols = RouteInspectBinding.CreateSymbols(route);
        return CliReportBinding.Close(RouteInspectBinding.CreateRequestBinding(
            symbols,
            new RouteInspectBindingComponents
            {
                Help = CliHelpContent.Empty,
                Operation = (request, cancellationToken) =>
                {
                    operationCall();
                    return ValueTask.FromResult(RouteInspectPresentationTestData.CompleteResult());
                },
            }), CommandBindingTestRendering.Create<RouteInspectResult>());
    }

    private static CliCoreApplication CreateApplication(
        Action bindingCall,
        Action operationCall)
    {
        var route = RouteBinding.CreateGroup();
        var symbols = RouteInspectBinding.CreateSymbols(route);
        var binding = CliReportBinding.Close(
            new CliRequestBinding<RouteInspectRequest, RouteInspectResult>
            {
                Command = symbols.InspectCommand,
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
            }, CommandBindingTestRendering.Create<RouteInspectResult>());
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
        CliFormat outputFormat = CliFormat.Text)
    {
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(outputFormat, CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }
}
