using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Framework.Tools.Models.Manifest;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Storage;

internal static class ToolStoreIdentity
{
    internal static ToolStoreBinding Binding(string kind, string physicalRoot)
    {
        physicalRoot = WorkspaceIdentity.NormalizePhysicalPath(physicalRoot);
        var hash = Hash($"{kind}\n{WorkspaceIdentity.Key(physicalRoot)}\n");
        return new(hash, kind, physicalRoot, hash);
    }

    internal static string UnitKey(string kind, string relativeRoot) => Hash($"{kind}\n{relativeRoot}\n");

    internal static string InstallPath(ToolManifest manifest, string target)
    {
        var artifacts = manifest.Targets[target].Lock?.Artifacts ?? [.. manifest.Targets[target].Artifacts.Concat(manifest.Runtime.ExtraDownloads)];
        artifacts = [.. artifacts.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        var digest = artifacts.Length == 1 && manifest.Artifacts[artifacts[0]].Integrity.StartsWith(ToolArtifactIntegrity.Sha256Prefix, StringComparison.Ordinal)
            ? ToolArtifactIntegrity.Key(manifest.Artifacts[artifacts[0]].Integrity)
            : Hash(string.Concat(artifacts.Select(id => $"{id}={manifest.Artifacts[id].Integrity}\n")));
        return $"{manifest.Id}/{manifest.Version}/{target}/{digest}";
    }

    private static string Hash(string value) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
