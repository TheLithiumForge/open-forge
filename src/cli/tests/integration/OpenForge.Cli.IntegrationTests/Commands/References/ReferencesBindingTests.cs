using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Shared.Documents;
using OpenForge.Cli.Core.Commands.References.Shared.Inspection;
using OpenForge.Cli.Core.Commands.References.Shared.Resolution;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.References;

public sealed class ReferencesBindingTests
{

    [Fact(DisplayName = "Closed References binding invokes the operation once and selects exactly one renderer with optional bounded diagnostics"), Trait("Feature", "references"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task ClosedBindingInvokesOperationOnceAndSelectsOneRenderer()
    {
        var symbols = ReferencesBinding.CreateSymbols();
        var operationCalls = 0;
        var humanCalls = 0;
        var jsonCalls = 0;
        SourcePhysicalPathResolver physicalPathResolver = (_, _) => throw new InvalidOperationException(
            "The failed operation boundary must stop before path resolution.");
        ReferencesMarkdownParser markdownParser = _ => throw new InvalidOperationException("markdown parse must not run");
        var operation = new ReferencesOperation(
            new ReferencesSourceResolver(
                (_, _) =>
                {
                    operationCalls++;
                    throw new IOException("expected boundary failure");
                },
                new SourceReferenceResolver(physicalPathResolver),
                new SourceUniverseFilterResolver(
                    new SourceReferenceResolver(physicalPathResolver))),
            new ReferencesLayerInspector(
                (_, _, _) => throw new InvalidOperationException("layer read must not run"),
                markdownParser),
            new ReferencesDestinationResolver(
                (_, _) => throw new InvalidOperationException("path resolve must not run"),
                (_, _, _) => throw new InvalidOperationException("utf8 read must not run"),
                markdownParser),
            new ReferencesResultBuilder());
        var binding = OpenForge.Cli.Core.Shell.Composition.CliReportBinding.Close(ReferencesBinding.CreateRequestBinding(
            symbols,
            new ReferencesBindingComponents
            {
                Help = CliHelpContent.Empty,
                Operation = operation,
            }), OpenForge.Cli.Core.UnitTests.Commands.Shared.Presentation.CommandBindingTestRendering.Create<ReferencesResult>(
                selected: _ => { jsonCalls++; }, textRendered: () => { humanCalls++; }, diagnostics: ["bounded diagnostic"]));

        using var output = new StringWriter();
        using var error = new StringWriter();
        string[] arguments = ["references", "docs"];
        var completion = await binding.InvokeAsync(
            new CliBindingParse(symbols.ReferencesCommand.Parse(arguments), arguments),
            Invocation(CliFormat.Json, CliDetail.Debug),
            new CliOutputWriters(output, error),
            CancellationToken.None);

        Assert.Equal(1, operationCalls);
        Assert.Equal(0, humanCalls);
        Assert.Equal(1, jsonCalls);
        Assert.Equal(1, completion.ExitCode);
        using var document = System.Text.Json.JsonDocument.Parse(output.ToString());
        Assert.Equal("json", document.RootElement.GetProperty("data").GetProperty("marker").GetString());
        Assert.Equal("bounded diagnostic" + Environment.NewLine, error.ToString());
    }

    private static CliInvocation Invocation(
        CliFormat format = CliFormat.Json,
        CliDetail? diagnosticDetail = null)
    {
        var workspace = ReferencesPresentationTestData.Workspace();
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(format, diagnosticDetail ?? CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }

}
