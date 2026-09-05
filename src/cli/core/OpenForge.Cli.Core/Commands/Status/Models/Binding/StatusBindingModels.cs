using System.CommandLine;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Status.Models.Binding;

internal sealed record StatusSymbols(Command StatusCommand)
{
    internal static StatusSymbols Create()
        => new(new Command(
            StatusDefinitions.StatusCommand.Name,
            StatusDefinitions.StatusCommand.Description));
}

internal sealed class StatusBindingComponents
{
    public required CliHelpContent Help { get; init; }

    public required StatusOperation Operation { get; init; }

    public required CliRendererSet<StatusResult> Renderers { get; init; }

    public CliDiagnosticRenderer<StatusResult>? DiagnosticRenderer { get; init; }
}
