using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal static class SourceReferenceParser
{
    internal static SourceReferenceParseResult Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        const string agentsPrefix = ".agents/";
        const string dotAgentsPrefix = "./.agents/";
        if (value.StartsWith(agentsPrefix, StringComparison.Ordinal)
            || value.StartsWith(dotAgentsPrefix, StringComparison.Ordinal))
        {
            var attemptedPath = value.StartsWith(dotAgentsPrefix, StringComparison.Ordinal)
                ? agentsPrefix + value[dotAgentsPrefix.Length..]
                : value;
            return SourceLogicalPath.IsCanonicalSource(attemptedPath)
                ? SourceReferenceParseResult.ValidPath(attemptedPath)
                : SourceReferenceParseResult.InvalidPath(
                    attemptedPath,
                    "An exact source path must contain non-empty .agents segments without traversal or control characters.");
        }

        if (value.StartsWith("./", StringComparison.Ordinal))
        {
            var path = value[2..];
            return SourceWorkspaceRelativePath.IsCanonical(path)
                ? SourceReferenceParseResult.ValidPath(path)
                : SourceReferenceParseResult.InvalidPath(path, "An exact source path must stay inside the workspace without traversal or control characters.");
        }

        return SourceIdentity.IsValidId(value)
            ? SourceReferenceParseResult.ValidId(value)
            : SourceReferenceParseResult.InvalidId(
                value,
                "A source ID must contain non-empty path segments without traversal or control characters.");
    }
}
