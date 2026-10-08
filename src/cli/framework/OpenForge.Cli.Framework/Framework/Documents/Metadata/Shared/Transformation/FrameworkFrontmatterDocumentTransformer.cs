using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation;

internal static class FrameworkFrontmatterDocumentTransformer
{
    // The Frontmatter Form section of .agents/memory/crystallized/documents/cli/shared-operation-contract.md defines payload rendering.
    private const string ScopeKey = "open-forge";
    private const string ApplyToKey = "applyTo";
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static FrameworkFrontmatterTransformResult Transform(ReadOnlyMemory<byte> document, FrontmatterForm targetForm)
        => targetForm switch
        {
            FrontmatterForm.Scoped => FrameworkFrontmatterTransformResult.Unchanged(document),
            FrontmatterForm.Root => TransformRoot(document),
            _ => throw new ArgumentOutOfRangeException(nameof(targetForm), targetForm, "The frontmatter form is not defined."),
        };

    private static FrameworkFrontmatterTransformResult TransformRoot(ReadOnlyMemory<byte> document)
    {
        string source;
        try
        {
            source = StrictUtf8.GetString(document.Span);
        }
        catch (DecoderFallbackException)
        {
            return FrameworkFrontmatterTransformResult.Invalid("The document is not valid UTF-8.");
        }

        var boundary = new MarkdownFrontmatterParser().Parse(source);
        if (boundary.State == MarkdownFrontmatterState.Missing)
        {
            return FrameworkFrontmatterTransformResult.Unchanged(document);
        }

        if (boundary.YamlSpan is not { } yamlSpan)
        {
            return FrameworkFrontmatterTransformResult.Invalid("The leading frontmatter boundary is unavailable.");
        }

        var yaml = new YamlDocumentParser().Parse(source[yamlSpan.Start..yamlSpan.End]);
        if (yaml.State != YamlDocumentState.Complete)
        {
            return FrameworkFrontmatterTransformResult.Invalid("The leading frontmatter is not valid YAML.");
        }

        if (yaml.Root?.Mapping is not { } root)
        {
            return FrameworkFrontmatterTransformResult.Unchanged(document);
        }

        var scopes = root.Where(entry => IsKey(entry, ScopeKey)).ToArray();
        if (scopes.Length == 0)
        {
            return FrameworkFrontmatterTransformResult.Unchanged(document);
        }

        if (scopes.Length != 1)
        {
            return FrameworkFrontmatterTransformResult.Invalid("The frontmatter has more than one open-forge key.");
        }

        var scope = scopes[0];
        if (scope.Value.Mapping is not { } members
            || root[0].Key.Span.Start != yaml.Root.Span.Start
            || members.Count > 0 && members[0].Key.Span.Start != scope.Value.Span.Start
            || members.Count == 0 && !yaml.Source.AsSpan(scope.Value.Span.Start, scope.Value.Span.Length).SequenceEqual("{}"))
        {
            return FrameworkFrontmatterTransformResult.Invalid("The open-forge value requires a block mapping or the canonical empty mapping.");
        }

        if (root.Any(entry => entry.Key.Scalar?.Value is "description" or "responsibility" or "tags"))
        {
            return FrameworkFrontmatterTransformResult.Invalid("Foreign root metadata collides with the Open Forge metadata.");
        }

        YamlMappingEntry? redundantApplyTo = null;
        if (root.Any(entry => IsKey(entry, ApplyToKey)) && members.Any(entry => IsKey(entry, ApplyToKey)))
        {
            var applicability = ApplyToMetadataReader.Read(yaml);
            if (applicability.State != ApplyToMetadataState.Valid)
            {
                return FrameworkFrontmatterTransformResult.Invalid("The root and scoped applyTo declarations are invalid or conflicting.");
            }

            redundantApplyTo = members.Single(entry => IsKey(entry, ApplyToKey));
        }

        return FrameworkFrontmatterByteEditor.Flatten(new FrameworkFrontmatterTransformInput
        {
            Bytes = document,
            Source = source,
            YamlSpan = yamlSpan,
            Scope = scope,
            RedundantApplyTo = redundantApplyTo,
        });
    }

    private static bool IsKey(YamlMappingEntry entry, string key)
        => string.Equals(entry.Key.Scalar?.Value, key, StringComparison.Ordinal);
}
