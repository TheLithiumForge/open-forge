using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;

internal static class PortableRelativePath
{
    internal static string Validate(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (!PortableWorkspacePath.TryNormalize(value, out var normalized) || normalized != value)
        {
            throw new ArgumentException("A Library path must be a canonical portable relative path.", parameterName);
        }
        return value;
    }
}
