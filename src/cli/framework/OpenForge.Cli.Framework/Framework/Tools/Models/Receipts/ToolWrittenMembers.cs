using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Receipts;

internal sealed record ToolStoreBinding(string Id, string Kind, string PhysicalRoot, string IdentitySha256);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolStoreReference(string Id, string Kind, string IdentitySha256);
internal sealed record ToolFileReceipt(string? StoreId, string Path, string Sha256, bool Executable);
internal sealed record ToolMemberReceipt(string Path, string Format, string Selector, JsonElement Value, bool CreatedFile);
internal sealed record ToolDirectoryReceipt(string StoreId, string Path, string[] Files, string[] Directories);
