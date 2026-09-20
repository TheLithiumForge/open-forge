using System.Text;
using OpenForge.Cli.Core.Presentation.Route.Create.Models;
using OpenForge.Cli.Core.Presentation.Route.Create.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Route.Create.Shared.Rendering;

internal static class RouteCreateDataTextRenderer
{
    internal static CliTextDocument Render(
        RouteCreateData data,
        CliSelection selection,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

        var spans = new List<CliTextSpan>();
        var builder = new StringBuilder();
        if (selection.Detail >= CliDetail.Standard)
        {
            foreach (var line in data.TextMetadata)
            {
                builder.Append("  ").Append(CliText.Escape(line)).Append('\n');
            }
        }

        foreach (var line in data.TextRows)
        {
            builder.Append("  ").Append(CliText.Escape(line)).Append('\n');
        }

        if (builder.Length > 0)
        {
            spans.Add(new CliTextSpan(builder.ToString()));
        }

        if (selection.Detail >= CliDetail.Full && data.Content is { } content
            && data.Target.Path is { } path)
        {
            spans.Add(new CliTextSpan(
                CliText.Escape(RouteCreateWording.FileHeader(path)) + "\n"));
            spans.Add(new CliTextSpan(content, Authored: true));
            if (content.Length == 0 || content[^1] is not ('\r' or '\n'))
            {
                spans.Add(new CliTextSpan("\n", Authored: true));
            }
        }

        if (selection.Detail >= CliDetail.Full)
        {
            foreach (var section in data.TextSections)
            {
                spans.Add(new CliTextSpan(
                    "  " + CliText.Escape(RouteCreateWording.EntriesSection(section.Path)) + "\n"));
                spans.Add(new CliTextSpan(
                    "    " + CliText.Escape(global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.FormatBefore($"{section.Before}")) + "\n"));
                spans.Add(new CliTextSpan(
                    "    " + CliText.Escape(global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.FormatAfter($"{section.After}")) + "\n"));
                spans.Add(new CliTextSpan(
                    "    " + CliText.Escape(RouteCreateWording.Verification(section.Verification)) + "\n"));
            }
        }

        if (data.Mode == "dry-run")
        {
            spans.Add(new CliTextSpan((global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged() + "\n")));
        }

        foreach (var line in data.TextNextLines)
        {
            spans.Add(new CliTextSpan(CliText.Escape(line) + "\n"));
        }

        return new CliTextDocument(spans);
    }
}
