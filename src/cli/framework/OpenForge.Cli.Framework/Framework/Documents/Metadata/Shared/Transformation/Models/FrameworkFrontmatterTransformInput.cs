using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;

internal sealed record FrameworkFrontmatterTransformInput
{
    public required ReadOnlyMemory<byte> Bytes { get; init; }

    public required string Source { get; init; }

    public required MarkdownTextSpan YamlSpan { get; init; }

    public required YamlMappingEntry Scope { get; init; }

    public YamlMappingEntry? RedundantApplyTo { get; init; }
}
