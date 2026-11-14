using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-config"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolConfigServiceTests
{
    private const string Target = "synthetic.json";
    private const string Other = "second.json";
    private const string ToolId = "synthetic-tool";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Fresh apply plans sequential edits without effects and preserves unrelated bytes")]
    public async Task FreshApply()
    {
        using var temporary = new ToolConfigFixture("tool-config");
        temporary.WriteText(Target, "{\"keep\": [ 1, 2 ],\"items\": [\"existing\"]}\n");
        var host = new NoGitHost();
        var service = Service(host);
        var request = Request(temporary, [Json("first", Target, "/items", "[\"existing\",\"added\"]"), Json("second", Target, "/enabled", "true", set: true)]);
        var prior = await File.ReadAllBytesAsync(temporary.Combine(Target), Token);
        var plan = await service.PlanAsync(request, Token);
        Assert.Equal(prior, await File.ReadAllBytesAsync(temporary.Combine(Target), Token));
        Assert.False(File.Exists(temporary.Combine(ToolBackupPaths.Beside(Target))));
        var applied = await service.ApplyAsync(plan, Token);
        Assert.Null(applied.Failure);
        Assert.Empty(applied.Unfinished);
        Assert.Equal(2, applied.RemainingAdditions.Count);
        var union = Assert.IsType<ToolJsonUnionAddition>(applied.RemainingAdditions[0]);
        Assert.Equal("added", Assert.Single(union.Items).GetString());
        var expected = Encoding.UTF8.GetBytes("{\"keep\": [ 1, 2 ],\"items\": [\"existing\",\"added\"],\"enabled\":true}\n");
        Assert.Equal(expected, await File.ReadAllBytesAsync(temporary.Combine(Target), Token));
        Assert.True(File.Exists(temporary.Combine(ToolBackupPaths.Beside(Target))));
        Assert.Single(applied.PendingBackups);
        Assert.False(Directory.Exists(temporary.Combine(TemporaryWorkspace.RuntimeDirectoryName)));
        var reapply = await service.ApplyAsync(await service.PlanAsync(request with { AppliedChanges = applied.RemainingAdditions }, Token), Token);
        Assert.Null(reapply.Failure);
        Assert.Equal(expected, await File.ReadAllBytesAsync(temporary.Combine(Target), Token));
        var removed = await RemoveAsync(service, Remove(temporary, applied.RemainingAdditions));
        Assert.Null(removed.Failure);
        Assert.Equal(prior, await File.ReadAllBytesAsync(temporary.Combine(Target), Token));
    }

    [Fact(DisplayName = "Drift after planning stops replacement and keeps unfamiliar bytes")]
    public async Task PlannedStateChanged()
    {
        using var temporary = new ToolConfigFixture("tool-config-drift");
        temporary.WriteText(Target, "{}");
        var service = Service();
        var plan = await service.PlanAsync(Request(temporary, [Json("json", Target, "/items", "[1]")]), Token);
        var changed = Encoding.UTF8.GetBytes("{\"foreign\":true}");
        await File.WriteAllBytesAsync(temporary.Combine(Target), changed, Token);
        var result = await service.ApplyAsync(plan, Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.RemainingAdditions);
        Assert.Single(result.Unfinished);
        Assert.Equal(changed, await File.ReadAllBytesAsync(temporary.Combine(Target), Token));
    }

    [Fact(DisplayName = "A later effect failure returns earlier verified receipts and retains their original backups")]
    public async Task PartialCompletion()
    {
        using var temporary = new ToolConfigFixture("tool-config-partial");
        temporary.WriteText(Target, "{}");
        temporary.WriteText(Other, "{}");
        var service = Service();
        var plan = await service.PlanAsync(Request(temporary, [Json("first", Target, "/items", "[1]"), Json("second", Other, "/items", "[2]")]), Token);
        await File.WriteAllTextAsync(temporary.Combine(Other), "{\"other\":true}", Token);
        var result = await service.ApplyAsync(plan, Token);
        Assert.NotNull(result.Failure);
        Assert.Equal("first", Assert.Single(result.RemainingAdditions).Id);
        Assert.Equal("second", Assert.Single(result.Unfinished));
        Assert.Equal(Encoding.UTF8.GetBytes("{}"), await File.ReadAllBytesAsync(temporary.Combine(ToolBackupPaths.Beside(Target)), Token));
        using var applied = JsonDocument.Parse(await File.ReadAllBytesAsync(temporary.Combine(Target), Token));
        Assert.Equal(1, applied.RootElement.GetProperty("items")[0].GetInt32());
    }

    [Fact(DisplayName = "A conflicting preflight performs none of the otherwise valid planned additions")]
    public async Task ConflictingPreflight()
    {
        using var temporary = new ToolConfigFixture("tool-config-conflict");
        temporary.WriteText(Target, "{\"present\":null}");
        var service = Service();
        var plan = await service.PlanAsync(Request(temporary, [Json("first", Target, "/items", "[1]"), Json("second", Target, "/present", "true", set: true)]), Token);
        Assert.Equal(ToolChangeDisposition.Conflict, plan.Changes[1].Disposition);
        var result = await service.ApplyAsync(plan, Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.RemainingAdditions);
        using var current = JsonDocument.Parse(await File.ReadAllBytesAsync(temporary.Combine(Target), Token));
        Assert.False(current.RootElement.TryGetProperty("items", out _));
        Assert.False(File.Exists(temporary.Combine(ToolBackupPaths.Beside(Target))));
    }

    [Fact(DisplayName = "Literal managed files apply migrate verify and remove through their recorded hash")]
    public async Task ManagedFileLifecycle()
    {
        using var temporary = new ToolConfigFixture("tool-config-file");
        var template = Encoding.UTF8.GetBytes("first\n");
        var service = Service(readTemplate: _ => template);
        var change = new ToolManagedFileChange
        {
            Id = "file",
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.AbsentFile,
            Description = "synthetic file",
            PathTemplate = Target,
            TemplateResource = "synthetic-resource"
        };
        var request = Request(temporary, [change]);
        var result = await service.ApplyAsync(await service.PlanAsync(request, Token), Token);
        Assert.Null(result.Failure);
        Assert.Equal(template, await File.ReadAllBytesAsync(temporary.Combine(Target), Token));
        var unrecorded = await service.PlanAsync(request, Token);
        Assert.Equal(ToolChangeDisposition.Conflict, Assert.Single(unrecorded.Changes).Disposition);
        request = request with { AppliedChanges = result.RemainingAdditions };
        var same = await service.PlanAsync(request, Token);
        Assert.Equal(ToolChangeDisposition.NoChange, Assert.Single(same.Changes).Disposition);
        template = Encoding.UTF8.GetBytes("second\n");
        var migration = await service.ApplyAsync(await service.PlanAsync(request, Token), Token);
        Assert.Null(migration.Failure);
        Assert.Equal(template, await File.ReadAllBytesAsync(temporary.Combine(Target), Token));
        var removal = await RemoveAsync(service, Remove(temporary, migration.RemainingAdditions));
        Assert.Null(removal.Failure);
        Assert.False(File.Exists(temporary.Combine(Target)));
    }

    [Fact(DisplayName = "Changed managed files keep their receipt and bytes during project removal")]
    public async Task ChangedManagedFile()
    {
        using var temporary = new ToolConfigFixture("tool-config-file-drift");
        var bytes = Encoding.UTF8.GetBytes("managed");
        var service = Service(readTemplate: _ => bytes);
        var change = new ToolManagedFileChange
        {
            Id = "file",
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.AbsentFile,
            Description = "synthetic file",
            PathTemplate = Target,
            TemplateResource = "synthetic-resource"
        };
        var result = await service.ApplyAsync(await service.PlanAsync(Request(temporary, [change]), Token), Token);
        Assert.Null(result.Failure);
        var changed = Encoding.UTF8.GetBytes("foreign");
        await File.WriteAllBytesAsync(temporary.Combine(Target), changed, Token);
        var removal = await RemoveAsync(service, Remove(temporary, result.RemainingAdditions));
        Assert.NotNull(removal.Failure);
        Assert.Single(removal.RemainingAdditions);
        Assert.Equal(changed, await File.ReadAllBytesAsync(temporary.Combine(Target), Token));
    }

    [Fact(DisplayName = "Block removal deletes only the uniquely marked region including changed interior")]
    public async Task BlockRemoval()
    {
        using var temporary = new ToolConfigFixture("tool-config-block");
        temporary.WriteText(Target, "unrelated\r\n");
        var service = Service();
        var block = new ToolMarkedBlockChange
        {
            Id = "block",
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.UniqueMarkedBlock,
            Description = "synthetic block",
            PathTemplate = Target,
            StartMarker = "# fixture start",
            EndMarker = "# fixture end",
            ContentTemplate = "managed"
        };
        var result = await service.ApplyAsync(await service.PlanAsync(Request(temporary, [block]), Token), Token);
        Assert.Null(result.Failure);
        await File.WriteAllTextAsync(temporary.Combine(Target), block.StartMarker + "\nchanged\n" + block.EndMarker + "\nunrelated\r\n", Token);
        var removal = await RemoveAsync(service, Remove(temporary, result.RemainingAdditions));
        Assert.Null(removal.Failure);
        Assert.Equal(Encoding.UTF8.GetBytes("unrelated\r\n"), await File.ReadAllBytesAsync(temporary.Combine(Target), Token));
        var repeated = await RemoveAsync(service, Remove(temporary, []));
        Assert.Null(repeated.Failure);
        Assert.Empty(repeated.RemovedChanges);
    }

    [Theory(DisplayName = "Project removal keeps account settings without invoking a restore whether current matched or drifted")]
    [InlineData(true), InlineData(false)]
    public async Task KeepsNativeSettings(bool matched)
    {
        using var temporary = new ToolConfigFixture("tool-config-native");
        var host = new NoGitHost(matched ? "off" : "changed");
        var service = Service(host);
        var setting = new ToolNativeSettingRecord
        {
            ToolId = ToolId,
            ChangeId = "native",
            NativeHomePath = temporary.Path,
            Previous = Native("on"),
            Applied = Native("off"),
            ManualRestoreArguments = ["config", "on"]
        };
        var receipt = new ToolNativeCommandAddition { Id = setting.ChangeId, AllowListId = "synthetic-native-setting" };
        var request = Remove(temporary, [receipt]) with { NativeSettings = [setting] };
        var removal = await RemoveAsync(service, request);
        Assert.Null(removal.Failure);
        Assert.Empty(removal.RemainingAdditions);
        Assert.Equal(receipt.Id, Assert.Single(removal.RemovedChanges));
        Assert.Same(setting, Assert.Single(removal.KeptNativeSettings));
        Assert.Empty(host.Calls);
    }

    [Fact(DisplayName = "A link target cannot redirect a configuration write outside its workspace")]
    public async Task RejectsLinkedLeaf()
    {
        using var temporary = new ToolConfigFixture("tool-config-link");
        temporary.WriteText(Other, "{}");
        temporary.CreateFileSymbolicLink(Target, temporary.Combine(Other));
        var service = Service();
        var plan = await service.PlanAsync(Request(temporary, [Json("json", Target, "/items", "[1]")]), Token);
        Assert.Equal(ToolChangeDisposition.Conflict, Assert.Single(plan.Changes).Disposition);
        Assert.NotNull((await service.ApplyAsync(plan, Token)).Failure);
        Assert.Equal(Encoding.UTF8.GetBytes("{}"), await File.ReadAllBytesAsync(temporary.Combine(Other), Token));
    }

    [Fact(DisplayName = "Cancellation retains completed receipts and unstarted work without creating runtime state")]
    public async Task Cancellation()
    {
        using var temporary = new ToolConfigFixture("tool-config-cancel");
        var service = Service();
        var plan = await service.PlanAsync(Request(temporary, [Json("json", Target, "/items", "[1]")]), Token);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var result = await service.ApplyAsync(plan, cancelled.Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.RemainingAdditions);
        Assert.Single(result.Unfinished);
        Assert.False(File.Exists(temporary.Combine(Target)));
    }

    private static ToolConfigRequest Request(ToolConfigFixture temporary, IReadOnlyList<ToolConfigChange> changes) => new()
    { ToolId = ToolId, Context = Context(temporary), ProfileDigest = "synthetic-digest", Changes = changes, AppliedChanges = [], NativeSettings = [] };
    private static ToolConfigRemoveRequest Remove(ToolConfigFixture temporary, IReadOnlyList<ToolAppliedChange> changes) => new()
    { ToolId = ToolId, Context = Context(temporary), Lock = new() { Version = "1.2.3", Channel = "synthetic-channel", Changes = changes }, NativeSettings = [] };
    private static ToolRecordedValue Native(string value) => new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = value };
    private static ToolJsonValueChange Json(string id, string path, string pointer, string value, bool set = false)
    {
        using var document = JsonDocument.Parse(value);
        return new()
        {
            Id = id,
            Scope = ToolEffectScope.Workspace,
            InitialState = set ? ToolInitialConfigState.AbsentMember : ToolInitialConfigState.JsonArrayOrMissing,
            Description = "synthetic JSON",
            PathTemplate = path,
            Pointer = pointer,
            Operation = set ? ToolJsonValueOperation.SetIfAbsent : ToolJsonValueOperation.Union,
            Value = document.RootElement.Clone()
        };
    }

    // Native tools and ambient Git are excluded from config tests. Backup Git behavior has real-process tests.
    private static ToolConfigService Service(NoGitHost? host = null, Func<string, byte[]>? readTemplate = null)
    {
        host ??= new NoGitHost();
        return new(host, new ToolBackupService(host), host, readTemplate ?? (_ => throw new InvalidOperationException()));
    }

    private static async Task<ToolConfigRemoveResult> RemoveAsync(ToolConfigService service, ToolConfigRemoveRequest request)
        => await service.RemoveAsync(await service.PlanRemoveAsync(request, Token), Token);

    private static ToolRecipeContext Context(ToolConfigFixture temporary) => new()
    {
        WorkspacePath = temporary.Path,
        HomePath = temporary.Combine("home"),
        BinPath = temporary.Combine("home/bin"),
        NativeHomePath = temporary.Path,
        NativeInstallDirectory = temporary.Combine("home/bin"),
        MainRoot = "synthetic-context",
        ClientRoots = new Dictionary<string, string>(),
    };

    private sealed class NoGitHost(string currentNative = "off") : IToolProcessHost, IToolNativePlanReader
    {
        internal List<ToolProcessRequest> Calls { get; } = [];
        public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken) => Task.FromResult<ToolResolvedCommand?>(null);
        public ToolNativePlanObservation Read(ToolNativePlanParser parser, ToolProcessResult result) => throw new InvalidOperationException("No inventory is used by this fixture.");
        public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
        {
            Calls.Add(request);
            return Task.FromResult(new ToolProcessResult { ExitCode = 1, TimedOut = false, Cancelled = false, StandardOutput = currentNative, StandardError = string.Empty });
        }
    }
}
