using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Loading;

internal enum SourceGeneratedEntriesState
{
    Complete,
    Absent,
    Unavailable,
}

internal sealed record SourceGeneratedEntry
{
    internal SourceGeneratedEntry(string destination, IEnumerable<string> tags)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(tags);
        var values = tags
            .Select(value => value ?? throw new ArgumentException("Generated-entry tags cannot contain null members.", nameof(tags)))
            .ToArray();
        Destination = destination;
        Tags = new ReadOnlyCollection<string>(values);
    }

    internal string Destination { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal bool HasTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        return Tags.Contains(tag, StringComparer.Ordinal);
    }
}

internal sealed record SourceGeneratedEntriesFacts
{
    internal SourceGeneratedEntriesFacts(
        SourceGeneratedEntriesState state,
        IEnumerable<SourceGeneratedEntry> entries,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The generated Entries state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(entries);
        var values = entries
            .Select(value => value ?? throw new ArgumentException("Generated Entries cannot contain null members.", nameof(entries)))
            .ToArray();
        if (state == SourceGeneratedEntriesState.Complete && cause is not null
            || state == SourceGeneratedEntriesState.Absent && (values.Length != 0 || cause is not null)
            || state == SourceGeneratedEntriesState.Unavailable && (values.Length != 0 || string.IsNullOrWhiteSpace(cause)))
        {
            throw new ArgumentException("Generated Entries facts do not match their state.");
        }

        State = state;
        Entries = new ReadOnlyCollection<SourceGeneratedEntry>(values);
        Cause = cause;
    }

    internal SourceGeneratedEntriesState State { get; }

    internal IReadOnlyList<SourceGeneratedEntry> Entries { get; }

    internal string? Cause { get; }

    internal static SourceGeneratedEntriesFacts Complete(IEnumerable<SourceGeneratedEntry> entries)
        => new(SourceGeneratedEntriesState.Complete, entries, null);

    internal static SourceGeneratedEntriesFacts Absent { get; } = new(SourceGeneratedEntriesState.Absent, [], null);

    internal static SourceGeneratedEntriesFacts Unavailable(string cause)
        => new(SourceGeneratedEntriesState.Unavailable, [], cause);
}
