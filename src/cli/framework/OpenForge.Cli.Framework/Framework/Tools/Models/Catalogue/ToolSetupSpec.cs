namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

internal enum ToolEffectScope { Workspace, Account }
internal enum ToolDeclaredEffectKind { Configuration, Telemetry, PathChange, SharedFileDeletion }

internal sealed record ToolDeclaredEffect
{
    public required string Id { get; init; }
    public required ToolEffectScope Scope { get; init; }
    public required ToolDeclaredEffectKind Kind { get; init; }
    public required string Description { get; init; }
    public string? PathTemplate { get; init; }
    public string? Selector { get; init; }
}

internal sealed record ToolSetupSpec
{
    public required IReadOnlyList<string> Arguments { get; init; }
    public IReadOnlyList<string>? DryRunArguments { get; init; }
    public required IReadOnlyDictionary<string, string?> Environment { get; init; }
    public required IReadOnlyList<ToolDeclaredEffect> Effects { get; init; }
    public required TimeSpan Timeout { get; init; }
}
