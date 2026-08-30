using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Rendering;

internal static class ExtensionCreateDiagnosticRenderer
{
    internal static string? Render(CliPresentationRequest<ExtensionCreateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"catalogue={Value(result.Catalogue)}",
            $"destination={Value(result.Destination)}",
            $"id={Value(result.StableId)}",
            $"mode={ExtensionCreateDefinitions.ReadMachineName(result.Mode)}",
            string.Create(CultureInfo.InvariantCulture, $"intended={result.IntendedEffects.Count}"),
            string.Create(CultureInfo.InvariantCulture, $"applied={result.AppliedEffects.Count}"),
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Count}"),
        };
        foreach (var finding in result.Findings)
        {
            values.Add(
                $"finding={ExtensionCreateDefinitions.ReadFindingCode(finding.Code)}:subject={Value(finding.Subject)}:cause={Value(finding.Cause)}");
        }

        return ExtensionCreateTextEscaping.Clamp(
            string.Join("; ", values),
            CliRenderingStage.MaximumDiagnosticLength);
    }

    private static string Value(string? value)
        => value is null
            ? "none"
            : ExtensionCreateTextEscaping.Clamp(
                ExtensionCreateTextEscaping.Escape(value),
                ExtensionCreateTextEscaping.DiagnosticValueLimit);
}
