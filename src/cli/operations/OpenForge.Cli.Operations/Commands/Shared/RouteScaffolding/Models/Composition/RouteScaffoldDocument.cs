using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Composition;

internal sealed record RouteScaffoldDocument
{
    public required string Title { get; init; }
    public required FrameworkDocumentMetadataEmission Metadata { get; init; }
    public required FrontmatterForm Form { get; init; }
    public bool IncludeDescription { get; init; } = true;
}
