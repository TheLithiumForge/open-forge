using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListHumanValues
{
    internal static string Selection(RouteListSelection selection)
    {
        return selection.Kind switch
        {
            RouteListSelectionKind.LoaderRoots => "loader roots from the authored Loader",
            RouteListSelectionKind.SourceId =>
                $"source ID {Optional(selection.AttemptedId)} -> {Optional(selection.ResolvedId)} at {Optional(selection.ResolvedPath)}",
            RouteListSelectionKind.SourcePath =>
                $"source path {Optional(selection.AttemptedPath)} -> {Optional(selection.ResolvedId)} at {Optional(selection.ResolvedPath)}",
            _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.Kind, "The route-list selection kind is not defined."),
        };
    }

    internal static string Values(IEnumerable<string> values)
    {
        return string.Concat(
            "[",
            string.Join(", ", values.Select(value => string.Concat("\"", Escape(value), "\""))),
            "]");
    }

    internal static string Optional(string? value)
    {
        return value is null ? "none" : Escape(value);
    }

    internal static string Number(int? value)
    {
        return value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "not applicable";
    }

    internal static string Depth(RouteListDepth? depth)
    {
        return depth?.MachineValue ?? "not established";
    }

    internal static string CoverageState(RouteListCoverageState state)
    {
        return state switch
        {
            RouteListCoverageState.NotStarted => "not started",
            RouteListCoverageState.Complete => "complete",
            RouteListCoverageState.Incomplete => "incomplete",
            RouteListCoverageState.Blocked => "blocked",
            RouteListCoverageState.Failed => "failed",
            RouteListCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The route-list coverage state is not defined."),
        };
    }

    internal static string RowKind(RouteListRowKind kind)
    {
        return kind switch
        {
            RouteListRowKind.Entrypoint => "entrypoint",
            RouteListRowKind.RoutedLeaf => "routed file",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The route-list row kind is not defined."),
        };
    }

    internal static string SelectionProvenance(RouteListSelectionProvenance provenance)
    {
        return provenance switch
        {
            RouteListSelectionProvenance.LoaderRoot => "Loader root",
            RouteListSelectionProvenance.ExplicitRoot => "explicit root",
            RouteListSelectionProvenance.DetachedRoot => "detached root",
            RouteListSelectionProvenance.Descendant => "descendant",
            _ => throw new ArgumentOutOfRangeException(nameof(provenance), provenance, "The route-list selection provenance is not defined."),
        };
    }

    internal static string SourceProvenance(RouteListSourceProvenance provenance)
    {
        return provenance switch
        {
            RouteListSourceProvenance.AuthoredEntrypoint => "authored entrypoint",
            RouteListSourceProvenance.AuthoredLeaf => "authored routed file",
            RouteListSourceProvenance.RoutedNative => "routed native source",
            _ => throw new ArgumentOutOfRangeException(nameof(provenance), provenance, "The route-list source provenance is not defined."),
        };
    }

    internal static string SelectedBy(CliWorkspace? workspace)
    {
        if (workspace is null)
        {
            return "none";
        }

        return workspace.SelectedBy switch
        {
            CliWorkspaceSelectionMethod.CurrentDirectory => "current directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "--workspace",
            _ => throw new ArgumentOutOfRangeException(nameof(workspace), workspace.SelectedBy, "The workspace selection method is not defined."),
        };
    }

    internal static string DepthExplanation(RouteListDepth? depth)
    {
        if (depth is null)
        {
            return "requested depth was not established";
        }

        if (depth.Kind == RouteListDepthKind.All)
        {
            return "all routed descendants";
        }

        var finiteDepth = depth.Value
            ?? throw new InvalidOperationException("A finite route-list depth requires a value.");
        return finiteDepth switch
        {
            0 => "selected roots only",
            1 => "selected roots plus direct routed children",
            _ => $"selected roots and routed descendants through relative depth {finiteDepth}",
        };
    }

    internal static string Escape(string value)
    {
        return RouteListTextEscaping.Escape(value);
    }
}
