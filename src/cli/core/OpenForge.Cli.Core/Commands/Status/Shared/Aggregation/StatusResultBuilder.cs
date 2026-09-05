using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal sealed class StatusResultBuilder
{
    internal StatusResult Build(StatusRequest request, StatusObservationSet observations)
    {
        var facts = new StatusFacts
        {
            Installation = new StatusInstallation(
                observations.WorkspaceEntry.Installation,
                observations.WorkspaceEntry.EntryPath,
                observations.WorkspaceEntry.LoaderPath),
            Context = StatusContextAggregator.Build(
                observations.Routes,
                observations.WorkspaceEntry.Installation),
            Structure = StatusStructureAggregator.Build(
                observations.Routes,
                observations.WorkspaceEntry.Installation),
            Lifecycle = StatusLifecycleAggregator.Build(
                observations.FrameworkLifecycle,
                observations.ExtensionLifecycle,
                StatusLifecycleAbsenceResolver.IsProven(observations)),
            Recovery = StatusRecoveryAggregator.Build(observations.RecoveryResiduals),
        };
        var findings = StatusFindingAggregator.Build(observations, facts);
        var status = StatusResultPolicy.ReadStatus(findings);
        return new StatusResult
        {
            Status = status,
            Workspace = request.Workspace,
            Next = StatusResultPolicy.ReadNext(status, findings),
            Facts = facts,
            Findings = findings,
        };
    }

    internal StatusResult Event(
        CliWorkspace? workspace,
        StatusFindingCode findingCode,
        string? subject,
        string cause)
    {
        var finding = new StatusFinding
        {
            Code = findingCode,
            Status = StatusDefinitions.ReadFindingStatus(findingCode),
            Subject = subject,
            Cause = cause,
        };
        var findings = new[] { finding };
        var status = StatusResultPolicy.ReadStatus(findings);
        return new StatusResult
        {
            Status = status,
            Workspace = workspace,
            Next = StatusResultPolicy.ReadNext(status, findings),
            Facts = new StatusFacts
            {
                Installation = new StatusInstallation(OperationalInstallationState.Blocked, null, null),
                Context = StatusContextAggregator.Unavailable(),
                Structure = StatusStructureAggregator.Unavailable(),
                Lifecycle = StatusLifecycleAggregator.Unavailable(),
                Recovery = StatusRecoveryAggregator.Unavailable(),
            },
            Findings = findings,
        };
    }
}
