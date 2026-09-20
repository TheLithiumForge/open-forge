using System.CommandLine;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Library.Inspect;

internal static class LibraryInspectBinding
{
    internal static LibraryInspectSymbols CreateSymbols(Command group)
    {
        ArgumentNullException.ThrowIfNull(group);
        var command = new Command(LibraryInspectDefinitions.Command.Name, LibraryInspectDefinitions.Command.Description);
        var libraryId = new Argument<string?>(LibraryDefinitions.LibraryId.Name)
        {
            Description = LibraryDefinitions.LibraryId.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        command.Arguments.Add(libraryId);
        group.Subcommands.Add(command);
        return new LibraryInspectSymbols
        {
            Command = command,
            LibraryId = libraryId,
        };
    }

    internal static CliRequestBinding<LibraryInspectRequest, LibraryInspectResult> CreateRequestBinding(LibraryInspectSymbols symbols, LibraryInspectBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        return new CliRequestBinding<LibraryInspectRequest, LibraryInspectResult>
        {
            Command = symbols.Command,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => LibraryInspectRequestBinder.Bind(parse, invocation, symbols),
            InvalidResultFactory = LibraryInspectRequestBinder.CreateInvalid,
            Operation = components.Operation.ExecuteAsync,
        };
    }
}
