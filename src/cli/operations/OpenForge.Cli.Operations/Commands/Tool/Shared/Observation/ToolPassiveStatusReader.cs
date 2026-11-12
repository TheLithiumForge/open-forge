using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Observation;

internal static class ToolPassiveStatusReader
{
    internal const string GuidancePath = ".agents/skills/open-forge-code-navigation/SKILL.md";

    internal static async ValueTask<ToolStatusRow> ReadAsync(
        CliWorkspace workspace,
        WorkspaceSettingsDocument settings,
        ToolInstalledRow row,
        ToolReceipt? receipt,
        CancellationToken token)
    {
        var inactive = row.Roles.Length == 0;
        if (receipt is null || row.Verification.State != ToolEvidenceState.Matching)
        {
            var unavailable = new ToolObservation(ToolEvidenceState.Unavailable, row.Verification.Cause ?? "A matching receipt is required.");
            return new(row.Id, row.Version, receipt?.Target, row.Roles, unavailable,
                inactive ? new(ToolEvidenceState.Inactive) : unavailable, unavailable,
                inactive ? new(ToolEvidenceState.Inactive) : unavailable, null, []);
        }

        var manifest = new ToolCatalogueReader().Definition(receipt.ManifestBytes, "receipt." + receipt.ManifestFormat, false).Manifest;
        var installedFiles = receipt.Files.Where(file => file.StoreId is not null || file.Path != GuidancePath).ToArray();
        var installation = installedFiles.Length == 0
            ? new ToolObservation(ToolEvidenceState.Unavailable, "The receipt has no installed file hashes.")
            : new ToolObservation(ToolEvidenceState.Verified);
        foreach (var file in installedFiles)
        {
            if (file.StoreId is not null)
            {
                installation = Worse(installation, new(ToolEvidenceState.Unavailable, "The external file needs a verified account-local store binding."));
                continue;
            }
            var read = await ReadWorkspaceFileAsync(workspace, file.Path, token).ConfigureAwait(false);
            installation = Worse(installation, read.State switch
            {
                ToolEvidenceState.Missing => new(ToolEvidenceState.Missing, file.Path),
                ToolEvidenceState.Unavailable => new(ToolEvidenceState.Unavailable, read.Cause),
                _ => FileHash(read, file.Sha256, ToolEvidenceState.Verified, file.Path),
            });
        }

        var configuration = new ToolObservation(inactive ? ToolEvidenceState.Inactive : ToolEvidenceState.Matching);
        if (!inactive)
        {
            var setting = settings.Tools[row.Id];
            if (setting.CodeRoot is { } codeRoot && codeRoot != receipt.CodeRoot
                || setting.Hosts is { } hosts && !hosts.Order(StringComparer.Ordinal).SequenceEqual(receipt.Hosts.Order(StringComparer.Ordinal)))
                configuration = new(ToolEvidenceState.Drifted, "The settings root or hosts differ from the receipt.");
            foreach (var member in receipt.Members)
            {
                var read = await ReadWorkspaceFileAsync(workspace, member.Path, token).ConfigureAwait(false);
                if (read.State == ToolEvidenceState.Missing) configuration = Worse(configuration, new(ToolEvidenceState.Drifted, member.Path));
                else if (read.State == ToolEvidenceState.Unavailable) configuration = Worse(configuration, new(ToolEvidenceState.Unavailable, read.Cause));
                else if (read.Bytes is { } bytes) configuration = Worse(configuration, ToolHostMemberReader.Compare(member, bytes));
            }
            if (receipt.Members.Length == 0 && receipt.Hosts.Length != 0)
                configuration = Worse(configuration, new(ToolEvidenceState.Unavailable, "The desired hosts have no recorded generated members."));
        }

        var guidance = new ToolObservation(ToolEvidenceState.Inactive);
        if (!inactive && manifest.Skill is { Optional: false })
        {
            if (IsExcluded(settings, GuidancePath)) guidance = new(ToolEvidenceState.Excluded, "The required usage Skill has a covering removal exclusion.");
            else
            {
                var expected = receipt.Files.SingleOrDefault(file => file.StoreId is null && file.Path == GuidancePath);
                var read = await ReadWorkspaceFileAsync(workspace, GuidancePath, token).ConfigureAwait(false);
                guidance = read.State switch
                {
                    ToolEvidenceState.Missing => new(ToolEvidenceState.Missing, "The active role's required usage Skill is missing."),
                    ToolEvidenceState.Unavailable => new(ToolEvidenceState.Unavailable, read.Cause),
                    _ when expected is null => new(ToolEvidenceState.Unavailable, "The usage Skill has no generated hash receipt."),
                    _ => FileHash(read, expected.Sha256, ToolEvidenceState.Matching, GuidancePath),
                };
            }
        }

        // Native adapters add a passive completion reader here. File presence never establishes freshness.
        var index = new ToolObservation(ToolEvidenceState.Unavailable, "The native index requires a verified local binding and a passive adapter reader.");
        if (manifest.Index.Location == "project-directory" && !manifest.Index.Path.Contains("${", StringComparison.Ordinal))
        {
            var path = receipt.CodeRoot == "." ? manifest.Index.Path : receipt.CodeRoot + "/" + manifest.Index.Path;
            var observed = ObserveWorkspacePath(workspace, path, token);
            index = observed switch
            {
                NoFollowLeafState.Missing => new(ToolEvidenceState.NeedsIndex),
                NoFollowLeafState.OrdinaryFile or NoFollowLeafState.Directory => new(ToolEvidenceState.PresentUnverified),
                _ => new(ToolEvidenceState.Unavailable, "The index isn't an accessible ordinary file or directory."),
            };
        }
        return new(row.Id, row.Version, receipt.Target, row.Roles, installation, configuration, index, guidance,
            JsonSerializer.SerializeToElement(manifest.Processes, ToolJsonContext.Default.ToolProcesses),
            [manifest.Index.Freshness, manifest.Qualification.Accuracy]);
    }

    private static bool IsExcluded(WorkspaceSettingsDocument settings, string path)
        => settings.RemovedFiles.Contains(path, StringComparer.Ordinal)
            || settings.RemovedDirectories.Any(directory => path == directory || path.StartsWith(directory + "/", StringComparison.Ordinal))
            || settings.RemovedCategories.Contains("skills", StringComparer.Ordinal);

    private static ToolObservation FileHash(ToolPassiveFile read, string expected, ToolEvidenceState matching, string path)
    {
        if (read.Bytes is not { } bytes) return new(ToolEvidenceState.Unavailable, read.Cause ?? "The file bytes are unavailable.");
        return new(ToolCatalogueReader.Hash(bytes) == expected ? matching : ToolEvidenceState.Changed, path);
    }

    private static ToolObservation Worse(ToolObservation previous, ToolObservation next)
    {
        static int Rank(ToolEvidenceState state) => state switch
        {
            ToolEvidenceState.Unavailable => 4,
            ToolEvidenceState.Changed or ToolEvidenceState.Drifted => 3,
            ToolEvidenceState.Missing => 2,
            _ => 0,
        };
        return Rank(next.State) > Rank(previous.State) ? next : previous;
    }

    internal static NoFollowLeafState ObserveWorkspacePath(CliWorkspace workspace, string relative, CancellationToken token)
    {
        if (!ToolIdentity.IsRelativePath(relative)) return NoFollowLeafState.Unknown;
        var paths = new PhysicalPathResolver();
        var logical = workspace.LexicalRoot;
        var segments = relative.Split('/');
        for (var index = 0; index < segments.Length; index++)
        {
            logical = Path.Combine(logical, segments[index]);
            var read = NoFollowLeafObserver.Observe(paths, workspace, logical, token);
            if (index == segments.Length - 1 || read.State != NoFollowLeafState.Directory) return read.State;
        }
        return NoFollowLeafState.Unknown;
    }

    internal static async ValueTask<ToolPassiveFile> ReadWorkspaceFileAsync(CliWorkspace workspace, string relative, CancellationToken token)
    {
        var observed = ObserveWorkspacePath(workspace, relative, token);
        if (observed == NoFollowLeafState.Missing) return new(ToolEvidenceState.Missing, null, null);
        if (observed != NoFollowLeafState.OrdinaryFile) return new(ToolEvidenceState.Unavailable, null, "The path or an ancestor isn't an ordinary file or directory.");
        try
        {
            var bytes = await File.ReadAllBytesAsync(Path.Combine(workspace.PhysicalRoot, relative.Replace('/', Path.DirectorySeparatorChar)), token).ConfigureAwait(false);
            return ObserveWorkspacePath(workspace, relative, token) == NoFollowLeafState.OrdinaryFile
                ? new(ToolEvidenceState.Complete, bytes, null) : new(ToolEvidenceState.Unavailable, null, "The path changed during the read.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new(ToolEvidenceState.Unavailable, null, "The file couldn't be read.");
        }
    }
}
