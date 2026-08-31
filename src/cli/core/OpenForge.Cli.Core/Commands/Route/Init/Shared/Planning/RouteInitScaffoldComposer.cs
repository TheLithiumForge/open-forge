using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed record RouteInitComposedScaffold(
    RouteInitMetadata Metadata,
    ImmutableArray<byte> Bytes);

internal sealed class RouteInitScaffoldComposer
{
    private readonly FrameworkMarkdownDocumentWriter _documentWriter = new();

    internal RouteInitComposedScaffold Compose(string id, string title, RouteInitMetadata metadata)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(metadata);
        var body = $"""

            # {title}

            {metadata.Description}.

            ## Axioms

            - inherited - No local axioms; loaded ancestor axioms remain active.

            ## Entries

            <!-- open-forge:generated-index:start -->

            - none - No entries - #Empty

            <!-- open-forge:generated-index:end -->
            """.TrimEnd('\n');
        var bytes = _documentWriter.Write(
            new FrameworkDocumentMetadata(
                description: metadata.Description,
                tags: metadata.Tags,
                responsibility: metadata.Responsibility),
            body);
        return new RouteInitComposedScaffold(
            metadata,
            bytes);
    }
}
