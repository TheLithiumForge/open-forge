using System.Text;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Presentation.Library.List.Models;
using OpenForge.Cli.Core.Presentation.Library.List.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Library.List.Shared.Rendering;

internal static class LibraryListDataTextRenderer
{
    internal static CliTextDocument Render(LibraryListData data, CliSelection selection, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);
        var spans = new List<CliTextSpan>();
        var builder = new StringBuilder();
        var rows = data.Libraries.Select(library => new[]
        {
            library.Id,
            LibraryListWording.LibraryRow(library.SourceFolder, library.DestinationFolder),
            LibraryListWording.LinkSummary(
                library.Links.Counts.Current,
                library.Links.Counts.Missing,
                library.Links.Counts.Changed,
                library.Links.Counts.Unavailable),
        }).ToArray();
        if (rows.Length > 0)
        {
            builder.Append(CliTable.Render(rows, (column, cell) => column == 0 ? style.Subject(cell) : cell));
        }

        if (data.ShowLinks)
        {
            foreach (var library in data.Libraries)
            {
                foreach (var link in library.Links.Rows)
                {
                    builder.Append("    ")
                        .Append(style.Subject(CliText.Escape(link.Path)))
                        .Append("  ")
                        .Append(CliText.Escape(LibraryListWording.LinkState(link.ResultState)))
                        .Append('\n');
                    if (data.ShowLinkTargets)
                    {
                        builder.Append("      ")
                            .Append(CliText.Escape(LibraryListWording.ExpectedTarget(link.ExpectedTarget)))
                            .Append('\n')
                            .Append("      ")
                            .Append(CliText.Escape(LibraryListWording.ObservedTarget(link.ObservedTarget)))
                            .Append('\n')
                            .Append("      ")
                            .Append(CliText.Escape(LibraryListWording.SourceId(link.SourceId)))
                            .Append('\n');
                    }
                }
            }
        }

        if (data.RecordCoverage is { } coverage)
        {
            builder.Append(CliText.Escape(LibraryListWording.RecordCoverage(coverage.ResultState))).Append('\n');
        }

        if (builder.Length > 0)
        {
            spans.Add(new CliTextSpan(builder.ToString()));
        }

        return new CliTextDocument(spans);
    }

}
