using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Loading;

public sealed class SourceGeneratedDestinationResolverCharacterizationTests
{
    private const string FixtureCategory = "root";
    private const string HighSurrogate = "\uD800";
    private const string LowSurrogate = "\uDC00";

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Generated destinations preserve literal characters and decode escaped runs exactly once"),
        InlineData("space%20name.md", ".agents/root/space name.md"),
        InlineData("space%C2%A0name.md", ".agents/root/space\u00A0name.md"),
        InlineData("caf%C3%A9-雪-%F0%9F%98%80.md", ".agents/root/café-雪-😀.md"),
        InlineData("caf%c3%a9.md", ".agents/root/café.md"),
        InlineData("café-雪-😀.md", ".agents/root/café-雪-😀.md"),
        InlineData("encoded%2520name.md", ".agents/root/encoded%20name.md"),
        InlineData("%20", ".agents/root/ ")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void DecodingPreservesFrozenCanonicalPaths(string destination, string expectedPath)
    {
        var actual = SourceGeneratedDestinationResolver.Resolve(DefaultWorkspaceLayout, ".agents/root/_root.md", false, destination);

        Assert.Equal(expectedPath, actual);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Generated destinations reject raw whitespace and malformed escaped byte runs"),
        InlineData("space name.md"),
        InlineData("space\tname.md"),
        InlineData("space\u00A0name.md"),
        InlineData("name%"),
        InlineData("name%2"),
        InlineData("name%GG"),
        InlineData("%C3"),
        InlineData("%E2%82"),
        InlineData("%F0%9F%98"),
        InlineData("%80"),
        InlineData("%C0%AF"),
        InlineData("%ED%A0%80"),
        InlineData("%F4%90%80%80"),
        InlineData("%C3%28"),
        InlineData("%C3é%A9"),
        InlineData("valid%C3%A9%GG"),
        InlineData("valid%C3%A9/%FF.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void InvalidDecodingReturnsNoCanonicalPath(string destination)
    {
        Assert.Null(SourceGeneratedDestinationResolver.Resolve(DefaultWorkspaceLayout, ".agents/root/_root.md", false, destination));
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Generated destinations retain containing-source and Loader traversal policies"),
        InlineData(false, "./child//../leaf.md", ".agents/root/leaf.md"),
        InlineData(true, "./child//../leaf.md", ".agents/leaf.md"),
        InlineData(false, "../leaf.md", ".agents/leaf.md"),
        InlineData(true, "../leaf.md", null),
        InlineData(false, "../../leaf.md", null),
        InlineData(false, "%2E%2E/leaf.md", ".agents/leaf.md"),
        InlineData(false, "", null),
        InlineData(false, "/leaf.md", null),
        InlineData(false, "leaf%09.md", null),
        InlineData(false, "leaf%23fragment.md", ".agents/root/leaf#fragment.md"),
        InlineData(false, "leaf%3Fname.md", ".agents/root/leaf?name.md"),
        InlineData(false, "leaf%3Aname.md", ".agents/root/leaf:name.md"),
        InlineData(false, "leaf#fragment.md", null),
        InlineData(false, "leaf?query.md", null),
        InlineData(false, "leaf:name.md", null)]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void PathPolicyRemainsLocal(bool isLoader, string destination, string? expectedPath)
    {
        Assert.Equal(expectedPath, SourceGeneratedDestinationResolver.Resolve(DefaultWorkspaceLayout, ".agents/root/_root.md", isLoader, destination));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Generated destinations preserve literal unpaired UTF-16 surrogates"), Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void LiteralSurrogatesAreNotReencoded()
    {
        var directory = WorkspaceLayout.DefaultMainRoot + "/" + FixtureCategory + "/";
        var source = directory + SourceFormClassifier.CanonicalEntrypointFileName(FixtureCategory);
        foreach (var surrogate in new[] { HighSurrogate, LowSurrogate })
        {
            var destination = surrogate + SourceFormClassifier.MarkdownExtension;
            Assert.Equal(directory + destination, SourceGeneratedDestinationResolver.Resolve(DefaultWorkspaceLayout, source, false, destination));
        }
    }

    private static readonly WorkspaceLayout DefaultWorkspaceLayout = WorkspaceLayout.Create(
            new CliWorkspace(
                Path.GetFullPath(nameof(SourceGeneratedDestinationResolverCharacterizationTests)),
                Path.GetFullPath(nameof(SourceGeneratedDestinationResolverCharacterizationTests)),
                CliWorkspaceSelectionMethod.ExplicitWorkspace),
            WorkspaceLayout.DefaultMainRoot);
}
