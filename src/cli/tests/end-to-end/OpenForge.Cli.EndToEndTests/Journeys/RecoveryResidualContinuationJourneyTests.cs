using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;

using Tokens = OpenForge.Cli.TestSupport.Contracts.PublishedTokens;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class RecoveryResidualContinuationJourneyTests
{
    private const string Feature = "recovery-residual-continuation";
    private static readonly string DraftName = TestDataHome.RecoveryDraftFileName(Guid.Parse("33333333-3333-3333-3333-333333333333"));
    private static readonly string DamagedName = TestDataHome.RecoveryFinalFileName(Guid.Parse("44444444-4444-4444-4444-444444444444"));
    private const string HistoryPath = "history.md";
    private const string IncludedPath = "included.md";
    private const string NewRoute = "guidance/new";
    private const string MovedRoute = "guidance/moved";
    private const string CustomRoute = "custom";
    private const string NewPath = Tokens.Sources.AgentsRoot + "/" + NewRoute + ".md";
    private const string MovedPath = Tokens.Sources.AgentsRoot + "/" + MovedRoute + ".md";
    private const string CustomEntrypoint = Tokens.Sources.AgentsRoot + "/" + CustomRoute + "/_custom.md";
    private const string ExtensionId = "toolkit";
    private const string ExtensionEntrypoint = "_toolkit.md";
    private const string GuidanceTag = "Guidance";
    private const string IncludedDescription = "Included document";
    private const string RelativeHome = "relative-home";
    private const string RetainedPreimage = "Retained preimage\n";
    private static readonly string JsonArgument = $"{Tokens.Syntax.FormatOption}={Tokens.Syntax.JsonFormat}";
    private static readonly string FullDetailArgument = $"{Tokens.Syntax.DetailOption}={Tokens.Presentation.FullDetail}";

    [Theory(DisplayName = "Route commands continue past unrelated residuals and retain every historical byte")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task RoutesContinueButCurrentCorruptionStops(bool legacy)
    {
        using var workspace = PublishedJourneyWorkspace.Create("residual-routes");
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(NewPath, MovedPath, CustomEntrypoint);
        Assert.Equal(0, (await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, Tokens.Install.Automatic))).ExitCode);
        workspace.WriteText(HistoryPath, RetainedPreimage);
        Assert.Equal(0, (await workspace.RunAsync(Tokens.Remove.Command, HistoryPath, Tokens.Remove.Automatic)).ExitCode);
        var local = PublishedWorkspaceLockStore.LocalRecoveryWorkspaceDirectory(workspace.Path);
        var store = legacy ? workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path) : local;
        Directory.CreateDirectory(store);
        if (legacy)
        {
            var final = Assert.Single(Directory.EnumerateFiles(local, "*" + Tokens.RecoveryFormat.FinalFileExtension));
            File.Move(final, Path.Combine(store, Path.GetFileName(final)));
        }

        SeedProblems(store);
        var history = Capture(store);
        workspace.WriteText(IncludedPath, "Included authored document\n");
        string[][] commands =
        [
            [Tokens.Route.Command, Tokens.RouteInclude.Command, IncludedPath, Tokens.RouteInclude.Description, IncludedDescription],
            [Tokens.Route.Command, Tokens.RouteExclude.Command, IncludedPath],
            [Tokens.Route.Command, Tokens.RouteCreate.Command, NewRoute, Tokens.RouteCreate.Description, "New note", Tokens.RouteCreate.Tag, GuidanceTag],
            [Tokens.Route.Command, Tokens.RouteUpdate.Command, NewRoute, Tokens.RouteUpdate.Description, "Changed note"],
            [Tokens.Route.Command, Tokens.RouteMove.Command, NewRoute, MovedPath],
            [Tokens.Route.Command, Tokens.RouteRemove.Command, MovedRoute, Tokens.RouteRemove.Automatic],
            [Tokens.Route.Command, Tokens.RouteInit.Command, CustomRoute, Tokens.RouteInit.Description, "Custom notes", Tokens.RouteInit.Tag, GuidanceTag],
        ];
        foreach (var command in commands)
        {
            var result = await workspace.RunAsync([.. command, JsonArgument, FullDetailArgument]);
            AssertHistoryWarning(result, history.Keys);
            AssertHistoryPreserved(history);
        }

        workspace.WriteText(Tokens.Ownership.RelativePath, "{ damaged current ownership");
        var before = workspace.SnapshotState(includeRuntime: true);
        var stopped = await workspace.RunAsync(Tokens.Route.Command, Tokens.RouteCreate.Command, NewRoute, Tokens.RouteCreate.Description, "Blocked note", Tokens.RouteCreate.Tag, GuidanceTag);
        Assert.Contains(stopped.ExitCode, new[] { 1, 3, 5 });
        Assert.Equal(before, workspace.SnapshotState(includeRuntime: true));
        AssertHistoryPreserved(history);
    }

    [Fact(DisplayName = "Extension install update and remove retain unrelated history while current corruption stops effects")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task ExtensionsContinueButCurrentCorruptionStops()
    {
        using var workspace = PublishedExtensionInstallWorkspace.Create();
        var target = PublishedExecutableTarget.Discover();
        Task<ProcessRunResult> Run(params string[] args) => PublishedJourneyProcess.RunAsync(target, workspace.WorkspacePath, args, workspace.EnvironmentVariables);
        Assert.Equal(0, (await Run(Tokens.Install.Command, Tokens.Install.Automatic, Tokens.Install.Frontmatter, Tokens.Settings.ScopedFrontmatter)).ExitCode);
        var store = PublishedWorkspaceLockStore.LocalRecoveryWorkspaceDirectory(workspace.WorkspacePath);
        Directory.CreateDirectory(store);
        SeedProblems(store);
        var history = Capture(store);
        var installed = await Run(Tokens.Extension.Command, Tokens.ExtensionInstall.Command, ExtensionId, Tokens.ExtensionInstall.Source, workspace.CataloguePath, Tokens.ExtensionInstall.Automatic, JsonArgument, FullDetailArgument);
        AssertHistoryWarning(installed, history.Keys);
        Assert.True(File.Exists(workspace.TargetPath));
        var source = Path.Combine(workspace.CataloguePath, ExtensionId, Tokens.ExtensionPackage.ContentDirectory, Tokens.Sources.AgentsRoot, ExtensionId, ExtensionEntrypoint);
        var updated = OpenForgeDocumentSeed.Metadata("Updated toolkit", ["Extension"], "# Updated toolkit\n");
        File.WriteAllText(source, updated);
        var update = await Run(Tokens.Extension.Command, Tokens.ExtensionUpdate.Command, ExtensionId, Tokens.ExtensionUpdate.Source, workspace.CataloguePath, Tokens.ExtensionUpdate.Automatic, JsonArgument, FullDetailArgument);
        AssertHistoryWarning(update, history.Keys);
        Assert.Equal(updated, File.ReadAllText(workspace.TargetPath));
        var removed = await Run(Tokens.Extension.Command, Tokens.ExtensionRemove.Command, ExtensionId, Tokens.ExtensionRemove.Automatic, JsonArgument, FullDetailArgument);
        AssertHistoryWarning(removed, history.Keys);
        Assert.False(File.Exists(workspace.TargetPath));
        AssertHistoryPreserved(history);
        File.WriteAllText(Path.Combine(workspace.WorkspacePath, Tokens.Ownership.RelativePath), "{ damaged ownership");
        var before = workspace.SnapshotWorkspace();
        var stopped = await Run(Tokens.Extension.Command, Tokens.ExtensionInstall.Command, ExtensionId, Tokens.ExtensionInstall.Source, workspace.CataloguePath, Tokens.ExtensionInstall.Automatic);
        Assert.Contains(stopped.ExitCode, new[] { 1, 3, 5 });
        Assert.Equal(before, workspace.SnapshotWorkspace());
        AssertHistoryPreserved(history);
    }

    [Fact(DisplayName = "Observers separate local and legacy residuals and local Cleanup survives unavailable legacy coverage")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task ObserversSeparateStoresAndCleanupContinuesLocally()
    {
        using var workspace = PublishedJourneyWorkspace.Create("residual-observers");
        workspace.ExpectCoreInstall();
        Assert.Equal(0, (await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, Tokens.Install.Automatic))).ExitCode);
        workspace.WriteText(HistoryPath, "Recovery preimage\n");
        Assert.Equal(0, (await workspace.RunAsync(Tokens.Remove.Command, HistoryPath, Tokens.Remove.Automatic)).ExitCode);
        var local = PublishedWorkspaceLockStore.LocalRecoveryWorkspaceDirectory(workspace.Path);
        var legacy = workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path);
        Directory.CreateDirectory(legacy);
        var bundle = Assert.Single(Directory.EnumerateFiles(local, "*" + Tokens.RecoveryFormat.FinalFileExtension));
        var oldBundle = Path.Combine(legacy, Path.GetFileName(bundle));
        File.Copy(bundle, oldBundle);
        var before = workspace.SnapshotState(includeRuntime: true);
        foreach (var command in new[] { Tokens.Status.Command, Tokens.Doctor.Command })
        {
            var result = await workspace.RunAsync(command, JsonArgument, FullDetailArgument);
            Assert.Empty(result.StandardError);
            using var json = JsonDocument.Parse(result.StandardOutput);
            var data = json.RootElement.GetProperty(Tokens.Json.Data);
            var stores = command == Tokens.Status.Command ? data.GetProperty(Tokens.StatusRecovery.Recovery).GetProperty(Tokens.StatusRecovery.Stores) : data.GetProperty(Tokens.DoctorData.RecoveryStores);
            var kindField = command == Tokens.Status.Command ? Tokens.StatusRecovery.Kind : Tokens.DoctorData.Kind;
            var pathField = command == Tokens.Status.Command ? Tokens.StatusRecovery.Path : Tokens.DoctorData.Path;
            var candidatesField = command == Tokens.Status.Command ? Tokens.StatusRecovery.Candidates : Tokens.DoctorData.Candidates;
            Assert.Equal([Tokens.Recovery.WorkspaceStore, Tokens.Recovery.LegacyStore], stores.EnumerateArray().Select(store => store.GetProperty(kindField).GetString()));
            Assert.Equal([local, legacy], stores.EnumerateArray().Select(store => store.GetProperty(pathField).GetString()));
            Assert.All(stores.EnumerateArray(), store => Assert.Single(store.GetProperty(candidatesField).EnumerateArray()));
            Assert.Equal(before, workspace.SnapshotState(includeRuntime: true));
        }

        var oldBytes = File.ReadAllBytes(oldBundle);
        var environment = new Dictionary<string, string>(workspace.ProcessEnvironment) { [Tokens.DataHome.Variable] = RelativeHome };
        var cleaned = await PublishedJourneyProcess.RunAsync(workspace.Target, workspace.Path, [Tokens.Cleanup.Command, JsonArgument, FullDetailArgument], environment);
        Assert.Equal(3, cleaned.ExitCode);
        Assert.False(File.Exists(bundle));
        Assert.Equal(oldBytes, File.ReadAllBytes(oldBundle));
        using var cleanup = JsonDocument.Parse(cleaned.StandardOutput);
        Assert.Equal(Tokens.Statuses.Incomplete, cleanup.RootElement.GetProperty(Tokens.Json.Status).GetString());
        var records = cleanup.RootElement.GetProperty(Tokens.Json.Data).GetProperty(Tokens.CleanupData.Stores);
        Assert.Equal(JsonValueKind.Null, records[1].GetProperty(Tokens.CleanupData.Path).ValueKind);
        Assert.NotEqual(JsonValueKind.Null, records[1].GetProperty(Tokens.CleanupData.Cause).ValueKind);
    }

    [Fact(DisplayName = "Verified final history does not warn on an unrelated Route Include")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task VerifiedFinalHistoryDoesNotWarn()
    {
        using var workspace = PublishedJourneyWorkspace.Create("residual-verified-final");
        workspace.ExpectCoreInstall();
        Assert.Equal(0, (await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, Tokens.Install.Automatic))).ExitCode);
        workspace.WriteText(HistoryPath, RetainedPreimage);
        Assert.Equal(0, (await workspace.RunAsync(Tokens.Remove.Command, HistoryPath, Tokens.Remove.Automatic)).ExitCode);
        var store = PublishedWorkspaceLockStore.LocalRecoveryWorkspaceDirectory(workspace.Path);
        var bundle = Assert.Single(Directory.EnumerateFiles(store, "*" + Tokens.RecoveryFormat.FinalFileExtension));
        var bytes = File.ReadAllBytes(bundle);
        var status = await workspace.RunAsync(Tokens.Status.Command, JsonArgument, FullDetailArgument);
        using (var observation = JsonDocument.Parse(status.StandardOutput))
        {
            var candidate = Assert.Single(observation.RootElement.GetProperty(Tokens.Json.Data).GetProperty(Tokens.StatusRecovery.Recovery).GetProperty(Tokens.StatusRecovery.Candidates).EnumerateArray());
            Assert.Equal(Tokens.Recovery.VerifiedIntegrity, candidate.GetProperty(Tokens.StatusRecovery.Integrity).GetString());
        }

        workspace.WriteText(IncludedPath, "Unrelated authored document\n");
        var included = await workspace.RunAsync(Tokens.Route.Command, Tokens.RouteInclude.Command, IncludedPath, Tokens.RouteInclude.Description, IncludedDescription, JsonArgument, FullDetailArgument);
        Assert.Equal(0, included.ExitCode);
        Assert.Equal(bytes, File.ReadAllBytes(bundle));
        using var result = JsonDocument.Parse(included.StandardOutput);
        Assert.DoesNotContain(result.RootElement.GetProperty(Tokens.Json.Findings).EnumerateArray(),
            finding => finding.GetProperty(Tokens.Json.Code).GetString() == Tokens.RouteInclude.RecoveryHistoryCode);
    }

    private static void SeedProblems(string store)
    {
        File.WriteAllText(Path.Combine(store, DraftName), "Interrupted prior operation\n");
        File.WriteAllText(Path.Combine(store, DamagedName), "Damaged prior archive\n");
    }

    private static Dictionary<string, byte[]> Capture(string store)
        => Directory.EnumerateFiles(store).ToDictionary(path => path, File.ReadAllBytes, StringComparer.Ordinal);

    private static void AssertHistoryPreserved(IReadOnlyDictionary<string, byte[]> history)
    {
        foreach (var item in history)
        {
            Assert.Equal(item.Value, File.ReadAllBytes(item.Key));
        }
    }

    private static void AssertHistoryWarning(ProcessRunResult result, IEnumerable<string> history)
    {
        Assert.Empty(result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput);
        var command = json.RootElement.GetProperty(Tokens.Json.Command).GetString();
        var route = command is not null && command.StartsWith(Tokens.Route.Command + " ", StringComparison.Ordinal);
        Assert.True(result.ExitCode == (route ? 0 : 2), result.StandardOutput);
        Assert.Equal(route ? Tokens.Statuses.Complete : Tokens.Statuses.Attention, json.RootElement.GetProperty(Tokens.Json.Status).GetString());
        Assert.Contains(json.RootElement.GetProperty(Tokens.Json.Findings).EnumerateArray(), finding => finding.GetProperty(Tokens.Json.Severity).GetString() == Tokens.Report.WarningSeverity);
        foreach (var path in history.Where(path => !path.EndsWith(Tokens.RecoveryFormat.FinalFileExtension, StringComparison.Ordinal) || Path.GetFileName(path) == DamagedName))
        {
            Assert.Contains(json.RootElement.GetProperty(Tokens.Json.Findings).EnumerateArray(),
                finding => finding.GetProperty(Tokens.Json.Subject).ValueKind == JsonValueKind.Object
                    && finding.GetProperty(Tokens.Json.Subject).GetProperty(Tokens.Json.Path).GetString() == path);
        }
    }
}
