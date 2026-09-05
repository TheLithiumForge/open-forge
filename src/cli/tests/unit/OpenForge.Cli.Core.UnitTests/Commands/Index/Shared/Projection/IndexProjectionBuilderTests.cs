using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Projection;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Projection;
using OpenForge.Cli.Core.Commands.Index.Shared.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared.Projection;

public sealed class IndexProjectionBuilderTests
{
    private const string RootPath = ".agents/root/_root.md";
    private const string NestedPath = ".agents/root/nested/_nested.md";

    [Theory(DisplayName = "Index projection assigns every catalogue issue to its accepted local owner")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    [InlineData(SourceCatalogueIssueCode.RootMissing, null)]
    [InlineData(SourceCatalogueIssueCode.RootUnsafe, null)]
    [InlineData(SourceCatalogueIssueCode.RootUnavailable, null)]
    [InlineData(SourceCatalogueIssueCode.DirectoryUnavailable, IndexFindingCode.DiscoveryIncomplete)]
    [InlineData(SourceCatalogueIssueCode.CandidateUnsafe, IndexFindingCode.TargetUnsafe)]
    [InlineData(SourceCatalogueIssueCode.CandidateUnavailable, IndexFindingCode.DiscoveryIncomplete)]
    [InlineData(SourceCatalogueIssueCode.IdentityUnavailable, IndexFindingCode.DiscoveryIncomplete)]
    [InlineData(SourceCatalogueIssueCode.IdentityCollision, null)]
    [InlineData(SourceCatalogueIssueCode.OrphanOverwrite, null)]
    internal void CatalogueIssueMappingsPreserveSelectionAndProjectionOwnership(
        SourceCatalogueIssueCode issueCode,
        IndexFindingCode? expectedCode)
    {
        Assert.Equal(expectedCode, IndexProjectionBuilder.ReadCatalogueFindingCode(issueCode));
    }

    [Fact(DisplayName = "Index projection blocks incompatible direct-child aliases and admits compatible aliases")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void DirectChildAliasCompatibilityUsesFormationProof()
    {
        var root = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var compatiblePhysicalPath = Physical("shared/compatible.md");
        var alpha = Source(".agents/root/alpha.md", SourceDocumentForm.Markdown, compatiblePhysicalPath);
        var zeta = Source(".agents/root/zeta.md", SourceDocumentForm.Markdown, compatiblePhysicalPath);
        var compatible = AliasFormation(root, alpha, zeta);
        var compatibleRoot = compatible.FindSource(RootPath)
            ?? throw new InvalidOperationException("The compatible formation must retain its target.");
        var compatibleSelection = Selection(compatible, compatibleRoot);

        var compatibleReadiness = IndexProjectionBuilder.ResolveCatalogue(
            compatible,
            compatibleSelection);

        Assert.Empty(compatibleReadiness.Findings);
        Assert.Empty(compatibleReadiness.UnavailableTargetPaths);
        var compatibleProjection = new IndexProjectionBuilder().Form(new IndexProjectionAssembly
        {
            Formation = compatible,
            Selection = compatibleSelection,
            Regions = [IndexProjectionRegionInput.FromDocument(
                compatibleRoot,
                new MarkdownDocumentParser().Parse(OpenForgeDocumentSeed.GeneratedEntries(entries: "stale")))],
            Metadata = [new GeneratedNavigationMetadata(
                alpha,
                SourceAuthoredMetadataFacts.Complete("Alpha", ["Docs"]))],
            Readiness = compatibleReadiness,
        });
        var entry = Assert.Single(Assert.Single(compatibleProjection.Projection.Regions).Entries);
        Assert.Equal(alpha.Identity.CanonicalBasePath, entry.CanonicalPath);

        var incompatiblePhysicalPath = Physical("shared/incompatible.md");
        var ordinary = Source(".agents/root/ordinary.md", SourceDocumentForm.Markdown, incompatiblePhysicalPath);
        var skill = Source(".agents/root/tool/SKILL.md", SourceDocumentForm.Skill, incompatiblePhysicalPath);
        var incompatible = AliasFormation(root, ordinary, skill);

        var incompatibleReadiness = IndexProjectionBuilder.ResolveCatalogue(
            incompatible,
            Selection(incompatible, incompatible.FindSource(RootPath)
                ?? throw new InvalidOperationException("The incompatible formation must retain its target.")));

        Assert.Equal(IndexFindingCode.TargetUnsafe, Assert.Single(incompatibleReadiness.Findings).Code);
        Assert.Equal([RootPath], incompatibleReadiness.UnavailableTargetPaths);
    }

    [Fact(DisplayName = "Index projection slices each target without leaking sibling, detached, or nested-entrypoint issues")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void CatalogueSlicesRetainOnlyTargetAndDirectDiscoveryDependencies()
    {
        var root = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var nested = Source(NestedPath, SourceDocumentForm.CanonicalEntrypoint);
        var ordinary = Source(".agents/root/ordinary.md", SourceDocumentForm.Markdown);
        var unsafeCandidate = Candidate(".agents/root/unsafe.md", PhysicalPathState.External);
        var issues = new[]
        {
            DirectoryIssue(".agents/root/unavailable.md", Physical(".agents/root/unavailable")),
            DirectoryIssue(".agents/root/nested/deep.md", Physical(".agents/root/nested/deep")),
            DirectoryIssue(".agents/sibling/unavailable.md", Physical(".agents/sibling/unavailable")),
            DirectoryIssue(".agents/detached/unavailable.md", Physical(".agents/detached/unavailable")),
            CandidateIssue(SourceCatalogueIssueCode.CandidateUnsafe, unsafeCandidate),
        };
        var formation = Formation([ordinary, nested, root], [unsafeCandidate], issues);

        var rootOnly = IndexProjectionBuilder.ResolveCatalogue(
            formation,
            Selection(formation, root));
        var rootAndNested = IndexProjectionBuilder.ResolveCatalogue(
            formation,
            Selection(formation, root, nested));

        Assert.Equal(
            [IndexFindingCode.TargetUnsafe, IndexFindingCode.DiscoveryIncomplete],
            rootOnly.Findings.Select(finding => finding.Code));
        Assert.All(rootOnly.Findings, finding => Assert.Equal(RootPath, finding.Source?.Path));
        Assert.Equal([RootPath], rootOnly.UnavailableTargetPaths);
        Assert.Equal(
            [IndexFindingCode.TargetUnsafe, IndexFindingCode.DiscoveryIncomplete, IndexFindingCode.DiscoveryIncomplete],
            rootAndNested.Findings.Select(finding => finding.Code));
        Assert.Equal(
            [RootPath, NestedPath],
            rootAndNested.UnavailableTargetPaths);
        Assert.Contains(rootAndNested.Findings, finding => finding.Source?.Path == NestedPath);
        Assert.DoesNotContain(
            rootAndNested.Findings,
            finding => finding.Source?.Path is ".agents/sibling/_sibling.md" or ".agents/detached/_detached.md");
    }

    [Fact(DisplayName = "Index projection retains distinct direct-child destinations across an automatic ID collision")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void IdentityCollisionIsNonblockingWhenCanonicalDestinationsRemainDistinct()
    {
        var root = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var leaf = Source(".agents/root/item.md", SourceDocumentForm.Markdown);
        var entrypoint = Source(".agents/root/item/_item.md", SourceDocumentForm.CanonicalEntrypoint);
        Assert.Equal(leaf.Identity.AutomaticId, entrypoint.Identity.AutomaticId);
        var issue = new SourceCatalogueIssue(
            SourceCatalogueIssueCode.IdentityCollision,
            leaf.Identity.CanonicalBasePath,
            [leaf.Identity.CanonicalBasePath, entrypoint.Identity.CanonicalBasePath],
            scopePhysicalPath: Physical(".agents/root"),
            failure: null);
        var formation = Formation([entrypoint, leaf, root], issues: [issue]);
        var selection = Selection(formation, root);

        var readiness = IndexProjectionBuilder.ResolveCatalogue(formation, selection);
        var projection = new IndexProjectionBuilder().Form(new IndexProjectionAssembly
        {
            Formation = formation,
            Selection = selection,
            Regions = [IndexProjectionRegionInput.FromDocument(
                root,
                new MarkdownDocumentParser().Parse(OpenForgeDocumentSeed.GeneratedEntries(entries: "stale")))],
            Metadata =
            [
                new GeneratedNavigationMetadata(
                    leaf,
                    SourceAuthoredMetadataFacts.Complete("Item leaf", ["Docs"])),
                new GeneratedNavigationMetadata(
                    entrypoint,
                    SourceAuthoredMetadataFacts.Complete("Item entrypoint", ["Docs"])),
            ],
            Readiness = readiness,
        });

        Assert.Empty(readiness.Findings);
        Assert.Empty(readiness.UnavailableTargetPaths);
        Assert.True(projection.IsComplete);
        Assert.Equal(
            ["item.md", "item/_item.md"],
            Assert.Single(projection.Projection.Regions).Entries.Select(entry => entry.Destination));
    }

    [Theory(DisplayName = "Index projection maps every neutral generated-navigation reason without cause inspection")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    [InlineData(GeneratedNavigationRegionUnavailableReason.RegionSourceUnsupported, IndexFindingCode.TargetUnsafe)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.SourceDocumentUnavailable, IndexFindingCode.ProjectionIncomplete)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.GeneratedRegionMissing, IndexFindingCode.GeneratedRegionUnsafe)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.GeneratedRegionInvalid, IndexFindingCode.GeneratedRegionUnsafe)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.GeneratedRegionUnavailable, IndexFindingCode.ProjectionIncomplete)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.GeneratedRegionLineEndingUnsupported, IndexFindingCode.GeneratedRegionUnsafe)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.TopologyUnavailable, IndexFindingCode.DiscoveryIncomplete)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.TopologyUnsafe, IndexFindingCode.TargetUnsafe)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.MetadataUnavailable, IndexFindingCode.MetadataIncomplete)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.MetadataInvalid, IndexFindingCode.MetadataUnsafe)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.MetadataUnrepresentable, IndexFindingCode.MetadataUnsafe)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.DestinationUnsafe, IndexFindingCode.TargetUnsafe)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.DestinationConflict, IndexFindingCode.TargetUnsafe)]
    [InlineData(GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable, IndexFindingCode.ProjectionIncomplete)]
    internal void GeneratedNavigationReasonsMapExhaustively(
        GeneratedNavigationRegionUnavailableReason reason,
        IndexFindingCode expectedCode)
    {
        Assert.Equal(expectedCode, IndexProjectionBuilder.ReadFindingCode(reason));
    }

    [Fact(DisplayName = "Index projection rejects an undefined generated-navigation reason")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void UndefinedGeneratedNavigationReasonIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            IndexProjectionBuilder.ReadFindingCode((GeneratedNavigationRegionUnavailableReason)int.MaxValue));
    }

    [Fact(DisplayName = "Index projection input counts only wholly parseable generated Entries interiors")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ProjectionInputDerivesBeforeCountFromTheAuthoritativeParser()
    {
        var source = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var parser = new MarkdownDocumentParser();
        var complete = IndexProjectionRegionInput.FromDocument(
            source,
            parser.Parse(OpenForgeDocumentSeed.GeneratedEntries(
                "- [Alpha](alpha.md) - #Docs\n- [Beta](beta.md) - #Docs")));
        var empty = IndexProjectionRegionInput.FromDocument(
            source,
            parser.Parse(OpenForgeDocumentSeed.GeneratedEntries("- none - No entries - #Empty")));
        var unparseable = IndexProjectionRegionInput.FromDocument(
            source,
            parser.Parse(OpenForgeDocumentSeed.GeneratedEntries("stale generated body")));
        var missing = IndexProjectionRegionInput.FromDocument(
            source,
            parser.Parse("# Root\n"));

        Assert.Equal(2, complete.BeforeEntryCount);
        Assert.Equal(0, empty.BeforeEntryCount);
        Assert.Null(unparseable.BeforeEntryCount);
        Assert.Null(missing.BeforeEntryCount);
    }

    [Fact(DisplayName = "Index projected regions derive expected counts for every generated-navigation state")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ProjectedRegionExpectedCountMappingRejectsUndefinedState()
    {
        Assert.Equal(
            2,
            IndexProjectedRegion.ReadExpectedEntryCount(
                GeneratedNavigationRegionState.Available,
                entryCount: 2));
        Assert.Null(IndexProjectedRegion.ReadExpectedEntryCount(
            GeneratedNavigationRegionState.Unavailable,
            entryCount: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => IndexProjectedRegion.ReadExpectedEntryCount(
            (GeneratedNavigationRegionState)int.MaxValue,
            entryCount: 0));
    }

    [Theory(DisplayName = "Index projection maps non-readable source-layer verification by document role")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    [InlineData(SourceLayerVerificationState.Missing, IndexFindingCode.TargetChanged, IndexFindingCode.TargetChanged)]
    [InlineData(SourceLayerVerificationState.Unsafe, IndexFindingCode.TargetUnsafe, IndexFindingCode.TargetUnsafe)]
    [InlineData(SourceLayerVerificationState.Unavailable, IndexFindingCode.ProjectionIncomplete, IndexFindingCode.MetadataIncomplete)]
    [InlineData(SourceLayerVerificationState.Changed, IndexFindingCode.TargetChanged, IndexFindingCode.TargetChanged)]
    [InlineData(SourceLayerVerificationState.Cancelled, IndexFindingCode.Interrupted, IndexFindingCode.Interrupted)]
    internal void VerificationStateMappingsPreserveAcquisitionMeaning(
        SourceLayerVerificationState state,
        IndexFindingCode expectedTargetCode,
        IndexFindingCode expectedMetadataCode)
    {
        var result = VerificationResult(state);

        Assert.Equal(
            expectedTargetCode,
            IndexProjectionBuilder.ReadAcquisitionFindingCode(result, IndexProjectionDocumentRole.Target));
        Assert.Equal(
            expectedMetadataCode,
            IndexProjectionBuilder.ReadAcquisitionFindingCode(result, IndexProjectionDocumentRole.Metadata));
    }

    [Theory(DisplayName = "Index projection maps verified read states by target and metadata purpose")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    [InlineData(FileReadState.Complete, null, null)]
    [InlineData(FileReadState.Missing, IndexFindingCode.TargetChanged, IndexFindingCode.TargetChanged)]
    [InlineData(FileReadState.InvalidEncoding, IndexFindingCode.TargetUnsafe, IndexFindingCode.MetadataUnsafe)]
    [InlineData(FileReadState.InvalidSyntax, IndexFindingCode.TargetUnsafe, IndexFindingCode.MetadataUnsafe)]
    [InlineData(FileReadState.AccessDenied, IndexFindingCode.ProjectionIncomplete, IndexFindingCode.MetadataIncomplete)]
    [InlineData(FileReadState.InputOutputFailure, IndexFindingCode.ProjectionIncomplete, IndexFindingCode.MetadataIncomplete)]
    [InlineData(FileReadState.Cancelled, IndexFindingCode.Interrupted, IndexFindingCode.Interrupted)]
    internal void VerifiedReadMappingsPreserveAcquisitionMeaning(
        FileReadState state,
        IndexFindingCode? expectedTargetCode,
        IndexFindingCode? expectedMetadataCode)
    {
        var result = VerifiedReadResult(state);

        Assert.Equal(
            expectedTargetCode,
            IndexProjectionBuilder.ReadAcquisitionFindingCode(result, IndexProjectionDocumentRole.Target));
        Assert.Equal(
            expectedMetadataCode,
            IndexProjectionBuilder.ReadAcquisitionFindingCode(result, IndexProjectionDocumentRole.Metadata));
    }

    [Fact(DisplayName = "Index projection forms one complete generated-navigation projection for the whole selected closure")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void OneAssemblyProjectsTheWholeClosureAndEnforcesCoverage()
    {
        var root = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", SourceDocumentForm.Markdown);
        var formation = Formation([child, root]);
        var selection = Selection(formation, root);
        var document = new MarkdownDocumentParser().Parse(
            OpenForgeDocumentSeed.GeneratedEntries(entries: "stale"));
        var assembly = new IndexProjectionAssembly
        {
            Formation = formation,
            Selection = selection,
            Regions = [IndexProjectionRegionInput.FromDocument(root, document)],
            Metadata = [new GeneratedNavigationMetadata(
                child,
                SourceAuthoredMetadataFacts.Complete("Child", ["Docs"]))],
            Readiness = new IndexProjectionReadiness([], []),
        };
        var builder = new IndexProjectionBuilder();

        var result = builder.Form(assembly);

        Assert.True(result.IsComplete);
        var region = Assert.Single(result.Projection.Regions);
        var projectedRegion = Assert.Single(result.Regions);
        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.Same(region, projectedRegion.Region);
        Assert.Equal(
            IndexLogicalSourceProjector.Project(
                source: root,
                formation: formation),
            projectedRegion.Source);
        Assert.Equal(IndexLogicalSourceScope.Detached, projectedRegion.Source.Scope);
        Assert.Null(projectedRegion.BeforeEntryCount);
        Assert.Equal(1, projectedRegion.ExpectedEntryCount);
        Assert.Equal("- [Child](child.md) - #Docs", Assert.Single(region.Entries).Line);
        Assert.Throws<ArgumentException>(() => new IndexProjectionFormation(
            selection: selection,
            projection: result.Projection,
            regions: [],
            findings: []));
        Assert.Throws<ArgumentException>(() => builder.Form(assembly with
        {
            Regions = [],
        }));
        var mismatchedSource = IndexLogicalSourceProjector.Project(
            source: Source(NestedPath, SourceDocumentForm.CanonicalEntrypoint),
            formation: formation);
        Assert.Throws<ArgumentException>(() => new IndexProjectedRegion(
            region: region,
            source: mismatchedSource,
            beforeEntryCount: projectedRegion.BeforeEntryCount));
    }

    [Fact(DisplayName = "Index projected closure targets retain shared rooted source facts")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void RootedProjectedRegionRetainsSharedLogicalSourceProjection()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var root = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", SourceDocumentForm.Markdown);
        var formation = Formation([loader, child, root]);
        var result = new IndexProjectionBuilder().Form(new IndexProjectionAssembly
        {
            Formation = formation,
            Selection = Selection(formation, root),
            Regions = [IndexProjectionRegionInput.FromDocument(
                root,
                new MarkdownDocumentParser().Parse(OpenForgeDocumentSeed.GeneratedEntries(entries: "stale")))],
            Metadata = [new GeneratedNavigationMetadata(
                child,
                SourceAuthoredMetadataFacts.Complete("Child", ["Docs"]))],
            Readiness = new IndexProjectionReadiness([], []),
        });

        var projectedRegion = Assert.Single(result.Regions);
        Assert.Equal(
            IndexLogicalSourceProjector.Project(
                source: root,
                formation: formation),
            projectedRegion.Source);
        Assert.Equal(IndexLogicalSourceScope.Rooted, projectedRegion.Source.Scope);
    }

    [Fact(DisplayName = "Index projection readiness rejects null and incoherent unavailable-target evidence")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ReadinessRequiresFindingsAndCanonicalUnavailableTargetsTogether()
    {
        var finding = new IndexFinding(
            IndexFindingCode.ProjectionIncomplete,
            sourceOccurrence: null,
            source: null,
            cause: "The exact projection is unavailable.",
            candidates: []);

        Assert.Throws<ArgumentException>(() => new IndexProjectionReadiness([finding], []));
        Assert.Throws<ArgumentException>(() => new IndexProjectionReadiness([], [RootPath]));
        Assert.Throws<ArgumentException>(() => new IndexProjectionReadiness([finding], ["root.md"]));
        Assert.Throws<ArgumentException>(() => new IndexProjectionReadiness(new IndexFinding[1], [RootPath]));
        var valid = new IndexProjectionReadiness([finding], [RootPath]);
        Assert.Equal(RootPath, Assert.Single(valid.UnavailableTargetPaths));
    }

    private static IndexSelectionResolution Selection(
        GeneratedNavigationFormation formation,
        params SourceLogicalSource[] targets)
    {
        var selected = targets.Select(target => IndexLogicalSourceProjector.Project(
            source: target,
            formation: formation)).ToArray();
        var rooted = selected.Count(source => source.Scope == IndexLogicalSourceScope.Rooted);
        var scope = rooted switch
        {
            0 => IndexSelectionScope.Detached,
            _ when rooted == selected.Length => IndexSelectionScope.Rooted,
            _ => IndexSelectionScope.Mixed,
        };
        return new IndexSelectionResolution(
            new IndexSelection(IndexSelectionOrigin.ExplicitSources, scope, selected),
            targets,
            []);
    }

    private static GeneratedNavigationFormation Formation(
        IEnumerable<SourceLogicalSource> sources,
        IEnumerable<SourceCandidate>? candidates = null,
        IEnumerable<SourceCatalogueIssue>? issues = null)
        => new GeneratedNavigationFormationBuilder().Build(
            GeneratedNavigationTestData.Catalogue(
                sources,
                additionalCandidates: candidates,
                issues: issues));

    private static GeneratedNavigationFormation AliasFormation(
        SourceLogicalSource root,
        SourceLogicalSource firstAlias,
        SourceLogicalSource secondAlias)
    {
        var issue = new SourceCatalogueIssue(
            SourceCatalogueIssueCode.PhysicalAlias,
            firstAlias.Identity.CanonicalBasePath,
            [firstAlias.Identity.CanonicalBasePath, secondAlias.Identity.CanonicalBasePath],
            scopePhysicalPath: Physical(".agents/root"),
            failure: null);
        return Formation([firstAlias, root, secondAlias], issues: [issue]);
    }

    private static SourceCatalogueIssue DirectoryIssue(string attemptedPath, string scopePhysicalPath)
        => new(
            SourceCatalogueIssueCode.DirectoryUnavailable,
            attemptedPath,
            relatedPaths: [],
            scopePhysicalPath: scopePhysicalPath,
            failure: null);

    private static SourceCatalogueIssue CandidateIssue(
        SourceCatalogueIssueCode code,
        SourceCandidate candidate)
        => new(
            code,
            candidate.CanonicalPath,
            relatedPaths: [],
            scopePhysicalPath: candidate.PhysicalParentPath,
            failure: null);

    private static SourceCandidate Candidate(string canonicalPath, PhysicalPathState state)
        => new(
            canonicalPath,
            SourceDocumentForm.Markdown,
            SourceIdentity.DeriveId(canonicalPath),
            state,
            physicalPath: state == PhysicalPathState.Contained ? Physical(canonicalPath) : null,
            physicalParentPath: Physical(SourceLogicalPath.ReadParent(canonicalPath)));

    private static SourceDocumentReadResult VerificationResult(SourceLayerVerificationState state)
    {
        var source = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var currentPhysicalPath = state == SourceLayerVerificationState.Changed
            ? Physical("changed/_root.md")
            : null;
        var failure = state == SourceLayerVerificationState.Unavailable
            ? new FilesystemFailure(FilesystemFailureKind.InputOutput, "The test read is unavailable.")
            : null;
        var verification = new SourceLayerVerification(source.Base, state, currentPhysicalPath, failure);
        var read = state == SourceLayerVerificationState.Missing
            ? FileReadResult<string>.Missing(source.Base.CanonicalPath)
            : null;
        return new SourceDocumentReadResult(source.Base, verification, read);
    }

    private static SourceDocumentReadResult VerifiedReadResult(FileReadState state)
    {
        var source = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var verification = new SourceLayerVerification(
            source.Base,
            SourceLayerVerificationState.Verified,
            source.Base.PhysicalPath,
            null);
        var read = state switch
        {
            FileReadState.Complete => FileReadResult<string>.Complete(source.Base.CanonicalPath, "# Root\n"),
            FileReadState.Missing => FileReadResult<string>.Missing(source.Base.CanonicalPath),
            FileReadState.InvalidEncoding
                or FileReadState.InvalidSyntax
                or FileReadState.AccessDenied
                or FileReadState.InputOutputFailure => FileReadResult<string>.Failed(
                    state,
                    source.Base.CanonicalPath,
                    Failure(state)),
            FileReadState.Cancelled => FileReadResult<string>.Cancelled(source.Base.CanonicalPath),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The test read state is not defined."),
        };
        return new SourceDocumentReadResult(source.Base, verification, read);
    }

    private static FilesystemFailure Failure(FileReadState state)
    {
        var kind = state switch
        {
            FileReadState.InvalidEncoding => FilesystemFailureKind.InvalidEncoding,
            FileReadState.InvalidSyntax => FilesystemFailureKind.InvalidSyntax,
            FileReadState.AccessDenied => FilesystemFailureKind.AccessDenied,
            FileReadState.InputOutputFailure => FilesystemFailureKind.InputOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The read state has no failure kind."),
        };
        return new FilesystemFailure(kind, "The test read failed.");
    }

    private static SourceLogicalSource Source(
        string canonicalPath,
        SourceDocumentForm form,
        string? physicalPath = null)
    {
        var identity = new SourceLogicalIdentity(
            SourceIdentity.DeriveId(canonicalPath)
                ?? throw new ArgumentException("An Index projection test source requires a derived ID.", nameof(canonicalPath)),
            canonicalPath);
        return new SourceLogicalSource(
            identity,
            new SourceLayer(
                canonicalPath,
                physicalPath ?? Physical(canonicalPath),
                form,
                SourceLayerKind.Base));
    }

    private static string Physical(string relativePath)
        => Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "open-forge-index-projection-unit",
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
}
