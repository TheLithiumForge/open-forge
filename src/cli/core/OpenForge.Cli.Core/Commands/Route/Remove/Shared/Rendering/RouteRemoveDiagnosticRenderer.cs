using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Rendering;

internal static class RouteRemoveDiagnosticRenderer
{
    private const int MaximumDiagnosticLength = 4095;

    internal static string? Render(CliPresentationRequest<RouteRemoveResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={RouteRemoveDefinitions.ReadMachineName(result.Mode)}",
            $"source={Value(result.Source.Id ?? result.Source.Requested)}",
            $"subject={Optional(result.Subject.Kind)}",
            $"completeness={RouteRemoveDefinitions.ReadMachineName(result.Plan.Completeness)}",
            $"safety={RouteRemoveDefinitions.ReadMachineName(result.Plan.Safety)}",
            string.Create(CultureInfo.InvariantCulture, $"effects={result.Effects.Length}"),
            $"recovery={RouteRemoveDefinitions.ReadMachineName(result.Recovery.State)}",
            $"verification={RouteRemoveDefinitions.ReadMachineName(result.Verification)}",
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Length}"),
        };
        values.AddRange(result.Findings.Select(finding =>
            $"finding={RouteRemoveDefinitions.ReadMachineName(finding.Code)}:target={Value(finding.Target)}:cause={Value(finding.Cause)}"));
        return RouteTextEscaping.Escape(
            string.Join("; ", values),
            MaximumDiagnosticLength);
    }

    private static string Optional(RouteRemoveSubjectKind? kind)
        => kind is { } established
            ? RouteRemoveDefinitions.ReadMachineName(established)
            : "none";

    private static string Value(string? value)
        => value is null
            ? "none"
            : RouteTextEscaping.Escape(
                value,
                RouteTextEscaping.DiagnosticValueLimit);
}
