namespace OpenForge.Cli.Core.Presentation.Shared.Models;

internal sealed record CliEffect
{
    public required string Path { get; init; }
    public required CliEffectKind Kind { get; init; }
    public required CliEffectAction Action { get; init; }
    public required CliEffectOutcome Outcome { get; init; }
    public string? Reason { get; init; }
    public string? Owner { get; init; }
    public string? Before { get; init; }
    public string? After { get; init; }
}

internal enum CliEffectKind { File, Directory, Section, Link, Record, Setting }
internal enum CliEffectAction { Created, Replaced, Restored, Deleted, Kept, Rewritten, Detached, Released }
internal enum CliEffectOutcome { Planned, Done, NotStarted, Unknown, Failed }
