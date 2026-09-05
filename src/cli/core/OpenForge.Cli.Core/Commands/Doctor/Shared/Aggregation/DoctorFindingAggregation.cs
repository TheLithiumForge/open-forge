using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;

internal static class DoctorFindingAggregation
{
    internal static IReadOnlyList<DoctorFinding> Order(IEnumerable<DoctorFinding> findings)
        => findings
            .OrderBy(finding => finding.Kind)
            .ThenBy(finding => finding.Subject.Kind)
            .ThenBy(finding => finding.Subject.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Subject.Identifier, StringComparer.Ordinal)
            .ThenBy(finding => finding.Subject.Location?.ByteOffset)
            .ToArray();

    internal static IReadOnlyList<DoctorNextAction> Actions(IEnumerable<DoctorFinding> findings)
        => findings.SelectMany(finding => finding.Actions)
            .Distinct()
            .OrderBy(action => action.Kind)
            .ThenBy(action => action.Operation)
            .ThenBy(action => action.Command, StringComparer.Ordinal)
            .ThenBy(action => action.Reason, StringComparer.Ordinal)
            .ToArray();

    internal static DoctorFindingCounts Count(IEnumerable<DoctorFinding> findings)
    {
        var values = findings.ToArray();
        return new DoctorFindingCounts
        {
            Resolution = new DoctorResolutionCounts
            {
                SafeExact = Available(values.LongCount(value => value.Resolution == DoctorResolutionLane.SafeExact)),
                GuidedChoice = Available(values.LongCount(value => value.Resolution == DoctorResolutionLane.GuidedChoice)),
                TargetedOperation = Available(values.LongCount(value => value.Resolution == DoctorResolutionLane.TargetedOperation)),
                ManualDecision = Available(values.LongCount(value => value.Resolution == DoctorResolutionLane.ManualDecision)),
                BlockedRepair = Available(values.LongCount(value => value.Resolution == DoctorResolutionLane.BlockedRepair)),
                Informational = Available(values.LongCount(value => value.Resolution == DoctorResolutionLane.Informational)),
            },
            Severity = new DoctorSeverityCounts
            {
                Information = Available(values.LongCount(value => value.Severity == DoctorFindingSeverity.Information)),
                Warning = Available(values.LongCount(value => value.Severity == DoctorFindingSeverity.Warning)),
                Error = Available(values.LongCount(value => value.Severity == DoctorFindingSeverity.Error)),
            },
        };
    }

    private static DoctorCount Available(long value)
        => new() { State = OperationalValueState.Available, Value = value };
}
