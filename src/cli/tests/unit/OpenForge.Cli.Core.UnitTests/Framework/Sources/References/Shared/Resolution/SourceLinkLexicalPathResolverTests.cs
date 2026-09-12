using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.References.Shared.Resolution;

public sealed class SourceLinkLexicalPathResolverTests
{
    [Fact(DisplayName = "A contained lexical destination retains both exact coordinates"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void ContainedDestinationRetainsBothCoordinates()
    {
        var lexicalRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lexical-tests"));

        var result = SourceLinkLexicalPathResolver.Resolve(
            lexicalRoot: lexicalRoot,
            sourceCanonicalPath: ".agents/source.md",
            decodedPath: "target.md");

        Assert.Equal(SourceLinkLexicalPathState.Complete, result.State);
        Assert.Equal(Path.Combine(lexicalRoot, ".agents", "target.md"), result.LexicalTarget);
        Assert.Equal(".agents/target.md", result.CanonicalPath);
    }

    [Fact(DisplayName = "A synthetic volume-root source has no containing directory or result coordinates"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void SyntheticVolumeRootSourceHasNoContainingDirectory()
    {
        var lexicalRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lexical-tests"));
        var volumeRoot = Path.GetPathRoot(lexicalRoot)
            ?? throw new InvalidOperationException("The test lexical root must have a volume root.");

        var result = SourceLinkLexicalPathResolver.Resolve(
            lexicalRoot: lexicalRoot,
            sourceCanonicalPath: volumeRoot,
            decodedPath: "target.md");

        Assert.Equal(SourceLinkLexicalPathState.SourceDirectoryMissing, result.State);
        Assert.Null(result.LexicalTarget);
        Assert.Null(result.CanonicalPath);
    }

    [Fact(DisplayName = "A decoded NUL destination retains the filtered malformed outcome without coordinates"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void DecodedNullCharacterIsMalformedWithoutCoordinates()
    {
        var lexicalRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lexical-tests"));

        var result = SourceLinkLexicalPathResolver.Resolve(
            lexicalRoot: lexicalRoot,
            sourceCanonicalPath: ".agents/source.md",
            decodedPath: "target\0.md");

        Assert.Equal(SourceLinkLexicalPathState.Malformed, result.State);
        Assert.Null(result.LexicalTarget);
        Assert.Null(result.CanonicalPath);
    }

    [Fact(DisplayName = "Traversal outside the lexical workspace yields no result coordinates"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void OutsideTraversalHasNoCoordinates()
    {
        var lexicalRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lexical-tests"));

        var result = SourceLinkLexicalPathResolver.Resolve(
            lexicalRoot: lexicalRoot,
            sourceCanonicalPath: ".agents/source.md",
            decodedPath: "../../outside.md");

        Assert.Equal(SourceLinkLexicalPathState.OutsideWorkspace, result.State);
        Assert.Null(result.LexicalTarget);
        Assert.Null(result.CanonicalPath);
    }
}
