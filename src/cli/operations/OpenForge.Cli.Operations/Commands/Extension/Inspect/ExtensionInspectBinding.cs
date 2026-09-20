using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect;

internal static class ExtensionInspectBinding
{
    internal static ExtensionInspectSymbols CreateSymbols(Command extensionGroup)
    {
        var command = new Command(
            ExtensionInspectDefinitions.InspectCommand.Name,
            ExtensionInspectDefinitions.InspectCommand.Description);
        var stableId = new Argument<string?>("stable-id")
        {
            Description = "One exact lowercase Extension stable ID.",
            // The shared terminal pipeline must be able to render leaf help
            // before domain binding. Domain invocation still rejects a missing
            // value in ExtensionInspectRequestBinder.
            Arity = ArgumentArity.ZeroOrOne,
        };
        var source = new Option<string?>(ExtensionInspectDefinitions.Source.Name)
        {
            Description = ExtensionInspectDefinitions.Source.Description,
            HelpName = ExtensionInspectDefinitions.Source.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
        };
        command.Arguments.Add(stableId);
        command.Options.Add(source);
        extensionGroup.Add(command);
        return new(command, stableId, source);
    }

    internal static CliRequestBinding<ExtensionInspectRequest, ExtensionInspectResult> CreateRequestBinding(
        ExtensionInspectSymbols symbols,
        ExtensionInspectBindingComponents components)
    {
        var binder = new ExtensionInspectRequestBinder(symbols);
        var invalidFactory = new ExtensionInspectWorkspaceResultFactory(symbols);
        return new CliRequestBinding<ExtensionInspectRequest, ExtensionInspectResult>
        {
            Command = symbols.InspectCommand,
            Help = components.Help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = binder.Bind,
            InvalidResultFactory = invalidFactory.Create,
            Operation = components.Operation.ExecuteAsync,
        };
    }
}
