using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Shared.Applicability;

[Trait("Feature", "source-applicability"), Trait("Evidence", "Unit")]
public sealed class SourceWorkingPathNormalizerTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Working paths normalize relative and rooted planned files without filesystem access")]
    public void NormalizesRelativeAndRootedPlannedFiles()
    {
        using var workspace = TemporaryWorkspace.Create("source-applicability");
        var relativePath = Path.Combine("src", "planned.cs");
        var rootedPath = Path.Combine(workspace.Path, "docs", "planned.md");

        var result = SourceWorkingPathNormalizer.Normalize(
            workspace.Path,
            [relativePath, rootedPath]);

        Assert.Equal(["src/planned.cs", "docs/planned.md"], result.Paths);
        Assert.Empty(result.InvalidPaths);
        Assert.False(File.Exists(Path.Combine(workspace.Path, relativePath)));
        Assert.False(File.Exists(rootedPath));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Working paths reject workspace root and paths outside the workspace")]
    public void RejectsRootAndOutsidePaths()
    {
        using var workspace = TemporaryWorkspace.Create("source-applicability");
        var parent = Path.GetDirectoryName(workspace.Path)
            ?? throw new InvalidOperationException("A temporary workspace must have a parent directory.");
        var outsidePath = Path.Combine(
            parent,
            $"{Path.GetFileName(workspace.Path)}-outside",
            "planned.cs");

        var result = SourceWorkingPathNormalizer.Normalize(
            workspace.Path,
            [workspace.Path, outsidePath]);

        Assert.Empty(result.Paths);
        Assert.Equal([workspace.Path, outsidePath], result.InvalidPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Working paths normalize separators and preserve first occurrence when deduplicating")]
    public void NormalizesSeparatorsAndDeduplicates()
    {
        using var workspace = TemporaryWorkspace.Create("source-applicability");
        var relativePath = Path.Combine("src", "planned.cs");
        var absolutePath = Path.GetFullPath(Path.Combine(workspace.Path, relativePath));

        var result = SourceWorkingPathNormalizer.Normalize(
            workspace.Path,
            [relativePath, absolutePath, relativePath, Path.Combine("src", "..", "src", "planned.cs")]);

        Assert.Equal(["src/planned.cs"], result.Paths);
        Assert.Empty(result.InvalidPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Working paths reject blank and control-character inputs")]
    public void RejectsBlankAndControlCharacterInputs()
    {
        using var workspace = TemporaryWorkspace.Create("source-applicability");
        var invalidPaths = new[] { "", " ", "src/name\nbreak.cs" };

        var result = SourceWorkingPathNormalizer.Normalize(workspace.Path, invalidPaths);

        Assert.Empty(result.Paths);
        Assert.Equal(invalidPaths, result.InvalidPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Working path deduplication is ordinal")]
    public void PreservesPathsThatDifferOnlyByCase()
    {
        using var workspace = TemporaryWorkspace.Create("source-applicability");

        var result = SourceWorkingPathNormalizer.Normalize(
            workspace.Path,
            [Path.Combine("src", "file.cs"), Path.Combine("Src", "file.cs")]);

        Assert.Equal(["src/file.cs", "Src/file.cs"], result.Paths);
        Assert.Empty(result.InvalidPaths);
    }
}
