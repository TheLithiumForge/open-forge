using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions.Embedded;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Serialization;

namespace OpenForge.Cli.IntegrationTests.Framework.Extensions;

public sealed class EmbeddedExtensionCatalogueIntegrationTests
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private const int InventoryArchiveOrder = 0;
    private const int PackageArchiveOrder = 1;
    private const string InventoryAssetPath = "inventory.json";
    private const string ManifestAssetPath = "extension.json";
    private const string PayloadAssetPrefix = "content/";

    [Fact(DisplayName = "Embedded Extension catalogue matches every authored package, asset, and hash"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    public void EmbeddedCatalogueMatchesAuthoredPackagesAssetsAndHashes()
    {
        var result = EmbeddedExtensionCatalogueReader.Read();

        Assert.Equal(ExtensionSourceReadState.Complete, result.State);
        Assert.Equal(ExtensionSourceKind.EmbeddedCatalogue, result.Kind);

        var authoredRoot = Path.Combine(AppContext.BaseDirectory, "ExtensionCatalogue");
        var authoredAssets = Directory
            .EnumerateFiles(authoredRoot, "*", SearchOption.AllDirectories)
            .ToDictionary(
                path => Path.GetRelativePath(authoredRoot, path).Replace('\\', '/'),
                File.ReadAllBytes,
                StringComparer.Ordinal);
        Assert.Equal(
            authoredAssets.Keys.Order(StringComparer.Ordinal),
            EmbeddedExtensionCatalogueAssets.All.Keys.Order(StringComparer.Ordinal));
        foreach (var (path, bytes) in authoredAssets)
        {
            Assert.True(EmbeddedExtensionCatalogueAssets.Read(path).Span.SequenceEqual(bytes));
        }

        var inventory = JsonSerializer.Deserialize(
            authoredAssets[InventoryAssetPath],
            ExtensionPackageJsonContext.Default.EmbeddedExtensionInventoryDocument);
        Assert.NotNull(inventory);
        Assert.Equal(inventory.Packages.Length, result.Packages.Count);
        foreach (var inventoryPackage in inventory.Packages)
        {
            var package = Assert.Single(
                result.Packages,
                candidate => string.Equals(candidate.Id, inventoryPackage.Id, StringComparison.Ordinal));
            var manifestAssetPath = $"{inventoryPackage.Id}/{ManifestAssetPath}";
            var manifest = JsonSerializer.Deserialize(
                authoredAssets[manifestAssetPath],
                ExtensionPackageJsonContext.Default.ExtensionManifestDocument);
            Assert.NotNull(manifest);

            Assert.Equal(inventoryPackage.Id, manifest.Id);
            Assert.Equal(manifest.Id, package.Id);
            Assert.Equal(manifest.Name, package.Name);
            Assert.Equal(manifest.Description, package.Description);
            Assert.Equal(manifest.Version, package.Version);
            Assert.Equal(manifest.Dependencies, package.Dependencies);
            Assert.Equal(ManifestAssetPath, package.ManifestPath);

            var payloadPaths = inventoryPackage.Assets
                .Select(asset => asset.Path)
                .Where(path => path.StartsWith(PayloadAssetPrefix, StringComparison.Ordinal))
                .ToArray();
            Assert.Equal(payloadPaths, package.Payload.Select(asset => asset.Path));
            Assert.Equal(payloadPaths.Length, package.PayloadFileCount);

            foreach (var asset in inventoryPackage.Assets)
            {
                var authoredAssetPath = $"{inventoryPackage.Id}/{asset.Path}";
                Assert.Equal(
                    asset.Sha256,
                    Convert.ToHexStringLower(SHA256.HashData(authoredAssets[authoredAssetPath])));
            }
        }
    }

    [Fact(DisplayName = "Embedded Extension archive decodes to the exact canonical authored asset stream"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    public void EmbeddedCatalogueArchiveDecodesToCanonicalAuthoredAssets()
    {
        var authoredRoot = Path.Combine(AppContext.BaseDirectory, "ExtensionCatalogue");
        var archiveLines = Directory
            .EnumerateFiles(authoredRoot, "*", SearchOption.AllDirectories)
            .Select(path => new
            {
                AssetPath = Path.GetRelativePath(authoredRoot, path).Replace('\\', '/'),
                Bytes = File.ReadAllBytes(path),
            })
            .OrderBy(asset => asset.AssetPath == InventoryAssetPath
                ? InventoryArchiveOrder
                : PackageArchiveOrder)
            .ThenBy(asset => asset.AssetPath, StringComparer.Ordinal)
            .Select(asset => $"{asset.AssetPath}\t{Convert.ToBase64String(asset.Bytes)}");
        var archiveText = $"{string.Join('\n', archiveLines)}\n";
        using var compressed = new MemoryStream(
            EmbeddedExtensionCatalogueAssets.Archive.ToArray(),
            writable: false);
        using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        var embeddedArchiveText = StrictUtf8.GetString(output.ToArray());

        Assert.Equal(archiveText, embeddedArchiveText);
    }
}
