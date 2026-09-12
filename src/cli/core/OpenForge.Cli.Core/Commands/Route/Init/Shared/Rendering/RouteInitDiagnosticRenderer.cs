using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;

internal static class RouteInitDiagnosticRenderer
{
    private const int MaximumDiagnosticLength = 4095;

    internal static string? Render(
        CliPresentationRequest<RouteInitResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={RouteInitDefinitions.ReadMachineName(result.Mode)}",
            $"scaffold={RouteInitDefinitions.ReadMachineName(result.Scaffold)}",
            $"target={Value(result.Target.Id ?? result.Target.Requested)}",
            $"completeness={RouteInitDefinitions.ReadMachineName(result.Plan.Completeness)}",
            $"safety={RouteInitDefinitions.ReadMachineName(result.Plan.Safety)}",
            string.Create(CultureInfo.InvariantCulture, $"entrypoints={result.Entrypoints.Length}"),
            string.Create(CultureInfo.InvariantCulture, $"effects={result.Effects.Length}"),
            $"lifecycle={RouteInitDefinitions.ReadMachineName(result.Lifecycle.Action)}/{RouteInitDefinitions.ReadMachineName(result.Lifecycle.Outcome)}",
            $"recovery={RouteInitDefinitions.ReadMachineName(result.Recovery.State)}",
            $"verification={RouteInitDefinitions.ReadMachineName(result.Verification)}",
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Length}"),
            $"next={(result.Next is null ? "none" : "present")}",
        };
        foreach (var finding in result.Findings)
        {
            values.Add(
                $"finding={RouteInitDefinitions.ReadMachineName(finding.Code)}:target={Value(finding.Target)}:cause={Value(finding.Cause)}");
        }

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
