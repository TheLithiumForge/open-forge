using System.Globalization;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

internal static class UpdateDiagnosticRenderer
{
    private const int MaximumDiagnosticLength = 4095;

    internal static string? Render(CliPresentationRequest<UpdateResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var lifecycleTrust = UpdateDefinitions.ReadMachineName(result.Lifecycle.Trust);
        var lifecycleCoverage = UpdateDefinitions.ReadMachineName(result.Lifecycle.Coverage);
        var lifecycleAction = UpdateDefinitions.ReadMachineName(result.Lifecycle.Action);
        var lifecycleOutcome = UpdateDefinitions.ReadMachineName(result.Lifecycle.Outcome);
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={UpdateDefinitions.ReadMachineName(result.Mode)}",
            $"force={MachineBoolean(result.Force)}",
            $"prune={MachineBoolean(result.Prune)}",
            $"automatic={MachineBoolean(result.Automatic)}",
            $"source={(result.Source is null ? "none" : "present")}",
            string.Create(CultureInfo.InvariantCulture, $"comparisons={result.Comparisons.Count}"),
            $"generated-navigation={(result.GeneratedNavigation is null ? "none" : UpdateDefinitions.ReadMachineName(result.GeneratedNavigation.Coverage))}",
            string.Create(CultureInfo.InvariantCulture, $"effects={result.Effects.Count}"),
            $"lifecycle={lifecycleTrust}/{lifecycleCoverage}/{lifecycleAction}/{lifecycleOutcome}",
            $"recovery={UpdateDefinitions.ReadMachineName(result.Recovery.State)}",
            $"verification={UpdateDefinitions.ReadMachineName(result.Verification)}",
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Count}"),
            $"next={(result.Next is null ? "none" : "present")}",
        };
        foreach (var finding in result.Findings)
        {
            var target = finding.Target is null
                ? "none"
                : UpdateTextEscaping.Escape(
                    finding.Target,
                    UpdateTextEscaping.DiagnosticValueLimit);
            values.Add(
                $"finding={UpdateDefinitions.ReadMachineName(finding.Code)}:target={target}:cause={UpdateTextEscaping.Escape(finding.Cause, UpdateTextEscaping.DiagnosticValueLimit)}");
        }

        return UpdateTextEscaping.Escape(
            string.Join("; ", values),
            MaximumDiagnosticLength);
    }

    private static string MachineBoolean(bool value) => value ? "true" : "false";
}
