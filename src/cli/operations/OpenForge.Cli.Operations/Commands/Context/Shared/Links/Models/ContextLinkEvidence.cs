using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Links.Models;

internal sealed record ContextAuthoredLinkEvidence
{
    public required ContextGraphSource Source { get; init; }

    public required ContextGraphLayer Layer { get; init; }

    public required MarkdownLinkFact Authored { get; init; }

    public required SourceLocation Location { get; init; }

    public required SourceLocation? DestinationLocation { get; init; }
}

internal sealed record ContextLinkDestinationResolution
{
    public required SourceLinkDestinationFacts Facts { get; init; }

    public required bool CaseMismatch { get; init; }
}
