using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Extension.List;

internal static class ExtensionListBinding
{
    internal static ExtensionListSymbols CreateSymbols(Command extensionGroup)
    {
        var command = new Command(
            name: ExtensionListDefinitions.ListCommand.Name,
            description: ExtensionListDefinitions.ListCommand.Description);
        var installed = new Option<bool>(ExtensionListDefinitions.Installed.Name)
        {
            Description = ExtensionListDefinitions.Installed.Description,
            Arity = ArgumentArity.Zero,
        };
        var available = new Option<bool>(ExtensionListDefinitions.Available.Name)
        {
            Description = ExtensionListDefinitions.Available.Description,
            Arity = ArgumentArity.Zero,
        };
        var source = new Option<string?>(ExtensionListDefinitions.Source.Name)
        {
            Description = ExtensionListDefinitions.Source.Description,
            HelpName = ExtensionListDefinitions.Source.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
        };
        command.Options.Add(installed);
        command.Options.Add(available);
        command.Options.Add(source);
        extensionGroup.Add(command);
        return new(
            ListCommand: command,
            Installed: installed,
            Available: available,
            Source: source);
    }

    internal static CliRequestBinding<ExtensionListRequest, ExtensionListResult> CreateRequestBinding(
        ExtensionListSymbols symbols,
        ExtensionListBindingComponents components)
    {
        var binder = new ExtensionListRequestBinder(symbols);
        var invalidFactory = new ExtensionListWorkspaceResultFactory(symbols);
        return new CliRequestBinding<ExtensionListRequest, ExtensionListResult>
        {
            Command = symbols.ListCommand,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = binder.Bind,
            InvalidResultFactory = invalidFactory.Create,
            Operation = components.Operation.ExecuteAsync,
        };
    }
}
