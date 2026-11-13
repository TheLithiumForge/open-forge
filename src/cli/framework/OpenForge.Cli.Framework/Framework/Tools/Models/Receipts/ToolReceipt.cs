using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Receipts;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolReceipt
{
    public required string Id { get; init; }
    public required string Version { get; init; }
    public required string Target { get; init; }
    public required string ManifestSha256 { get; init; }
    public required byte[] ManifestBytes { get; init; }
    public required string ManifestFormat { get; init; }
    public required int AdapterRevision { get; init; }
    public required string CodeRoot { get; init; }
    public required string[] Hosts { get; init; }
    public required ToolStoreReference[] Bindings { get; init; }
    public required ToolFileReceipt[] Files { get; init; }
    public required ToolMemberReceipt[] Members { get; init; }
    public required ToolDirectoryReceipt[] RuntimeDirectories { get; init; }
    public required Dictionary<string, string> NativeProjects { get; init; }
}
