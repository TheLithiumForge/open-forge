namespace OpenForge.Cli.OutputText.Tool.Status;

internal static class ToolStatusText
{
    // @OpenForgeText tool.status.headline
    internal static string Headline() => "Tool readiness.";
    // @OpenForgeText tool.status.finding
    internal static string Finding() => "Check the tool facts.";
    // @OpenForgeText tool.status.tool-row
    internal static string ToolRow(string id, string name, string? range, string? state)
        => $"{id}  {name}  {range ?? "undeclared"}  {state ?? "catalogue"}";
    // @OpenForgeText tool.status.bin
    internal static string Bin(string? path, string? readiness) => $"Bin: {path ?? "unavailable"} ({readiness ?? "unavailable"})";
    // @OpenForgeText tool.status.manager-bin
    internal static string ManagerBin(string? path, string? readiness) => $"Manager global bin: {path ?? "unavailable"} ({readiness ?? "unavailable"})";
    // @OpenForgeText tool.status.version
    internal static string Version(string? locked, string? detected) => $"Version: locked {locked ?? "missing"}, detected {detected ?? "unavailable"}";
    // @OpenForgeText tool.status.tool-path
    internal static string ToolPath(string? path) => $"Tool path: {path ?? "unavailable"}";
    // @OpenForgeText tool.status.runtime-path
    internal static string RuntimePath(string? path) => $"Runtime path: {path ?? "unavailable"}";
    // @OpenForgeText tool.status.machine
    internal static string Machine(string? machine, string? native) => $"Machine records: {machine ?? "unavailable"}, native settings {native ?? "unavailable"}";
    // @OpenForgeText tool.status.stored-change
    internal static string StoredChange(string id, string disposition, string? current, string? desired, string? conflict)
        => $"Stored setting {id}: {current ?? "unavailable"} -> {desired ?? "unavailable"}, {disposition}{(conflict is null ? string.Empty : ", " + conflict)}";
    // @OpenForgeText tool.status.native-override
    internal static string Override(string variable, string effective) => $"  Effective native value from {variable}: {effective}";
    // @OpenForgeText tool.status.native-override-state
    private static string OverrideState(string state) => $"  Native environment override: {state}";
    // @OpenForgeText tool.status.retired-addition
    internal static string RetiredAddition(string id, string disposition, string? conflict)
        => $"Retired addition {id}: {disposition}{(conflict is null ? string.Empty : ", " + conflict)}";
    // @OpenForgeText tool.status.native-setting
    internal static string NativeSetting(string id, string home, string? previous, string? applied, IReadOnlyList<string> manual)
        => $"{id} in {home}: previous stored value {previous}, applied stored value {applied}. Manual setting change: {string.Join(" ", manual)}";
    // @OpenForgeText tool.status.native-override-unavailable
    internal static string OverrideUnavailable() => OverrideState("unchecked or unavailable");
    // @OpenForgeText tool.status.native-override-none
    internal static string OverrideNone() => OverrideState("none");
}
