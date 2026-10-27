using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Presentation.Status.Models;
using OpenForge.Cli.Core.Presentation.Status.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Status.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Status.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

[Trait("Boundary", "OS"), Trait("Feature", "status-outside"), Trait("Evidence", "Integration")]
public sealed class StatusOutsideIntegrationTests
{
    public static IEnumerable<object[]> Registrations()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, "knowledge/main" })
            foreach (var listed in new[] { false, true })
                foreach (var file in new[] { false, true }) yield return [root, listed, file];
    }

    [Theory, MemberData(nameof(Registrations))]
    public async Task PhysicalMeasurementsAndIndependentFileFactsSurviveComposition(string root, bool listed, bool file)
    {
        using var fixture = new Fixture(root, listed, file);
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read();
        var outside = Assert.Single(result.Facts.Context.OutsideRoutes!);
        Assert.Equal(Fixture.Directory, outside.Path);
        Assert.Equal(StatusOutsideRouteState.Available, outside.State);
        Assert.Equal(listed, outside.Listed);
        Assert.Equal(5, result.Facts.Context.TotalAvailable.Files.Value);
        Assert.Equal(fixture.Bytes.Sum(bytes => (long)bytes.Length), result.Facts.Context.TotalAvailable.Utf8Bytes.Value);
        var startup = listed ? fixture.Bytes : fixture.Bytes.Take(2).ToArray();
        Assert.Equal(startup.Length, result.Facts.Context.Startup.Current.Files.Value);
        Assert.Equal(startup.Sum(bytes => (long)bytes.Length), result.Facts.Context.Startup.Current.Utf8Bytes.Value);
        Assert.Equal(listed ? 2 : 0, result.Facts.Context.Continuity.Files.Value);
        Assert.Equal(listed ? ["./" + Fixture.Child] : Array.Empty<string>(), result.Facts.Context.ContinuitySources.Select(source => source.SourceId));
        if (listed)
        {
            var contribution = Assert.Single(result.Facts.Context.ContinuitySources);
            Assert.Equal(new[] { Fixture.Child, Fixture.Overwrite }, contribution.Layers.Select(layer => layer.Path));
        }
        if (file)
        {
            Assert.NotNull(result.Facts.Context.IncludedFiles);
            Assert.Equal(listed ? 1 : 0, result.Facts.Context.IncludedFiles.Listed);
            Assert.Equal(listed ? 0 : 1, result.Facts.Context.IncludedFiles.Unlisted);
        }
        Assert.Equal(before, fixture.Files.SnapshotHashes());
        var repeated = await fixture.Read();
        Assert.Equal(result.Facts.Context.TotalAvailable, repeated.Facts.Context.TotalAvailable);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task MissingUnrelatedRegistrationDoesNotEraseKnownStartup(string root)
    {
        using var fixture = new Fixture(root, listed: true, file: false);
        fixture.Register([Fixture.Directory, "docs/absent"]);
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read();
        var outside = Assert.IsAssignableFrom<IReadOnlyList<StatusOutsideRoute>>(result.Facts.Context.OutsideRoutes);
        Assert.Equal(new[] { Fixture.Directory, "docs/absent" }, outside.Select(route => route.Path));
        Assert.Equal(StatusOutsideRouteState.Missing, outside[1].State);
        Assert.Equal(StatusValueState.Unavailable, result.Facts.Context.TotalAvailable.Files.State);
        Assert.Equal(StatusValueState.Available, result.Facts.Context.Startup.Current.Files.State);
        Assert.Contains(result.Findings, finding => finding.Code == StatusFindingCode.ContextInventoryIncomplete && finding.Subject == "docs/absent");
        Assert.DoesNotContain(result.Findings, finding => finding.Code == StatusFindingCode.WorkspaceUnsafe);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task WorkspaceEntryAliasRetainsLogicalContinuityAndCountsOnePhysicalLayer(string root)
    {
        using var fixture = new Fixture(root, listed: true, file: true, alias: true);
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read();
        Assert.Equal(4, result.Facts.Context.TotalAvailable.Files.Value);
        Assert.Equal(fixture.Bytes.Skip(1).Sum(bytes => (long)bytes.Length), result.Facts.Context.TotalAvailable.Utf8Bytes.Value);
        Assert.Equal("./" + Fixture.Child, Assert.Single(result.Facts.Context.ContinuitySources).SourceId);
        Assert.Equal(2, result.Facts.Context.Continuity.Files.Value);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task UnreadableLoaderKeepsKnownDirectoryAndUnknownListing(string root)
    {
        using var fixture = new Fixture(root, listed: true, file: false);
        fixture.Files.ReplaceBytes(fixture.Layout.LoaderPath, [0xff, 0xfe, 0xff]);
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read();
        var outside = Assert.Single(result.Facts.Context.OutsideRoutes!);
        Assert.Equal(StatusOutsideRouteState.Available, outside.State);
        Assert.Null(outside.Listed);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task FileOnlyRegistrationPreservesAbsentOutsideCollection(string root)
    {
        using var fixture = new Fixture(root, listed: true, file: true);
        fixture.Register([]);
        var before = fixture.Files.SnapshotHashes();
        var run = await CliHostCapture.RunAsync([PublishedTokens.Status.Command, PublishedTokens.Syntax.FormatOption,
            PublishedTokens.Syntax.JsonFormat], fixture.Files.Path);
        using var json = JsonDocument.Parse(run.Output);
        Assert.False(json.RootElement.GetProperty(PublishedTokens.Json.Data).TryGetProperty(PublishedTokens.StatusOutsideRoutes.Collection, out _));
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task SafelyAbsentLoaderEstablishesFalseListing(string root)
    {
        using var fixture = new Fixture(root, listed: false, file: false);
        fixture.Files.MoveFile(fixture.Layout.LoaderPath, "retained-loader.md");
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read();
        Assert.False(Assert.Single(result.Facts.Context.OutsideRoutes!).Listed);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task UnknownSettingsInventoryDoesNotInventAnEmptyRegistrationSet(string root)
    {
        using var fixture = new Fixture(root, listed: true, file: false);
        fixture.Files.ReplaceBytes(WorkspaceSettingsDefinitions.RelativePath, [0xff, 0xfe, 0xff]);
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read();
        Assert.Null(result.Facts.Context.OutsideRoutes);
        var retained = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        var finding = Assert.Single(result.Findings, finding => finding.Code == StatusFindingCode.ContextInventoryIncomplete);
        Assert.Equal(WorkspaceSettingsDefinitions.RelativePath, finding.Subject);
        Assert.Equal(retained.Settings.Cause, finding.Cause);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == StatusFindingCode.OperationFailed);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        var run = await CliHostCapture.RunAsync([PublishedTokens.Status.Command, PublishedTokens.Syntax.FormatOption,
            PublishedTokens.Syntax.JsonFormat, PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.FullDetail], fixture.Files.Path);
        using var json = JsonDocument.Parse(run.Output);
        var projected = Assert.Single(json.RootElement.GetProperty(PublishedTokens.Json.Findings).EnumerateArray(),
            item => item.GetProperty(PublishedTokens.Json.Code).GetString() == StatusWording.MachineCode(finding.Code));
        Assert.Equal(WorkspaceSettingsDefinitions.RelativePath,
            projected.GetProperty(PublishedTokens.Json.Subject).GetProperty(PublishedTokens.Json.Path).GetString());
        Assert.Equal(retained.Settings.Cause, Assert.Single(projected.GetProperty(CliJsonFields.Evidence).EnumerateArray())
            .GetProperty(CliJsonFields.Value).GetString());
        Assert.False(json.RootElement.GetProperty(PublishedTokens.Json.Data).TryGetProperty(PublishedTokens.StatusOutsideRoutes.Collection, out _));
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    public static IEnumerable<object[]> CompatibilityListingCases()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, "knowledge/main" })
            foreach (var name in new[] { SourceFormClassifier.IndexFileName, SourceFormClassifier.UnderscoreIndexFileName })
                foreach (var present in new[] { false, true }) yield return [root, name, present];
    }

    [Theory, MemberData(nameof(CompatibilityListingCases))]
    public async Task CompatibilityListingProjectsActualPresenceWithoutChangingAuthoredRows(string root, string name, bool present)
    {
        using var fixture = new Fixture(root, listed: false, file: false);
        var target = Fixture.Directory + "/" + name;
        if (present) fixture.Files.MoveFile(Fixture.Anchor, target);
        fixture.Files.ReplaceText(fixture.Layout.LoaderPath, GeneratedLoaderDocumentBuilder.Build(
            $"- [Authored row]({Path.GetRelativePath(root, target).Replace('\\', '/')}) - #{SourceLoadingClosureResolver.LoadNowTag}\n"));
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read();
        Assert.Equal(present, Assert.Single(result.Facts.Context.OutsideRoutes!).Listed);
        var run = await CliHostCapture.RunAsync([PublishedTokens.Status.Command, PublishedTokens.Syntax.FormatOption,
            PublishedTokens.Syntax.JsonFormat], fixture.Files.Path);
        using var json = JsonDocument.Parse(run.Output);
        var row = Assert.Single(json.RootElement.GetProperty(PublishedTokens.Json.Data)
            .GetProperty(PublishedTokens.StatusOutsideRoutes.Collection).EnumerateArray());
        Assert.Equal(present, row.GetProperty(PublishedTokens.StatusOutsideRoutes.Listed).GetBoolean());
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task UnknownCompatibilityPresenceOmitsTheListingValue(string root)
    {
        using var fixture = new Fixture(root, listed: false, file: false);
        var target = Fixture.Directory + "/" + SourceFormClassifier.IndexFileName;
        fixture.Files.MoveFile(Fixture.Anchor, target);
        fixture.Files.ReplaceText(fixture.Layout.LoaderPath, GeneratedLoaderDocumentBuilder.Build(
            $"- [Authored row]({Path.GetRelativePath(root, target).Replace('\\', '/')}) - #{SourceLoadingClosureResolver.LoadNowTag}\n"));
        var paths = new PhysicalPathResolver(path => PhysicalIdentityTracker.PathComparer.Equals(path, fixture.Files.Combine(target))
            ? new PathComponent(PathComponentState.Inaccessible, path, null, null,
                FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, new UnauthorizedAccessException("Controlled anchor observation")))
            : LinkTargetReader.Read(path));
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read(paths);
        Assert.Null(Assert.Single(result.Facts.Context.OutsideRoutes!).Listed);
        var data = Assert.IsType<StatusData>(StatusReportSelector.Select(result, new CliSelection(CliDetail.Full)).Data);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(data, StatusDataJsonContext.Default.StatusData));
        Assert.False(Assert.Single(json.RootElement.GetProperty(PublishedTokens.StatusOutsideRoutes.Collection).EnumerateArray())
            .TryGetProperty(PublishedTokens.StatusOutsideRoutes.Listed, out _));
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData("knowledge/main", false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData("knowledge/main", true)]
    public async Task OutsideAvailabilityDoesNotBecomeAWorkspaceSafetyFailure(string root, bool unavailable)
    {
        using var fixture = new Fixture(root, listed: true, file: false, link: !unavailable);
        var paths = unavailable ? new PhysicalPathResolver(path => PhysicalIdentityTracker.PathComparer.Equals(path, fixture.Files.Combine(Fixture.Directory))
            ? new PathComponent(PathComponentState.Inaccessible, path, null, null,
                FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, new UnauthorizedAccessException("Controlled outside observation")))
            : LinkTargetReader.Read(path)) : new PhysicalPathResolver();
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read(paths);
        var outside = Assert.Single(result.Facts.Context.OutsideRoutes!);
        Assert.Equal(unavailable ? StatusOutsideRouteState.Unavailable : StatusOutsideRouteState.Unsafe, outside.State);
        Assert.Contains(result.Findings, finding => finding.Code == StatusFindingCode.ContextInventoryIncomplete && finding.Subject == Fixture.Directory);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == StatusFindingCode.WorkspaceUnsafe);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task ConditionedRepeatedRowsRemainPendingWithoutInventedMeasurements(string root)
    {
        using var fixture = new Fixture(root, listed: true, file: true);
        fixture.Files.ReplaceText(Fixture.Anchor, OpenForgeDocumentSeed.Metadata("Independent team", ["Body"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Prefix = "# Team",
            Entries = $"- [First](child.md) - #{SourceLoadingClosureResolver.LoadNowTag} - applies to `src/**`\n"
                + $"- [Second](child.md) - #{SourceLoadingClosureResolver.KeepInMindTag} - applies to `tests/**`\n",
            LineEnding = "\n",
        })));
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read();
        Assert.Equal(StatusValueState.Available, result.Facts.Context.TotalAvailable.Files.State);
        Assert.Equal(StatusValueState.Unavailable, result.Facts.Context.Startup.Current.Files.State);
        Assert.NotEmpty(result.Facts.Context.IncludedFiles!.Pending);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task OrdinaryPrivateTagDoesNotAcquireLoadingFromRegistration(string root)
    {
        using var fixture = new Fixture(root, listed: true, file: false);
        fixture.Files.ReplaceText(Fixture.Anchor, OpenForgeDocumentSeed.Metadata("Independent team", ["Body"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Prefix = "# Team",
            Entries = "- [Child](child.md) - #Private\n",
            LineEnding = "\n",
        })));
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read();
        Assert.Equal(5, result.Facts.Context.TotalAvailable.Files.Value);
        Assert.Equal(3, result.Facts.Context.Startup.Current.Files.Value);
        Assert.Equal(0, result.Facts.Context.Continuity.Files.Value);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task StaleCanonicalListingDoesNotLoadOrReplaceTheCompatibilityAnchor(string root)
    {
        using var fixture = new Fixture(root, listed: true, file: false);
        fixture.Files.MoveFile(Fixture.Anchor, Fixture.Directory + "/index.md");
        var before = fixture.Files.SnapshotHashes();
        var result = await fixture.Read();
        var outside = Assert.Single(result.Facts.Context.OutsideRoutes!);
        Assert.True(outside.Listed);
        Assert.Equal(StatusOutsideRouteState.Available, outside.State);
        Assert.Equal(StatusValueState.Unavailable, result.Facts.Context.TotalAvailable.Files.State);
        Assert.Equal(StatusValueState.Unavailable, result.Facts.Context.Startup.Current.Files.State);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Fact]
    public async Task OutsideHumanAndJsonViewsAreReviewed()
    {
        var captures = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, "knowledge/main" })
        {
            using var fixture = new Fixture(root, listed: false, file: true);
            var before = fixture.Files.SnapshotHashes();
            var run = await CliHostCapture.RunAsync([PublishedTokens.Status.Command, PublishedTokens.Syntax.FormatOption,
                PublishedTokens.Syntax.JsonFormat], fixture.Files.Path);
            await new ReadCommandOutputCapture(fixture.Files.Path).MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = root == WorkspaceLayout.DefaultMainRoot ? "outside-default" : "outside-custom",
                Arguments = [PublishedTokens.Status.Command],
                ExitCode = run.ExitCode,
            }, snapshotCollector: captures);
            Assert.Equal(before, fixture.Files.SnapshotHashes());
        }
        CommandOutputSnapshot.MatchDetailSnapshot(captures);
    }

    private sealed class Fixture : IDisposable
    {
        internal const string Directory = "docs/team";
        internal const string Anchor = Directory + "/_team.md";
        internal const string Child = Directory + "/child.md";
        internal const string Overwrite = Directory + "/child.overwrite.md";
        internal TemporaryWorkspace Files { get; } = TemporaryWorkspace.Create("status-outside");
        internal WorkspaceLayout Layout { get; }
        private readonly bool _file;
        private bool _settingsWritten;
        internal byte[][] Bytes { get; }
        internal CliWorkspace Workspace => new(Files.Path, Files.Path, CliWorkspaceSelectionMethod.CurrentDirectory);

        internal Fixture(string root, bool listed, bool file, bool alias = false, bool link = false)
        {
            Layout = WorkspaceLayout.Create(Workspace, root).WithOutsideRoutes([Directory]);
            _file = file;
            Register([Directory]);
            var texts = new[]
            {
                "# Independent workspace\n\nOriginal bytes.\n",
                GeneratedLoaderDocumentBuilder.Build(listed ? $"- [Authored team]({Path.GetRelativePath(root, Anchor).Replace('\\', '/')}) - #{SourceLoadingClosureResolver.LoadNowTag}\n" : string.Empty),
                OpenForgeDocumentSeed.Metadata("Independent team", ["Body"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
                {
                    Prefix = "# Team\n\nAuthored bytes.",
                    Entries = $"- [Child](child.md) - #{SourceLoadingClosureResolver.LoadNowTag} #{SourceLoadingClosureResolver.KeepInMindTag}\n",
                    LineEnding = "\n",
                })),
                OpenForgeDocumentSeed.Metadata("Independent child", ["Body"], "# Child\n\nIndependent \u03c0 bytes.\n"),
                "Independent overwrite \u03c0 bytes.\r\n",
            };
            var paths = new[] { SourceLogicalPath.WorkspaceEntryPath, Layout.LoaderPath, Anchor, Child, Overwrite };
            if (link) paths = paths.Select(path => path.StartsWith(Directory + "/", StringComparison.Ordinal)
                ? "docs/real" + path[Directory.Length..] : path).ToArray();
            Bytes = texts.Select(Encoding.UTF8.GetBytes).ToArray();
            for (var index = alias ? 1 : 0; index < paths.Length; index++) Files.WriteBytes(paths[index], Bytes[index]);
            if (alias) Files.CreateFileSymbolicLink(SourceLogicalPath.WorkspaceEntryPath, Files.Combine(Child));
            if (link) Files.CreateDirectorySymbolicLink(Directory, Files.Combine("docs/real"));
        }

        internal void Register(string[] directories)
        {
            var text = $$"""
                {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{Layout.MainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":[{{string.Join(",", directories.Select(directory => $"\"{directory}\""))}}],"{{WorkspaceSettingsDefinitions.RootsProperty}}":[{{(_file ? $"\"{Anchor}\"" : string.Empty)}}]}
                """;
            if (_settingsWritten) Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, text);
            else Files.WriteText(WorkspaceSettingsDefinitions.RelativePath, text);
            _settingsWritten = true;
        }

        internal async Task<StatusResult> Read(PhysicalPathResolver? physicalPaths = null)
        {
            var paths = physicalPaths ?? new PhysicalPathResolver();
            return await new StatusOperation(RepairOperationalContributorFactory.Create(paths), paths)
                .ExecuteAsync(new StatusRequest(Workspace), TestContext.Current.CancellationToken);
        }

        public void Dispose() => Files.Dispose();
    }
}
