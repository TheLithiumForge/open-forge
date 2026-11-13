using System.Text.RegularExpressions;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

internal static partial class ToolIdentity
{
    internal const string IdPattern = "\\A[a-z0-9][a-z0-9-]{0,127}\\z";
    internal const string VersionPattern = "\\A(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)(?:-((?:0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*)(?:\\.(?:0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*))*))?(?:\\+[0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*)?\\z";

    internal static bool IsId(string? value) => value is not null && Id().IsMatch(value);
    internal static bool IsToolId(string? value) => value != "none" && IsId(value);
    internal static bool IsVersion(string? value) => value is not null && Version().IsMatch(value);
    internal static bool IsSha256(string? value) => value is not null && LowerHash().IsMatch(value);
    internal static bool IsRelativePath(string? value) => PortableWorkspacePath.TryNormalize(value, out _);
    internal static bool IsIgnorePath(string value) => !value.Contains("${", StringComparison.Ordinal) && IsRelativePath(value.EndsWith('/') ? value[..^1] : value);
    internal static bool IsWorkspaceOutputTemplate(string value)
    {
        var relative = value.StartsWith(ToolVocabulary.CodeRootPrefix, StringComparison.Ordinal) ? value[ToolVocabulary.CodeRootPrefix.Length..] : value;
        return !relative.Contains("${", StringComparison.Ordinal) && IsRelativePath(relative);
    }

    [GeneratedRegex(IdPattern, RegexOptions.CultureInvariant)] private static partial Regex Id();
    [GeneratedRegex(VersionPattern, RegexOptions.CultureInvariant)] private static partial Regex Version();
    [GeneratedRegex("\\A[0-9a-f]{64}\\z", RegexOptions.CultureInvariant)] private static partial Regex LowerHash();
}
