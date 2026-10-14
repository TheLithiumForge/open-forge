namespace OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;

internal sealed record CliRequestReplay(string Command, string SubjectCommand, string RequestCommand, string DebugCommand, string AutomaticCommand)
{
    internal string OptionArguments { get; init; } = string.Empty;
    internal bool WorkspaceSpecified { get; init; }
    internal string? PrunePreviewCommand { get; init; }

    internal string? ApplyCommand { get; init; }
    internal string? AutomaticApplyCommand { get; init; }

    internal string? InteractiveCommand { get; init; }
}
