using OpenForge.Cli.Core.Framework.Extensions.Embedded;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models.Registration;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational;

internal sealed class ExtensionLifecycleDoctorReader(
    ExtensionSourceObservationReader sourceReader,
    ExtensionLifecycleTargetReader targetReader,
    ExtensionBridgeRegistrationObservationReader bridgeReader,
    PhysicalPathResolver? physicalPathResolver = null)
{
    internal async ValueTask<ExtensionLifecycleDoctorView> ReadAsync(
        CliWorkspace workspace, WorkspaceOwnershipRead ownership, bool includeEmbedded, CancellationToken cancellationToken)
    {
        var observation = await new WorkspaceLayoutReader(physicalPathResolver ?? new PhysicalPathResolver()).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        var retained = new WorkspaceLayoutRead(observation.State, observation.Settings, ownership, observation.Layout, observation.Issues, observation.Expectations);
        return await ReadAsync(retained, includeEmbedded, cancellationToken).ConfigureAwait(false);
    }

    internal async ValueTask<ExtensionLifecycleDoctorView> ReadAsync(
        WorkspaceLayoutRead observation,
        bool includeEmbedded,
        CancellationToken cancellationToken)
    {
        var ownership = observation.Ownership;
        var layout = observation.Layout;
        var cause = observation.Issues.FirstOrDefault()?.Cause;
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
        IReadOnlyList<ExtensionSourceObservation> sources = layout is null
            ? packages.Select(package => package.Source).Distinct(StringComparer.Ordinal)
                .Select(source => new ExtensionSourceObservation(source, new ExtensionSourceReadResult(
                    ExtensionSourceReadState.Unavailable, null, source ?? EmbeddedExtensionCatalogueReader.Identity, [], cause))).ToArray()
            : await sourceReader.ReadAsync(layout.Workspace, packages, includeEmbedded, cancellationToken).ConfigureAwait(false);
        var settings = observation.Settings;
        FrontmatterForm? form = settings.State is WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete
            ? settings.Document.Frontmatter : null;
        var targets = await targetReader.ReadDoctorAsync(layout, ownership.Document, sources, form, cancellationToken, cause)
            .ConfigureAwait(false);
        var sharing = ownership.State is WorkspaceOwnershipReadState.Complete or WorkspaceOwnershipReadState.Absent
            ? new SourceSharing(ownership.Document.Framework?.GitIgnoredRoutes ?? []) : null;
        var bridges = layout is null
            ? ExtensionBridgeRegistrationFacts.Incomplete(cause ?? "The workspace layout is unavailable.")
            : await bridgeReader.ReadAsync(layout, ownership.Document, sources, sharing, cancellationToken).ConfigureAwait(false);
        var comparisons = packages.Select(package => Compare(package, sources)).ToArray();
        var sourceAvailability = packages.Length == 0
            ? OperationalSourceAvailability.NotApplicable
            : layout is not null && sources.All(source => ExtensionSourceObservationReader.ReadAvailability(source.Read)
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
