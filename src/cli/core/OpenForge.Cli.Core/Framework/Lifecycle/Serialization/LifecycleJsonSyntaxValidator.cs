using System.Text.Json;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Serialization;

internal static class LifecycleJsonSyntaxValidator
{
    internal static void ValidateNoDuplicateProperties(ReadOnlySpan<byte> utf8Json)
    {
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
                _ = objectProperties.Pop();
                continue;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var property = reader.GetString()
                    ?? throw new JsonException("A JSON property name cannot be null.");
                if (objectProperties.Count == 0 || !objectProperties.Peek().Add(property))
                {
                    throw new JsonException($"The JSON property '{property}' is duplicated.");
                }

                if (objectProperties.Count == 1
                    && string.Equals(property, "framework", StringComparison.Ordinal))
                {
                    if (!reader.Read())
                    {
                        throw new JsonException("The framework property has no value.");
                    }

                    if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
                    {
                        reader.Skip();
                    }
                }
            }
        }
    }
}
