using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.Commands.Route.List;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational;

[Trait("Boundary", "OS"), Trait("Feature", "outside-observation"), Trait("Evidence", "Integration")]
public sealed class RouteOutsideObservationIntegrationTests
{
    [Theory(DisplayName = "Workspace entry aliases count one physical layer and retain each logical loading role")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(RouteListOutsideWorkspace.CustomRoot, false), InlineData(RouteListOutsideWorkspace.CustomRoot, true)]
    public async Task WorkspaceEntryAliasRetainsIndependentLoadingRoles(string root, bool exposed)
    {
        using var fixture = new WorkspaceEntryFixture(root, alias: true, exposed);
        var paths = new PhysicalPathResolver();
        var expectedPhysical = paths.ResolveCandidate(fixture.Workspace.LexicalRoot, fixture.Workspace.PhysicalRoot,
            fixture.Files.Combine(WorkspaceEntryFixture.Child)).GetContainedPhysicalPath();
        var retained = await new WorkspaceLayoutReader(paths).ReadAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        var before = fixture.Files.SnapshotHashes();
        var observed = await Create(paths).ReadOutsideAsync(retained, TestContext.Current.CancellationToken);

        Assert.Same(retained, observed.LayoutRead);
        AssertMeasurement(observed.Total, fixture.Bytes(fixture.Layout.LoaderPath, fixture.Anchor, WorkspaceEntryFixture.Child, WorkspaceEntryFixture.Overwrite));
        AssertMeasurement(observed.Startup, exposed
            ? fixture.Bytes(fixture.Layout.LoaderPath, fixture.Anchor, WorkspaceEntryFixture.Child, WorkspaceEntryFixture.Overwrite)
            : fixture.Bytes(fixture.Layout.LoaderPath, fixture.Anchor, WorkspaceEntryFixture.Child));
        AssertMeasurement(observed.Continuity, exposed ? fixture.Bytes(WorkspaceEntryFixture.Child, WorkspaceEntryFixture.Overwrite) : []);
        var child = Assert.Single(observed.Layers, layer => layer.LogicalPaths.Contains(WorkspaceEntryFixture.Child));
        Assert.Equal(expectedPhysical, child.PhysicalPath, PhysicalIdentityTracker.PathComparer);
        Assert.Equal(new[] { SourceLogicalPath.WorkspaceEntryPath, WorkspaceEntryFixture.Child }.Order(StringComparer.Ordinal), child.LogicalPaths);
        Assert.True(child.LoadingStartup);
        Assert.Equal(exposed, child.LoadingContinuity);
        var overwrite = Assert.Single(observed.Layers, layer => layer.LogicalPaths.Contains(WorkspaceEntryFixture.Overwrite));
        Assert.Equal(exposed, overwrite.LoadingStartup);
        Assert.Equal(exposed, overwrite.LoadingContinuity);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + WorkspaceEntryFixture.Child,
            observed.IncludedCatalogue!.FindByPath(WorkspaceEntryFixture.Child)!.Identity.AutomaticId);
        Assert.Equal(exposed, observed.IncludedCatalogue.IncludedRows.Any(row => row.Path == WorkspaceEntryFixture.Child));
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "An ordinary distinct workspace entry keeps its own physical contribution")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task DistinctWorkspaceEntryRetainsItsIndependentMeasurements(string root)
    {
        using var fixture = new WorkspaceEntryFixture(root, alias: false, exposed: true);
        var paths = new PhysicalPathResolver();
        var retained = await new WorkspaceLayoutReader(paths).ReadAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        var before = fixture.Files.SnapshotHashes();
        var observed = await Create(paths).ReadOutsideAsync(retained, TestContext.Current.CancellationToken);
        var expected = fixture.Bytes(SourceLogicalPath.WorkspaceEntryPath, fixture.Layout.LoaderPath, fixture.Anchor,
            WorkspaceEntryFixture.Child, WorkspaceEntryFixture.Overwrite);
        AssertMeasurement(observed.Total, expected);
        AssertMeasurement(observed.Startup, expected);
        AssertMeasurement(observed.Continuity, fixture.Bytes(WorkspaceEntryFixture.Child, WorkspaceEntryFixture.Overwrite));
        Assert.Single(observed.Layers, layer => layer.LogicalPaths.SequenceEqual([SourceLogicalPath.WorkspaceEntryPath]));
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "An unproved workspace entry physical identity leaves affected measurements unavailable")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task UnprovedWorkspaceEntryIdentityDoesNotBecomeAnInventedContribution(string root)
    {
        using var fixture = new WorkspaceEntryFixture(root, alias: false, exposed: true);
        var entryReads = 0;
        var observeEntry = false;
        var paths = new PhysicalPathResolver(path =>
        {
            if (PhysicalIdentityTracker.PathComparer.Equals(path, fixture.Files.Combine(SourceLogicalPath.WorkspaceEntryPath))
                && observeEntry && ++entryReads > 1)
                return new PathComponent(PathComponentState.Inaccessible, path, null, null,
                    FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, new UnauthorizedAccessException()));
            return LinkTargetReader.Read(path);
        });
        var retained = await new WorkspaceLayoutReader(paths).ReadAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        observeEntry = true;
        var before = fixture.Files.SnapshotHashes();
        var observed = await Create(paths).ReadOutsideAsync(retained, TestContext.Current.CancellationToken);
        Assert.NotNull(observed.Inspection!.WorkspaceEntry.Text);
        Assert.Equal(2, entryReads);
        Assert.DoesNotContain(observed.Layers, layer => layer.LogicalPaths.Contains(SourceLogicalPath.WorkspaceEntryPath));
        AssertUnavailable(observed.Total);
        AssertUnavailable(observed.Startup);
        AssertMeasurement(observed.Continuity, fixture.Bytes(WorkspaceEntryFixture.Child, WorkspaceEntryFixture.Overwrite));
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    private static void AssertUnavailable(ContextMeasurementObservation measurement)
    {
        Assert.All(new[] { measurement.Files, measurement.Characters, measurement.Utf8Bytes, measurement.EstimatedTokens }, value =>
        {
            Assert.Equal(OperationalValueState.Unavailable, value.State);
            Assert.Null(value.Value);
        });
    }

    private static void AssertMeasurement(ContextMeasurementObservation measurement, IReadOnlyList<byte[]> expected)
    {
        var characters = expected.Sum(bytes => new UTF8Encoding(false, true).GetString(bytes).EnumerateRunes().LongCount());
        Assert.All(new[] { measurement.Files, measurement.Characters, measurement.Utf8Bytes, measurement.EstimatedTokens },
            value => Assert.Equal(OperationalValueState.Available, value.State));
        Assert.Equal(expected.Count, measurement.Files.Value);
        Assert.Equal(expected.Sum(bytes => (long)bytes.Length), measurement.Utf8Bytes.Value);
        Assert.Equal(characters, measurement.Characters.Value);
        Assert.Equal((long)Math.Ceiling(characters / 4m), measurement.EstimatedTokens.Value);
    }

    private sealed class WorkspaceEntryFixture : IDisposable
    {
        internal const string DirectoryPath = "docs/team";
        internal const string Child = DirectoryPath + "/ordinary.md";
        internal const string Overwrite = DirectoryPath + "/ordinary.overwrite.md";
        private readonly Dictionary<string, byte[]> _inputs = new(StringComparer.Ordinal);
        internal TemporaryWorkspace Files { get; } = TemporaryWorkspace.Create("workspace-entry-partition");
        internal CliWorkspace Workspace => new(Files.Path, Files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
        internal WorkspaceLayout Layout { get; }
        internal string Anchor { get; } = WorkspaceRoutePath.CanonicalEntrypointPath(DirectoryPath);

        internal WorkspaceEntryFixture(string root, bool alias, bool exposed)
        {
            Layout = WorkspaceLayout.Create(Workspace, root).WithOutsideRoutes([DirectoryPath]);
            Files.WriteText(WorkspaceSettingsDefinitions.RelativePath,
                $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{DirectoryPath}}"]}""");
            var relative = Path.GetRelativePath(root, Anchor).Replace('\\', '/');
            Write(Layout.LoaderPath, GeneratedLoaderDocumentBuilder.Build($"- [Team]({relative}) - #{SourceLoadingClosureResolver.LoadNowTag}\n"));
            Write(Anchor, OpenForgeDocumentSeed.Metadata("Independent team", ["Body"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Prefix = "# Team\n\nIndependent authored bytes.",
                Entries = exposed ? $"- [Ordinary](ordinary.md) - #{SourceLoadingClosureResolver.LoadNowTag} #{SourceLoadingClosureResolver.KeepInMindTag}\n"
                    : OpenForge.Cli.Core.Framework.Documents.Markdown.MarkdownEntriesSectionReader.EmptyEntry,
                LineEnding = "\n",
            })));
            Write(Child, OpenForgeDocumentSeed.Metadata("Independent child", ["Body"], "# Child\n\nπ and 😀 bytes.\n"));
            Write(Overwrite, "Independent overwrite π bytes.\r\n");
            if (alias) Files.CreateFileSymbolicLink(SourceLogicalPath.WorkspaceEntryPath, Files.Combine(Child));
            else Write(SourceLogicalPath.WorkspaceEntryPath, "# Independent workspace\n\nSeparate π bytes.\n");
        }

        internal byte[][] Bytes(params string[] paths) => paths.Select(path => _inputs[path]).ToArray();

        private void Write(string path, string text)
        {
            _inputs.Add(path, Encoding.UTF8.GetBytes(text));
            Files.WriteText(path, text);
        }

        public void Dispose() => Files.Dispose();
    }

    public static IEnumerable<object[]> Overlaps()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, RouteListOutsideWorkspace.CustomRoot })
            foreach (var listed in new[] { false, true })
                foreach (var fileIncluded in new[] { false, true })
                    yield return [root, listed, fileIncluded];
    }

    [Theory, MemberData(nameof(Overlaps))]
    public async Task PhysicalPartitionRetainsRowsAndCountsOverlappingInventoryOnce(string root, bool listed, bool fileIncluded)
    {
        using var fixture = new RouteListOutsideWorkspace(root, listed, fileIncluded: fileIncluded);
        var before = fixture.Inner.SnapshotHashes();
        var paths = new PhysicalPathResolver();
        var retained = await new WorkspaceLayoutReader(paths).ReadAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        var reader = Create(paths);
        var observation = await reader.ReadOutsideAsync(retained, TestContext.Current.CancellationToken);
        Assert.Same(retained, observation.LayoutRead);
        Assert.True(observation.RegistrationInventoryComplete);
        var registration = Assert.Single(observation.Routes);
        Assert.Equal(WorkspaceOutsideRouteReadState.Available, registration.Directory.State);
        Assert.Equal(listed, registration.Listed);
        Assert.Equal([fixture.Anchor], registration.EntrypointPaths);
        Assert.Equal(observation.Layers.Count, observation.Layers.Select(layer => layer.PhysicalPath)
            .Distinct(PhysicalIdentityTracker.PathComparer).Count());
        var child = Assert.Single(observation.Layers, layer => layer.LogicalPaths.Contains(RouteListOutsideWorkspace.Child));
        Assert.True(child.InSourceInventory);
        Assert.Equal(listed, child.InLinkedFiles);
        Assert.Single(child.Source.Layers);
        var overwrite = Assert.Single(observation.Layers, layer => layer.LogicalPaths.Contains(RouteListOutsideWorkspace.Overwrite));
        Assert.True(overwrite.InSourceInventory);
        Assert.NotEqual(child.PhysicalPath, overwrite.PhysicalPath);
        Assert.Equal(OperationalValueState.Available, observation.Total.Files.State);
        Assert.Equal(observation.Layers.Count, observation.Total.Files.Value);
        var expected = observation.Layers.Sum(layer => (long)Encoding.UTF8.GetByteCount(Assert.Single(layer.Source.Layers).Text!));
        Assert.Equal(expected, observation.Total.Utf8Bytes.Value);
        var rows = observation.IncludedCatalogue!.IncludedRows.Where(row => row.Path == RouteListOutsideWorkspace.Child).ToArray();
        Assert.Equal(new[] { "First child", "Second child" }, rows.Select(row => row.Entry.Description));
        Assert.NotEqual(rows[0].Entry.ApplyTo, rows[1].Entry.ApplyTo);
        if (listed)
        {
            Assert.NotEmpty(observation.Loading!.PendingConditions);
            Assert.Equal(OperationalValueState.Unavailable, observation.Startup.Files.State);
            Assert.Null(child.LoadingStartup);
        }
        else
        {
            Assert.Empty(observation.Loading!.PendingConditions);
            Assert.False(child.LoadingStartup);
        }
        Assert.Equal("First child", observation.IncludedCatalogue.FindByPath(RouteListOutsideWorkspace.Child)!.IncludedEntry!.Description);
        if (fileIncluded)
            Assert.True(Assert.Single(observation.IncludedFiles!.Files, file => file.Path == fixture.Anchor).Registered);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task CompatibilityAnchorAndUnknownDirectoryRemainSeparateFacts(string root)
    {
        using var fixture = new RouteListOutsideWorkspace(root, listed: false, compatibility: true);
        var paths = new PhysicalPathResolver();
        var retained = await new WorkspaceLayoutReader(paths).ReadAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        var available = await Create(paths).ReadOutsideAsync(retained, TestContext.Current.CancellationToken);
        var route = Assert.Single(available.Routes);
        Assert.Equal(WorkspaceOutsideRouteReadState.Available, route.Directory.State);
        Assert.Equal(NoFollowLeafState.Missing, route.Directory.CatalogueObservation!.State);
        Assert.Equal([fixture.Anchor], route.EntrypointPaths);
        var unknown = new WorkspaceLayoutRead(retained.State, retained.Settings, retained.Ownership, retained.Layout,
            retained.Issues, retained.Expectations, [retained.OutsideRoutes[0] with { State = WorkspaceOutsideRouteReadState.Unavailable }]);
        var observed = await Create(paths).ReadOutsideAsync(unknown, TestContext.Current.CancellationToken);
        Assert.Same(unknown, observed.LayoutRead);
        Assert.Equal(WorkspaceOutsideRouteReadState.Unavailable, Assert.Single(observed.Routes).Directory.State);
        Assert.Empty(observed.Routes[0].SourcePaths);
        Assert.Equal(OperationalValueState.Unavailable, observed.Total.Files.State);
        Assert.Null(observed.Total.Files.Value);
        Assert.True(observed.RegistrationInventoryComplete);
        var withoutObservations = new WorkspaceLayoutRead(retained.State, retained.Settings, retained.Ownership, retained.Layout,
            retained.Issues, retained.Expectations);
        var incomplete = await Create(paths).ReadOutsideAsync(withoutObservations, TestContext.Current.CancellationToken);
        Assert.False(incomplete.RegistrationInventoryComplete);
        Assert.Empty(incomplete.Routes);
        Assert.Equal(OperationalValueState.Unavailable, incomplete.Total.Files.State);
        Assert.Null(incomplete.Total.Files.Value);
    }

    private static RouteObservationReader Create(PhysicalPathResolver paths)
        => new(new RouteSourceInspector(new SourceReadSessionReader(paths), paths),
            new RouteGeneratedNavigationReader(paths), new RouteDoctorGeneratedNavigationReader());

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task CompleteLoadingMeasuresEachPhysicalLayerOnceAndUnreadableLoaderKeepsListingUnknown(string root)
    {
        using var fixture = new RouteListOutsideWorkspace(root, fileIncluded: true);
        fixture.Inner.ReplaceText(fixture.Anchor, RouteListOutsideWorkspace.Document("Retained parent",
            "- [Effective child](child.md) - #LoadNow #KeepInMind\n"));
        var paths = new PhysicalPathResolver();
        var retained = await new WorkspaceLayoutReader(paths).ReadAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        var before = fixture.Inner.SnapshotHashes();
        var reader = Create(paths);
        var result = await reader.ReadOutsideAsync(retained, TestContext.Current.CancellationToken);
        Assert.Empty(result.Loading!.Issues);
        Assert.Empty(result.Loading.PendingConditions);
        Assert.True(Assert.Single(result.Routes).Listed);
        Assert.Equal(OperationalValueState.Available, result.Startup.Files.State);
        var startup = result.Layers.Where(layer => layer.LoadingStartup is true).ToArray();
        Assert.Equal(startup.Length, result.Startup.Files.Value);
        Assert.Equal(startup.Sum(layer => (long)Encoding.UTF8.GetByteCount(Assert.Single(layer.Source.Layers).Text!)), result.Startup.Utf8Bytes.Value);
        Assert.Equal(2, result.Layers.Count(layer => layer.Source.Path == RouteListOutsideWorkspace.Child && layer.LoadingContinuity is true));
        Assert.Equal(2, result.Continuity.Files.Value);
        await reader.ReadStatusAsync(retained, null, TestContext.Current.CancellationToken, result);
        await reader.ReadDoctorAsync(retained, null, TestContext.Current.CancellationToken, result);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        fixture.Inner.ReplaceBytes(fixture.Layout.LoaderPath, [0xc3, 0x28]);
        var unavailable = await Create(paths).ReadOutsideAsync(retained, TestContext.Current.CancellationToken);
        Assert.Null(Assert.Single(unavailable.Routes).Listed);
        Assert.Equal(OperationalValueState.Unavailable, unavailable.Startup.Files.State);
        Assert.Null(unavailable.Startup.Files.Value);
    }
}
