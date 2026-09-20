using System.CommandLine;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Binding;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Library.Attach;

internal static class LibraryAttachBinding
{
    internal static LibraryAttachSymbols CreateSymbols(Command group)
    {
        ArgumentNullException.ThrowIfNull(group);
        var command = new Command(LibraryAttachDefinitions.Command.Name, LibraryAttachDefinitions.Command.Description);
        var libraryId = new Argument<string?>(LibraryDefinitions.LibraryId.Name)
        {
            Description = LibraryDefinitions.LibraryId.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        command.Arguments.Add(libraryId);
        var sourceRoot = new Argument<string?>(LibraryDefinitions.SourceRoot.Name)
        {
            Description = LibraryDefinitions.SourceRoot.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        command.Arguments.Add(sourceRoot);
        var destinationRoot = new Option<string?>(LibraryDefinitions.DestinationRoot.Name)
        {
            Description = LibraryDefinitions.DestinationRoot.Description,
            HelpName = LibraryDefinitions.DestinationRoot.ValueName,
            Arity = ArgumentArity.ExactlyOne,
        };
        command.Options.Add(destinationRoot);
        var dryRun = new Option<bool>(LibraryDefinitions.DryRun.Name)
        {
            Description = LibraryDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };
        command.Options.Add(dryRun);
        var automatic = new Option<bool>(LibraryAttachDefinitions.Automatic.Name)
        {
            Description = LibraryAttachDefinitions.Automatic.Description,
            Arity = ArgumentArity.Zero,
        };
        command.Options.Add(automatic);
        var allow = new Option<string[]>(PermissionOptions.AllowPath.Name)
        {
            Description = PermissionOptions.AllowPath.Description,
            HelpName = PermissionOptions.AllowPath.ValueName,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        command.Options.Add(allow);
        group.Subcommands.Add(command);
        return new LibraryAttachSymbols
        {
            Command = command,
            LibraryId = libraryId,
            SourceRoot = sourceRoot,
            DestinationRoot = destinationRoot,
            DryRun = dryRun,
            Automatic = automatic,
            Allow = allow,
        };
    }

    internal static CliRequestBinding<LibraryAttachRequest, LibraryAttachResult> CreateRequestBinding(LibraryAttachSymbols symbols, LibraryAttachBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        return new CliRequestBinding<LibraryAttachRequest, LibraryAttachResult>
        {
            Command = symbols.Command,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => LibraryAttachRequestBinder.Bind(parse, invocation, symbols),
            InvalidResultFactory = LibraryAttachRequestBinder.CreateInvalid,
            Operation = components.Operation.ExecuteAsync,
        };
    }
}
