using System.Globalization;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Rendering;

internal static class IndexDiagnosticRenderer
{
    private const int MaximumDiagnosticLength = 4095;

    internal static string? Render(CliPresentationRequest<IndexResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={IndexDefinitions.ReadMachineName(result.Mode)}",
            $"selection={IndexDefinitions.ReadMachineName(result.Selection.Scope)}",
            string.Create(CultureInfo.InvariantCulture, $"regions={result.Counts.Regions}"),
            string.Create(CultureInfo.InvariantCulture, $"updates={result.Counts.Updates}"),
            string.Create(CultureInfo.InvariantCulture, $"applied={result.Counts.Applied}"),
            string.Create(CultureInfo.InvariantCulture, $"verified={result.Counts.Verified}"),
            $"recovery={IndexDefinitions.ReadMachineName(result.Recovery.State)}",
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Count}"),
        };
        foreach (var finding in result.Findings)
        {
            values.Add(
                $"finding={IndexDefinitions.ReadMachineName(finding.Code)}:cause={CommandTextEscaping.Escape(finding.Cause, CommandTextEscaping.DiagnosticValueLimit)}");
        }

        return CommandTextEscaping.Escape(
            string.Join("; ", values),
            MaximumDiagnosticLength);
    }
}
