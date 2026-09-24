using System.Text;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsWriteTests
{
    private static string Add(string existing, string path)
    {
        var written = WorkspaceSettingsCodec.AddAllowInstallPath(Encoding.UTF8.GetBytes(existing), path);
        return Encoding.UTF8.GetString(Assert.IsType<byte[]>(written));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "An absent file becomes one entry")]
    public void CreatesFromNothing()
    {
        var written = WorkspaceSettingsCodec.AddAllowInstallPath(ReadOnlyMemory<byte>.Empty, "docs");

        var result = WorkspaceSettingsCodec.Read(Assert.IsType<byte[]>(written));
        Assert.Equal(["docs"], result.Document!.AllowInstallPaths);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "An entry is appended after the existing ones")]
    public void AppendsInOrder()
    {
        var text = Add("""{"allowInstallPaths":["tools"]}""", "docs");

        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(text));
        Assert.Equal(["tools", "docs"], result.Document!.AllowInstallPaths);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "An already admitted entry is not added twice")]
    public void IsIdempotent()
        => Assert.Null(WorkspaceSettingsCodec.AddAllowInstallPath(
            Encoding.UTF8.GetBytes("""{"allowInstallPaths":["docs"]}"""), "docs"));

    /// <summary>
    /// The reason for round-tripping the object model rather than rewriting from
    /// the parsed settings: a key this CLI does not recognise still belongs to the
    /// author, and writing must not silently drop it.
    /// </summary>
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Unknown keys and their order survive the write")]
    public void PreservesUnknownKeys()
    {
        const string existing = """
            {"$schema":"https://example.invalid/s.json",
             "removedCategories":["templates"],
             "removedFiles":[".agents/memory/_memory.md"],
             "rules":{"reference.cycle":"off"},
             "allowInstallPaths":["tools"],
             "future":{"nested":[1,2,{"deep":true}]}}
            """;

        var text = Add(existing, "docs");

        Assert.Contains("\"$schema\"", text, StringComparison.Ordinal);
        Assert.Contains("\"reference.cycle\"", text, StringComparison.Ordinal);
        Assert.Contains("\"deep\"", text, StringComparison.Ordinal);
        Assert.True(
            text.IndexOf("\"$schema\"", StringComparison.Ordinal)
                < text.IndexOf("\"removedCategories\"", StringComparison.Ordinal),
            "Authored key order must survive.");

        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(text));
        Assert.Equal(["tools", "docs"], result.Document!.AllowInstallPaths);
        Assert.Equal(["templates"], result.Document.RemovedCategories);
        Assert.Equal([".agents/memory/_memory.md"], result.Document.RemovedFiles);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A missing allowInstallPaths key is created")]
    public void CreatesTheKey()
    {
        var text = Add("""{"removedCategories":["skills"]}""", "docs");

        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(text));
        Assert.Equal(["docs"], result.Document!.AllowInstallPaths);
        Assert.Equal(["skills"], result.Document.RemovedCategories);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Comments are lost, which is why the loss is asserted rather than assumed")]
    public void DropsComments()
    {
        var text = Add("""
            {
              // Open Forge may write here
              "allowInstallPaths": ["tools"]
            }
            """, "docs");

        Assert.DoesNotContain("Open Forge may write here", text, StringComparison.Ordinal);
        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(text));
        Assert.Equal(["tools", "docs"], result.Document!.AllowInstallPaths);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "A file that is not an object, or whose key is not an array, is refused")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    [InlineData("""{"allowInstallPaths":"docs"}""")]
    [InlineData("""{"allowInstallPaths":{"docs":true}}""")]
    public void RefusesUnwritableShapes(string existing)
        => Assert.Throws<ArgumentException>(
            () => WorkspaceSettingsCodec.AddAllowInstallPath(Encoding.UTF8.GetBytes(existing), "docs"));

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A null allowInstallPaths is replaced rather than refused")]
    public void ReplacesNullKey()
    {
        var text = Add("""{"allowInstallPaths":null}""", "docs");

        var result = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(text));
        Assert.Equal(["docs"], result.Document!.AllowInstallPaths);
    }
}
