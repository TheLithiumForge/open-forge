using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.GitIgnore;

internal sealed class LibraryGitIgnoreCleanup(LibraryMutationWorkspace workspace) : IDisposable
{
    public void Dispose()
    {
        DeleteLink("docs/a.md", "../shared/team-knowledge/a.md");
        DeleteLink("docs/b.md", "../shared/team-knowledge/b.md", "../shared/other/b.md");
        DeleteLink("docs/c.md", "../shared/local/c.md");
        DeleteLink("leaf.md", "shared/team-knowledge/leaf.md");
        DeleteLink("knowledge/leaf.md", "../shared/team-knowledge/leaf.md");
        DeleteLink(".gitignore", "shared/team-knowledge/.gitignore");
        DeleteLink(".agents/guidance/leaf.md", "../../shared/team-knowledge/.agents/guidance/leaf.md");
        var ignore = workspace.Absolute(".gitignore");
        if (File.Exists(ignore))
        {
            Assert.Equal((FileAttributes)0, File.GetAttributes(ignore) & (FileAttributes.ReparsePoint | FileAttributes.Directory | FileAttributes.Device));
            File.Delete(ignore);
        }
        foreach (var path in new[] { "docs", "knowledge", ".agents/guidance" })
        {
            var directory = new DirectoryInfo(workspace.Absolute(path));
            if (directory.Exists && !directory.EnumerateFileSystemInfos().Any())
            {
                Assert.Null(directory.LinkTarget);
                directory.Delete();
            }
        }
    }

    private void DeleteLink(string path, params string[] acceptedTargets)
    {
        var entry = new FileInfo(workspace.Absolute(path));
        if (entry.LinkTarget is not { } target) return;
        Assert.Contains(target, acceptedTargets);
        entry.Delete();
    }
}
