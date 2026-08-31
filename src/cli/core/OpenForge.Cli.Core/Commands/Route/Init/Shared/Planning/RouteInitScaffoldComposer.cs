using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed record RouteInitComposedScaffold(
    RouteInitMetadata Metadata,
    ImmutableArray<byte> Bytes);

internal sealed class RouteInitScaffoldComposer
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly FrameworkDocumentMetadataEmitter _metadataEmitter = new();

    internal RouteInitComposedScaffold Compose(string id, string title, RouteInitMetadata metadata)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(metadata);
        var yaml = _metadataEmitter.Emit(new FrameworkDocumentMetadata(
            metadata.Description,
            metadata.Tags,
            metadata.Responsibility));
        var document = "---\n"
            + yaml
            + "---\n\n"
            + $"# {title}\n\n"
            + $"{metadata.Description}.\n\n"
            + "## Axioms\n\n"
            + "- inherited - No local axioms; loaded ancestor axioms remain active.\n\n"
            + "## Entries\n\n"
            + "<!-- open-forge:generated-index:start -->\n\n"
            + "- none - No entries - #Empty\n\n"
            + "<!-- open-forge:generated-index:end -->";
        return new RouteInitComposedScaffold(
            metadata,
            ImmutableArray.CreateRange(StrictUtf8.GetBytes(document)));
    }
}
