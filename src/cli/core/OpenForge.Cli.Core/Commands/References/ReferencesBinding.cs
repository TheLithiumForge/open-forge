using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.References;

internal static class ReferencesBinding
{
    internal static ReferencesSymbols CreateSymbols() => ReferencesSymbols.CreateSymbols();

    internal static CliCommandBinding<ReferencesRequest, ReferencesResult> Close(
        ReferencesSymbols symbols,
        ReferencesBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(components.Operation);

        var resultBuilder = new ReferencesResultBuilder();
        var binder = new ReferencesRequestBinder(symbols, resultBuilder);
        var workspaceResultFactory = new ReferencesWorkspaceResultFactory(symbols, resultBuilder);
        return new CliCommandBinding<ReferencesRequest, ReferencesResult>(
            symbols.ReferencesCommand,
            new CliCommandBindingComponents<ReferencesRequest, ReferencesResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = binder.Bind,
                InvalidResultFactory = workspaceResultFactory.Create,
                Operation = components.Operation.ExecuteAsync,
                Renderers = components.Renderers,
                DiagnosticRenderer = components.DiagnosticRenderer,
            });
    }
}
