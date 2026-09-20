using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Filesystem;

public sealed class RouteListInventoryFactsTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list inventory request defaults, validates, sorts, and snapshots logical roots")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void RequestOwnsCanonicalLogicalRootSnapshot()
    {
        using var cancellation = new CancellationTokenSource();
        var roots = new List<string> { ".agents/z", ".agents/a" };
        var request = new RouteListInventoryRequest(Workspace(), roots, cancellation.Token);
        roots.Clear();
        var defaultRequest = new RouteListInventoryRequest(Workspace(), cancellation.Token);

        Assert.Equal([".agents/a", ".agents/z"], request.LogicalRoots);
        Assert.Equal([".agents"], defaultRequest.LogicalRoots);
        Assert.Equal(cancellation.Token, request.CancellationToken);
        Assert.Throws<ArgumentException>(() => new RouteListInventoryRequest(Workspace(), [], default));
        Assert.Throws<ArgumentException>(() => new RouteListInventoryRequest(Workspace(), ["agents"], default));
        Assert.Throws<ArgumentException>(() => new RouteListInventoryRequest(
            Workspace(),
            [".agents/a", ".agents/a/child"],
            default));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list inventory facts snapshot sources, findings, aliases, and catalogue projection")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void FactsSnapshotAndOrderEveryProjection()
    {
        var first = Source("collision", ".agents/collision.md", "physical-one.md");
        var second = Source(
            "collision",
            ".agents/collision/_collision.md",
            "physical-two.md",
            RouteListSourceKind.Entrypoint,
            SourceDocumentForm.CanonicalEntrypoint);
        var sources = new List<RouteListInventorySource> { second, first };
        var findings = new List<RouteListFilesystemFinding>
        {
            new RouteListFilesystemFinding(
                RouteListFindingCode.ReadUnavailable,
                CliSemanticStatus.Incomplete,
                ".agents/z.md",
                "The source metadata could not be read."),
            RouteListFilesystemFindingPolicy.OrphanOverwrite(".agents/a.overwrite.md"),
        };
        var aliases = new List<RouteListPhysicalAlias>
        {
            Alias(".agents/z.md", "physical-two.md", ".agents/a.md"),
        };

        var neutral = NeutralInputs(sources, isCancelled: true);
        var facts = RouteListInventoryFacts.Create(
            neutral.SourceCatalogue,
            neutral.ProjectionBuildResult,
            sources,
            findings,
            aliases);
        sources.Clear();
        findings.Clear();
        aliases.Clear();

        Assert.Equal(
            [".agents/collision.md", ".agents/collision/_collision.md"],
            facts.Sources.Select(source => source.Source.CanonicalPath));
        Assert.Equal(
            [".agents/a.overwrite.md", ".agents/z.md"],
            facts.Findings.Select(finding => finding.CanonicalLogicalSubject));
        Assert.Single(facts.PhysicalAliases);
        Assert.Same(neutral.SourceCatalogue, facts.SourceCatalogue);
        Assert.Same(neutral.ProjectionBuildResult, facts.ProjectionBuildResult);
        Assert.Equal(2, facts.ProjectionBuildResult.ProjectionSet.FindAllById("collision").Count);
        Assert.Same(
            first.Source,
            facts.ProjectionBuildResult.ProjectionSet.FindByPath(".agents/collision.md"));
        Assert.Equal(RouteListInventoryState.Incomplete, facts.State);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list inventory findings use canonical subject and machine-code order with stable ties")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void FindingOrderIsDeterministicAndStableForTies()
    {
        var firstTie = new RouteListFilesystemFinding(
            RouteListFindingCode.AuthoredForm,
            CliSemanticStatus.Attention,
            ".agents/a.md",
            "First tied cause.");
        var secondTie = new RouteListFilesystemFinding(
            RouteListFindingCode.AuthoredForm,
            CliSemanticStatus.Attention,
            ".agents/a.md",
            "Second tied cause.");
        var neutral = NeutralInputs([]);
        var facts = RouteListInventoryFacts.Create(
            neutral.SourceCatalogue,
            neutral.ProjectionBuildResult,
            [],
            [
                RouteListFilesystemFindingPolicy.MetadataMalformed(".agents/a.md"),
                firstTie,
                RouteListFilesystemFindingPolicy.MetadataMissing(".agents/z.md"),
                secondTie,
            ],
            []);

        Assert.Equal(
            [
                "route-list.authored-form:First tied cause.",
                "route-list.authored-form:Second tied cause.",
                "route-list.metadata-malformed:The source frontmatter or metadata shape is malformed.",
                "route-list.metadata-missing:Required source metadata is missing.",
            ],
            facts.Findings.Select(finding => $"{finding.MachineCode}:{finding.Cause}"));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list inventory state applies interruption, blocked, and incomplete precedence")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void MixedFindingStateUsesAcceptedPrecedence()
    {
        var emptySources = Array.Empty<RouteListInventorySource>();
        var attentionNeutral = NeutralInputs(emptySources);
        var attention = RouteListInventoryFacts.Create(
            attentionNeutral.SourceCatalogue,
            attentionNeutral.ProjectionBuildResult,
            emptySources,
            [RouteListFilesystemFindingPolicy.OrphanOverwrite(".agents/orphan.overwrite.md")],
            []);
        var incompleteNeutral = NeutralInputs(emptySources);
        var incomplete = RouteListInventoryFacts.Create(
            incompleteNeutral.SourceCatalogue,
            incompleteNeutral.ProjectionBuildResult,
            emptySources,
            [
                RouteListFilesystemFindingPolicy.OrphanOverwrite(".agents/orphan.overwrite.md"),
                new RouteListFilesystemFinding(
                    RouteListFindingCode.ReadUnavailable,
                    CliSemanticStatus.Incomplete,
                    ".agents/missing.md",
                    "The source metadata could not be read."),
            ],
            []);
        var blockedNeutral = NeutralInputs(emptySources);
        var blocked = RouteListInventoryFacts.Create(
            blockedNeutral.SourceCatalogue,
            blockedNeutral.ProjectionBuildResult,
            emptySources,
            [
                RouteListFilesystemFindingPolicy.MetadataMissing(".agents/missing.md"),
                PhysicalBoundary(".agents/external"),
            ],
            []);
        var interruptedNeutral = NeutralInputs(emptySources);
        var interrupted = RouteListInventoryFacts.Create(
            interruptedNeutral.SourceCatalogue,
            interruptedNeutral.ProjectionBuildResult,
            emptySources,
            [
                PhysicalBoundary(".agents/external"),
                RouteListFilesystemFindingPolicy.Interrupted(".agents/next"),
            ],
            []);

        Assert.Equal(RouteListInventoryState.Complete, attention.State);
        Assert.Equal(RouteListInventoryState.Incomplete, incomplete.State);
        Assert.Equal(RouteListInventoryState.Blocked, blocked.State);
        Assert.Equal(RouteListInventoryState.Interrupted, interrupted.State);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list interruption factory retains known safe sources, findings, and aliases")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void InterruptionFactoryRetainsKnownEvidence()
    {
        var source = Source("safe", ".agents/safe.md", "safe.md");
        var known = RouteListFilesystemFindingPolicy.MetadataMissing(".agents/known.md");
        var alias = Alias(".agents/safe-alias.md", "safe.md", ".agents/safe.md");
        var sources = new[] { source };
        var neutral = NeutralInputs(sources);

        var facts = RouteListInventoryFacts.Interrupted(
            neutral.SourceCatalogue,
            neutral.ProjectionBuildResult,
            sources,
            [known],
            [alias],
            RouteListFilesystemFindingPolicy.Interrupted(".agents/next"));

        Assert.Equal(RouteListInventoryState.Interrupted, facts.State);
        Assert.Same(neutral.SourceCatalogue, facts.SourceCatalogue);
        Assert.Same(neutral.ProjectionBuildResult, facts.ProjectionBuildResult);
        Assert.Same(source, Assert.Single(facts.Sources));
        Assert.Contains(known, facts.Findings);
        Assert.Contains(facts.Findings, finding => finding.Code == RouteListFindingCode.Interrupted);
        Assert.Same(alias, Assert.Single(facts.PhysicalAliases));
        var emptySources = Array.Empty<RouteListInventorySource>();
        var invalidNeutral = NeutralInputs(emptySources);
        Assert.Throws<ArgumentException>(() => RouteListInventoryFacts.Interrupted(
            invalidNeutral.SourceCatalogue,
            invalidNeutral.ProjectionBuildResult,
            emptySources,
            [],
            [],
            known));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list repeated contained identity remains an alias fact rather than a cycle")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void FiniteRepeatedIdentityIsAnAttentionAlias()
    {
        var alias = Alias(".agents/alias.md", "target.md", ".agents/target.md");
        var neutral = NeutralInputs([]);
        var facts = RouteListInventoryFacts.Create(
            neutral.SourceCatalogue,
            neutral.ProjectionBuildResult,
            [],
            [RouteListFilesystemFindingPolicy.IdentityCollision(".agents/alias.md")],
            [alias]);

        Assert.Equal(RouteListInventoryState.Complete, facts.State);
        Assert.Equal(RouteListFindingCode.IdentityCollision, Assert.Single(facts.Findings).Code);
        Assert.Equal(".agents/target.md", Assert.Single(facts.PhysicalAliases).FirstCanonicalLogicalPath);
        Assert.DoesNotContain(facts.Findings, finding => finding.Code == RouteListFindingCode.PhysicalBoundary);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list inventory facts reject duplicate logical sources and aliases")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DuplicateLogicalFactsAreRejected()
    {
        var source = Source("one", ".agents/one.md", "one.md");
        var alias = Alias(".agents/alias.md", "one.md", ".agents/one.md");
        var sourceNeutral = NeutralInputs([source]);
        var aliasNeutral = NeutralInputs([]);

        Assert.Throws<ArgumentException>(() => RouteListInventoryFacts.Create(
            sourceNeutral.SourceCatalogue,
            sourceNeutral.ProjectionBuildResult,
            [source, source],
            [],
            []));
        Assert.Throws<ArgumentException>(() => RouteListInventoryFacts.Create(
            aliasNeutral.SourceCatalogue,
            aliasNeutral.ProjectionBuildResult,
            [],
            [],
            [alias, alias]));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route-list inventory file facts require absolute normalized physical paths")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void InventoryFileFactRejectsUnprovedPhysicalPathShapes()
    {
        var nonNormalized = Path.Combine(Path.GetTempPath(), "route-list", "..", "file.md");

        Assert.Throws<ArgumentException>(() => new RouteSourceDocument(
            ".agents/file.md",
            "relative.md",
            SourceDocumentForm.Markdown,
            FileReadState.Complete,
            "body"));
        Assert.Throws<ArgumentException>(() => new RouteSourceDocument(
            ".agents/file.md",
            nonNormalized,
            SourceDocumentForm.Markdown,
            FileReadState.Complete,
            "body"));
    }

    private static (
        SourceCatalogue SourceCatalogue,
        RouteSourceProjectionBuildResult ProjectionBuildResult) NeutralInputs(
        IReadOnlyList<RouteListInventorySource> sources,
        IEnumerable<RouteOverwriteFact>? overwriteFacts = null,
        bool isCancelled = false)
    {
        var logicalSources = sources
            .Select(source => LogicalSource(source.Source))
            .ToArray();
        var projections = sources
            .Select((source, index) => Projection(source.Source, logicalSources[index]))
            .ToArray();
        var suppliedOverwriteFacts = (overwriteFacts ?? [])
            .Where(fact => fact.State != RouteOverwriteState.Paired)
            .ToArray();
        var pairedOverwriteFacts = sources
            .Where(source => source.Source.Overwrite is not null)
            .Select(source =>
            {
                var overwrite = source.Source.Overwrite
                    ?? throw new InvalidOperationException("The fixed source must retain its overwrite layer.");
                return new RouteOverwriteFact(
                    RouteOverwriteState.Paired,
                    overwrite,
                    [source.Source.CanonicalPath]);
            })
            .ToArray();
        var allOverwriteFacts = suppliedOverwriteFacts
            .Concat(pairedOverwriteFacts)
            .ToArray();

        var candidates = new List<SourceCandidate>();
        foreach (var logicalSource in logicalSources)
        {
            candidates.Add(Candidate(logicalSource.Base, logicalSource.Identity.AutomaticId));
            if (logicalSource.Overwrite is not null)
            {
                candidates.Add(Candidate(logicalSource.Overwrite, logicalSource.Identity.AutomaticId));
            }
        }

        candidates.AddRange(suppliedOverwriteFacts.Select(fact => Candidate(
            NeutralLayer(fact.Overwrite),
            automaticId: null)));

        var sourceCatalogue = new SourceCatalogue(
            CatalogueWorkspace(),
            candidates,
            logicalSources,
            [],
            isCancelled);
        var projectionSet = new RouteSourceProjectionSet(projections, allOverwriteFacts);
        var projectionBuildResult = new RouteSourceProjectionBuildResult(
            projectionSet,
            projections,
            suppliedOverwriteFacts.Select(fact => Read(
                NeutralLayer(fact.Overwrite),
                fact.Overwrite.Body ?? "overwrite")),
            isCancelled);
        return (sourceCatalogue, projectionBuildResult);
    }

    private static SourceLogicalSource LogicalSource(RouteSource source)
    {
        return new SourceLogicalSource(
            new SourceLogicalIdentity(source.Id, source.CanonicalPath),
            NeutralLayer(source.Base),
            source.Overwrite is null ? null : NeutralLayer(source.Overwrite));
    }

    private static RouteSourceProjection Projection(
        RouteSource source,
        SourceLogicalSource logicalSource)
    {
        var baseRead = Read(logicalSource.Base, source.Base.Body ?? "body");
        var overwrite = source.Overwrite;
        var overwriteRead = overwrite is null
            ? null
            : Read(
                logicalSource.Overwrite
                    ?? throw new InvalidOperationException("The fixed logical source must retain its overwrite layer."),
                overwrite.Body ?? "overwrite");
        return new RouteSourceProjection(logicalSource, source, baseRead, overwriteRead);
    }

    private static SourceDocumentReadResult Read(SourceLayer layer, string body)
    {
        return new SourceDocumentReadResult(
            layer,
            new SourceLayerVerification(
                layer,
                SourceLayerVerificationState.Verified,
                layer.PhysicalPath,
                null),
            FileReadResult<string>.Complete(layer.CanonicalPath, body));
    }

    private static SourceCandidate Candidate(SourceLayer layer, string? automaticId)
    {
        var physicalParentPath = Path.GetDirectoryName(layer.PhysicalPath)
            ?? throw new InvalidOperationException("The fixed physical path must have a parent directory.");
        return new SourceCandidate(
            layer.CanonicalPath,
            layer.Form,
            automaticId,
            PhysicalPathState.Contained,
            layer.PhysicalPath,
            physicalParentPath);
    }

    private static SourceLayer NeutralLayer(RouteSourceDocument document)
    {
        return new SourceLayer(
            document.CanonicalLogicalPath,
            document.PhysicalPath,
            document.Form,
            document.Form == SourceDocumentForm.OverwriteCompanion
                ? SourceLayerKind.Overwrite
                : SourceLayerKind.Base);
    }

    private static CliWorkspace CatalogueWorkspace()
    {
        var agentsPath = Path.GetDirectoryName(
            RouteSourceTestData.PhysicalPath(".agents/placeholder.md"))
            ?? throw new InvalidOperationException("The fixed agents path must have a parent directory.");
        var root = Path.GetDirectoryName(agentsPath)
            ?? throw new InvalidOperationException("The fixed workspace path must have a parent directory.");
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static RouteListInventorySource Source(
        string id,
        string canonicalPath,
        string physicalName,
        RouteListSourceKind kind = RouteListSourceKind.RoutedLeaf,
        SourceDocumentForm form = SourceDocumentForm.Markdown)
    {
        var sourceKind = kind switch
        {
            RouteListSourceKind.Loader => RouteSourceKind.Loader,
            RouteListSourceKind.Entrypoint => RouteSourceKind.Entrypoint,
            RouteListSourceKind.RoutedLeaf => RouteSourceKind.Markdown,
            RouteListSourceKind.RoutedNative => RouteSourceKind.Native,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The test source kind is not routable."),
        };
        var selectionSource = RouteSourceTestData.Source(
            canonicalPath,
            sourceKind,
            form: form);
        return new RouteListInventorySource(selectionSource);
    }

    private static RouteListPhysicalAlias Alias(string path, string physicalName, string firstPath)
    {
        return new RouteListPhysicalAlias(path, Physical(physicalName), firstPath);
    }

    private static RouteListFilesystemFinding PhysicalBoundary(string subject)
    {
        return new RouteListFilesystemFinding(
            RouteListFindingCode.PhysicalBoundary,
            CliSemanticStatus.Blocked,
            subject,
            "The physical boundary is blocked.");
    }

    private static CliWorkspace Workspace()
    {
        var root = Physical("workspace");
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static string Physical(string name)
    {
        return Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-route-list-unit", name));
    }
}
