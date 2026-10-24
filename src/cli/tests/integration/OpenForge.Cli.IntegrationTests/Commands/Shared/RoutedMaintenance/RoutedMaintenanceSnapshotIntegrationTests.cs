using System.Collections.Concurrent;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.RoutedMaintenance;

[Trait("Feature", "metadata-completion"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RoutedMaintenanceSnapshotIntegrationTests
{
    [Fact(DisplayName = "Maintenance acquires one exact snapshot per eligible file with bounded concurrent reads")]
    public async Task ReadsEachEligibleSourceOnceWithoutWriting()
    {
        using var temporary = TemporaryWorkspace.Create("maintenance-snapshot-batch");
        const string root = ".agents/guidance/_guidance.md";
        temporary.CreateFile(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Guidance](guidance/_guidance.md)\n");
        temporary.CreateFile(root, "---\ndescription: Guidance\ntags: [Guidance]\n---\n# Guidance\n\n## Entries\n");
        for (var index = 0; index < 32; index++) temporary.CreateFile($".agents/guidance/note-{index}.md", $"# Note {index}\n");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var token = TestContext.Current.CancellationToken;
        var catalogue = await new SourceCatalogueReader().ReadAsync(new SourceCatalogueRequest(WorkspaceLayout.Create(workspace, WorkspaceLayout.DefaultMainRoot), [".agents"]), token);
        var reads = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        var components = new Dictionary<string, int>(StringComparer.Ordinal);
        var paths = new PhysicalPathResolver(path =>
        {
            components[path] = components.GetValueOrDefault(path) + 1;
            return LinkTargetReader.Read(path);
        });
        var active = 0;
        var snapshots = new SourceDocumentSnapshotReader();
        var planner = new RoutedMaintenancePlanner(paths, readSnapshot: async (selected, verification, cancellation) =>
        {
            reads.AddOrUpdate(verification.Layer.CanonicalPath, 1, static (_, count) => count + 1);
            var concurrent = Interlocked.Increment(ref active);
            try
            {
                Assert.InRange(concurrent, 1, RoutedMaintenanceSnapshotReader.MaximumConcurrentReads);
                return await snapshots.ReadAsync(selected, verification, cancellation);
            }
            finally { Interlocked.Decrement(ref active); }
        });
        var before = temporary.SnapshotHashes();

        var intent = await planner.PrepareAsync(new()
        {
            Catalogue = catalogue,
            RootPaths = [root],
            SelectedSources = [.. catalogue.Sources],
            Policy = new() { Layout = catalogue.Layout, Settings = WorkspaceSettingsDocument.Empty, Ownership = WorkspaceOwnershipDocument.Empty },
        }, token);

        Assert.Equal(33, reads.Count);
        Assert.All(reads.Values, count => Assert.Equal(1, count));
        Assert.Equal(33, intent.Originals.Count);
        // One ancestor observation and one fresh leaf observation for the Loader's parent.
        Assert.Equal(2, components[temporary.Combine(".agents")]);
        Assert.Equal(1, components[temporary.Combine(".agents/guidance/note-0.md")]);
        Assert.Equal(0, active);
        Assert.Equal(before, temporary.SnapshotHashes());

        reads.Clear();
        var reused = await planner.PrepareAsync(new()
        {
            Catalogue = catalogue,
            RootPaths = [root],
            SelectedSources = [.. catalogue.Sources],
            Policy = new() { Layout = catalogue.Layout, Settings = WorkspaceSettingsDocument.Empty, Ownership = WorkspaceOwnershipDocument.Empty },
            ObservedDocuments = intent.Documents,
        }, token);
        Assert.Equal(33, reads.Count);
        Assert.All(reads.Values, count => Assert.Equal(1, count));
        Assert.Equal(4, components[temporary.Combine(".agents")]);
        Assert.Equal(2, components[temporary.Combine(".agents/guidance/note-0.md")]);
        Assert.Equal(intent.Originals.Keys.Order(StringComparer.Ordinal), reused.Originals.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(before, temporary.SnapshotHashes());

        var snapshot = reused.Originals[".agents/guidance/note-0.md"];
        File.AppendAllText(snapshot.LogicalPath, "Changed after diagnosis.\n");
        var validation = await new FileExpectationValidator(new PhysicalPathResolver()).ValidateAsync(workspace, snapshot.Expectation, token);
        Assert.NotEqual(FileExpectationValidationState.Matched, validation.State);
    }
}
