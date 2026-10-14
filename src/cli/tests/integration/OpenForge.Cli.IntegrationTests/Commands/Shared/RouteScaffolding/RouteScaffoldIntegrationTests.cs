using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Discovery;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Rename;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Sharing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.RouteScaffolding;

[Trait("Feature", "route-scaffolding"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteScaffoldIntegrationTests
{
    [Theory(DisplayName = "Deep real folders form one final Create per gap and bounded existing navigation changes")]
    [InlineData((int)FrontmatterForm.Scoped)]
    [InlineData((int)FrontmatterForm.Root)]
    public async Task DeepChainHasFinalProjectedCreates(int formValue)
    {
        using var temporary = TemporaryWorkspace.Create("scaffold-deep");
        var form = (FrontmatterForm)formValue;
        Seed(temporary, form);
        WriteScaffold(temporary, "memory/crystallized", "Crystallized", ["Memory", "Crystallized"], form, "index.md");
        temporary.CreateFile(".agents/memory/crystallized/platform/payments/stripe/node.md", Note);
        temporary.CreateDirectory(".agents/memory/resources/empty");
        temporary.CreateFile(".agents/memory/resources/asset.txt", "resource");
        var request = await ReadRequestAsync(temporary);
        var discovery = new RouteScaffoldChainDiscovery().Discover(request);
        Assert.False(discovery.IsBlocked);
        Assert.Equal(["memory/crystallized/platform", "memory/crystallized/platform/payments", "memory/crystallized/platform/payments/stripe"],
            discovery.Gaps.Select(gap => gap.FolderId));

        var builder = new RouteScaffoldSourceBuilder();
        var composer = new RouteScaffoldComposer();
        var intents = discovery.Gaps.Select(gap => new RouteScaffoldEntrypointIntent
        {
            Source = builder.Create(request.Catalogue.Workspace, gap.CanonicalPath),
            Before = FileStateSnapshot.Missing(temporary.Combine(gap.CanonicalPath)),
            IntendedBytes = composer.Compose(RouteScaffoldComposer.FolderDocument(
                gap.FolderId,
                Title(gap.FolderId),
                ["Memory", "Crystallized"], form)),
        }).ToArray();
        var formation = builder.Add(request.Catalogue, intents.Select(intent => intent.Source));
        var documents = await ReadDocumentsAsync(request.Catalogue);
        foreach (var intent in intents)
            documents.Add(intent.Source.Identity.CanonicalBasePath, new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(intent.IntendedBytes.AsSpan())));
        var metadata = formation.Sources.Where(source => source.Base.Form != SourceDocumentForm.Loader)
            .Select(source => new GeneratedNavigationMetadata(source, new SourceAuthoredMetadataParser().Parse(
                documents[source.Identity.CanonicalBasePath], source.Base.Form))).ToArray();
        var regions = formation.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form))
            .Select(source => new GeneratedNavigationRegionInput(source, documents[source.Identity.CanonicalBasePath]));
        var projection = new GeneratedNavigationProjector().Project(new GeneratedNavigationProjectionRequest(formation, regions, metadata));
        Assert.True(projection.IsComplete);
        Assert.NotNull(formation.Topology.ReadAbsoluteDepth(".agents/memory/crystallized/platform/payments/stripe/node.md"));
        var changes = intents.SelectMany(intent => new RouteScaffoldRenamePlanner().Complete(
            intent, projection.Regions.Single(region => ReferenceEquals(region.Source, intent.Source))).Changes).ToArray();
        Assert.Equal(3, changes.Length);
        Assert.All(changes, change => Assert.Equal(PlannedFileChangeKind.Create, change.Kind));
        Assert.Equal(changes.Length, changes.Select(change => change.LogicalPath).Distinct(StringComparer.Ordinal).Count());
        Assert.Contains("[Notes in memory/crystallized/platform/payments](payments/_payments.md) - #Memory #Crystallized",
            Encoding.UTF8.GetString(changes[0].IntendedBytes.AsSpan()), StringComparison.Ordinal);
        Assert.Contains("[Node](node.md) - #Memory #Crystallized", Encoding.UTF8.GetString(changes[^1].IntendedBytes.AsSpan()), StringComparison.Ordinal);
        Assert.True(projection.Regions.Single(region => region.CanonicalPath == ".agents/memory/crystallized/index.md").Change?.RequiresUpdate);
        Assert.DoesNotContain(changes, change => change.LogicalPath.EndsWith("_crystallized.md", StringComparison.Ordinal));
        Assert.All(changes, change => Assert.False(File.Exists(change.LogicalPath)));
        Assert.Equal(Note, File.ReadAllText(temporary.Combine(".agents/memory/crystallized/platform/payments/stripe/node.md")));
    }

    [Fact(DisplayName = "Real catalogue discovery preserves compatibility and blocks duplicate and occupied identities")]
    public async Task RealIdentityBoundaries()
    {
        using var temporary = TemporaryWorkspace.Create("scaffold-identities");
        Seed(temporary, FrontmatterForm.Scoped);
        WriteScaffold(temporary, "memory/team", "Team", ["Memory"], FrontmatterForm.Scoped, "references.md");
        temporary.CreateFile(".agents/memory/team/deep/note.md", Note);
        var request = await ReadRequestAsync(temporary);
        Assert.Equal("memory/team/deep", Assert.Single(new RouteScaffoldChainDiscovery().Discover(request).Gaps).FolderId);
        WriteScaffold(temporary, "memory/team", "Team", ["Memory"], FrontmatterForm.Scoped);
        request = await ReadRequestAsync(temporary);
        Assert.Contains(new RouteScaffoldChainDiscovery().Discover(request).Boundaries,
            boundary => boundary.Kind == RouteScaffoldBoundaryKind.EntrypointAmbiguous);

        temporary.CreateFile(".agents/memory/collision.md", Note);
        temporary.CreateFile(".agents/memory/collision/deep/note.md", Note);
        request = await ReadRequestAsync(temporary);
        Assert.Contains(new RouteScaffoldChainDiscovery().Discover(request).Boundaries,
            boundary => boundary.Kind == RouteScaffoldBoundaryKind.IdentityOccupied);
        temporary.CreateDirectory(".agents/memory/occupied/_occupied.md");
        temporary.CreateFile(".agents/memory/occupied/note.md", Note);
        request = await ReadRequestAsync(temporary);
        request = request with { OccupiedPaths = [".agents/memory/occupied/_occupied.md"] };
        Assert.Contains(new RouteScaffoldChainDiscovery().Discover(request).Boundaries,
            boundary => boundary.Kind == RouteScaffoldBoundaryKind.PathOccupied);
    }

    [Fact(DisplayName = "Real folder discovery excludes unknown roots removals and native package resources")]
    public async Task RealPolicyBoundaries()
    {
        using var temporary = TemporaryWorkspace.Create("scaffold-policies");
        Seed(temporary, FrontmatterForm.Scoped);
        temporary.CreateFile(".agents/unknown/deep/note.md", Note);
        temporary.CreateFile(".agents/memory/removed/deep/note.md", Note);
        temporary.CreateFile(".agents/memory/tool/SKILL.md", "---\nname: tool\ndescription: Tool\n---\n\n# Tool\n");
        temporary.CreateFile(".agents/memory/tool/references/deep/note.md", Note);
        temporary.CreateFile(".agents/memory/orphan/deep/note.overwrite.md", Note);
        var request = await ReadRequestAsync(temporary);
        request = request with { Settings = WorkspaceSettingsDocument.Empty with { RemovedDirectories = [".agents/memory/removed"] } };
        Assert.Empty(new RouteScaffoldChainDiscovery().Discover(request).Gaps);
        request = request with { Settings = WorkspaceSettingsDocument.Empty with { RemovedCategories = ["memory"] } };
        Assert.Empty(new RouteScaffoldChainDiscovery().Discover(request).Gaps);
        request = request with
        {
            Sharing = new SourceSharing([new SourceSharingRoute(".agents/memory/removed", ".agents/memory/removed/index.md")]),
        };
        Assert.Contains(new RouteScaffoldChainDiscovery().Discover(request).Boundaries,
            boundary => boundary.Kind == RouteScaffoldBoundaryKind.SharedEntrypointMissing);
    }

    [Theory(DisplayName = "Proven folder rename preserves authored bytes and plans one recovered Delete beside final Create")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProvenRenamePreservesAuthoredBytes(bool proven)
    {
        using var temporary = TemporaryWorkspace.Create("scaffold-rename");
        Seed(temporary, FrontmatterForm.Scoped);
        const string oldText = "---\r\nopen-forge:\r\n  description: Billing rules\r\n  tags: [Guidance]\r\n---\r\n\r\n# Billing\r\n\r\n## Axioms\r\n\r\n- Keep the rounding rules.\r\n\r\n## Entries\r\n\r\n- none - No entries - #Empty\r\n\r\n## Authored\r\n\r\nKeep this exact.\r\n";
        var oldBytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(oldText)).ToArray();
        var oldPhysical = temporary.CreateFile(".agents/memory/invoices/_billing.md", oldBytes);
        temporary.CreateFile(".agents/memory/invoices/node.md", Note);
        var request = await ReadRequestAsync(temporary);
        var builder = new RouteScaffoldSourceBuilder();
        var oldSource = request.Catalogue.FindByPath(".agents/memory/invoices/_billing.md") ?? throw new InvalidOperationException();
        var destination = builder.Create(request.Catalogue.Workspace, ".agents/memory/invoices/_invoices.md");
        var candidate = new RouteScaffoldRenameCandidate
        {
            Destination = destination,
            DestinationBefore = FileStateSnapshot.Missing(temporary.Combine(destination.Identity.CanonicalBasePath)),
            ScaffoldBytes = new RouteScaffoldComposer().Compose(RouteScaffoldComposer.FolderDocument("memory/invoices", "Invoices", ["Memory"], FrontmatterForm.Scoped)),
            OldSource = oldSource,
            OldSnapshot = FileStateSnapshot.File(oldPhysical, oldPhysical, oldBytes),
        };
        var proof = new RouteScaffoldRenameProof
        {
            OldCanonicalPath = ".agents/memory/billing/_billing.md",
            CurrentCanonicalPath = oldSource.Identity.CanonicalBasePath,
            CurrentContentHash = FileExpectation.Hash(oldBytes),
            Basis = RouteScaffoldRenameBasis.CommittedContent,
        };
        var planner = new RouteScaffoldRenamePlanner();
        var intent = planner.Decide(candidate, proven ? proof : null);
        var formation = builder.Replace(request.Catalogue, [destination], proven ? [oldSource.Identity.CanonicalBasePath] : []);
        var documents = await ReadDocumentsAsync(request.Catalogue);
        documents[destination.Identity.CanonicalBasePath] = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(intent.IntendedBytes.AsSpan()));
        var metadata = formation.Sources.Where(source => source.Base.Form != SourceDocumentForm.Loader)
            .Select(source => new GeneratedNavigationMetadata(source, new SourceAuthoredMetadataParser().Parse(documents[source.Identity.CanonicalBasePath], source.Base.Form)));
        var projection = new GeneratedNavigationProjector().Project(new GeneratedNavigationProjectionRequest(formation,
            [new GeneratedNavigationRegionInput(destination, documents[destination.Identity.CanonicalBasePath])], metadata));
        var region = Assert.Single(projection.Regions);
        var plan = planner.Complete(intent, region);
        Assert.Equal(proven ? 2 : 1, plan.Changes.Length);
        Assert.Equal(PlannedFileChangeKind.Create, plan.Changes[0].Kind);
        if (proven)
        {
            Assert.Equal(PlannedFileChangeKind.Delete, plan.Changes[1].Kind);
            Assert.Single(plan.RecoveryTargets);
            var change = region.Change ?? throw new InvalidOperationException();
            Assert.True(change.BeforeDocumentBytes.AsSpan().SequenceEqual(oldBytes));
            Assert.True(change.PrefixPreserved && change.SuffixPreserved);
            Assert.DoesNotContain("_billing.md", change.ExpectedBody, StringComparison.Ordinal);
        }
        else
        {
            Assert.Empty(plan.RecoveryTargets);
            Assert.NotNull(intent.OldNavigation);
            Assert.Contains("_billing.md", region.ExpectedBody ?? "", StringComparison.Ordinal);
        }
        Assert.Equal(oldBytes, File.ReadAllBytes(oldPhysical));
        Assert.False(File.Exists(destination.Base.PhysicalPath));
    }

    private const string Note = "---\nopen-forge:\n  description: Node\n  tags: [Memory, Crystallized]\n---\n\n# Node\n\nAuthored note.\n";

    private static void Seed(TemporaryWorkspace temporary, FrontmatterForm form)
    {
        temporary.CreateFile(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Memory](memory/_memory.md) - #Memory\n");
        WriteScaffold(temporary, "memory", "Memory", ["Memory"], form);
    }

    private static void WriteScaffold(TemporaryWorkspace temporary, string id, string title, string[] tags, FrontmatterForm form, string? fileName = null)
    {
        var path = fileName is null ? RouteScaffoldSourceBuilder.CanonicalPath(id) : $".agents/{id}/{fileName}";
        temporary.CreateFile(path, new RouteScaffoldComposer().Compose(RouteScaffoldComposer.FolderDocument(id, title, tags, form)).ToArray());
    }

    private static string Title(string id)
    {
        var name = SourceLogicalPath.ReadFileName($".agents/{id}");
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    private static async Task<RouteScaffoldDiscoveryRequest> ReadRequestAsync(TemporaryWorkspace temporary)
    {
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var catalogue = await new SourceCatalogueReader().ReadAsync(new SourceCatalogueRequest(workspace, [".agents"]), TestContext.Current.CancellationToken);
        return new RouteScaffoldDiscoveryRequest
        {
            Catalogue = catalogue,
            RecognizedRootPaths = [".agents/memory/_memory.md"],
            EligibleSources = catalogue.Sources.ToImmutableArray(),
            Settings = WorkspaceSettingsDocument.Empty,
        };
    }

    private static async Task<Dictionary<string, MarkdownDocumentFacts>> ReadDocumentsAsync(SourceCatalogue catalogue)
    {
        var documents = new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal);
        var reader = new SourceDocumentReader(catalogue.Workspace);
        foreach (var source in catalogue.Sources)
        {
            var read = await reader.ReadAsync(source.Base, TestContext.Current.CancellationToken);
            documents.Add(source.Identity.CanonicalBasePath, new MarkdownDocumentParser().Parse(read.Read?.Value ?? throw new InvalidOperationException()));
        }
        return documents;
    }
}
