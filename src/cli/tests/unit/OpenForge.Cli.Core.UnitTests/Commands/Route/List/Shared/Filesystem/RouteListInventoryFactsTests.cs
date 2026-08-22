using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Filesystem;

public sealed class RouteListInventoryFactsTests
{
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
            RouteSourceForm.CanonicalEntrypoint);
        var sources = new List<RouteListInventorySource> { second, first };
        var findings = new List<RouteListFilesystemFinding>
        {
            RouteListFilesystemFindingPolicy.MetadataMissing(".agents/z.md"),
            RouteListFilesystemFindingPolicy.OrphanOverwrite(".agents/a.overwrite.md"),
        };
        var aliases = new List<RouteListPhysicalAlias>
        {
            Alias(".agents/z.md", "physical-two.md", ".agents/a.md"),
        };

        var facts = RouteListInventoryFacts.Create(sources, findings, aliases);
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
        Assert.Equal(2, facts.Catalogue.FindById("collision").Count);
        Assert.Same(first.Source, facts.Catalogue.FindByPath(".agents/collision.md"));
        Assert.Equal(RouteListInventoryState.Incomplete, facts.State);
    }

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
        var facts = RouteListInventoryFacts.Create(
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

    [Fact(DisplayName = "Route-list inventory state applies interruption, blocked, and incomplete precedence")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void MixedFindingStateUsesAcceptedPrecedence()
    {
        var attention = RouteListInventoryFacts.Create(
            [],
            [RouteListFilesystemFindingPolicy.OrphanOverwrite(".agents/orphan.overwrite.md")],
            []);
        var incomplete = RouteListInventoryFacts.Create(
            [],
            [
                RouteListFilesystemFindingPolicy.OrphanOverwrite(".agents/orphan.overwrite.md"),
                RouteListFilesystemFindingPolicy.MetadataMissing(".agents/missing.md"),
            ],
            []);
        var blocked = RouteListInventoryFacts.Create(
            [],
            [
                RouteListFilesystemFindingPolicy.MetadataMissing(".agents/missing.md"),
                PhysicalBoundary(".agents/external"),
            ],
            []);
        var interrupted = RouteListInventoryFacts.Create(
            [],
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

    [Fact(DisplayName = "Route-list interruption factory retains known safe sources, findings, and aliases")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void InterruptionFactoryRetainsKnownEvidence()
    {
        var source = Source("safe", ".agents/safe.md", "safe.md");
        var known = RouteListFilesystemFindingPolicy.MetadataMissing(".agents/known.md");
        var alias = Alias(".agents/safe-alias.md", "safe.md", ".agents/safe.md");

        var facts = RouteListInventoryFacts.Interrupted(
            [source],
            [known],
            [alias],
            RouteListFilesystemFindingPolicy.Interrupted(".agents/next"));

        Assert.Equal(RouteListInventoryState.Interrupted, facts.State);
        Assert.Same(source, Assert.Single(facts.Sources));
        Assert.Contains(known, facts.Findings);
        Assert.Contains(facts.Findings, finding => finding.Code == RouteListFindingCode.Interrupted);
        Assert.Same(alias, Assert.Single(facts.PhysicalAliases));
        Assert.Throws<ArgumentException>(() => RouteListInventoryFacts.Interrupted(
            [],
            [],
            [],
            known));
    }

    [Fact(DisplayName = "Route-list repeated contained identity remains an alias fact rather than a cycle")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void FiniteRepeatedIdentityIsAnAttentionAlias()
    {
        var alias = Alias(".agents/alias.md", "target.md", ".agents/target.md");
        var facts = RouteListInventoryFacts.Create(
            [],
            [RouteListFilesystemFindingPolicy.IdentityCollision(".agents/alias.md")],
            [alias]);

        Assert.Equal(RouteListInventoryState.Complete, facts.State);
        Assert.Equal(RouteListFindingCode.IdentityCollision, Assert.Single(facts.Findings).Code);
        Assert.Equal(".agents/target.md", Assert.Single(facts.PhysicalAliases).FirstCanonicalLogicalPath);
        Assert.DoesNotContain(facts.Findings, finding => finding.Code == RouteListFindingCode.PhysicalBoundary);
    }

    [Fact(DisplayName = "Route-list inventory facts reject duplicate logical sources and aliases")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DuplicateLogicalFactsAreRejected()
    {
        var source = Source("one", ".agents/one.md", "one.md");
        var alias = Alias(".agents/alias.md", "one.md", ".agents/one.md");

        Assert.Throws<ArgumentException>(() => RouteListInventoryFacts.Create([source, source], [], []));
        Assert.Throws<ArgumentException>(() => RouteListInventoryFacts.Create([], [], [alias, alias]));
    }

    [Fact(DisplayName = "Route-list inventory file facts require absolute normalized physical paths")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void InventoryFileFactRejectsUnprovedPhysicalPathShapes()
    {
        var nonNormalized = Path.Combine(Path.GetTempPath(), "route-list", "..", "file.md");

        Assert.Throws<ArgumentException>(() => new RouteSourceDocument(
            ".agents/file.md",
            "relative.md",
            RouteSourceForm.Markdown,
            FileReadState.Complete,
            "body"));
        Assert.Throws<ArgumentException>(() => new RouteSourceDocument(
            ".agents/file.md",
            nonNormalized,
            RouteSourceForm.Markdown,
            FileReadState.Complete,
            "body"));
    }

    private static RouteListInventorySource Source(
        string id,
        string canonicalPath,
        string physicalName,
        RouteListSourceKind kind = RouteListSourceKind.RoutedLeaf,
        RouteSourceForm form = RouteSourceForm.Markdown)
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
