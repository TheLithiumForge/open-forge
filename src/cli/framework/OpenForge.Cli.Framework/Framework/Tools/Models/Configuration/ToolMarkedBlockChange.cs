namespace OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

internal sealed record ToolMarkedBlockChange : ToolConfigChange
{
    public required string PathTemplate { get; init; }
    public required string StartMarker { get; init; }
    public required string EndMarker { get; init; }
    public required string ContentTemplate { get; init; }
}
