using System.Security.Cryptography;
using System.Text;

namespace OpenForge.Cli.Core.Framework.Workspace;

internal static class WorkspaceIdentity
{
    internal static string NormalizePhysicalPath(string physicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        if (!Path.IsPathFullyQualified(physicalPath))
        {
            throw new ArgumentException(
                "A workspace physical path must be fully qualified.",
                nameof(physicalPath));
        }

        var fullPath = Path.GetFullPath(physicalPath);
        var normalized = Path.TrimEndingDirectorySeparator(fullPath);
        return string.IsNullOrEmpty(normalized)
            ? Path.GetPathRoot(fullPath)
                ?? throw new ArgumentException(
                    "A workspace physical path requires a rooted identity.",
                    nameof(physicalPath))
            : normalized;
    }

    internal static string Key(string physicalPath)
    {
        var normalized = NormalizePhysicalPath(physicalPath);
        var identity = OperatingSystem.IsWindows()
            ? normalized.ToUpperInvariant()
            : normalized;
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }
}
