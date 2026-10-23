using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class RecoveryResidualContinuationJourneyTests
{
    private const string Feature = "recovery-residual-continuation";
    private const string DraftName = "operation-33333333333333333333333333333333.draft";
    private const string DamagedName = "operation-44444444444444444444444444444444.zip";

    [Theory(DisplayName = "Route commands continue past unrelated residuals and retain every historical byte")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task RoutesContinueButCurrentCorruptionStops(bool legacy)
    {
        using var workspace = PublishedJourneyWorkspace.Create("residual-routes");
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(".agents/guidance/new.md", ".agents/guidance/moved.md", ".agents/custom/_custom.md");
        Assert.Equal(0, (await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--automatic"))).ExitCode);
        workspace.WriteText("history.md", "Retained preimage\n");
        Assert.Equal(0, (await workspace.RunAsync("remove", "history.md", "--automatic")).ExitCode);
        var local = PublishedWorkspaceLockStore.LocalRecoveryWorkspaceDirectory(workspace.Path);
        var store = legacy ? workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path) : local;
        Directory.CreateDirectory(store);
        if (legacy)
        {
            var final = Assert.Single(Directory.EnumerateFiles(local, "*.zip"));
            File.Move(final, Path.Combine(store, Path.GetFileName(final)));
        }

        SeedProblems(store);
        var history = Capture(store);
        workspace.WriteText("included.md", "Included authored document\n");
        string[][] commands =
        [
            ["route", "include", "included.md", "--description", "Included document"],
            ["route", "exclude", "included.md"],
            ["route", "create", "guidance/new", "--description", "New note", "--tag", "Guidance"],
            ["route", "update", "guidance/new", "--description", "Changed note"],
            ["route", "move", "guidance/new", ".agents/guidance/moved.md"],
            ["route", "remove", "guidance/moved", "--automatic"],
            ["route", "init", "custom", "--description", "Custom notes", "--tag", "Guidance"],
        ];
        foreach (var command in commands)
        {
            var result = await workspace.RunAsync([.. command, "--format=json", "--detail=full"]);
            AssertHistoryWarning(result, history.Keys);
            AssertHistoryPreserved(history);
        }

        workspace.WriteText(".agents/open-forge.lock.json", "{ damaged current ownership");
        var before = workspace.SnapshotState(includeRuntime: true);
        var stopped = await workspace.RunAsync("route", "create", "guidance/new", "--description", "Blocked note", "--tag", "Guidance");
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
        Assert.Equal(0, (await Run("install", "--automatic", "--frontmatter", "scoped")).ExitCode);
        var store = PublishedWorkspaceLockStore.LocalRecoveryWorkspaceDirectory(workspace.WorkspacePath);
        Directory.CreateDirectory(store);
        SeedProblems(store);
        var history = Capture(store);
        var installed = await Run("extension", "install", "toolkit", "--source", workspace.CataloguePath, "--automatic", "--format=json", "--detail=full");
        AssertHistoryWarning(installed, history.Keys);
        Assert.True(File.Exists(workspace.TargetPath));
        var source = Path.Combine(workspace.CataloguePath, "toolkit/content/.agents/toolkit/_toolkit.md");
        var updated = OpenForgeDocumentSeed.Metadata("Updated toolkit", ["Extension"], "# Updated toolkit\n");
        File.WriteAllText(source, updated);
        var update = await Run("extension", "update", "toolkit", "--source", workspace.CataloguePath, "--automatic", "--format=json", "--detail=full");
        AssertHistoryWarning(update, history.Keys);
        Assert.Equal(updated, File.ReadAllText(workspace.TargetPath));
        var removed = await Run("extension", "remove", "toolkit", "--automatic", "--format=json", "--detail=full");
        AssertHistoryWarning(removed, history.Keys);
        Assert.False(File.Exists(workspace.TargetPath));
        AssertHistoryPreserved(history);
        File.WriteAllText(Path.Combine(workspace.WorkspacePath, ".agents/open-forge.lock.json"), "{ damaged ownership");
        var before = workspace.SnapshotWorkspace();
        var stopped = await Run("extension", "install", "toolkit", "--source", workspace.CataloguePath, "--automatic");
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
        Assert.Equal(0, (await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--automatic"))).ExitCode);
        workspace.WriteText("history.md", "Recovery preimage\n");
        Assert.Equal(0, (await workspace.RunAsync("remove", "history.md", "--automatic")).ExitCode);
        var local = PublishedWorkspaceLockStore.LocalRecoveryWorkspaceDirectory(workspace.Path);
        var legacy = workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path);
        Directory.CreateDirectory(legacy);
        var bundle = Assert.Single(Directory.EnumerateFiles(local, "*.zip"));
        var oldBundle = Path.Combine(legacy, Path.GetFileName(bundle));
        File.Copy(bundle, oldBundle);
        var before = workspace.SnapshotState(includeRuntime: true);
        foreach (var command in new[] { "status", "doctor" })
        {
            var result = await workspace.RunAsync(command, "--format=json", "--detail=full");
            Assert.Empty(result.StandardError);
            using var json = JsonDocument.Parse(result.StandardOutput);
            var data = json.RootElement.GetProperty("data");
            var stores = command == "status" ? data.GetProperty("recovery").GetProperty("stores") : data.GetProperty("recoveryStores");
            Assert.Equal(["workspace", "legacy"], stores.EnumerateArray().Select(store => store.GetProperty("kind").GetString()));
            Assert.Equal([local, legacy], stores.EnumerateArray().Select(store => store.GetProperty("path").GetString()));
            Assert.All(stores.EnumerateArray(), store => Assert.Single(store.GetProperty("candidates").EnumerateArray()));
            Assert.Equal(before, workspace.SnapshotState(includeRuntime: true));
        }

        var oldBytes = File.ReadAllBytes(oldBundle);
        var environment = new Dictionary<string, string>(workspace.ProcessEnvironment) { ["OPENFORGE_DATA_HOME"] = "relative-home" };
        var cleaned = await PublishedJourneyProcess.RunAsync(workspace.Target, workspace.Path, ["cleanup", "--format=json", "--detail=full"], environment);
        Assert.Equal(3, cleaned.ExitCode);
        Assert.False(File.Exists(bundle));
        Assert.Equal(oldBytes, File.ReadAllBytes(oldBundle));
        using var cleanup = JsonDocument.Parse(cleaned.StandardOutput);
        Assert.Equal("incomplete", cleanup.RootElement.GetProperty("status").GetString());
        var records = cleanup.RootElement.GetProperty("data").GetProperty("stores");
        Assert.Equal(JsonValueKind.Null, records[1].GetProperty("path").ValueKind);
        Assert.NotEqual(JsonValueKind.Null, records[1].GetProperty("cause").ValueKind);
    }

    [Fact(DisplayName = "Verified final history does not warn on an unrelated Route Include")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task VerifiedFinalHistoryDoesNotWarn()
    {
        using var workspace = PublishedJourneyWorkspace.Create("residual-verified-final");
        workspace.ExpectCoreInstall();
        Assert.Equal(0, (await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--automatic"))).ExitCode);
        workspace.WriteText("history.md", "Retained preimage\n");
        Assert.Equal(0, (await workspace.RunAsync("remove", "history.md", "--automatic")).ExitCode);
        var store = PublishedWorkspaceLockStore.LocalRecoveryWorkspaceDirectory(workspace.Path);
        var bundle = Assert.Single(Directory.EnumerateFiles(store, "*.zip"));
        var bytes = File.ReadAllBytes(bundle);
        var status = await workspace.RunAsync("status", "--format=json", "--detail=full");
        using (var observation = JsonDocument.Parse(status.StandardOutput))
        {
            var candidate = Assert.Single(observation.RootElement.GetProperty("data").GetProperty("recovery").GetProperty("candidates").EnumerateArray());
            Assert.Equal("verified", candidate.GetProperty("integrity").GetString());
        }

        workspace.WriteText("included.md", "Unrelated authored document\n");
        var included = await workspace.RunAsync("route", "include", "included.md", "--description", "Included document", "--format=json", "--detail=full");
        Assert.Equal(0, included.ExitCode);
        Assert.Equal(bytes, File.ReadAllBytes(bundle));
        using var result = JsonDocument.Parse(included.StandardOutput);
        Assert.DoesNotContain(result.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "route-include.recovery-history");
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
        var command = json.RootElement.GetProperty("command").GetString();
        var route = command is not null && command.StartsWith("route ", StringComparison.Ordinal);
        Assert.True(result.ExitCode == (route ? 0 : 2), result.StandardOutput);
        Assert.Equal(route ? "completed" : "completed-with-warnings", json.RootElement.GetProperty("status").GetString());
        Assert.Contains(json.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("severity").GetString() == "warning");
        foreach (var path in history.Where(path => !path.EndsWith(".zip", StringComparison.Ordinal) || Path.GetFileName(path) == DamagedName))
        {
            Assert.Contains(path, result.StandardOutput.Replace("\\\\", "\\", StringComparison.Ordinal), StringComparison.Ordinal);
        }
    }
}
