using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.UnitTests.Commands.Shared.RouteScaffolding;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared.Planning;

[Trait("Feature", "index-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class IndexCompletionPlanningTests
{
    [Fact(DisplayName = "Index assembles final entrypoint creates before one coalesced original-byte replacement")]
    public void CoalescesMetadataAndEntries()
    {
        var request = RouteScaffoldChainTests.Request([".agents/guidance/_guidance.md"], [".agents/guidance/team/note.md"]);
        var workspace = request.Catalogue.Workspace;
        var root = request.Catalogue.FindByPath(".agents/guidance/_guidance.md") ?? throw new InvalidOperationException();
        var child = request.Catalogue.FindByPath(".agents/guidance/team/note.md") ?? throw new InvalidOperationException();
        var sourceBuilder = new RouteScaffoldSourceBuilder();
        var created = sourceBuilder.Create(request.Catalogue.Layout, ".agents/guidance/team/_team.md");
        var original = Encoding.UTF8.GetBytes("---\ndescription: Keep this # comment\n---\n\n# Guidance\n\n## Entries\n\n- stale\n\n## Authored\n\nKeep this too.\n");
        var completion = new FrameworkMetadataCompleter().Complete(new()
        {
            CanonicalPath = root.Identity.CanonicalBasePath,
            Bytes = [.. original],
            DefaultTags = ["Guidance"],
            Form = FrontmatterForm.Scoped,
        });
        var scaffold = new RouteScaffoldComposer().Compose(RouteScaffoldComposer.FolderDocument("guidance/team", "Team", ["Guidance"], FrontmatterForm.Scoped));
        var parser = new MarkdownDocumentParser();
        var documents = new Dictionary<string, OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownDocumentFacts>
        {
            [root.Identity.CanonicalBasePath] = parser.Parse(Encoding.UTF8.GetString(completion.Bytes.AsSpan())),
            [created.Identity.CanonicalBasePath] = parser.Parse(Encoding.UTF8.GetString(scaffold.AsSpan())),
            [child.Identity.CanonicalBasePath] = parser.Parse("---\ndescription: Note\ntags: [Guidance]\n---\n\n# Note\n"),
        };
        var formation = sourceBuilder.Add(request.Catalogue, [created]);
        var projection = new GeneratedNavigationProjector().Project(new(formation,
            new[] { root, created }.Select(source => new GeneratedNavigationRegionInput(source, documents[source.Identity.CanonicalBasePath])),
            formation.Sources.Select(source => new GeneratedNavigationMetadata(source, new SourceAuthoredMetadataParser().Parse(documents[source.Identity.CanonicalBasePath], source.Base.Form)))));
        var before = FileStateSnapshot.File(root.Base.PhysicalPath, root.Base.PhysicalPath, original);
        var intent = new RoutedMaintenanceIntent
        {
            Formation = formation,
            Documents = documents,
            Originals = new Dictionary<string, FileStateSnapshot> { [root.Identity.CanonicalBasePath] = before },
            Metadata = [new(root.Identity.CanonicalBasePath, completion)],
            Entrypoints = [new() { Source = created, Before = FileStateSnapshot.Missing(created.Base.PhysicalPath), IntendedBytes = scaffold }],
            Limitations = [],
            OldNavigation = [],
            RemovedPaths = [],
        };
        var effects = RoutedMaintenanceEffectAssembler.Assemble(workspace, intent, projection);
        Assert.Equal(2, effects.Changes.Length);
        Assert.Equal(PlannedFileChangeKind.Create, effects.Changes[0].Kind);
        Assert.Equal(PlannedFileChangeKind.Replace, effects.Changes[1].Kind);
        Assert.Equal(2, effects.Changes.Select(change => change.LogicalPath).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(original, Assert.Single(effects.RecoveryTargets).Before.Bytes);
        Assert.Contains("[Note](note.md) - #Guidance", Encoding.UTF8.GetString(effects.Changes[0].IntendedBytes.AsSpan()), StringComparison.Ordinal);
        var final = Encoding.UTF8.GetString(effects.Changes[1].IntendedBytes.AsSpan());
        Assert.Contains("description: Keep this # comment", final, StringComparison.Ordinal);
        Assert.Contains("[Notes in guidance/team](team/_team.md) - #Guidance", final, StringComparison.Ordinal);
        Assert.EndsWith("## Authored\n\nKeep this too.\n", final, StringComparison.Ordinal);
    }
}
