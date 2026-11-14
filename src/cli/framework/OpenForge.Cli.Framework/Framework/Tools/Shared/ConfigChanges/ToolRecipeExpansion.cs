using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

internal static class ToolRecipeExpansion
{
    internal static void Validate(ToolRecipeContext context)
    {
        foreach (var path in new[] { context.WorkspacePath, context.HomePath, context.BinPath, context.NativeHomePath, context.NativeInstallDirectory }.Concat(context.ClientRoots.Values))
            if (!Path.IsPathFullyQualified(path) || path.Contains('\0')) throw new InvalidDataException("Recipe roots must be resolved absolute paths.");
        if (!PortableWorkspacePath.TryNormalize(context.MainRoot, out _)) throw new InvalidDataException("The recipe main root must be a portable relative path.");
        foreach (var key in context.ClientRoots.Keys)
            if (!IsClientKey(key))
                throw new InvalidDataException("A recipe client key is invalid.");
    }

    internal static string Expand(string text, ToolRecipeContext context, string? previous = null)
        => ExpandAvailable(text, context, previous) ?? throw new InvalidDataException("A recipe token is unknown or its value is unavailable.");

    internal static string? ExpandAvailable(string text, ToolRecipeContext context, string? previous = null)
    {
        var output = new StringBuilder();
        var unavailable = false;
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] != '$' || index + 1 == text.Length) { output.Append(text[index]); continue; }
            if (text.AsSpan(index).StartsWith(ToolRecipeDefinitions.DollarEscapeToken, StringComparison.Ordinal)) { output.Append('$'); index++; continue; }
            if (text[index + 1] != '{') { output.Append('$'); continue; }
            var end = text.IndexOf('}', index + 2);
            if (end < 0) throw new InvalidDataException("A recipe token is incomplete.");
            var token = text[index..(end + 1)];
            var value = token switch
            {
                ToolRecipeDefinitions.WorkspaceToken => context.WorkspacePath,
                ToolRecipeDefinitions.HomeToken => context.HomePath,
                ToolRecipeDefinitions.BinToken => context.BinPath,
                ToolRecipeDefinitions.NativeHomeToken => context.NativeHomePath,
                ToolRecipeDefinitions.NativeInstallDirectoryToken => context.NativeInstallDirectory,
                ToolRecipeDefinitions.MainRootToken => context.MainRoot,
                ToolRecipeDefinitions.PreviousValueToken when previous is not null => previous,
                _ => Client(token, context),
            };
            if (value is null) unavailable = true;
            else
            {
                if (value.Contains('\0')) throw new InvalidDataException("A recipe value contains a null character.");
                output.Append(value);
            }
            index = end;
        }
        var expanded = output.ToString();
        if (expanded.Contains('\0')) throw new InvalidDataException("A recipe contains a null character.");
        return unavailable ? null : expanded;
    }

    internal static ToolConfigChange Change(ToolConfigChange change, ToolRecipeContext context) => change switch
    {
        ToolJsonValueChange json => json with { PathTemplate = Expand(json.PathTemplate, context), Value = json.Value is { } value ? Json(value, context) : null },
        ToolMarkedBlockChange block => block with
        {
            PathTemplate = Expand(block.PathTemplate, context),
            ContentTemplate = Expand(block.ContentTemplate, context),
            WhenNotIgnored = block.WhenNotIgnored is { } condition ? new(Expand(condition.WorkspaceRelativePath, context)) : null
        },
        ToolManagedFileChange file => file with { PathTemplate = Expand(file.PathTemplate, context) },
        ToolNativeConfigChange native => native,
        _ => throw new ArgumentOutOfRangeException(nameof(change), change, "The config recipe kind is unsupported."),
    };

    internal static JsonElement Json(JsonElement source, ToolRecipeContext context)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream)) Write(writer, source, context);
        using var result = JsonDocument.Parse(stream.ToArray());
        return result.RootElement.Clone();
    }

    private static string? Client(string token, ToolRecipeContext context)
    {
        if (token.StartsWith(ToolRecipeDefinitions.ClientTokenPrefix, StringComparison.Ordinal))
        {
            var key = token[ToolRecipeDefinitions.ClientTokenPrefix.Length..^1];
            if (IsClientKey(key)) return context.ClientRoots.GetValueOrDefault(key);
        }
        throw new InvalidDataException("A recipe token is unknown or its value is unavailable.");
    }

    private static bool IsClientKey(string key)
        => key.Length > 0 && key[0] is >= 'a' and <= 'z' && key.All(character => character is >= 'a' and <= 'z' or >= '0' and <= '9' or '-');

    private static void Write(Utf8JsonWriter writer, JsonElement value, ToolRecipeContext context)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String: writer.WriteStringValue(Expand(value.GetString() ?? string.Empty, context)); break;
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in value.EnumerateObject()) { writer.WritePropertyName(property.Name); Write(writer, property.Value, context); }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in value.EnumerateArray()) Write(writer, item, context);
                writer.WriteEndArray();
                break;
            default: value.WriteTo(writer); break;
        }
    }
}
