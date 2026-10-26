using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using System.Collections.Immutable;
using System.Diagnostics;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Input;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

internal sealed class RenameEvidenceGitWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _temporary = TemporaryWorkspace.Create("rename-evidence");
    internal string RepositoryRoot { get; }
    internal string Root { get; private set; }
    internal CliWorkspace Workspace => new(Root, Root, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    internal RenameEvidenceGitWorkspace()
    {
        RepositoryRoot = _temporary.CreateDirectory("repository");
        Root = RepositoryRoot;
    }

    internal async Task InitializeAsync(bool unborn = false, bool nested = false)
    {
        await GitAsync("init", "--quiet");
        if (unborn) return;
        var source = FindSourceRoot();
        var runner = new GitProcessRunner("git", Configure);
        var common = await runner.RunAsync(source, ["-c", $"safe.directory={source.Replace('\\', '/')}", "rev-parse", "--git-common-dir"], TestContext.Current.CancellationToken);
        Assert.NotNull(common);
        Assert.Equal(0, common.Value.ExitCode);
        var objects = Path.GetFullPath(Path.Combine(source, common.Value.Output.Trim(), "objects"));
        var head = await runner.RunAsync(source, ["-c", $"safe.directory={source.Replace('\\', '/')}", "rev-parse", "HEAD"], TestContext.Current.CancellationToken);
        Assert.NotNull(head);
        Assert.Equal(0, head.Value.ExitCode);
        Directory.CreateDirectory(Path.Combine(RepositoryRoot, ".git", "objects", "info"));
        File.WriteAllText(Path.Combine(RepositoryRoot, ".git", "objects", "info", "alternates"), objects.Replace('\\', '/') + "\n");
        await GitAsync("update-ref", "HEAD", head.Value.Output.Trim());
        await GitAsync("checkout", "HEAD", "--", ".agents");
        if (nested)
        {
            await GitAsync("checkout", "HEAD", "--", "src/extensions/collaboration/content");
            Root = Path.Combine(RepositoryRoot, "src", "extensions", "collaboration", "content");
        }
    }

    internal RenameEvidenceRequest Request(params string[] missing)
        => new()
        {
            Workspace = Workspace,
            CandidatePaths = Directory.Exists(Path.Combine(Root, ".agents"))
                ? Directory.EnumerateFiles(Path.Combine(Root, ".agents"), "*.md", SearchOption.AllDirectories)
                    .Select(path => Path.GetRelativePath(Root, path).Replace(Path.DirectorySeparatorChar, '/')).Order(StringComparer.Ordinal).ToImmutableArray()
                : [],
            MissingPaths = missing.ToImmutableArray(),
            CandidateUniverseComplete = true,
            Policy = new() { Layout = WorkspaceLayout.Create(Workspace, WorkspaceLayout.DefaultMainRoot) },
        };

    internal string PathFor(string path) => Path.Combine(Root, path.Replace('/', Path.DirectorySeparatorChar));

    internal void Write(string path, string text)
    {
        var physical = PathFor(path);
        Directory.CreateDirectory(Path.GetDirectoryName(physical) ?? throw new InvalidOperationException());
        File.WriteAllText(physical, text);
    }

    internal void Move(string oldPath, string newPath)
    {
        var destination = PathFor(newPath);
        Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? throw new InvalidOperationException());
        File.Move(PathFor(oldPath), destination);
    }

    internal async Task GitAsync(params string[] arguments)
    {
        var result = await new GitProcessRunner("git", Configure).RunAsync(RepositoryRoot, arguments, TestContext.Current.CancellationToken);
        Assert.True(result is { ExitCode: 0 }, result?.Error ?? "Git did not start.");
    }

    internal void Configure(ProcessStartInfo start)
    {
        start.Environment["GIT_CONFIG_GLOBAL"] = Path.Combine(RepositoryRoot, "absent-global-config");
        start.Environment["GIT_CONFIG_SYSTEM"] = Path.Combine(RepositoryRoot, "absent-system-config");
        start.Environment["GIT_CONFIG_NOSYSTEM"] = "1";
        start.Environment["GIT_CONFIG_COUNT"] = "0";
        start.Environment.Remove("GIT_CONFIG_PARAMETERS");
        start.Environment.Remove("GIT_TRACE");
    }

    internal IReadOnlyDictionary<string, string> Snapshot() => _temporary.SnapshotHashes();

    private static string FindSourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OpenForge.Cli.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("The build-owned source root is unavailable.");
    }

    public void Dispose()
    {
        Assert.Null(new DirectoryInfo(RepositoryRoot).LinkTarget);
        Assert.StartsWith(_temporary.Path + Path.DirectorySeparatorChar, RepositoryRoot, StringComparison.Ordinal);
        foreach (var file in Directory.EnumerateFiles(RepositoryRoot, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal);
        Directory.Delete(RepositoryRoot, recursive: true);
        _temporary.Dispose();
    }
}
