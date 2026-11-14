using OpenForge.Cli.Composition.Shared.Tools.Path;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.Tools.Path;

[Trait("Feature", "tool-path"), Trait("Evidence", "Integration")]
public sealed class ToolPathHostTests
{
    [Theory(DisplayName = "PATH edits require both a selected choice and live confirmation")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task KeepsUnattendedPath(bool chosen, bool live)
    {
        using var temporary = TemporaryWorkspace.Create("tool-path");
        var environment = new PathEnvironment(windows: true);
        var host = new ToolPathHost(environment, temporary.Path, new Backups(), () => live);
        var facts = await host.ObserveAsync(temporary.Path, TestContext.Current.CancellationToken);
        var result = await host.AddAsync(new() { Facts = facts, Choice = chosen ? ToolPathChoice.AddForMe : ToolPathChoice.Manual, ChosenShellFiles = [] }, TestContext.Current.CancellationToken);
        Assert.Equal(0, environment.Writes);
        Assert.NotEmpty(result.ManualLines);
    }

    [Fact(DisplayName = "Windows PATH adds and removes only its entry through the injected user environment")]
    public async Task ChangesOnlyUserEntry()
    {
        using var temporary = TemporaryWorkspace.Create("tool-path");
        var existing = temporary.Combine("existing");
        var later = temporary.Combine("later");
        var environment = new PathEnvironment(windows: true) { Stored = existing };
        var host = new ToolPathHost(environment, temporary.Path, new Backups(), () => true);
        var facts = await host.ObserveAsync(temporary.Path, TestContext.Current.CancellationToken);
        var result = await host.AddAsync(new() { Facts = facts, Choice = ToolPathChoice.AddForMe, ChosenShellFiles = [] }, TestContext.Current.CancellationToken);
        Assert.Null(result.Failure);
        Assert.NotNull(result.State);
        Assert.True(ToolPathPolicy.Contains(environment.Stored, facts.BinPath, windows: true));
        environment.Stored += $";{later}";
        var removal = await host.RemoveAsync(new(result.State), TestContext.Current.CancellationToken);
        Assert.Null(removal.Failure);
        Assert.Equal($"{existing};{later}", environment.Stored);
        Assert.Equal(2, environment.Writes);
        Assert.Single(removal.RemovedTargets);
    }

    [Fact(DisplayName = "Unix PATH writes only chosen files and keeps unrelated bytes through removal")]
    public async Task ChangesChosenShellFiles()
    {
        using var temporary = TemporaryWorkspace.Create("tool-path");
        var selected = temporary.CreateFile("chosen-profile", "# before without newline");
        var untouched = temporary.CreateFile(ToolPathPolicy.StartupFiles[0].AbsolutePath, "# untouched\n");
        var before = await File.ReadAllBytesAsync(selected, TestContext.Current.CancellationToken);
        var untouchedBefore = await File.ReadAllBytesAsync(untouched, TestContext.Current.CancellationToken);
        var backups = new Backups();
        var host = new ToolPathHost(new PathEnvironment(windows: false), temporary.Path, backups, () => true);
        var facts = await host.ObserveAsync(temporary.Path, TestContext.Current.CancellationToken);
        Assert.Contains(facts.DetectedShellFiles, file => file.AbsolutePath == untouched);
        var result = await host.AddAsync(new() { Facts = facts, Choice = ToolPathChoice.AddForMe, ChosenShellFiles = [new(selected, ToolShellKind.Posix)] }, TestContext.Current.CancellationToken);
        Assert.Null(result.Failure);
        Assert.NotNull(result.State);
        Assert.Single(result.State.Changes);
        Assert.False(result.State.Changes[0].Prior.Present);
        Assert.Equal(untouchedBefore, await File.ReadAllBytesAsync(untouched, TestContext.Current.CancellationToken));
        var removal = await host.RemoveAsync(new(result.State), TestContext.Current.CancellationToken);
        Assert.Null(removal.Failure);
        Assert.Empty(removal.KeptTargets);
        Assert.Equal(before, await File.ReadAllBytesAsync(selected, TestContext.Current.CancellationToken));
        Assert.Equal(2, backups.Prepared);
        Assert.Equal(0, backups.Completed);
        Assert.True(Assert.Single(result.PendingBackups).Verified);
        Assert.True(Assert.Single(removal.PendingBackups).Verified);
        Assert.Equal(selected, result.PendingBackups[0].Backup.TargetPath);
        Assert.NotEqual(result.PendingBackups[0].ExpectedAppliedSha256, removal.PendingBackups[0].ExpectedAppliedSha256);
    }

    [Fact(DisplayName = "Changed shell PATH blocks are kept")]
    public async Task KeepsChangedBlock()
    {
        using var temporary = TemporaryWorkspace.Create("tool-path");
        var selected = temporary.CreateFile("custom-profile");
        File.Delete(selected);
        var host = new ToolPathHost(new PathEnvironment(windows: false), temporary.Path, new Backups(), () => true);
        var facts = await host.ObserveAsync(temporary.Path, TestContext.Current.CancellationToken);
        var result = await host.AddAsync(new() { Facts = facts, Choice = ToolPathChoice.AddForMe, ChosenShellFiles = [new(selected, ToolShellKind.Fish)] }, TestContext.Current.CancellationToken);
        Assert.NotNull(result.State);
        await File.AppendAllTextAsync(selected, "# unrelated\n", TestContext.Current.CancellationToken);
        var changed = (await File.ReadAllTextAsync(selected, TestContext.Current.CancellationToken)).Replace(ToolPathPolicy.EndMarker, $"# changed\n{ToolPathPolicy.EndMarker}", StringComparison.Ordinal);
        await File.WriteAllTextAsync(selected, changed, TestContext.Current.CancellationToken);
        var removal = await host.RemoveAsync(new(result.State), TestContext.Current.CancellationToken);
        Assert.Equal(selected, Assert.Single(removal.KeptTargets));
        Assert.Equal(changed, await File.ReadAllTextAsync(selected, TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Removal without live consent keeps every PATH target")]
    public async Task KeepsUnconfirmedRemoval()
    {
        using var temporary = TemporaryWorkspace.Create("tool-path");
        var live = true;
        var environment = new PathEnvironment(windows: true);
        var host = new ToolPathHost(environment, temporary.Path, new Backups(), () => live);
        var facts = await host.ObserveAsync(temporary.Path, TestContext.Current.CancellationToken);
        var result = await host.AddAsync(new() { Facts = facts, Choice = ToolPathChoice.AddForMe, ChosenShellFiles = [] }, TestContext.Current.CancellationToken);
        Assert.NotNull(result.State);
        live = false;
        var removal = await host.RemoveAsync(new(result.State), TestContext.Current.CancellationToken);
        Assert.Equal(1, environment.Writes);
        Assert.Empty(removal.RemovedTargets);
        Assert.Single(removal.KeptTargets);
    }

    private sealed class PathEnvironment(bool windows) : ToolPathEnvironment
    {
        internal string? Stored { get; set; }
        internal int Writes { get; private set; }
        internal override bool IsWindows => windows;
        internal override string? ProcessPath => Stored;
        internal override string? UserPath { get => Stored; set { Writes++; Stored = value; } }
    }

    private sealed class Backups : IToolBackupService
    {
        internal int Prepared { get; private set; }
        internal int Completed { get; private set; }
        public Task<ToolBackupResult> PrepareAsync(ToolBackupRequest request, CancellationToken cancellationToken)
        {
            Prepared++;
            return Task.FromResult(new ToolBackupResult { TargetPath = request.TargetPath, Kind = ToolBackupKind.PriorAbsence });
        }
        public Task<ToolBackupCompletionResult> CompleteAsync(ToolBackupCompletionRequest request, CancellationToken cancellationToken)
        {
            Completed++;
            return Task.FromResult(new ToolBackupCompletionResult(true, null, null));
        }
    }
}
