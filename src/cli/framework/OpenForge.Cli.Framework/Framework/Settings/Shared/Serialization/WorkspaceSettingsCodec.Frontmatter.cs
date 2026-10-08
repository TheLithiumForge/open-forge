using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

internal static partial class WorkspaceSettingsCodec
{
    internal static byte[]? SetFrontmatter(ReadOnlyMemory<byte> existing, FrontmatterForm form)
    {
        var name = WorkspaceSettingsDefinitions.ReadFrontmatterName(form);
        JsonNode? root = null;
        if (!existing.IsEmpty)
        {
            var decoded = Read(existing);
            if (decoded.Document is null)
            {
                throw new ArgumentException(decoded.Cause, nameof(existing));
            }

            if (decoded.Document.DeclaredFrontmatter == form)
            {
                return null;
            }

            root = JsonNode.Parse(
                existing.Span,
                documentOptions: new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                });
        }

        root ??= new JsonObject
        {
            [WorkspaceSettingsDefinitions.SchemaProperty] = WorkspaceSettingsDefinitions.SchemaUrl,
            [WorkspaceSettingsDefinitions.SchemaVersionProperty] = WorkspaceSettingsDefinitions.SchemaVersion,
        };

        if (root is not JsonObject settings)
        {
            throw new ArgumentException(
                $"{WorkspaceSettingsDefinitions.FileName} must contain a JSON object.",
                nameof(existing));
        }

        settings[WorkspaceSettingsDefinitions.FrontmatterProperty] = JsonValue.Create(name);
        return Encoding.UTF8.GetBytes(settings.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static FrontmatterForm? ReadDeclaredFrontmatter(JsonElement root)
    {
        if (!root.TryGetProperty(WorkspaceSettingsDefinitions.FrontmatterProperty, out var value))
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw new ArgumentException($"\"{WorkspaceSettingsDefinitions.FrontmatterProperty}\" must be a string.");
        }

        if (!WorkspaceSettingsDefinitions.TryReadFrontmatter(value.GetString(), out var form))
        {
            throw new ArgumentException(
                $"\"{WorkspaceSettingsDefinitions.FrontmatterProperty}\" must be \"{WorkspaceSettingsDefinitions.FrontmatterRootName}\" or \"{WorkspaceSettingsDefinitions.FrontmatterScopedName}\".");
        }

        return form;
    }
}
