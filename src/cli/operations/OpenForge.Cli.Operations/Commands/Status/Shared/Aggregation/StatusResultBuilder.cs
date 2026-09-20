using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusResultBuilder
{
    internal static StatusResult Build(StatusRequest request, StatusObservationSet observations)
    {
        var facts = new StatusFacts
        {
            Installation = new StatusInstallation(
                StatusStateMap.Installation(observations.WorkspaceEntry.Installation),
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
                StatusLifecycleAbsenceResolver.IsProven(observations),
                observations.Routes.GeneratedNavigation
                    .Where(target => target.State == OperationalGeneratedNavigationState.Current)
                    .Select(target => target.Path)
                    .ToHashSet(StringComparer.Ordinal)),
            Library = StatusLibraryAggregator.Build(observations.Libraries),
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

    internal static StatusResult Event(
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
                Installation = new StatusInstallation(StatusInstallationState.Blocked, null, null),
                Context = StatusContextAggregator.Unavailable(),
                Structure = StatusStructureAggregator.Unavailable(),
                Lifecycle = StatusLifecycleAggregator.Unavailable(),
                Library = StatusLibraryAggregator.Unavailable("The Library observation was not completed for this Status event."),
                Recovery = StatusRecoveryAggregator.Unavailable(),
            },
            Findings = findings,
        };
    }
}
