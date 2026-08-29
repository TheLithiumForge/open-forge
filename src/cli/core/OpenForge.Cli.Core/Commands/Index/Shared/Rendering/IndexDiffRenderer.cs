using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Rendering;

internal static class IndexDiffRenderer
{
    internal static string Render(IndexRegion region)
    {
        var change = region.Change
            ?? throw new ArgumentException("An Index diff requires one changed region.", nameof(region));
        var builder = new StringBuilder();
        builder.AppendLine(
            $"@@ {{\"id\":{JsonString(region.Source.Id)},\"path\":{JsonString(region.Source.Path)},\"scope\":{JsonString(IndexDefinitions.ReadMachineName(region.Source.Scope))}}} @@");
        AppendTokens(builder, '-', change.BeforeBody);
        AppendTokens(builder, '+', change.ExpectedBody);
        return builder.ToString();
    }

    internal static IReadOnlyList<string> Tokenize(string body)
    {
        if (body.Length == 0)
        {
            return [string.Empty];
        }

        var tokens = new List<string>();
        var start = 0;
        for (var index = 0; index < body.Length; index++)
        {
            if (body[index] != '\n')
            {
                continue;
            }

            tokens.Add(body[start..(index + 1)]);
            start = index + 1;
        }

        if (start < body.Length)
        {
            tokens.Add(body[start..]);
        }

        return tokens;
    }

    private static void AppendTokens(StringBuilder builder, char prefix, string body)
    {
        foreach (var token in Tokenize(body))
        {
            builder.Append($"{prefix} {token}");
            if (!token.EndsWith('\n'))
            {
                builder.AppendLine();
            }
        }
    }

    private static string JsonString(string value)
        => $"\"{JsonEncodedText.Encode(value, JavaScriptEncoder.Default)}\"";
}
