using OpenForge.Cli.Core.Commands.Index.Models.Binding;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Shared.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Index;

internal static class IndexBinding
{
    internal static IndexSymbols CreateSymbols() => IndexSymbols.Create();

    internal static CliCommandBinding<IndexRequest, IndexResult> Close(
        IndexSymbols symbols,
        CliHelpContent help,
        CliOperation<IndexRequest, IndexResult> operation,
        CliRendererSet<IndexResult> renderers,
        CliDiagnosticRenderer<IndexResult>? diagnosticRenderer)
    {
        var resultBuilder = new IndexResultBuilder();
        return new CliCommandBinding<IndexRequest, IndexResult>(
            symbols.IndexCommand,
            new CliCommandBindingComponents<IndexRequest, IndexResult>
            {
                Help = help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = new IndexRequestBinder(symbols, resultBuilder).Bind,
                InvalidResultFactory = new IndexWorkspaceResultFactory(symbols, resultBuilder).Create,
                Operation = operation,
                Renderers = renderers,
                DiagnosticRenderer = diagnosticRenderer,
            });
    }
}
