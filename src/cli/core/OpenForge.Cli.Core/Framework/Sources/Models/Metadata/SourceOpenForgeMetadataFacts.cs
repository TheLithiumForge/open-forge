using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Metadata;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

internal enum SourceOpenForgeMetadataState
{
    Complete,
    Missing,
    Malformed,
}

internal sealed record SourceOpenForgeMetadataFacts
{
    internal SourceOpenForgeMetadataFacts(
        SourceOpenForgeMetadataState state,
        string? description,
        IEnumerable<string> tags)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Open Forge source metadata state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(tags);
        var values = tags
            .Select(value => value ?? throw new ArgumentException("Open Forge source metadata tags cannot contain null members.", nameof(tags)))
            .ToArray();
        if (state == SourceOpenForgeMetadataState.Complete)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            if (values.Length == 0 || values.Any(value => !SourceOpenForgeMetadataParser.IsValidTag(value)))
            {
                throw new ArgumentException("Complete Open Forge source metadata requires valid tags.", nameof(tags));
            }
        }
        else if (description is not null || values.Length != 0)
        {
            throw new ArgumentException("Only complete Open Forge source metadata carries authored values.");
        }

        State = state;
        Description = description;
        Tags = new ReadOnlyCollection<string>(values);
    }

    internal SourceOpenForgeMetadataState State { get; }

    internal string? Description { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal static SourceOpenForgeMetadataFacts Complete(string description, IEnumerable<string> tags)
        => new(SourceOpenForgeMetadataState.Complete, description, tags);

    internal static SourceOpenForgeMetadataFacts WithoutValues(SourceOpenForgeMetadataState state)
        => new(state, null, []);
}
