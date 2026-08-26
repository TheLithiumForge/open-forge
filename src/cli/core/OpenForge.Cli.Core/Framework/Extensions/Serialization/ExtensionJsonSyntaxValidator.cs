using System.Text.Json;

namespace OpenForge.Cli.Core.Framework.Extensions.Serialization;

internal static class ExtensionJsonSyntaxValidator
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
            }
            else if (reader.TokenType == JsonTokenType.EndObject)
            {
                _ = objectProperties.Pop();
            }
            else if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var property = reader.GetString()
                    ?? throw new JsonException("A JSON property name cannot be null.");
                if (objectProperties.Count == 0 || !objectProperties.Peek().Add(property))
                {
                    throw new JsonException($"The JSON property '{property}' is duplicated.");
                }
            }
        }
    }
}
