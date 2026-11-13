namespace OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

internal sealed record ToolNativeConfigChange : ToolConfigChange
{
    public required string AllowListId { get; init; }
    public required IReadOnlyList<string> ApplyArguments { get; init; }
    public required IReadOnlyList<string> ReadArguments { get; init; }
    public required string ValueParserId { get; init; }
    public required string DesiredValue { get; init; }
    public required IReadOnlyList<string> RestoreArgumentsTemplate { get; init; }
}
