using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Serialization;
using System.Text;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

internal static class ToolReceiptValidator
{
    internal static void ValidateJson(ReadOnlyMemory<byte> bytes)
    {
        using var json = JsonDocument.Parse(bytes);
        foreach (var name in new[] { "tools", "recovery" })
        {
            ToolManifestValidator.Require(json.RootElement.EnumerateObject().Count(property => property.NameEquals(name)) <= 1, "duplicate lock section");
            if (json.RootElement.TryGetProperty(name, out var value))
                JsonDuplicatePropertyValidator.ValidateNoDuplicateProperties(Encoding.UTF8.GetBytes(value.GetRawText()));
        }
    }

    internal static void ValidateRecovery(IReadOnlyList<ToolRecoveryRecord> records)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var record in records)
            ToolManifestValidator.Require(record is not null && ToolIdentity.IsId(record.Id) && ids.Add(record.Id)
                && ToolIdentity.IsRelativePath(record.Location) && ToolIdentity.IsSha256(record.Digest)
                && !string.IsNullOrWhiteSpace(record.State), "invalid recovery record");
    }

    internal static void Validate(IReadOnlyDictionary<string, ToolReceipt> receipts)
    {
        foreach (var (id, receipt) in receipts)
        {
            ToolManifestValidator.Require(receipt is not null && ToolIdentity.IsToolId(id) && receipt.Id == id
                && ToolIdentity.IsVersion(receipt.Version) && ToolVocabulary.Targets.Contains(receipt.Target), "invalid tool receipt identity");
            ToolManifestValidator.Require(receipt.ManifestBytes is not null && ToolIdentity.IsSha256(receipt.ManifestSha256)
                && ToolCatalogueReader.Hash(receipt.ManifestBytes) == receipt.ManifestSha256, "invalid manifest receipt digest");
            ToolManifestValidator.Require(receipt.ManifestFormat is "json" or "yaml", "unknown manifest snapshot format");
            var definition = new ToolCatalogueReader().Definition(receipt.ManifestBytes, $"snapshot.{receipt.ManifestFormat}", true);
            ToolManifestValidator.Require(definition.Manifest.Id == id && definition.Manifest.Version == receipt.Version
                && definition.Manifest.Adapter.Revision == receipt.AdapterRevision, "manifest and tool receipt identities disagree");
            ToolManifestValidator.Require(receipt.CodeRoot == "." || ToolIdentity.IsRelativePath(receipt.CodeRoot), "invalid recorded code root");
            ToolManifestValidator.Distinct(receipt.Hosts, "receipt hosts");
            ToolManifestValidator.Require(receipt.Hosts.All(ToolVocabulary.Hosts.Contains), "invalid receipt host");
            ToolManifestValidator.Require(receipt.Bindings is not null && receipt.Files is not null && receipt.Members is not null
                && receipt.RuntimeDirectories is not null && receipt.NativeProjects is not null, "tool receipt coverage cannot be null");
            var bindings = new HashSet<string>(StringComparer.Ordinal);
            foreach (var binding in receipt.Bindings)
                ToolManifestValidator.Require(binding is not null && ToolIdentity.IsId(binding.Id) && bindings.Add(binding.Id)
                    && binding.Kind is "install" or "archive" or "runtime-state" or "native-cache"
                    && ToolIdentity.IsSha256(binding.IdentitySha256), "invalid store binding reference");
            foreach (var file in receipt.Files)
                ToolManifestValidator.Require(file is not null && (file.StoreId is null || bindings.Contains(file.StoreId))
                    && ToolIdentity.IsRelativePath(file.Path) && ToolIdentity.IsSha256(file.Sha256), "invalid written file receipt");
            foreach (var member in receipt.Members)
                ToolManifestValidator.Require(member is not null && ToolIdentity.IsRelativePath(member.Path)
                    && member.Format is ToolVocabulary.JsonMembers or ToolVocabulary.TomlTable or "native-settings" or "native-collections"
                    && !string.IsNullOrWhiteSpace(member.Selector) && member.Value.ValueKind != JsonValueKind.Undefined, "invalid written member receipt");
            foreach (var directory in receipt.RuntimeDirectories)
            {
                ToolManifestValidator.Require(directory is not null && bindings.Contains(directory.StoreId)
                    && (directory.Path == "." || ToolIdentity.IsRelativePath(directory.Path)), "invalid runtime directory receipt");
                ToolManifestValidator.Distinct(directory.Files, "runtime files");
                ToolManifestValidator.Distinct(directory.Directories, "runtime directories");
                ToolManifestValidator.Require(directory.Files.Concat(directory.Directories).All(ToolIdentity.IsRelativePath), "invalid runtime member path");
            }
        }
    }
}
