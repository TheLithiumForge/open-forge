namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Effects;

internal sealed record LibraryGitIgnoreEffectView
{
    public required string Path { get; init; }
    public required string Action { get; init; }
    public required string[] Paths { get; init; }
    public required LibraryExpectedState Expected { get; init; }
}
