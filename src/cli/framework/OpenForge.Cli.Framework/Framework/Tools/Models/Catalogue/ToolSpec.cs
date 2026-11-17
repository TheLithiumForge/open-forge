namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

internal enum ToolVersionSelectionKind { LatestSupported, MinorRange, Exact }

internal sealed record ToolVersionSelection
{
    public required ToolVersionSelectionKind Kind { get; init; }
    public ToolVersionRange? Range { get; init; }
    public string? ExactVersion { get; init; }
}

internal sealed record ToolSpec(string Id, ToolVersionSelection Version);

internal sealed record ToolConsent(bool Yes, bool CanPrompt);
