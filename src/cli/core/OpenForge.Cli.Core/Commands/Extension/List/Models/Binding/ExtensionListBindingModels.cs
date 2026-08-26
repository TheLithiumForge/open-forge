using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.List.Models.Binding;

internal sealed record ExtensionListSymbols(
    Command ListCommand,
    Option<bool> Installed,
    Option<bool> Available,
    Option<string?> Source)
{
    internal static ExtensionListSymbols Create(Command extensionGroup)
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
}

internal sealed class ExtensionListBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required ExtensionListOperation Operation { get; init; }

    public required CliRendererSet<ExtensionListResult> Renderers { get; init; }

    public CliDiagnosticRenderer<ExtensionListResult>? DiagnosticRenderer { get; init; }
}
