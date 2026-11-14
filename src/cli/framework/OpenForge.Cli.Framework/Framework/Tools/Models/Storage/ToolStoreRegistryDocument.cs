using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Storage;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolStoreRegistryDocument
{
    public int SchemaVersion { get; init; } = ToolStoreRegistryDefinitions.SchemaVersion;
    public required Dictionary<string, string> Roots { get; init; }
    public required Dictionary<string, ToolStoredUnit> Units { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolStoredUnit
{
    public required string Kind { get; init; }
    public required bool Verified { get; init; }
    public required string Path { get; init; }
    public required ToolStoreBinding Binding { get; init; }
    public required string RecipeSha256 { get; init; }
    public required ToolFileReceipt[] Files { get; init; }
    public required string[] Directories { get; init; }
    public required Dictionary<string, ToolLocalRegistration> Workspaces { get; init; }
    public required string[] Dependencies { get; init; }
    public required string[] Dependents { get; init; }
    public string? StagingPath { get; init; }
    public bool PreviouslyVerified { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolLocalRegistration
{
    public required string PhysicalWorkspace { get; init; }
    public required string CodeRoot { get; init; }
    public string? ProcessConsentSha256 { get; init; }
}

internal static class ToolStoreRegistryDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string File = "registry.json";
    internal const string Lock = "store.lock";
    internal const string Install = "install";
    internal const string Archive = "archive";
    internal const string Runtime = "runtime";
    internal const string State = "state";
    internal const string StagingDirectory = ".staging";
}
