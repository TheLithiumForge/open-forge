using System.Text;
using OpenForge.Cli.Core.Presentation.Route.Update.Models;
using OpenForge.Cli.Core.Presentation.Route.Update.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Route.Update.Shared.Rendering;

internal static class RouteUpdateDataTextRenderer
{
    internal static CliTextDocument Render(
        RouteUpdateData data,
        CliSelection selection,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

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

        if (selection.Detail >= CliDetail.Standard)
        {
            foreach (var effect in data.TextEffects)
            {
                builder.Append("  ")
                    .Append(CliText.Escape(effect.Action))
                    .Append('\n');
                if (selection.Detail >= CliDetail.Full)
                {
                    if (effect.Before is { } before)
                    {
                        builder.Append("    ")
                            .Append(CliText.Escape(global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.FormatBefore($"{before}")))
                            .Append('\n');
                    }

                    if (effect.After is { } after)
                    {
                        builder.Append("    ")
                            .Append(CliText.Escape(global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.FormatAfter($"{after}")))
                            .Append('\n');
                    }
                }
            }
        }

        if (selection.Detail >= CliDetail.Full)
        {
            if (data.FrontmatterBefore is { } before)
            {
                builder.Append("  ")
                    .Append(CliText.Escape(RouteUpdateWording.FrontmatterBefore()))
                    .Append('\n');
                builder.Append("    ").Append(CliText.Escape(before)).Append('\n');
            }

            if (data.FrontmatterAfter is { } after)
            {
                builder.Append("  ")
                    .Append(CliText.Escape(RouteUpdateWording.FrontmatterAfter()))
                    .Append('\n');
                builder.Append("    ").Append(CliText.Escape(after)).Append('\n');
            }
        }

        if (data.ShowNoChanges)
        {
            builder.Append((global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged() + "\n"));
        }

        return builder.Length == 0
            ? new CliTextDocument([])
            : new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }
}
