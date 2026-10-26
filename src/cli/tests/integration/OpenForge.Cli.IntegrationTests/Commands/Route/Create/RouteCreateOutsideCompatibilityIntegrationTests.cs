using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

[Trait("Feature", "route-create"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteCreateOutsideCompatibilityIntegrationTests
{
    private const string CustomRoot = "knowledge/main";
    private const string Directory = "docs/team";
    private const string Tag = "Fixture";
    private const string Description = "Independent created note";
    private const string Target = Directory + "/scope/note" + SourceFormClassifier.MarkdownExtension;
    private const string Sibling = Directory + "/existing" + SourceFormClassifier.MarkdownExtension;
    private static readonly string Canonical = WorkspaceRoutePath.CanonicalEntrypointPath(Directory);
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    public enum LoaderBoundaryScenario { Malformed, Unsafe, DuplicateRoot, Unreadable }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName, false, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName, true, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.UnderscoreIndexFileName, false, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.UnderscoreIndexFileName, true, false)]
    [InlineData(CustomRoot, SourceFormClassifier.IndexFileName, false, false)]
    [InlineData(CustomRoot, SourceFormClassifier.IndexFileName, true, false)]
    [InlineData(CustomRoot, SourceFormClassifier.UnderscoreIndexFileName, false, false)]
    [InlineData(CustomRoot, SourceFormClassifier.UnderscoreIndexFileName, true, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName, true, true)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.UnderscoreIndexFileName, true, true)]
    [InlineData(CustomRoot, SourceFormClassifier.IndexFileName, true, true)]
    [InlineData(CustomRoot, SourceFormClassifier.UnderscoreIndexFileName, true, true)]
    public async Task ExistingCompatibilityRootSupportsPreviewCreateVerificationAndRepeat(string mainRoot, string fileName, bool listed, bool staleCanonicalRow)
    {
        using var fixture = new Fixture(mainRoot, fileName, listed, staleCanonicalRow);
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);
        var original = File.ReadAllBytes(fixture.Files.Combine(fixture.Root));
        var loader = File.ReadAllBytes(fixture.Files.Combine(fixture.Layout.LoaderPath));
        var settings = File.ReadAllBytes(fixture.Files.Combine(WorkspaceSettingsDefinitions.RelativePath));
        var sibling = File.ReadAllBytes(fixture.Files.Combine(Sibling));
        var preview = await RouteCreateOperationFactory.Create().ExecuteAsync(fixture.Request(RouteCreateMode.DryRun), Token);

        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(RouteCreateMode.DryRun, preview.Mode);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + Target, preview.Target.Id);
        Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
        Assert.False(System.IO.Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(fixture.Layout.Workspace)));

        var build = await new RouteCreatePlanBuilder().BuildAsync(fixture.Request(RouteCreateMode.Apply), Token);
        var plan = Assert.IsType<RouteCreatePlan>(build.Plan);
        var root = plan.NavigationFormation.Catalogue.FindByPath(fixture.Root) ?? throw new InvalidOperationException();
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Root, root.Identity.AutomaticId);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + Target, plan.TargetSource.Identity.AutomaticId);
        Assert.Equal(Target, plan.TargetSource.Identity.CanonicalBasePath);
        Assert.DoesNotContain(plan.FileChanges, change => change.LogicalPath == fixture.Files.Combine(Canonical));
        Assert.Contains(plan.NavigationChanges, change => change.Source.Identity.CanonicalBasePath == fixture.Root);
        fixture.Own(plan);

        var applied = await new RouteCreateApplicationOperation().ExecuteAsync(plan, Token);

        Assert.Equal(RouteCreateVerificationState.Verified, applied.Verification);
        Assert.Empty(applied.Findings);
        Assert.All(applied.Effects, effect => Assert.Equal(RouteCreateEffectOutcome.Verified, effect.Outcome));
        foreach (var change in plan.FileChanges) Assert.Equal(change.IntendedBytes.ToArray(), File.ReadAllBytes(change.LogicalPath));
        Assert.False(File.Exists(fixture.Files.Combine(Canonical)));
        Assert.Equal(loader, File.ReadAllBytes(fixture.Files.Combine(fixture.Layout.LoaderPath)));
        Assert.Equal(settings, File.ReadAllBytes(fixture.Files.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.Equal(sibling, File.ReadAllBytes(fixture.Files.Combine(Sibling)));
        Assert.False(File.Exists(fixture.Files.Combine(WorkspaceOwnershipDefinitions.RelativePath)));
        AssertOutsideEntriesPreserved(original, File.ReadAllBytes(fixture.Files.Combine(fixture.Root)));
        var document = new MarkdownDocumentParser().Parse(File.ReadAllText(fixture.Files.Combine(fixture.Root)));
        var rows = SourceGeneratedEntriesParser.Parse(document).Entries;
        Assert.Equal(new[] { Path.GetFileName(Sibling), "scope/" + Path.GetFileName(WorkspaceRoutePath.CanonicalEntrypointPath(Directory + "/scope")) },
            rows.Select(row => row.Destination).Order(StringComparer.Ordinal));

        var after = fixture.Files.SnapshotHashes(includeRuntime: true);
        var repeat = Assert.IsType<RouteCreatePlan>((await new RouteCreatePlanBuilder().BuildAsync(fixture.Request(RouteCreateMode.Apply), Token)).Plan);
        Assert.True(repeat.IsNoOp);
        var repeated = await RouteCreateOperationFactory.Create().ExecuteAsync(fixture.Request(RouteCreateMode.Apply), Token);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Effects);
        Assert.Equal(after, fixture.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(CustomRoot, true)]
    public async Task MissingAndAmbiguousRootsRemainEffectFree(string mainRoot, bool ambiguous)
    {
        using var fixture = new Fixture(mainRoot, ambiguous ? SourceFormClassifier.IndexFileName : null, listed: false);
        if (ambiguous)
            fixture.Files.CreateFile(Directory + "/" + SourceFormClassifier.UnderscoreIndexFileName, RootDocument());
        else
            fixture.Files.CreateFile(WorkspaceRoutePath.CanonicalEntrypointPath(Directory + "/scope"), RootDocument());
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);

        var build = await new RouteCreatePlanBuilder().BuildAsync(fixture.Request(RouteCreateMode.Apply), Token);

        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings,
            finding => finding.Code == (ambiguous ? RouteCreateFindingCode.RouteAmbiguous : RouteCreateFindingCode.ParentMissing));
        Assert.Empty(build.Formation.Effects);
        Assert.False(File.Exists(fixture.Files.Combine(Canonical)));
        Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
        Assert.False(System.IO.Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(fixture.Layout.Workspace)));
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, LoaderBoundaryScenario.Malformed), InlineData(CustomRoot, LoaderBoundaryScenario.Malformed)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, LoaderBoundaryScenario.Unsafe), InlineData(CustomRoot, LoaderBoundaryScenario.Unsafe)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, LoaderBoundaryScenario.DuplicateRoot), InlineData(CustomRoot, LoaderBoundaryScenario.DuplicateRoot)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, LoaderBoundaryScenario.Unreadable), InlineData(CustomRoot, LoaderBoundaryScenario.Unreadable)]
    public async Task StaleCanonicalRowDoesNotSuppressOtherLoaderBoundaries(string mainRoot, LoaderBoundaryScenario scenario)
    {
        using var fixture = new Fixture(mainRoot, SourceFormClassifier.IndexFileName, listed: true, staleCanonicalRow: true);
        var expected = RouteCreateFindingCode.GeneratedRegionUnsafe;
        FileStream? unreadable = null;
        switch (scenario)
        {
            case LoaderBoundaryScenario.Malformed:
                fixture.SetLoaderEntries(Canonical, "broken%2/_broken" + SourceFormClassifier.MarkdownExtension);
                break;
            case LoaderBoundaryScenario.Unsafe:
                fixture.SetLoaderEntries(Canonical, "unsafe%5Cpath" + SourceFormClassifier.MarkdownExtension);
                break;
            case LoaderBoundaryScenario.DuplicateRoot:
                var primaryRoot = WorkspaceRoutePath.CanonicalEntrypointPath(mainRoot + "/fixture");
                fixture.Files.CreateFile(primaryRoot, RootDocument());
                fixture.SetLoaderEntries(Canonical, primaryRoot, primaryRoot);
                break;
            case LoaderBoundaryScenario.Unreadable:
                if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
                expected = RouteCreateFindingCode.InspectionIncomplete;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);
        try
        {
            if (scenario == LoaderBoundaryScenario.Unreadable)
                unreadable = File.Open(fixture.Files.Combine(fixture.Layout.LoaderPath), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var build = await new RouteCreatePlanBuilder().BuildAsync(fixture.Request(RouteCreateMode.Apply), Token);
            Assert.Null(build.Plan);
            Assert.Contains(build.Formation.Findings, finding => finding.Code == expected);
            Assert.Empty(build.Formation.Effects);
        }
        finally
        {
            unreadable?.Dispose();
        }
        Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
        Assert.False(File.Exists(fixture.Files.Combine(Canonical)));
        Assert.False(System.IO.Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(fixture.Layout.Workspace)));
    }

    private static void AssertOutsideEntriesPreserved(byte[] beforeBytes, byte[] afterBytes)
    {
        var parser = new MarkdownDocumentParser();
        var before = parser.Parse(Encoding.UTF8.GetString(beforeBytes));
        var after = parser.Parse(Encoding.UTF8.GetString(afterBytes));
        var beforeSpan = before.GeneratedRegion.EntriesBlock?.Span ?? throw new InvalidOperationException();
        var afterSpan = after.GeneratedRegion.EntriesBlock?.Span ?? throw new InvalidOperationException();
        Assert.Equal(Encoding.UTF8.GetBytes(before.Source[..beforeSpan.Start]), Encoding.UTF8.GetBytes(after.Source[..afterSpan.Start]));
        Assert.Equal(Encoding.UTF8.GetBytes(before.Source[beforeSpan.End..]), Encoding.UTF8.GetBytes(after.Source[afterSpan.End..]));
    }

    private static string RootDocument()
        => OpenForgeDocumentSeed.Metadata("Independent root metadata", [Tag],
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Prefix = "# Authored compatibility root\r\n\r\nPreserve café and all authored bytes.",
                Entries = $"- [Existing note]({Path.GetFileName(Sibling)}) - #{Tag}",
                LineEnding = "\r\n",
            }) + "\r\n## Authored tail\r\n\r\nUnchanged trailing section.\r\n");

    private sealed class Fixture : IDisposable
    {
        internal TemporaryWorkspace Files { get; } = TemporaryWorkspace.Create("route-create-outside-compatibility");
        internal WorkspaceLayout Layout { get; }
        internal string Root { get; }
        private readonly List<string> _createdFiles = [];
        private readonly List<string> _createdDirectories = [];

        internal Fixture(string mainRoot, string? fileName, bool listed, bool staleCanonicalRow = false)
        {
            Layout = WorkspaceLayout.Create(new CliWorkspace(Files.Path, Files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), mainRoot);
            Root = Directory + "/" + (fileName ?? SourceFormClassifier.IndexFileName);
            var settings = WorkspaceSettingsCodec.SetMainRoot(ReadOnlyMemory<byte>.Empty, mainRoot) ?? [];
            settings = WorkspaceSettingsCodec.AddOutsideRoute(settings, Directory) ?? throw new InvalidOperationException();
            Files.CreateFile(WorkspaceSettingsDefinitions.RelativePath, settings);
            var prefix = string.Concat(Enumerable.Repeat("../", mainRoot.Split('/').Length));
            var loaderDestination = staleCanonicalRow ? Canonical : Root;
            Files.CreateFile(Layout.LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(listed ? $"- [Authored row]({prefix}{loaderDestination}) - #{Tag}" : string.Empty));
            Files.CreateFile(Sibling, OpenForgeDocumentSeed.Metadata("Existing note", [Tag], "# Existing\r\n"));
            if (fileName is not null) Files.CreateFile(Root, RootDocument());
        }

        internal RouteCreateRequest Request(RouteCreateMode mode)
            => new(Layout.Workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + Target,
                new RouteCreateMetadataInput(Description, [Tag], null), null, mode);

        internal void SetLoaderEntries(params string[] destinations)
        {
            var prefix = string.Concat(Enumerable.Repeat("../", Layout.MainRoot.Split('/').Length));
            Files.ReplaceText(Layout.LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(
                string.Join('\n', destinations.Select(destination => $"- [Authored row]({prefix}{destination}) - #{Tag}"))));
        }

        internal void Own(RouteCreatePlan plan)
        {
            _createdFiles.AddRange(plan.FileChanges.Where(change => change.Kind == PlannedFileChangeKind.Create).Select(change => change.LogicalPath));
            _createdDirectories.AddRange(plan.DirectoryCreations.Select(change => change.LogicalPath));
        }

        public void Dispose()
        {
            foreach (var file in _createdFiles)
            {
                if (Path.GetRelativePath(Files.Path, file).StartsWith("..", StringComparison.Ordinal)) throw new InvalidOperationException();
                if (!File.Exists(file)) continue;
                if ((File.GetAttributes(file) & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0) throw new InvalidOperationException();
                File.Delete(file);
            }
            foreach (var directory in _createdDirectories.OrderByDescending(value => value.Length))
            {
                var relative = Path.GetRelativePath(Files.Path, directory);
                if (relative.StartsWith("..", StringComparison.Ordinal)) throw new InvalidOperationException();
                if (System.IO.Directory.Exists(directory)) System.IO.Directory.Delete(directory, recursive: false);
            }
            Files.Dispose();
        }
    }
}
