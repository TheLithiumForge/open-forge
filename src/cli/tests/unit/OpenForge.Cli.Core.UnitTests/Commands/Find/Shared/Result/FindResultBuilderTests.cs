using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Projection;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Result;

public sealed class FindResultBuilderTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Find every finding code retains its fixed machine code, semantic status, and ordered result finding"),
        InlineData("InvalidInput", "Invalid", "find.invalid-input", "NotStarted", false),
        InlineData("InvalidSelector", "Invalid", "find.invalid-selector", "NotStarted", false),
        InlineData("WorkspaceUnavailable", "Blocked", "find.workspace-unavailable", "Blocked", false),
        InlineData("WorkspaceUnsafe", "Blocked", "find.workspace-unsafe", "Blocked", false),
        InlineData("SelectorAmbiguous", "Blocked", "find.selector-ambiguous", "Blocked", false),
        InlineData("SelectorUnsafe", "Blocked", "find.selector-unsafe", "Blocked", false),
        InlineData("IdentityCollision", "Attention", "find.identity-collision", "Complete", false),
        InlineData("CandidateUnsafe", "Incomplete", "find.candidate-unsafe", "Incomplete", false),
        InlineData("LayerUnresolved", "Incomplete", "find.layer-unresolved", "Incomplete", false),
        InlineData("InspectionUnavailable", "Incomplete", "find.inspection-unavailable", "Incomplete", false),
        InlineData("InvalidEncoding", "Incomplete", "find.invalid-encoding", "Incomplete", false),
        InlineData("FrontmatterUnavailable", "Incomplete", "find.frontmatter-unavailable", "Incomplete", false),
        InlineData("SectionAmbiguous", "Incomplete", "find.section-ambiguous", "Incomplete", false),
        InlineData("ProjectionMissing", "Attention", "find.projection-missing", "Complete", false),
        InlineData("ProjectionUnavailable", "Incomplete", "find.projection-unavailable", "Incomplete", false),
        InlineData("OperationFailed", "Failed", "find.operation-failed", "Failed", true),
        InlineData("Interrupted", "Interrupted", "find.interrupted", "Interrupted", false)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void EveryFindingCodeMapsToItsFixedStatusAndOrder(
        string codeValue,
        string expectedStatusValue,
        string expectedMachineCode,
        string expectedMatchingCoverageValue,
        bool includeShuffledFindings)
    {
        var code = Enum.Parse<FindFindingCode>(codeValue);
        var expectedStatus = Enum.Parse<CliSemanticStatus>(expectedStatusValue);
        var expectedMatchingCoverage = Enum.Parse<FindCoverageState>(expectedMatchingCoverageValue);
        IReadOnlyList<FindFinding> findings = includeShuffledFindings
            ? [
                CreateFinding(FindFindingCode.OperationFailed, CliSemanticStatus.Failed),
                CreateFinding(FindFindingCode.IdentityCollision, CliSemanticStatus.Attention),
                CreateFinding(FindFindingCode.InspectionUnavailable, CliSemanticStatus.Incomplete),
            ]
            : [CreateFinding(code, expectedStatus)];
        var input = CreateInput(
            new FindStageCompletion(expectedMatchingCoverage, FindProjectionCoverageState.NotRequested),
            findings,
            TerminalFor(expectedStatus));

        var result = new FindResultBuilder().Build(input);

        Assert.Equal(expectedStatus, result.Status);
        if (includeShuffledFindings)
        {
            Assert.Equal(
                [
                    FindFindingCode.IdentityCollision,
                    FindFindingCode.InspectionUnavailable,
                    FindFindingCode.OperationFailed,
                ],
                result.Findings.Select(finding => finding.Code));
            Assert.Equal(
                [
                    CliSemanticStatus.Attention,
                    CliSemanticStatus.Incomplete,
                    CliSemanticStatus.Failed,
                ],
                result.Findings.Select(finding => finding.Status));
            Assert.Equal(
                [
                    "find.identity-collision",
                    "find.inspection-unavailable",
                    "find.operation-failed",
                ],
                result.Findings.Select(finding => FindDefinitions.ReadFindingCode(finding.Code)));
            var operationFinding = Assert.Single(
                result.Findings,
                finding => finding.Code == code);
            Assert.Equal(expectedMachineCode, FindDefinitions.ReadFindingCode(operationFinding.Code));
        }
        else
        {
            var finding = Assert.Single(result.Findings);
            Assert.Equal(code, finding.Code);
            Assert.Equal(expectedStatus, finding.Status);
            Assert.Equal(expectedMachineCode, FindDefinitions.ReadFindingCode(finding.Code));
        }
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Find result counts, match positions, canonical order, and projection payload invariants are exact"),
        InlineData("ordered-payload", 2, 2, true),
        InlineData("zero-candidates", 0, 0, false)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void CoverageCountsAndPayloadInvariantsRemainExact(
        string scenario,
        int expectedCandidateCount,
        int expectedMatchCount,
        bool contentRequested)
    {
        var first = CreateSource(".agents/a.md");
        var second = CreateSource(".agents/b.md");
        var request = CreateRequest(contentRequested ? ["metadata", "frontmatter", "body"] : []);
        var matches = scenario switch
        {
            "ordered-payload" => new[] { Match(second, 8), Match(first, 9) },
            "zero-candidates" => Array.Empty<FindMatch>(),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The result count scenario is not defined."),
        };
        FindProjection[] projections = scenario == "ordered-payload"
            ? [
                TextProjection(second, FindContentPartKind.Body, "second-body"),
                MetadataProjection(first, 1),
                TextProjection(second, FindContentPartKind.Frontmatter, "second-frontmatter"),
                MetadataProjection(second, 2),
                TextProjection(first, FindContentPartKind.Body, "first-body"),
                TextProjection(first, FindContentPartKind.Frontmatter, "first-frontmatter"),
            ]
            : [];
        var universe = new FindUniverse(
            FindUniverseMode.Default,
            [],
            [],
            expectedCandidateCount,
            expectedCandidateCount,
            0);
        var input = new FindResultInput(
            new FindRequestEcho(request.Workspace, request.UniverseFilter, request.Query, request.Presentation),
            universe,
            [],
            matches,
            projections,
            [],
            new FindStageCompletion(
                FindCoverageState.Complete,
                contentRequested
                    ? FindProjectionCoverageState.Complete
                    : FindProjectionCoverageState.NotRequested),
            null);

        var result = new FindResultBuilder().Build(input);

        Assert.Equal(expectedCandidateCount, result.Universe.CandidateCount);
        Assert.Equal(expectedMatchCount, result.Matches.Count);
        Assert.Equal(expectedMatchCount, result.Universe.MatchedCount);
        Assert.Equal(
            Enumerable.Range(1, expectedMatchCount),
            result.Matches.Select(match => match.Position));
        if (scenario == "ordered-payload")
        {
            Assert.Equal(
                [first.Identity.AutomaticId, second.Identity.AutomaticId],
                result.Matches.Select(match => match.Id));
            Assert.Equal(
                [first.Identity.CanonicalBasePath, second.Identity.CanonicalBasePath],
                result.Matches.Select(match => match.Path));
            Assert.Equal(
                [
                    FindContentPartKind.Metadata,
                    FindContentPartKind.Frontmatter,
                    FindContentPartKind.Body,
                ],
                result.Matches[0].Projections.Select(projection => projection.Part));
            Assert.Equal(
                [
                    FindContentPartKind.Metadata,
                    FindContentPartKind.Frontmatter,
                    FindContentPartKind.Body,
                ],
                result.Matches[1].Projections.Select(projection => projection.Part));
            Assert.All(
                result.Matches.SelectMany(match => match.Projections),
                projection => Assert.Equal(FindProjectionState.Available, projection.State));
            Assert.Equal(first.Identity.AutomaticId, Assert.IsType<FindMetadata>(
                result.Matches[0].Projections[0].Metadata).Id);
            Assert.Equal("first-frontmatter", result.Matches[0].Projections[1].Text);
            Assert.Equal("first-body", result.Matches[0].Projections[2].Text);
        }
        else
        {
            Assert.Empty(result.Matches);
            Assert.Empty(result.Findings);
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Find ambiguous section projection makes result status and projection coverage incomplete while matching coverage remains complete")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void AmbiguousSectionProjectionPreservesCompleteMatchingCoverage()
    {
        var source = CreateSource(".agents/ambiguous.md");
        var request = CreateRequest(["section:Intro"]);
        var input = new FindResultInput(
            new FindRequestEcho(request.Workspace, request.UniverseFilter, request.Query, request.Presentation),
            new FindUniverse(FindUniverseMode.Default, [], [], 1, 1, 0),
            [],
            [Match(source, 1)],
            [new FindProjection(
                FindContentPartKind.Section,
                "Intro",
                SourceLayerKind.Base,
                source.Base.CanonicalPath,
                FindProjectionState.Ambiguous,
                null,
                null,
                [],
                null)],
            [],
            new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.Incomplete),
            null);

        var result = new FindResultBuilder().Build(input);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(FindCoverageState.Incomplete, result.Coverage.State);
        Assert.Equal(FindCoverageState.Complete, result.Coverage.Matching);
        Assert.Equal(FindProjectionCoverageState.Incomplete, result.Coverage.Projection);
        var match = Assert.Single(result.Matches);
        Assert.Equal(FindProjectionState.Ambiguous, Assert.Single(match.Projections).State);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(FindFindingCode.SectionAmbiguous, finding.Code);
        Assert.Equal(source.Identity.AutomaticId, finding.Source?.Id);
        Assert.Equal(source.Base.CanonicalPath, finding.Path);
        Assert.Equal("Intro", finding.Region?.Name);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Find result construction emits an identical matching and inspection finding instance exactly once")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void IdenticalInspectionFindingInstanceIsRetainedOnce()
    {
        var source = CreateSource(".agents/inspection.md");
        var finding = CreateFinding(FindFindingCode.InspectionUnavailable, CliSemanticStatus.Incomplete, source);
        var request = CreateRequest([]);
        var input = new FindResultInput(
            new FindRequestEcho(request.Workspace, request.UniverseFilter, request.Query, request.Presentation),
            new FindUniverse(FindUniverseMode.Default, [], [], 1, null, null),
            [UnavailableInspection(source, finding)],
            [],
            [],
            [finding],
            new FindStageCompletion(FindCoverageState.Incomplete, FindProjectionCoverageState.NotRequested),
            null);

        var result = new FindResultBuilder().Build(input);

        var resultFinding = Assert.Single(result.Findings);
        Assert.Same(finding, resultFinding);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Find status precedence and every fixed next action match the Interface contract"),
        InlineData("ordinary"),
        InlineData("blocked-and-terminal")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void StatusPrecedenceAndEveryNextActionMatchTheInterface(string scenarioGroup)
    {
        IReadOnlyList<StatusScenario> scenarios = scenarioGroup switch
        {
            "ordinary" =>
            [
                new(
                    CliSemanticStatus.Complete,
                    [],
                    new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.NotRequested),
                    null,
                    null,
                    null),
                new(
                    CliSemanticStatus.Attention,
                    [
                        new(FindFindingCode.IdentityCollision, CliSemanticStatus.Attention),
                        new(FindFindingCode.ProjectionMissing, CliSemanticStatus.Attention),
                    ],
                    new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.NotRequested),
                    null,
                    null,
                    null),
                new(
                    CliSemanticStatus.Incomplete,
                    [
                        new(FindFindingCode.ProjectionMissing, CliSemanticStatus.Attention),
                        new(FindFindingCode.InspectionUnavailable, CliSemanticStatus.Incomplete),
                    ],
                    new FindStageCompletion(FindCoverageState.Incomplete, FindProjectionCoverageState.NotRequested),
                    null,
                    "open-forge doctor",
                    "Inspect the unavailable source or projection facts before relying on this Find result."),
                new(
                    CliSemanticStatus.Invalid,
                    [
                        new(FindFindingCode.InvalidInput, CliSemanticStatus.Invalid),
                        new(FindFindingCode.InvalidSelector, CliSemanticStatus.Invalid),
                    ],
                    new FindStageCompletion(FindCoverageState.NotStarted, FindProjectionCoverageState.NotRequested),
                    null,
                    "open-forge find --help",
                    "Correct the named Find input, then rerun the request."),
            ],
            "blocked-and-terminal" =>
            [
                new(
                    CliSemanticStatus.Blocked,
                    [
                        new(FindFindingCode.InspectionUnavailable, CliSemanticStatus.Incomplete),
                        new(FindFindingCode.SelectorUnsafe, CliSemanticStatus.Blocked),
                        new(FindFindingCode.IdentityCollision, CliSemanticStatus.Attention),
                    ],
                    new FindStageCompletion(FindCoverageState.Blocked, FindProjectionCoverageState.NotRequested),
                    null,
                    "open-forge doctor",
                    "Inspect the blocked workspace or source boundary before rerunning Find."),
                new(
                    CliSemanticStatus.Blocked,
                    [
                        new(FindFindingCode.InspectionUnavailable, CliSemanticStatus.Incomplete),
                        new(FindFindingCode.SelectorAmbiguous, CliSemanticStatus.Blocked),
                        new(FindFindingCode.IdentityCollision, CliSemanticStatus.Attention),
                    ],
                    new FindStageCompletion(FindCoverageState.Blocked, FindProjectionCoverageState.NotRequested),
                    null,
                    "open-forge find",
                    "Replace every ambiguous selector with one listed exact path, then rerun the same request."),
                new(
                    CliSemanticStatus.Failed,
                    [
                        new(FindFindingCode.InspectionUnavailable, CliSemanticStatus.Incomplete),
                        new(FindFindingCode.OperationFailed, CliSemanticStatus.Failed),
                        new(FindFindingCode.IdentityCollision, CliSemanticStatus.Attention),
                    ],
                    new FindStageCompletion(FindCoverageState.Failed, FindProjectionCoverageState.NotRequested),
                    new FindTerminalEvent(
                        FindTerminalEventKind.Failed,
                        "Boundary failed while forming the Find result."),
                    "open-forge find --detail debug",
                    "Report the failure and retry the same request with bounded diagnostics."),
                new(
                    CliSemanticStatus.Interrupted,
                    [
                        new(FindFindingCode.InspectionUnavailable, CliSemanticStatus.Incomplete),
                        new(FindFindingCode.Interrupted, CliSemanticStatus.Interrupted),
                        new(FindFindingCode.IdentityCollision, CliSemanticStatus.Attention),
                    ],
                    new FindStageCompletion(FindCoverageState.Interrupted, FindProjectionCoverageState.NotRequested),
                    new FindTerminalEvent(
                        FindTerminalEventKind.Interrupted,
                        "The Find invocation was cancelled."),
                    "open-forge find",
                    "Rerun the same Find request."),
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(scenarioGroup), scenarioGroup, "The result status scenario group is not defined."),
        };

        foreach (var scenario in scenarios)
        {
            var findings = scenario.Findings
                .Select(finding => CreateFinding(finding.Code, finding.Status))
                .ToArray();
            var input = CreateInput(scenario.Completion, findings, scenario.TerminalEvent);

            var result = new FindResultBuilder().Build(input);

            Assert.Equal(scenario.ExpectedStatus, result.Status);
            if (scenario.ExpectedNextCommand is null)
            {
                Assert.Null(result.Next);
            }
            else
            {
                var next = Assert.IsType<CliNextAction>(result.Next);
                Assert.Equal(scenario.ExpectedNextCommand, next.Command);
                Assert.Equal(scenario.ExpectedNextReason, next.Reason);
            }
        }
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Find failed, interrupted, and incomplete results retain safe matches and bounded facts"),
        InlineData("InspectionUnavailable", "Incomplete", "Incomplete"),
        InlineData("OperationFailed", "Failed", "Complete"),
        InlineData("Interrupted", "Interrupted", "Complete")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void FailedInterruptedAndIncompleteResultsRetainSafeFacts(
        string findingCodeValue,
        string expectedStatusValue,
        string expectedMatchingCoverageValue)
    {
        var findingCode = Enum.Parse<FindFindingCode>(findingCodeValue);
        var expectedStatus = Enum.Parse<CliSemanticStatus>(expectedStatusValue);
        var expectedMatchingCoverage = Enum.Parse<FindCoverageState>(expectedMatchingCoverageValue);
        var source = CreateSource(".agents/safe.md");
        var request = CreateRequest([]);
        var finding = CreateFinding(findingCode, expectedStatus, source);
        var terminalEvent = expectedStatus switch
        {
            CliSemanticStatus.Failed => new FindTerminalEvent(
                FindTerminalEventKind.Failed,
                "Boundary failed while forming the Find result."),
            CliSemanticStatus.Interrupted => new FindTerminalEvent(
                FindTerminalEventKind.Interrupted,
                "The Find invocation was cancelled."),
            _ => null,
        };
        var input = new FindResultInput(
            new FindRequestEcho(request.Workspace, request.UniverseFilter, request.Query, request.Presentation),
            new FindUniverse(FindUniverseMode.Default, [], [], 1, 1, 0),
            [],
            [Match(source, 7)],
            [],
            [finding],
            new FindStageCompletion(expectedMatchingCoverage, FindProjectionCoverageState.NotRequested),
            terminalEvent);

        var result = new FindResultBuilder().Build(input);

        Assert.Equal(expectedStatus, result.Status);
        Assert.Single(result.Matches);
        Assert.Equal(source.Identity.AutomaticId, result.Matches[0].Id);
        Assert.Equal(expectedMatchingCoverage, result.Coverage.Matching);
        Assert.Contains(result.Findings, value => value.Code == findingCode);
        Assert.DoesNotContain(result.Findings, value => value.Cause.Contains("InvalidOperationException", StringComparison.Ordinal));
    }

    private static FindResultInput CreateInput(
        FindStageCompletion completion,
        IEnumerable<FindFinding> findings,
        FindTerminalEvent? terminalEvent)
    {
        var request = CreateRequest([]);
        return new FindResultInput(
            new FindRequestEcho(request.Workspace, request.UniverseFilter, request.Query, request.Presentation),
            new FindUniverse(FindUniverseMode.Default, [], [], 0, 0, null),
            [],
            [],
            [],
            findings,
            completion,
            terminalEvent);
    }

    private static FindRequest CreateRequest(IEnumerable<string> contentValues)
    {
        var contentParts = contentValues.Select(ReadContentPart).ToArray();
        var query = new FindQuery(
            [],
            [],
            FindRequirement.All,
            new FindRegionSelection(
                [],
                [new FindRegion(FindRegionKind.Frontmatter, null, "frontmatter")],
                [new FindRegion(FindRegionKind.Body, null, "body")]));
        return new FindRequest(
            CreateWorkspace(),
            new FindUniverseFilter([], []),
            query,
            new FindPresentationSelection(
                null,
                CliDetail.Standard,
                new FindContentSelection(contentParts, contentParts)));
    }

    private static FindTerminalEvent? TerminalFor(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Failed => new FindTerminalEvent(
                FindTerminalEventKind.Failed,
                "Boundary failed while forming the Find result."),
            CliSemanticStatus.Interrupted => new FindTerminalEvent(
                FindTerminalEventKind.Interrupted,
                "The Find invocation was cancelled."),
            _ => null,
        };

    private static FindFinding CreateFinding(
        FindFindingCode code,
        CliSemanticStatus expectedStatus,
        SourceLogicalSource? source = null)
    {
        var sourceIdentity = source is null
            ? null
            : new FindSourceIdentity(source.Identity.AutomaticId, source.Identity.CanonicalBasePath);
        return new FindFinding(
            code,
            expectedStatus,
            source is null ? null : source.Identity.AutomaticId,
            "The bounded Find evidence is not fully available.",
            null,
            null,
            sourceIdentity,
            source is null ? null : SourceLayerKind.Base,
            source?.Base.CanonicalPath,
            null,
            null,
            []);
    }

    private static FindLayerInspectionFacts UnavailableInspection(
        SourceLogicalSource source,
        FindFinding finding)
        => new(
            source,
            source.Base,
            null,
            null,
            null,
            null,
            [finding]);

    private static FindMatch Match(SourceLogicalSource source, int position)
        => new(
            position,
            source.Identity.AutomaticId,
            source.Identity.CanonicalBasePath,
            "Description",
            [],
            []);

    private static FindProjection MetadataProjection(SourceLogicalSource source, int position)
        => new(
            FindContentPartKind.Metadata,
            null,
            null,
            null,
            FindProjectionState.Available,
            new FindMetadata(
                position,
                source.Identity.AutomaticId,
                source.Identity.CanonicalBasePath,
                FindRouteState.Routed,
                source.Identity.AutomaticId,
                [new FindMetadataLayer(SourceLayerKind.Base, source.Base.CanonicalPath)]),
            null,
            [],
            null);

    private static FindProjection TextProjection(
        SourceLogicalSource source,
        FindContentPartKind part,
        string text)
        => new(
            part,
            null,
            SourceLayerKind.Base,
            source.Base.CanonicalPath,
            FindProjectionState.Available,
            null,
            text,
            [],
            new SourceLocation(1, 1, 0, text.Length));

    private static FindContentPart ReadContentPart(string value)
        => value switch
        {
            "metadata" => new FindContentPart(FindContentPartKind.Metadata, null, "metadata"),
            "frontmatter" => new FindContentPart(FindContentPartKind.Frontmatter, null, "frontmatter"),
            "headings" => new FindContentPart(FindContentPartKind.Headings, null, "headings"),
            "body" => new FindContentPart(FindContentPartKind.Body, null, "body"),
            _ when value.StartsWith("section:", StringComparison.Ordinal)
                => new FindContentPart(FindContentPartKind.Section, value["section:".Length..], value),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The result content part is not defined."),
        };

    private static SourceLogicalSource CreateSource(string path)
    {
        var root = CreateWorkspace().PhysicalRoot;
        var id = SourceIdentity.DeriveId(path)
            ?? throw new InvalidOperationException("The result source must have a derived ID.");
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, path),
            new SourceLayer(
                path,
                Physical(root, path),
                SourceDocumentForm.Markdown,
                SourceLayerKind.Base));
    }

    private static CliWorkspace CreateWorkspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-result-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static string Physical(string root, string logicalPath)
        => Path.GetFullPath(Path.Combine(root, logicalPath.Replace('/', Path.DirectorySeparatorChar)));

    private sealed record FindingExpectation(
        FindFindingCode Code,
        CliSemanticStatus Status);

    private sealed record StatusScenario(
        CliSemanticStatus ExpectedStatus,
        IReadOnlyList<FindingExpectation> Findings,
        FindStageCompletion Completion,
        FindTerminalEvent? TerminalEvent,
        string? ExpectedNextCommand,
        string? ExpectedNextReason);
}
