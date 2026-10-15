using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Filesystem.PhysicalPaths;

[Trait("Feature", "cli-filesystem"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class PathComponentReadSessionIntegrationTests
{
    [Fact(DisplayName = "Read sessions observe shared ordinary ancestors once and each file leaf afresh")]
    public void SharedAncestorsHaveLinearObservationCost()
    {
        using var temporary = TemporaryWorkspace.Create("path-read-session");
        var first = temporary.CreateFile("a/b/c/first.md", "first");
        var second = temporary.CreateFile("a/b/c/second.md", "second");
        var session = new PathComponentReadSession();
        var resolver = new PhysicalPathResolver(session);

        Assert.Equal(PhysicalPathState.Contained, resolver.ResolveCandidate(temporary.Path, temporary.Path, first).State);
        Assert.Equal(PhysicalPathState.Contained, resolver.ResolveCandidate(temporary.Path, temporary.Path, second).State);
        Assert.Equal(5, session.ObservationCount);

        File.Delete(first);
        Assert.Equal(PhysicalPathState.Missing, resolver.ResolveCandidate(temporary.Path, temporary.Path, first).State);
        Assert.Equal(6, session.ObservationCount);
        Assert.Equal(PhysicalPathState.Missing, new PhysicalPathResolver().ResolveCandidate(temporary.Path, temporary.Path, first).State);
    }

    [Fact(DisplayName = "A fresh read session observes changed directory mappings")]
    public void DirectoryObservationsDoNotSurviveReadSessions()
    {
        using var temporary = TemporaryWorkspace.Create("path-read-refresh");
        var file = temporary.CreateFile("folder/value.md", "value");
        Assert.Equal(PhysicalPathState.Contained, new PhysicalPathResolver(new PathComponentReadSession())
            .ResolveCandidate(temporary.Path, temporary.Path, file).State);
        File.Delete(file);
        Directory.Delete(temporary.Combine("folder"));
        var folder = temporary.Combine("folder");
        File.WriteAllText(folder, "now a file");
        try
        {
            Assert.Equal(PhysicalPathState.Missing, new PhysicalPathResolver(new PathComponentReadSession())
                .ResolveCandidate(temporary.Path, temporary.Path, file).State);
        }
        finally
        {
            File.Delete(folder);
            Directory.CreateDirectory(folder);
        }
    }
}
