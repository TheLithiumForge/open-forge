using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Export;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Uninstall;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Stripping;

/// <summary>An owned workspace for uninstall and export evidence, with exact-byte helpers and an owned export destination.</summary>
internal sealed class StripIntegrationWorkspace : IDisposable
{
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);
    private readonly TemporaryWorkspace _temporary;
    private readonly WorkspaceLockTestStore _lockStore;
    private readonly HashSet<string> _ownedDirectories = new(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
    private readonly HashSet<string> _ownedFiles = new(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
    private DirectoryInfo? _destination;

    private StripIntegrationWorkspace(TemporaryWorkspace temporary, WorkspaceLockTestStore lockStore)
    {
        _temporary = temporary;
        _lockStore = lockStore;
        Workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _ = lockStore.Track(Workspace);
    }

    internal CliWorkspace Workspace { get; }

    internal string Path => _temporary.Path;

    internal static StripIntegrationWorkspace Create(string purpose)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        return new StripIntegrationWorkspace(temporary, lockStore);
    }

    internal string Combine(string path) => _temporary.Combine(path);

    // Seed files go through the temporary workspace whenever their nearest existing folder is one it owns, so it can
    // delete them. Below a folder the CLI created, the workspace under test owns them, so they are written directly
    // and removed with that folder.
    internal void Write(string path, string text) => WriteBytes(path, Utf8.GetBytes(text));

    internal void WriteBytes(string path, byte[] bytes)
    {
        var physical = System.IO.Path.GetFullPath(Combine(path));
        var parent = System.IO.Path.GetDirectoryName(physical)!;
        if (_ownedFiles.Contains(physical) && File.Exists(physical))
        {
            _temporary.ReplaceBytes(path, bytes);
            return;
        }

        var existing = parent;
        var created = new List<string>();
        while (!Directory.Exists(existing))
        {
            created.Add(existing);
            existing = System.IO.Path.GetDirectoryName(existing)!;
        }

        var root = System.IO.Path.GetFullPath(Path).TrimEnd(System.IO.Path.DirectorySeparatorChar);
        if (!File.Exists(physical) && (existing.Equals(root, StringComparison.OrdinalIgnoreCase) || _ownedDirectories.Contains(existing)))
        {
            _ownedDirectories.UnionWith(created);
            _temporary.CreateFile(path, bytes);
            _ownedFiles.Add(physical);
            return;
        }

        Directory.CreateDirectory(parent);
        File.WriteAllBytes(physical, bytes);
    }

    internal string Read(string path) => File.ReadAllText(Combine(path), Utf8);

    internal byte[] ReadBytes(string path) => File.ReadAllBytes(Combine(path));

    internal bool Exists(string path) => File.Exists(Combine(path)) || Directory.Exists(Combine(path));

    internal IReadOnlyDictionary<string, string> SnapshotHashes() => _temporary.SnapshotHashes();

    internal async Task InstallAsync()
    {
        var completion = await RunAsync([
            InstallDefinitions.CommandIdentity,
            InstallDefinitions.PresetOption,
            InstallConfigurationChoices.Name(InstallPreset.Minimal),
            InstallDefinitions.Automatic.Name]);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
    }

    internal async Task IncludeAsync(string folder)
    {
        var result = await new RouteIncludeOperation(_lockStore.StoreRoot).ExecuteAsync(
            new RouteIncludeRequest { Workspace = Workspace, File = folder }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
    }

    internal StripRequest Request(StripCommandKind command, ImmutableArray<StripPart> parts, string? flatten = null, bool dryRun = false)
        => new()
        {
            Workspace = Workspace,
            Command = command,
            Parts = parts,
            Flatten = flatten,
            DryRun = dryRun,
            Automatic = true,
        };

    internal StripOperationRunner Runner()
        => new(_lockStore.StoreRoot, null, static _ => throw new InvalidOperationException("Automatic runs never ask."));

    // An empty folder outside the workspace that this fixture owns and deletes.
    internal string Destination => (_destination ??= Directory.CreateTempSubdirectory("open-forge-export-")).FullName;

    internal StripRequest ExportRequest(ImmutableArray<StripPart> parts, string? flatten = null, bool dryRun = false)
        => Request(StripCommandKind.Export, parts, flatten, dryRun) with { Destination = Destination, Automatic = false };

    // These workspaces are not Git work trees, so export lists their files by walking them.
    internal ValueTask<StripResult> ExportAsync(StripRequest request)
        => new ExportOperation(static (_, _) => ValueTask.FromResult<IReadOnlyList<string>?>(null))
            .ExecuteAsync(request, TestContext.Current.CancellationToken);

    internal string ReadExport(string path) => File.ReadAllText(System.IO.Path.Combine(Destination, path), Utf8);

    internal byte[] ReadExportBytes(string path) => File.ReadAllBytes(System.IO.Path.Combine(Destination, path));

    // Every file in the export destination, as canonical relative paths. The copied fixture owner marker is not
    // part of any scenario.
    internal IReadOnlyList<string> ExportFiles()
        => [.. Directory.EnumerateFiles(Destination, "*", SearchOption.AllDirectories)
            .Select(file => System.IO.Path.GetRelativePath(Destination, file).Replace(System.IO.Path.DirectorySeparatorChar, '/'))
            .Where(path => path != TemporaryWorkspace.OwnershipMarkerName)
            .Order(StringComparer.Ordinal)];

    internal ValueTask<StripResult> UninstallAsync(StripRequest request)
        => new UninstallOperation(Runner()).ExecuteAsync(request, TestContext.Current.CancellationToken);

    internal WorkspaceLockStoreRoot LockStoreRoot => _lockStore.StoreRoot;

    internal async Task<CliProcessCompletion> RunAsync(IReadOnlyList<string> arguments)
        => (await RunWithOutputAsync(arguments)).Completion;

    internal async Task<(CliProcessCompletion Completion, string Output, string Error)> RunWithOutputAsync(IReadOnlyList<string> arguments)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity(CliSyntaxDefinitions.ExecutableName, nameof(StripIntegrationWorkspace)),
            new CliCompositionInputs
            {
                StandardInput = TextReader.Null,
                PromptOutput = TextWriter.Null,
                StandardInputRedirected = true,
                PromptOutputRedirected = true,
                LockStoreRoot = _lockStore.StoreRoot,
            });
        var completion = await application.RunAsync(
            [.. arguments],
            new CliProcessEnvironment(Path),
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);
        return (completion, output.ToString(), error.ToString());
    }

    public void Dispose()
    {
        try
        {
            // Strip, flatten and route commands write product files into seeded folders. Remove them, deepest first,
            // so the temporary workspace finds only the entries it created.
            foreach (var directory in _ownedDirectories.Where(Directory.Exists).OrderByDescending(directory => directory.Length))
            {
                foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
                {
                    if (_ownedFiles.Contains(entry) || _ownedDirectories.Contains(entry)) continue;
                    if (Directory.Exists(entry) && !File.GetAttributes(entry).HasFlag(FileAttributes.ReparsePoint)) Directory.Delete(entry, recursive: true);
                    else File.Delete(entry);
                }
            }

            // Install and uninstall create entries the temporary workspace never owned.
            foreach (var entry in new[] { WorkspaceLayout.DefaultMainRoot, WorkspaceLayoutNames.AgentHostFileName, WorkspaceLayoutNames.ClaudeHostFileName, WorkspaceRuntimePaths.DirectoryName })
            {
                var path = Combine(entry);
                if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
                else if (File.Exists(path)) File.Delete(path);
            }
        }
        finally
        {
            if (_destination is { Exists: true } destination)
            {
                destination.Delete(recursive: true);
            }

            _lockStore.Dispose();
            _temporary.Dispose();
        }
    }
}
