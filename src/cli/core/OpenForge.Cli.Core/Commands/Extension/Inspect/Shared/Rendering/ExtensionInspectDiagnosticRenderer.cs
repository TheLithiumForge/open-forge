using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectDiagnosticRenderer
{
    private const int MaximumDiagnosticLength = 4095;

    internal static string? Render(CliPresentationRequest<ExtensionInspectResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"workspace={Value(result.Workspace?.LexicalRoot)}",
            $"subject={Value(result.Subject.Id)}",
            $"source={Value(result.Source.Identity)}",
            $"source-state={ExtensionInspectWireVocabulary.SourceState(result.Source.State)}",
            $"lifecycle={ExtensionInspectWireVocabulary.LifecycleTrust(result.Lifecycle.Trust)}",
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Count}"),
        };
        foreach (var finding in result.Findings)
        {
            values.Add(
                $"finding={ExtensionInspectDefinitions.ReadFindingCode(finding.Code)}:subject={Value(finding.Subject)}:path={Value(finding.Path)}:cause={Value(finding.Cause)}");
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
