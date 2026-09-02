using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

internal static class RouteUpdateDiagnosticRenderer
{
    private const int MaximumDiagnosticLength = 4095;

    internal static string? Render(
        CliPresentationRequest<RouteUpdateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={RouteUpdateDefinitions.ReadMachineName(result.Mode)}",
            $"target={Value(result.Target.Id ?? result.Target.Requested)}",
            $"completeness={RouteUpdateDefinitions.ReadMachineName(result.Plan.Completeness)}",
            $"safety={RouteUpdateDefinitions.ReadMachineName(result.Plan.Safety)}",
            $"body={RouteUpdateDefinitions.ReadMachineName(result.Plan.Body)}",
            string.Create(CultureInfo.InvariantCulture, $"effects={result.Effects.Length}"),
            $"recovery={RouteUpdateDefinitions.ReadMachineName(result.Recovery.State)}",
            $"verification={RouteUpdateDefinitions.ReadMachineName(result.Verification)}",
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Length}"),
        };
        values.AddRange(result.Findings.Select(finding =>
            $"finding={RouteUpdateDefinitions.ReadMachineName(finding.Code)}:target={Value(finding.Target)}:cause={Value(finding.Cause)}"));
        return RouteUpdateTextEscaping.Escape(
            string.Join("; ", values),
            MaximumDiagnosticLength);
    }

    private static string Value(string? value)
        => value is null
            ? "none"
            : RouteUpdateTextEscaping.Escape(
                value,
                RouteUpdateTextEscaping.DiagnosticValueLimit);
}
