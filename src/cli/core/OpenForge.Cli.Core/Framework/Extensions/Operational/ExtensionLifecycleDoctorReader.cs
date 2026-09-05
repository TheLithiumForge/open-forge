using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational;

internal sealed class ExtensionLifecycleDoctorReader(
    LifecycleDocumentReader lifecycleReader,
    ExtensionSourceObservationReader sourceReader,
    ExtensionLifecycleTargetReader targetReader,
    LifecycleOwnershipReader ownershipReader)
{
    internal async ValueTask<ExtensionLifecycleDoctorView> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var lifecycle = await lifecycleReader
            .ReadExtensionsAsync(workspace, cancellationToken)
            .ConfigureAwait(false);
        var sources = await sourceReader
            .ReadAsync(workspace, lifecycle.Packages, includeEmbedded: true, cancellationToken)
            .ConfigureAwait(false);
        var ownership = await ownershipReader
            .ReadAsync(workspace, cancellationToken)
            .ConfigureAwait(false);
        var targets = await targetReader
            .ReadDoctorAsync(workspace, lifecycle.Paths, cancellationToken)
            .ConfigureAwait(false);
        var packages = lifecycle.Packages
            .Select(package => Compare(package, sources))
            .ToArray();
        var sourceAvailability = lifecycle.State == LifecycleReadState.Complete
            && lifecycle.Trust == LifecycleExtensionTrust.Absent
            ? OperationalSourceAvailability.NotApplicable
            : ExtensionLifecycleEvaluation.ReadSourceAvailability(lifecycle, sources);
        var assessment = ExtensionLifecycleDoctorAssessment.Create(
            ExtensionLifecycleEvaluation.ReadViewState(lifecycle),
            ReadSection(lifecycle),
            ReadManagedSet(lifecycle, targets),
            ExtensionLifecycleEvaluation.ReadLifecycleState(lifecycle),
            sourceAvailability);
        return ExtensionLifecycleDoctorView.Create(
            assessment,
            lifecycle,
            sources,
            ownership,
            ExtensionLifecycleDoctorFacts.Create(targets, packages));
    }

    private static ExtensionLifecycleSectionState ReadSection(
        LifecycleReadResult lifecycle)
        => lifecycle.State switch
        {
            LifecycleReadState.Complete => ExtensionLifecycleSectionState.Present,
            LifecycleReadState.Missing => ExtensionLifecycleSectionState.DocumentMissing,
            LifecycleReadState.Invalid when lifecycle.Trust == LifecycleExtensionTrust.Blocked =>
                ExtensionLifecycleSectionState.Blocked,
            LifecycleReadState.Invalid => ExtensionLifecycleSectionState.Invalid,
            LifecycleReadState.Unavailable => ExtensionLifecycleSectionState.Unavailable,
            LifecycleReadState.Cancelled => ExtensionLifecycleSectionState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(lifecycle),
                lifecycle.State,
                "The Extension lifecycle section state is not defined."),
        };

    private static ExtensionManagedSetState ReadManagedSet(
        LifecycleReadResult lifecycle,
        IReadOnlyList<ExtensionManagedTargetDoctorObservation> targets)
    {
        if (targets.Count == 0)
        {
            return ExtensionManagedSetState.Empty;
        }

        if (lifecycle.Trust != LifecycleExtensionTrust.Trusted
            || targets.Any(target => target.Target.State is OperationalTargetState.Unavailable
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
        LifecycleInstalledPackage package,
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
