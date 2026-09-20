using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational;

internal sealed class ExtensionLifecycleDoctorReader(
    ExtensionSourceObservationReader sourceReader,
    ExtensionLifecycleTargetReader targetReader,
    ExtensionBridgeRegistrationObservationReader bridgeReader)
{
    internal async ValueTask<ExtensionLifecycleDoctorView> ReadAsync(
        CliWorkspace workspace,
        WorkspaceOwnershipRead ownership,
        bool includeEmbedded,
        CancellationToken cancellationToken)
    {
        if (ownership.Document.Extensions.GroupBy(package => package.Id, StringComparer.Ordinal).Any(group => group.Count() > 1))
        {
            ownership = ownership with
            {
                State = WorkspaceOwnershipReadState.Invalid,
                Document = ownership.Document with { Extensions = [] },
                Cause = "The lock contains ambiguous Extension identifiers; no Extension ownership claims were used.",
            };
        }
        var packages = ownership.Document.Extensions;
        var sources = await sourceReader.ReadAsync(workspace, packages, includeEmbedded, cancellationToken)
            .ConfigureAwait(false);
        var targets = await targetReader.ReadDoctorAsync(workspace, ownership.Document, sources, cancellationToken)
            .ConfigureAwait(false);
        var bridges = await bridgeReader.ReadAsync(workspace, ownership.Document, sources, cancellationToken)
            .ConfigureAwait(false);
        var comparisons = packages.Select(package => Compare(package, sources)).ToArray();
        var sourceAvailability = packages.Length == 0
            ? OperationalSourceAvailability.NotApplicable
            : sources.All(source => ExtensionSourceObservationReader.ReadAvailability(source.Read)
                == OperationalSourceAvailability.Available)
                ? OperationalSourceAvailability.Available : OperationalSourceAvailability.Unavailable;
        var state = targets.Any(target => target.Target.State == OperationalTargetState.Blocked)
            ? OperationalViewState.Blocked
            : sourceAvailability == OperationalSourceAvailability.Unavailable
                || targets.Any(target => target.Target.State == OperationalTargetState.Unavailable)
                ? OperationalViewState.Incomplete : OperationalViewState.Complete;
        var assessment = ExtensionLifecycleDoctorAssessment.Create(
            state,
            ReadSection(ownership),
            ReadManagedSet(targets),
            ownership.State == WorkspaceOwnershipReadState.Complete
                ? OperationalLifecycleState.Trusted : OperationalLifecycleState.Incomplete,
            sourceAvailability);
        return ExtensionLifecycleDoctorView.Create(assessment, sources, ownership,
            ExtensionLifecycleDoctorFacts.Create(targets, comparisons, bridges));
    }

    private static ExtensionLifecycleSectionState ReadSection(WorkspaceOwnershipRead ownership)
        => ownership.State switch
        {
            WorkspaceOwnershipReadState.Complete => ExtensionLifecycleSectionState.Present,
            WorkspaceOwnershipReadState.Absent => ExtensionLifecycleSectionState.DocumentMissing,
            WorkspaceOwnershipReadState.Invalid => ExtensionLifecycleSectionState.Invalid,
            WorkspaceOwnershipReadState.Unavailable => ExtensionLifecycleSectionState.Unavailable,
            _ => throw new ArgumentOutOfRangeException(nameof(ownership)),
        };

    private static ExtensionManagedSetState ReadManagedSet(
        IReadOnlyList<ExtensionManagedTargetDoctorObservation> targets)
    {
        if (targets.Count == 0)
        {
            return ExtensionManagedSetState.Empty;
        }

        if (targets.Any(target => target.Target.State is OperationalTargetState.Unavailable
                or OperationalTargetState.Blocked))
        {
            return ExtensionManagedSetState.Unavailable;
        }

        var current = targets.Count(target =>
            target.Target.State == OperationalTargetState.Current);
        if (current == targets.Count)
        {
            return ExtensionManagedSetState.Current;
        }

        return current == 0
            ? ExtensionManagedSetState.NonCurrent
            : ExtensionManagedSetState.Mixed;
    }

    private static ExtensionInstalledPackageComparison Compare(
        ExtensionOwnership package,
        IReadOnlyList<ExtensionSourceObservation> sources)
    {
        var source = sources.Single(observation => string.Equals(
            observation.RecordedSource,
            package.Source,
            StringComparison.Ordinal));
        var installed = ExtensionSourceObservationReader.Project(package, sources);
        if (source.Read.State != ExtensionSourceReadState.Complete)
        {
            return ExtensionInstalledPackageComparison.SourceUnavailable(
                installed,
                source.Read.Cause ?? "The exact Extension package source is unavailable.");
        }

        var matches = source.Read.Packages
            .Where(candidate => string.Equals(candidate.Id, package.Id, StringComparison.Ordinal))
            .ToArray();
        if (matches.Length == 0)
        {
            return ExtensionInstalledPackageComparison.Missing(installed);
        }

        if (matches.Length > 1)
        {
            return ExtensionInstalledPackageComparison.Ambiguous(installed);
        }

        if (!string.Equals(matches[0].Version, package.Version, StringComparison.Ordinal))
        {
            return ExtensionInstalledPackageComparison.VersionMismatch(installed, matches[0]);
        }

        return matches[0].Dependencies.SequenceEqual(package.Dependencies, StringComparer.Ordinal)
            ? ExtensionInstalledPackageComparison.Current(installed, matches[0])
            : ExtensionInstalledPackageComparison.DependencyMismatch(installed, matches[0]);
    }
}
