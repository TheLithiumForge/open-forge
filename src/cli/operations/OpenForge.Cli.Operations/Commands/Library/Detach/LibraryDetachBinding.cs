using OpenForge.Cli.Core.Commands.Shared;
using System.CommandLine;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Library.Detach;

internal static class LibraryDetachBinding
{
    internal static LibraryDetachSymbols CreateSymbols(Command group)
    {
        ArgumentNullException.ThrowIfNull(group);
        var command = new Command(LibraryDetachDefinitions.Command.Name, LibraryDetachDefinitions.Command.Description);
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
        var automatic = new Option<bool>(LibraryDetachDefinitions.Automatic.Name)
        {
            Description = LibraryDetachDefinitions.Automatic.Description,
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
        return new LibraryDetachSymbols
        {
            Command = command,
            LibraryId = libraryId,
            DryRun = dryRun,
            Automatic = automatic,
            Allow = allowPath,
        };
    }

    internal static CliRequestBinding<LibraryDetachRequest, LibraryDetachResult> CreateRequestBinding(LibraryDetachSymbols symbols, LibraryDetachBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        return new CliRequestBinding<LibraryDetachRequest, LibraryDetachResult>
        {
            Command = symbols.Command,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => LibraryDetachRequestBinder.Bind(parse, invocation, symbols),
            InvalidResultFactory = LibraryDetachRequestBinder.CreateInvalid,
            Operation = components.Operation.ExecuteAsync,
        };
    }
}
