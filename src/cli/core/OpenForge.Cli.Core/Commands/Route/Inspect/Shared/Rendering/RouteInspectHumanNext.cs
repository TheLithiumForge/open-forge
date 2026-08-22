using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectHumanNext
{
    internal static string? Line(RouteInspectResult result)
    {
        if (result.Next is null)
        {
            return null;
        }

        return result.Status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Attention => result.Selection.SelectionMethod
                == RouteInspectSelectionMethod.Interactive
                ? "Next: rerun with the exact path for non-interactive use."
                : null,
            CliSemanticStatus.Blocked => BlockedLine(result),
            CliSemanticStatus.Incomplete => "Next: open-forge doctor",
            CliSemanticStatus.Invalid => "Next: correct the named source or input.",
            CliSemanticStatus.Failed => "Next: report the failure and retry with bounded diagnostics.",
            CliSemanticStatus.Interrupted => "Next: rerun the same request.",
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The status is not defined."),
        };
    }

    private static string BlockedLine(RouteInspectResult result)
    {
        var collision = result.Conditions.FirstOrDefault(
            condition => condition.Code == RouteInspectConditionCode.AmbiguousSource);
        if (collision is { Paths.Count: > 0 })
        {
            return "Next: rerun with one of the listed exact paths.";
        }

        if (result.Conditions.Any(condition => condition.Code == RouteInspectConditionCode.AmbiguousRoute))
        {
            return "Next: rerun with the exact source path after resolving the ambiguous route.";
        }

        return "Next: open-forge doctor";
    }
}
