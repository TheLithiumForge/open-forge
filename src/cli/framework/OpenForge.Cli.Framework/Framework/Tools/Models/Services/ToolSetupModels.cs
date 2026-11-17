using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal sealed record ToolSetupRequest
{
    public required ToolSetupSpec Setup { get; init; }
    public required ToolCommandTarget Invocation { get; init; }
    public ToolCommandTarget? InstalledInvocation { get; init; }
    public required ToolRecipeContext Context { get; init; }
}

internal enum ToolNativePreviewKind { Description, Plan, DryRun }

internal sealed record ToolResolvedNativeOutput(
    string PathPattern,
    ToolEffectScope Scope,
    ToolNativeOutputMatch Match,
    ToolNativeOutputPurpose Purpose,
    string? ClientId);

internal sealed record ToolSetupPlan
{
    public required ToolSetupRequest Request { get; init; }
    public required ToolProcessRequest Command { get; init; }
    public required ToolNativePreviewKind PreviewKind { get; init; }
    public IReadOnlyList<string>? DetectedClients { get; init; }
    public required IReadOnlyList<ToolResolvedNativeOutput> PossibleOutputs { get; init; }
    public required IReadOnlyDictionary<string, string?> ExpectedFileSha256 { get; init; }
    public required IReadOnlyList<string> Problems { get; init; }
}

internal sealed record ToolSetupResult
{
    public ToolProcessResult? Process { get; init; }
    public required bool Verified { get; init; }
    public required IReadOnlyList<string> Unfinished { get; init; }
}

internal sealed record ToolNativePlanObservation
{
    public required IReadOnlyList<string> DetectedClients { get; init; }
    public required IReadOnlyList<string> Paths { get; init; }
    public required IReadOnlyList<string> Problems { get; init; }
}
