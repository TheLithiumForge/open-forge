using System.Text;
using System.Text.RegularExpressions;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

// The rewrite contract accepts simple command and args lines, checked as text without a TOML parser.
internal static partial class ToolTextEntries
{
    internal static ToolEntrySpan[] Rewrite(byte[] bytes, ToolTomlCommandRewrite rewrite)
    {
        var source = ToolRegistrationBytes.Utf8.GetString(bytes);
        var lines = ToolRegistrationBytes.Lines(source);
        var members = TomlMembers(lines, rewrite.Table);
        var command = CommandMatch(members.Command, rewrite.Expected.Command);
        var args = ArgsMatch(members.Args, rewrite.Expected.Args);
        if (!command.Success || !args.Success) throw new InvalidDataException("The TOML command differs from its declared original.");
        return [ReplaceLine(source, members.Command, command, Quote(rewrite.Replacement.Command)),
            ReplaceLine(source, members.Args, args, "[" + string.Join(", ", rewrite.Replacement.Args.Select(Quote)) + "]")];
    }

    internal static bool Matches(byte[] bytes, ToolTomlCommandRewrite rewrite, bool replacement)
    {
        var members = TomlMembers(ToolRegistrationBytes.Lines(ToolRegistrationBytes.Utf8.GetString(bytes)), rewrite.Table);
        return Matches(members, replacement ? rewrite.Replacement : rewrite.Expected);
    }

    internal static ToolEntrySpan Rewrite(byte[] bytes, ToolMarkedLineRewrite rewrite)
    {
        var source = ToolRegistrationBytes.Utf8.GetString(bytes);
        var lines = MarkedLines(source, rewrite);
        var matches = lines.Where(line => line.Content == rewrite.Expected).ToArray();
        if (matches.Length != 1) throw new InvalidDataException("The marked block requires one exact original line.");
        return ToolRegistrationBytes.Span(source, matches[0].Start, matches[0].ContentEnd, rewrite.Replacement);
    }

    internal static bool Matches(byte[] bytes, ToolMarkedLineRewrite rewrite, bool replacement)
        => MarkedLines(ToolRegistrationBytes.Utf8.GetString(bytes), rewrite)
            .Count(line => line.Content == (replacement ? rewrite.Replacement : rewrite.Expected)) == 1;

    private static ToolTextLine[] MarkedLines(string source, ToolMarkedLineRewrite rewrite)
    {
        RequireSingleLine(rewrite.StartMarker);
        RequireSingleLine(rewrite.EndMarker);
        RequireSingleLine(rewrite.Expected);
        RequireSingleLine(rewrite.Replacement);
        var lines = ToolRegistrationBytes.Lines(source);
        var start = lines.Where(line => line.Content == rewrite.StartMarker).ToArray();
        var end = lines.Where(line => line.Content == rewrite.EndMarker).ToArray();
        if (start.Length != 1 || end.Length != 1 || start[0].End > end[0].Start)
            throw new InvalidDataException("The declared block markers are missing, duplicated or reversed.");
        return [.. lines.Where(line => line.Start >= start[0].End && line.End <= end[0].Start)];
    }

    private static (ToolTextLine Command, ToolTextLine Args) TomlMembers(ToolTextLine[] lines, string table)
    {
        if (!SimpleTable().IsMatch(table)) throw new InvalidDataException("A TOML rewrite requires a simple named table.");
        foreach (var line in lines)
        {
            var text = line.Content.TrimStart('\uFEFF', ' ', '\t');
            if (text.StartsWith('#')) continue;
            if (text.Contains("\"\"\"", StringComparison.Ordinal) || text.Contains("'''", StringComparison.Ordinal)
                || text.StartsWith('[') && text.Contains('\\'))
                throw new InvalidDataException("Multiline values and escaped table headers prevent an exact TOML text selection.");
        }
        var aliases = string.Join(@"\s*\.\s*", table.Split('.').Select(part => $"(?:{Regex.Escape(part)}|\"{Regex.Escape(part)}\"|'{Regex.Escape(part)}')"));
        var aliasHeader = new Regex(@"\A\s*\[\[?\s*" + aliases + @"\s*\]\]?\s*(?:#.*)?\z", RegexOptions.CultureInvariant);
        var headers = lines.Select((line, index) => (line, index)).Where(item => aliasHeader.IsMatch(item.line.Content.TrimStart('\uFEFF'))).ToArray();
        if (headers.Length != 1 || !Regex.IsMatch(headers[0].line.Content.TrimStart('\uFEFF'), @"\A\s*\[" + Regex.Escape(table) + @"\]\s*(?:#.*)?\z", RegexOptions.CultureInvariant))
            throw new InvalidDataException("The TOML table selector is missing, ambiguous or outside the simple shape.");
        var commands = new List<ToolTextLine>();
        var args = new List<ToolTextLine>();
        for (var index = headers[0].index + 1; index < lines.Length; index++)
        {
            var text = lines[index].Content.Trim();
            if (text.StartsWith('[')) break;
            if (text.Length == 0 || text.StartsWith('#')) continue;
            var assignment = Assignment().Match(text);
            if (!assignment.Success || text.Contains("\"\"\"", StringComparison.Ordinal) || text.Contains("'''", StringComparison.Ordinal))
                throw new InvalidDataException("The selected TOML table requires single-line assignments.");
            var key = assignment.Groups[1].Value;
            if (key.Trim('\'', '"') is ToolJsonEntries.Command or ToolJsonEntries.Args && key is not (ToolJsonEntries.Command or ToolJsonEntries.Args))
                throw new InvalidDataException("The TOML command keys are outside the simple shape.");
            if (key == ToolJsonEntries.Command) commands.Add(lines[index]);
            if (key == ToolJsonEntries.Args) args.Add(lines[index]);
        }
        // A dotted assignment could declare the same member outside the named table.
        if (lines.Any(line => Regex.IsMatch(line.Content, @"\A\s*" + aliases + @"\s*\.\s*(?:command|args)\s*=", RegexOptions.CultureInvariant)))
            throw new InvalidDataException("A dotted TOML command selector is ambiguous.");
        if (commands.Count != 1 || args.Count != 1) throw new InvalidDataException("The TOML table requires one command line and one args line.");
        return (commands[0], args[0]);
    }

    private static bool Matches((ToolTextLine Command, ToolTextLine Args) members, ToolCommandValue expected)
        => CommandMatch(members.Command, expected.Command).Success && ArgsMatch(members.Args, expected.Args).Success;

    private static Match CommandMatch(ToolTextLine line, string command)
        => ValueMatch(line, ToolJsonEntries.Command, Regex.Escape(Quote(command)));

    private static Match ArgsMatch(ToolTextLine line, string[] args)
        => ValueMatch(line, ToolJsonEntries.Args, @"\[\s*" + string.Join(@"\s*,\s*", args.Select(argument => Regex.Escape(Quote(argument)))) + @"\s*\]");

    private static Match ValueMatch(ToolTextLine line, string key, string pattern)
        => Regex.Match(line.Content, @"\A\s*" + Regex.Escape(key) + @"\s*=\s*(?<value>" + pattern + @")\s*(?:#.*)?\z", RegexOptions.CultureInvariant);

    private static ToolEntrySpan ReplaceLine(string source, ToolTextLine line, Match match, string value)
        => ToolRegistrationBytes.Span(source, line.Start + match.Groups["value"].Index, line.Start + match.Groups["value"].Index + match.Groups["value"].Length, value);

    private static string Quote(string value)
    {
        var result = new StringBuilder("\"");
        foreach (var rune in value.EnumerateRunes())
        {
            if (rune.Value is '"' or '\\') result.Append('\\').Append(rune.ToString());
            else if (rune.Value < 0x20 || rune.Value == 0x7f) result.Append(System.Globalization.CultureInfo.InvariantCulture, $"\\u{rune.Value:X4}");
            else result.Append(rune.ToString());
        }
        return result.Append('"').ToString();
    }

    internal static void RequireSingleLine(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new InvalidDataException("A declared marker or guidance line must be one nonempty line.");
    }

    [GeneratedRegex(@"\A[A-Za-z0-9_-]+(?:\.[A-Za-z0-9_-]+)*\z", RegexOptions.CultureInvariant)]
    private static partial Regex SimpleTable();
    [GeneratedRegex("\\A([A-Za-z0-9_-]+|\"[^\"]+\"|'[^']+')\\s*=", RegexOptions.CultureInvariant)]
    private static partial Regex Assignment();
}
