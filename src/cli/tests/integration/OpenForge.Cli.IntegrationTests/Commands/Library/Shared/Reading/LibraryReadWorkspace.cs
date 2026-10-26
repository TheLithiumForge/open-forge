using OpenForge.Cli.Core.Commands.Library.Inspect;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.List;
using OpenForge.Cli.Core.Commands.Library.List.Models.Request;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;

internal sealed class LibraryReadWorkspace : IDisposable
{
    internal const string RecordPath = ".agents/open-forge.lock.json";
    internal const string SourceRoot = "shared/team";
    internal const string ReviewPath = ".agents/directives/review.md";
    internal const string ReviewTarget = "../../shared/team/.agents/directives/review.md";
    private readonly TemporaryWorkspace _temporary = TemporaryWorkspace.Create("library-read");

    internal LibraryReadWorkspace()
    {
        Workspace = new CliWorkspace(_temporary.Path, _temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _temporary.CreateDirectory(".agents/directives");
        _temporary.WriteText("AGENTS.md", "# Consumer\n");
        _temporary.WriteText(".agents/loader.md", "# Consumer loader\n");
        _temporary.WriteText(".agents/directives/_directives.md", "# Consumer directives\n");
        _temporary.WriteText(".agents/directives/_directives.overwrite.md", "# Consumer overrides\n");
        _temporary.WriteText(".agents/open-forge.lifecycle.json", "consumer lifecycle must stay untouched\n");
        _temporary.WriteText("local-sibling.txt", "consumer sibling\n");
    }

    internal CliWorkspace Workspace { get; }
    internal string Path => _temporary.Path;
    internal TemporaryWorkspace Files => _temporary;
    internal LibraryListRequest ListRequest => new() { Workspace = Workspace };
    internal LibraryInspectRequest InspectRequest(string id = "team-knowledge")
        => new() { Workspace = Workspace, LibraryId = LibraryId.Create(id) };

    internal void Source()
        => _temporary.CreateDirectory($"{SourceRoot}/.agents");

    internal void SourceFile(string path = ReviewPath)
        => _temporary.WriteText($"{SourceRoot}/{path}", $"# Source {path}\nsource bytes stay unchanged\n");

    internal void Record(params string[] paths)
        => _temporary.WriteText(RecordPath, $$"""
            {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team","destinationRoot":".","paths":[{{string.Join(',', paths.Select(path => $"\"{path}\""))}}]}]}
            """);

    internal void CurrentLink(string path = ReviewPath)
    {
        // Independent relative-path oracle for this fixture's exact contained source root.
        var parent = System.IO.Path.GetDirectoryName(_temporary.Combine(path))
            ?? throw new InvalidOperationException("A projected file requires a parent.");
        var target = System.IO.Path.GetRelativePath(parent, _temporary.Combine($"{SourceRoot}/{path}"));
        _temporary.CreateFileSymbolicLink(path, target);
    }

    internal async Task<LibraryListResult> ListAsync(CancellationToken cancellationToken)
        => await new LibraryListOperation().ExecuteAsync(ListRequest, cancellationToken);

    internal async Task<LibraryInspectResult> InspectAsync(CancellationToken cancellationToken, string id = "team-knowledge")
        => await new LibraryInspectOperation().ExecuteAsync(InspectRequest(id), cancellationToken);

    internal SortedDictionary<string, string> Snapshot()
        => Snapshot(includeRuntime: false);

    internal SortedDictionary<string, string> Snapshot(bool includeRuntime)
    {
        var entries = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in _temporary.SnapshotHashes(includeRuntime))
        {
            entries.Add(pair.Key, pair.Value);
        }

        CaptureEntries(new DirectoryInfo(Path), entries, includeRuntime);
        return entries;
    }

    internal void AssertNoPersistentState()
    {
        var lockRoot = WorkspaceLockStoreRoot.ResolveForCurrentUser(Environment.SpecialFolderOption.DoNotVerify);
        Assert.NotNull(lockRoot);
        Assert.False(File.Exists(WorkspaceLockPathIdentity.LockPath(lockRoot, Workspace)));
        var recoveryRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.DoNotVerify);
        Assert.NotNull(recoveryRoot);
        Assert.False(Directory.Exists(RecoveryBundlePathIdentity.WorkspaceDirectory(recoveryRoot, Path)));
    }

    private void CaptureEntries(DirectoryInfo directory, SortedDictionary<string, string> entries, bool includeRuntime)
    {
        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
            if (!includeRuntime && TemporaryWorkspace.IsRuntimeEntry(Path, entry.FullName))
            {
                continue;
            }

            var relative = System.IO.Path.GetRelativePath(Path, entry.FullName).Replace('\\', '/');
            if (entry.LinkTarget is { } target)
            {
                entries[relative] = $"link:{target}";
            }
            else if (entry is DirectoryInfo child)
            {
                entries.Add($"{relative}/", "directory");
                CaptureEntries(child, entries, includeRuntime);
            }
        }
    }

    public void Dispose() => _temporary.Dispose();
}
