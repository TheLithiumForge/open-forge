namespace OpenForge.Cli.EndToEndTests.Shared.Journeys;

internal static class PublishedJourneyAssertions
{
    internal static void AssertOnlyFileMutations(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        params string[] permittedPaths)
    {
        var permitted = permittedPaths.ToHashSet(StringComparer.Ordinal);
        foreach (var path in before.Keys.Union(after.Keys, StringComparer.Ordinal))
        {
            if (permitted.Contains(path))
            {
                var previousKind = before.TryGetValue(path, out var permittedPrevious)
                    ? OrdinaryEntryKind(permittedPrevious)
                    : null;
                var currentKind = after.TryGetValue(path, out var permittedCurrent)
                    ? OrdinaryEntryKind(permittedCurrent)
                    : null;
                if (previousKind is not null && currentKind is not null)
                {
                    Assert.Equal(previousKind, currentKind);
                }
                continue;
            }

            Assert.True(before.TryGetValue(path, out var previous), $"Unexpected added path: {path}");
            Assert.True(after.TryGetValue(path, out var current), $"Unexpected removed path: {path}");
            if (previous.StartsWith("type=directory;", StringComparison.Ordinal)
                && current.StartsWith("type=directory;", StringComparison.Ordinal)
                && permitted.Any(target => IsParent(path, target)))
            {
                // File creation/replacement can change its containing directory's
                // write time. Keep directory identity/attributes and every other
                // observed field; this allowance never applies to read-only calls.
                Assert.Equal(WithoutWriteTime(previous), WithoutWriteTime(current));
            }
            else
            {
                Assert.Equal(previous, current);
            }
        }
    }

    private static string OrdinaryEntryKind(string description)
    {
        var kind = description.Split(';', 2)[0];
        Assert.True(kind is "type=file" or "type=directory", $"Expected an ordinary mutation target: {description}");
        return kind;
    }

    private static bool IsParent(string directory, string target)
        => directory == "."
            || (directory.EndsWith("/.", StringComparison.Ordinal)
                ? target.StartsWith(directory[..^1], StringComparison.Ordinal)
                : target.StartsWith(directory + "/", StringComparison.Ordinal));

    private static string WithoutWriteTime(string description)
        => string.Join(';', description.Split(';').Where(
            field => !field.StartsWith("lastWriteUtcTicks=", StringComparison.Ordinal)));
}
