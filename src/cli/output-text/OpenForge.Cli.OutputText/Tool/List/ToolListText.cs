namespace OpenForge.Cli.OutputText.Tool.List;

internal static class ToolListText
{
    // @OpenForgeText tool.list.headline
    internal static string Headline() => "Catalogue tools.";
    // @OpenForgeText tool.list.finding
    internal static string Finding() => "Check the tool facts.";
    // @OpenForgeText tool.list.tool-row
    internal static string ToolRow(string id, string name, string? range, string? state)
        => $"{id}  {name}  {range ?? "undeclared"}  {state ?? "catalogue"}";
    // @OpenForgeText tool.list.origin
    internal static string Origin(string value) => $"Origin: {value}";
    // @OpenForgeText tool.list.upstream
    internal static string Upstream(string value) => $"Upstream: {value}";
    // @OpenForgeText tool.list.licence
    internal static string Licence(string value) => $"Licence: {value}";
    // @OpenForgeText tool.list.channels
    internal static string Channels(IEnumerable<string> values) => $"Channels: {string.Join(", ", values)}";
    // @OpenForgeText tool.list.profiles
    internal static string Profiles(IEnumerable<string> values) => $"Supported profiles: {string.Join(", ", values)}";
}
