using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Run.Models.Resolution;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Run.Shared.Resolution;

internal sealed class ToolRunPinSeed
{
    internal const string Entry = "fixture.bin";
    internal CliWorkspace Workspace { get; } = new(Path.GetFullPath("fixture-workspace"), Path.GetFullPath("fixture-workspace"), CliWorkspaceSelectionMethod.CurrentDirectory);
    internal ToolRunRequest Request { get; }
    internal ToolRunPinEvidence Evidence { get; }
    internal ToolCatalogueReader Catalogue { get; }
    internal string Key { get; }
    internal string WorkspaceKey => WorkspaceIdentity.Key(Workspace.PhysicalRoot);
    internal ToolStoredUnit Unit => Evidence.Registry.Units[Key];
    internal ToolLaunchBinding Launch => Unit.Workspaces[WorkspaceKey].Launch ?? throw new InvalidOperationException();

    internal ToolRunPinSeed()
    {
        var source = ToolFixtureSource.ManifestForAllTargets();
        source["admission"] = "enabled";
        var qualified = new JsonArray();
        foreach (var target in ToolVocabulary.Targets) qualified.Add(target);
        source["qualification"]?["targets"] = qualified;
        var bytes = Encoding.UTF8.GetBytes(source.ToJsonString());
        var hash = ToolCatalogueReader.Hash(bytes);
        var manifest = new ToolCatalogueReader().Definition(bytes, ToolFixtureSource.ManifestName, false).Manifest;
        Catalogue = new(new ToolReleaseSupport([new(ToolFixtureSource.Id, ToolFixtureSource.Version, hash, manifest.Adapter.Revision)], [new(manifest.Adapter.Id, manifest.Adapter.Revision)]));
        var path = ToolStoreIdentity.InstallPath(manifest, ToolNativeTarget.Current);
        Key = ToolStoreIdentity.UnitKey(ToolStoreRegistryDefinitions.Install, path);
        var binding = ToolStoreIdentity.Binding(ToolStoreRegistryDefinitions.Install, Path.GetFullPath("fixture-install"));
        ToolFileReceipt[] files = [new(binding.Id, Entry, ToolCatalogueReader.Hash(ToolFixtureSource.ArtifactBytes()), true)];
        var launch = new ToolLaunchBinding
        {
            ToolId = ToolFixtureSource.Id,
            Version = ToolFixtureSource.Version,
            ManifestSha256 = hash,
            StoreId = binding.Id,
            Executable = Path.Combine(binding.PhysicalRoot, Entry),
            PrefixArguments = [],
            Environment = [],
            Files = [new(binding.Id, Entry, ToolFixtureSource.ArtifactBytes().Length, DateTime.UnixEpoch.AddDays(1).Ticks)],
        };
        var unit = new ToolStoredUnit
        {
            Kind = ToolStoreRegistryDefinitions.Install,
            Verified = true,
            Path = path,
            Binding = binding,
            RecipeSha256 = hash,
            Files = files,
            Directories = [],
            Dependencies = [],
            Dependents = [],
            Workspaces = new() { [WorkspaceKey] = new() { PhysicalWorkspace = Workspace.PhysicalRoot, CodeRoot = ".", Launch = launch } },
        };
        var receipt = new ToolReceipt
        {
            Id = ToolFixtureSource.Id,
            Version = ToolFixtureSource.Version,
            Target = ToolNativeTarget.Current,
            ManifestSha256 = hash,
            ManifestBytes = bytes,
            ManifestFormat = "json",
            AdapterRevision = manifest.Adapter.Revision,
            CodeRoot = ".",
            Hosts = [.. ToolVocabulary.Hosts],
            Bindings = [new(binding.Id, binding.Kind, binding.IdentitySha256)],
            Files = files,
            RuntimeDirectories = [],
            NativeProjects = [],
        };
        Evidence = new(new() { Version = ToolFixtureSource.Version, Roles = [] }, receipt, new() { Roots = [], Units = new() { [Key] = unit } });
        Request = new() { WorkspacePath = null, CurrentDirectory = Workspace.PhysicalRoot, HostProjectDirectory = null, Id = ToolFixtureSource.Id, Arguments = [], Interactive = false };
    }

    internal ToolRunPinEvidence WithUnit(ToolStoredUnit unit) => Evidence with { Registry = Evidence.Registry with { Units = new() { [Key] = unit } } };
    internal ToolRunPinEvidence WithLaunch(ToolLaunchBinding? launch) => WithUnit(Unit with
    { Workspaces = new() { [WorkspaceKey] = Unit.Workspaces[WorkspaceKey] with { Launch = launch } } });
}
