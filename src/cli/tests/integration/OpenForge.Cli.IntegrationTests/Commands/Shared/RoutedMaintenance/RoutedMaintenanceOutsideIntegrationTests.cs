using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.RoutedMaintenance;

[Trait("Feature", "routed-maintenance"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RoutedMaintenanceOutsideIntegrationTests
{
    private const string Route = "docs/team";
    private const string Other = "reference";
    private const string CustomRoot = "knowledge/main";
    private const string Body = "# Authored note\r\n\r\nKeep these bytes.\r\n";
    private const string Note = "note" + SourceFormClassifier.MarkdownExtension;
    private static readonly string Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(Route);
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Available outside roots named after primary categories keep generic Workspace defaults through nested scopes")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, WorkspaceCategoryNames.Directives), InlineData(CustomRoot, WorkspaceCategoryNames.Directives)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, WorkspaceCategoryNames.Memory), InlineData(CustomRoot, WorkspaceCategoryNames.Memory)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, WorkspaceLayoutNames.SkillsDirectoryName), InlineData(CustomRoot, WorkspaceLayoutNames.SkillsDirectoryName)]
    public async Task CategoryNamesDoNotGrantPrimarySemantics(string mainRoot, string directory)
    {
        using var temporary = Seed(mainRoot, directory);
        var path = directory + "/scope/working/" + SourceFormClassifier.NativeSkillFileName;
        temporary.CreateFile(path, Body);
        var observation = await Observe(temporary);
        var catalogue = (await new SourceReadSessionReader(new PhysicalPathResolver()).ReadIncludedAsync(Assert.IsType<WorkspaceLayout>(observation.Layout), Token)).Catalogue;
        var before = Bytes(temporary);
        var intent = await new RoutedMaintenancePlanner(new PhysicalPathResolver()).PrepareAsync(Request(catalogue, observation) with
        {
            RootPaths = [WorkspaceRoutePath.CanonicalEntrypointPath(directory)],
        }, Token);
        Assert.Empty(intent.Limitations);
        Assert.Equal(path, Assert.Single(intent.Metadata).Path);
        Assert.Equal(RoutedMetadataDefaults.Tags(RoutedMetadataCategory.Workspace), intent.Metadata[0].Completion.Tags);
        AssertBytes(before, temporary);
    }

    [Theory(DisplayName = "Outside completion restores a selected missing anchor with generic metadata and retains all formation facts")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public async Task BoundedCompletion(string mainRoot)
    {
        using var temporary = Seed(mainRoot);
        var note = Route + "/" + WorkspaceCategoryNames.Memory + "/scope/working/" + Note;
        var skill = Route + "/" + WorkspaceLayoutNames.SkillsDirectoryName + "/" + SourceFormClassifier.NativeSkillFileName;
        temporary.CreateFile(note, Body);
        temporary.CreateFile(skill, Body);
        temporary.CreateFile(Other + "/nested/" + Note, Body);
        var observation = await Observe(temporary);
        var layout = Assert.IsType<WorkspaceLayout>(observation.Layout);
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadIncludedAsync(layout, Token);
        var request = Request(session.Catalogue, observation);
        var before = Bytes(temporary);
        var reads = new List<string>();
        var snapshots = new SourceDocumentSnapshotReader();
        var planner = new RoutedMaintenancePlanner(new PhysicalPathResolver(), readSnapshot: async (workspace, verification, token) =>
        {
            lock (reads) reads.Add(verification.Layer.CanonicalPath);
            return await snapshots.ReadAsync(workspace, verification, token);
        });

        var intent = await planner.PrepareAsync(request, Token);

        Assert.Empty(intent.Limitations);
        Assert.Equal(2, intent.Metadata.Length);
        Assert.All(intent.Metadata, completion => Assert.Equal(RoutedMetadataDefaults.Tags(RoutedMetadataCategory.Workspace), completion.Completion.Tags));
        Assert.Contains(intent.Entrypoints, entry => entry.Source.Identity.CanonicalBasePath == Anchor);
        Assert.All(intent.Entrypoints, entry =>
        {
            Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + entry.Source.Identity.CanonicalBasePath, entry.Source.Identity.AutomaticId);
            Assert.True(layout.TryGetMaintenanceBoundary(entry.Source.Identity.CanonicalBasePath, out var boundary));
            Assert.Equal(Route, boundary.DirectoryPath);
        });
        Assert.Same(layout, intent.Formation.Catalogue.Layout);
        var loader = Assert.IsType<SourceLogicalSource>(intent.Formation.Loader);
        var loaderBytes = File.ReadAllBytes(temporary.Combine(layout.LoaderPath));
        var loaderInput = new GeneratedNavigationRegionInput(loader, new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(loaderBytes)));
        var loaderRegion = new GeneratedNavigationRegionPlanner().Plan(new(intent.Formation, [loaderInput], []), loaderInput);
        Assert.Empty(loaderRegion.Entries);
        Assert.Equal(loaderBytes, Assert.IsType<GeneratedNavigationBoundedChange>(loaderRegion.Change).ExpectedDocumentBytes.ToArray());
        Assert.NotEmpty(session.Catalogue.IncludedRows);
        Assert.Equal(session.Catalogue.IncludedRows, intent.Formation.Catalogue.IncludedRows);
        Assert.Equal(session.Catalogue.Aliases, intent.Formation.Catalogue.Aliases);
        Assert.Equal(session.Catalogue.Candidates, intent.Formation.Catalogue.Candidates);
        Assert.Equal(session.Catalogue.Issues, intent.Formation.Catalogue.Issues);
        Assert.Equal(new[] { note, skill }.Order(StringComparer.Ordinal), reads.Order(StringComparer.Ordinal));
        Assert.All(intent.Metadata, completion => Assert.Equal(FrameworkMetadataCompletionState.NoOp,
            new RoutedMetadataCompletionPolicy().Complete(new()
            {
                File = Facts(completion.Path),
                Bytes = completion.Completion.Bytes,
            }, request.Policy).Completion.State));
        Assert.All(intent.Entrypoints, entry => Assert.Empty(new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(entry.IntendedBytes.AsSpan())).Links));
        AssertBytes(before, temporary);
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
    }

    [Theory(DisplayName = "Outside maintenance requires the retained matching available observation and explicit selected root")]
    [InlineData("absent"), InlineData("different-layout"), InlineData("missing"), InlineData("unsafe"), InlineData("unavailable"), InlineData("unselected")]
    public async Task ObservationAndSelectionAreRequired(string scenario)
    {
        using var temporary = Seed(CustomRoot);
        temporary.CreateFile(Route + "/nested/" + Note, Body);
        var observation = await Observe(temporary);
        var layout = Assert.IsType<WorkspaceLayout>(observation.Layout);
        var catalogue = (await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(layout, Token)).Catalogue;
        var request = Request(catalogue, observation);
        var policy = request.Policy;
        if (scenario == "absent") policy = policy with { LayoutRead = null };
        if (scenario == "different-layout")
        {
            var different = WorkspaceLayout.Create(layout.Workspace, layout.MainRoot).WithOutsideRoutes([Route, Other]);
            policy = policy with { LayoutRead = CopyObservation(observation, different, observation.OutsideRoutes) };
        }
        if (scenario is "missing" or "unsafe" or "unavailable")
        {
            var state = scenario switch
            {
                "missing" => WorkspaceOutsideRouteReadState.Missing,
                "unsafe" => WorkspaceOutsideRouteReadState.Unsafe,
                _ => WorkspaceOutsideRouteReadState.Unavailable,
            };
            policy = policy with { LayoutRead = CopyObservation(observation, layout, [.. observation.OutsideRoutes.Select(route => route with { State = state })]) };
        }
        var before = Bytes(temporary);
        var planner = new RoutedMaintenancePlanner(new PhysicalPathResolver(), readSnapshot: (_, _, _) => throw new InvalidOperationException("No snapshot is authorized."));
        var intent = await planner.PrepareAsync(request with { Policy = policy, RootPaths = scenario == "unselected" ? [] : request.RootPaths }, Token);
        Assert.Empty(intent.Originals);
        Assert.Empty(intent.Metadata);
        Assert.Empty(intent.Entrypoints);
        AssertBytes(before, temporary);
    }

    [Theory(DisplayName = "Outside completion keeps managed generated removed private and Library projected files protected")]
    [InlineData("managed"), InlineData("generated"), InlineData("removed"), InlineData("private"), InlineData("library")]
    public async Task ProtectionIsPreserved(string scenario)
    {
        using var temporary = Seed(CustomRoot);
        var note = Route + "/nested/" + Note;
        temporary.CreateFile(note, Body);
        var observation = await Observe(temporary);
        var layout = Assert.IsType<WorkspaceLayout>(observation.Layout);
        var catalogue = (await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(layout, Token)).Catalogue;
        var request = Request(catalogue, observation);
        var ownership = scenario switch
        {
            "managed" => WorkspaceOwnershipDocument.Empty with { Framework = new(new("fixture", null), [note], []) },
            "generated" => WorkspaceOwnershipDocument.Empty with { Framework = new(new("fixture", null), [], [new(note, WorkspaceOwnershipDefinitions.ManagedBlockRegion)]) },
            "private" => WorkspaceOwnershipDocument.Empty with { Framework = new(new("fixture", null), [], []) { GitIgnoredRoutes = [new(Route + "/nested", WorkspaceRoutePath.CanonicalEntrypointPath(Route + "/nested"))] } },
            "library" => WorkspaceOwnershipDocument.Empty with { Libraries = [new("fixture", "upstream", Route, [note])] },
            _ => WorkspaceOwnershipDocument.Empty,
        };
        var settings = scenario == "removed" ? request.Policy.Settings with { RemovedDirectories = [Route + "/nested"] } : request.Policy.Settings;
        var before = Bytes(temporary);
        var intent = await new RoutedMaintenancePlanner(new PhysicalPathResolver(), readSnapshot: (_, _, _) => throw new InvalidOperationException("Protected bytes cannot be acquired."))
            .PrepareAsync(request with { Policy = request.Policy with { Ownership = ownership, Settings = settings } }, Token);
        Assert.Empty(intent.Metadata);
        Assert.Empty(intent.Entrypoints);
        AssertBytes(before, temporary);
    }

    [Theory(DisplayName = "Outside source aliases and escaped directory mappings cannot authorize snapshots or new chains")]
    [InlineData(false), InlineData(true)]
    public async Task PhysicalAliasesAreDeclined(bool escape)
    {
        using var temporary = Seed(CustomRoot);
        temporary.CreateFile(Other + "/" + Note, Body);
        temporary.CreateFile(Route + "/real/" + Note, Body);
        var aliasDirectory = Route + "/alias";
        temporary.CreateDirectorySymbolicLink(aliasDirectory, temporary.Combine(escape ? Other : Route + "/real"));
        var observation = await Observe(temporary);
        var layout = Assert.IsType<WorkspaceLayout>(observation.Layout);
        var catalogue = (await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(layout, Token)).Catalogue;
        var aliasPath = aliasDirectory + "/" + Note;
        var candidate = Assert.IsType<SourceCandidate>(catalogue.FindCandidateByPath(aliasPath));
        var source = new SourceLogicalSource(new(SourceWorkspaceRelativePath.LiteralReferencePrefix + aliasPath, aliasPath),
            new(aliasPath, candidate.PhysicalPath ?? throw new InvalidOperationException(), candidate.Form ?? throw new InvalidOperationException(), SourceLayerKind.Base));
        var retained = new SourceCatalogue(layout, catalogue.Candidates, [source], catalogue.Issues, false, catalogue.Aliases, catalogue.IncludedRows);
        var before = Bytes(temporary);
        var intent = await new RoutedMaintenancePlanner(new PhysicalPathResolver(), readSnapshot: (_, _, _) => throw new InvalidOperationException("Aliases cannot be acquired."))
            .PrepareAsync(Request(retained, observation) with { SelectedSources = [source] }, Token);
        Assert.Empty(intent.Originals);
        Assert.Empty(intent.Entrypoints);
        AssertBytes(before, temporary);
    }

    [Fact(DisplayName = "Outside planning honors cancellation and never rereads altered settings")]
    public async Task CancellationAndRetainedSettings()
    {
        using var temporary = Seed(CustomRoot);
        temporary.CreateFile(Route + "/nested/" + Note, Body);
        var observation = await Observe(temporary);
        var catalogue = (await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(Assert.IsType<WorkspaceLayout>(observation.Layout), Token)).Catalogue;
        temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, "invalid settings after observation");
        var before = Bytes(temporary);
        var planner = new RoutedMaintenancePlanner(new PhysicalPathResolver());
        Assert.NotEmpty((await planner.PrepareAsync(Request(catalogue, observation), Token)).Metadata);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await planner.PrepareAsync(Request(catalogue, observation), cancellation.Token));
        AssertBytes(before, temporary);
    }

    [Fact(DisplayName = "Exact selected coordinates never redirect a merged outside alias into a primary source")]
    public async Task ExactSelectionDoesNotRedirect()
    {
        using var temporary = Seed(CustomRoot);
        var primaryFolder = CustomRoot + "/" + WorkspaceCategoryNames.Guidance;
        var primaryPath = primaryFolder + "/" + Note;
        temporary.CreateFile(primaryPath, Body);
        temporary.CreateFile(WorkspaceRoutePath.CanonicalEntrypointPath(primaryFolder), Body);
        var aliasPath = Route + "/" + Note;
        temporary.CreateFileSymbolicLink(aliasPath, temporary.Combine(primaryPath));
        var observation = await Observe(temporary);
        var layout = Assert.IsType<WorkspaceLayout>(observation.Layout);
        var catalogue = (await new SourceReadSessionReader(new PhysicalPathResolver()).ReadIncludedAsync(layout, Token)).Catalogue;
        var primary = Assert.IsType<SourceLogicalSource>(catalogue.FindByPath(primaryPath));
        var before = Bytes(temporary);
        var intent = await new RoutedMaintenancePlanner(new PhysicalPathResolver(), readSnapshot: (_, _, _) => throw new InvalidOperationException("Exact aliases cannot redirect a read."))
            .PrepareAsync(Request(catalogue, observation) with
            {
                RootPaths = [Anchor, WorkspaceRoutePath.CanonicalEntrypointPath(primaryFolder)],
                SelectedSources = [primary],
                SelectedSourcePaths = [aliasPath],
            }, Token);
        Assert.Empty(intent.Originals);
        Assert.Empty(intent.Metadata);
        Assert.Empty(intent.Entrypoints);
        AssertBytes(before, temporary);
    }

    [Theory(DisplayName = "Outside malformed Markdown retains skip rules while opaque resources and overwrite bytes stay untouched")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public async Task MalformedAndOpaqueContent(string mainRoot)
    {
        using var temporary = Seed(mainRoot);
        var malformed = Route + "/bad" + SourceFormClassifier.MarkdownExtension;
        var note = Route + "/" + Note;
        temporary.CreateFile(malformed, "---\ninvalid: [\n---\n# Broken metadata\n");
        temporary.CreateFile(note, Body);
        var overwrite = SourceOverwritePath.ReadAdjacentPath(note);
        temporary.CreateFile(overwrite, Body);
        temporary.CreateFile(Route + "/asset.txt", [0xFF, 0xFE]);
        var observation = await Observe(temporary);
        var catalogue = (await new SourceReadSessionReader(new PhysicalPathResolver()).ReadIncludedAsync(Assert.IsType<WorkspaceLayout>(observation.Layout), Token)).Catalogue;
        var before = Bytes(temporary);
        var intent = await new RoutedMaintenancePlanner(new PhysicalPathResolver()).PrepareAsync(Request(catalogue, observation), Token);
        Assert.Equal(malformed, Assert.Single(intent.Limitations, limitation => limitation.Malformed).Path);
        Assert.DoesNotContain(malformed, intent.Replacements.Keys);
        Assert.DoesNotContain(overwrite, intent.Replacements.Keys);
        Assert.Equal(note, Assert.Single(intent.Metadata).Path);
        AssertBytes(before, temporary);
    }

    private static RoutedMetadataFileFacts Facts(string path) => new()
    {
        CanonicalPath = path,
        LoaderRootEntrypointPath = Anchor,
        RouteEntrypointPaths = [],
        IsSelected = true,
        IsReadable = true,
        IsSafeOrdinaryLeaf = true,
        IsGeneratedFile = false,
        HasManagedAuthoredRegions = false,
    };

    private static RoutedMaintenanceRequest Request(SourceCatalogue catalogue, WorkspaceLayoutRead observation) => new()
    {
        Catalogue = catalogue,
        RootPaths = [Anchor],
        SelectedSources = [.. catalogue.Sources],
        SelectedSourcePaths = [.. catalogue.Sources.Select(source => source.Identity.CanonicalBasePath)],
        Policy = new() { Layout = catalogue.Layout, LayoutRead = observation, Settings = observation.Settings.Document, Ownership = WorkspaceOwnershipDocument.Empty },
    };

    private static WorkspaceLayoutRead CopyObservation(WorkspaceLayoutRead original, WorkspaceLayout layout, ImmutableArray<WorkspaceOutsideRouteRead> routes)
        => new(original.State, original.Settings, original.Ownership, layout, original.Issues, original.Expectations, routes);

    private static TemporaryWorkspace Seed(string mainRoot, string directory = Route)
    {
        var temporary = TemporaryWorkspace.Create("outside-maintenance");
        temporary.CreateFile(WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{directory}}","{{Other}}"]}""");
        var included = "included/" + Note;
        var destination = Path.GetRelativePath(mainRoot, included).Replace(Path.DirectorySeparatorChar, '/');
        temporary.CreateFile(mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName,
            $"# Loader\n\n## {MarkdownEntriesSectionReader.EntriesHeadingText}\n\n- [Linked note]({destination})\n");
        temporary.CreateFile(included, Body);
        temporary.CreateFile("included/sibling" + SourceFormClassifier.MarkdownExtension, Body);
        temporary.CreateDirectory(directory);
        temporary.CreateDirectory(Other);
        return temporary;
    }

    private static ValueTask<WorkspaceLayoutRead> Observe(TemporaryWorkspace temporary)
        => new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(new(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), Token);

    private static Dictionary<string, byte[]> Bytes(TemporaryWorkspace temporary)
        => Directory.GetFiles(temporary.Path, "*", SearchOption.AllDirectories).ToDictionary(path => Path.GetRelativePath(temporary.Path, path), File.ReadAllBytes, StringComparer.Ordinal);

    private static void AssertBytes(Dictionary<string, byte[]> before, TemporaryWorkspace temporary)
    {
        var after = Bytes(temporary);
        Assert.Equal(before.Keys.Order(StringComparer.Ordinal), after.Keys.Order(StringComparer.Ordinal));
        foreach (var file in before) Assert.Equal(file.Value, after[file.Key]);
    }
}
