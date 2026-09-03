using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;

internal static class RouteMoveDiagnosticRenderer
{
    private const int MaximumDiagnosticLength = 4095;

    internal static string? Render(CliPresentationRequest<RouteMoveResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={RouteMoveDefinitions.ReadMachineName(result.Mode)}",
            $"source={Value(result.Source.Id ?? result.Source.Requested)}",
            $"destination={Value(result.Destination.Id ?? result.Destination.Requested)}",
            $"subject={Optional(result.Subject.Kind)}",
            $"completeness={RouteMoveDefinitions.ReadMachineName(result.Plan.Completeness)}",
            $"safety={RouteMoveDefinitions.ReadMachineName(result.Plan.Safety)}",
            string.Create(CultureInfo.InvariantCulture, $"effects={result.Effects.Length}"),
            $"recovery={RouteMoveDefinitions.ReadMachineName(result.Recovery.State)}",
            $"verification={RouteMoveDefinitions.ReadMachineName(result.Verification)}",
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Length}"),
        };
        values.AddRange(result.Findings.Select(finding =>
            $"finding={RouteMoveDefinitions.ReadMachineName(finding.Code)}:target={Value(finding.Target)}:cause={Value(finding.Cause)}"));
        return RouteTextEscaping.Escape(
            string.Join("; ", values),
            MaximumDiagnosticLength);
    }

    private static string Optional(RouteMoveSubjectKind? kind)
        => kind is { } established
            ? RouteMoveDefinitions.ReadMachineName(established)
            : "none";

    private static string Value(string? value)
        => value is null
            ? "none"
            : RouteTextEscaping.Escape(
                value,
                RouteTextEscaping.DiagnosticValueLimit);
}
