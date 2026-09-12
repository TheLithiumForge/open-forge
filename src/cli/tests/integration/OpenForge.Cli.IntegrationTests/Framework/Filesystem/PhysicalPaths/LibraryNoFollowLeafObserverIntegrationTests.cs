using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Filesystem.PhysicalPaths;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryNoFollowLeafObserverIntegrationTests
{
    [Theory(DisplayName = "No-follow observations classify ordinary missing directory and exact dangling link objects")]
    [InlineData("missing", "Missing"), InlineData("file", "OrdinaryFile"), InlineData("directory", "Directory")]
    [InlineData("relative", "RelativeFileLink"), InlineData("dangling", "RelativeFileLink"), InlineData("absolute", "Link")]
    public static void ObservesLeafWithoutFollowingTarget(string scenario, string expectedState)
    {
        using var temporary = TemporaryWorkspace.Create("library-no-follow");
        var target = temporary.CreateFile("source/a.md", "source bytes");
        temporary.CreateDirectory(".agents");
        var path = temporary.Combine(".agents/a.md");
        switch (scenario)
        {
            case "file": temporary.CreateFile(".agents/a.md", "local"); break;
            case "directory": temporary.CreateDirectory(".agents/a.md"); break;
            case "relative": temporary.CreateFileSymbolicLink(".agents/a.md", "../source/a.md"); break;
            case "dangling": temporary.CreateFileSymbolicLink(".agents/a.md", "../source/missing.md"); break;
            case "absolute": temporary.CreateFileSymbolicLink(".agents/a.md", target); break;
        }
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var result = NoFollowLeafObserver.Observe(new PhysicalPathResolver(), workspace, path, TestContext.Current.CancellationToken);

        Assert.Equal(expectedState, result.State.ToString());
        Assert.Equal(path, result.LogicalPath);
        if (scenario is "relative" or "dangling")
        {
            var link = Assert.IsType<RelativeFileLinkIdentity>(result.RelativeFileLink);
            Assert.Equal(scenario == "relative" ? "../source/a.md" : "../source/missing.md", link.RawRelativeTarget);
            Assert.Equal(NoFollowLinkKind.SymbolicLink, link.LinkKind);
        }
        if (scenario == "absolute")
        {
            Assert.Null(result.RelativeFileLink);
            var identity = Assert.IsType<NoFollowLinkIdentity>(result.Link);
            Assert.Equal(target, identity.RawTarget);
            Assert.Equal(NoFollowLinkTargetForm.Absolute, identity.TargetForm);
        }
        Assert.Equal("source bytes", File.ReadAllText(target));
        Assert.False(File.Exists(temporary.Combine("source/missing.md")));
    }

    [Fact(DisplayName = "No-follow observation retains raw identity without resolving an external final-link target")]
    public void ObservesExternalLinkObjectWithoutEnteringTarget()
    {
        using var temporary = TemporaryWorkspace.Create("library-no-follow-external");
        using var external = TemporaryWorkspace.Create("library-no-follow-target");
        var target = external.CreateFile("private.md", "untouched");
        var linkPath = temporary.CreateFileSymbolicLink(".agents/a.md", target);
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var result = NoFollowLeafObserver.Observe(new PhysicalPathResolver(), workspace, linkPath, TestContext.Current.CancellationToken);

        Assert.Equal(NoFollowLeafState.Link, result.State);
        Assert.Equal(target, Assert.IsType<NoFollowLinkIdentity>(result.Link).RawTarget);
        Assert.Equal("untouched", File.ReadAllText(target));
    }
}
