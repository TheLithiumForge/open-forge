using System.Text;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Projection;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Planning;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared.Planning;

public sealed class IndexPlanBuilderTests
{
    private const string AlphaPath = ".agents/alpha/_alpha.md";
    private const string UnchangedPath = ".agents/middle/_middle.md";
    private const string UpdatePath = ".agents/zeta/_zeta.md";
    private const string BeforeBody = "old entry\r\n";
    private const string ExpectedBody = "new entry\r\n";
    private const string Prefix = "# Root\r\n<!-- open-forge:generated-index:start -->\r\n";
    private const string Suffix = "<!-- open-forge:generated-index:end -->\r\nFooter\r\n";

    [Fact(DisplayName = "Index planning maps every projected state in canonical path order")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ProjectedStatesFormTruthfulOrderedPublicRegions()
    {
        var update = Available(
            path: UpdatePath,
            beforeBody: BeforeBody,
            expectedBody: ExpectedBody,
            beforeEntryCount: null);
        var unavailable = Unavailable(AlphaPath);
        var unchanged = Available(
            path: UnchangedPath,
            beforeBody: "current\n",
            expectedBody: "current\n",
            beforeEntryCount: 0);
        var input = Input(IndexMode.Apply, Projection(update, unavailable, unchanged));

        var plan = new IndexPlanBuilder().Build(input);

        Assert.False(plan.IsComplete);
        Assert.False(plan.IsNoOp);
        Assert.Equal(
            [AlphaPath, UnchangedPath, UpdatePath],
            plan.Regions.Select(region => region.Source.Path));

        var notEstablished = plan.Regions[0];
        Assert.Equal(IndexRegionAction.NotEstablished, notEstablished.Action);
        Assert.Equal(IndexRegionOutcome.NotEstablished, notEstablished.Outcome);
        Assert.Null(notEstablished.BeforeEntryCount);
        Assert.Null(notEstablished.ExpectedEntryCount);
        Assert.Null(notEstablished.Change);

        var alreadyCurrent = plan.Regions[1];
        Assert.Equal(IndexRegionAction.Unchanged, alreadyCurrent.Action);
        Assert.Equal(IndexRegionOutcome.AlreadyCurrent, alreadyCurrent.Outcome);
        Assert.Equal(0, alreadyCurrent.BeforeEntryCount);
        Assert.Equal(0, alreadyCurrent.ExpectedEntryCount);
        Assert.Null(alreadyCurrent.Change);

        var plannedUpdate = plan.Regions[2];
        Assert.Equal(IndexRegionAction.Update, plannedUpdate.Action);
        Assert.Equal(IndexRegionOutcome.NotStarted, plannedUpdate.Outcome);
        Assert.Null(plannedUpdate.BeforeEntryCount);
        Assert.Equal(0, plannedUpdate.ExpectedEntryCount);
        Assert.Equal(BeforeBody, Assert.IsType<IndexChange>(plannedUpdate.Change).BeforeBody);
        Assert.Equal(ExpectedBody, Assert.IsType<IndexChange>(plannedUpdate.Change).ExpectedBody);
        Assert.Empty(plan.Updates);
        Assert.Empty(plan.RecoveryTargets);
    }

    [Fact(DisplayName = "Index planning derives exact existing-target updates for apply and dry-run")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void CompleteUpdatesRetainBoundedBytesIdentityAndModeOutcome()
    {
        var projected = Available(
            path: UpdatePath,
            beforeBody: BeforeBody,
            expectedBody: ExpectedBody,
            beforeEntryCount: null);
        var projection = Projection(projected);
        var cases = new[]
        {
            (Mode: IndexMode.Apply, Outcome: IndexRegionOutcome.NotStarted),
            (Mode: IndexMode.DryRun, Outcome: IndexRegionOutcome.NotRequested),
        };

        foreach (var testCase in cases)
        {
            var input = Input(testCase.Mode, projection);
            var plan = new IndexPlanBuilder().Build(input);

            Assert.True(plan.IsComplete);
            Assert.False(plan.IsNoOp);
            Assert.Same(input, plan.Input);
            var region = Assert.Single(plan.Regions);
            Assert.Equal(testCase.Outcome, region.Outcome);
            Assert.Null(region.BeforeEntryCount);
            Assert.Equal(0, region.ExpectedEntryCount);
            var publicChange = Assert.IsType<IndexChange>(region.Change);
            Assert.Equal(BeforeBody, publicChange.BeforeBody);
            Assert.Equal(ExpectedBody, publicChange.ExpectedBody);

            var recoveryTarget = Assert.Single(plan.RecoveryTargets);
            var plannedChange = Assert.Single(plan.Updates);
            var boundedChange = projected.Region.Change
                ?? throw new InvalidOperationException("The update fixture requires its bounded change.");
            Assert.Same(recoveryTarget.Change, plannedChange);
            Assert.Equal(PlannedFileChangeKind.ReplaceGeneratedRegion, plannedChange.Kind);
            Assert.Equal(
                SourceLogicalPath.ToLexicalPath(input.Request.Workspace.LexicalRoot, UpdatePath),
                plannedChange.LogicalPath);
            Assert.Equal(projected.Region.PhysicalPath, recoveryTarget.Before.PhysicalPath);
            Assert.True(recoveryTarget.Before.HasBytes);
            Assert.True(recoveryTarget.RequiresRecovery);
            Assert.True(recoveryTarget.Before.Bytes.AsSpan().SequenceEqual(boundedChange.BeforeDocumentBytes.AsSpan()));
            Assert.True(plannedChange.IntendedBytes.AsSpan().SequenceEqual(boundedChange.ExpectedDocumentBytes.AsSpan()));
            Assert.True(boundedChange.PrefixPreserved);
            Assert.True(boundedChange.SuffixPreserved);
            Assert.Equal($"{Prefix}{BeforeBody}{Suffix}", Encoding.UTF8.GetString(recoveryTarget.Before.Bytes.AsSpan()));
            Assert.Equal($"{Prefix}{ExpectedBody}{Suffix}", Encoding.UTF8.GetString(plannedChange.IntendedBytes.AsSpan()));
        }
    }

    [Fact(DisplayName = "Index planning recognizes a complete unchanged projection as a verified no-op")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void CompleteUnchangedProjectionIsNoOp()
    {
        var projected = Available(
            path: UnchangedPath,
            beforeBody: "current\n",
            expectedBody: "current\n",
            beforeEntryCount: 0);

        var plan = new IndexPlanBuilder().Build(Input(IndexMode.Apply, Projection(projected)));

        Assert.True(plan.IsComplete);
        Assert.True(plan.IsNoOp);
        var region = Assert.Single(plan.Regions);
        Assert.Equal(IndexRegionAction.Unchanged, region.Action);
        Assert.Equal(IndexRegionOutcome.AlreadyCurrent, region.Outcome);
        Assert.Equal(0, region.BeforeEntryCount);
        Assert.Equal(0, region.ExpectedEntryCount);
        Assert.Empty(plan.Updates);
        Assert.Empty(plan.RecoveryTargets);
    }

    [Fact(DisplayName = "Index planning orders complete recovery targets by canonical projected path")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void CompleteRecoveryTargetsFollowCanonicalProjectionOrder()
    {
        var zeta = Available(
            path: UpdatePath,
            beforeBody: "zeta before\n",
            expectedBody: "zeta expected\n",
            beforeEntryCount: 1);
        var alpha = Available(
            path: AlphaPath,
            beforeBody: "alpha before\n",
            expectedBody: "alpha expected\n",
            beforeEntryCount: 1);
        var input = Input(IndexMode.Apply, Projection(zeta, alpha));

        var plan = new IndexPlanBuilder().Build(input);

        var expectedPaths = new[]
        {
            SourceLogicalPath.ToLexicalPath(input.Request.Workspace.LexicalRoot, AlphaPath),
            SourceLogicalPath.ToLexicalPath(input.Request.Workspace.LexicalRoot, UpdatePath),
        };
        Assert.Equal(expectedPaths, plan.Updates.Select(change => change.LogicalPath));
        Assert.Equal(expectedPaths, plan.RecoveryTargets.Select(target => target.Change.LogicalPath));
    }

    [Fact(DisplayName = "Index plan invariants reject incoherent regions and recovery targets")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void PlanRejectsNullMismatchedAndPartialExecutableFacts()
    {
        var projected = Available(
            path: UpdatePath,
            beforeBody: BeforeBody,
            expectedBody: ExpectedBody,
            beforeEntryCount: 1);
        var input = Input(IndexMode.Apply, Projection(projected));
        var valid = new IndexPlanBuilder().Build(input);
        var source = Assert.Single(valid.Regions).Source;

        Assert.Throws<ArgumentException>(() => new IndexPlan(
            input: input,
            regions: new IndexRegion[1],
            recoveryTargets: valid.RecoveryTargets));
        Assert.Throws<ArgumentException>(() => new IndexPlan(
            input: input,
            regions: valid.Regions,
            recoveryTargets: new RecoveryBundleTarget[1]));
        Assert.Throws<ArgumentException>(() => new IndexPlan(
            input: input,
            regions: [IndexRegion.NotEstablished(source)],
            recoveryTargets: valid.RecoveryTargets));
        Assert.Throws<ArgumentException>(() => new IndexPlan(
            input: input,
            regions: [Assert.Single(valid.Regions).WithOutcome(IndexRegionOutcome.Applied)],
            recoveryTargets: valid.RecoveryTargets));
        Assert.Throws<ArgumentException>(() => new IndexPlan(
            input: input,
            regions: valid.Regions,
            recoveryTargets: []));

        var otherLogicalPath = Physical("workspace/other.md");
        var otherPhysicalPath = Physical("physical/other.md");
        var otherBefore = FileStateSnapshot.File(
            logicalPath: otherLogicalPath,
            physicalPath: otherPhysicalPath,
            bytes: "before"u8);
        var otherChange = PlannedFileChange.ReplaceGeneratedRegion(
            expectation: otherBefore.Expectation,
            intendedDocumentBytes: "after"u8);
        var otherTarget = RecoveryBundleTarget.Create(
            change: otherChange,
            before: otherBefore);
        Assert.Throws<ArgumentException>(() => new IndexPlan(
            input: input,
            regions: valid.Regions,
            recoveryTargets: [otherTarget]));

        var exactTarget = Assert.Single(valid.RecoveryTargets);
        var wrongBefore = FileStateSnapshot.File(
            logicalPath: exactTarget.Before.LogicalPath,
            physicalPath: exactTarget.Before.PhysicalPath
                ?? throw new InvalidOperationException("The update fixture requires its target identity."),
            bytes: "wrong before"u8);
        var wrongBeforeChange = PlannedFileChange.ReplaceGeneratedRegion(
            expectation: wrongBefore.Expectation,
            intendedDocumentBytes: exactTarget.Change.IntendedBytes.AsSpan());
        Assert.Throws<ArgumentException>(() => new IndexPlan(
            input: input,
            regions: valid.Regions,
            recoveryTargets: [RecoveryBundleTarget.Create(
                change: wrongBeforeChange,
                before: wrongBefore)]));
        var wrongIntendedChange = PlannedFileChange.ReplaceGeneratedRegion(
            expectation: exactTarget.Before.Expectation,
            intendedDocumentBytes: "wrong intended"u8);
        Assert.Throws<ArgumentException>(() => new IndexPlan(
            input: input,
            regions: valid.Regions,
            recoveryTargets: [RecoveryBundleTarget.Create(
                change: wrongIntendedChange,
                before: exactTarget.Before)]));

        var incompleteProjected = Unavailable(AlphaPath);
        var incompleteInput = Input(IndexMode.Apply, Projection(incompleteProjected));
        Assert.Throws<ArgumentException>(() => new IndexPlan(
            input: incompleteInput,
            regions: [IndexRegion.NotEstablished(incompleteProjected.Source)],
            recoveryTargets: valid.RecoveryTargets));
        var unknownCount = Available(
            path: UnchangedPath,
            beforeBody: "current\n",
            expectedBody: "current\n",
            beforeEntryCount: null);
        Assert.Throws<InvalidOperationException>(() =>
            new IndexPlanBuilder().Build(Input(IndexMode.Apply, Projection(unknownCount))));
    }

    [Fact(DisplayName = "Index planning maps every mode and rejects undefined values")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void InitialOutcomeMappingIsExact()
    {
        Assert.Equal(IndexRegionOutcome.NotStarted, IndexPlanningOutcome.ReadInitial(IndexMode.Apply));
        Assert.Equal(IndexRegionOutcome.NotRequested, IndexPlanningOutcome.ReadInitial(IndexMode.DryRun));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexPlanningOutcome.ReadInitial((IndexMode)int.MaxValue));
    }

    private static IndexPlanningInput Input(
        IndexMode mode,
        IndexProjectionFormation projection)
        => new()
        {
            Request = new IndexRequest(
                Workspace(),
                projection.Selection.Selection.Sources.Select(source => source.Path),
                mode),
            Projection = projection,
        };

    private static IndexProjectionFormation Projection(params IndexProjectedRegion[] regions)
    {
        var ordered = regions.OrderBy(region => region.Source.Path, StringComparer.Ordinal).ToArray();
        var navigation = new GeneratedNavigationProjection(ordered.Select(region => region.Region));
        var selection = new IndexSelectionResolution(
            new IndexSelection(
                IndexSelectionOrigin.ExplicitSources,
                IndexSelectionScope.Rooted,
                ordered.Select(region => region.Source)),
            ordered.Select(region => region.Region.Source),
            []);
        var findings = navigation.IsComplete
            ? Array.Empty<IndexFinding>()
            : [new IndexFinding(
                IndexFindingCode.ProjectionIncomplete,
                sourceOccurrence: null,
                source: ordered.First(region => region.Region.State == GeneratedNavigationRegionState.Unavailable).Source,
                cause: "One projected region is unavailable for planning evidence.",
                candidates: [])];
        return new IndexProjectionFormation(
            selection: selection,
            projection: navigation,
            regions: ordered,
            findings: findings);
    }

    private static IndexProjectedRegion Available(
        string path,
        string beforeBody,
        string expectedBody,
        int? beforeEntryCount)
    {
        var source = Source(path);
        var change = new GeneratedNavigationBoundedChange(new GeneratedNavigationBoundedChangeInput
        {
            ContentLocation = new SourceLocation(
                line: 2,
                column: 1,
                byteOffset: Encoding.UTF8.GetByteCount(Prefix),
                byteLength: Encoding.UTF8.GetByteCount(beforeBody)),
            BeforeBody = beforeBody,
            ExpectedBody = expectedBody,
            Prefix = Prefix,
            Suffix = Suffix,
        });
        var region = GeneratedNavigationRegion.Available(
            source: source,
            entries: [],
            change: change);
        return new IndexProjectedRegion(
            region: region,
            source: LogicalSource(source),
            beforeEntryCount: beforeEntryCount);
    }

    private static IndexProjectedRegion Unavailable(string path)
    {
        var source = Source(path);
        return new IndexProjectedRegion(
            region: GeneratedNavigationRegion.Unavailable(
                source,
                GeneratedNavigationRegionUnavailableReason.GeneratedRegionMissing,
                "The generated region is missing."),
            source: LogicalSource(source),
            beforeEntryCount: null);
    }

    private static IndexLogicalSource LogicalSource(SourceLogicalSource source)
        => new(
            id: source.Identity.AutomaticId,
            path: source.Identity.CanonicalBasePath,
            scope: IndexLogicalSourceScope.Rooted);

    private static SourceLogicalSource Source(string path)
    {
        var automaticId = SourceIdentity.DeriveId(path)
            ?? throw new ArgumentException("An Index planning test source requires a derived ID.", nameof(path));
        return new SourceLogicalSource(
            new SourceLogicalIdentity(
                automaticId: automaticId,
                canonicalBasePath: path),
            new SourceLayer(
                canonicalPath: path,
                physicalPath: Physical($"physical/{path}"),
                form: SourceDocumentForm.CanonicalEntrypoint,
                kind: SourceLayerKind.Base));
    }

    private static CliWorkspace Workspace()
        => new(
            lexicalRoot: Physical("workspace"),
            physicalRoot: Physical("workspace"),
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static string Physical(string relativePath)
        => Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "open-forge-index-planning-unit",
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
}
