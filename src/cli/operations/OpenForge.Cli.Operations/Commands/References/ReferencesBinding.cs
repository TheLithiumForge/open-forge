using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.References;

internal static class ReferencesBinding
{
    internal static ReferencesSymbols CreateSymbols() => ReferencesSymbols.CreateSymbols();

    internal static CliRequestBinding<ReferencesRequest, ReferencesResult> CreateRequestBinding(
        ReferencesSymbols symbols,
        ReferencesBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(components.Operation);

        var binder = new ReferencesRequestBinder(symbols);
        var workspaceResultFactory = new ReferencesWorkspaceResultFactory(symbols);
        return new CliRequestBinding<ReferencesRequest, ReferencesResult>
        {
            Command = symbols.ReferencesCommand,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = binder.Bind,
            InvalidResultFactory = workspaceResultFactory.Create,
            Operation = components.Operation.ExecuteAsync,
        };
    }
}
