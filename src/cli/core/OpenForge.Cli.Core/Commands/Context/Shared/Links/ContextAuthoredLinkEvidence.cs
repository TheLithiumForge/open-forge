using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Links;

internal sealed record ContextAuthoredLinkEvidence
{
    public required ContextGraphSource Source { get; init; }

    public required ContextGraphLayer Layer { get; init; }

    public required MarkdownLinkFact Authored { get; init; }

    public required SourceLocation Location { get; init; }

    public required SourceLocation? DestinationLocation { get; init; }
}
