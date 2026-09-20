using System.Text;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Presentation.Index.Models;
using OpenForge.Cli.Core.Presentation.Index.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Index.Shared.Rendering;

internal static class IndexDataTextRenderer
{
    internal static CliTextDocument Render(IndexData data, CliSelection selection, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);
        var spans = new List<CliTextSpan>();
        var builder = new StringBuilder();
        var visible = data.TextRegions.Where(region => region.ResultAction == IndexRegionAction.Update || selection.Detail >= CliDetail.Full).ToArray();
        var rows = visible.Select(region => new[]
        {
            region.Path,
            region.ResultOutcome switch
            {
                IndexRegionOutcome.AlreadyCurrent => IndexWording.CurrentRow(),
                IndexRegionOutcome.NotStarted => IndexWording.NotStarted(),
                IndexRegionOutcome.NotEstablished or IndexRegionOutcome.Unknown => IndexWording.Unknown(),
                IndexRegionOutcome.Applied or IndexRegionOutcome.Verified or IndexRegionOutcome.NotRequested => IndexWording.EntryCounts(region.Before, region.After),
                _ => throw new ArgumentOutOfRangeException(nameof(data)),
            },
        }).ToArray();
        if (rows.Length > 0)
        {
            builder.Append(CliTable.Render(rows, (column, cell) => column == 0 ? style.Subject(cell) : cell));
        }
        if (data.NothingWritten && data.CurrentCount > 0)
            builder.Append("  ").Append(IndexWording.OtherCurrent(data.CurrentCount)).Append('\n');
        else if (selection.Detail >= CliDetail.Standard && selection.Detail < CliDetail.Full && data.CurrentCount > 0)
            builder.Append(style.Dim(IndexWording.Unchanged(data.CurrentCount))).Append('\n');
        if (builder.Length > 0) spans.Add(new CliTextSpan(builder.ToString()));
        if (data.Mode == "dry-run" && selection.Detail >= CliDetail.Standard)
        {
            foreach (var region in visible.Where(region => region.ResultAction == IndexRegionAction.Update))
            {
                spans.Add(new CliTextSpan(CliText.Escape(IndexWording.EntriesSection(region.Path)) + "\n"));
                foreach (var line in region.Diff)
                {
                    spans.Add(new CliTextSpan(line, Authored: true));
                    if (line.Length > 0 && line[^1] is not ('\n' or '\r')) spans.Add(new CliTextSpan("\n"));
                }
            }
        }
        if (selection.Detail >= CliDetail.Full)
        {
            if (data.Selection is { } selected && selected.Sources.Count > 0)
                spans.Add(new CliTextSpan(CliText.Escape(selected.Origin == "automatic-loader" ? IndexWording.LoaderSelection()
                    : IndexWording.ExplicitSelection(string.Join(", ", selected.Sources.Select(source => source.Path)))) + "\n"));
            var recovery = data.Recovery.State switch
            {
                IndexRecoveryState.NotRequired => IndexWording.NoRecovery(),
                IndexRecoveryState.NotCreated => IndexWording.RecoveryNotCreated(),
                IndexRecoveryState.Removed => IndexWording.RecoveryRemoved(),
                IndexRecoveryState.Retained => IndexWording.RecoveryRetained(data.Recovery.ResidualPath!),
                IndexRecoveryState.Unknown => IndexWording.RecoveryUnknown(),
                _ => throw new ArgumentOutOfRangeException(nameof(data)),
            };
            spans.Add(new CliTextSpan(CliText.Escape(recovery) + "\n"));
        }
        if (data.Mode == "dry-run" && data.Changes.Count > 0)
            spans.Add(new CliTextSpan(IndexWording.NoChanges() + "\n"));
        return new CliTextDocument(spans);
    }
}
