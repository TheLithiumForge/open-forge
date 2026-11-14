using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;
using OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using System.Text;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-ignore-coverage"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolEffectiveIgnoreTests
{
    private const string ProbeName = "synthetic-coverage-probe";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Effective-ignore conditions reject linked directories before invoking Git")]
    public async Task LinkedDirectory()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Ignored = true;
        Directory.CreateDirectory(scenario.Files.Combine("elsewhere"));
        Directory.CreateSymbolicLink(scenario.Files.Combine("generated"), scenario.Files.Combine("elsewhere"));
        await Assert.ThrowsAsync<IOException>(() => new ToolEffectiveIgnore(scenario).IsIgnoredAsync(scenario.Context, "generated/", TestContext.Current.CancellationToken));
        Assert.Empty(scenario.Calls);
    }

    [Theory(DisplayName = "Effective coverage checks directory data through root rules and native internal ignore files")]
    [InlineData(false, false), InlineData(true, false), InlineData(true, true)]
    public async Task DirectoryCoverage(bool internalIgnore, bool keepIgnoreFile)
    {
        using var scenario = new ToolServiceScenario();
        var host = new ToolGitTestHost(scenario.Files.Combine("git-home"));
        var initialized = await GitAsync(host, scenario, ["init", "--quiet", "--template="]);
        Assert.Equal(0, initialized.ExitCode);
        var check = new ToolEffectiveIgnore(host);
        Assert.False(await check.IsIgnoredAsync(scenario.Context, "generated/", TestContext.Current.CancellationToken));
        if (internalIgnore)
        {
            Directory.CreateDirectory(scenario.Files.Combine("generated"));
            await File.WriteAllTextAsync(scenario.Files.Combine($"generated/{ToolEffectiveIgnore.IgnoreFileName}"), keepIgnoreFile ? $"*\n!{ToolEffectiveIgnore.IgnoreFileName}\n" : "*\n", TestContext.Current.CancellationToken);
        }
        else scenario.Files.WriteText(ToolEffectiveIgnore.IgnoreFileName, "generated/\n");
        Assert.True(await check.IsIgnoredAsync(scenario.Context, "generated/", TestContext.Current.CancellationToken));
        Assert.False(File.Exists(scenario.Files.Combine("generated/" + ProbeName)));
    }

    [Theory(DisplayName = "A probe-only root rule or partial internal ignore does not cover directory data")]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3)]
    public async Task NarrowCoverage(int mode)
    {
        using var scenario = new ToolServiceScenario();
        var host = new ToolGitTestHost(scenario.Files.Combine("git-home"));
        Assert.Equal(0, (await GitAsync(host, scenario, ["init", "--quiet", "--template="])).ExitCode);
        Directory.CreateDirectory(scenario.Files.Combine("generated"));
        scenario.Files.WriteText("generated/index-data", "synthetic data");
        var ignorePath = mode is 0 or 3 ? ToolEffectiveIgnore.IgnoreFileName : $"generated/{ToolEffectiveIgnore.IgnoreFileName}";
        var patterns = mode switch
        {
            2 => "*\n!index-data\n",
            3 => "generated/*\n!generated/index-data\n",
            _ => "*coverage-probe\n!index-data\n"
        };
        var original = Encoding.UTF8.GetBytes(patterns);
        await File.WriteAllBytesAsync(scenario.Files.Combine(ignorePath), original, Token);
        Assert.Equal(0, (await GitAsync(host, scenario, ["check-ignore", "--no-index", "--quiet", "--", "generated/" + ProbeName])).ExitCode);
        Assert.Equal(1, (await GitAsync(host, scenario, ["check-ignore", "--no-index", "--quiet", "--", "generated/index-data"])).ExitCode);
        Assert.False(await new ToolEffectiveIgnore(host).IsIgnoredAsync(scenario.Context, "generated/", Token));
        var service = new ToolConfigService(host, new ToolBackupService(host), scenario);
        var plan = await service.PlanAsync(scenario.Request(Block()), Token);
        Assert.Equal(ToolChangeDisposition.Add, Assert.Single(plan.Changes).Disposition);
        var applied = await service.ApplyAsync(plan, Token);
        Assert.Null(applied.Failure);
        Assert.Single(applied.RemainingAdditions);
        Assert.Equal(0, (await GitAsync(host, scenario, ["check-ignore", "--no-index", "--quiet", "--", "generated/index-data"])).ExitCode);
        if (mode is 1 or 2) Assert.Equal(original, await File.ReadAllBytesAsync(scenario.Files.Combine(ignorePath), Token));
    }

    [Theory(DisplayName = "Conditional blocks defeat later negations or conflict without changing an unterminated final line")]
    [InlineData(false), InlineData(true)]
    public async Task LaterNegation(bool terminated)
    {
        using var scenario = new ToolServiceScenario();
        var host = new ToolGitTestHost(scenario.Files.Combine("git-home"));
        Assert.Equal(0, (await GitAsync(host, scenario, ["init", "--quiet", "--template="])).ExitCode);
        var original = Encoding.UTF8.GetBytes("# synthetic original\r\n!generated/" + (terminated ? "\r\n" : ""));
        await File.WriteAllBytesAsync(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName), original, Token);
        var service = new ToolConfigService(host, new ToolBackupService(host), scenario);
        var plan = await service.PlanAsync(scenario.Request(Block()), Token);
        Assert.Equal(original, await File.ReadAllBytesAsync(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName), Token));
        var result = await service.ApplyAsync(plan, Token);
        if (!terminated)
        {
            Assert.Equal(ToolChangeDisposition.Conflict, Assert.Single(plan.Changes).Disposition);
            Assert.NotNull(result.Failure);
            Assert.Empty(result.RemainingAdditions);
            Assert.Empty(result.PendingBackups);
            Assert.Equal(original, await File.ReadAllBytesAsync(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName), Token));
            return;
        }
        Assert.Null(result.Failure);
        Assert.Single(result.RemainingAdditions);
        Assert.True(await new ToolEffectiveIgnore(host).IsIgnoredAsync(scenario.Context, "generated/", Token));
        var applied = await File.ReadAllBytesAsync(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName), Token);
        Assert.True(applied.AsSpan().StartsWith(original));
        var removed = await RemoveAsync(service, scenario, result.RemainingAdditions);
        Assert.Null(removed.Failure);
        Assert.Empty(removed.RemainingAdditions);
        Assert.Equal(original, await File.ReadAllBytesAsync(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName), Token));
    }

    [Fact(DisplayName = "A conditional block keeps an unrelated unterminated final line byte for byte through removal")]
    public async Task UnterminatedUnrelatedLine()
    {
        using var scenario = new ToolServiceScenario();
        var host = new ToolGitTestHost(scenario.Files.Combine("git-home"));
        Assert.Equal(0, (await GitAsync(host, scenario, ["init", "--quiet", "--template="])).ExitCode);
        var original = Encoding.UTF8.GetBytes("# synthetic final line");
        await File.WriteAllBytesAsync(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName), original, Token);
        var service = new ToolConfigService(host, new ToolBackupService(host), scenario);
        var result = await service.ApplyAsync(await service.PlanAsync(scenario.Request(Block()), Token), Token);
        Assert.Null(result.Failure);
        Assert.True(await new ToolEffectiveIgnore(host).IsIgnoredAsync(scenario.Context, "generated/", Token));
        Assert.True((await File.ReadAllBytesAsync(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName), Token)).AsSpan().EndsWith(original));
        Assert.Null((await RemoveAsync(service, scenario, result.RemainingAdditions)).Failure);
        Assert.Equal(original, await File.ReadAllBytesAsync(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName), Token));
    }

    [Theory(DisplayName = "An ineffective recorded block migrates to effective placement or conflicts while keeping its receipt")]
    [InlineData(false), InlineData(true)]
    public async Task IneffectiveReapply(bool terminated)
    {
        using var scenario = new ToolServiceScenario();
        var host = new ToolGitTestHost(scenario.Files.Combine("git-home"));
        Assert.Equal(0, (await GitAsync(host, scenario, ["init", "--quiet", "--template="])).ExitCode);
        var change = Block();
        var block = ToolMarkedBlocks.Render(change.StartMarker, change.EndMarker, change.ContentTemplate);
        var original = Encoding.UTF8.GetBytes("!generated/" + (terminated ? "\n" : ""));
        var before = block.Concat(original).ToArray();
        await File.WriteAllBytesAsync(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName), before, Token);
        var receipt = new ToolMarkedBlockAddition
        {
            Id = change.Id,
            Path = change.PathTemplate,
            StartMarker = change.StartMarker,
            EndMarker = change.EndMarker,
            ContentSha256 = FileExpectation.Hash(block)
        };
        var service = new ToolConfigService(host, new ToolBackupService(host), scenario);
        var plan = await service.PlanAsync(scenario.Request(change) with { AppliedChanges = [receipt] }, Token);
        var result = await service.ApplyAsync(plan, Token);
        Assert.Single(result.RemainingAdditions);
        if (terminated)
        {
            Assert.Equal(ToolChangeDisposition.Migrate, Assert.Single(plan.Changes).Disposition);
            Assert.Null(result.Failure);
            Assert.True(await new ToolEffectiveIgnore(host).IsIgnoredAsync(scenario.Context, "generated/", Token));
            Assert.Null((await RemoveAsync(service, scenario, result.RemainingAdditions)).Failure);
            Assert.Equal(original, await File.ReadAllBytesAsync(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName), Token));
        }
        else
        {
            Assert.Equal(ToolChangeDisposition.Conflict, Assert.Single(plan.Changes).Disposition);
            Assert.NotNull(result.Failure);
            Assert.Empty(result.CompletedChanges);
            Assert.Equal(before, await File.ReadAllBytesAsync(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName), Token));
        }
    }

    [Fact(DisplayName = "A supported edit that does not cover its condition conflicts during preflight")]
    public async Task IneffectiveRecipe()
    {
        using var scenario = new ToolServiceScenario();
        var host = new ToolGitTestHost(scenario.Files.Combine("git-home"));
        Assert.Equal(0, (await GitAsync(host, scenario, ["init", "--quiet", "--template="])).ExitCode);
        var service = new ToolConfigService(host, new ToolBackupService(host), scenario);
        var plan = await service.PlanAsync(scenario.Request(Block() with { ContentTemplate = "other/" }), Token);
        Assert.Equal(ToolChangeDisposition.Conflict, Assert.Single(plan.Changes).Disposition);
        var result = await service.ApplyAsync(plan, Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.RemainingAdditions);
        Assert.Empty(result.PendingBackups);
        Assert.False(File.Exists(scenario.Files.Combine(ToolEffectiveIgnore.IgnoreFileName)));
    }

    private static async Task<ToolConfigRemoveResult> RemoveAsync(ToolConfigService service, ToolServiceScenario scenario, IReadOnlyList<ToolAppliedChange> additions)
    {
        var plan = await service.PlanRemoveAsync(new()
        {
            ToolId = ToolServiceScenario.ToolId,
            Context = scenario.Context,
            Lock = new() { Version = "1.2.3", Channel = "synthetic", Changes = additions },
            NativeSettings = []
        }, Token);
        return await service.RemoveAsync(plan, Token);
    }

    private static ToolMarkedBlockChange Block() => new()
    {
        Id = "synthetic-ignore",
        Scope = ToolEffectScope.Workspace,
        InitialState = ToolInitialConfigState.UniqueMarkedBlock,
        Description = "synthetic data exclusion",
        PathTemplate = ToolEffectiveIgnore.IgnoreFileName,
        StartMarker = "# synthetic start",
        EndMarker = "# synthetic end",
        ContentTemplate = "generated/",
        WhenNotIgnored = new("generated/")
    };

    private static Task<ToolProcessResult> GitAsync(ToolGitTestHost host, ToolServiceScenario scenario, string[] arguments)
        => host.RunAsync(new()
        {
            ExecutablePath = ToolGitEnvironment.Executable,
            Arguments = arguments,
            WorkingDirectory = scenario.Context.WorkspacePath,
            Environment = new Dictionary<string, string?>(),
            Timeout = TimeSpan.FromSeconds(10),
            MaximumOutputBytes = 4096
        }, Token);
}
