using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Presentation.Doctor;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using System.Text.Json;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Rendering;

[Trait("Feature", "doctor-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
public sealed class DoctorMaintenanceNextTests
{
    [Theory(DisplayName = "Every incomplete Doctor headline names unfinished work without claiming no problems")]
    [InlineData("healthy", 1, "Doctor could not finish 1 check.")]
    [InlineData("healthy", 2, "Doctor could not finish 2 checks.")]
    [InlineData("healthy", 0, "Doctor could not finish.")]
    [InlineData("info-only", 1, "Doctor could not finish 1 check.")]
    [InlineData("warnings-only", 1, "Doctor could not finish 1 check.")]
    [InlineData("error-and-warnings", 1, "Doctor could not finish 1 check.")]
    public void IncompleteHeadline(string situation, int unfinished, string expected)
    {
        var original = DoctorBeforeOutputFixtures.Create(situation);
        var coverage = Assert.IsType<DoctorCoverageCounts>(original.Diagnosis.CoverageCounts);
        var result = original with
        {
            Status = CliSemanticStatus.Incomplete,
            Diagnosis = original.Diagnosis with
            {
                Coverage = DoctorCoverageState.Incomplete,
                CoverageCounts = unfinished == 0 ? null : coverage with { Checks = 6, ChecksComplete = 6 - unfinished },
            },
        };
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var text = CliRenderingStage.Render(new CliPresentationRequest<DoctorResult>(result, new(CliFormat.Text, detail, null)), DoctorPresentation.Rendering);
            Assert.StartsWith(expected, text.PrimaryContent, StringComparison.Ordinal);
            Assert.DoesNotContain("No problems found", text.PrimaryContent, StringComparison.Ordinal);
            Assert.Equal(CliSemanticStatus.Incomplete, DoctorReportSelector.Select(result, new(detail)).Status);
        }
    }

    [Fact(DisplayName = "Doctor resolves a lost fragment before another repair or Index can discard its metadata identity")]
    public void PreserveFragmentIdentity()
    {
        var guided = Finding(DoctorFindingKind.ReferenceTargetMissing, DoctorResolutionLane.GuidedChoice, "open-forge repair", DoctorNextOperation.Repair) with
        {
            Maintenance = new() { Condition = DoctorMaintenanceCondition.RenamedFragmentMissing, PreserveRenameIdentity = true },
        };
        var safe = Finding(DoctorFindingKind.ReferenceTargetRenamed, DoctorResolutionLane.SafeExact, "open-forge repair --automatic", DoctorNextOperation.Repair);
        var index = Finding(DoctorFindingKind.RouteGeneratedEntryExtra, DoctorResolutionLane.TargetedOperation, "open-forge index", DoctorNextOperation.Index);
        Assert.Equal("open-forge repair", DoctorReportSelector.Select(Result([index, safe, guided]), new(CliDetail.Standard)).Next?.Command);
    }

    [Theory(DisplayName = "Doctor renders a sole manual action only as Next and keeps row actions for multiple findings")]
    [InlineData(1, (int)CliDetail.Minimal)]
    [InlineData(2, (int)CliDetail.Minimal)]
    [InlineData(1, (int)CliDetail.Standard)]
    [InlineData(2, (int)CliDetail.Standard)]
    [InlineData(1, (int)CliDetail.Full)]
    [InlineData(2, (int)CliDetail.Full)]
    [InlineData(1, (int)CliDetail.Debug)]
    [InlineData(2, (int)CliDetail.Debug)]
    public void ManualActionLayout(int count, int detailValue)
    {
        const string action = "Update or remove this link in .agents/guidance/source.md at line 4, column 3.";
        var first = Finding(DoctorFindingKind.ReferenceTargetMissing, DoctorResolutionLane.ManualDecision, action);
        var second = first with { Subject = DoctorDomainSupport.Subject(DoctorSubjectKind.Path, ".agents/guidance/z.md") };
        var result = Result(new[] { first, second }.Take(count).ToArray());
        var detail = (CliDetail)detailValue;
        var text = CliRenderingStage.Render(new CliPresentationRequest<DoctorResult>(result, new(CliFormat.Text, detail, null)), DoctorPresentation.Rendering);
        Assert.Equal(count == 1 ? 1 : 3, text.PrimaryContent.Split(action, StringSplitOptions.None).Length - 1);
        Assert.Contains($"Next: {action}", text.PrimaryContent, StringComparison.Ordinal);
        var json = CliRenderingStage.Render(new CliPresentationRequest<DoctorResult>(result, new(CliFormat.Json, detail, null)), DoctorPresentation.Rendering);
        using var document = JsonDocument.Parse(json.PrimaryContent);
        Assert.All(document.RootElement.GetProperty("findings").EnumerateArray(), finding => Assert.Single(finding.GetProperty("actions").EnumerateArray()));
    }

    [Theory(DisplayName = "Doctor chooses one continuation in safety safe Index guided and manual order")]
    [InlineData(0, null)]
    [InlineData(1, "Edit the authored link in .agents/guidance/source.md at line 4.")]
    [InlineData(2, "open-forge repair")]
    [InlineData(3, "open-forge index")]
    [InlineData(4, "open-forge repair --automatic")]
    [InlineData(5, "Review .agents/guidance/unsafe.md before continuing.")]
    public void DeterministicContinuation(int count, string? expected)
    {
        var findings = new[]
        {
            Finding(DoctorFindingKind.ReferenceTargetMissing, DoctorResolutionLane.ManualDecision, "Edit the authored link in .agents/guidance/source.md at line 4."),
            Finding(DoctorFindingKind.ReferenceTargetMissing, DoctorResolutionLane.GuidedChoice, "open-forge repair", DoctorNextOperation.Repair),
            Finding(DoctorFindingKind.RouteMetadataRequiredMissing, DoctorResolutionLane.TargetedOperation, "open-forge index", DoctorNextOperation.Index),
            Finding(DoctorFindingKind.ReferenceSameTargetPath, DoctorResolutionLane.SafeExact, "open-forge repair --automatic", DoctorNextOperation.Repair),
            Finding(DoctorFindingKind.RouteEscape, DoctorResolutionLane.BlockedRepair, "Review .agents/guidance/unsafe.md before continuing."),
        }.Take(count).Reverse().ToArray();
        var report = DoctorReportSelector.Select(Result(findings), new(CliDetail.Minimal));
        Assert.Equal(expected, report.Next?.Command);
    }

    [Fact(DisplayName = "Informational Doctor findings do not select a continuation")]
    public void InformationalResult()
    {
        var finding = Finding(DoctorFindingKind.RecoveryBundleRecognized, DoctorResolutionLane.SafeExact, "open-forge cleanup", DoctorNextOperation.Repair) with
        {
            Severity = DoctorFindingSeverity.Information,
        };
        var report = DoctorReportSelector.Select(Result([finding]) with { Status = CliSemanticStatus.Complete }, new(CliDetail.Full));
        Assert.Null(report.Next);
    }

    private static DoctorFinding Finding(DoctorFindingKind kind, DoctorResolutionLane lane, string command, DoctorNextOperation? operation = null)
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(kind, "Observed fixture", lane, new()
            {
                Kind = operation is null ? DoctorNextActionKind.ManualDecision : DoctorNextActionKind.AcceptedOperation,
                Operation = operation,
                Command = operation is null ? null : command,
                Reason = command,
            }),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, ".agents/guidance/source.md"),
            DoctorDomainSupport.Provenance(DoctorDomainKind.LocalReferences, DoctorProvenanceSource.LocalReferences),
            [new DoctorStateEvidence(DoctorObservedState.Missing)]);

    private static DoctorResult Result(IReadOnlyList<DoctorFinding> findings)
    {
        var counts = DoctorFindingAggregation.Count(findings);
        return new()
        {
            Workspace = null,
            Next = null,
            Status = findings.Count == 0 ? CliSemanticStatus.Complete : CliSemanticStatus.Attention,
            Diagnosis = new()
            {
                Coverage = DoctorCoverageState.Complete,
                CoverageCounts = null,
                Counts = counts,
                Actions = [],
                Domains = [new()
                {
                    Domain = DoctorDomainKind.LocalReferences,
                    Boundary = new() { Kind = DoctorBoundaryKind.LocalReferenceUniverse, Path = null },
                    Coverage = DoctorCoverageState.Complete,
                    Lifecycle = null,
                    SourceAvailability = null,
                    Limitations = [],
                    Counts = counts,
                    Findings = findings,
                    Actions = [],
                }],
            },
        };
    }
}
