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
                Assert.True(DirectoryMetadataMatchesAfterChildMutation(previous, current), $"Directory metadata changed: {path}");
            }
            else
            {
                Assert.Equal(previous, current);
            }
        }
    }

    internal static bool DirectoryMetadataMatchesAfterChildMutation(string previous, string current)
    {
        if (!previous.StartsWith("type=directory;", StringComparison.Ordinal)
            || !current.StartsWith("type=directory;", StringComparison.Ordinal)) return false;

        // Creating/replacing children changes the parent write time. On Unix,
        // runtimes without birth-time support also report that value as creation
        // time. Exclude it only when both observations identify that fallback.
        // Callers must first establish that child mutation is permitted here.
        var creationIsWriteTime = !OperatingSystem.IsWindows()
            && CreationIsWriteTime(previous) && CreationIsWriteTime(current);
        return WithoutMutableTimes(previous, creationIsWriteTime) == WithoutMutableTimes(current, creationIsWriteTime);
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

    private static bool CreationIsWriteTime(string description)
    {
        var fields = description.Split(';');
        return fields.Single(field => field.StartsWith("creationUtcTicks=", StringComparison.Ordinal)).Split('=')[1]
            == fields.Single(field => field.StartsWith("lastWriteUtcTicks=", StringComparison.Ordinal)).Split('=')[1];
    }

    private static string WithoutMutableTimes(string description, bool creationIsWriteTime)
        => string.Join(';', description.Split(';').Where(
            field => !field.StartsWith("lastWriteUtcTicks=", StringComparison.Ordinal)
                && !(creationIsWriteTime && field.StartsWith("creationUtcTicks=", StringComparison.Ordinal))));
}
