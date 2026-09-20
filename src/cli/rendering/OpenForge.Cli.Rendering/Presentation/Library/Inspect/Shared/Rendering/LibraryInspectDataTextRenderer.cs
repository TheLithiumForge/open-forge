using System.Text;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Rendering;

internal static class LibraryInspectDataTextRenderer
{
    internal static CliTextDocument Render(LibraryInspectData data, CliSelection selection, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

        var builder = new StringBuilder();
        if (data.ShowFiles)
        {
            if (data.ShowTargets)
            {
                var pathWidth = data.TextFiles
                    .Select(file => CliText.Escape(file.DestinationPath).EnumerateRunes().Count())
                    .DefaultIfEmpty()
                    .Max();
                foreach (var file in data.TextFiles)
                {
                    var path = CliText.Escape(file.DestinationPath);
                    builder.Append("  ")
                        .Append(style.Subject(path))
                        .Append(' ', pathWidth - path.EnumerateRunes().Count() + 2)
                        .Append(CliText.Escape(LibraryInspectWording.Relation(file.Relation)))
                        .Append('\n');
                    builder.Append("      ")
                        .Append(CliText.Escape(LibraryInspectWording.ExpectedTarget(file.ExpectedTarget)))
                        .Append('\n')
                        .Append("      ")
                        .Append(CliText.Escape(LibraryInspectWording.ObservedTarget(file.ObservedTarget)))
                        .Append('\n');
                }
            }
            else
            {
                var rows = data.TextFiles.Select(file => new[]
                {
                    file.DestinationPath,
                    LibraryInspectWording.Relation(file.Relation),
                }).ToArray();
                builder.Append(CliTable.Render(rows, (column, cell) =>
                    column == 0 ? style.Subject(cell) : cell));
            }
        }

        if (data.Inventory is { } inventory)
        {
            builder.Append(CliText.Escape(LibraryInspectWording.Inventory(inventory.Eligible, inventory.Excluded)))
                .Append('\n');
        }

        return builder.Length == 0
            ? new CliTextDocument([])
            : new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }
}
