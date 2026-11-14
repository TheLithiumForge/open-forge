using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusFindingAggregator
{
    internal static IReadOnlyList<StatusFinding> Build(
        StatusObservationSet observations,
        StatusFacts facts)
    {
        var findings = new StatusFindingCollector();
        StatusContextFindingAggregator.Add(findings, observations, facts);
        StatusLifecycleFindingAggregator.Add(findings, observations, facts.Lifecycle);
        StatusRecoveryFindingAggregator.Add(findings, observations.RecoveryResiduals, facts.Recovery);
        foreach (var backup in observations.ToolBackups)
            findings.Add(backup.SafePaths ? StatusFindingCode.ToolBackupPresent : StatusFindingCode.ToolBackupUnavailable,
                backup.Backup, backup.Cause ?? "A sibling backup is present at a Tool control path.");
        return [.. findings.Complete(), .. facts.Library.Findings];
    }
}
