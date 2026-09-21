using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Actions;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Rendering;

public sealed class DoctorTargetedIndexAdviceTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Targeted Index advice renders one catalogue command without the generic fallback")]
    [InlineData((int)DoctorFindingKind.RouteGeneratedRegionStale)]
    [InlineData((int)DoctorFindingKind.RouteGeneratedEntryMissing)]
    [InlineData((int)DoctorFindingKind.RouteOverwriteIndependentIndex)]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void SupportedTargetedFindingsRenderOneExplicitIndexCommand(int kindValue)
    {
        const string cataloguePath = ".agents/catalogue/_catalogue.md";
        var finding = TargetedFinding((DoctorFindingKind)kindValue, cataloguePath);

        var action = Assert.Single(DoctorWording.Actions(finding));

        Assert.Equal(CliNextActionKind.Command, action.Kind);
        Assert.Equal($"open-forge index {cataloguePath}", action.Command);
        Assert.Equal("Refresh the generated Entries in this catalogue.", action.Reason);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Unsupported targeted Index advice is one exact manual sentence")]
    [InlineData(".agents/owner's/_references.md")]
    [InlineData(".agents/owner\"/_references.md")]
    [InlineData(".agents/$team/_references.md")]
    [InlineData(".agents/`team`/_references.md")]
    [InlineData(".agents/100%/_references.md")]
    [InlineData(".agents/team\u0001/_references.md")]
    [InlineData("-catalogue.md")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void UnsupportedTargetedFindingRendersExactPathWithoutGenericFallback(string cataloguePath)
    {
        var finding = TargetedFinding(DoctorFindingKind.RouteGeneratedRegionMissing, cataloguePath);
        var expected = $"Run open-forge index with {cataloguePath} as its source argument, using your shell's quoting rules.";

        var action = Assert.Single(DoctorWording.Actions(finding));

        Assert.Equal(CliNextActionKind.Sentence, action.Kind);
        Assert.Equal(expected, action.Command);
        Assert.Equal(expected, action.Reason);
        Assert.NotEqual("open-forge index", action.Command);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Doctor Next prefers the explicit catalogue command and preserves the targeted resolution")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ReportNextUsesExplicitCommandAndTargetedResolution()
    {
        const string cataloguePath = ".agents/catalogue/_catalogue.md";
        var result = Result(TargetedFinding(DoctorFindingKind.RouteGeneratedRegionStale, cataloguePath));

        var report = DoctorReportSelector.Select(result, new(CliDetail.Minimal));
        var finding = Assert.Single(report.Findings);

        Assert.Equal(CliResolution.TargetedOperation, finding.Resolution);
        Assert.Equal("open-forge index .agents/catalogue/_catalogue.md", report.Next?.Command);
        Assert.Equal(CliNextActionKind.Command, report.Next?.Kind);
        Assert.Equal("Refresh the generated Entries in this catalogue.", report.Next?.Reason);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Doctor Next uses the explicit manual Index sentence when a command is unsupported")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ReportNextUsesExplicitManualInstruction()
    {
        const string cataloguePath = ".agents/catalogue/owner\"s.md";
        var result = Result(TargetedFinding(DoctorFindingKind.RouteOverwriteIndependentIndex, cataloguePath));
        var expected = $"Run open-forge index with {cataloguePath} as its source argument, using your shell's quoting rules.";

        var report = DoctorReportSelector.Select(result, new(CliDetail.Minimal));

        Assert.Equal(expected, report.Next?.Command);
        Assert.Equal(CliNextActionKind.Sentence, report.Next?.Kind);
        Assert.NotEqual("open-forge index", report.Next?.Command);
        Assert.Equal(CliResolution.TargetedOperation, Assert.Single(report.Findings).Resolution);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Unrelated route findings retain their generic Index fallback")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void UnrelatedRouteFindingKeepsExistingFallback()
    {
        var finding = DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(
                DoctorFindingKind.RouteUnreachable,
                "A source is not reachable through established route facts.",
                DoctorResolutionLane.ManualDecision),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Route, ".agents/detached.md"),
            DoctorDomainSupport.Provenance(
                DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation,
                DoctorProvenanceSource.RouteInventory,
                ".agents/detached.md"),
            [new DoctorStateEvidence(DoctorObservedState.Unavailable)]);

        var action = Assert.Single(DoctorWording.Actions(finding));

        Assert.Equal("open-forge index", action.Command);
        Assert.Equal(CliNextActionKind.Command, action.Kind);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Doctor Next prefers a copyable Index command over another catalogue's manual instruction")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ReportNextPrefersCommandAcrossCataloguesInTheSameDomain()
    {
        var result = Result(
            TargetedFinding(DoctorFindingKind.RouteGeneratedRegionStale, ".agents/a's/_catalogue.md"),
            TargetedFinding(DoctorFindingKind.RouteGeneratedRegionStale, ".agents/z/_catalogue.md"));

        var report = DoctorReportSelector.Select(result, new(CliDetail.Minimal));

        Assert.Equal("open-forge index .agents/z/_catalogue.md", report.Next?.Command);
        Assert.Equal(CliNextActionKind.Command, report.Next?.Kind);
        Assert.Equal(2, report.Findings.Count);
    }

    private static DoctorFinding TargetedFinding(DoctorFindingKind kind, string cataloguePath)
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(
                kind,
                "Generated navigation differs from current route facts.",
                DoctorResolutionLane.TargetedOperation,
                DoctorIndexAction.ForPath(cataloguePath)),
            DoctorDomainSupport.Subject(
                kind == DoctorFindingKind.RouteOverwriteIndependentIndex
                    ? DoctorSubjectKind.Route
                    : DoctorSubjectKind.GeneratedRegion,
                cataloguePath),
            DoctorDomainSupport.Provenance(
                DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation,
                DoctorProvenanceSource.GeneratedNavigation,
                cataloguePath),
            [new DoctorStateEvidence(DoctorObservedState.Changed)]);

    private static DoctorResult Result(params DoctorFinding[] findings)
    {
        var counts = DoctorFindingAggregation.Count(findings);
        var domain = new DoctorDomainReport
        {
            Domain = DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation,
            Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.RouteUniverse, Path = null },
            Coverage = DoctorCoverageState.Complete,
            Lifecycle = null,
            SourceAvailability = null,
            Limitations = [],
            Counts = counts,
            Findings = findings,
            Actions = DoctorFindingAggregation.Actions(findings),
        };
        return new DoctorResult
        {
            Status = CliSemanticStatus.Attention,
            Workspace = null,
            Next = null,
            Diagnosis = new DoctorDiagnosis
            {
                Coverage = DoctorCoverageState.Complete,
                Counts = counts,
                CoverageCounts = null,
                Actions = DoctorFindingAggregation.Actions(findings),
                Domains = [domain],
            },
        };
    }
}
