using System.Globalization;
using System.Text;

namespace OpenForge.Cli.Core.Presentation.Shared.Text;

internal static class CliText
{
    internal static string Escape(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var builder = new StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            switch (character)
            {
                case '\n': builder.Append(@"\n"); break;
                case '\r': builder.Append(@"\r"); break;
                case '\t': builder.Append(@"\t"); break;
                default:
                    if (char.IsHighSurrogate(character) && index + 1 < value.Length && char.IsLowSurrogate(value[index + 1]))
                    {
                        builder.Append(character).Append(value[++index]);
                    }
                    else if (char.IsControl(character) || char.IsSurrogate(character))
                    {
                        builder.Append(CultureInfo.InvariantCulture, $"\\u{(int)character:X4}");
                    }
                    else
                    {
                        builder.Append(character);
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    internal static string Clamp(string value, int maximum)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegative(maximum);
        if (value.Length <= maximum)
        {
            return value;
        }

        if (maximum < 3)
        {
            return new string('.', maximum);
        }

        var end = maximum - 3;
        if (end > 0 && char.IsHighSurrogate(value[end - 1]) && char.IsLowSurrogate(value[end]))
        {
            end--;
        }

        return value[..end] + "...";
    }

    internal static string Plural(long count, string singular, string? plural = null)
        => count == 1 ? singular : plural ?? singular + "s";

    internal static string Tokens(long count)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatAboutKTokens(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{count / 1000.0:0.0}"));

    internal static string Bytes(long count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        string[] units = ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"];
        var unit = 0;
        var value = (double)count;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0
            ? string.Create(CultureInfo.InvariantCulture, $"{count} B")
            : string.Create(CultureInfo.InvariantCulture, $"{value:0.0} {units[unit]}");
    }

    internal static string PlatformLineEndings(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace("\n", Environment.NewLine, StringComparison.Ordinal);
    }
}
