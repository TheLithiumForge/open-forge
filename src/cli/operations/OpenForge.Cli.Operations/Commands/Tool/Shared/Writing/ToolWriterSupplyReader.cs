using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing;

internal static class ToolWriterSupplyReader
{
    internal static async ValueTask<ToolWriterSupply> ReadAsync(ToolStoreRegistry registry, ToolDefinition definition, string target, CancellationToken token)
    {
        var manifest = definition.Manifest;
        if (manifest.Runtime.Kind == "locked-package")
            throw ToolWriterReader.Failure(ToolWriteIssueCode.PinNotSupported, CliSemanticStatus.Blocked, manifest.Id, "Private Node runtime preparation is awaiting a maintainer decision.");
        var store = await ToolWriterReader.StoreAsync(registry, definition, target, token).ConfigureAwait(false);
        var artifacts = new List<ToolWriterArtifact>();
        foreach (var id in manifest.Targets[target].Artifacts.Concat(manifest.Runtime.ExtraDownloads).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            var artifact = manifest.Artifacts[id];
            if (!ToolArtifactSource.IsOfficial(manifest, artifact) || artifact.Bytes is null)
                throw ToolWriterReader.Failure(ToolWriteIssueCode.PinNotSupported, CliSemanticStatus.Blocked, id, "A pinned official artifact URL and byte size are required.");
            var path = ToolArtifactIntegrity.Key(artifact.Integrity);
            var recipe = ToolCatalogueReader.Hash(Encoding.UTF8.GetBytes(string.Create(CultureInfo.InvariantCulture, $"{artifact.Bytes}\n{artifact.Integrity}\n")));
            store.Registry.Document.Units.TryGetValue(ToolStoreIdentity.UnitKey(ToolStoreRegistryDefinitions.Archive, path), out var cached);
            if (cached is not null)
            {
                if (cached.RecipeSha256 != recipe)
                    throw ToolWriterReader.Failure(ToolWriteIssueCode.StoreUnavailable, CliSemanticStatus.Incomplete, id, "The cached artifact belongs to another descriptor.");
                var members = ToolStoreUnitReader.Members(registry.Archives, cached.Path, token);
                if (members.Any(member => !cached.Files.Any(file => file.Path == member) && !cached.Directories.Contains(member, StringComparer.Ordinal)))
                    throw ToolWriterReader.Failure(ToolWriteIssueCode.StoreUnavailable, CliSemanticStatus.Incomplete, id, "The cached artifact contains unrecognized members.");
                if (!(await ToolStoreUnitReader.ReadAsync(registry.Archives, cached, token).ConfigureAwait(false)).Verified) cached = null;
            }
            artifacts.Add(new(id, artifact, ToolArtifactSource.Resolve(manifest, artifact), path, recipe, cached));
        }
        return new(definition, target, store, [.. artifacts]);
    }
}
