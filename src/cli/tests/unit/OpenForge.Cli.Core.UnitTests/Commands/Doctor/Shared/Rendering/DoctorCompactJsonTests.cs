using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Rendering;

public sealed class DoctorCompactJsonTests
{
    [Fact(DisplayName = "Doctor compact JSON shares exact ordered candidate sets and preserves every diagnostic")]
    [Trait("Feature", "compact-json"), Trait("Evidence", "Unit")]
    public void CandidateReferencesPreserveExactFactsAndNullEmptyDistinction()
    {
        var first = DoctorCandidateTestData.Finding(DoctorFindingKind.ReferenceTargetMissing);
        var clone = DoctorCandidateTestData.Finding(DoctorFindingKind.ReferenceCandidatesOne);
        var candidates = Assert.IsType<DoctorCandidateSet>(first.Candidates);
        var candidate = Assert.Single(candidates.Items);
        var other = candidate with { Subject = candidate.Subject with { Path = ".agents/other.md" } };
        var ordered = candidates with { Cardinality = DoctorCandidateCardinality.Several, Items = [candidate, other] };
        DoctorFinding[] findings =
        [
            first,
            clone,
            first with { Candidates = null },
            first with { Candidates = candidates with { Cardinality = DoctorCandidateCardinality.None, Items = [] } },
            first with { Candidates = ordered },
            first with { Candidates = ordered with { Items = [other, candidate] } },
            first with { Candidates = candidates with { Items = [candidate with { Evidence = [candidate.Evidence[0] with { Value = "different" }] }] } },
            first with { Candidates = candidates with { Items = [candidate with { Provenance = candidate.Provenance with { Source = DoctorProvenanceSource.RouteInventory } }] } },
        ];
        var result = Result(findings);
        var compact = Render(result, CliView.Compact);
        var expanded = Render(result, CliView.Expanded);
        Assert.Equal(compact, Render(result, CliView.Compact));
        Assert.True(compact.Length < expanded.Length);
        using var shortDocument = JsonDocument.Parse(compact);
        using var fullDocument = JsonDocument.Parse(expanded);
        var shortRoot = shortDocument.RootElement;
        Assert.Equal(2, shortRoot.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("compact", shortRoot.GetProperty("view").GetString());
        var shortResult = shortRoot.GetProperty("result");
        var sets = shortResult.GetProperty("candidateSets");
        Assert.Equal(6, sets.GetArrayLength());
        Assert.Equal([1, 2, 3, 4, 5, 6], sets.EnumerateArray().Select(set => set.GetProperty("id").GetInt32()));
        var shortFindings = shortResult.GetProperty("domains")[0].GetProperty("findings");
        Assert.Equal(findings.Length, shortFindings.GetArrayLength());
        Assert.Equal(new int?[] { 1, 1, null, 2, 3, 4, 5, 6 }, shortFindings.EnumerateArray()
            .Select(finding => finding.GetProperty("candidateSet").ValueKind == JsonValueKind.Null ? (int?)null : finding.GetProperty("candidateSet").GetInt32()));
        var fullResult = fullDocument.RootElement.GetProperty("result");
        Assert.True(JsonElement.DeepEquals(fullResult.GetProperty("counts"), shortResult.GetProperty("counts")));
        Assert.Equal(8, shortResult.GetProperty("counts").GetProperty("severity").GetProperty("warning").GetProperty("value").GetInt32());
        var fullFindings = fullResult.GetProperty("domains")[0].GetProperty("findings");
        for (var index = 0; index < findings.Length; index++)
        {
            var actual = shortFindings[index];
            var expected = JsonNode.Parse(fullFindings[index].GetRawText())?.AsObject()
                ?? throw new InvalidOperationException("Missing finding.");
            var reference = actual.GetProperty("candidateSet");
            if (reference.ValueKind != JsonValueKind.Null)
            {
                var retained = JsonNode.Parse(sets[reference.GetInt32() - 1].GetRawText())?.AsObject()
                    ?? throw new InvalidOperationException("Missing candidate set.");
                Assert.True(retained.Remove("id"));
                Assert.True(JsonNode.DeepEquals(expected["candidates"], retained));
            }
            else
            {
                Assert.Null(expected["candidates"]);
            }

            Assert.True(expected.Remove("provenance"));
            Assert.True(expected.Remove("candidates"));
            expected["candidateSet"] = JsonNode.Parse(reference.GetRawText());
            Assert.True(JsonNode.DeepEquals(expected, JsonNode.Parse(actual.GetRawText())));
        }
    }

    private static string Render(DoctorResult result, CliView view)
        => DoctorJsonRenderer.Render(new CliPresentationRequest<DoctorResult>(result,
            new CliPresentation(CliOutputFormat.Json, view, CliVerbosity.Normal)));

    private static DoctorResult Result(IReadOnlyList<DoctorFinding> findings)
    {
        var counts = DoctorFindingAggregation.Count(findings);
        return new DoctorResult
        {
            Status = CliSemanticStatus.Attention,
            Workspace = null,
            Next = null,
            Diagnosis = new DoctorDiagnosis
            {
                Coverage = DoctorCoverageState.Complete,
                Counts = counts,
                Actions = [],
                Domains = [new DoctorDomainReport
                {
                    Domain = DoctorDomainKind.LocalReferences,
                    Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.Workspace, Path = "/work/demo" },
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
