using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Metadata;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

internal enum SourceAuthoredMetadataState
{
    NotApplicable,
    Complete,
    Missing,
    Malformed,
}

internal sealed record SourceAuthoredMetadataFacts
{
    private SourceAuthoredMetadataFacts(
        SourceAuthoredMetadataState state,
        string? description,
        IEnumerable<string> tags)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The authored source metadata state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(tags);
        var values = tags
            .Select(value => value ?? throw new ArgumentException("Authored source metadata tags cannot contain null members.", nameof(tags)))
            .ToArray();
        if (state == SourceAuthoredMetadataState.Complete)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            if (values.Any(value => !SourceOpenForgeMetadataParser.IsValidTag(value)))
            {
                throw new ArgumentException("Complete authored source metadata requires valid tags.", nameof(tags));
            }
        }
        else if (description is not null || values.Length != 0)
        {
            throw new ArgumentException("Only complete authored source metadata carries authored values.");
        }

        State = state;
        Description = description;
        Tags = new ReadOnlyCollection<string>(values);
    }

    internal SourceAuthoredMetadataState State { get; }

    internal string? Description { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal static SourceAuthoredMetadataFacts Complete(string description, IEnumerable<string> tags)
        => new(SourceAuthoredMetadataState.Complete, description, tags);

    internal static SourceAuthoredMetadataFacts WithoutValues(SourceAuthoredMetadataState state)
        => new(state, null, []);
}
