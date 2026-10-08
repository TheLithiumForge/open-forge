using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;

internal sealed record FrameworkDocumentMetadataSyntax
{
    internal static FrameworkDocumentMetadataSyntax Absent { get; } = new();

    internal FrontmatterForm? AuthoredForm { get; init; }

    internal YamlMappingEntry? ScopedEntry { get; init; }

    internal YamlTextSpan? ScopedKeySpan => ScopedEntry?.Key.Span;

    internal YamlTextSpan? ScopedValueSpan => ScopedEntry?.Value.Span;

    internal YamlNode? SelectedMapping { get; init; }

    internal ImmutableArray<FrameworkMetadataMemberSyntax> Members { get; init; } = [];
}
