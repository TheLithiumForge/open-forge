using System.Text.Json;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-config-permissions"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolConfigPermissionTests
{
    private const string Target = "synthetic.json";
    private const string Other = "other.json";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Saved destination grants admit configuration changes and missing grants block every effect")]
    [InlineData(false), InlineData(true)]
    public async Task SavedGrant(bool granted)
    {
        using var scenario = new ToolServiceScenario();
        if (granted) scenario.Files.Grant(Target);
        scenario.Files.WriteText(Target, "{}");
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Set("value", Target, "true")), Token);
        Assert.Equal(granted ? WorkspacePermissionDecision.Granted : WorkspacePermissionDecision.Required, plan.Permissions.Decision);
        Assert.Equal(Target, Assert.Single(plan.Permissions.Required));
        if (granted) Assert.Empty(plan.Permissions.Missing);
        else Assert.Equal(Target, Assert.Single(plan.Permissions.Missing));
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.False(result.Cancelled);
        if (granted)
        {
            Assert.Null(result.Failure);
            Assert.Single(result.CompletedChanges);
            using var current = JsonDocument.Parse(await File.ReadAllBytesAsync(scenario.Files.Combine(Target), Token));
            Assert.True(current.RootElement.GetProperty("value").GetBoolean());
        }
        else
        {
            Assert.NotNull(result.Failure);
            Assert.Empty(result.CompletedChanges);
            Assert.Empty(result.PendingBackups);
            Assert.Equal("{}"u8.ToArray(), await File.ReadAllBytesAsync(scenario.Files.Combine(Target), Token));
            Assert.False(File.Exists(scenario.Files.Combine(ToolBackupPaths.Beside(Target))));
        }
    }

    [Fact(DisplayName = "Per-invocation preview grants union with saved grants without writing workspace settings")]
    public async Task PreviewGrantUnion()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.Grant(Target);
        var settings = scenario.Files.Combine(WorkspaceSettingsDefinitions.RelativePath);
        var original = await File.ReadAllBytesAsync(settings, Token);
        scenario.Context = scenario.Context with { AdditionalAllowInstallPaths = [Other] };
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Set("first", Target, "true"), ToolServiceScenario.Set("second", Other, "false")), Token);
        Assert.Equal(WorkspacePermissionDecision.Granted, plan.Permissions.Decision);
        Assert.Empty(plan.Permissions.Missing);
        Assert.Equal(2, plan.Permissions.Required.Length);
        Assert.False(File.Exists(scenario.Files.Combine(Target)));
        Assert.False(File.Exists(scenario.Files.Combine(Other)));
        Assert.Equal(original, await File.ReadAllBytesAsync(settings, Token));
        var applied = await scenario.Service.ApplyAsync(plan, Token);
        Assert.Null(applied.Failure);
        Assert.Equal(2, applied.CompletedChanges.Count);
        Assert.Equal(original, await File.ReadAllBytesAsync(settings, Token));
    }

    [Fact(DisplayName = "Revoking a saved grant after planning blocks configuration replacement before backups")]
    public async Task RevokedApply()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.Grant(Target);
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Set("value", Target, "true")), Token);
        scenario.Files.Grant();
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.NotNull(result.Failure);
        Assert.False(result.Cancelled);
        Assert.Empty(result.CompletedChanges);
        Assert.Empty(result.PendingBackups);
        Assert.False(File.Exists(scenario.Files.Combine(Target)));
    }

    [Fact(DisplayName = "Removal needs the current destination grant and revalidates an earlier granted plan")]
    public async Task RemovalGrant()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.Grant(Target);
        var applied = await scenario.Service.ApplyAsync(await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Set("value", Target, "true")), Token), Token);
        Assert.Null(applied.Failure);
        var request = scenario.Removal(applied.RemainingAdditions);
        var granted = await scenario.Service.PlanRemoveAsync(request, Token);
        Assert.Equal(WorkspacePermissionDecision.Granted, granted.Permissions.Decision);
        var before = await File.ReadAllBytesAsync(scenario.Files.Combine(Target), Token);
        scenario.Files.Grant();
        var denied = await scenario.Service.PlanRemoveAsync(request, Token);
        Assert.Equal(Target, Assert.Single(denied.Permissions.Missing));
        var stopped = await scenario.Service.RemoveAsync(granted, Token);
        Assert.NotNull(stopped.Failure);
        Assert.Empty(stopped.RemovedChanges);
        Assert.Single(stopped.RemainingAdditions);
        Assert.Equal(before, await File.ReadAllBytesAsync(scenario.Files.Combine(Target), Token));
        scenario.Files.Grant(Target);
        var removed = await scenario.Service.RemoveAsync(await scenario.Service.PlanRemoveAsync(request, Token), Token);
        Assert.Null(removed.Failure);
        Assert.Empty(removed.RemainingAdditions);
    }

    [Fact(DisplayName = "Migrating a recorded addition requires grants for its old and new destinations")]
    public async Task MigrationGrant()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.Grant(Target);
        var original = ToolServiceScenario.Set("value", Target, "true");
        var applied = await scenario.Service.ApplyAsync(await scenario.Service.PlanAsync(scenario.Request(original), Token), Token);
        Assert.Null(applied.Failure);
        var request = scenario.Request(original with { PathTemplate = Other }) with { AppliedChanges = applied.RemainingAdditions };
        var denied = await scenario.Service.PlanAsync(request, Token);
        Assert.Equal(Other, Assert.Single(denied.Permissions.Missing));
        Assert.Equal(2, denied.Permissions.Required.Length);
        var stopped = await scenario.Service.ApplyAsync(denied, Token);
        Assert.NotNull(stopped.Failure);
        Assert.Empty(stopped.RemovedChanges);
        Assert.Single(stopped.RemainingAdditions);
        Assert.True(File.Exists(scenario.Files.Combine(Target)));
        Assert.False(File.Exists(scenario.Files.Combine(Other)));
        scenario.Files.Grant(Target, Other);
        var migrated = await scenario.Service.ApplyAsync(await scenario.Service.PlanAsync(request, Token), Token);
        Assert.Null(migrated.Failure);
        Assert.Equal(Other, Assert.IsType<ToolJsonValueAddition>(Assert.Single(migrated.RemainingAdditions)).Path);
    }

    [Theory(DisplayName = "Default and custom content roots keep their existing implicit destination admission")]
    [InlineData(false), InlineData(true)]
    public async Task ImplicitRoot(bool custom)
    {
        using var scenario = new ToolServiceScenario();
        var root = custom ? "knowledge" : WorkspaceLayout.DefaultMainRoot;
        scenario.Files.ConfigurePermissions(root);
        scenario.Context = scenario.Context with { MainRoot = root };
        var relative = root + "/memory/synthetic.json";
        Directory.CreateDirectory(Path.GetDirectoryName(scenario.Files.Combine(relative)) ?? throw new InvalidOperationException());
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Set("value", relative, "true")), Token);
        Assert.Equal(WorkspacePermissionDecision.NotRequired, plan.Permissions.Decision);
        Assert.Empty(plan.Permissions.Required);
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.Null(result.Failure);
        Assert.True(File.Exists(scenario.Files.Combine(relative)));
    }

    [Fact(DisplayName = "A directory grant admits a concrete destination through the existing allow-list policy")]
    public async Task DirectoryGrant()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.Grant("synthetic-data");
        Directory.CreateDirectory(scenario.Files.Combine("synthetic-data"));
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Set("value", "synthetic-data/value.json", "true")), Token);
        Assert.Equal(WorkspacePermissionDecision.Granted, plan.Permissions.Decision);
        Assert.Null((await scenario.Service.ApplyAsync(plan, Token)).Failure);
    }

    [Fact(DisplayName = "A custom main root does not implicitly admit the old default content root")]
    public async Task OldRootNeedsGrant()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.ConfigurePermissions("knowledge");
        scenario.Context = scenario.Context with { MainRoot = "knowledge" };
        var target = WorkspaceLayout.DefaultMainRoot + "/memory/value.json";
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Set("value", target, "true")), Token);
        Assert.Equal(target, Assert.Single(plan.Permissions.Missing));
        Assert.NotNull((await scenario.Service.ApplyAsync(plan, Token)).Failure);
        Assert.False(File.Exists(scenario.Files.Combine(target)));
    }

    [Fact(DisplayName = "An empty config plan does not evaluate unrelated malformed workspace settings")]
    public async Task NoMutationsNeedNoPermissionRead()
    {
        using var scenario = new ToolServiceScenario();
        Directory.CreateDirectory(scenario.Files.Combine(WorkspaceSettingsDefinitions.DirectoryName));
        scenario.Files.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{");
        var plan = await scenario.Service.PlanAsync(scenario.Request(), Token);
        Assert.Equal(WorkspacePermissionDecision.NotRequired, plan.Permissions.Decision);
        Assert.Null((await scenario.Service.ApplyAsync(plan, Token)).Failure);
        Assert.Empty(scenario.Calls);
    }

    [Theory(DisplayName = "Malformed or linked workspace settings cannot be bypassed by per-invocation grants")]
    [InlineData(false), InlineData(true)]
    public async Task UnreadablePolicy(bool linked)
    {
        using var scenario = new ToolServiceScenario();
        using var outside = new ToolConfigFixture("foreign-policy");
        Directory.CreateDirectory(scenario.Files.Combine(WorkspaceSettingsDefinitions.DirectoryName));
        if (linked)
        {
            outside.Grant(Target);
            scenario.Files.CreateFileSymbolicLink(WorkspaceSettingsDefinitions.RelativePath, outside.Combine(WorkspaceSettingsDefinitions.RelativePath));
        }
        else scenario.Files.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{");
        scenario.Context = scenario.Context with { AdditionalAllowInstallPaths = [Target] };
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Set("value", Target, "true")), Token);
        Assert.Equal(WorkspacePermissionDecision.Required, plan.Permissions.Decision);
        Assert.Equal(Target, Assert.Single(plan.Permissions.Missing));
        Assert.NotNull((await scenario.Service.ApplyAsync(plan, Token)).Failure);
        Assert.False(File.Exists(scenario.Files.Combine(Target)));
    }

    [Fact(DisplayName = "A matching recorded no-op needs no destination grant")]
    public async Task SafeNoOp()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.WriteText(Target, "{\"value\":true}");
        var change = ToolServiceScenario.Set("value", Target, "true");
        var receipt = new ToolJsonValueAddition { Id = change.Id, Path = Target, Pointer = change.Pointer, Value = change.Value ?? throw new InvalidOperationException() };
        var plan = await scenario.Service.PlanAsync(scenario.Request(change) with { AppliedChanges = [receipt] }, Token);
        Assert.Equal(WorkspacePermissionDecision.NotRequired, plan.Permissions.Decision);
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.Null(result.Failure);
        Assert.Same(receipt, Assert.Single(result.RemainingAdditions));
        Assert.Empty(result.PendingBackups);
    }

    [Fact(DisplayName = "A denied file addition prevents an earlier native account setting command")]
    public async Task DeniedBeforeNativeEffect()
    {
        using var scenario = new ToolServiceScenario();
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Native(), ToolServiceScenario.Set("value", Target, "true")), Token);
        Assert.Equal(Target, Assert.Single(plan.Permissions.Missing));
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(scenario.Calls);
        Assert.Empty(result.CompletedChanges);
        Assert.Empty(result.NativeSettings);
        Assert.False(File.Exists(scenario.NativeFile));
    }

    [Fact(DisplayName = "Revocation during backup preparation keeps the backup and prevents the target write")]
    public async Task RevalidateAfterBackup()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.Grant(Target);
        scenario.Files.WriteText(Target, "{}");
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Set("value", Target, "true")), Token);
        scenario.Resolving = _ => scenario.Files.Grant();
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.CompletedChanges);
        var pending = Assert.Single(result.PendingBackups);
        Assert.False(pending.Verified);
        Assert.True(File.Exists(pending.Backup.BackupPath));
        Assert.Equal("{}"u8.ToArray(), await File.ReadAllBytesAsync(scenario.Files.Combine(Target), Token));
    }

}
