using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

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
    public required int MaximumOutputBytes { get; init; }
    public ToolNativePlanSpec? NativePlan { get; init; }
    public required IReadOnlyList<ToolNativeOutputSpec> PossibleOutputs { get; init; }
}

internal enum ToolNativePlanParser { CbmInstallPlanV1 }
internal enum ToolNativeOutputMatch { Exact, ExistingPattern }
internal enum ToolNativeOutputPurpose { Backup, ReportOnly }

internal sealed record ToolNativePlanSpec
{
    public required IReadOnlyList<string> Arguments { get; init; }
    public required ToolNativePlanParser Parser { get; init; }
    public required TimeSpan Timeout { get; init; }
    public required int MaximumOutputBytes { get; init; }
}

internal sealed record ToolNativeOutputSpec
{
    public required string PathTemplate { get; init; }
    public required ToolEffectScope Scope { get; init; }
    public required ToolNativeOutputMatch Match { get; init; }
    public required ToolNativeOutputPurpose Purpose { get; init; }
    public ToolOperatingSystem? OperatingSystem { get; init; }
    public string? ClientId { get; init; }
}
