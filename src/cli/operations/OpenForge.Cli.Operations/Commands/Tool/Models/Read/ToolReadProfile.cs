using System.Text.Json;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Read;

internal sealed record ToolReadProfile
{
    public required string Id { get; init; }
    public required string Range { get; init; }
    public required string MinimumToolVersion { get; init; }
    public required string MinimumOpenForgeVersion { get; init; }
    public required IReadOnlyList<string> TestedVersions { get; init; }
    public string? NativeHomePolicy { get; init; }
    public required ToolReadCommand Setup { get; init; }
    public required ToolReadCommand Uninstall { get; init; }
    public required IReadOnlyList<ToolReadConfig> Additions { get; init; }
}
internal sealed record ToolReadCommand(IReadOnlyList<string> Arguments, IReadOnlyList<string>? DryRunArguments, IReadOnlyDictionary<string, string?> Environment, double TimeoutSeconds, IReadOnlyList<ToolReadEffect> Effects)
{
    public int MaximumOutputBytes { get; init; }
    public ToolReadNativePlan? NativePlan { get; init; }
    public IReadOnlyList<ToolReadNativeOutput> PossibleOutputs { get; init; } = [];
}
internal sealed record ToolReadNativePlan(IReadOnlyList<string> Arguments, string Parser, double TimeoutSeconds, int MaximumOutputBytes);
internal sealed record ToolReadNativeOutput(string PathTemplate, string Scope, string Match, string Purpose, string? OperatingSystem, string? ClientId);
internal sealed record ToolReadStoredValue(string PathTemplate, string Parser, string MissingValue, int MaximumBytes);
internal sealed record ToolReadEffect(string Id, string Scope, string Kind, string Description, string? PathTemplate, string? Selector);
internal sealed record ToolReadConfig
{
    public required string Id { get; init; }
    public required string Kind { get; init; }
    public required string Scope { get; init; }
    public required string Description { get; init; }
    public string? InitialState { get; init; }
    public string? WhenNotIgnored { get; init; }
    public string? PathTemplate { get; init; }
    public string? Pointer { get; init; }
    public JsonElement? Value { get; init; }
    public string? StartMarker { get; init; }
    public string? EndMarker { get; init; }
    public string? ContentTemplate { get; init; }
    public string? TemplateResource { get; init; }
    public string? AllowListId { get; init; }
    public IReadOnlyList<string>? ApplyArguments { get; init; }
    public IReadOnlyList<string>? ReadArguments { get; init; }
    public IReadOnlyDictionary<string, string?>? ReadEnvironment { get; init; }
    public ToolReadStoredValue? StoredValue { get; init; }
    public string? ValueParserId { get; init; }
    public double? ReadTimeoutSeconds { get; init; }
    public int? ReadMaximumOutputBytes { get; init; }
    public string? DesiredValue { get; init; }
    public IReadOnlyList<string>? ManualRestoreArguments { get; init; }
}
internal sealed record ToolReadChange(string Id, string Disposition, string? Current, string? Desired, string? Conflict)
{
    public IReadOnlyDictionary<string, string>? NativeEnvironmentOverrides { get; init; }
}
