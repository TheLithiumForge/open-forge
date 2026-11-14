namespace OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

internal enum ToolNativeStoredValueParser { CodeGraphTelemetryV1 }

internal sealed record ToolNativeStoredValueSpec
{
    public required string PathTemplate { get; init; }
    public required ToolNativeStoredValueParser Parser { get; init; }
    public required string MissingValue { get; init; }
    public required int MaximumBytes { get; init; }
}

internal static class ToolNativeConfigDefinitions
{
    internal const string CodeGraphTelemetryAllowListId = "codegraph-telemetry";
    internal const string CodeGraphTelemetryStatusParserId = "codegraph-telemetry-status-v1";
    internal const string TelemetryStatusPrefix = "Telemetry: ";
    internal const string EnabledStatus = "enabled";
    internal const string DisabledStatus = "disabled";
    internal const string SavedChoiceReason = "your saved choice";
    internal const string DefaultReason = "default";
    internal const string DoNotTrackVariable = "DO_NOT_TRACK";
    internal const string CodeGraphTelemetryVariable = "CODEGRAPH_TELEMETRY";
    internal const string CodeGraphNoDownloadVariable = "CODEGRAPH_NO_DOWNLOAD";
    internal const string EnabledValue = "on";
    internal const string DisabledValue = "off";
}

internal sealed record ToolNativeConfigChange : ToolConfigChange
{
    public required string AllowListId { get; init; }
    public required IReadOnlyList<string> ApplyArguments { get; init; }
    public IReadOnlyList<string>? ReadArguments { get; init; }
    public IReadOnlyDictionary<string, string?>? ReadEnvironment { get; init; }
    public string? ValueParserId { get; init; }
    public TimeSpan? ReadTimeout { get; init; }
    public int? ReadMaximumOutputBytes { get; init; }
    public ToolNativeStoredValueSpec? StoredValue { get; init; }
    public required string DesiredValue { get; init; }
    public required IReadOnlyList<string> ManualRestoreArgumentsTemplate { get; init; }
}
