using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;

internal sealed class RouteInspectGeneratedEntries
{
    private RouteInspectGeneratedEntries(
        bool isAvailable,
        IEnumerable<RouteInspectGeneratedEntry> entries,
        string? reason)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (isAvailable && reason is not null
            || !isAvailable && string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Generated Entries availability fields do not match.");
        }

        IsAvailable = isAvailable;
        Entries = new ReadOnlyCollection<RouteInspectGeneratedEntry>(entries.ToArray());
        Reason = reason;
    }

    internal bool IsAvailable { get; }

    internal IReadOnlyList<RouteInspectGeneratedEntry> Entries { get; }

    internal string? Reason { get; }

    internal string ReadReason()
    {
        if (IsAvailable || Reason is not { } reason)
        {
            throw new InvalidOperationException("Unavailable generated Entries require a reason.");
        }

        return reason;
    }

    internal static RouteInspectGeneratedEntries Available(
        IEnumerable<RouteInspectGeneratedEntry> entries)
    {
        return new RouteInspectGeneratedEntries(true, entries, null);
    }

    internal static RouteInspectGeneratedEntries Unavailable(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        return new RouteInspectGeneratedEntries(false, [], reason);
    }
}

internal sealed class RouteInspectGeneratedEntry
{
    internal RouteInspectGeneratedEntry(
        string destination,
        IEnumerable<string> tags)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(tags);
        var materializedTags = tags.ToArray();
        if (materializedTags.Any(tag => string.IsNullOrWhiteSpace(tag)))
        {
            throw new ArgumentException("Generated entry tags cannot be empty.", nameof(tags));
        }

        Destination = destination;
        Tags = new ReadOnlyCollection<string>(materializedTags);
    }

    internal string Destination { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal bool HasTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        return Tags.Contains($"#{tag}", StringComparer.Ordinal);
    }
}
