using OpenForge.Cli.Core.Commands.Shared;
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
        var automatic = new Option<bool>(LibrarySyncDefinitions.Automatic.Name)
        {
            Description = LibrarySyncDefinitions.Automatic.Description,
            Arity = ArgumentArity.Zero,
        };
        var allowPath = new Option<string[]>(PermissionOptions.AllowPath.Name)
        {
            Description = PermissionOptions.AllowPath.Description,
            HelpName = PermissionOptions.AllowPath.ValueName,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        command.Options.Add(dryRun);
        command.Options.Add(automatic);
        command.Options.Add(allowPath);
        group.Subcommands.Add(command);
        return new LibrarySyncSymbols
        {
            Command = command,
            LibraryId = libraryId,
            DryRun = dryRun,
            Automatic = automatic,
            Allow = allowPath,
        };
    }

    internal static CliRequestBinding<LibrarySyncRequest, LibrarySyncResult> CreateRequestBinding(LibrarySyncSymbols symbols, LibrarySyncBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        return new CliRequestBinding<LibrarySyncRequest, LibrarySyncResult>
        {
            Command = symbols.Command,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => LibrarySyncRequestBinder.Bind(parse, invocation, symbols),
            InvalidResultFactory = LibrarySyncRequestBinder.CreateInvalid,
            Operation = components.Operation.ExecuteAsync,
        };
    }
}
