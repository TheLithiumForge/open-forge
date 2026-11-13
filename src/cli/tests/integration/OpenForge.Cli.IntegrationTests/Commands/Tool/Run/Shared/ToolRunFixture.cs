using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Tool.Run;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Tools;

using OpenForge.Cli.TestSupport.Tools.Run;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Run.Shared;

internal sealed class ToolRunFixture : IDisposable
{
    internal const string AccountDirectory = "account";
    internal const string WorkspaceDirectory = "workspace";
    internal const string ChildDirectory = "nested";
    internal const string DataMember = "immutable.dat";
    internal const string FakeEntry = "fixture.bin";
    internal TemporaryWorkspace Temporary { get; } = TemporaryWorkspace.Create("tool-run");
    internal ToolStoreRegistry Store { get; }
    internal CliWorkspace Workspace { get; }
    internal ToolCatalogueReader Catalogue { get; private set; } = new();
    private ToolReceipt? _receipt;
    internal ToolReceipt Receipt => _receipt ?? throw new InvalidOperationException("The fixture hasn't been initialized.");
    internal string UnitPath { get; private set; } = "";
    internal string Key => ToolStoreIdentity.UnitKey(ToolStoreRegistryDefinitions.Install, UnitPath);
    internal string Cwd { get; }
    internal string ManifestPath => Path.Combine(Workspace.LexicalRoot, ToolFixtureSource.ManifestName);
    internal string EntryPath => Store.Installs.At(UnitPath + "/" + Receipt.Files.First(file => file.Executable).Path);
    internal ToolRunOperation Operation => new(Catalogue, Store.Paths);
    internal ToolRunRequest Request => new()
    {
        WorkspacePath = Workspace.LexicalRoot,
        CurrentDirectory = Cwd,
        HostProjectDirectory = null,
        Id = ToolFixtureSource.Id,
        Arguments = [],
        Interactive = false,
    };

    private ToolRunFixture()
    {
        var paths = new PhysicalPathResolver();
        var workspace = Temporary.CreateDirectory(WorkspaceDirectory);
        Workspace = new(workspace, paths.ResolveRoot(workspace).GetContainedPhysicalPath(), CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Cwd = Temporary.CreateDirectory(WorkspaceDirectory + "/" + ChildDirectory);
        Store = new(ToolStoreLocator.FromRoot(Temporary.CreateDirectory(AccountDirectory)));
    }

    internal static async Task<ToolRunFixture> CreateAsync(CancellationToken token, bool executable = false, bool mcp = false)
    {
        var fixture = new ToolRunFixture();
        try { await fixture.InitializeAsync(token, executable, mcp); return fixture; }
        catch { fixture.Dispose(); throw; }
    }

    private async Task InitializeAsync(CancellationToken token, bool executable, bool mcp)
    {
        var entry = executable ? Path.GetFileName(ToolRunProbe.Executable) : FakeEntry;
        var source = ToolFixtureSource.ManifestForAllTargets();
        source["admission"] = "enabled";
        var qualified = new JsonArray();
        foreach (var target in ToolVocabulary.Targets) qualified.Add((JsonNode?)JsonValue.Create(target));
        source["qualification"]?["targets"] = qualified;
        foreach (var target in source["targets"]?.AsObject() ?? throw new InvalidOperationException())
        {
            target.Value?["entry"] = entry;
            if (executable) target.Value?["launcher"] = new JsonObject { ["executable"] = entry, ["args"] = new JsonArray(ToolRunProbe.Argument, ToolRunProbe.ChildMode) };
        }
        if (mcp) source["mcp"]?["args"] = new JsonArray(ToolRunChild.Hold);
        var bytes = Encoding.UTF8.GetBytes(source.ToJsonString());
        var manifest = new ToolCatalogueReader().Definition(bytes, ToolFixtureSource.ManifestName, false).Manifest;
        var digest = ToolCatalogueReader.Hash(bytes);
        Catalogue = new(new ToolReleaseSupport([new(manifest.Id, manifest.Version, digest, manifest.Adapter.Revision)], [new(manifest.Adapter.Id, manifest.Adapter.Revision)]));
        await File.WriteAllBytesAsync(ManifestPath, bytes, token);
        UnitPath = ToolStoreIdentity.InstallPath(manifest, ToolNativeTarget.Current);
        await Store.InitializeAsync(token);
        Store.Installs.EnsureDirectory(UnitPath, token);
        var root = Store.Installs.At(UnitPath);
        var members = new List<string>();
        if (executable)
        {
            foreach (var path in Directory.EnumerateFiles(AppContext.BaseDirectory))
            {
                var name = Path.GetFileName(path);
                File.Copy(path, Path.Combine(root, name));
                members.Add(name);
            }
        }
        else { await File.WriteAllBytesAsync(Path.Combine(root, entry), ToolFixtureSource.ArtifactBytes(), token); members.Add(entry); }
        await File.WriteAllBytesAsync(Path.Combine(root, DataMember), ToolFixtureSource.ArtifactBytes(), token);
        members.Add(DataMember);
        var binding = ToolStoreIdentity.Binding(ToolStoreRegistryDefinitions.Install, root);
        var files = new List<ToolFileReceipt>();
        var stamps = new List<ToolFileStamp>();
        foreach (var member in members.Order(StringComparer.Ordinal))
        {
            var path = Path.Combine(root, member);
            var isExecutable = member == entry;
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | (isExecutable ? UnixFileMode.UserExecute : 0));
            files.Add(new(binding.Id, member, ToolCatalogueReader.Hash(await File.ReadAllBytesAsync(path, token)), isExecutable));
            var info = new FileInfo(path);
            stamps.Add(new(binding.Id, member, info.Length, info.LastWriteTimeUtc.Ticks));
        }
        var launch = new ToolLaunchBinding
        {
            ToolId = manifest.Id,
            Version = manifest.Version,
            ManifestSha256 = digest,
            StoreId = binding.Id,
            Executable = Path.Combine(root, entry),
            PrefixArguments = executable ? [ToolRunProbe.Argument, ToolRunProbe.ChildMode] : [],
            Environment = new() { [ToolRunChild.EnvironmentName] = ToolRunChild.EnvironmentValue },
            Files = [.. stamps],
            McpArguments = mcp ? [[ToolRunChild.Hold]] : null,
        };
        var unit = new ToolStoredUnit
        {
            Kind = ToolStoreRegistryDefinitions.Install,
            Verified = true,
            Path = UnitPath,
            Binding = binding,
            RecipeSha256 = digest,
            Files = [.. files],
            Directories = [],
            Dependencies = [],
            Dependents = [],
            Workspaces = new() { [WorkspaceIdentity.Key(Workspace.PhysicalRoot)] = new() { PhysicalWorkspace = Workspace.PhysicalRoot, CodeRoot = ".", Launch = launch } },
        };
        await RewriteRegistryAsync(new() { [Key] = unit }, token);
        _receipt = new()
        {
            Id = manifest.Id,
            Version = manifest.Version,
            Target = ToolNativeTarget.Current,
            ManifestSha256 = digest,
            ManifestBytes = bytes,
            ManifestFormat = "json",
            AdapterRevision = manifest.Adapter.Revision,
            CodeRoot = ".",
            Hosts = [.. ToolVocabulary.Hosts],
            Bindings = [new(binding.Id, binding.Kind, binding.IdentitySha256)],
            Files = [.. files],
            RuntimeDirectories = [],
            NativeProjects = [],
        };
        WriteSettings(new() { Version = manifest.Version, Roles = [] });
        WriteReceipt(Receipt);
    }

    internal void WriteSettings(WorkspaceToolSetting setting)
        => WriteWorkspace(WorkspaceSettingsDefinitions.RelativePath, WorkspaceSettingsCodec.SetTools(ReadOnlyMemory<byte>.Empty,
            new Dictionary<string, WorkspaceToolSetting> { [ToolFixtureSource.Id] = setting }, ImmutableDictionary<string, OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle.ToolRunSummary>.Empty));

    internal void WriteReceipt(ToolReceipt receipt)
        => WriteWorkspace(WorkspaceOwnershipDefinitions.RelativePath,
            WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with { Tools = ImmutableDictionary<string, ToolReceipt>.Empty.Add(ToolFixtureSource.Id, receipt) }));

    private void WriteWorkspace(string relative, byte[] bytes)
    {
        var path = WorkspaceDirectory + "/" + relative;
        if (File.Exists(Temporary.Combine(path))) Temporary.ReplaceBytes(path, bytes);
        else Temporary.WriteBytes(path, bytes);
    }

    internal async Task RewriteRegistryAsync(Dictionary<string, ToolStoredUnit> units, CancellationToken token)
    {
        using var lease = ToolStoreLease.Acquire(Store.State, token);
        var prior = await Store.ReadAsync(token);
        await Store.WriteAsync(lease, prior, prior.Document with { Units = units }, token);
    }

    public void Dispose()
    {
        // Combine checks that each product-created subtree stays inside the marker-owned root.
        foreach (var path in new[] { WorkspaceDirectory, AccountDirectory })
        {
            var directory = new DirectoryInfo(Temporary.Combine(path));
            if (directory.Exists) { DeleteChildren(directory); directory.Delete(); }
        }
        Temporary.Dispose();
    }

    private static void DeleteChildren(DirectoryInfo directory)
    {
        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
            if (entry is DirectoryInfo child && child.LinkTarget is null) { DeleteChildren(child); child.Delete(); }
            else entry.Delete();
        }
    }
}
