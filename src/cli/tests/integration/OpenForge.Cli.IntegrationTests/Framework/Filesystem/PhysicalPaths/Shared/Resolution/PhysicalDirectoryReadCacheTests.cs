using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Filesystem.PhysicalPaths.Shared.Resolution;

public sealed class PhysicalDirectoryReadCacheTests
{
    [Fact(DisplayName = "A physical read session inspects shared directories once and keeps file leaves and new sessions fresh"),
        Trait("Feature", "physical-paths"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public void DirectoryReadsAreBoundedWithinOneStage()
    {
        using var temporary = TemporaryWorkspace.Create("physical-directory-work");
        temporary.WriteText("parent/child/one.md", "one");
        temporary.WriteText("parent/child/two.md", "two");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var calls = new Dictionary<string, int>(StringComparer.Ordinal);
        var resolver = new PhysicalPathResolver(path =>
        {
            calls[path] = calls.GetValueOrDefault(path) + 1;
            return LinkTargetReader.Read(path);
        });
        var session = resolver.CreateReadSession();
        var first = temporary.Combine("parent/child/one.md");
        var second = temporary.Combine("parent/child/two.md");
        Assert.Equal(PhysicalPathState.Contained, session.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, first).State);
        Assert.Equal(PhysicalPathState.Contained, session.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, first).State);
        Assert.Equal(PhysicalPathState.Contained, session.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, second).State);
        Assert.Equal(1, calls[temporary.Combine("parent")]);
        Assert.Equal(1, calls[temporary.Combine("parent/child")]);
        Assert.Equal(2, calls[first]);
        Assert.Equal(1, calls[second]);

        File.Delete(first);
        Assert.Equal(PhysicalPathState.Missing, session.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, first).State);
        Assert.Equal(3, calls[first]);
        Assert.Equal(PhysicalPathState.Contained, session.CreateReadSession().ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, second).State);
        Assert.Equal(2, calls[temporary.Combine("parent")]);
        Assert.Equal(2, calls[temporary.Combine("parent/child")]);
    }
}
