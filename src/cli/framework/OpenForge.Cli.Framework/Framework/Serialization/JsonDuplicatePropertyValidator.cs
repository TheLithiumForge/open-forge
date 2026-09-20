using System.Text.Json;

namespace OpenForge.Cli.Core.Framework.Serialization;

/// <summary>
/// Performs the narrow token-level duplicate-property check shared by strict JSON readers.
/// Semantic decoding remains owned by each source-generated JSON context.
/// </summary>
internal static class JsonDuplicatePropertyValidator
{
    internal static void ValidateNoDuplicateProperties(ReadOnlySpan<byte> utf8Json)
        => ValidateNoDuplicateProperties(utf8Json, ignoredRootProperty: null);

    internal static void ValidateNoDuplicateProperties(
        ReadOnlySpan<byte> utf8Json,
        string? ignoredRootProperty)
    {
        if (ignoredRootProperty is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ignoredRootProperty);
        }

        var reader = new Utf8JsonReader(
            utf8Json,
            new JsonReaderOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
            });
        var objectProperties = new Stack<HashSet<string>>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                objectProperties.Push(new HashSet<string>(StringComparer.Ordinal));
                continue;
            }

            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (objectProperties.Count == 0)
                {
                    throw new JsonException("The JSON object structure is invalid.");
                }

                _ = objectProperties.Pop();
                continue;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            if (objectProperties.Count == 0)
            {
                throw new JsonException("A JSON property must be contained in an object.");
            }

            var property = reader.GetString()
                ?? throw new JsonException("A JSON property name cannot be null.");
            if (!objectProperties.Peek().Add(property))
            {
                throw new JsonException($"The JSON property '{property}' is duplicated.");
            }

            if (objectProperties.Count == 1
                && string.Equals(property, ignoredRootProperty, StringComparison.Ordinal))
            {
                if (!reader.Read())
                {
                    throw new JsonException($"The JSON property '{property}' has no value.");
                }

                if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
                {
                    reader.Skip();
                }
            }
        }
    }
}
