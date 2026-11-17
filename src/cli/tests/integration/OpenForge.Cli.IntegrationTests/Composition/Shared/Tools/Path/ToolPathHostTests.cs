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

    [Fact(DisplayName = "Windows PATH appends only its entry through the injected user environment and keeps the value kind")]
    public async Task AppendsOnlyUserEntry()
    {
        using var temporary = TemporaryWorkspace.Create("tool-path");
        var existing = temporary.Combine("existing");
        var environment = new PathEnvironment(windows: true) { Stored = existing };
        var host = new ToolPathHost(environment, temporary.Path, new Backups(), () => true);
        var facts = await host.ObserveAsync(temporary.Path, TestContext.Current.CancellationToken);
        var result = await host.AddAsync(new() { Facts = facts, Choice = ToolPathChoice.AddForMe, ChosenShellFiles = [] }, TestContext.Current.CancellationToken);
        Assert.Null(result.Failure);
        Assert.NotNull(result.State);
        Assert.Equal(string.Join(ToolWindowsUserPath.Separator, existing, facts.BinPath), environment.Stored);
        Assert.Equal(1, environment.Writes);
        Assert.Equal(1, environment.Announcements);
        Assert.True(environment.Expandable);
    }

    [Fact(DisplayName = "An unreadable Windows user PATH is never written and the user gets manual steps")]
    public async Task UnreadableUserPathStaysUnchanged()
    {
        using var temporary = TemporaryWorkspace.Create("tool-path");
        var environment = new PathEnvironment(windows: true) { Unreadable = true };
        var host = new ToolPathHost(environment, temporary.Path, new Backups(), () => true);
        var facts = await host.ObserveAsync(temporary.Path, TestContext.Current.CancellationToken);
        var result = await host.AddAsync(new() { Facts = facts, Choice = ToolPathChoice.AddForMe, ChosenShellFiles = [] }, TestContext.Current.CancellationToken);
        Assert.NotNull(result.Failure);
        Assert.NotEmpty(result.ManualLines);
        Assert.Equal(0, environment.Writes);
        Assert.Equal(0, environment.Announcements);
    }

    [Fact(DisplayName = "Unix PATH writes only the marked block in chosen files and keeps every other byte")]
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
        Assert.Equal(before, (await File.ReadAllBytesAsync(selected, TestContext.Current.CancellationToken))[..before.Length]);
        // Remove only names the marked block in its manual steps.
        Assert.True(ToolPathPolicy.HasMarkedBlock(selected));
        Assert.False(ToolPathPolicy.HasMarkedBlock(untouched));
        Assert.Equal(1, backups.Prepared);
        Assert.Equal(0, backups.Completed);
        Assert.True(Assert.Single(result.PendingBackups).Verified);
        Assert.Equal(selected, result.PendingBackups[0].Backup.TargetPath);
    }

    // The registry adapter's stand-in. No test reads or writes the real user PATH.
    private sealed class PathEnvironment(bool windows) : ToolPathEnvironment
    {
        internal string? Stored { get; set; }
        internal bool Expandable { get; set; } = true;
        internal bool Unreadable { get; set; }
        internal int Writes { get; private set; }
        internal int Announcements { get; private set; }
        internal override bool IsWindows => windows;
        internal override string? ProcessPath => Stored;
        internal override ToolUserPathValue? ReadUserPath() => Unreadable ? null : new(Stored ?? string.Empty, Expandable, Stored is not null);
        internal override bool WriteUserPath(ToolUserPathValue value)
        {
            Writes++;
            Stored = value.Text;
            Expandable = value.Expandable;
            return true;
        }
        internal override void AnnounceEnvironmentChange() => Announcements++;
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
