using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Composition;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed record RouteInitComposedScaffold(
    RouteInitMetadata Metadata,
    ImmutableArray<byte> Bytes);

internal sealed class RouteInitScaffoldComposer
{
    private readonly RouteScaffoldComposer _composer = new();

    internal RouteInitComposedScaffold Compose(string id, string title, RouteInitMetadata metadata, FrontmatterForm form)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(metadata);
        var bytes = _composer.Compose(new RouteScaffoldDocument
        {
            Title = title,
            Metadata = new FrameworkDocumentMetadataEmission(
                description: metadata.Description,
                tags: metadata.Tags,
                responsibility: metadata.Responsibility)
            {
                ApplyTo = metadata.ApplyTo,
            },
            Form = form,
        });
        return new RouteInitComposedScaffold(
            metadata,
            bytes);
    }
}
