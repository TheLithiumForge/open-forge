using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Request;

internal static class RepairPathValidation
{
    internal static string ValidateMarkdown(string path, string parameterName)
    {
        var validated = SourceWorkspaceRelativePath.ValidateMarkdown(path, parameterName);
        return ValidatePortable(validated, parameterName);
    }

    internal static string ValidateTargetPath(string path, string parameterName)
    {
        var validated = SourceWorkspaceRelativePath.Validate(path, parameterName, allowWorkspaceRoot: false);
        if (validated.Contains('#') || validated.Contains('?'))
        {
            throw new ArgumentException(
                "A Repair target path cannot contain an unencoded fragment or query delimiter.",
                parameterName);
        }

        return ValidatePortable(validated, parameterName);
    }

    private static string ValidatePortable(string path, string parameterName)
    {
        if (!PortableWorkspacePath.TryNormalize(path, out var normalized)
            || !string.Equals(path, normalized, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The path must be slash-separated and relative to the selected workspace.",
                parameterName);
        }

        return normalized;
    }
}
