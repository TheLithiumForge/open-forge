using System.Text;
using OpenForge.Cli.Core.Framework.Extensions;
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

    private static ExtensionPackageFact Read(string json)
        => ExtensionManifestReader.ReadManifestOnly(Encoding.UTF8.GetBytes(json));
}
