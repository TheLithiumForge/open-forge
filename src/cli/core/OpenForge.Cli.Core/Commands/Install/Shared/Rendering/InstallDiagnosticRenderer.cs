using System.Globalization;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Rendering;

internal static class InstallDiagnosticRenderer
{
    private const int MaximumDiagnosticLength = 4095;

    internal static string? Render(CliPresentationRequest<InstallResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var facts = result.Facts;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"mode={InstallDefinitions.ReadMachineName(result.Mode)}",
            $"force={MachineBoolean(result.Force)}",
            $"automatic={MachineBoolean(result.Automatic)}",
            $"classification={(facts.Classification is { } classification ? InstallDefinitions.ReadMachineName(classification) : "none")}",
            string.Create(CultureInfo.InvariantCulture, $"effects={facts.Effects.Count}"),
            $"lifecycle={InstallDefinitions.ReadMachineName(facts.Lifecycle.Action)}/{InstallDefinitions.ReadMachineName(facts.Lifecycle.Outcome)}",
            $"recovery={InstallDefinitions.ReadMachineName(facts.Recovery.State)}",
            $"verification={InstallDefinitions.ReadMachineName(facts.Verification.State)}",
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Count}"),
            $"next={(result.Next is null ? "none" : "present")}",
        };
        foreach (var finding in result.Findings)
        {
            var target = finding.Subject is null
                ? "none"
                : InstallTextEscaping.Escape(
                    finding.Subject,
                    InstallTextEscaping.DiagnosticValueLimit);
            values.Add(
                $"finding={InstallDefinitions.ReadMachineName(finding.Code)}:target={target}:cause={InstallTextEscaping.Escape(finding.Cause, InstallTextEscaping.DiagnosticValueLimit)}");
        }

        return InstallTextEscaping.Escape(
            string.Join("; ", values),
            MaximumDiagnosticLength);
    }

    private static string MachineBoolean(bool value) => value ? "true" : "false";
}
