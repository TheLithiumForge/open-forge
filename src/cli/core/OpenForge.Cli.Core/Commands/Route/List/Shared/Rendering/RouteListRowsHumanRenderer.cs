using static OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering.RouteListHumanValues;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListRowsHumanRenderer
{
    internal static void Add(ICollection<string> lines, RouteListRow row, bool expanded)
    {
        var indentation = new string(' ', checked(row.RelativeDepth * 2));
        lines.Add($"{indentation}{Escape(row.Id)}  {Escape(row.Path)}");
        lines.Add($"{indentation}  {Escape(row.Description)}; tags: {Values(row.Tags)}");
        if (!expanded)
        {
            return;
        }

        var parent = row.ParentId is null ? "none" : $"{Escape(row.ParentId)}; {Optional(row.ParentPath)}";
        lines.Add($"{indentation}  Parent: {parent}");
        lines.Add($"{indentation}  Depth: absolute {Number(row.AbsoluteDepth)}; relative {row.RelativeDepth}");
        lines.Add($"{indentation}  Kind: {RowKind(row.Kind)}; direct children: {Number(row.DirectChildCount)}");
        lines.Add($"{indentation}  Selected as: {SelectionProvenance(row.Provenance.Selection)}");
        lines.Add($"{indentation}  Source: {SourceProvenance(row.Provenance.Source)}; overwrite: {(row.Provenance.HasOverwrite ? "present" : "none")}");
    }
}
