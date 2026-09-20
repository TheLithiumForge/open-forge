using OpenForge.Cli.Core.Commands.Find.Shared.Application;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using System.CommandLine;
using System.Text.Json;
using OpenForge.Cli.Core.UnitTests.Commands.Shared.Presentation;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Commands.Find.Shared.Projection;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Selection;
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
using OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find;

public sealed class FindPresentationBindingTests
{

    [Theory(DisplayName = "Closed Find binding invokes its operation once and selects one renderer without a second operation path"),
        InlineData((int)CliFormat.Text, (int)CliDetail.Standard, "human", 1, 0),
        InlineData((int)CliFormat.Json, (int)CliDetail.Debug, "json", 1, 1),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
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
        var binding = CliReportBinding.Close(
            FindBinding.CreateRequestBinding(symbols, new FindBindingComponents { Help = CliHelpContent.Empty, Operation = operation }),
            CommandBindingTestRendering.Create<FindResult>(
                selected: _ => { jsonCalls++; },
                textRendered: () => { humanCalls++; },
                diagnostics: ["bounded diagnostic"]));
        string[] arguments = ["find"];
        var parse = symbols.FindCommand.Parse(arguments);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var completion = await binding.InvokeAsync(
            new CliBindingParse(parse, arguments),
            Invocation(
                Workspace(),
                (CliFormat)formatValue,
                (CliDetail)verbosityValue),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);

        Assert.Equal(1, operationCalls);
        Assert.Equal(expectedRenderer == "human" ? expectedRendererCalls : 0, humanCalls);
        Assert.Equal(1, jsonCalls);
        Assert.Equal(expectedDiagnosticCalls == 1, error.ToString().Contains("bounded diagnostic", StringComparison.Ordinal));
        Assert.Equal(1, completion.ExitCode);
        if (expectedRenderer == "json")
        {
            using var document = JsonDocument.Parse(output.ToString());
            Assert.Equal("json", document.RootElement.GetProperty("data").GetProperty("marker").GetString());
        }
        else
        {
            Assert.Equal(string.Empty, output.ToString());
        }
        Assert.Equal(
            expectedRenderer == "human"
                ? "human" + Environment.NewLine
                : "bounded diagnostic" + Environment.NewLine,
            error.ToString());
    }

    [Theory(DisplayName = "Find help and version remain terminal modes before workspace selection and operation"), InlineData("--help"), InlineData("--version"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task TerminalModesBypassWorkspaceAndOperation(string terminalMode)
    {
        var command = new Command("find");
        var bindingCalls = 0;
        var operationCalls = 0;
        var binding = CliReportBinding.Close(
            new CliRequestBinding<FindRequest, FindResult>
            {
                Command = command,
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
            }, CommandBindingTestRendering.Create<FindResult>());
        var tree = CliCommandTree.Create(
            CliHelpContent.Empty,
            [new CliRootBranch(command, binding.Help)],
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

    private static CliInvocation Invocation(
        CliWorkspace workspace,
        CliFormat format = CliFormat.Json,
        CliDetail? diagnosticDetail = null)
        => new(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(format, diagnosticDetail ?? CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-binding-presentation-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

}

