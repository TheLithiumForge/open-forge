using System.CommandLine;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Binding;
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
        var dryRun = new Option<bool>(LibraryDefinitions.DryRun.Name)
        {
            Description = LibraryDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };
        command.Options.Add(dryRun);
        group.Subcommands.Add(command);
        return new LibraryAttachSymbols
        {
            Command = command,
            LibraryId = libraryId,
            SourceRoot = sourceRoot,
            DryRun = dryRun,
        };
    }

    internal static CliCommandBinding<LibraryAttachRequest, LibraryAttachResult> Close(LibraryAttachSymbols symbols, LibraryAttachBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        return new CliCommandBinding<LibraryAttachRequest, LibraryAttachResult>(symbols.Command,
            new CliCommandBindingComponents<LibraryAttachRequest, LibraryAttachResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = (parse, invocation) => LibraryAttachRequestBinder.Bind(parse, invocation, symbols),
                InvalidResultFactory = LibraryAttachRequestBinder.CreateInvalid,
                Operation = LibraryAttachOperation.ExecuteAsync,
                Renderers = components.Renderers,
            });
    }
}
