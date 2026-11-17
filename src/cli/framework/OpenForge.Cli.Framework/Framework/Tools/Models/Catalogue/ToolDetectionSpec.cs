namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

internal enum ToolVersionParser { StableSemVer }

internal sealed record ToolDetectionSpec
{
    public required string Command { get; init; }
    public required IReadOnlyList<string> Arguments { get; init; }
    public required TimeSpan Timeout { get; init; }
    public required int MaximumOutputBytes { get; init; }
    public required IReadOnlyDictionary<string, string?> Environment { get; init; }
    public required ToolVersionParser Parser { get; init; }
}
