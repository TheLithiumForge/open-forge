using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Shared.Entries.Models;

internal sealed record MarkdownEntryRow
{
    internal MarkdownEntryRow(
        string description,
        string destination,
        IEnumerable<string> tags,
        IEnumerable<ApplyToPattern>? applyTo = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(tags);
        var tagValues = tags.ToArray();
        if (tagValues.Any(string.IsNullOrEmpty))
        {
            throw new ArgumentException("Markdown entry tags cannot be empty.", nameof(tags));
        }

        var patternValues = (applyTo ?? []).ToArray();
        if (patternValues.Any(pattern => pattern is null))
        {
            throw new ArgumentException("Markdown entry patterns cannot contain null members.", nameof(applyTo));
        }

        Description = description;
        Destination = destination;
        Tags = new ReadOnlyCollection<string>(tagValues);
        ApplyTo = new ReadOnlyCollection<ApplyToPattern>(patternValues);
    }

    internal string Description { get; }

    internal string Destination { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal IReadOnlyList<ApplyToPattern> ApplyTo { get; }
}
