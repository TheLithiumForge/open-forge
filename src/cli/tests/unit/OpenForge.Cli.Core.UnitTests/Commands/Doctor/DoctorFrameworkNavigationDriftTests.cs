using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorFrameworkNavigationDriftTests
{
    private const string CurrentPath = ".agents/current.md";
    private const string GeneratedPath = ".agents/_entries.md";
    private const string ChangedPath = ".agents/changed.md";

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Readable generated navigation drift keeps target evidence without establishing partial Framework lifecycle")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ReadableGeneratedNavigationDriftKeepsTargetEvidenceWithoutPartialLifecycle()
    {
        var targets = new[] { CurrentTarget(), ChangedGeneratedTarget() };

        var withoutCurrentRoute = Inspect(
            targets,
            FrameworkManagedSetState.Mixed,
            currentNavigationPaths: new HashSet<string>(StringComparer.Ordinal));
        AssertGeneratedNavigationEvidence(withoutCurrentRoute);

        var withCurrentRoute = Inspect(
            targets,
            FrameworkManagedSetState.Mixed,
            currentNavigationPaths: new HashSet<string>([GeneratedPath], StringComparer.Ordinal));
        AssertNormalizedGeneratedNavigation(withCurrentRoute);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A changed ordinary Framework target with a current sibling retains partial lifecycle")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ChangedOrdinaryTargetWithCurrentSiblingRetainsPartialLifecycle()
    {
        var report = Inspect(
            [CurrentTarget(), ChangedOrdinaryTarget()],
            FrameworkManagedSetState.Mixed);

        Assert.Contains(report.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkPartialLifecycle);
        Assert.Contains(report.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkManagedChanged
            && finding.Provenance.Path == ChangedPath);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A missing generated target retains partial lifecycle and missing-target evidence")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void MissingGeneratedTargetRetainsExistingSafetyEvidence()
    {
        var report = Inspect(
            [CurrentTarget(), MissingGeneratedTarget()],
            FrameworkManagedSetState.Mixed);

        Assert.Contains(report.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkPartialLifecycle);
        Assert.Contains(report.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkManagedMissing
            && finding.Provenance.Path == GeneratedPath);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "An unavailable generated target retains incomplete coverage")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void UnavailableGeneratedTargetRetainsCoverage()
    {
        var report = Inspect(
            [CurrentTarget(), UnavailableGeneratedTarget()],
            FrameworkManagedSetState.Unavailable);

        Assert.DoesNotContain(report.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkPartialLifecycle);
        Assert.Equal(DoctorCoverageState.Incomplete, report.Coverage);
        Assert.NotEmpty(report.Limitations);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Unavailable Framework source does not suppress changed-target evidence")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void UnavailableSourceRetainsChangedTargetEvidence()
    {
        var report = Inspect(
            [CurrentTarget(), ChangedGeneratedTarget()],
            FrameworkManagedSetState.Unavailable,
            OperationalSourceAvailability.Unavailable,
            new HashSet<string>([GeneratedPath], StringComparer.Ordinal));

        Assert.DoesNotContain(report.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkPartialLifecycle);
        Assert.Contains(report.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkManagedChanged
            && finding.Provenance.Path == GeneratedPath);
        Assert.Equal(OperationalSourceAvailability.Unavailable, report.SourceAvailability);
    }

    private static void AssertGeneratedNavigationEvidence(DoctorDomainReport report)
    {
        Assert.DoesNotContain(report.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkPartialLifecycle);
        var finding = Assert.Single(report.Findings, candidate =>
            candidate.Kind == DoctorFindingKind.FrameworkManagedChanged
            && candidate.Provenance.Path == GeneratedPath);
        Assert.Contains(finding.Evidence, evidence => evidence is DoctorComparisonEvidence);
        Assert.Contains(report.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkRootRegionBoundary
            && finding.Provenance.Path == GeneratedPath);
    }

    private static void AssertNormalizedGeneratedNavigation(DoctorDomainReport report)
    {
        Assert.DoesNotContain(report.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkPartialLifecycle);
        Assert.DoesNotContain(report.Findings, finding =>
            (finding.Kind is DoctorFindingKind.FrameworkManagedChanged
                or DoctorFindingKind.FrameworkRootRegionBoundary)
            && finding.Provenance.Path == GeneratedPath);
    }

    private static DoctorDomainReport Inspect(
        IReadOnlyList<FrameworkManagedTargetDoctorObservation> targets,
        FrameworkManagedSetState managedSet,
        OperationalSourceAvailability sourceAvailability = OperationalSourceAvailability.Available,
        IReadOnlySet<string>? currentNavigationPaths = null)
    {
        var view = FrameworkLifecycleDoctorView.Create(
            FrameworkLifecycleDoctorAssessment.Create(
                OperationalViewState.Complete,
                sourceAvailability == OperationalSourceAvailability.Available
                    ? OperationalLifecycleState.Trusted
                    : OperationalLifecycleState.Incomplete,
                sourceAvailability,
                managedSet),
            DoctorOperationTestSupport.CreateCompleteOwnership(),
            AvailablePayload(),
            targets);
        return FrameworkLifecycleDoctorInspector.Inspect(
            view,
            new RecoveryResidualDoctorView(OperationalViewState.Complete, [], Cause: null),
            DoctorOperationTestSupport.CreateCompleteOwnership(),
            absenceProven: false,
            currentNavigationPaths ?? new HashSet<string>(StringComparer.Ordinal));
    }

    private static FrameworkManagedTargetDoctorObservation CurrentTarget()
        => FrameworkManagedTargetDoctorObservation.Observed(
            new FrameworkManagedTargetObservation
            {
                Path = CurrentPath,
                Kind = FrameworkManagedTargetKind.File,
                SourceAssetPath = CurrentPath,
                Region = null,
                IntendedFingerprint = "current-intended",
                Source = ValidSource(),
                State = OperationalTargetState.Current,
            },
            boundary: null,
            fingerprint: "current-actual");

    private static FrameworkManagedTargetDoctorObservation ChangedGeneratedTarget()
        => FrameworkManagedTargetDoctorObservation.Observed(
            new FrameworkManagedTargetObservation
            {
                Path = GeneratedPath,
                Kind = FrameworkManagedTargetKind.GeneratedRegion,
                SourceAssetPath = null,
                Region = "entries",
                IntendedFingerprint = "generated-intended",
                Source = ValidSource(),
                State = OperationalTargetState.Changed,
            },
            boundary: FrameworkManagedTargetBoundaryKind.RootRegion,
            fingerprint: "generated-actual");

    private static FrameworkManagedTargetDoctorObservation ChangedOrdinaryTarget()
        => FrameworkManagedTargetDoctorObservation.Observed(
            new FrameworkManagedTargetObservation
            {
                Path = ChangedPath,
                Kind = FrameworkManagedTargetKind.File,
                SourceAssetPath = ChangedPath,
                Region = null,
                IntendedFingerprint = "changed-intended",
                Source = ValidSource(),
                State = OperationalTargetState.Changed,
            },
            boundary: null,
            fingerprint: "changed-actual");

    private static FrameworkManagedTargetDoctorObservation MissingGeneratedTarget()
        => FrameworkManagedTargetDoctorObservation.AtBoundary(
            new FrameworkManagedTargetObservation
            {
                Path = GeneratedPath,
                Kind = FrameworkManagedTargetKind.GeneratedRegion,
                SourceAssetPath = null,
                Region = "entries",
                IntendedFingerprint = "generated-intended",
                Source = ValidSource(),
                State = OperationalTargetState.Missing,
            },
            boundary: null,
            ManagedTargetReadState.Missing,
            cause: null);

    private static FrameworkManagedTargetDoctorObservation UnavailableGeneratedTarget()
        => FrameworkManagedTargetDoctorObservation.AtBoundary(
            new FrameworkManagedTargetObservation
            {
                Path = GeneratedPath,
                Kind = FrameworkManagedTargetKind.GeneratedRegion,
                SourceAssetPath = null,
                Region = "entries",
                IntendedFingerprint = "generated-intended",
                Source = ValidSource(),
                State = OperationalTargetState.Unavailable,
            },
            boundary: null,
            ManagedTargetReadState.Unavailable,
            cause: "generated target unavailable");

    private static FrameworkTargetSourceValidation ValidSource()
        => new(FrameworkTargetSourceState.Valid, cause: null);

    private static FrameworkPayloadReadResult AvailablePayload()
        => FrameworkPayloadReadResult.Available(
            FrameworkPayload.Create(
            [
                FrameworkPayloadAsset.Create("AGENTS.md", "agents"u8),
                FrameworkPayloadAsset.Create("CLAUDE.md", "claude"u8),
                FrameworkPayloadAsset.Create(".agents/loader.md", "loader"u8),
            ]));
}
