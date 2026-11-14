using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records;

internal sealed class ToolRecordFixture : IDisposable
{
    internal const string UserProperty = "fixture-note";
    internal const string UserValue = "kept";
    internal TemporaryWorkspace Temporary { get; } = TemporaryWorkspace.Create("tool-records");
    internal WorkspaceLockTestStore Locks { get; } = WorkspaceLockTestStore.Create("tool-record-locks");
    internal CliWorkspace Workspace { get; }
    internal byte[] OriginalSettings { get; } = Encoding.UTF8.GetBytes($"{{\"{UserProperty}\":\"{UserValue}\"}}\n");
    internal WorkspaceOwnershipDocument OriginalOwnership { get; } = WorkspaceOwnershipDocument.Empty with
    { Framework = new(new("fixture-framework", ToolFixtureSource.Version), ["fixture.md"], []) };

    internal ToolRecordFixture(bool existing = true)
    {
        Workspace = new(Temporary.Path, new PhysicalPathResolver().ResolveRoot(Temporary.Path).GetContainedPhysicalPath(), CliWorkspaceSelectionMethod.ExplicitWorkspace);
        if (existing)
        {
            Temporary.WriteBytes(WorkspaceSettingsDefinitions.RelativePath, OriginalSettings);
            Temporary.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(OriginalOwnership));
        }
    }

    internal async ValueTask<WorkspaceLockLease> LeaseAsync(CancellationToken token)
    {
        var acquired = await Locks.AcquireAsync(new(Workspace, ToolVocabulary.InstallAction, Guid.NewGuid()), token);
        return acquired.Lease ?? throw new InvalidOperationException("The fixture lease wasn't acquired.");
    }

    internal async ValueTask<ToolRecordWrite> WriteAsync(ToolCommandState state, int? exit, CancellationToken token)
    {
        var settings = await ToolFileReader.ReadAsync(Workspace, WorkspaceSettingsDefinitions.RelativePath, token);
        var receipt = await ToolFileReader.ReadAsync(Workspace, WorkspaceOwnershipDefinitions.RelativePath, token);
        var snapshot = ToolFixtureSource.ManifestBytes();
        var manifest = new ToolCatalogueReader().Definition(snapshot, ToolFixtureSource.ManifestName, false).Manifest;
        var selected = manifest.Targets.OrderBy(pair => pair.Key, StringComparer.Ordinal).First(pair => pair.Value.Entry is not null);
        var record = new ToolRunRecord
        {
            Version = manifest.Version,
            Target = selected.Key,
            ManifestSha256 = ToolCatalogueReader.Hash(snapshot),
            ManifestBytes = snapshot,
            ManifestFormat = "json",
            CodeRoot = ".",
            Hosts = [.. ToolVocabulary.Hosts.Order(StringComparer.Ordinal)],
            Action = ToolVocabulary.InstallAction,
            State = state,
            Commands = [new()
            {
                Action = ToolVocabulary.InstallAction, Host = null, Entry = selected.Value.Entry ?? throw new InvalidOperationException("The fixture entry is missing."),
                EntrySha256 = ToolCatalogueReader.Hash(ToolFixtureSource.ArtifactBytes()), Args = [], Environment = new(StringComparer.Ordinal), State = state, ExitCode = exit, Cause = null,
            }],
        };
        var summary = new ToolRunSummary(record.Action, state, [new(ToolVocabulary.InstallAction, null, state, exit)]);
        var sourceSettings = settings.HasBytes ? settings.Bytes.AsMemory() : ReadOnlyMemory<byte>.Empty;
        var sourceLock = receipt.HasBytes ? WorkspaceOwnershipCodec.Read(receipt.Bytes.AsMemory()).Document : WorkspaceOwnershipDocument.Empty;
        var ownership = sourceLock ?? throw new InvalidOperationException("The fixture lock is invalid.");
        return new(settings, receipt,
            WorkspaceSettingsCodec.SetTools(sourceSettings, ImmutableDictionary<string, WorkspaceToolSetting>.Empty,
                new Dictionary<string, ToolRunSummary>(StringComparer.Ordinal) { [ToolFixtureSource.Id] = summary }),
            WorkspaceOwnershipCodec.Write(ownership with { ToolRuns = ImmutableDictionary<string, ToolRunRecord>.Empty.Add(ToolFixtureSource.Id, record) }));
    }

    public void Dispose()
    {
        var parent = Temporary.Combine(WorkspaceSettingsDefinitions.DirectoryName);
        foreach (var path in new[] { WorkspaceSettingsDefinitions.RelativePath, WorkspaceOwnershipDefinitions.RelativePath })
        {
            var target = Temporary.Combine(path);
            var backup = Temporary.Combine(OpenForge.Cli.Core.Framework.Tools.Shared.Backups.ToolBackupPaths.Beside(path));
            if (File.Exists(target)) File.Delete(target);
            if (File.Exists(backup)) File.Delete(backup);
        }
        if (Directory.Exists(parent)) Directory.Delete(parent, recursive: false);
        Locks.Dispose();
        Temporary.Dispose();
    }
}
