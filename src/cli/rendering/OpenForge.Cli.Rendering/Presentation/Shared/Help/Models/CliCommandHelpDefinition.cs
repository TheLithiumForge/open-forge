using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Help.Models;

internal sealed record CliCommandHelpDefinition
{
    public required string Usage { get; init; }

    public required string Examples { get; init; }

    public required string Behavior { get; init; }

    public required IReadOnlyList<CliHelpSection> Details { get; init; }

    public required IReadOnlyList<(string Label, string? Description)> RelatedCommands { get; init; }
}
