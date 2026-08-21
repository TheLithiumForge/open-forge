using System.CommandLine;
using System.CommandLine.Help;

namespace OpenForge.Cli.Core.Shell.Presentation;

internal static class CliHelpRenderer
{
    internal static string Render(ParseResult parseResult, CliHelpContent productContent)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(productContent);

        using var writer = new StringWriter();
        parseResult.InvocationConfiguration.Output = writer;
        _ = new HelpAction { MaxWidth = 120 }.Invoke(parseResult);

        var standardHelp = writer.ToString().TrimEnd();
        if (productContent.Sections.Count == 0)
        {
            return standardHelp;
        }

        var builder = new System.Text.StringBuilder(standardHelp);
        foreach (var section in productContent.Sections)
        {
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine($"{section.Heading}:");
            builder.Append(section.Body.TrimEnd());
        }

        return builder.ToString();
    }
}
