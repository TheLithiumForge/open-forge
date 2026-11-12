using System.Globalization;
using System.Text.RegularExpressions;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;

internal static partial class SourceLinkFileFragmentReader
{
    internal static SourceLinkFragmentKind Classify(string path, string? fragment)
    {
        if (fragment is null) return SourceLinkFragmentKind.None;
        if (path.EndsWith(SourceFormClassifier.MarkdownExtension, StringComparison.OrdinalIgnoreCase))
            return SourceLinkFragmentKind.Heading;
        return LineRange().IsMatch(fragment) ? SourceLinkFragmentKind.LineRange : SourceLinkFragmentKind.Symbol;
    }

    internal static SourceLinkDestinationFacts? ReadFailure(string source, SourceLinkTarget target, string rawFragment, string fragment)
    {
        var range = LineRange().Match(fragment);
        if (range.Success)
        {
            var lineCount = 0;
            using var reader = new StringReader(source);
            while (reader.ReadLine() is not null) lineCount++;
            if (int.TryParse(range.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var start)
                && int.TryParse(range.Groups[2].Success ? range.Groups[2].Value : range.Groups[1].Value,
                    NumberStyles.None, CultureInfo.InvariantCulture, out var end)
                && start >= 1 && end >= start && end <= lineCount)
                return null;
            return Failure(target, rawFragment, SourceLinkTargetResolution.LineOutOfRange,
                SourceLinkDestinationFindingCode.LineOutOfRange, "The linked line range is outside the target file's lines or is reversed.");
        }

        var tokens = Identifier().Matches(source).Select(match => match.Value).ToHashSet(StringComparer.Ordinal);
        if (fragment.Split('.').All(part => tokens.Contains(part))) return null;
        return Failure(target, rawFragment, SourceLinkTargetResolution.SymbolMissing,
            SourceLinkDestinationFindingCode.SymbolMissing, "At least one dotted symbol part is not an identifier token in the target file.");
    }

    private static SourceLinkDestinationFacts Failure(SourceLinkTarget target, string fragment,
        SourceLinkTargetResolution resolution, SourceLinkDestinationFindingCode code, string cause)
        => new()
        {
            Fragment = fragment,
            Target = target with { Resolution = resolution },
            Finding = new() { Code = code, Cause = cause, Candidates = [] },
        };

    [GeneratedRegex(@"\AL([0-9]+)(?:-L([0-9]+))?\z", RegexOptions.CultureInvariant)]
    private static partial Regex LineRange();

    [GeneratedRegex(@"(?<![\w$])[\p{L}_$][\w$]*", RegexOptions.CultureInvariant)]
    private static partial Regex Identifier();
}
