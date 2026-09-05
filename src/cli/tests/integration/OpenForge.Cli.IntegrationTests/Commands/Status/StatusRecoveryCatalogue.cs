using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal static class StatusRecoveryCatalogue
{
    internal static string CandidatePath(
        string workspaceDirectory,
        Guid operationId,
        RecoveryBundleCandidateKind kind)
        => Path.Combine(
            workspaceDirectory,
            kind == RecoveryBundleCandidateKind.Final
                ? RecoveryBundleFormatV1.FinalFileName(operationId)
                : RecoveryBundleFormatV1.DraftFileName(operationId));

    internal static IReadOnlyDictionary<string, string> SnapshotEntries(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return new SortedDictionary<string, string>(StringComparer.Ordinal);
        }

        var snapshot = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var path in Directory.EnumerateFileSystemEntries(
                     directory,
                     "*",
                     SearchOption.TopDirectoryOnly)
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            var attributes = File.GetAttributes(path);
            var relativePath = Path.GetRelativePath(directory, path).Replace('\\', '/');
            snapshot[relativePath] = (attributes & FileAttributes.Directory) != 0
                ? "directory"
                : Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        }

        return snapshot;
    }

    internal static void DeleteExact(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: false);
        }
    }

    internal static void DeleteEmptyDirectory(string path)
    {
        if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path);
        }
    }
}
