using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Maintenance;
using OpenForge.Cli.Core.Framework.OperationalContributors;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;

internal static class DoctorResultBuilder
{
    internal static DoctorResult Build(DoctorRequest request, DoctorObservation observation)
    {
        var lifecycleAbsence = OperationalLifecycleAbsenceProof.IsProven(
            OperationalLifecycleAbsenceEvidence.FromDoctor(
                observation.WorkspaceEntry,
                observation.Routes,
                observation.RecoveryResiduals,
                observation.FrameworkLifecycle,
                observation.ExtensionLifecycle));
        var domains = new DoctorDomainReport[]
        {
            LibraryDoctorInspector.Inspect(
                request.Workspace,
                WorkspaceEntryDoctorInspector.Inspect(
                    request.Workspace, observation.WorkspaceEntry, observation.Routes,
                    missingLinkScopeEntries: observation.LocalReferences.MissingLinkScopeEntries,
                    unrecognizedSettingsSchemaVersion: observation.LocalReferences.UnrecognizedSettingsSchemaVersion),
                observation.Libraries,
                observation.RecoveryResiduals,
                observation.LibraryResiduals),
            RecoveryResidualDoctorInspector.Inspect(observation.RecoveryResiduals),
            RouteDoctorInspector.Inspect(observation.Routes),
            LocalReferenceDoctorInspector.Inspect(observation.LocalReferences, observation.Maintenance),
            FrameworkLifecycleDoctorInspector.Inspect(
                observation.FrameworkLifecycle,
                observation.RecoveryResiduals,
                observation.ExtensionLifecycle.Ownership,
                lifecycleAbsence,
                observation.Routes.GeneratedNavigation
                    .Where(target => target.State == OperationalGeneratedNavigationState.Current)
                    .Select(target => target.Path)
                    .ToHashSet(StringComparer.Ordinal)),
            ExtensionLifecycleDoctorInspector.Inspect(observation.ExtensionLifecycle),
        };
        domains = DoctorMaintenanceGuidance.Apply(domains, observation);
        var layout = observation.Routes.Catalogue?.Layout;
        if (layout is not null) domains = domains.Select(domain => domain with
        {
            Findings = domain.Findings.Select(finding => DoctorLayoutPresentation.Project(layout, finding)).ToArray(),
        }).ToArray();
        var coverage = domains.Max(domain => domain.Coverage);
        var status = DoctorResultPolicy.ReadStatus(coverage, domains);
        return new DoctorResult
        {
            Status = status,
            Workspace = request.Workspace,
            Next = null,
            Diagnosis = new DoctorDiagnosis
            {
                Coverage = coverage,
                Counts = DoctorFindingAggregation.Count(domains.SelectMany(domain => domain.Findings)),
                CoverageCounts = CoverageCounts(observation, domains),
                Actions = DoctorFindingAggregation.Actions(domains.SelectMany(domain => domain.Findings)),
                Domains = domains,
            },
        };
    }

    internal static DoctorResult Event(
        CliWorkspace? workspace,
        CliSemanticStatus status,
        DoctorFindingKind? kind,
        string cause)
    {
        var coverage = status == CliSemanticStatus.Blocked
            ? DoctorCoverageState.Blocked
            : DoctorCoverageState.Incomplete;
        DoctorFinding[] firstFindings = [];
        if (kind is { } findingKind)
        {
            var observedState = status == CliSemanticStatus.Blocked
                ? DoctorObservedState.Blocked
                : DoctorObservedState.Incomplete;
            firstFindings =
            [
                DoctorDomainSupport.Create(
                    DoctorDomainSupport.Error(findingKind, cause),
                    DoctorDomainSupport.Subject(DoctorSubjectKind.Workspace, workspace?.LexicalRoot),
                    DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry, workspace?.LexicalRoot),
                    [new DoctorStateEvidence(observedState)]),
            ];
        }
        var domains = Enum.GetValues<DoctorDomainKind>()
            .Select((domain, index) => EventDomain(domain, coverage, cause, index == 0 ? firstFindings : []))
            .ToArray();
        return new DoctorResult
        {
            Status = status,
            Workspace = workspace,
            Next = null,
            Diagnosis = new DoctorDiagnosis
            {
                Coverage = coverage,
                Counts = UnavailableCounts(),
                CoverageCounts = UnavailableCoverageCounts(),
                Actions = DoctorFindingAggregation.Actions(firstFindings),
                Domains = domains,
            },
        };
    }

    private static DoctorDomainReport EventDomain(
        DoctorDomainKind domain,
        DoctorCoverageState coverage,
        string cause,
        IReadOnlyList<DoctorFinding> findings)
        => new()
        {
            Domain = domain,
            Boundary = new DoctorBoundary { Kind = ReadBoundary(domain), Path = null },
            Coverage = coverage,
            Lifecycle = null,
            SourceAvailability = null,
            Limitations = [DoctorDomainSupport.Limitation(coverage, cause)],
            Counts = UnavailableCounts(),
            Findings = findings,
            Actions = DoctorFindingAggregation.Actions(findings),
        };

    private static DoctorCount Unavailable()
        => new() { State = OperationalValueState.Unavailable, Value = null };

    private static DoctorFindingCounts UnavailableCounts()
    {
        var count = Unavailable();
        return new DoctorFindingCounts
        {
            Resolution = new DoctorResolutionCounts
            {
                SafeExact = count,
                GuidedChoice = count,
                TargetedOperation = count,
                ManualDecision = count,
                BlockedRepair = count,
                Informational = count,
            },
            Severity = new DoctorSeverityCounts
            {
                Information = count,
                Warning = count,
                Error = count,
            },
        };
    }

    private static DoctorCoverageCounts CoverageCounts(
        DoctorObservation observation,
        IReadOnlyList<DoctorDomainReport> domains)
    {
        var references = observation.LocalReferences.References;
        var lifecycle = observation.FrameworkLifecycle;
        var extensions = observation.ExtensionLifecycle;
        var libraries = observation.Libraries;
        return new DoctorCoverageCounts
        {
            Checks = Enum.GetValues<DoctorDomainKind>().LongLength,
            ChecksComplete = domains.LongCount(domain => domain.Coverage == DoctorCoverageState.Complete),
            LinksChecked = observation.LocalReferences.UnavailableCause is null ? references.Count : null,
            LinksIgnored = observation.LocalReferences.IgnoredLinkCount,
            LinksValid = observation.LocalReferences.UnavailableCause is not null ? null : references.LongCount(reference => reference.Facts.Target.Resolution
                == SourceLinkTargetResolution.Complete),
            ExternalLinksNotChecked = observation.LocalReferences.UnavailableCause is not null ? null : references.LongCount(reference => reference.Facts.Target.Resolution
                == SourceLinkTargetResolution.ExternalUnchecked),
            ImageLinks = observation.LocalReferences.UnavailableCause is not null ? null : references.LongCount(reference => reference.Kind == LocalReferenceKind.Image),
            RoutesChecked = observation.Routes.Catalogue is null ? null : observation.Routes.Routes.RouteFacts.Count,
            FrameworkFiles = lifecycle.State == OperationalViewState.Complete
                ? lifecycle.Targets.Count
                : null,
            ExtensionsInstalled = extensions.State == OperationalViewState.Complete
                ? extensions.Packages.Count
                : null,
            LibrariesRegistered = libraries.Record.Record?.Libraries.Length,
        };
    }

    private static DoctorCoverageCounts UnavailableCoverageCounts()
        => new()
        {
            Checks = null,
            ChecksComplete = null,
            LinksChecked = null,
            LinksValid = null,
            ExternalLinksNotChecked = null,
            ImageLinks = null,
            RoutesChecked = null,
            FrameworkFiles = null,
            ExtensionsInstalled = null,
            LibrariesRegistered = null,
        };

    private static DoctorBoundaryKind ReadBoundary(DoctorDomainKind domain)
        => domain switch
        {
            DoctorDomainKind.WorkspaceEntry => DoctorBoundaryKind.Workspace,
            DoctorDomainKind.RecoveryResiduals => DoctorBoundaryKind.RecoveryStore,
            DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation => DoctorBoundaryKind.RouteUniverse,
            DoctorDomainKind.LocalReferences => DoctorBoundaryKind.LocalReferenceUniverse,
            DoctorDomainKind.FrameworkLifecycle => DoctorBoundaryKind.FrameworkLifecycle,
            DoctorDomainKind.ExtensionLifecycle => DoctorBoundaryKind.ExtensionLifecycle,
            _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, "The Doctor domain is not defined."),
        };

}
