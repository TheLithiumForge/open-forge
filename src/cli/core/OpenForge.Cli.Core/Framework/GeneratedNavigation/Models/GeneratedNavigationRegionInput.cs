using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

internal sealed record GeneratedNavigationRegionInput
{
    internal GeneratedNavigationRegionInput(
        SourceLogicalSource source,
        MarkdownDocumentFacts document)
    {
        Source = source;
        Document = document;
    }

    internal GeneratedNavigationRegionInput(
        SourceLogicalSource source,
        string unavailableCause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unavailableCause);

        Source = source;
        UnavailableCause = unavailableCause;
    }

    internal SourceLogicalSource Source { get; }

    internal MarkdownDocumentFacts? Document { get; }

    internal string? UnavailableCause { get; }
}
