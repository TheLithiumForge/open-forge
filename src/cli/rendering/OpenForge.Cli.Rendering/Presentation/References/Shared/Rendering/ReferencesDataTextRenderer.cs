using System.Text;
using OpenForge.Cli.Core.Presentation.References.Models;
using OpenForge.Cli.Core.Presentation.References.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.References.Shared.Rendering;

internal static class ReferencesDataTextRenderer
{
    internal static CliTextDocument Render(ReferencesData data, CliSelection selection, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);
        var builder = new StringBuilder();
        AppendIncoming(builder, data, style);
        AppendOutgoing(builder, data, style);
        AppendTrailers(builder, data);
        var spans = new List<CliTextSpan>();
        if (builder.Length > 0)
        {
            spans.Add(new CliTextSpan(builder.ToString()));
        }

        return new CliTextDocument(spans);
    }

    private static void AppendIncoming(StringBuilder builder, ReferencesData data, CliTextStyle style)
    {
        if (data.Incoming is not { Count: > 0 } incoming)
        {
            return;
        }

        foreach (var row in incoming)
        {
            builder.Append("  in   ")
                .Append(style.Subject(CliText.Escape(row.Path)))
                .Append(CliText.Escape(row.Location));
            if (row.Layer is { } layer)
            {
                builder.Append("   ").Append(CliText.Escape(layer));
            }

            builder.Append('\n');
        }
    }

    private static void AppendOutgoing(StringBuilder builder, ReferencesData data, CliTextStyle style)
    {
        if (data.Outgoing is not { Count: > 0 } outgoing)
        {
            return;
        }

        foreach (var row in outgoing)
        {
            builder.Append("  out  ")
                .Append(CliText.Escape(row.Location))
                .Append("   ")
                .Append(style.Subject(CliText.Escape(row.Destination)));
            if (data.ShowResolvedTargets && row.ResolvedPath is { } resolved)
            {
                builder.Append(" -> ").Append(CliText.Escape(resolved));
            }

            if (row.RowState is { } state)
            {
                builder.Append("   ").Append(CliText.Escape(state));
            }

            if (row.Layer is { } layer)
            {
                builder.Append("   ").Append(CliText.Escape(layer));
            }

            builder.Append('\n');
        }
    }

    private static void AppendTrailers(StringBuilder builder, ReferencesData data)
    {
        if (data.ShowResolvedTargets)
        {
            builder.Append(CliText.Escape(ReferencesWording.Counts(
                data.Incoming?.Count ?? 0,
                data.Outgoing?.Count ?? 0))).Append('\n');
            if (data.Filters is { } filters)
            {
                builder.Append(CliText.Escape(
                    ReferencesWording.FilterSummary(filters.Include, filters.Exclude))).Append('\n');
            }
        }

        if (!data.ShowLayers || data.Scanned is not { } scanned)
        {
            return;
        }

        builder.Append(CliText.Escape(ReferencesWording.ScannedHeading(scanned.Count))).Append('\n');
        foreach (var source in scanned)
        {
            builder.Append("    ")
                .Append(CliText.Escape(source.Path))
                .Append("   ")
                .Append(CliText.Escape(source.Layer))
                .Append('\n');
        }
    }
}
