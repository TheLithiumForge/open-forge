namespace OpenForge.Cli.Core.Presentation.Shared.Prompts.Models;

internal sealed record CliSelectionRow(string Label, string Mark = "", bool Disabled = false);

internal sealed record CliSelectionFrame
{
    public required string Question { get; init; }
    public required IReadOnlyList<CliSelectionRow> Rows { get; init; }
    public required int Focus { get; init; }
    public required string Position { get; init; }
    public required IReadOnlyList<string> Controls { get; init; }
    public IReadOnlyList<string> Context { get; init; } = [];
    public IReadOnlyList<string> Details { get; init; } = [];
    public string? Notice { get; init; }
}
