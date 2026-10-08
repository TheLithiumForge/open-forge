using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;

internal sealed record FrameworkMetadataMemberSyntax
{
    public required FrameworkMetadataField Field { get; init; }

    public required FrontmatterForm Form { get; init; }

    public required YamlMappingEntry Entry { get; init; }

    internal YamlTextSpan KeySpan => Entry.Key.Span;

    internal YamlTextSpan ValueSpan => Entry.Value.Span;

    public required ImmutableArray<YamlTextSpan> TagItemSpans { get; init; }
}
