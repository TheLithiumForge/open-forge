using System.Globalization;

namespace OpenForge.Cli.Core.Presentation.Shared.Content;

internal static class ContentPartsWording
{
    internal static string Delimiter(string path, string? id, string? layer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var suffix = string.Empty;
        if (id is not null)
        {
            suffix = layer is "overwrite"
                ? string.Create(CultureInfo.InvariantCulture, $" ({id}, overwrite)")
                : $" ({id})";
        }
        return $"=== {path}{suffix} ===";
    }

    internal static string IncludedBecause(string reason)
        => global::OpenForge.Cli.OutputText.Shared.ContentPartsWording.IncludedBecause(reason);

    internal static string Route(string route)
        => global::OpenForge.Cli.OutputText.Shared.ContentPartsWording.Route(route);

    internal static string Scope(string scope)
        => global::OpenForge.Cli.OutputText.Shared.ContentPartsWording.Scope(scope);

    internal static string Order(int order)
        => global::OpenForge.Cli.OutputText.Shared.ContentPartsWording.Order(order);

    internal static string Layer(string layer)
        => global::OpenForge.Cli.OutputText.Shared.ContentPartsWording.Layer(layer);

    internal static string Heading(string text, int level, int? line)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        if (level < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, "A content heading level must be positive.");
        }

        var prefix = new string('#', level);
        return line is { } lineNumber
            ? string.Create(CultureInfo.InvariantCulture, $"line {lineNumber}: {prefix} {text}")
            : $"{prefix} {text}";
    }

    internal static string Metadata(string name, string value)
        => $"{name}: {value}";
}
