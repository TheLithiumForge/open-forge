using System.Diagnostics;
using OpenForge.Cli.Composition.Shared.RouteInspectMatchingFiles;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.MatchingFiles;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect;

[Trait("Feature", "route-inspect"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteInspectMatchingFilesGitIntegrationTests
{
    [Fact(DisplayName = "Git inventory keeps current tracked and untracked files and excludes ignored deleted and administrative paths")]
    public async Task GitInventoryFiltersCurrentFiles()
    {
        using var repository = new GitWorkspace();
        await repository.Git("init");
        repository.Write("tracked.txt");
        repository.Write("deleted.txt");
        await repository.Git("add", "tracked.txt", "deleted.txt");
        repository.Write(".gitignore", "*.txt\n");
        repository.Write("ignored.txt");
        repository.Write("untracked.md");
        repository.Write(".git/info/exclude", "repo-ignore.md\n");
        repository.Write("repo-ignore.md");
        repository.Write("global-ignore.md");
        File.WriteAllText(repository.GlobalIgnore, "global-ignore.md\n");
        await repository.Git("config", "core.excludesFile", repository.GlobalIgnore);
        File.Delete(Path.Combine(repository.Root, "deleted.txt"));

        var result = await Scan(repository);

        Assert.True(result.Complete);
        Assert.Equal(RouteInspectMatchingFilesScope.GitTrackedAndUntracked, result.Scope);
        Assert.Equal([".gitignore", "tracked.txt", "untracked.md"], result.Paths);
    }

    [Fact(DisplayName = "A workspace inside a larger Git repository retains workspace-relative inventory")]
    public async Task WorkspaceInsideRepository()
    {
        using var repository = new GitWorkspace();
        await repository.Git("init");
        repository.Write("outside.md");
        repository.Write("workspace/inside.md");
        await repository.Git("add", ".");
        var root = Path.Combine(repository.Root, "workspace");
        var result = await Scan(repository, root);
        Assert.True(result.Complete);
        Assert.Equal(["inside.md"], result.Paths);
    }

    [Fact(DisplayName = "Linked worktree inventory uses its selected root and excludes the administrative pointer")]
    public async Task LinkedWorktree()
    {
        using var repository = new GitWorkspace();
        await repository.Git("init");
        repository.Write("tracked.md");
        await repository.Git("add", ".");
        await repository.Git("-c", "user.name=Scan Test", "-c", "user.email=scan@example.invalid", "commit", "-m", "fixture");
        var linked = Path.Combine(repository.Directory, "linked");
        await repository.Git("worktree", "add", "--detach", linked);
        File.WriteAllText(Path.Combine(linked, "new.md"), "");

        var result = await Scan(repository, linked);

        Assert.True(result.Complete);
        Assert.Equal(RouteInspectMatchingFilesScope.GitTrackedAndUntracked, result.Scope);
        Assert.Equal(["new.md", "tracked.md"], result.Paths);
    }

    [Theory(DisplayName = "Non-Git workspaces and a missing Git executable use the ignore-aware workspace walk")]
    [InlineData(false), InlineData(true)]
    public async Task WalkFallback(bool missingExecutable)
    {
        using var repository = new GitWorkspace();
        repository.Write(".gitignore", "ignored.md\n");
        repository.Write("ignored.md");
        repository.Write("docs/guide.md");
        repository.Write("module/.git", "gitdir: elsewhere");
        repository.Write("module/hidden.md");
        var executable = missingExecutable ? Path.Combine(repository.Directory, "missing-git") : "git";
        var adapter = new RouteInspectGitFileEnumerator(executable, repository.Configure);

        var result = await Scan(repository, adapter: adapter);

        Assert.True(result.Complete);
        Assert.Equal(RouteInspectMatchingFilesScope.WorkspaceFiles, result.Scope);
        Assert.Equal([".gitignore", "docs/guide.md"], result.Paths);
    }

    [Theory(DisplayName = "A refused Git probe with a repository marker uses the ignore-aware walk")]
    [InlineData("directory"), InlineData("ancestor"), InlineData("file")]
    public async Task RefusedProbeUsesIgnoreAwareWalk(string markerKind)
    {
        using var repository = new GitWorkspace();
        var root = repository.Root;
        if (markerKind == "file")
        {
            repository.Write(".git", "gitdir: missing");
        }
        else
        {
            await repository.Git("init");
        }

        if (markerKind == "ancestor")
        {
            root = Directory.CreateDirectory(Path.Combine(repository.Root, "nested", "workspace")).FullName;
        }

        File.WriteAllText(Path.Combine(root, ".gitignore"), "ignored.md\n");
        File.WriteAllText(Path.Combine(root, "ignored.md"), "");
        File.WriteAllText(Path.Combine(root, "visible.md"), "");
        var calls = 0;
        var adapter = new RouteInspectGitFileEnumerator(configureProcess: start =>
        {
            repository.Configure(start);
            calls++;
            // Force a nonzero probe without interpreting any localized Git diagnostic.
            start.ArgumentList[start.ArgumentList.IndexOf("rev-parse")] = "not-a-real-git-command";
        });
        var inventory = await adapter.EnumerateAsync(root, TestContext.Current.CancellationToken);
        Assert.Equal(1, calls);
        Assert.Equal(RouteInspectMatchingFilesScope.WorkspaceFiles, inventory.Scope);
        Assert.Null(inventory.Failure);
        Assert.Equal([".gitignore", "visible.md"], inventory.Paths.Order(StringComparer.Ordinal));
    }

    [Fact(DisplayName = "Walk fallback excludes a root .git directory even when Git cannot start")]
    public async Task WalkExcludesGitDirectory()
    {
        using var repository = new GitWorkspace();
        repository.Write(".git/config");
        repository.Write("visible.md");
        var adapter = new RouteInspectGitFileEnumerator(Path.Combine(repository.Directory, "missing-git"), repository.Configure);
        var result = await Scan(repository, adapter: adapter);
        Assert.Equal(["visible.md"], result.Paths);
    }

    [Theory(DisplayName = "A failed warning-bearing or malformed inventory uses the ignore-aware walk")]
    [InlineData("failure"), InlineData("warning"), InlineData("malformed")]
    public async Task GitFailureUsesWalk(string failure)
    {
        using var repository = new GitWorkspace();
        await repository.Git("init");
        repository.Write("visible.md");
        repository.Write("ignored.md");
        await repository.Git("add", "ignored.md");
        repository.Write(".gitignore", "ignored.md\n");
        var adapter = new RouteInspectGitFileEnumerator(configureProcess: start =>
        {
            repository.Configure(start);
            if (start.ArgumentList.Contains("ls-files"))
            {
                if (failure == "warning")
                {
                    start.Environment["GIT_TRACE"] = "1";
                }
                else if (failure == "malformed")
                {
                    start.ArgumentList.Remove("-z");
                }
                else
                {
                    start.ArgumentList.Insert(start.ArgumentList.IndexOf("ls-files") + 1, "--not-a-git-option");
                }
            }
        });

        var result = await Scan(repository, adapter: adapter);
        Assert.True(result.Complete);
        Assert.Equal(RouteInspectMatchingFilesScope.WorkspaceFiles, result.Scope);
        Assert.Null(result.Reason);
        Assert.Equal([".gitignore", "visible.md"], result.Paths);
    }

    [Fact(DisplayName = "Fallback walk applies nested rules and never reads ignore files in pruned directories")]
    public async Task NestedIgnoreRulesAndPruning()
    {
        using var repository = new GitWorkspace();
        repository.Write(".gitignore", "docs/**\n!docs/keep.md\n*.log\npruned/\n!pruned/keep.md\n[[:alpha:]]\n");
        repository.Write("docs/keep.md");
        repository.Write("docs/drop.md");
        repository.Write("nested/.gitignore", "!keep.log\n*.tmp\n");
        repository.Write("nested/keep.log");
        repository.Write("nested/drop.log");
        repository.Write("nested/drop.tmp");
        repository.Write("pruned/.gitignore", "!keep.md\nunsupported\\\n");
        repository.Write("pruned/keep.md");
        using var locked = new FileStream(Path.Combine(repository.Root, "pruned", ".gitignore"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var adapter = new RouteInspectGitFileEnumerator(Path.Combine(repository.Directory, "missing-git"), repository.Configure);

        var result = await Scan(repository, adapter: adapter);

        Assert.True(result.Complete);
        Assert.Null(result.Reason);
        Assert.Equal([".gitignore", "docs/keep.md", "nested/.gitignore", "nested/keep.log"], result.Paths);
        Assert.Equal("Skipped unsupported .gitignore lines: 1.", result.Note);
    }

    [Theory(DisplayName = "Required unreadable or undecodable ignore files discard the fallback inventory")]
    [InlineData(false), InlineData(true)]
    public async Task IgnoreFileUnavailable(bool undecodable)
    {
        using var repository = new GitWorkspace();
        repository.Write("visible.md");
        repository.Write("nested/.gitignore", "*.log");
        var ignorePath = Path.Combine(repository.Root, "nested", ".gitignore");
        if (undecodable)
        {
            File.WriteAllBytes(ignorePath, [0xFF]);
        }

        using var locked = undecodable ? null : new FileStream(ignorePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var adapter = new RouteInspectGitFileEnumerator(Path.Combine(repository.Directory, "missing-git"), repository.Configure);
        var result = await Scan(repository, adapter: adapter);
        Assert.False(result.Complete);
        Assert.Equal(RouteInspectMatchingFilesScope.WorkspaceFiles, result.Scope);
        Assert.Equal(RouteInspectMatchingFilesReason.FilesUnavailable, result.Reason);
        Assert.Empty(result.Paths);
        Assert.Null(result.Count);
    }

    [Theory(DisplayName = "A Git fallback transition keeps the original deadline and caller cancellation")]
    [InlineData(false), InlineData(true)]
    public async Task FallbackDoesNotRestartDeadline(bool cancel)
    {
        using var repository = new GitWorkspace();
        repository.Write("visible.md");
        using var cancellation = new CancellationTokenSource();
        var clock = new RouteInspectScanClock();
        var calls = 0;
        var adapter = new RouteInspectGitFileEnumerator(Path.Combine(repository.Directory, "missing-git"), start =>
        {
            repository.Configure(start);
            calls++;
            if (cancel)
            {
                cancellation.Cancel();
            }
            else
            {
                clock.Advance(RouteInspectMatchingFiles.ScanDeadline);
            }
        }, timeProvider: clock);
        var result = await adapter.EnumerateAsync(repository.Root, cancellation.Token);
        Assert.Equal(1, calls);
        Assert.Equal(cancel ? RouteInspectMatchingFilesReason.Cancelled : RouteInspectMatchingFilesReason.ScanTimeout, result.Failure);
        Assert.Empty(result.Paths);
    }

    [Theory(DisplayName = "Git scan timeout and caller cancellation clean up the child and retain established scope")]
    [InlineData(false), InlineData(true)]
    public async Task StopsChild(bool cancel)
    {
        using var repository = new GitWorkspace();
        await repository.Git("init");
        using var cancellation = new CancellationTokenSource();
        var clock = new RouteInspectScanClock();
        Process? child = null;
        var adapter = new RouteInspectGitFileEnumerator(configureProcess: start =>
        {
            repository.Configure(start);
            if (!start.ArgumentList.Contains("ls-files"))
            {
                return;
            }

            start.ArgumentList.Clear();
            start.ArgumentList.Add("hash-object");
            start.ArgumentList.Add("--stdin");
            start.RedirectStandardInput = true;
        }, timeProvider: clock, processStarted: process =>
        {
            if (process.StartInfo.RedirectStandardInput)
            {
                child = Process.GetProcessById(process.Id);
                Assert.False(child.HasExited);
                if (cancel)
                {
                    cancellation.Cancel();
                }
                else
                {
                    clock.Advance(RouteInspectMatchingFiles.ScanDeadline);
                }
            }
        });

        try
        {
            var result = await adapter.EnumerateAsync(repository.Root, cancellation.Token);
            Assert.Equal(cancel ? RouteInspectMatchingFilesReason.Cancelled : RouteInspectMatchingFilesReason.ScanTimeout, result.Failure);
            Assert.Equal(RouteInspectMatchingFilesScope.GitTrackedAndUntracked, result.Scope);
            Assert.Empty(result.Paths);
            Assert.NotNull(child);
            Assert.True(child.HasExited);
        }
        finally
        {
            if (child is not null)
            {
                if (!child.HasExited)
                {
                    child.Kill(entireProcessTree: true);
                    await child.WaitForExitAsync(CancellationToken.None);
                }

                child.Dispose();
            }
        }
    }

    [Fact(DisplayName = "Git adapter hardens read-only process launch without changing repository configuration")]
    public async Task ProcessHardening()
    {
        using var repository = new GitWorkspace();
        await repository.Git("init");
        var calls = 0;
        var adapter = new RouteInspectGitFileEnumerator(configureProcess: start =>
        {
            repository.Configure(start);
            calls++;
            Assert.False(start.UseShellExecute);
            Assert.True(start.CreateNoWindow);
            Assert.True(start.RedirectStandardError);
            Assert.True(start.RedirectStandardOutput);
            Assert.Contains("core.fsmonitor=false", start.ArgumentList);
            Assert.Contains("--no-optional-locks", start.ArgumentList);
            Assert.Equal("0", start.Environment["GIT_OPTIONAL_LOCKS"]);
            Assert.Equal("0", start.Environment["GIT_TERMINAL_PROMPT"]);
            Assert.False(start.Environment.ContainsKey("GIT_DIR"));
            Assert.DoesNotContain(start.ArgumentList, argument => argument.StartsWith("safe.directory", StringComparison.Ordinal));
        });
        var before = File.ReadAllBytes(Path.Combine(repository.Root, ".git", "config"));
        var result = await adapter.EnumerateAsync(repository.Root, TestContext.Current.CancellationToken);
        Assert.Null(result.Failure);
        Assert.Equal(2, calls);
        Assert.Equal(before, File.ReadAllBytes(Path.Combine(repository.Root, ".git", "config")));
    }

    [Fact(DisplayName = "Matching scans exclude file and directory links without following external targets")]
    public async Task ExcludesLinks()
    {
        using var workspace = TemporaryWorkspace.Create("matching-links");
        var root = workspace.CreateDirectory("workspace");
        var target = workspace.CreateFile("target.md", "");
        workspace.CreateFile("workspace/visible.md", "");
        Assert.True(workspace.TryCreateFileSymbolicLink("workspace/link.md", target, out _), "File links are required for this scope evidence.");
        Assert.True(workspace.TryCreateDirectorySymbolicLink("workspace/linked", workspace.Path, out _), "Directory links are required for this scope evidence.");
        var scanner = new RouteInspectMatchingFilesScanner((_, _) => ValueTask.FromResult(
            RouteInspectFileEnumeration.Success(RouteInspectMatchingFilesScope.WorkspaceFiles, ["link.md", "linked/target.md", "visible.md"])));
        var result = await scanner.ScanAsync(root, RestrictiveApplicability(), TestContext.Current.CancellationToken);
        Assert.True(result.Complete);
        Assert.Equal(["visible.md"], result.Paths);
        var walk = new RouteInspectGitFileEnumerator(Path.Combine(workspace.Path, "missing-git"));
        var inventory = await walk.EnumerateAsync(root, TestContext.Current.CancellationToken);
        Assert.Null(inventory.Failure);
        Assert.Equal(["visible.md"], inventory.Paths);
    }

    [Fact(DisplayName = "Directory caching preserves nested exclusions and refreshes between matching scans")]
    public async Task CachedDirectoriesPreserveResultsAndScanLifetime()
    {
        using var workspace = TemporaryWorkspace.Create("matching-directory-cache");
        var root = workspace.CreateDirectory("workspace");
        string[] candidates = ["shared/a.md", "shared/b.md", "shared/deep/c.md", "blocked/deep/a.md", "blocked/deep/b.md", "linked/a.md"];
        foreach (var path in candidates.Where(path => !path.StartsWith("linked/", StringComparison.Ordinal)))
        {
            workspace.CreateFile($"workspace/{path}");
        }

        workspace.CreateFile("workspace/blocked/.git");
        var outside = workspace.CreateDirectory("outside");
        workspace.CreateFile("outside/a.md");
        Assert.True(workspace.TryCreateDirectorySymbolicLink("workspace/linked", outside, out _), "Directory links are required for this scope evidence.");
        var scanner = new RouteInspectMatchingFilesScanner((_, _) => ValueTask.FromResult(
            RouteInspectFileEnumeration.Success(RouteInspectMatchingFilesScope.WorkspaceFiles, candidates.Concat(candidates.Reverse()))));
        var applicability = RestrictiveApplicability();
        var first = await scanner.ScanAsync(root, applicability, TestContext.Current.CancellationToken);
        Assert.True(first.Complete);
        Assert.Equal(["shared/a.md", "shared/b.md", "shared/deep/c.md"], first.Paths);
        Assert.Equal(3, first.Count);

        var walked = RouteInspectWorkspaceFileWalker.Enumerate(root, TestContext.Current.CancellationToken);
        Assert.Null(walked.Failure);
        Assert.Equal(first.Paths, walked.Paths.Order(StringComparer.Ordinal));

        workspace.CreateFile("workspace/shared/deep/.git");
        var second = await scanner.ScanAsync(root, applicability, TestContext.Current.CancellationToken);
        Assert.True(second.Complete);
        Assert.Equal(["shared/a.md", "shared/b.md"], second.Paths);
        Assert.Equal(2, second.Count);
        var walkedAgain = RouteInspectWorkspaceFileWalker.Enumerate(root, TestContext.Current.CancellationToken);
        Assert.Null(walkedAgain.Failure);
        Assert.Equal(second.Paths, walkedAgain.Paths.Order(StringComparer.Ordinal));
    }

    private static ValueTask<RouteInspectMatchingFiles> Scan(
        GitWorkspace repository,
        string? root = null,
        RouteInspectGitFileEnumerator? adapter = null)
    {
        adapter ??= new RouteInspectGitFileEnumerator(configureProcess: repository.Configure);
        var scanner = new RouteInspectMatchingFilesScanner(adapter.EnumerateAsync);
        return scanner.ScanAsync(root ?? repository.Root, RestrictiveApplicability(), TestContext.Current.CancellationToken);
    }

    [Theory(DisplayName = "Fallback ignores only workspace-local rules and preserves skipped-line notes at zero matches")]
    [InlineData(false), InlineData(true)]
    public async Task WorkspaceIgnoreBoundaryAndZeroNote(bool zero)
    {
        using var repository = new GitWorkspace();
        repository.Write(".gitignore", "*\n");
        repository.Write("selected/.gitignore", "dangling\\\n");
        repository.Write("selected/visible.md");
        var adapter = new RouteInspectGitFileEnumerator(Path.Combine(repository.Directory, "missing-git"), repository.Configure);
        var scanner = new RouteInspectMatchingFilesScanner(adapter.EnumerateAsync);
        var result = await scanner.ScanAsync(Path.Combine(repository.Root, "selected"),
            RestrictiveApplicability(zero ? "absent/**" : "**/*.*"), TestContext.Current.CancellationToken);
        Assert.True(result.Complete);
        Assert.Equal(zero ? 0 : 2, result.Count);
        Assert.Equal(zero ? [] : [".gitignore", "visible.md"], result.Paths);
        Assert.Equal((zero ? RouteInspectMatchingFiles.NoMatchesNote + " " : "") + "Skipped unsupported .gitignore lines: 1.", result.Note);
    }

    private static SourceApplicabilityResult RestrictiveApplicability(string text = "**/*.*")
    {
        var pattern = ApplyToPatternMatcher.Parse(text).Pattern ?? throw new InvalidOperationException("Invalid test pattern.");
        var metadata = ApplyToMetadataFacts.Valid([pattern],
            [new ApplyToDeclaration(ApplyToMetadataLocation.Root, new YamlTextSpan(0, 1), new YamlTextSpan(2, 1), [pattern])]);
        return SourceApplicabilityEvaluator.Evaluate([new SourceApplyToCondition("source.md", metadata)], []);
    }

    // Git creates its own files, so this fixture owns the entire temporary tree.
    private sealed class GitWorkspace : IDisposable
    {
        internal string Directory { get; } = System.IO.Directory.CreateTempSubdirectory("open-forge-matching-git-").FullName;
        internal string Root { get; }
        internal string GlobalIgnore => Path.Combine(Directory, "global-ignore");

        internal GitWorkspace()
        {
            Root = System.IO.Directory.CreateDirectory(Path.Combine(Directory, "workspace")).FullName;
        }

        internal void Write(string path, string contents = "")
        {
            var absolute = Path.Combine(Root, path);
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(absolute) ?? Root);
            File.WriteAllText(absolute, contents);
        }

        internal void Configure(ProcessStartInfo start)
        {
            start.Environment["GIT_CONFIG_GLOBAL"] = Path.Combine(Directory, "global-config");
            start.Environment["GIT_CONFIG_SYSTEM"] = Path.Combine(Directory, "system-config");
            start.Environment["GIT_CONFIG_NOSYSTEM"] = "1";
            start.Environment["GIT_CONFIG_COUNT"] = "0";
            start.Environment.Remove("GIT_CONFIG_PARAMETERS");
            start.Environment.Remove("GIT_TRACE");
            start.Environment["HOME"] = Directory;
            start.Environment["XDG_CONFIG_HOME"] = Directory;
            start.Environment["GIT_TERMINAL_PROMPT"] = "0";
            start.Environment.Remove("GIT_DIR");
            start.Environment.Remove("GIT_WORK_TREE");
            start.Environment.Remove("GIT_INDEX_FILE");
        }

        internal async Task Git(params string[] arguments)
        {
            var start = new ProcessStartInfo("git")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            Configure(start);
            start.ArgumentList.Add("-C");
            start.ArgumentList.Add(Root);
            foreach (var argument in arguments)
            {
                start.ArgumentList.Add(argument);
            }

            using var process = Process.Start(start) ?? throw new InvalidOperationException("Git did not start.");
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(30));
            try
            {
                await process.WaitForExitAsync(deadline.Token);
                Assert.True(process.ExitCode == 0, await error);
                await output;
            }
            finally
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync(CancellationToken.None);
                }
            }
        }

        public void Dispose()
        {
            foreach (var file in System.IO.Directory.EnumerateFiles(Directory, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            System.IO.Directory.Delete(Directory, recursive: true);
        }
    }
}
