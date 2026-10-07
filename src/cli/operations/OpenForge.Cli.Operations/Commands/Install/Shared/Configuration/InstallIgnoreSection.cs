using OpenForge.Cli.Core.Framework.Sources.Models.Sharing;
using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

internal static class InstallIgnoreSection
{
    internal const string Path = ".gitignore";
    internal const string Begin = "# BEGIN OPEN FORGE INSTALL";
    internal const string End = "# END OPEN FORGE INSTALL";
    private static readonly UTF8Encoding Utf8 = new(false, true);

    internal static ImmutableArray<string> Read(ReadOnlySpan<byte> bytes)
    {
        var section = Locate(Utf8.GetString(bytes));
        if (section.Start < 0) return [];
        var lines = section.Body.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var routes = ImmutableArray.CreateBuilder<string>();
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            var id = InstallConfigurationChoices.RouteIds.FirstOrDefault(id => Pattern(id) == line || ContentsPattern(id) == line)
                ?? throw new InvalidDataException("The Install Git-ignore section contains an unsupported pattern.");
            if (line == ContentsPattern(id))
            {
                if (++index >= lines.Length || !lines[index].StartsWith("!/", StringComparison.Ordinal))
                    throw new InvalidDataException("The Install Git-ignore section requires a shared entrypoint after each contents pattern.");
                var entrypoint = lines[index][2..];
                if (SourceLogicalPath.ReadParent(entrypoint) != InstallConfigurationChoices.Directory(id)
                    || !SourceFormClassifier.TryClassify(entrypoint, out var form) || !SourceFormClassifier.IsEntrypoint(form))
                    throw new InvalidDataException("The Install Git-ignore section contains an unsupported shared entrypoint.");
            }
            routes.Add(id);
        }
        return routes.Distinct(StringComparer.Ordinal).ToImmutableArray();
    }

    internal static byte[] Rewrite(ReadOnlySpan<byte> bytes, ImmutableArray<InstallRouteSelection> routes,
        IReadOnlyList<SourceSharingRoute>? sharedRoutes = null)
    {
        var text = Utf8.GetString(bytes);
        _ = Read(bytes);
        var section = Locate(text);
        var lineEnding = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var patterns = routes.Where(row => row.Action == InstallRouteAction.GitIgnore)
            .SelectMany(row => new[] { ContentsPattern(row.Id), $"!/{sharedRoutes?.Single(route => route.Directory == InstallConfigurationChoices.Directory(row.Id)).Entrypoint
                ?? InstallConfigurationChoices.Entrypoint(row.Id)}" }).ToArray();
        var replacement = patterns.Length == 0 ? string.Empty
            : string.Join(lineEnding, new[] { Begin }.Concat(patterns).Append(End)) + lineEnding;
        if (section.Start >= 0)
            return Utf8.GetBytes(text[..section.Start] + replacement + text[section.EndExclusive..]);
        if (replacement.Length == 0) return bytes.ToArray();
        var separator = text.Length == 0 || text.EndsWith('\n') ? string.Empty : lineEnding;
        return Utf8.GetBytes(text + separator + replacement);
    }

    private static string Pattern(string id) => $"/{InstallConfigurationChoices.Directory(id)}/";
    private static string ContentsPattern(string id) => $"/{InstallConfigurationChoices.Directory(id)}/*";

    private static InstallIgnoreSectionSpan Locate(string text)
    {
        var start = -1;
        var end = -1;
        var bodyStart = -1;
        var bodyEnd = -1;
        var offset = 0;
        foreach (var line in text.Split('\n'))
        {
            var content = line.TrimEnd('\r');
            if (content.Contains(Begin, StringComparison.Ordinal) || content.Contains(End, StringComparison.Ordinal))
            {
                if (content == Begin && start < 0 && end < 0)
                {
                    start = offset;
                    bodyStart = Math.Min(text.Length, offset + line.Length + 1);
                }
                else if (content == End && start >= 0 && end < 0)
                {
                    bodyEnd = offset;
                    end = Math.Min(text.Length, offset + line.Length + 1);
                }
                else throw new InvalidDataException("The Install Git-ignore section has ambiguous or duplicate markers.");
            }
            offset += line.Length + 1;
        }
        if (start >= 0 && end < 0) throw new InvalidDataException("The Install Git-ignore section is missing its closing marker.");
        return start < 0 ? new(-1, -1, string.Empty) : new(start, end, text[bodyStart..bodyEnd]);
    }

}
