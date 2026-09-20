namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal sealed record ContextCounts
{
    internal ContextCounts(
        long? sources,
        long? tokens,
        long? bytes,
        long? linksFollowed,
        long? linksNotFollowed,
        string? unavailableReason)
    {
        if (sources is < 0 || tokens is < 0 || bytes is < 0
            || linksFollowed is < 0 || linksNotFollowed is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sources), "Context counts cannot be negative.");
        }

        if (unavailableReason is not null && string.IsNullOrWhiteSpace(unavailableReason))
        {
            throw new ArgumentException("A Context count limitation must contain a reason.", nameof(unavailableReason));
        }

        Sources = sources;
        Tokens = tokens;
        Bytes = bytes;
        LinksFollowed = linksFollowed;
        LinksNotFollowed = linksNotFollowed;
        UnavailableReason = unavailableReason;
    }

    internal long? Sources { get; }

    internal long? Tokens { get; }

    internal long? Bytes { get; }

    internal long? LinksFollowed { get; }

    internal long? LinksNotFollowed { get; }

    internal string? UnavailableReason { get; }

    internal static ContextCounts Unavailable(string reason, bool linksNotRequested = false)
        => new(
            sources: null,
            tokens: null,
            bytes: null,
            linksFollowed: linksNotRequested ? 0 : null,
            linksNotFollowed: linksNotRequested ? 0 : null,
            unavailableReason: reason);

    internal static ContextCounts Complete(
        long sources,
        long tokens,
        long bytes,
        long linksFollowed,
        long linksNotFollowed)
        => new(
            sources,
            tokens,
            bytes,
            linksFollowed,
            linksNotFollowed,
            unavailableReason: null);
}

internal enum ContextMetadataState
{
    NotApplicable,
    Complete,
    Missing,
    Malformed,
    Unavailable,
}

internal sealed record ContextSourceMetadata
{
    internal ContextSourceMetadata(
        ContextMetadataState state,
        string? description,
        IEnumerable<string> tags)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Context metadata state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(tags);
        var values = tags
            .Select(value => value ?? throw new ArgumentException("Context metadata tags cannot contain null members.", nameof(tags)))
            .ToArray();
        if (state == ContextMetadataState.Complete)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
        }
        else if (description is not null || values.Length != 0)
        {
            throw new ArgumentException("Only complete Context metadata carries values.");
        }

        State = state;
        Description = description;
        Tags = values;
    }

    internal ContextMetadataState State { get; }

    internal string? Description { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal static ContextSourceMetadata WithoutValues(ContextMetadataState state)
        => new(state, null, []);

    internal static ContextSourceMetadata Complete(string description, IEnumerable<string> tags)
        => new(ContextMetadataState.Complete, description, tags);
}
