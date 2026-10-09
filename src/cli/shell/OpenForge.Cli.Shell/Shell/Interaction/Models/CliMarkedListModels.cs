namespace OpenForge.Cli.Core.Shell.Interaction.Models;

// The Shared CLI Operation Contract defines the marked list's complete answer and fixed mark order.
internal sealed record CliMark(string Symbol, string Legend);
internal sealed record CliMarkedRow<T>(T Value, string Label, string? Summary, int Mark, string? Lock) where T : notnull;
internal sealed record CliMarkedListQuestion<T>(
    string Question, IReadOnlyList<CliMark> Marks, IReadOnlyList<CliMarkedRow<T>> Rows, Func<T, int, string?> Details) where T : notnull;
internal sealed record CliMarkedValue<T>(T Value, int Mark) where T : notnull;
internal sealed record CliMarkedSelection<T>(IReadOnlyList<CliMarkedValue<T>> Rows) where T : notnull;
