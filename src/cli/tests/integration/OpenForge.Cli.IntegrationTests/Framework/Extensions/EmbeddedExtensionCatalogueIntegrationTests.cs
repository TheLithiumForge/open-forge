using System.Security.Cryptography;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions.Embedded;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.IntegrationTests.Framework.Extensions;

public sealed class EmbeddedExtensionCatalogueIntegrationTests
{
    [Fact(DisplayName = "Embedded Extension catalogue matches every authored package, asset, and hash"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    public void EmbeddedCatalogueMatchesAuthoredPackagesAssetsAndHashes()
    {
        var result = EmbeddedExtensionCatalogueReader.Read();
        Assert.Equal(ExtensionSourceReadState.Complete, result.State);
        Assert.Equal(ExtensionSourceKind.EmbeddedCatalogue, result.Kind);

        var authored = ReadAuthoredAssets();
        var embedded = EmbeddedExtensionCatalogueAssets.All;
        Assert.Equal(authored.Keys.Order(StringComparer.Ordinal), embedded.Keys.Order(StringComparer.Ordinal));
        foreach (var (path, bytes) in authored)
        {
            Assert.True(embedded[path].Span.SequenceEqual(bytes), path);
        }

        var manifests = authored.Where(asset => asset.Key.EndsWith("/extension.json", StringComparison.Ordinal)
            && asset.Key.Count(character => character == '/') == 1).ToArray();
        Assert.NotEmpty(manifests);
        Assert.Equal(manifests.Length, result.Packages.Count);
        Assert.Equal(result.Packages.Select(package => package.Id).Order(StringComparer.Ordinal),
            result.Packages.Select(package => package.Id));
        foreach (var manifestAsset in manifests)
        {
            using var document = JsonDocument.Parse(manifestAsset.Value);
            var manifest = document.RootElement;
            var id = manifest.GetProperty("id").GetString();
            var package = Assert.Single(result.Packages, candidate => candidate.Id == id);
            Assert.Equal(manifest.GetProperty("name").GetString(), package.Name);
            Assert.Equal(manifest.GetProperty("description").GetString(), package.Description);
            Assert.Equal(manifest.GetProperty("version").GetString(), package.Version);
            Assert.Equal(manifest.GetProperty("dependencies").EnumerateArray().Select(value => value.GetString()),
                package.Dependencies);
            Assert.All(package.Dependencies, dependency =>
                Assert.Contains(result.Packages, candidate => candidate.Id == dependency));
            Assert.Equal("extension.json", package.ManifestPath);

            var directory = manifestAsset.Key[..manifestAsset.Key.IndexOf('/')];
            var prefix = $"{directory}/content/";
            var payload = authored.Where(asset => asset.Key.StartsWith(prefix, StringComparison.Ordinal))
                .OrderBy(asset => asset.Key, StringComparer.Ordinal).ToArray();
            Assert.Equal(payload.Length, package.PayloadFileCount);
            Assert.Equal(payload.Select(asset => asset.Key[(directory.Length + 1)..]),
                package.Payload.Select(asset => asset.Path));
            foreach (var asset in payload)
            {
                var fact = Assert.Single(package.Payload, candidate =>
                    candidate.Path == asset.Key[(directory.Length + 1)..]);
                Assert.Equal(asset.Key[prefix.Length..], fact.TargetPath);
                Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(asset.Value)), fact.Sha256);
                Assert.Equal(asset.Value.Length, fact.ByteLength);
                Assert.Equal(asset.Value, fact.Bytes?.ToArray());
            }
        }
    }

    [Fact(DisplayName = "Core manifest resources contain the exact authored Extension asset set and bytes"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Integration")]
    public void ManifestResourcesContainExactAuthoredAssets()
    {
        const string prefix = "OpenForge.Extensions.Payload/";
        var assembly = typeof(EmbeddedExtensionCatalogueReader).Assembly;
        var resources = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(prefix, StringComparison.Ordinal))
            .ToDictionary(name => name[prefix.Length..], StringComparer.Ordinal);
        var authored = ReadAuthoredAssets();
        Assert.Equal(authored.Keys.Order(StringComparer.Ordinal), resources.Keys.Order(StringComparer.Ordinal));
        foreach (var (path, bytes) in authored)
        {
            using var stream = assembly.GetManifestResourceStream(resources[path]);
            Assert.NotNull(stream);
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            Assert.Equal(bytes, buffer.ToArray());
        }
    }

    private static Dictionary<string, byte[]> ReadAuthoredAssets()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "ExtensionCatalogue");
        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(root, path).Replace('\\', '/'),
                File.ReadAllBytes, StringComparer.Ordinal);
    }
}
