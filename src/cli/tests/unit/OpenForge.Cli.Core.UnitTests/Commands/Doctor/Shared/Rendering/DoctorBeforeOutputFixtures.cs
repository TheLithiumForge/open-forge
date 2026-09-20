using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Rendering;

internal static class DoctorBeforeOutputFixtures
{
    internal static DoctorResult Create(string situation)
        => situation switch
        {
            "healthy" => Diagnosis([]),
            "info-only" => Diagnosis([OwnershipMissing()]),
            "warnings-only" => Diagnosis([.. BrokenLinks(), .. InformationalFindings()]),
            "error-and-warnings" => Diagnosis([MalformedMetadata(), .. BrokenLinks(), .. InformationalFindings()]),
            "incomplete" => Diagnosis([UnavailableExtension()], DoctorCoverageState.Incomplete),
            "blocked-workspace" => DoctorResultBuilder.Event(null, CliSemanticStatus.Blocked, DoctorFindingKind.WorkspaceUnavailable, "The selected workspace is unavailable."),
            "invalid-input" => DoctorResultBuilder.Event(null, CliSemanticStatus.Invalid, null, "The selected workspace path is invalid."),
            "changed-extension-file" => Diagnosis([ChangedExtension()]),
            "stale-entries" => Diagnosis([StaleEntries()]),
            "library-drift" => Diagnosis([MissingLibraryLink()]),
            "recovery-bundle" => Diagnosis([RecoveryBundle()]),
            _ => throw new ArgumentOutOfRangeException(nameof(situation), situation, "Unknown Doctor output fixture."),
        };

    private static DoctorResult Diagnosis(DoctorFinding[] findings, DoctorCoverageState coverage = DoctorCoverageState.Complete)
    {
        var domains = findings.GroupBy(finding => finding.Provenance.Domain).Select(group => new DoctorDomainReport
        {
            Domain = group.Key,
            Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.Workspace, Path = null },
            Coverage = coverage,
            Lifecycle = null,
            SourceAvailability = null,
            Limitations = [],
            Counts = DoctorFindingAggregation.Count(group),
            Findings = group.ToArray(),
            Actions = DoctorFindingAggregation.Actions(group),
        }).ToArray();
        return new DoctorResult
        {
            Status = DoctorResultPolicy.ReadStatus(coverage, domains),
            Workspace = null,
            Next = null,
            Diagnosis = new DoctorDiagnosis
            {
                Coverage = coverage,
                Counts = DoctorFindingAggregation.Count(findings),
                CoverageCounts = CoverageCounts(coverage),
                Actions = DoctorFindingAggregation.Actions(findings),
                Domains = domains,
            },
        };
    }

    private static DoctorCoverageCounts CoverageCounts(DoctorCoverageState coverage)
        => new()
        {
            Checks = 6,
            ChecksComplete = coverage == DoctorCoverageState.Complete ? 6 : 5,
            LinksChecked = 21,
            LinksValid = 21,
            ExternalLinksNotChecked = 0,
            ImageLinks = 1,
            RoutesChecked = 20,
            FrameworkFiles = 12,
            ExtensionsInstalled = 3,
            LibrariesRegistered = 1,
        };

    private static DoctorFinding[] BrokenLinks()
    {
        var candidate = DoctorCandidateTestData.Finding(DoctorFindingKind.ReferenceTargetMissing);
        return [candidate, candidate with
        {
            Subject = candidate.Subject with { Path = ".agents/directives/second.md", Identifier = "missing.md" },
            Provenance = candidate.Provenance with { Path = ".agents/directives/second.md" },
            Candidates = null,
        }];
    }

    private static DoctorFinding[] InformationalFindings()
        =>
        [
            OwnershipMissing(),
            OwnershipMissing() with
            {
                Kind = DoctorFindingKind.RouteAxiomsInvalid,
                Subject = DoctorDomainSupport.Subject(DoctorSubjectKind.Route, ".agents/routes.md"),
                Provenance = DoctorDomainSupport.Provenance(
                    DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation,
                    DoctorProvenanceSource.RouteMetadata,
                    ".agents/routes.md"),
                Message = "The route Axioms section is invalid.",
            },
            OwnershipMissing() with
            {
                Kind = DoctorFindingKind.ReferenceTargetUnsupported,
                Subject = DoctorDomainSupport.Subject(DoctorSubjectKind.SourceOccurrence, ".agents/directives/review.md", "guide.png"),
                Provenance = DoctorDomainSupport.Provenance(
                    DoctorDomainKind.LocalReferences,
                    DoctorProvenanceSource.LocalReferences,
                    ".agents/directives/review.md"),
                Message = "The target kind is outside the supported local boundary.",
            },
        ];

    private static DoctorFinding OwnershipMissing()
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Information(DoctorFindingKind.FrameworkOwnershipObservation,
                "The ownership lock is absent; no Framework ownership claims are recorded."),
            DoctorDomainSupport.Subject(DoctorSubjectKind.ManagedFile, ".agents/open-forge.lock.json"),
            DoctorDomainSupport.Provenance(DoctorDomainKind.FrameworkLifecycle, DoctorProvenanceSource.FrameworkLifecycle, ".agents/open-forge.lock.json"),
            [new DoctorStateEvidence(DoctorObservedState.Unavailable)]);

    private static DoctorFinding MalformedMetadata()
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(DoctorFindingKind.WorkspaceFrontmatterMalformed, "The frontmatter block is not closed.", DoctorResolutionLane.ManualDecision),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, ".agents/directives/broken.md"),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry, ".agents/directives/broken.md"),
            [new DoctorStateEvidence(DoctorObservedState.Blocked)]);

    private static DoctorFinding UnavailableExtension()
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Information(DoctorFindingKind.ExtensionSourceUnavailable, "The Extension source could not be read."),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Extension, "./packages/toolkit", "toolkit"),
            DoctorDomainSupport.Provenance(DoctorDomainKind.ExtensionLifecycle, DoctorProvenanceSource.ExtensionSource, "./packages/toolkit"),
            [new DoctorStateEvidence(DoctorObservedState.Unavailable)]);

    private static DoctorFinding ChangedExtension()
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(DoctorFindingKind.ExtensionManagedChanged,
                "An Extension-managed target differs from its current intended payload.", DoctorResolutionLane.TargetedOperation),
            DoctorDomainSupport.Subject(DoctorSubjectKind.ManagedFile, ".agents/toolkit/example.md", "toolkit"),
            DoctorDomainSupport.Provenance(DoctorDomainKind.ExtensionLifecycle, DoctorProvenanceSource.ExtensionLifecycle, ".agents/toolkit/example.md"),
            [new DoctorComparisonEvidence("current-intended-fingerprint", "current-fingerprint")]);

    private static DoctorFinding StaleEntries()
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(DoctorFindingKind.RouteGeneratedRegionStale, "The generated Entries region is stale.", DoctorResolutionLane.TargetedOperation),
            DoctorDomainSupport.Subject(DoctorSubjectKind.GeneratedRegion, ".agents/directives/_directives.md"),
            DoctorDomainSupport.Provenance(DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation, DoctorProvenanceSource.GeneratedNavigation, ".agents/directives/_directives.md"),
            [new DoctorStateEvidence(DoctorObservedState.Changed)]);

    private static DoctorFinding MissingLibraryLink()
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(DoctorFindingKind.LibraryProjectionMissing, "A registered Library link is missing.", DoctorResolutionLane.ManualDecision),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, ".agents/directives/library.md", "team-knowledge"),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry, ".agents/directives/library.md"),
            [new DoctorStateEvidence(DoctorObservedState.Missing)]);

    private static DoctorFinding RecoveryBundle()
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Information(DoctorFindingKind.RecoveryBundleRecognized, "A recognized recovery bundle remains."),
            DoctorDomainSupport.Subject(DoctorSubjectKind.RecoveryItem, "recovery/operation.ofr"),
            DoctorDomainSupport.Provenance(DoctorDomainKind.RecoveryResiduals, DoctorProvenanceSource.RecoveryResiduals, "recovery/operation.ofr"),
            [new DoctorStateEvidence(DoctorObservedState.Present)]);
}
