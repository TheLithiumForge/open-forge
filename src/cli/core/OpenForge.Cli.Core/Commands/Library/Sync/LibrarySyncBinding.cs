using System.CommandLine;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Library.Sync;

internal static class LibrarySyncBinding
{
    internal static LibrarySyncSymbols CreateSymbols(Command group)
    {
        ArgumentNullException.ThrowIfNull(group);
        var command = new Command(LibrarySyncDefinitions.Command.Name, LibrarySyncDefinitions.Command.Description);
        var libraryId = new Argument<string?>(LibraryDefinitions.LibraryId.Name)
        {
            Description = LibraryDefinitions.LibraryId.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        command.Arguments.Add(libraryId);
        var dryRun = new Option<bool>(LibraryDefinitions.DryRun.Name)
        {
            Description = LibraryDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };
        command.Options.Add(dryRun);
        group.Subcommands.Add(command);
        return new LibrarySyncSymbols
        {
            Command = command,
            LibraryId = libraryId,
            DryRun = dryRun,
        };
    }

    internal static CliCommandBinding<LibrarySyncRequest, LibrarySyncResult> Close(LibrarySyncSymbols symbols, LibrarySyncBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        return new CliCommandBinding<LibrarySyncRequest, LibrarySyncResult>(symbols.Command,
            new CliCommandBindingComponents<LibrarySyncRequest, LibrarySyncResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = (parse, invocation) => LibrarySyncRequestBinder.Bind(parse, invocation, symbols),
                InvalidResultFactory = LibrarySyncRequestBinder.CreateInvalid,
                Operation = components.Operation.ExecuteAsync,
                Renderers = components.Renderers,
            });
    }
}
