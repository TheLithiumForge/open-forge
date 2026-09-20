using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Request;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

internal sealed class LibraryMutationWorkspace : IDisposable
{
    internal const string Leaf = ".agents/directives/review.md";
    internal const string SourceRoot = "shared/team-knowledge";
    internal const string RecordPath = ".agents/open-forge.lock.json";
    internal const string OwnershipPath = ".agents/open-forge.lock.json";
    internal const string SourceBytes = "# Review\n\nRetain source bytes exactly.\n";
    private readonly TemporaryWorkspace _workspace = TemporaryWorkspace.Create("library-mutation");

    internal LibraryMutationWorkspace()
    {
        _workspace.CreateDirectory(".agents");
        _workspace.CreateDirectory($"{SourceRoot}/.agents");
    }

    internal string Path => _workspace.Path;
    internal string Absolute(string path) => _workspace.Combine(path);
    internal CliWorkspace Workspace => new(Path, Path, CliWorkspaceSelectionMethod.CurrentDirectory);
    internal void Write(string path, string text) => _workspace.WriteText(path, text);
    internal void Replace(string path, string text) => _workspace.ReplaceText(path, text);
    internal void Source(string path = Leaf) => SourceAt(SourceRoot, path);
    internal void SourceAt(string sourceRoot, string path) => Write($"{sourceRoot}/{path}", SourceBytes);
    internal void Directory(string path) => _workspace.CreateDirectory(path);
    internal void Link(string path = Leaf, string? rawTarget = null)
    {
        var parent = System.IO.Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Missing projection parent.");
        _workspace.CreateFileSymbolicLink(path, rawTarget ?? System.IO.Path.GetRelativePath(parent, $"{SourceRoot}/{path}").Replace('\\', '/'));
    }

    internal void ConsumerRoute()
    {
        Write("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        Write(".agents/loader.md", """
            # Loader

            ## Entries
            - [Directives](directives/_directives.md) - #Directive
            """);
        Write(".agents/directives/_directives.md", """
            ---
            open-forge:
              description: Directives
              tags: [Directive]
            ---
            # Directives

            ## Entries
            - [Review](review.md) - #Directive
            """);
    }

    internal void RoutedSource()
        => Write($"{SourceRoot}/{Leaf}", """
            ---
            open-forge:
              description: Review
              tags: [Directive]
            ---
            # Review

            Retain source-owned body.
            """);

    internal void DirectoryLink(string path, string target) => _workspace.CreateDirectorySymbolicLink(path, target);
    internal void Record(params string[] paths) => RecordAt(".", paths);

    internal void RecordAt(string destinationRoot, params string[] paths)
        => OwnershipAt(destinationRoot, paths);

    internal void OwnershipAt(string destinationRoot, params string[] paths)
    {
        File.WriteAllBytes(_workspace.Combine(OwnershipPath), WorkspaceOwnershipCodec.Write(new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            Framework: null,
            Extensions: [],
            Libraries:
            [
                new LibraryOwnership(
                    "team-knowledge",
                    SourceRoot,
                    destinationRoot,
                    [.. paths.Order(StringComparer.Ordinal)]),
            ])));
    }

    internal LibraryPermissionOperation Permissions { get; } = new(
        LibraryPermissionTestPrompt.Create(TextReader.Null, TextWriter.Null, canPrompt: false));

    internal LibraryAttachRequest Attach(LibraryMode mode = LibraryMode.DryRun)
        => Attach("team-knowledge", SourceRoot, ".", mode);

    internal LibraryAttachRequest Attach(
        string libraryId,
        string destinationRoot,
        LibraryMode mode = LibraryMode.DryRun)
        => Attach(libraryId, SourceRoot, destinationRoot, mode);

    internal LibraryAttachRequest Attach(
        string libraryId,
        string sourceRoot,
        string destinationRoot,
        LibraryMode mode = LibraryMode.DryRun)
        => new()
        {
            Workspace = Workspace,
            LibraryId = LibraryId.Create(libraryId),
            AllowPrompt = false,
            Automatic = mode == LibraryMode.Apply,
            SourceRoot = WorkspaceRelativeDirectory.Create(sourceRoot),
            DestinationRoot = LibraryDestinationRoot.Create(destinationRoot),
            Mode = mode,
        };
    internal LibrarySyncRequest Sync(LibraryMode mode = LibraryMode.DryRun)
        => new() { Workspace = Workspace, LibraryId = LibraryId.Create("team-knowledge"), AllowPrompt = false, Automatic = mode == LibraryMode.Apply, Mode = mode };
    internal LibraryDetachRequest Detach(LibraryMode mode = LibraryMode.DryRun)
        => Detach("team-knowledge", mode);

    internal LibraryDetachRequest Detach(string libraryId, LibraryMode mode = LibraryMode.DryRun)
        => new() { Workspace = Workspace, LibraryId = LibraryId.Create(libraryId), AllowPrompt = false, Automatic = mode == LibraryMode.Apply, Mode = mode };

    internal IReadOnlyDictionary<string, string> Snapshot()
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in _workspace.SnapshotHashes())
        {
            result.Add(pair.Key, pair.Value);
        }

        Observe(new DirectoryInfo(Path), result);
        return result;
    }

    private void Observe(DirectoryInfo directory, IDictionary<string, string> result)
    {
        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
            var relative = System.IO.Path.GetRelativePath(Path, entry.FullName).Replace('\\', '/');
            if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                result[relative] = $"link:{entry.LinkTarget}";
            }
            else if (entry is DirectoryInfo child)
            {
                result[relative] = "directory";
                Observe(child, result);
            }
        }
    }

    public void Dispose()
    {
        var ownershipPath = Absolute(OwnershipPath);
        if (File.Exists(ownershipPath))
        {
            var attributes = File.GetAttributes(ownershipPath);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException("The Library ownership cleanup target is not an ordinary file.");
            }

            File.Delete(ownershipPath);
        }

        _workspace.Dispose();
    }
}
