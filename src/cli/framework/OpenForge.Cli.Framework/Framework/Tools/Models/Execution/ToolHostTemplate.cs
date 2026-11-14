using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Execution;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolHostTemplate
{
    internal const string ClaudeFile = ".mcp.json";
    internal const string CodexFile = ".codex/config.toml";
    internal const string ClaudeSelectorPrefix = "/mcpServers/";
    internal const string CodexSelectorPrefix = "mcp_servers.";

    public required string Path { get; init; }

    public required string Selector { get; init; }
}
