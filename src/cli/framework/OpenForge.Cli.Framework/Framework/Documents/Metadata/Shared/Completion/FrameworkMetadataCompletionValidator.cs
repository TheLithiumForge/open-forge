using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using YamlDotNet.Core;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;

internal static class FrameworkMetadataCompletionValidator
{
    internal static string? ReadMalformedCause(FrameworkDocumentMetadataFacts metadata, YamlDocumentFacts yaml)
    {
        if (yaml.State != YamlDocumentState.Complete) return "The frontmatter contains invalid YAML.";
        if (ReadMalformedCause(metadata) is { } cause) return cause;
        if (yaml.Root is { Kind: not YamlNodeKind.Mapping } root && !IsEmptyDocument(root))
            return "The frontmatter is not a metadata mapping.";
        if (metadata.Syntax.ScopedEntry is { Value.Kind: not YamlNodeKind.Mapping })
            return "The open-forge declaration is not a metadata mapping.";

        return null;
    }

    internal static string? ReadMalformedCause(FrameworkDocumentMetadataFacts metadata)
    {
        if (metadata.State == FrameworkDocumentMetadataState.Malformed) return "The frontmatter contains malformed or duplicate metadata.";
        if (metadata.ApplyTo.State == ApplyToMetadataState.Invalid) return "The applyTo declaration is invalid or conflicting.";

        foreach (var member in metadata.Syntax.Members)
        {
            var value = member.Entry.Value;
            if (member.Field is FrameworkMetadataField.Description or FrameworkMetadataField.Responsibility)
            {
                if (value.Scalar is not { } scalar || !IsString(scalar) || IsNull(scalar) || string.IsNullOrWhiteSpace(scalar.Value))
                    return $"The supplied {member.Entry.Key.Scalar?.Value} is empty, null or not a scalar.";
            }

            if (member.Field == FrameworkMetadataField.Tags)
            {
                if (value.Sequence is not { Count: > 0 } tags
                    || tags.Any(tag => tag.Scalar is not { } scalar || !IsString(scalar) || IsNull(scalar) || !FrameworkDocumentMetadataTagGrammar.IsValid(scalar.Value))
                    || tags.Select(tag => tag.Scalar?.Value).Distinct(StringComparer.Ordinal).Count() != tags.Count)
                    return "The supplied tags are empty, null, duplicate or invalid.";
            }
        }

        return null;
    }

    internal static bool IsEmptyDocument(YamlNode node)
        => node.Scalar is { Value.Length: 0, Span.Length: 0 };

    private static bool IsString(YamlScalar scalar)
        => scalar.Tag is "" or "?" or "!" or "tag:yaml.org,2002:str" or "!!str" or "!<tag:yaml.org,2002:str>";

    private static bool IsNull(YamlScalar scalar)
        => scalar.Tag is "tag:yaml.org,2002:null" or "!!null" or "!<tag:yaml.org,2002:null>"
            || scalar.Style == ScalarStyle.Plain && scalar.IsPlainImplicit
                && (scalar.Value is "" or "~" || string.Equals(scalar.Value, "null", StringComparison.OrdinalIgnoreCase));
}
