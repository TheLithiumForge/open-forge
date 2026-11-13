using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing;

internal sealed class ToolWriterSupplyStager(ToolStoreRegistry registry, ToolDownload download)
{
    internal const string ArchiveMember = "artifact";
    private readonly List<string> _stages = [];
    internal IReadOnlyList<string> Stages => _stages;

    internal async ValueTask<ToolWriterStagedSupply> StageAsync(ToolWriterSupply supply, CancellationToken token)
    {
        if (supply.Store.Verified) return new([], null);
        await registry.InitializeAsync(token).ConfigureAwait(false);
        var archives = new List<ToolPreparedStoreUnit>();
        var extractions = new List<(ToolWriterArtifact Artifact, string Archive)>();
        foreach (var artifact in supply.Artifacts)
        {
            string archive;
            if (artifact.Cached is { } cached)
            {
                archive = registry.Archives.At(cached.Path + "/" + ArchiveMember);
                await ToolArtifactIntegrity.VerifyAsync(archive, artifact.Artifact.Bytes ?? throw new InvalidDataException("An artifact needs a byte size."), artifact.Artifact.Integrity, token).ConfigureAwait(false);
            }
            else
            {
                var stage = CreateStage(registry.Archives, token);
                archive = registry.Archives.At(stage + "/" + ArchiveMember);
                await download(new(artifact.Source, artifact.Artifact.Bytes ?? throw new InvalidDataException("An artifact needs a byte size."), artifact.Artifact.Integrity, archive), token).ConfigureAwait(false);
                await ToolArtifactIntegrity.VerifyAsync(archive, artifact.Artifact.Bytes.Value, artifact.Artifact.Integrity, token).ConfigureAwait(false);
                archives.Add(new()
                {
                    Kind = ToolStoreRegistryDefinitions.Archive,
                    Path = artifact.Path,
                    RecipeSha256 = artifact.RecipeSha256,
                    StagingPath = stage,
                    Files = [new(null, ArchiveMember, await ToolArtifactIntegrity.FileHashAsync(archive, token).ConfigureAwait(false), false)],
                    Directories = [],
                    Dependencies = [],
                });
            }
            extractions.Add((artifact, archive));
        }

        // Check every archive before extracting any member. Destination collisions span the complete artifact set.
        var files = new HashSet<string>(StringComparer.Ordinal);
        var directories = new HashSet<string>(StringComparer.Ordinal);
        long expanded = 0;
        foreach (var (artifact, archive) in extractions)
        {
            var plan = await ToolArchiveExtractor.ReadAsync(new(artifact.Artifact, archive, registry.Installs.Root), token).ConfigureAwait(false);
            expanded = checked(expanded + plan.ExpandedBytes);
            foreach (var member in plan.Members)
            {
                var key = PortableWorkspacePath.CreatePortableKey(member.Path);
                if (member.Directory)
                {
                    if (files.Contains(key)) throw new InvalidDataException("Artifact members share a file and directory destination.");
                    directories.Add(key);
                }
                else if (directories.Contains(key) || !files.Add(key)) throw new InvalidDataException("Artifacts share a file destination.");
                for (var parent = Parent(key); parent is not null; parent = Parent(parent))
                {
                    if (files.Contains(parent)) throw new InvalidDataException("An artifact file is also a member's parent.");
                    directories.Add(parent);
                }
            }
        }
        var stageRoot = CreateStage(registry.Installs, token);
        var drive = new DriveInfo(Path.GetPathRoot(registry.Installs.Root) ?? throw new IOException("The install volume isn't available."));
        if (drive.AvailableFreeSpace < expanded) throw new IOException("The install volume has insufficient space.");
        var receipts = new List<ToolFileReceipt>();
        var allDirectories = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (artifact, archive) in extractions)
        {
            var part = CreateStage(registry.Installs, token);
            var extracted = await ToolArchiveExtractor.ExtractAsync(new(artifact.Artifact, archive, registry.Installs.At(part)), token).ConfigureAwait(false);
            foreach (var directory in extracted.Directories.OrderBy(path => path.Count(character => character == '/')).ThenBy(path => path, StringComparer.Ordinal))
            {
                registry.Installs.EnsureDirectory(stageRoot + "/" + directory, token);
                allDirectories.Add(directory);
            }
            foreach (var file in extracted.Files)
            {
                token.ThrowIfCancellationRequested();
                File.Move(registry.Installs.At(part + "/" + file.Path), registry.Installs.At(stageRoot + "/" + file.Path));
                receipts.Add(file);
            }
            foreach (var directory in extracted.Directories.OrderByDescending(path => path.Count(character => character == '/')))
                Directory.Delete(registry.Installs.At(part + "/" + directory), recursive: false);
            Directory.Delete(registry.Installs.At(part), recursive: false);
        }
        return new([.. archives], new()
        {
            Kind = ToolStoreRegistryDefinitions.Install,
            Path = ToolStoreIdentity.InstallPath(supply.Definition.Manifest, supply.Target),
            RecipeSha256 = supply.Definition.Sha256,
            StagingPath = stageRoot,
            Files = [.. receipts.OrderBy(file => file.Path, StringComparer.Ordinal)],
            Directories = [.. allDirectories.Order(StringComparer.Ordinal)],
            Dependencies = [.. supply.Artifacts.Select(artifact => ToolStoreIdentity.UnitKey(ToolStoreRegistryDefinitions.Archive, artifact.Path)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
        });
    }

    private string CreateStage(ToolStoreArea area, CancellationToken token)
    {
        var stage = ToolStoreRegistryDefinitions.StagingDirectory + "/" + Guid.NewGuid().ToString("N");
        area.EnsureDirectory(stage, token);
        _stages.Add(area.At(stage));
        return stage;
    }

    private static string? Parent(string path) => path.LastIndexOf('/') is var separator && separator >= 0 ? path[..separator] : null;
}
