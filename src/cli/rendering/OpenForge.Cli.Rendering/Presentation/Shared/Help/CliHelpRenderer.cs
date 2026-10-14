using System.CommandLine;
using System.Text;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Shared;

namespace OpenForge.Cli.Core.Presentation.Shared.Help;

internal static class CliHelpRenderer
{
    private const int Indent = 2;
    private const int ColumnGap = 2;
    private const int FallbackIndent = Indent + 4;
    private const int MinimumDescriptionWidth = 20;

    internal static string Render(ParseResult parseResult, CliHelpContent productContent, int width)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(productContent);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);

        var command = parseResult.CommandResult.Command;
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(command.Description))
        {
            AppendText(builder, command.Description, width, Indent);
            builder.AppendLine();
            builder.AppendLine();
        }

        builder.AppendLine($"{SharedText.HelpHeadingUsage()}:");
        builder.AppendLine($"  {productContent.Usage ?? CliHelpSymbols.Usage(parseResult)}");
        builder.AppendLine();
        AppendSections(builder, productContent.Introduction, width);
        AppendRows(builder, SharedText.HelpHeadingArguments(),
            command.Arguments.Where(argument => !argument.Hidden).Select(argument => ($"<{argument.Name}>", argument.Description)), width);
        AppendCommands(builder, command, productContent.CommandGroups, width);
        var optionHeading = command is RootCommand ? SharedText.HelpHeadingGlobalOptions() : SharedText.HelpHeadingOptions();
        AppendRows(builder, optionHeading, CliHelpSymbols.Options(parseResult, !productContent.SummarizeGlobalOptions), width);
        if (productContent.SummarizeGlobalOptions && CliHelpSymbols.GlobalOptionNames(parseResult) is { Length: > 0 } globalNames)
        {
            AppendText(builder, SharedText.HelpGlobalOptionsSummary(globalNames), width, 0);
            builder.AppendLine();
            builder.AppendLine();
        }

        AppendSections(builder, productContent.Sections, width);
        if (productContent.Footer is { } footer)
        {
            AppendText(builder, footer, width, Indent);
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendCommands(StringBuilder builder, Command command, IReadOnlyList<CliHelpCommandGroup> groups, int width)
    {
        if (groups.Count == 0)
        {
            AppendRows(builder, SharedText.HelpHeadingCommands(),
                command.Subcommands.Where(child => !child.Hidden).Select(child => (CliHelpSymbols.CommandLabel(child), child.Description)), width);
            return;
        }

        foreach (var group in groups)
        {
            AppendRows(builder, group.Heading,
                group.Commands.Where(child => command.Subcommands.Contains(child) && !child.Hidden)
                    .Select(child => (child.Name, child.Description)), width);
        }
    }

    private static void AppendRows(StringBuilder builder, string heading,
        IEnumerable<(string Label, string? Description)> rows, int width)
    {
        var items = rows.ToArray();
        if (items.Length == 0)
        {
            return;
        }

        builder.AppendLine($"{heading}:");
        AppendRowBody(builder, items, width);
        builder.AppendLine();
    }

    private static void AppendRowBody(StringBuilder builder, IReadOnlyList<(string Label, string? Description)> rows,
        int width, int? maximumNameWidth = null)
    {
        var limit = Math.Min(maximumNameWidth ?? width / 3, Math.Max(0, width - Indent - ColumnGap - MinimumDescriptionWidth));
        var nameWidth = rows.Where(row => row.Label.Length <= limit).Select(row => row.Label.Length).DefaultIfEmpty().Max();
        var descriptionIndent = Indent + nameWidth + ColumnGap;
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Description))
            {
                builder.AppendLine($"  {row.Label}");
            }
            else if (row.Label.Length > limit)
            {
                builder.AppendLine($"  {row.Label}");
                AppendText(builder, row.Description, width, FallbackIndent);
                builder.AppendLine();
            }
            else
            {
                AppendText(builder, row.Description, width, descriptionIndent, $"  {row.Label.PadRight(nameWidth)}  ");
                builder.AppendLine();
            }
        }
    }

    private static void AppendSections(StringBuilder builder, IReadOnlyList<CliHelpSection> sections, int width)
    {
        foreach (var section in sections)
        {
            if (section.Children.Count == 0 && section.Rows.Count == 0 && section.Body.Length == 0)
            {
                continue;
            }

            builder.AppendLine($"{section.Heading}:");
            AppendSection(builder, section, width);
            builder.AppendLine();
            builder.AppendLine();
        }
    }

    private static void AppendSection(StringBuilder builder, CliHelpSection section, int width)
    {
        if (section.Rows.Count > 0)
        {
            AppendRowBody(builder, section.Rows, width, section.MaximumNameWidth);
            if (section.Body.Length == 0 && section.Children.Count == 0)
            {
                builder.Length -= Environment.NewLine.Length;
                return;
            }
        }

        if (section.Children.Count > 0)
        {
            var first = true;
            foreach (var child in section.Children)
            {
                if (!first)
                {
                    builder.AppendLine();
                }

                first = false;
                if (child.PreserveLines)
                {
                    builder.AppendLine($"  {child.Heading}:");
                    AppendSection(builder, child, width);
                }
                else
                {
                    AppendText(builder, $"{child.Heading}: {child.Body.Trim()}", width, Indent);
                }
            }
        }
        else if (section.PreserveLines)
        {
            builder.Append(string.Join(Environment.NewLine, section.Body.Split('\n').Select(line => $"  {line.Trim()}")));
        }
        else if (section.Body.Length > 0)
        {
            AppendText(builder, section.Body, width, Indent);
        }
    }

    private static void AppendText(StringBuilder builder, string text, int width, int indent, string? prefix = null)
    {
        using var reader = new StringReader(text);
        var firstLine = true;
        while (reader.ReadLine() is { } line)
        {
            if (!firstLine)
            {
                builder.AppendLine();
            }

            var start = firstLine ? prefix : null;
            firstLine = false;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            builder.Append(start ?? new string(' ', indent));
            var column = start?.Length ?? indent;
            var firstWord = true;
            foreach (var word in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!firstWord && column + 1 + word.Length > width)
                {
                    builder.AppendLine();
                    builder.Append(' ', indent);
                    column = indent;
                    firstWord = true;
                }

                if (!firstWord)
                {
                    builder.Append(' ');
                    column++;
                }

                builder.Append(word);
                column += word.Length;
                firstWord = false;
            }
        }
    }
}
