using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorCountsHumanRenderer
{
    internal static void Append(StringBuilder builder, DoctorFindingCounts counts, string indent)
    {
        var severity = counts.Severity;
        var resolution = counts.Resolution;
        DoctorCount[] values = [severity.Error, severity.Warning, severity.Information, resolution.SafeExact, resolution.GuidedChoice,
            resolution.TargetedOperation, resolution.ManualDecision, resolution.BlockedRepair, resolution.Informational];
        if (values.All(value => value is { State: OperationalValueState.Available, Value: 0 }))
        {
            builder.AppendLine($"{indent}Findings: none");
            return;
        }

        var errors = Count(severity.Error, "error", "errors");
        var warnings = Count(severity.Warning, "warning", "warnings");
        var information = Count(severity.Information, "informational finding", "informational findings");
        var exact = Count(resolution.SafeExact, "exact repair", "exact repairs");
        var choices = Count(resolution.GuidedChoice, "choice", "choices");
        var commands = Count(resolution.TargetedOperation, "targeted command", "targeted commands");
        var manual = Count(resolution.ManualDecision, "manual decision", "manual decisions");
        var blocked = Count(resolution.BlockedRepair, "blocked repair", "blocked repairs");
        var informational = Count(resolution.Informational, "informational finding", "informational findings");
        builder.AppendLine($"""
            {indent}Findings: {errors}, {warnings}, {information}
            {indent}Resolution: {exact}, {choices}, {commands}
            {indent}  {manual}, {blocked}, {informational}
            """);
    }

    private static string Count(DoctorCount count, string singular, string plural)
    {
        if (count.State != OperationalValueState.Available || count.Value is null)
        {
            return $"{singular} count {Value(count)}";
        }

        return $"{Value(count)} {(count.Value == 1 ? singular : plural)}";
    }

    private static string Value(DoctorCount count)
        => count.State == OperationalValueState.Available && count.Value is { } available
            ? available.ToString(CultureInfo.InvariantCulture)
            : DoctorWireVocabulary.ValueState(count.State);
}
