using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Rendering;

internal static class RouteCreateDiagnosticRenderer
{
    private const int MaximumDiagnosticLength = 4095;

    internal static string? Render(
        CliPresentationRequest<RouteCreateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={RouteCreateDefinitions.ReadMachineName(result.Mode)}",
            $"target={Value(result.Target.Id ?? result.Target.Requested)}",
            $"completeness={RouteCreateDefinitions.ReadMachineName(result.Plan.Completeness)}",
            $"safety={RouteCreateDefinitions.ReadMachineName(result.Plan.Safety)}",
            string.Create(CultureInfo.InvariantCulture, $"effects={result.Effects.Length}"),
            $"recovery={RouteCreateDefinitions.ReadMachineName(result.Recovery.State)}",
            $"verification={RouteCreateDefinitions.ReadMachineName(result.Verification)}",
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Length}"),
        };
        values.AddRange(result.Findings.Select(finding =>
            $"finding={RouteCreateDefinitions.ReadMachineName(finding.Code)}:target={Value(finding.Target)}:cause={Value(finding.Cause)}"));
        return RouteTextEscaping.Escape(
            string.Join("; ", values),
            MaximumDiagnosticLength);
    }

    private static string Value(string? value)
        => value is null
            ? "none"
            : RouteTextEscaping.Escape(
                value,
                RouteTextEscaping.DiagnosticValueLimit);
}
