using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.GitIgnore;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.GitIgnore.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.GitIgnore;

internal static class ToolGitIgnoreSection
{
    internal const string Begin = "# BEGIN OPEN FORGE TOOLS";
    internal const string End = "# END OPEN FORGE TOOLS";
    private static readonly GitIgnoreSectionMarkers Markers = new(Begin, End, "Tool Git-ignore");

    internal static byte[] Rewrite(ReadOnlySpan<byte> bytes, ImmutableArray<string> currentPaths, ImmutableArray<string> intendedPaths)
        => ManagedGitIgnoreSection.Rewrite(Markers, bytes, currentPaths, intendedPaths);
}
