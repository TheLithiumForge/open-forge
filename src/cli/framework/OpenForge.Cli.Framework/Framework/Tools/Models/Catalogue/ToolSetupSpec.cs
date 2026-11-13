namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

internal enum ToolEffectScope { Workspace, Account }

internal sealed record ToolDeclaredEffect
{
    public required string Id { get; init; }
    public required ToolEffectScope Scope { get; init; }
    public required string Description { get; init; }
    public string? PathTemplate { get; init; }
    public string? Selector { get; init; }
}

internal sealed record ToolSetupSpec
{
    public required IReadOnlyList<string> Arguments { get; init; }
    public required IReadOnlyList<ToolDeclaredEffect> Effects { get; init; }
    public required TimeSpan Timeout { get; init; }
}
