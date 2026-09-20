using System.Text;
using OpenForge.Cli.Core.Presentation.Route.Init.Models;
using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Route.Init.Shared.Rendering;

internal static class RouteInitDataTextRenderer
{
    internal static CliTextDocument Render(
        RouteInitData data,
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

        if (data.TextAdvisory is { } advisory)
        {
            builder.Append("  ").Append(CliText.Escape(advisory)).Append('\n');
        }

        if (builder.Length > 0)
        {
            spans.Add(new CliTextSpan(builder.ToString()));
        }

        if (selection.Detail >= CliDetail.Full)
        {
            foreach (var entrypoint in data.TextEntrypoints)
            {
                if (entrypoint.Content is not { } content)
                {
                    continue;
                }

                spans.Add(new CliTextSpan(
                    CliText.Escape(RouteInitWording.FileHeader(entrypoint.Path)) + "\n"));
                spans.Add(new CliTextSpan(content, Authored: true));
                if (content.Length == 0 || content[^1] is not ('\r' or '\n'))
                {
                    spans.Add(new CliTextSpan("\n", Authored: true));
                }
            }

            foreach (var section in data.TextSections)
            {
                spans.Add(new CliTextSpan(
                    "  " + CliText.Escape(RouteInitWording.EntriesSection(section.Path)) + "\n"));
                spans.Add(new CliTextSpan(
                    "    " + CliText.Escape(global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.FormatBefore($"{section.TextBefore ?? section.Before}")) + "\n"));
                spans.Add(new CliTextSpan(
                    "    " + CliText.Escape(global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.FormatAfter($"{section.TextAfter ?? section.After}")) + "\n"));
                spans.Add(new CliTextSpan(
                    "    " + CliText.Escape(RouteInitWording.Verification(section.Verification)) + "\n"));
            }

            if (data.FrameworkFingerprint is { } fingerprint)
            {
                spans.Add(new CliTextSpan(
                    "  " + CliText.Escape(RouteInitWording.FrameworkFingerprint(fingerprint)) + "\n"));
            }

            if (data.TextRecovery is { } recovery)
            {
                spans.Add(new CliTextSpan("  " + CliText.Escape(recovery) + "\n"));
            }
        }

        if (data.Mode == "dry-run")
        {
            spans.Add(new CliTextSpan((global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged() + "\n")));
        }

        return new CliTextDocument(spans);
    }
}
