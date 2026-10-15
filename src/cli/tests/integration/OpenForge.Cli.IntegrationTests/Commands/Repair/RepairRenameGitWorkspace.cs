using System.Diagnostics;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

internal sealed class RepairRenameGitWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _temporary = TemporaryWorkspace.Create("repair-rename");
    internal string Root { get; }
    private string? _originalTree;
    internal CliWorkspace Workspace => new(Root, Root, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    internal RepairRenameGitWorkspace() => Root = _temporary.CreateDirectory("repository");

    internal async Task InitializeAsync(IReadOnlyDictionary<string, string>? documents = null, bool unborn = false)
    {
        await GitAsync("init", "--quiet");
        var source = new DirectoryInfo(AppContext.BaseDirectory);
        while (source is not null && !File.Exists(Path.Combine(source.FullName, "OpenForge.Cli.slnx"))) source = source.Parent;
        var sourceRoot = source?.FullName ?? throw new InvalidOperationException();
        var runner = new GitProcessRunner("git", Configure);
        var common = await runner.RunAsync(sourceRoot, ["-c", $"safe.directory={sourceRoot.Replace('\\', '/')}", "rev-parse", "--git-common-dir"], TestContext.Current.CancellationToken);
        var head = await runner.RunAsync(sourceRoot, ["-c", $"safe.directory={sourceRoot.Replace('\\', '/')}", "rev-parse", "HEAD"], TestContext.Current.CancellationToken);
        Assert.True(common is { ExitCode: 0 } && head is { ExitCode: 0 });
        var objects = Path.GetFullPath(Path.Combine(sourceRoot, common.Value.Output.Trim(), "objects"));
        Directory.CreateDirectory(PathFor(".git/objects/info"));
        File.WriteAllText(PathFor(".git/objects/info/alternates"), objects.Replace('\\', '/') + "\n");
        await GitAsync("update-ref", "HEAD", head.Value.Output.Trim());
        await GitAsync("checkout", "HEAD", "--", ".agents/guidance/cli-design.md");
        Write(".agents/guidance/cli-design.md", "---\nopen-forge:\n  description: Guidance for clear command line operations\n  tags: [Guidance]\n---\n\n# CLI Design\n\nEach command owns one complete operation.\n");
        Write("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        Write(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build("- [Guidance](guidance/_guidance.md) - #Guidance")
            .Replace("# Open Forge Loader", "# Open Forge Loader\n\n## Axioms\n\n- Preserve authored content.", StringComparison.Ordinal));
        Write(".agents/guidance/_guidance.md", "---\nopen-forge:\n  description: Guidance\n  tags: [Guidance]\n---\n\n# Guidance\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n");
        foreach (var item in documents ?? new Dictionary<string, string>()) Write(item.Key, item.Value);
        // A scratch-only tree replacement provides a small HEAD fixture without creating a commit.
        _originalTree = (await GitAsync("rev-parse", "HEAD^{tree}")).Trim();
        await RecordTreeAsync();
        if (unborn) await GitAsync("update-ref", "-d", "HEAD");
    }

    internal async Task RecordTreeAsync()
    {
        await GitAsync("add", "--", ".agents", "AGENTS.md");
        var tree = await GitAsync("write-tree");
        await GitAsync("replace", "-f", _originalTree ?? throw new InvalidOperationException(), tree.Trim());
    }

    internal string PathFor(string path) => Path.Combine(Root, path.Replace('/', Path.DirectorySeparatorChar));
    internal void Write(string path, string text)
    {
        var physical = PathFor(path);
        Directory.CreateDirectory(Path.GetDirectoryName(physical) ?? throw new InvalidOperationException());
        File.WriteAllText(physical, text);
    }
    internal void Move(string oldPath, string newPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(PathFor(newPath)) ?? throw new InvalidOperationException());
        File.Move(PathFor(oldPath), PathFor(newPath));
    }
    internal void Configure(ProcessStartInfo start)
    {
        start.Environment["GIT_CONFIG_GLOBAL"] = PathFor("absent-global");
        start.Environment["GIT_CONFIG_SYSTEM"] = PathFor("absent-system");
        start.Environment["GIT_CONFIG_NOSYSTEM"] = "1";
    }
    internal async Task<string> GitAsync(params string[] arguments)
    {
        var result = await new GitProcessRunner("git", Configure).RunAsync(Root, arguments, TestContext.Current.CancellationToken);
        Assert.True(result is { ExitCode: 0 }, result?.Error);
        return (result ?? throw new InvalidOperationException("Git did not start.")).Output;
    }
    public void Dispose()
    {
        Assert.Null(new DirectoryInfo(Root).LinkTarget);
        Assert.StartsWith(_temporary.Path + Path.DirectorySeparatorChar, Root, StringComparison.Ordinal);
        foreach (var file in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal);
        Directory.Delete(Root, recursive: true);
        _temporary.Dispose();
    }
}
