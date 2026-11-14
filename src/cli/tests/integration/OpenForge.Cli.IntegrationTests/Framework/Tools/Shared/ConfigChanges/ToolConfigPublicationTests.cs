using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Filesystem;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-config-publication"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolConfigPublicationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Retired recipes plan guarded removal before new additions and return the complete receipt set")]
    public async Task RetiredRecipe()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.WriteText("config.json", "{\"old\":true,\"keep\":42}");
        var old = new ToolJsonValueAddition { Id = "old", Path = "config.json", Pointer = "/old", Value = Element("true") };
        var request = scenario.Request(Json("new", "/new", "true")) with { AppliedChanges = [old] };
        var plan = await scenario.Service.PlanAsync(request, Token);
        Assert.Single(plan.Removals);
        Assert.Single(plan.Changes);
        using (var before = JsonDocument.Parse(await File.ReadAllBytesAsync(scenario.Files.Combine("config.json"), Token))) Assert.True(before.RootElement.GetProperty("old").GetBoolean());
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.Null(result.Failure);
        Assert.Equal("old", Assert.Single(result.RemovedChanges));
        Assert.Equal("new", Assert.Single(result.RemainingAdditions).Id);
        Assert.Single(result.PendingBackups);
        Assert.True(File.Exists(scenario.Files.Combine(ToolBackupPaths.Beside("config.json"))));
        using var document = JsonDocument.Parse(await File.ReadAllBytesAsync(scenario.Files.Combine("config.json"), Token));
        Assert.False(document.RootElement.TryGetProperty("old", out _));
        Assert.Equal(42, document.RootElement.GetProperty("keep").GetInt32());
        await scenario.CompleteAsync(result.PendingBackups);
        Assert.False(File.Exists(scenario.Files.Combine(ToolBackupPaths.Beside("config.json"))));
    }

    [Fact(DisplayName = "One-observation apply and remove calls keep untouched and unstarted receipts")]
    public async Task CompleteReplacementSets()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.WriteText("config.json", "{\"first\":true,\"second\":false}");
        var first = new ToolJsonValueAddition { Id = "first", Path = "config.json", Pointer = "/first", Value = Element("true") };
        var second = new ToolJsonValueAddition { Id = "second", Path = "config.json", Pointer = "/second", Value = Element("false") };
        var request = scenario.Request(Json("first", "/first", "true"), Json("second", "/second", "false")) with { AppliedChanges = [first, second] };
        var plan = await scenario.Service.PlanAsync(request, Token);
        var result = await scenario.Service.ApplyAsync(plan with { Changes = [plan.Changes[0]] }, Token);
        Assert.Null(result.Failure);
        Assert.Equal(new[] { first.Id, second.Id }, result.RemainingAdditions.Select(value => value.Id));
        var removal = await scenario.Service.PlanRemoveAsync(new()
        {
            ToolId = request.ToolId,
            Context = request.Context,
            Lock = new() { Version = "1.2.3", Channel = "synthetic", Changes = result.RemainingAdditions },
            NativeSettings = []
        }, Token);
        var refreshed = removal.Request.Lock with { Changes = [first with { Value = Element("true") }, second with { Value = Element("false") }] };
        var removed = await scenario.Service.RemoveAsync(removal with { Request = removal.Request with { Lock = refreshed }, Changes = [removal.Changes[0]] }, Token);
        Assert.Null(removed.Failure);
        Assert.Equal(second.Id, Assert.Single(removed.RemainingAdditions).Id);
        await scenario.CompleteAsync(removed.PendingBackups);
    }

    [Fact(DisplayName = "Removal drift after planning keeps the complete receipt set and unfamiliar bytes")]
    public async Task RemovalDrift()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.WriteText("config.json", "{\"value\":true}");
        var receipt = new ToolJsonValueAddition { Id = "value", Path = "config.json", Pointer = "/value", Value = Element("true") };
        var plan = await scenario.Service.PlanRemoveAsync(new()
        {
            ToolId = ToolServiceScenario.ToolId,
            Context = scenario.Context,
            Lock = new() { Version = "1.2.3", Channel = "synthetic", Changes = [receipt] },
            NativeSettings = []
        }, Token);
        await File.WriteAllTextAsync(scenario.Files.Combine("config.json"), "{\"foreign\":42}", Token);
        var result = await scenario.Service.RemoveAsync(plan, Token);
        Assert.NotNull(result.Failure);
        Assert.Same(receipt, Assert.Single(result.RemainingAdditions));
        Assert.Empty(result.RemovedChanges);
        Assert.Single(result.Unfinished);
    }

    [Fact(DisplayName = "Managed file templates expand strict UTF-8 content once")]
    public async Task ManagedTemplate()
    {
        using var scenario = new ToolServiceScenario();
        var change = new ToolManagedFileChange
        {
            Id = "template",
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.AbsentFile,
            Description = "synthetic template",
            PathTemplate = "${mainRoot}.txt",
            TemplateResource = "synthetic-resource"
        };
        var result = await scenario.Service.ApplyAsync(await scenario.Service.PlanAsync(scenario.Request(change), Token), Token);
        Assert.Null(result.Failure);
        Assert.Equal(scenario.Context.MainRoot + "\n", await File.ReadAllTextAsync(scenario.Files.Combine(scenario.Context.MainRoot + ".txt"), Token));
    }

    [Fact(DisplayName = "Effective coverage skips an unrecorded block and keeps its matching recorded receipt")]
    public async Task EffectiveCoverage()
    {
        using var scenario = new ToolServiceScenario();
        var host = new ToolGitTestHost(scenario.Files.Combine("git-home"));
        var initialized = await host.RunAsync(new()
        {
            ExecutablePath = ToolGitEnvironment.Executable,
            Arguments = ["init", "--quiet", "--template="],
            WorkingDirectory = scenario.Context.WorkspacePath,
            Environment = new Dictionary<string, string?>(),
            Timeout = TimeSpan.FromSeconds(10),
            MaximumOutputBytes = 4096
        }, Token);
        Assert.Equal(0, initialized.ExitCode);
        var service = new ToolConfigService(host, new ToolBackupService(host), scenario);
        var block = new ToolMarkedBlockChange
        {
            Id = "ignore",
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.UniqueMarkedBlock,
            Description = "synthetic ignore",
            PathTemplate = ".gitignore",
            StartMarker = "# synthetic begin",
            EndMarker = "# synthetic end",
            ContentTemplate = "generated/",
            WhenNotIgnored = new("generated/")
        };
        var first = await service.ApplyAsync(await service.PlanAsync(scenario.Request(block), Token), Token);
        Assert.Null(first.Failure);
        var receipt = Assert.Single(first.RemainingAdditions);
        var kept = await service.ApplyAsync(await service.PlanAsync(scenario.Request(block) with { AppliedChanges = [receipt] }, Token), Token);
        Assert.Null(kept.Failure);
        Assert.Same(receipt, Assert.Single(kept.RemainingAdditions));
        Assert.Empty(kept.PendingBackups);
        var unrecorded = await service.PlanAsync(scenario.Request(block with { Id = "other", PathTemplate = "other-ignore" }), Token);
        var skipped = await service.ApplyAsync(unrecorded, Token);
        Assert.Null(skipped.Failure);
        Assert.Empty(skipped.RemainingAdditions);
        Assert.False(File.Exists(scenario.Files.Combine("other-ignore")));
    }

    [Fact(DisplayName = "A stale retired-recipe plan cannot delete recreated data after its receipt was cleared")]
    public async Task ClearedReceiptReplay()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.WriteText("config.json", "{\"old\":true}");
        var receipt = new ToolJsonValueAddition { Id = "old", Path = "config.json", Pointer = "/old", Value = Element("true") };
        var plan = await scenario.Service.PlanAsync(scenario.Request() with { AppliedChanges = [receipt] }, Token);
        var first = await scenario.Service.ApplyAsync(plan, Token);
        Assert.Null(first.Failure);
        Assert.Empty(first.RemainingAdditions);
        await File.WriteAllTextAsync(scenario.Files.Combine("config.json"), "{\"old\":true}", Token);
        var replay = await scenario.Service.ApplyAsync(plan with { Request = plan.Request with { AppliedChanges = first.RemainingAdditions } }, Token);
        Assert.NotNull(replay.Failure);
        Assert.Empty(replay.RemovedChanges);
        Assert.Empty(replay.PendingBackups);
        using var current = JsonDocument.Parse(await File.ReadAllBytesAsync(scenario.Files.Combine("config.json"), Token));
        Assert.True(current.RootElement.GetProperty("old").GetBoolean());
    }

    [Theory(DisplayName = "Moving a recorded set file or block requires its original applied content")]
    [InlineData("set"), InlineData("file"), InlineData("block")]
    public async Task TargetMigrationDrift(string kind)
    {
        using var scenario = new ToolServiceScenario();
        ToolConfigChange change = kind switch
        {
            "set" => Json("move", "/value", "true"),
            "file" => new ToolManagedFileChange
            {
                Id = "move",
                Scope = ToolEffectScope.Workspace,
                InitialState = ToolInitialConfigState.AbsentFile,
                Description = "synthetic file",
                PathTemplate = "config.json",
                TemplateResource = "synthetic-resource"
            },
            "block" => new ToolMarkedBlockChange
            {
                Id = "move",
                Scope = ToolEffectScope.Workspace,
                InitialState = ToolInitialConfigState.UniqueMarkedBlock,
                Description = "synthetic block",
                PathTemplate = "config.json",
                StartMarker = "# synthetic start",
                EndMarker = "# synthetic end",
                ContentTemplate = "managed"
            },
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        var first = await scenario.Service.ApplyAsync(await scenario.Service.PlanAsync(scenario.Request(change), Token), Token);
        Assert.Null(first.Failure);
        var moved = change switch
        {
            ToolJsonValueChange json => (ToolConfigChange)(json with { PathTemplate = "moved.json" }),
            ToolManagedFileChange file => file with { PathTemplate = "moved.json" },
            ToolMarkedBlockChange block => block with { PathTemplate = "moved.json" },
            _ => throw new ArgumentOutOfRangeException(nameof(change)),
        };
        if (change is ToolMarkedBlockChange blockChange)
            await File.WriteAllTextAsync(scenario.Files.Combine("config.json"), blockChange.StartMarker + "\ndrift\n" + blockChange.EndMarker + "\n", Token);
        else if (kind == "set") await File.WriteAllTextAsync(scenario.Files.Combine("config.json"), "{}", Token);
        else File.Delete(scenario.Files.Combine("config.json"));
        var plan = await scenario.Service.PlanAsync(scenario.Request(moved) with { AppliedChanges = first.RemainingAdditions }, Token);
        Assert.Equal(ToolChangeDisposition.Conflict, Assert.Single(plan.Removals).Disposition);
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.NotNull(result.Failure);
        Assert.Single(result.RemainingAdditions);
        Assert.Empty(result.RemovedChanges);
        Assert.False(File.Exists(scenario.Files.Combine("moved.json")));
    }

    [Fact(DisplayName = "Failed backup preparation returns its retained handle unverified without changing or discarding files")]
    public async Task RetainedFailedPreparation()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.WriteText("config.json", "{}");
        var target = scenario.Files.Combine("config.json");
        var backup = await scenario.Backups.PrepareAsync(new(target), Token);
        Assert.Null(backup.Failure);
        var backupPath = Assert.IsType<string>(backup.BackupPath);
        var plan = await scenario.Service.PlanAsync(scenario.Request(Json("value", "/value", "true")), Token);
        await File.WriteAllTextAsync(backupPath, "synthetic changed backup", Token);
        var savedBytes = await File.ReadAllBytesAsync(backupPath, Token);
        var targetBytes = await File.ReadAllBytesAsync(target, Token);
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.NotNull(result.Failure);
        var pending = Assert.Single(result.PendingBackups);
        Assert.False(pending.Verified);
        Assert.Null(pending.ExpectedAppliedSha256);
        Assert.NotNull(pending.Backup.Failure);
        Assert.Equal(ToolBackupKind.Sibling, pending.Backup.Kind);
        Assert.Equal(backupPath, pending.Backup.BackupPath);
        Assert.Equal(target, pending.Backup.TargetPath);
        Assert.Empty(result.CompletedChanges);
        Assert.Empty(result.RemainingAdditions);
        Assert.Single(result.Unfinished);
        Assert.Equal(targetBytes, await File.ReadAllBytesAsync(target, Token));
        Assert.Equal(savedBytes, await File.ReadAllBytesAsync(backupPath, Token));
    }

    private static ToolJsonValueChange Json(string id, string pointer, string value) => new()
    {
        Id = id,
        Scope = ToolEffectScope.Workspace,
        InitialState = ToolInitialConfigState.AbsentMember,
        Description = "synthetic JSON",
        PathTemplate = "config.json",
        Pointer = pointer,
        Operation = ToolJsonValueOperation.SetIfAbsent,
        Value = Element(value)
    };
    private static JsonElement Element(string value) { using var document = JsonDocument.Parse(value); return document.RootElement.Clone(); }
}
