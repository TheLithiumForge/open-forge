using System.CommandLine;
using System.CommandLine.Help;
using System.Text;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Help;

internal static class CliHelpRenderer
{
    internal static string Render(ParseResult parseResult, CliHelpContent productContent, int width)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(productContent);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        using var writer = new StringWriter();
        parseResult.InvocationConfiguration.Output = writer;
        _ = new HelpAction { MaxWidth = width }.Invoke(parseResult);

        var builder = new StringBuilder(writer.ToString().TrimEnd());
        foreach (var section in productContent.Sections)
        {
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine($"{section.Heading}:");
            AppendSection(builder, section.Body, width);
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendSection(StringBuilder builder, string body, int width)
    {
        const string indent = "  ";
        using var reader = new StringReader(body);
        var firstLine = true;
        while (reader.ReadLine() is { } line)
        {
            if (!firstLine)
            {
                builder.AppendLine();
            }

            firstLine = false;
            var column = indent.Length;
            builder.Append(indent);
            foreach (var word in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (column > indent.Length)
                {
                    if (column + 1 + word.Length > width)
                    {
                        builder.AppendLine();
                        builder.Append(indent);
                        column = indent.Length;
                    }
                    else
                    {
                        builder.Append(' ');
                        column++;
                    }
                }

                builder.Append(word);
                column += word.Length;
            }
        }
    }
}
