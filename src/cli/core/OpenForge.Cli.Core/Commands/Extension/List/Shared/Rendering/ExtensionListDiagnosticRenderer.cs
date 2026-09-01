using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;

internal static class ExtensionListDiagnosticRenderer
{
    private const int MaximumDiagnosticLength = 4095;

    internal static string? Render(CliPresentationRequest<ExtensionListResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"workspace={Value(result.Workspace?.LexicalRoot)}",
            $"source={Value(result.Source?.Identity)}",
            $"source-state={(result.Source is null ? "none" : ExtensionListJsonProjection.SourceState(result.Source.State))}",
            string.Create(CultureInfo.InvariantCulture, $"installed={result.Installed.Count}"),
            string.Create(CultureInfo.InvariantCulture, $"available={result.Available.Count}"),
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Count}"),
        };
        foreach (var finding in result.Findings)
        {
            values.Add(
                $"finding={ExtensionListDefinitions.ReadFindingCode(finding.Code)}:subject={Value(finding.Subject)}:cause={Value(finding.Cause)}");
        }

        return ExtensionTextEscaping.Clamp(string.Join("; ", values), MaximumDiagnosticLength);
    }

    private static string Value(string? value)
        => value is null
            ? "none"
            : ExtensionTextEscaping.Clamp(
                ExtensionTextEscaping.Escape(value),
                ExtensionTextEscaping.DiagnosticValueLimit);
}
