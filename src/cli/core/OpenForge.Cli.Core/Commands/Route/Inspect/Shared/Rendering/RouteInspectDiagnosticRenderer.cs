using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectDiagnosticRenderer
{
    internal static string? Render(CliPresentationRequest<RouteInspectResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        var result = presentation.Result;
        var lines = new List<string>
        {
            "route-inspect diagnostics",
            $"status={RouteInspectJsonNames.Status(result.Status)}",
            $"selection.reference-kind={RouteInspectJsonNames.ReferenceKind(result.Selection.ReferenceKind)}",
            $"selection.method={RouteInspectJsonNames.SelectionMethod(result.Selection.SelectionMethod)}",
            $"selection.requested={Value(result.Selection.RequestedReference)}",
            $"selection.candidate-count={result.Selection.CandidatePaths.Count}",
            $"identity={(result.Identity is null ? "none" : "present")}",
            $"profile={(result.Profile is null ? "none" : "present")}",
            $"observations={result.Observations.Count}",
            $"conditions={result.Conditions.Count}",
            $"next={(result.Next is null ? "none" : "present")}",
        };
        AddWorkspace(lines, result.Workspace);
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddWorkspace(
        ICollection<string> lines,
        OpenForge.Cli.Core.Framework.Workspace.CliWorkspace? workspace)
    {
        if (workspace is null)
        {
            lines.Add("workspace=none");
            return;
        }

        lines.Add($"workspace.lexical={Value(workspace.LexicalRoot)}");
        lines.Add($"workspace.physical={Value(workspace.PhysicalRoot)}");
    }

    private static string Value(string? value)
    {
        if (value is null)
        {
            return "none";
        }

        return RouteInspectTextEscaping.Clamp(
            RouteInspectTextEscaping.Escape(value, RouteInspectTextEscaping.DiagnosticValueLimit),
            RouteInspectTextEscaping.DiagnosticValueLimit);
    }
}
