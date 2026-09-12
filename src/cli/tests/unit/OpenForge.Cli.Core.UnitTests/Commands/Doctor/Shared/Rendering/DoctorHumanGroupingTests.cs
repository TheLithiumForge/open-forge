using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Rendering;

public sealed class DoctorHumanGroupingTests
{
    [Theory(DisplayName = "Doctor views show shared candidates once and retain both findings"), InlineData(false), InlineData(true), Trait("Feature", "doctor-presentation"), Trait("Evidence", "Unit")]
    public void SharedCandidateDetailsRenderOnceWithoutLosingFindings(bool expanded)
    {
        var first = Finding(DoctorFindingKind.ReferenceTargetMissing);
        var second = Finding(DoctorFindingKind.ReferenceCandidatesOne);
        var findings = new[] { first, second };
        var view = expanded ? CliView.Expanded : CliView.Compact;

        var text = Render(findings, view);

        Assert.Equal(expanded ? DoctorHumanSnapshots.Expanded : DoctorHumanSnapshots.Compact, text);
        Assert.Equal(2, findings.Length);
        Assert.Same(first, findings[0]);
        Assert.Same(second, findings[1]);
        Assert.NotSame(first.Candidates, second.Candidates);
        Assert.Single(first.Candidates!.Items);
        Assert.Single(second.Candidates!.Items);
    }

    [Fact(DisplayName = "Distinct link occurrences retain separate candidate groups"), Trait("Feature", "doctor-presentation"), Trait("Evidence", "Unit")]
    public void DistinctLocationsDoNotMerge()
    {
        var first = Finding(DoctorFindingKind.ReferenceTargetMissing);
        var second = first with { Subject = first.Subject with { Location = new SourceLocation(12, 4, 250, 40) } };

        var text = Render([first, second], CliView.Compact);

        Assert.Equal(2, text.Split("Possible targets:", StringSplitOptions.None).Length - 1);
        Assert.Equal(2, text.Split("[reference.target-missing]", StringSplitOptions.None).Length - 1);
    }

    [Fact(DisplayName = "Candidate grouping preserves different evidence and ordering"), Trait("Feature", "doctor-presentation"), Trait("Evidence", "Unit")]
    public void CandidateEqualityRetainsDistinctFacts()
    {
        var first = Finding(DoctorFindingKind.ReferenceTargetMissing).Candidates!;
        var clone = Finding(DoctorFindingKind.ReferenceTargetMissing).Candidates!;
        var candidate = Assert.Single(first.Items);
        var changed = first with { Items = [candidate with { Provenance = candidate.Provenance with { Source = DoctorProvenanceSource.RouteInventory } }] };
        var other = candidate with { Subject = candidate.Subject with { Path = ".agents/guidance/another.md" } };
        var ordered = first with { Cardinality = DoctorCandidateCardinality.Several, Items = [candidate, other] };
        var reversed = ordered with { Items = [other, candidate] };

        Assert.Equal(3, new[] { first, clone, changed, ordered }.Distinct(DoctorCandidateSetComparer.Instance).Count());
        Assert.False(DoctorCandidateSetComparer.Instance.Equals(ordered, reversed));
        Assert.False(DoctorCandidateSetComparer.Instance.Equals(first, first with { Cardinality = DoctorCandidateCardinality.None }));
    }

    [Fact(DisplayName = "Shared actions appear once while distinct reasons remain available"), Trait("Feature", "doctor-presentation"), Trait("Evidence", "Unit")]
    public void ActionsRetainDistinctReasons()
    {
        var action = new DoctorNextAction
        {
            Kind = DoctorNextActionKind.ReviewCandidates,
            Operation = null,
            Command = null,
            Reason = "Check the linked heading before choosing a target.",
        };
        var other = action with { Reason = "Confirm the intended source file." };
        var first = Finding(DoctorFindingKind.ReferenceTargetMissing) with { Actions = [action] };
        var second = Finding(DoctorFindingKind.ReferenceCandidatesOne) with { Actions = [action, other] };

        var text = Render([first, second], CliView.Expanded);

        Assert.Equal(2, text.Split("Next:", StringSplitOptions.None).Length - 1);
        Assert.Contains(action.Reason, text, StringComparison.Ordinal);
        Assert.Contains(other.Reason, text, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Doctor keeps additional actions and omits only duplicate summaries"), InlineData(false), InlineData(true), Trait("Feature", "doctor-presentation"), Trait("Evidence", "Unit")]
    public void ActionScopesPreserveAdditionalActionsAndOriginalJson(bool compact)
    {
        var shared = new DoctorNextAction { Kind = DoctorNextActionKind.ReviewCandidates, Operation = null, Command = "review shared", Reason = "Shared review." };
        var categoryOnly = shared with { Command = "review category" };
        var overallOnly = shared with { Command = "review overall" };
        var finding = Finding(DoctorFindingKind.ReferenceTargetMissing) with { Actions = [shared] };
        var counts = DoctorFindingAggregation.Count([finding]);
        var domain = new DoctorDomainReport
        {
            Domain = DoctorDomainKind.LocalReferences,
            Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.Workspace, Path = "/work/demo" },
            Coverage = DoctorCoverageState.Complete,
            Lifecycle = null,
            SourceAvailability = null,
            Limitations = [],
            Counts = counts,
            Findings = [finding],
            Actions = [shared, categoryOnly],
        };
        var result = new DoctorResult
        {
            Status = CliSemanticStatus.Attention,
            Workspace = null,
            Next = null,
            Diagnosis = new DoctorDiagnosis
            {
                Coverage = DoctorCoverageState.Complete,
                Counts = counts,
                Actions = [shared, categoryOnly, overallOnly],
                Domains = [domain],
            },
        };
        var jsonRequest = new CliPresentationRequest<DoctorResult>(result, new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal));
        var before = DoctorJsonRenderer.Render(jsonRequest);
        var humanRequest = new CliPresentationRequest<DoctorResult>(result, new CliPresentation(CliOutputFormat.Human, compact ? CliView.Compact : CliView.Expanded, CliVerbosity.Normal));

        var text = DoctorHumanRenderer.Render(humanRequest);

        foreach (var command in new[] { "review shared", "review category", "review overall" })
        {
            Assert.Equal(1, text.Split(command, StringSplitOptions.None).Length - 1);
        }

        Assert.Equal(before, DoctorJsonRenderer.Render(jsonRequest));
        Assert.Equal(1, result.Diagnosis.Counts.Severity.Warning.Value);
    }

    [Fact(DisplayName = "Human paths are complete and terminal control characters remain escaped"), Trait("Feature", "doctor-presentation"), Trait("Evidence", "Unit")]
    public void LongPathsAreNotTruncated()
    {
        var path = ".agents/" + new string('a', 600) + "/document.md";
        var finding = Finding(DoctorFindingKind.ReferenceTargetMissing);
        var text = Render([finding with { Subject = finding.Subject with { Path = path } }], CliView.Compact);

        Assert.Contains(path + ":12:4", text, StringComparison.Ordinal);
        Assert.Equal("before\uFFFDafter", DoctorHumanRenderer.Text("before\u001bafter"));
    }

    [Fact(DisplayName = "Every Doctor finding has a deliberate human title"), Trait("Feature", "doctor-presentation"), Trait("Evidence", "Unit")]
    public void FindingTitlesCoverTheCatalogue()
    {
        foreach (var kind in Enum.GetValues<DoctorFindingKind>())
        {
            Assert.False(string.IsNullOrWhiteSpace(DoctorFindingTitles.Read(kind)));
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => DoctorFindingTitles.Read((DoctorFindingKind)int.MaxValue));
    }

    private static string Render(IReadOnlyList<DoctorFinding> findings, CliView view)
    {
        var builder = new StringBuilder();
        DoctorFindingHumanRenderer.Append(builder, findings, view);
        return builder.ToString().Trim().ReplaceLineEndings("\n");
    }

    private static DoctorFinding Finding(DoctorFindingKind kind)
    {
        var location = new SourceLocation(12, 4, 200, 40);
        var provenance = new DoctorProvenance
        {
            Domain = DoctorDomainKind.LocalReferences,
            Source = DoctorProvenanceSource.LocalReferences,
            Path = ".agents/directives/review.md",
            Location = location,
        };
        return new DoctorFinding
        {
            Kind = kind,
            Severity = DoctorFindingSeverity.Warning,
            Message = "The linked file was not found.",
            Subject = new DoctorSubject
            {
                Kind = DoctorSubjectKind.SourceOccurrence,
                Path = provenance.Path,
                Identifier = "../guidance/testing.md",
                Location = location,
            },
            Evidence = [new DoctorStateEvidence(DoctorObservedState.Missing)],
            Provenance = provenance,
            Resolution = DoctorResolutionLane.GuidedChoice,
            Candidates = new DoctorCandidateSet
            {
                Cardinality = DoctorCandidateCardinality.One,
                Items = [new DoctorCandidate
                {
                    Subject = new DoctorSubject
                    {
                        Kind = DoctorSubjectKind.Target,
                        Path = ".agents/guidance/testing.md",
                        Identifier = null,
                        Location = null,
                    },
                    Evidence = [new DoctorCandidateBasis { Kind = DoctorCandidateBasisKind.Filename, Value = "testing.md", Location = null }],
                    Provenance = provenance with { Path = ".agents/guidance/testing.md", Location = null },
                }],
            },
            Proposal = null,
            Actions = [],
        };
    }
}
