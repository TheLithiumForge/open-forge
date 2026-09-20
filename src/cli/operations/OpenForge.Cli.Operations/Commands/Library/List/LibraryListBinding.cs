using System.CommandLine;
using OpenForge.Cli.Core.Commands.Library.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.List.Models.Request;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.List.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Library.List;

internal static class LibraryListBinding
{
    internal static LibraryListSymbols CreateSymbols(Command group)
    {
        ArgumentNullException.ThrowIfNull(group);
        var command = new Command(LibraryListDefinitions.Command.Name, LibraryListDefinitions.Command.Description);
        group.Subcommands.Add(command);
        return new LibraryListSymbols
        {
            Command = command,
        };
    }

    internal static CliRequestBinding<LibraryListRequest, LibraryListResult> CreateRequestBinding(LibraryListSymbols symbols, LibraryListBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        return new CliRequestBinding<LibraryListRequest, LibraryListResult>
        {
            Command = symbols.Command,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => LibraryListRequestBinder.Bind(parse, invocation, symbols),
            InvalidResultFactory = LibraryListRequestBinder.CreateInvalid,
            Operation = components.Operation.ExecuteAsync,
        };
    }
}
