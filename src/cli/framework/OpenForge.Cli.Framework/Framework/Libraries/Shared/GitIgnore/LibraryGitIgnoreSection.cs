using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.GitIgnore;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.GitIgnore.Models;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.GitIgnore;

internal static class LibraryGitIgnoreSection
{
    internal const string Begin = "# BEGIN OPEN FORGE LIBRARIES";
    internal const string End = "# END OPEN FORGE LIBRARIES";
    private static readonly GitIgnoreSectionMarkers Markers = new(Begin, End, "Library Git-ignore");

    internal static byte[] Rewrite(ReadOnlySpan<byte> bytes, ImmutableArray<string> currentPaths, ImmutableArray<string> intendedPaths)
        => ManagedGitIgnoreSection.Rewrite(Markers, bytes, currentPaths, intendedPaths);
}
