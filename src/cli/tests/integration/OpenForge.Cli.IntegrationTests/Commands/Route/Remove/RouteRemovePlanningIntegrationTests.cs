using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Filesystem;
using OpenForge.Cli.Core.Commands.Route.Shared.Navigation;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemovePlanningIntegrationTests
{
    [Fact(DisplayName = "Route Remove category observations retain exact inventory tuples and fresh bytes"), Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task CategoryObservationsRetainExactTuplesAndFreshSnapshots()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-inventory-facts");
        const string category = ".agents/guidance/topics/";
        var documents = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["_topics.md"] = OpenForgeDocumentSeed.Metadata("Topics", ["Route"],
                "# Topics\n\n" + OpenForgeDocumentSeed.GeneratedEntries("- [Child](child.md) - #Route\n- [Nested](nested/_nested.md) - #Route")),
            ["_topics.overwrite.md"] = "# Local topics\n",
            ["assets/settings.json"] = "{\"enabled\":true}\n",
            ["child.md"] = OpenForgeDocumentSeed.Metadata("Child", ["Route"], "# Child\n"),
            ["child.overwrite.md"] = "# Local child\n",
            ["image.bin"] = "resource\n",
            ["native/SKILL.md"] = OpenForgeDocumentSeed.Metadata("Native", ["Route"], "# Native\n"),
            ["nested/_nested.md"] = OpenForgeDocumentSeed.Metadata("Nested", ["Route"],
                "# Nested\n\n" + OpenForgeDocumentSeed.GeneratedEntries("- none - No entries - #Empty")),
            ["notes.md"] = OpenForgeDocumentSeed.Metadata("Notes", ["Route"], "# Notes\n"),
            ["UPPER.MD"] = "# Ordinary resource\n",
        };
        foreach (var (relativePath, contents) in documents)
        {
            workspace.WriteText(category + relativePath, contents);
        }

        workspace.WriteText(category + "empty/marker", "temporary");
        workspace.DeleteFile(category + "empty/marker");
        var before = workspace.SnapshotHashes();
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(
            new RouteRemoveRequest(workspace.Workspace, RouteRemoveIntegrationWorkspace.CategoryId, RouteRemoveMode.DryRun),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteRemovePlan>(build.Plan);
        var subject = plan.Projection.Subject;
        var physical = new PhysicalPathResolver();
        var reader = new RouteCategoryFilesystemReader(physical, new FileExpectationValidator(physical));
        var input = new RouteCategoryFilesystemRequest
        {
            Workspace = subject.Request.Workspace,
            EntrypointLogicalPath = subject.Layers[0].Snapshot.LogicalPath,
            Catalogue = subject.Catalogue,
            EntrypointPaths = [.. subject.Layers.Select(layer => layer.Layer.CanonicalPath)],
            ExposedPaths = subject.NavigationExposure.ExposedPaths,
        };
        var read = await reader.ReadAsync(input, TestContext.Current.CancellationToken);
        Assert.Equal(RouteCategoryFilesystemReadState.Complete, read.State);
        Assert.Null(read.Cause);
        var inventory = read;
        (string Path, RouteCategoryFilesystemItemKind Kind, SourceLayerKind? Layer, string? Id)[] expected =
        [
            (".", RouteCategoryFilesystemItemKind.Directory, null, null),
            ("UPPER.MD", RouteCategoryFilesystemItemKind.Resource, null, null),
            ("_topics.md", RouteCategoryFilesystemItemKind.Entrypoint, SourceLayerKind.Base, "guidance/topics"),
            ("_topics.overwrite.md", RouteCategoryFilesystemItemKind.Entrypoint, SourceLayerKind.Overwrite, "guidance/topics"),
            ("assets", RouteCategoryFilesystemItemKind.Directory, null, null),
            ("assets/settings.json", RouteCategoryFilesystemItemKind.Resource, null, null),
            ("child.md", RouteCategoryFilesystemItemKind.RoutedMarkdown, SourceLayerKind.Base, "guidance/topics/child"),
            ("child.overwrite.md", RouteCategoryFilesystemItemKind.RoutedMarkdown, SourceLayerKind.Overwrite, "guidance/topics/child"),
            ("empty", RouteCategoryFilesystemItemKind.Directory, null, null),
            ("image.bin", RouteCategoryFilesystemItemKind.Resource, null, null),
            ("native", RouteCategoryFilesystemItemKind.Directory, null, null),
            ("native/SKILL.md", RouteCategoryFilesystemItemKind.NativeSource, SourceLayerKind.Base, "guidance/topics/native"),
            ("nested", RouteCategoryFilesystemItemKind.Directory, null, null),
            ("nested/_nested.md", RouteCategoryFilesystemItemKind.RoutedMarkdown, SourceLayerKind.Base, "guidance/topics/nested"),
            ("notes.md", RouteCategoryFilesystemItemKind.UnroutedMarkdown, SourceLayerKind.Base, "guidance/topics/notes"),
        ];
        Assert.Equal(expected, inventory.Items.Select(item => (item.RelativePath, item.Kind, item.Layer, item.SourceId)));
        foreach (var item in inventory.Items)
        {
            var expectedPath = workspace.Combine((category + (item.RelativePath == "." ? "" : item.RelativePath)).TrimEnd('/'));
            Assert.Equal(expectedPath, item.SourcePath);
            Assert.Equal(expectedPath, item.Snapshot.LogicalPath);
            Assert.Equal(expectedPath, item.Snapshot.PhysicalPath);
            if (documents.TryGetValue(item.RelativePath, out var contents))
            {
                Assert.Equal(System.Text.Encoding.UTF8.GetBytes(contents), item.Snapshot.Bytes);
            }
            else
            {
                Assert.Empty(item.Snapshot.Bytes);
            }
        }

        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.WriteText(category + "image.bin", "changed\r\n");
        workspace.WriteText(category + "new.bin", "new\n");
        var changedBefore = workspace.SnapshotHashes();
        var changed = await reader.ReadAsync(input, TestContext.Current.CancellationToken);
        var changedInventory = changed;
        Assert.Equal(expected.Length + 1, changedInventory.Items.Length);
        Assert.Equal("changed\r\n"u8.ToArray(), Assert.Single(changedInventory.Items, item => item.RelativePath == "image.bin").Snapshot.Bytes);
        Assert.Equal("new\n"u8.ToArray(), Assert.Single(changedInventory.Items, item => item.RelativePath == "new.bin").Snapshot.Bytes);
        Assert.Equal("resource\n"u8.ToArray(), Assert.Single(inventory.Items, item => item.RelativePath == "image.bin").Snapshot.Bytes);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var interrupted = await reader.ReadAsync(input, cancelled.Token);
        Assert.Equal(RouteCategoryFilesystemReadState.Interrupted, interrupted.State);
        Assert.Empty(interrupted.Items);
        Assert.Equal(changedBefore, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Remove generated exposure retains lexical paths and observes unavailable parents afresh"), Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task GeneratedExposureRetainsLexicalPathsAndFreshAvailability()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-exposure-facts");
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(
            new RouteRemoveRequest(workspace.Workspace, RouteRemoveIntegrationWorkspace.CategoryId, RouteRemoveMode.DryRun),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteRemovePlan>(build.Plan);
        var subject = plan.Projection.Subject;
        var discovery = new RouteRemoveSubjectDiscovery
        {
            Request = subject.Request,
            Catalogue = subject.Catalogue,
            SelectedSource = subject.SelectedSource,
            Source = subject.Source,
            Kind = subject.Kind,
        };
        workspace.WriteText(".agents/loader.md", OpenForgeDocumentSeed.GeneratedEntries("- none - No entries - #Empty"));
        workspace.WriteText(".agents/loader-topics/_loader-topics.md", OpenForgeDocumentSeed.GeneratedEntries("- none - No entries - #Empty"));
        workspace.WriteText(".agents/guidance/_guidance.md", OpenForgeDocumentSeed.GeneratedEntries(
            "- [Guide](old%20guide.md#part) - #Route\n- [Again](./old%20guide.md#other) - #Route\n- [Topics](topics/_topics.md) - #Route"));
        workspace.WriteText(".agents/guidance/topics/_topics.md", OpenForgeDocumentSeed.GeneratedEntries("- [Child](child.md#part) - #Route"));
        var before = workspace.SnapshotHashes();
        var reader = new RouteNavigationExposureReader(new MarkdownDocumentParser());
        var complete = await reader.ReadAsync(discovery.Request.Workspace, discovery.Catalogue, TestContext.Current.CancellationToken);
        Assert.False(complete.IsCancelled);
        Assert.Empty(complete.UnavailableParents);
        Assert.Equal(
            [".agents/guidance/old guide.md", ".agents/guidance/topics/_topics.md", ".agents/guidance/topics/child.md"],
            complete.ExposedPaths);
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.DeleteFile(".agents/guidance/topics/_topics.md");
        var missingBefore = workspace.SnapshotHashes();
        var unavailable = await reader.ReadAsync(discovery.Request.Workspace, discovery.Catalogue, TestContext.Current.CancellationToken);
        Assert.False(unavailable.IsCancelled);
        Assert.Equal([".agents/guidance/topics/_topics.md"], unavailable.UnavailableParents);
        Assert.Equal([".agents/guidance/old guide.md", ".agents/guidance/topics/_topics.md"], unavailable.ExposedPaths);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var interrupted = await reader.ReadAsync(discovery.Request.Workspace, discovery.Catalogue, cancelled.Token);
        Assert.True(interrupted.IsCancelled);
        Assert.Empty(interrupted.ExposedPaths);
        Assert.Empty(interrupted.UnavailableParents);
        Assert.Equal(missingBefore, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Remove dry-run plans a leaf pair and performs no persistent effect"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task DryRunPlansLeafPairWithoutWrites()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-leaf-dry-run");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--dry-run", "--json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using var document = JsonDocument.Parse(output.ToString());
        var root = document.RootElement;
        Assert.Equal("route remove", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        var result = root.GetProperty("result");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal("leaf", result.GetProperty("subject").GetProperty("kind").GetString());
        Assert.Contains(
            result.GetProperty("subject").GetProperty("layers").EnumerateArray(),
            layer => layer.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.LeafPath);
        Assert.Contains(
            result.GetProperty("subject").GetProperty("layers").EnumerateArray(),
            layer => layer.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.LeafOverwritePath);
        Assert.Contains(
            result.GetProperty("references").GetProperty("detachments").EnumerateArray(),
            detachment => detachment.GetProperty("visibleLabel").GetString() == "Old guide");
        Assert.Contains(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == RouteRemoveIntegrationWorkspace.LeafPath);
        Assert.Equal("not-created", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Route Remove category dry-run inventories every contained regular item and generated consequence"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task DryRunInventoriesCompleteCategoryAndProjection()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-category-dry-run");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.CategoryId, "--dry-run", "--json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using var document = JsonDocument.Parse(output.ToString());
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("category", result.GetProperty("subject").GetProperty("kind").GetString());
        var items = result.GetProperty("subject").GetProperty("items").EnumerateArray().ToArray();
        Assert.Contains(items, item => item.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.CategoryPath);
        Assert.Contains(items, item => item.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.CategoryChildPath);
        Assert.Contains(items, item => item.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.CategoryNotesPath);
        Assert.Contains(items, item => item.GetProperty("sourcePath").GetString() == RouteRemoveIntegrationWorkspace.CategoryResourcePath);
        Assert.Contains(
            result.GetProperty("generatedNavigation").GetProperty("regions").EnumerateArray(),
            region => region.GetProperty("reasons").EnumerateArray().Any(reason => reason.GetString() == "old-parent"));
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Route Remove projects a Loader-exposed category through the Loader region with exact bounded bytes"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationNavigation")]
    public async Task LoaderProjectionRetainsExactBeforeAndExpectedRegionBytes()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-loader-projection");
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.LoaderCategoryId,
                RouteRemoveMode.Apply),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteRemovePlan>(build.Plan);

        var region = Assert.Single(
            plan.Preview.GeneratedNavigation.Regions,
            value => value.Path == RouteRemoveIntegrationWorkspace.LoaderPath);
        Assert.Equal(RouteRemoveGeneratedState.Changed, region.State);
        Assert.Equal([RouteRemoveGeneratedReason.Loader], region.Reasons);

        var edit = Assert.Single(
            plan.Projection.Navigation.DocumentEdits,
            value => value.LogicalPath == workspace.Combine(RouteRemoveIntegrationWorkspace.LoaderPath));
        var beforeBytes = Encoding.UTF8.GetBytes(
            "\n- [Guidance](guidance/_guidance.md) - #Route\n- [Loader topics](loader-topics/_loader-topics.md) - #Route\n");
        var expectedBytes = Encoding.UTF8.GetBytes(
            "\n- [Guidance](guidance/_guidance.md) - #Route\n");
        Assert.Equal(beforeBytes, edit.BeforeBytes);
        Assert.Equal(expectedBytes, edit.ExpectedBytes);
        Assert.Equal(FileExpectation.Hash(beforeBytes), FileExpectation.Hash(edit.BeforeBytes.AsSpan()));
        Assert.Equal(FileExpectation.Hash(expectedBytes), FileExpectation.Hash(edit.ExpectedBytes.AsSpan()));
        Assert.Equal(
            FileExpectation.Hash(Encoding.UTF8.GetBytes(workspace.ReadText(RouteRemoveIntegrationWorkspace.LoaderPath))),
            FileExpectation.Hash(edit.Snapshot.Bytes.AsSpan()));
    }

    [Theory(DisplayName = "Route Remove refuses missing or claimed ownership before effects"),
     InlineData("missing", "blocked", "route-remove.ownership-unavailable"),
     InlineData("framework-claim", "blocked", "route-remove.ownership-claimed")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task OwnershipBoundariesAreWriteFree(
        string scenario,
        string expectedStatus,
        string expectedCode)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create($"route-remove-ownership-{scenario}");
        if (scenario == "missing")
        {
            workspace.RemoveLifecycle();
        }
        else
        {
            workspace.SeedFrameworkClaim(RouteRemoveIntegrationWorkspace.LeafPath);
        }

        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId],
            output,
            error);

        var primary = expectedStatus == "incomplete" ? output.ToString() : error.ToString();
        Assert.Equal(expectedStatus == "incomplete" ? 3 : 5, completion.ExitCode);
        Assert.Equal(expectedStatus, completion.Status switch
        {
            CliSemanticStatus.Incomplete => "incomplete",
            CliSemanticStatus.Blocked => "blocked",
            _ => completion.Status.ToString().ToLowerInvariant(),
        });
        Assert.Contains(expectedCode, primary, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
        workspace.AssertNoLockInfrastructure();
    }
}
