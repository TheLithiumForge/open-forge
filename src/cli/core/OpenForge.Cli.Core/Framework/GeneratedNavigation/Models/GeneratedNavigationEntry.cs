using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

internal sealed record GeneratedNavigationEntry
{
    internal GeneratedNavigationEntry(
        SourceLogicalSource source,
        string description,
        string destination,
        IEnumerable<string> tags,
        string line)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentException.ThrowIfNullOrWhiteSpace(line);

        var values = tags
            .Select(tag => tag ?? throw new ArgumentException(
                "Generated navigation tags cannot contain null members.",
                nameof(tags)))
            .ToArray();
        if (values.Length == 0)
        {
            throw new ArgumentException(
                "A generated navigation entry requires at least one tag.",
                nameof(tags));
        }

        Source = source;
        Description = description;
        Destination = destination;
        Tags = new ReadOnlyCollection<string>(values);
        Line = line;
    }

    internal SourceLogicalSource Source { get; }

    internal string CanonicalPath => Source.Identity.CanonicalBasePath;

    internal string PhysicalPath => Source.Base.PhysicalPath;

    internal string Description { get; }

    internal string Destination { get; }

    internal IReadOnlyList<string> Tags { get; }

    internal string Line { get; }
}
