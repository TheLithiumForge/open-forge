using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorCountsHumanRenderer
{
    internal static void Append(StringBuilder builder, DoctorFindingCounts counts, string indent)
    {
        builder.AppendLine($"{indent}resolution counts:");
        Append(builder, indent, "safe-exact", counts.Resolution.SafeExact);
        Append(builder, indent, "guided-choice", counts.Resolution.GuidedChoice);
        Append(builder, indent, "targeted-operation", counts.Resolution.TargetedOperation);
        Append(builder, indent, "manual-decision", counts.Resolution.ManualDecision);
        Append(builder, indent, "blocked-repair", counts.Resolution.BlockedRepair);
        Append(builder, indent, "informational", counts.Resolution.Informational);
        builder.AppendLine($"{indent}severity counts:");
        Append(builder, indent, "information", counts.Severity.Information);
        Append(builder, indent, "warning", counts.Severity.Warning);
        Append(builder, indent, "error", counts.Severity.Error);
    }

    private static void Append(StringBuilder builder, string indent, string label, DoctorCount count)
    {
        var value = count.State == OperationalValueState.Available && count.Value is { } available
            ? available.ToString(CultureInfo.InvariantCulture)
            : DoctorWireVocabulary.ValueState(count.State);
        builder.AppendLine($"{indent}  {label}: {value}");
    }
}
