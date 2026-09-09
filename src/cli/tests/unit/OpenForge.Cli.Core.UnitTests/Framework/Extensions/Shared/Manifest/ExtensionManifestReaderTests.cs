using OpenForge.Cli.Core.Framework.Extensions.Shared.Manifest;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Extensions.Shared.Manifest;

public sealed class ExtensionManifestReaderTests
{
    private const string ValidManifest = """{"id":"toolkit","name":"Toolkit","description":"A toolkit.","version":"1.2.3","dependencies":["alpha","base"]}""";

    [Theory(DisplayName = "Manifest reader preserves every manifest fact and explicit package contents"), Trait("Feature", "extension-manifest"), Trait("Evidence", "Unit")]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadsManifestFacts(bool emptyPayload)
    {
        var files = new[]
        {
            ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot { Path = "content/z.txt", State = ExtensionPackageFileReadState.Missing }),
            ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot { Path = "content/a.txt", State = ExtensionPackageFileReadState.Blocked }),
        };
        var bytes = Encoding.UTF8.GetBytes(ValidManifest);

        var package = emptyPayload
            ? ExtensionManifestReader.Read(bytes, "extension.json", [])
            : ExtensionManifestReader.Read(bytes, "toolkit/extension.json", files);

        Assert.Equal("toolkit", package.Id);
        Assert.Equal("Toolkit", package.Name);
        Assert.Equal("A toolkit.", package.Description);
        Assert.Equal("1.2.3", package.Version);
        Assert.Equal(["alpha", "base"], package.Dependencies);
        Assert.Equal(emptyPayload ? "extension.json" : "toolkit/extension.json", package.ManifestPath);
        if (emptyPayload)
        {
            Assert.Empty(package.Payload);
        }
        else
        {
            Assert.Equal([files[1], files[0]], package.Payload);
            files[0] = files[1];
            Assert.Equal("content/z.txt", package.Payload[1].Path);
        }
    }

    [Theory(DisplayName = "Manifest validation rejects invalid required values and dependency ordering"), Trait("Feature", "extension-manifest"), Trait("Evidence", "Unit")]
    [InlineData("\"id\":\"toolkit\"", "\"id\":\"Invalid_ID\"")]
    [InlineData("\"name\":\"Toolkit\"", "\"name\":\" \"")]
    [InlineData("\"description\":\"A toolkit.\"", "\"description\":null")]
    [InlineData("\"version\":\"1.2.3\"", "\"version\":\"\"")]
    [InlineData("[\"alpha\",\"base\"]", "null")]
    [InlineData("[\"alpha\",\"base\"]", "[\"Invalid_ID\"]")]
    [InlineData("[\"alpha\",\"base\"]", "[\"base\",\"alpha\"]")]
    [InlineData("[\"alpha\",\"base\"]", "[\"toolkit\"]")]
    public void RejectsOwnedInvalidValues(string original, string replacement)
    {
        var bytes = Encoding.UTF8.GetBytes(ValidManifest.Replace(original, replacement, StringComparison.Ordinal));

        Assert.Throws<JsonException>(() => ExtensionManifestReader.Read(bytes, "extension.json", []));
    }

    [Fact(DisplayName = "Duplicate dependency IDs retain the distinct manifest conflict classification"), Trait("Feature", "extension-manifest"), Trait("Evidence", "Unit")]
    public void ClassifiesDuplicateDependencies()
    {
        var bytes = Encoding.UTF8.GetBytes(ValidManifest.Replace("[\"alpha\",\"base\"]", "[\"alpha\",\"alpha\"]", StringComparison.Ordinal));

        var full = Assert.Throws<ExtensionManifestDependencyConflictException>(() => ExtensionManifestReader.Read(bytes, "extension.json", []));

        Assert.Equal("Extension dependencies cannot contain duplicate stable IDs.", full.Message);
    }

    [Theory(DisplayName = "Manifest readers reject absent malformed and ambiguous package documents"), Trait("Feature", "extension-manifest"), Trait("Evidence", "Unit")]
    [InlineData("null")]
    [InlineData("{")]
    [InlineData("{}")]
    [InlineData("{\"id\":\"toolkit\",\"id\":\"other\",\"name\":\"Toolkit\",\"description\":\"Test\",\"version\":\"1\",\"dependencies\":[]}")]
    [InlineData("{\"id\":\"toolkit\",\"name\":\"Toolkit\",\"description\":\"Test\",\"version\":\"1\",\"dependencies\":[],\"unknown\":true}")]
    public void RejectsInvalidDocuments(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);

        Assert.ThrowsAny<JsonException>(() => ExtensionManifestReader.Read(bytes, "extension.json", []));
    }

    [Fact(DisplayName = "Manifest decoding rejects invalid UTF-8 before package formation"), Trait("Feature", "extension-manifest"), Trait("Evidence", "Unit")]
    public void RejectsInvalidUtf8()
    {
        byte[] bytes = [0xff];

        Assert.Throws<DecoderFallbackException>(() => ExtensionManifestReader.Read(bytes, "extension.json", []));
    }
}
