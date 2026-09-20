using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairLibraryResidualFindingReaderTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair retains Library subjects as manual findings with their original cause")]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void LibrarySubjectRetainsCauseAndMarkdownSubjectPath()
    {
        var library = Finding(
            DoctorSubjectKind.LibraryProjection,
            ".agents/library-drift/library-note.md",
            "The registered Library projection is not current.");
        var framework = Finding(
            DoctorSubjectKind.GeneratedRegion,
            ".agents/guidance/_guidance.md",
            "Generated navigation is stale.");

        var findings = RepairLibraryResidualFindingReader.Read([library, framework]).ToArray();

        var finding = Assert.Single(findings);
        Assert.Equal(RepairFindingCode.ManualFindingRemaining, finding.Code);
        Assert.Equal(library.Message, finding.Cause);
        Assert.Equal(library.Subject.Path, finding.SourceCanonicalPath);
        Assert.Null(finding.Occurrence);
        Assert.Null(finding.Target);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Repair keeps non-Markdown Library observations manual without inventing a source occurrence")]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void NonMarkdownLibrarySubjectHasNoRepairLocation()
    {
        var finding = Assert.Single(RepairLibraryResidualFindingReader.Read([
            Finding(
                DoctorSubjectKind.Library,
                ".agents/open-forge.lock.json",
                "The Library registration could not be established."),
        ]));

        Assert.Equal(RepairFindingCode.ManualFindingRemaining, finding.Code);
        Assert.Equal("The Library registration could not be established.", finding.Cause);
        Assert.Null(finding.SourceCanonicalPath);
        Assert.Null(finding.Occurrence);
        Assert.Equal(".agents/open-forge.lock.json", finding.Observation?.Path);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Repair does not turn Library information into a residual warning")]
    [Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void LibraryInformationDoesNotBecomeAWarning()
    {
        var information = Finding(DoctorSubjectKind.Library, null, "Known Library state.") with
        {
            Severity = DoctorFindingSeverity.Information,
        };
        Assert.Empty(RepairLibraryResidualFindingReader.Read([information]));
    }

    private static DoctorFinding Finding(
        DoctorSubjectKind subjectKind,
        string? path,
        string message)
        => new()
        {
            Kind = subjectKind == DoctorSubjectKind.GeneratedRegion
                ? DoctorFindingKind.RouteGeneratedRegionStale
                : DoctorFindingKind.LibraryProjectionRetargeted,
            Severity = DoctorFindingSeverity.Warning,
            Message = message,
            Subject = new DoctorSubject
            {
                Kind = subjectKind,
                Path = path,
                Identifier = null,
                Location = null,
            },
            Evidence = [],
            Provenance = new DoctorProvenance
            {
                Domain = DoctorDomainKind.WorkspaceEntry,
                Source = DoctorProvenanceSource.WorkspaceEntry,
                Path = path,
                Location = null,
            },
            Resolution = DoctorResolutionLane.ManualDecision,
            Candidates = null,
            Proposal = null,
            Actions = [],
        };
}
