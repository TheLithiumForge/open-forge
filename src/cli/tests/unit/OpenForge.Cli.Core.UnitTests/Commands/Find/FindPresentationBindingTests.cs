using System.CommandLine;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Application;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Commands.Find.Shared.Projection;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find;

public sealed class FindPresentationBindingTests
{
    [Fact(DisplayName = "Find binding exposes the exact seven-option command and preserves one command identity"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void BindingSymbolsExposeExactCommandIdentity()
    {
        var symbols = FindBinding.CreateSymbols();

        Assert.Equal("find", symbols.FindCommand.Name);
        Assert.Empty(symbols.FindCommand.Aliases);
        Assert.Empty(symbols.FindCommand.Arguments);
        Assert.Empty(symbols.FindCommand.Subcommands);
        Assert.Equal(
            ["--include", "--exclude", "--tag", "--heading", "--require", "--within", "--content"],
            symbols.FindCommand.Options.Select(option => option.Name));
        AssertRepeatable(symbols.Include);
        AssertRepeatable(symbols.Exclude);
        AssertRepeatable(symbols.Tag);
        AssertRepeatable(symbols.Heading);
        AssertSingleton(symbols.Require);
        AssertSingleton(symbols.Within);
        AssertSingleton(symbols.Content);
    }

    [Fact(DisplayName = "Find binding closure requires its typed operation, one renderer catalogue, and optional diagnostic renderer"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void BindingClosureUsesRequiredComponentsExactlyOnce()
    {
        var symbols = FindBinding.CreateSymbols();
        var operation = FindOperationFactory.Create();
        var components = new FindBindingComponents
        {
            Help = new CliHelpContent([
                new CliHelpSection("Syntax", "find")
            ]),
            Operation = operation,
            Renderers = new CliRendererSet<FindResult>(_ => "human", _ => "json"),
            DiagnosticRenderer = _ => "bounded diagnostic",
        };

        var binding = FindBinding.Close(symbols, components);

        Assert.Same(symbols.FindCommand, binding.Command);
        Assert.Same(components.Help, binding.Help);
        Assert.Equal(CliWorkspaceRequirement.Required, binding.WorkspaceRequirement);
    }

    [Theory(DisplayName = "Closed Find binding invokes its operation once and selects one renderer without a second operation path"),
        InlineData((int)CliOutputFormat.Human, (int)CliVerbosity.Normal, "human", 1, 0),
        InlineData((int)CliOutputFormat.Json, (int)CliVerbosity.Verbose, "json", 1, 1),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public async Task ClosedBindingInvokesOneOperationAndOneSelectedRenderer(
        int formatValue,
        int verbosityValue,
        string expectedRenderer,
        int expectedRendererCalls,
        int expectedDiagnosticCalls)
    {
        var symbols = FindBinding.CreateSymbols();
        var operationCalls = 0;
        var humanCalls = 0;
        var jsonCalls = 0;
        var diagnosticCalls = 0;
        SourcePhysicalPathResolver physicalPathResolver = (_, _) => throw new InvalidOperationException(
            "The failed boundary must stop before path resolution.");
        var operation = new FindOperation(
            new FindSourceResolver(
                (_, _) =>
                {
                    operationCalls++;
                    throw new IOException("The test operation boundary failed deterministically.");
                },
                new FindUniverseResolver(new SourceUniverseFilterResolver(
                    new SourceReferenceResolver(physicalPathResolver))),
                physicalPathResolver,
                (_, _, _) => throw new InvalidOperationException("The failed boundary must stop before route facts.")),
            new FindLayerInspector(
                (_, _, _) => throw new InvalidOperationException("The failed boundary must stop before layer reads."),
                _ => throw new InvalidOperationException("The failed boundary must stop before Markdown parsing."),
                _ => throw new InvalidOperationException("The failed boundary must stop before frontmatter parsing."),
                new FindBodyTagScanner()),
            new FindMatcher(),
            new FindProjectionBuilder(),
            new FindResultBuilder());
        var binding = FindBinding.Close(
            symbols,
            new FindBindingComponents
            {
                Help = CliHelpContent.Empty,
                Operation = operation,
                Renderers = new CliRendererSet<FindResult>(
                    _ =>
                    {
                        humanCalls++;
                        return "human";
                    },
                    _ =>
                    {
                        jsonCalls++;
                        return "json";
                    }),
                DiagnosticRenderer = _ =>
                {
                    diagnosticCalls++;
                    return "bounded diagnostic";
                },
            });
        string[] arguments = ["find"];
        var parse = symbols.FindCommand.Parse(arguments);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var completion = await binding.InvokeAsync(
            new CliBindingParse(parse, arguments),
            Invocation(
                Workspace(),
                (CliOutputFormat)formatValue,
                (CliVerbosity)verbosityValue),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        Assert.Equal(1, operationCalls);
        Assert.Equal(expectedRenderer == "human" ? expectedRendererCalls : 0, humanCalls);
        Assert.Equal(expectedRenderer == "json" ? expectedRendererCalls : 0, jsonCalls);
        Assert.Equal(expectedDiagnosticCalls, diagnosticCalls);
        Assert.Equal(1, completion.ExitCode);
        Assert.Equal(
            expectedRenderer == "json" ? "json" + Environment.NewLine : string.Empty,
            output.ToString());
        Assert.Equal(
            expectedRenderer == "human"
                ? "human" + Environment.NewLine
                : "bounded diagnostic" + Environment.NewLine,
            error.ToString());
    }

    [Fact(DisplayName = "Typed Find invalid input retains an explicitly malformed content request with no parsed parts"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void InvalidContentRetainsExplicitPresenceState()
    {
        var symbols = FindBinding.CreateSymbols();
        string[] arguments = ["find", "--content=unknown-part"];
        var parse = symbols.FindCommand.Parse(arguments);
        Assert.Empty(parse.Errors);
        var bound = new FindRequestBinder(symbols, new FindResultBuilder()).Bind(
            new CliBindingParse(parse, arguments),
            Invocation(Workspace()));

        var result = Assert.IsType<FindResult>(bound.InvalidResult);
        Assert.Null(bound.Request);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.True(result.Presentation.Content.IsRequested);
        Assert.Empty(result.Presentation.Content.Supplied);
        Assert.Empty(result.Presentation.Content.Effective);
        Assert.Equal(FindProjectionCoverageState.NotStarted, result.Coverage.Projection);
    }

    [Fact(DisplayName = "Find workspace selection failure produces the typed blocked result with a null workspace"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void WorkspaceSelectionFailureUsesTypedBlockedResult()
    {
        var symbols = FindSymbols.Create();
        string[] arguments = ["find", "--include=docs"];
        var parse = symbols.FindCommand.Parse(arguments);
        var invalidInput = new CliInvalidBindingInput(
            new CliInvalidInput(
                "cli.workspace.invalid",
                CliInvalidInputSource.Workspace,
                ["The selected workspace is missing."]),
            GlobalInput(),
            new CliProcessEnvironment(Path.GetTempPath()),
            new CliBindingParse(parse, arguments));

        var result = FindBindingSupport.CreateWorkspaceUnavailableResult(
            new FindResultBuilder(),
            FindBindingSupport.ReadQueryInput(parse, symbols, CliView.Expanded),
            invalidInput);

        Assert.Null(result.Workspace);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(FindFindingCode.WorkspaceUnavailable, Assert.Single(result.Findings).Code);
        Assert.Empty(result.Matches);
    }

    [Theory(DisplayName = "Find help and version remain terminal modes before workspace selection and operation"), InlineData("--help"), InlineData("--version"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public async Task TerminalModesBypassWorkspaceAndOperation(string terminalMode)
    {
        var command = new Command("find");
        var bindingCalls = 0;
        var operationCalls = 0;
        var binding = new CliCommandBinding<FindRequest, FindResult>(
            command,
            new CliCommandBindingComponents<FindRequest, FindResult>
            {
                Help = new CliHelpContent([
                    new CliHelpSection("Syntax", "find")
                ]),
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = (_, _) =>
                {
                    bindingCalls++;
                    throw new InvalidOperationException("Terminal modes must not bind.");
                },
                InvalidResultFactory = _ => throw new InvalidOperationException("Terminal modes must not create an invalid result."),
                Operation = (_, _) =>
                {
                    operationCalls++;
                    return ValueTask.FromResult(FindPresentationTestData.CompleteResult());
                },
                Renderers = new CliRendererSet<FindResult>(_ => "human", _ => "json"),
            });
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(command, binding.Help, [])],
            [binding]);
        var application = new CliCoreApplication(
            new CliProcessIdentity("open-forge", "test"),
            tree,
            new CliWorkspaceSelector(new PhysicalPathResolver()));
        using var output = new StringWriter();
        using var error = new StringWriter();

        var completion = await application.RunAsync(
            ["find", terminalMode, "--workspace", Path.Combine(Path.GetTempPath(), "missing-find-terminal")],
            new CliProcessEnvironment(Path.GetTempPath()),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(0, bindingCalls);
        Assert.Equal(0, operationCalls);
        Assert.NotEmpty(output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    private static void AssertRepeatable<T>(Option<T[]> option)
    {
        Assert.Equal(ArgumentArity.ZeroOrMore, option.Arity);
        Assert.False(option.AllowMultipleArgumentsPerToken);
        Assert.Equal(typeof(T[]), option.ValueType);
    }

    private static void AssertSingleton<T>(Option<T> option)
    {
        Assert.Equal(ArgumentArity.ZeroOrOne, option.Arity);
        Assert.Equal(typeof(T), option.ValueType);
    }

    private static CliInvocation Invocation(
        CliWorkspace workspace,
        CliOutputFormat format = CliOutputFormat.Json,
        CliVerbosity verbosity = CliVerbosity.Normal)
        => new(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(format, CliView.Expanded, verbosity),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);

    private static CliGlobalInput GlobalInput()
        => new(
            null,
            0,
            CliOutputFormat.Json,
            0,
            CliView.Expanded,
            0,
            CliVerbosity.Normal,
            0,
            false,
            0,
            false,
            0);

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-binding-presentation-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
