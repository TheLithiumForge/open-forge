using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;

internal static class PortableRelativePath
{
    internal static string Validate(
        string value,
        string parameterName,
        bool requireAgentsPrefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (Path.IsPathFullyQualified(value)
            || value.Contains((char)92)
            || value.StartsWith('/')
            || value.EndsWith('/'))
        {
            throw new ArgumentException(
                "A workspace-relative path must be a slash-separated relative path.",
                parameterName);
        }

        var segments = value.Split('/');
        if (segments.Any(segment => segment is "" or "." or ".."))
        {
            throw new ArgumentException(
                "A workspace-relative path cannot contain empty, dot, or dot-dot segments.",
                parameterName);
        }

        if (requireAgentsPrefix
            && (segments.Length < 2
                || !string.Equals(segments[0], ".agents", StringComparison.Ordinal)))
        {
            throw new ArgumentException(
                "An eligible Library path must be beneath .agents/.",
                parameterName);
        }

        return value;
    }
}
