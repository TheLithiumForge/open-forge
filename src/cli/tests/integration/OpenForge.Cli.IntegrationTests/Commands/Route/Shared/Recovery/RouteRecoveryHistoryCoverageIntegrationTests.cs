using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Ownership;
using System.Collections.Frozen;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Recovery;

[Trait("Boundary", "OS"), Trait("Feature", "route-recovery-history"), Trait("Evidence", "Integration")]
public sealed class RouteRecoveryHistoryCoverageIntegrationTests
{
    private const string GuideDescription = "Guide";
    private const string DocsTag = "Docs";
    private const string LeafDescription = "Leaf";
    private const string NewDescription = "New";
    private const string ChangedDescription = "Changed";
    private const string GuidePath = "guide.md";
    private const string DocsId = "docs";
    private const string LeafId = DocsId + "/leaf";
    private const string MovedId = DocsId + "/moved";
    private const string NewId = DocsId + "/new";
    private const string LeafPath = SourceLogicalPath.AgentsRoot + "/" + LeafId + ".md";
    private const string MovedPath = SourceLogicalPath.AgentsRoot + "/" + MovedId + ".md";
    private const string NewPath = SourceLogicalPath.AgentsRoot + "/" + NewId + ".md";
    private const string NewDirectoryPath = SourceLogicalPath.AgentsRoot + "/" + NewId;
    private const string NewEntrypointPath = NewDirectoryPath + "/_new.md";
    private const string DocsEntrypointPath = SourceLogicalPath.AgentsRoot + "/" + DocsId + "/_docs.md";
    private const string RetainedBytes = "Retained historical bytes";
    private const string UnavailableDirectory = "Unavailable historical directory";
    private const string RetainedDraft = "Retained legacy draft";

    private static readonly FrozenDictionary<RecoveryBundleOperation, (string History, string Coverage)> FindingCodes =
        new Dictionary<RecoveryBundleOperation, (string History, string Coverage)>
        {
            [RecoveryBundleOperation.Include] = (RouteIncludeDefinitions.RecoveryHistoryCode, RouteIncludeDefinitions.RecoveryCoverageUnavailableCode),
            [RecoveryBundleOperation.Exclude] = (RouteExcludeDefinitions.RecoveryHistoryCode, RouteExcludeDefinitions.RecoveryCoverageUnavailableCode),
            [RecoveryBundleOperation.Create] = (RouteCreateDefinitions.RecoveryHistoryCode,
            RouteCreateDefinitions.RecoveryCoverageUnavailableCode),
            [RecoveryBundleOperation.Init] = (RouteInitDefinitions.RecoveryHistoryCode,
            RouteInitDefinitions.RecoveryCoverageUnavailableCode),
            [RecoveryBundleOperation.Move] = (RouteMoveDefinitions.RecoveryHistoryCode,
            RouteMoveDefinitions.RecoveryCoverageUnavailableCode),
            [RecoveryBundleOperation.Update] = (RouteUpdateDefinitions.RecoveryHistoryCode,
            RouteUpdateDefinitions.RecoveryCoverageUnavailableCode),
            [RecoveryBundleOperation.Remove] = (RouteRemoveDefinitions.RecoveryHistoryCode,
            RouteRemoveDefinitions.RecoveryCoverageUnavailableCode),
        }.ToFrozenDictionary();

    [Theory(DisplayName = "Route commands omit verified finals from both stores on application and repeat no-ops")]
    [InlineData(RecoveryBundleOperation.Include), InlineData(RecoveryBundleOperation.Exclude), InlineData(RecoveryBundleOperation.Create), InlineData(RecoveryBundleOperation.Init)]
    [InlineData(RecoveryBundleOperation.Move), InlineData(RecoveryBundleOperation.Update), InlineData(RecoveryBundleOperation.Remove)]
    public async Task VerifiedHistoryIsSilent(object commandValue)
    {
        var command = Assert.IsType<RecoveryBundleOperation>(commandValue);
        using var files = SeedWorkspace(command == RecoveryBundleOperation.Exclude);
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var target = files.Combine(GuidePath);
        var snapshot = FileStateSnapshot.File(target, target, File.ReadAllBytes(target));
        var prepared = await RecoveryBundleStore.PrepareAsync(RecoveryBundleInput.Create(
            workspace, RouteIncludeDefinitions.CommandIdentity,
            RecoveryBundleAttribution.Create(RecoveryBundleProducer.Route, RecoveryBundleOperation.Include, workspace),
            Guid.NewGuid(), [RecoveryBundleTarget.Create(PlannedFileChange.Delete(snapshot.Expectation), snapshot)]),
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var legacy = RecoveryStoreLocator.Legacy(workspace);
        Assert.NotNull(legacy);
        Directory.CreateDirectory(legacy.WorkspaceDirectory);
        var legacyPath = Path.Combine(legacy.WorkspaceDirectory, Path.GetFileName(preparation.BundlePath));
        File.Copy(preparation.BundlePath, legacyPath);
        var bytes = File.ReadAllBytes(preparation.BundlePath);
        try
        {
            var result = await CliHostCapture.RunAsync(Arguments(command), files.Path);

            Assert.True(result.ExitCode == 0, result.Output + result.Error);
            using var json = JsonDocument.Parse(result.Output);
            Assert.Empty(json.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray());
            AssertApplied(command, files, json.RootElement);
            if (command is not (RecoveryBundleOperation.Move or RecoveryBundleOperation.Remove))
            {
                var before = files.SnapshotHashes();
                string[] arguments = command == RecoveryBundleOperation.Init ? [RouteDefinitions.RouteGroup.Name, RouteInitDefinitions.InitCommand.Name, NewId, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat] : Arguments(command);
                var repeat = await CliHostCapture.RunAsync(arguments, files.Path);
                Assert.True(repeat.ExitCode == 0, repeat.Output + repeat.Error);
                using var repeated = JsonDocument.Parse(repeat.Output);
                Assert.Empty(repeated.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray());
                Assert.Equal(before, files.SnapshotHashes());
            }
            Assert.Equal(bytes, File.ReadAllBytes(preparation.BundlePath));
            Assert.Equal(bytes, File.ReadAllBytes(legacyPath));
        }
        finally
        {
            File.Delete(preparation.BundlePath);
            File.Delete(legacyPath);
            RemoveCreatedTargets(files);
        }
    }

    [Theory(DisplayName = "Route commands warn about every retained path across both recovery stores")]
    [InlineData(RecoveryBundleOperation.Include), InlineData(RecoveryBundleOperation.Exclude), InlineData(RecoveryBundleOperation.Create), InlineData(RecoveryBundleOperation.Init)]
    [InlineData(RecoveryBundleOperation.Move), InlineData(RecoveryBundleOperation.Update), InlineData(RecoveryBundleOperation.Remove)]
    public async Task AllObservedPathsRemainDistinct(object commandValue)
    {
        var command = Assert.IsType<RecoveryBundleOperation>(commandValue);
        using var files = SeedWorkspace(command == RecoveryBundleOperation.Exclude);
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var local = RecoveryStoreLocator.Local(workspace);
        var legacy = RecoveryStoreLocator.Legacy(workspace);
        Assert.NotNull(legacy);
        Directory.CreateDirectory(local.WorkspaceDirectory);
        Directory.CreateDirectory(legacy.WorkspaceDirectory);
        var identity = Guid.NewGuid();
        string[] paths =
        [
            Path.Combine(local.WorkspaceDirectory, RecoveryBundleFormatV1.DraftFileName(Guid.NewGuid())),
            Path.Combine(local.WorkspaceDirectory, RecoveryBundleFormatV1.FinalFileName(identity)),
            Path.Combine(legacy.WorkspaceDirectory, RecoveryBundleFormatV1.FinalFileName(identity)),
        ];
        foreach (var path in paths) await File.WriteAllTextAsync(path, RetainedBytes, TestContext.Current.CancellationToken);
        try
        {
            var result = await CliHostCapture.RunAsync(Arguments(command), files.Path);

            Assert.True(result.ExitCode == 0, result.Output + result.Error);
            using var json = JsonDocument.Parse(result.Output);
            var warnings = json.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray()
                .Where(finding => finding.GetProperty(CliJsonFields.Code).GetString() == FindingCodes[command].History).ToArray();
            Assert.Equal(paths.Order(StringComparer.Ordinal),
                warnings.Select(finding => finding.GetProperty(CliJsonFields.Subject).GetProperty(CliJsonFields.Path).GetString()).Order(StringComparer.Ordinal));
            Assert.All(warnings, finding => Assert.Equal(CliReportVocabulary.Name(CliSeverity.Warning), finding.GetProperty(CliJsonFields.Severity).GetString()));
            Assert.All(paths, path => Assert.Equal(RetainedBytes, File.ReadAllText(path)));
            AssertApplied(command, files, json.RootElement);
        }
        finally
        {
            foreach (var path in paths) File.Delete(path);
            RemoveCreatedTargets(files);
        }
    }

    [Theory(DisplayName = "Route commands proceed when historical legacy coverage is unavailable")]
    [InlineData(RecoveryBundleOperation.Include), InlineData(RecoveryBundleOperation.Exclude), InlineData(RecoveryBundleOperation.Create), InlineData(RecoveryBundleOperation.Init)]
    [InlineData(RecoveryBundleOperation.Move), InlineData(RecoveryBundleOperation.Update), InlineData(RecoveryBundleOperation.Remove)]
    public async Task UnavailableLegacyCoverageWarnsWithoutBlocking(object commandValue)
    {
        var command = Assert.IsType<RecoveryBundleOperation>(commandValue);
        using var files = SeedWorkspace(command == RecoveryBundleOperation.Exclude);
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var legacy = RecoveryStoreLocator.Legacy(workspace);
        Assert.NotNull(legacy);
        Directory.CreateDirectory(legacy.RootPath);
        await File.WriteAllTextAsync(legacy.WorkspaceDirectory, UnavailableDirectory, TestContext.Current.CancellationToken);
        try
        {
            var result = await CliHostCapture.RunAsync(Arguments(command), files.Path);

            Assert.True(result.ExitCode == 0, result.Output + result.Error);
            using var json = JsonDocument.Parse(result.Output);
            var warning = Assert.Single(json.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray(),
                finding => finding.GetProperty(CliJsonFields.Code).GetString() == FindingCodes[command].Coverage);
            Assert.Equal(CliReportVocabulary.Name(CliSeverity.Warning), warning.GetProperty(CliJsonFields.Severity).GetString());
            Assert.Equal(legacy.WorkspaceDirectory, warning.GetProperty(CliJsonFields.Subject).GetProperty(CliJsonFields.Path).GetString());
            Assert.Equal(UnavailableDirectory, await File.ReadAllTextAsync(legacy.WorkspaceDirectory, TestContext.Current.CancellationToken));
            AssertApplied(command, files, json.RootElement);
        }
        finally
        {
            File.Delete(legacy.WorkspaceDirectory);
            RemoveCreatedTargets(files);
        }
    }

    [Theory(DisplayName = "Successful Route no-ops retain legacy history warnings without creating runtime state")]
    [InlineData(RecoveryBundleOperation.Include), InlineData(RecoveryBundleOperation.Exclude), InlineData(RecoveryBundleOperation.Init), InlineData(RecoveryBundleOperation.Update)]
    public async Task NoOpWarnsWithoutRuntimeCreation(object commandValue)
    {
        var command = Assert.IsType<RecoveryBundleOperation>(commandValue);
        using var files = SeedWorkspace(command == RecoveryBundleOperation.Include);
        var workspace = new CliWorkspace(files.Path, files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var legacy = RecoveryStoreLocator.Legacy(workspace);
        Assert.NotNull(legacy);
        Directory.CreateDirectory(legacy.WorkspaceDirectory);
        var path = Path.Combine(legacy.WorkspaceDirectory, RecoveryBundleFormatV1.DraftFileName(Guid.NewGuid()));
        await File.WriteAllTextAsync(path, RetainedDraft, TestContext.Current.CancellationToken);
        string[] arguments = command switch
        {
            RecoveryBundleOperation.Include => [RouteDefinitions.RouteGroup.Name, RouteIncludeDefinitions.Command.Name, GuidePath, RouteIncludeDefinitions.Description.Name, GuideDescription, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
            RecoveryBundleOperation.Exclude => [RouteDefinitions.RouteGroup.Name, RouteExcludeDefinitions.Command.Name, GuidePath, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
            RecoveryBundleOperation.Init => [RouteDefinitions.RouteGroup.Name, RouteInitDefinitions.InitCommand.Name, DocsId, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
            RecoveryBundleOperation.Update => [RouteDefinitions.RouteGroup.Name, RouteUpdateDefinitions.UpdateCommand.Name, LeafId, RouteUpdateDefinitions.Description.Name, LeafDescription, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
            _ => throw new ArgumentOutOfRangeException(nameof(command)),
        };
        var before = files.SnapshotHashes();
        try
        {
            var result = await CliHostCapture.RunAsync(arguments, files.Path);

            Assert.True(result.ExitCode == 0, result.Output + result.Error);
            using var json = JsonDocument.Parse(result.Output);
            var warning = Assert.Single(json.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray(),
                finding => finding.GetProperty(CliJsonFields.Code).GetString() == FindingCodes[command].History);
            Assert.Equal(path, warning.GetProperty(CliJsonFields.Subject).GetProperty(CliJsonFields.Path).GetString());
            Assert.Equal(CliReportVocabulary.Name(CliSeverity.Warning), warning.GetProperty(CliJsonFields.Severity).GetString());
            Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace)));
            Assert.Equal(before, files.SnapshotHashes());
            Assert.Equal(RetainedDraft, await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static void AssertApplied(RecoveryBundleOperation command, TemporaryWorkspace files, JsonElement report)
    {
        if (command == RecoveryBundleOperation.Move)
        {
            Assert.False(File.Exists(files.Combine(LeafPath)));
            Assert.True(File.Exists(files.Combine(MovedPath)));
        }
        else
        {
            Assert.NotEmpty(report.GetProperty(CliJsonFields.Effects).EnumerateArray());
        }
    }

    private static void RemoveCreatedTargets(TemporaryWorkspace files)
    {
        File.Delete(files.Combine(NewPath));
        File.Delete(files.Combine(MovedPath));
        if (Directory.Exists(files.Combine(NewDirectoryPath)))
        {
            File.Delete(files.Combine(NewEntrypointPath));
            Directory.Delete(files.Combine(NewDirectoryPath));
        }
    }

    private static string[] Arguments(RecoveryBundleOperation command) => command switch
    {
        RecoveryBundleOperation.Include => [RouteDefinitions.RouteGroup.Name, RouteIncludeDefinitions.Command.Name, GuidePath, RouteIncludeDefinitions.Description.Name, GuideDescription, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
        RecoveryBundleOperation.Exclude => [RouteDefinitions.RouteGroup.Name, RouteExcludeDefinitions.Command.Name, GuidePath, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
        RecoveryBundleOperation.Create => [RouteDefinitions.RouteGroup.Name, RouteCreateDefinitions.CreateCommand.Name, NewId, RouteCreateDefinitions.Description.Name, NewDescription, RouteCreateDefinitions.Tag.Name, DocsTag, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
        RecoveryBundleOperation.Init => [RouteDefinitions.RouteGroup.Name, RouteInitDefinitions.InitCommand.Name, NewId, RouteInitDefinitions.Description.Name, NewDescription, RouteInitDefinitions.Tag.Name, DocsTag, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
        RecoveryBundleOperation.Move => [RouteDefinitions.RouteGroup.Name, RouteMoveDefinitions.MoveCommand.Name, LeafId, MovedId, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
        RecoveryBundleOperation.Update => [RouteDefinitions.RouteGroup.Name, RouteUpdateDefinitions.UpdateCommand.Name, LeafId, RouteUpdateDefinitions.Description.Name, ChangedDescription, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
        RecoveryBundleOperation.Remove => [RouteDefinitions.RouteGroup.Name, RouteRemoveDefinitions.RemoveCommand.Name, LeafId, RouteRemoveDefinitions.Automatic.Name, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
        _ => throw new ArgumentOutOfRangeException(nameof(command)),
    };

    private static TemporaryWorkspace SeedWorkspace(bool included)
    {
        var files = TemporaryWorkspace.Create("route-history-coverage");
        var guideRow = included ? $"\n- [{GuideDescription}](../{GuidePath})" : string.Empty;
        files.CreateFile(SourceLogicalPath.LoaderPath, $"# Loader\n\n## Entries\n\n- [{DocsTag}]({Path.GetRelativePath(SourceLogicalPath.AgentsRoot, DocsEntrypointPath).Replace('\\', '/')}) - #{DocsTag}{guideRow}\n");
        var settings = new JsonObject { [WorkspaceSettingsDefinitions.SchemaVersionProperty] = WorkspaceSettingsDefinitions.SchemaVersion };
        if (included) settings[WorkspaceSettingsDefinitions.RootsProperty] = new JsonArray(GuidePath);
        files.CreateFile(WorkspaceSettingsDefinitions.RelativePath, settings.ToJsonString());
        files.CreateFile(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
        {
            Framework = new(new("framework", null), [], []),
        }));
        files.CreateFile(DocsEntrypointPath, $"---\nopen-forge:\n  description: {DocsTag}\n  tags: [{DocsTag}]\n---\n# {DocsTag}\n\n## Entries\n\n- [{LeafDescription}]({Path.GetFileName(LeafPath)}) - #{DocsTag}\n");
        files.CreateFile(LeafPath, $"---\nopen-forge:\n  description: {LeafDescription}\n  tags: [{DocsTag}]\n---\n# {LeafDescription}\n");
        files.CreateFile(GuidePath, $"# {GuideDescription}\n");
        return files;
    }
}
