using System.Text;
using OpenForge.Cli.Core.Presentation.Library.Sync.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Rendering;

internal static class LibrarySyncDataTextRenderer
{
    internal static CliTextDocument Render(
        LibrarySyncData data,
        CliSelection selection,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

        var builder = new StringBuilder();
        foreach (var row in data.TextRows)
        {
            if (row.IsStandalone)
            {
                builder.Append("  ")
                    .Append(CliText.Escape(row.Label))
                    .Append('\n');
                continue;
            }

            if (row.IsSection)
            {
                builder.Append("  ")
                    .Append(CliText.Escape(row.Label))
                    .Append(' ')
                    .Append(style.Subject(CliText.Escape(row.Path)))
                    .Append('\n');
            }
            else
            {
                builder.Append("  ")
                    .Append(CliText.Escape(row.Label).PadRight(9));
                if (row.Label.Length >= 9)
                {
                    builder.Append("  ");
                }

                builder
                    .Append(style.Subject(CliText.Escape(row.Path)));
                if (row.Suffix is { } suffix)
                {
                    builder.Append("   (")
                        .Append(CliText.Escape(suffix))
                        .Append(')');
                }

                builder.Append('\n');
                if (row.Target is { } target && selection.Detail >= CliDetail.Standard)
                {
                    builder.Append("    ")
                        .Append(CliText.Escape(global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatTarget($"{target}")))
                        .Append('\n');
                }
            }
        }

        foreach (var line in data.TextSummaryLines)
        {
            builder.Append("  ")
                .Append(CliText.Escape(line))
                .Append('\n');
        }

        foreach (var line in data.TextDetailLines)
        {
            builder.Append("  ")
                .Append(CliText.Escape(line))
                .Append('\n');
        }

        return builder.Length == 0
            ? new CliTextDocument([])
            : new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }
}
