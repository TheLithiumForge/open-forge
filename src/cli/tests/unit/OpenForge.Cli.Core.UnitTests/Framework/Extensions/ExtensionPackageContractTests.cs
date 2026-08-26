using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Embedded;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Extensions;

public sealed class ExtensionPackageContractTests
{
    [Theory(DisplayName = "Extension stable IDs accept only lowercase ASCII hyphen segments"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Unit")]
    [InlineData("development-toolkit", true)]
    [InlineData("a1", true)]
    [InlineData("Development-toolkit", false)]
    [InlineData("development_toolkit", false)]
    [InlineData("-development", false)]
    [InlineData("development-", false)]
    [InlineData("development--toolkit", false)]
    [InlineData("", false)]
    public void StableIdGrammarIsExact(string value, bool expected)
    {
        Assert.Equal(expected, ExtensionIdentity.IsValidStableId(value));
    }

    [Theory(DisplayName = "Extension target paths remain portable contained relative paths"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Unit")]
    [InlineData(".agents/workflows/design.md", true)]
    [InlineData("../escape.md", false)]
    [InlineData(".agents\\escape.md", false)]
    [InlineData("/absolute.md", false)]
    [InlineData(".agents//empty.md", false)]
    [InlineData(".agents/./same.md", false)]
    [InlineData(".agents/name.", false)]
    [InlineData(".agents/name ", false)]
    [InlineData(".agents/forbidden?.md", false)]
    [InlineData(".agents/forbidden<.md", false)]
    [InlineData(".agents/forbidden>.md", false)]
    [InlineData(".agents/forbidden:.md", false)]
    [InlineData(".agents/forbidden\".md", false)]
    [InlineData(".agents/forbidden|.md", false)]
    [InlineData(".agents/forbidden*.md", false)]
    [InlineData(".agents/control\n.md", false)]
    [InlineData("CON", false)]
    [InlineData("con.txt", false)]
    [InlineData("COM1", false)]
    [InlineData("LPT9.md", false)]
    [InlineData("CLOCK$", false)]
    [InlineData("CONIN$", false)]
    [InlineData("COM¹.txt", false)]
    [InlineData(".agents/café.md", true)]
    [InlineData(".agents/café.md", false)]
    public void TargetPathGrammarIsExact(string value, bool expected)
    {
        var actual = ExtensionTargetPath.TryNormalize(value, out var normalized);

        Assert.Equal(expected, actual);
        Assert.Equal(expected ? value : string.Empty, normalized);
    }

    [Fact(DisplayName = "Strict manifest accepts the complete current package shape"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Unit")]
    public void StrictManifestAcceptsCurrentShape()
    {
        var manifest = Read("""
            {
              "id": "toolkit",
              "name": "Toolkit",
              "description": "A toolkit.",
              "version": "1.0.0",
              "dependencies": ["base"]
            }
            """);

        Assert.Equal("toolkit", manifest.Id);
        Assert.Equal(["base"], manifest.Dependencies);
    }

    [Theory(DisplayName = "Strict manifest rejects unknown duplicate and ambiguous identity facts"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Unit")]
    [InlineData("unknown", "\"extra\": true,", "toolkit")]
    [InlineData("duplicate", "\"id\": \"other\",", "toolkit")]
    [InlineData("invalid-id", "", "Invalid_ID")]
    public void StrictManifestRejectsInvalidFacts(
        string scenario,
        string extra,
        string id = "toolkit")
    {
        var json = $$"""
            {
              {{extra}}
              "id": "{{id}}",
              "name": "Toolkit",
              "description": "A toolkit.",
              "version": "1.0.0",
              "dependencies": []
            }
            """;

        _ = scenario;
        Assert.ThrowsAny<Exception>(() => Read(json));
    }

    [Fact(DisplayName = "Embedded Extension catalogue verifies its complete authored inventory and hashes"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Unit")]
    public void EmbeddedCatalogueVerifiesInventoryAndHashes()
    {
        var result = EmbeddedExtensionCatalogueReader.Read();

        Assert.Equal(ExtensionSourceReadState.Complete, result.State);
        Assert.Equal(ExtensionSourceKind.EmbeddedCatalogue, result.Kind);
        var package = Assert.Single(result.Packages);
        Assert.Equal("development-toolkit", package.Id);
        Assert.Equal("0.1.0", package.Version);
        Assert.Equal(21, package.PayloadFileCount);
        Assert.Empty(package.Dependencies);

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

        using var inventory = JsonDocument.Parse(authoredAssets["inventory.json"]);
        var inventoryPackage = Assert.Single(inventory.RootElement.GetProperty("packages").EnumerateArray());
        foreach (var asset in inventoryPackage.GetProperty("assets").EnumerateArray())
        {
            var path = $"development-toolkit/{asset.GetProperty("path").GetString()}";
            Assert.Equal(
                asset.GetProperty("sha256").GetString(),
                Convert.ToHexStringLower(SHA256.HashData(authoredAssets[path])));
        }
    }

    [Fact(DisplayName = "Embedded Extension archive regenerates exactly from the authored asset set"), Trait("Feature", "extension-discovery"), Trait("Evidence", "Unit")]
    public void EmbeddedCatalogueArchiveRegeneratesExactly()
    {
        var authoredRoot = Path.Combine(AppContext.BaseDirectory, "ExtensionCatalogue");
        var archiveLines = Directory
            .EnumerateFiles(authoredRoot, "*", SearchOption.AllDirectories)
            .Select(path => new
            {
                AssetPath = Path.GetRelativePath(authoredRoot, path).Replace('\\', '/'),
                Bytes = File.ReadAllBytes(path),
            })
            .OrderBy(asset => asset.AssetPath == "inventory.json" ? 0 : 1)
            .ThenBy(asset => asset.AssetPath, StringComparer.Ordinal)
            .Select(asset => $"{asset.AssetPath}\t{Convert.ToBase64String(asset.Bytes)}");
        var archiveText = $"{string.Join('\n', archiveLines)}\n";
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            gzip.Write(Encoding.UTF8.GetBytes(archiveText));
        }

        Assert.True(EmbeddedExtensionCatalogueAssets.Archive.SequenceEqual(output.ToArray()));
    }

    private static ExtensionPackageFact Read(string json)
        => ExtensionManifestReader.Read(Encoding.UTF8.GetBytes(json), 0);
}
