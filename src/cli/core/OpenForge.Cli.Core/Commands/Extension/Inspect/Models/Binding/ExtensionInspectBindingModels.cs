using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Binding;

internal sealed record ExtensionInspectSymbols(
    Command InspectCommand,
    Argument<string?> StableId,
    Option<string?> Source)
{
    internal static ExtensionInspectSymbols Create(Command extensionGroup)
        => ExtensionInspectSymbolFactory.Create(extensionGroup);
}

internal static class ExtensionInspectSymbolFactory
{
    internal static ExtensionInspectSymbols Create(Command extensionGroup)
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
}

internal sealed class ExtensionInspectBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required ExtensionInspectOperation Operation { get; init; }

    public required CliRendererSet<ExtensionInspectResult> Renderers { get; init; }

    public CliDiagnosticRenderer<ExtensionInspectResult>? DiagnosticRenderer { get; init; }
}
