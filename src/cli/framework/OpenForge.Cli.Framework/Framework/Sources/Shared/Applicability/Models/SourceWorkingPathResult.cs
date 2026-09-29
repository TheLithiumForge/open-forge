namespace OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

internal sealed class SourceWorkingPathResult(
    IReadOnlyList<string> paths,
    IReadOnlyList<string> invalidPaths)
{
    internal IReadOnlyList<string> Paths { get; } = paths;

    internal IReadOnlyList<string> InvalidPaths { get; } = invalidPaths;
}
