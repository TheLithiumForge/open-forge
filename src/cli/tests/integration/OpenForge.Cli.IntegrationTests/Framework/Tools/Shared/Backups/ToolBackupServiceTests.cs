using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Filesystem;
using System.Text;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Backups;

[Trait("Feature", "tool-backups"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolBackupServiceTests
{
    private const string Target = "synthetic.bin";
    private static readonly byte[] Original = [0, 127, 255];
    private static readonly byte[] First = [1, 2];
    private static readonly byte[] Second = [3, 4];
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Sequential file edits keep the first sibling backup until the final result verifies")]
    public async Task OriginalAcrossSequentialEdits()
    {
        using var fixture = new BackupFixture();
        await File.WriteAllBytesAsync(fixture.TargetPath, Original, Token);
        var first = await fixture.Service.PrepareAsync(new(fixture.TargetPath), Token);
        Assert.Null(first.Failure);
        Assert.Equal(ToolBackupKind.Sibling, first.Kind);
        await File.WriteAllBytesAsync(fixture.TargetPath, First, Token);
        var continuation = await fixture.Service.PrepareAsync(new(fixture.TargetPath), Token);
        Assert.Same(first, continuation);
        await File.WriteAllBytesAsync(fixture.TargetPath, Second, Token);
        Assert.Equal(Original, await File.ReadAllBytesAsync(Assert.IsType<string>(first.BackupPath), Token));
        var complete = await fixture.Service.CompleteAsync(new(first, FileExpectation.Hash(Second), Verified: true), Token);
        Assert.Null(complete.Failure);
        Assert.True(complete.Discarded);
        Assert.False(File.Exists(first.BackupPath));
        Assert.Equal(Second, await File.ReadAllBytesAsync(fixture.TargetPath, Token));
    }

    [Theory(DisplayName = "A failed or mismatched completion retains both the backup and current target without rollback")]
    [InlineData(false), InlineData(true)]
    public async Task FailedCompletion(bool verify)
    {
        using var fixture = new BackupFixture();
        await File.WriteAllBytesAsync(fixture.TargetPath, Original, Token);
        var backup = await fixture.Service.PrepareAsync(new(fixture.TargetPath), Token);
        Assert.Null(backup.Failure);
        await File.WriteAllBytesAsync(fixture.TargetPath, First, Token);
        var complete = await fixture.Service.CompleteAsync(new(backup, FileExpectation.Hash(Second), Verified: verify), Token);
        Assert.NotNull(complete.Failure);
        Assert.False(complete.Discarded);
        Assert.Equal(backup.BackupPath, complete.KeptPath);
        Assert.Equal(Original, await File.ReadAllBytesAsync(Assert.IsType<string>(backup.BackupPath), Token));
        Assert.Equal(First, await File.ReadAllBytesAsync(fixture.TargetPath, Token));
    }

    [Fact(DisplayName = "An occupied unrelated backup destination blocks preparation without changing either file")]
    public async Task OccupiedBackup()
    {
        using var fixture = new BackupFixture();
        await File.WriteAllBytesAsync(fixture.TargetPath, Original, Token);
        await File.WriteAllBytesAsync(ToolBackupPaths.Beside(fixture.TargetPath), Second, Token);
        var backup = await fixture.Service.PrepareAsync(new(fixture.TargetPath), Token);
        Assert.NotNull(backup.Failure);
        Assert.Equal(Original, await File.ReadAllBytesAsync(fixture.TargetPath, Token));
        Assert.Equal(Second, await File.ReadAllBytesAsync(ToolBackupPaths.Beside(fixture.TargetPath), Token));
    }

    [Fact(DisplayName = "A changed operation backup prevents continuation and cannot be discarded")]
    public async Task ChangedBackup()
    {
        using var fixture = new BackupFixture();
        await File.WriteAllBytesAsync(fixture.TargetPath, Original, Token);
        var backup = await fixture.Service.PrepareAsync(new(fixture.TargetPath), Token);
        Assert.Null(backup.Failure);
        await File.WriteAllBytesAsync(Assert.IsType<string>(backup.BackupPath), Second, Token);
        var failed = await fixture.Service.PrepareAsync(new(fixture.TargetPath), Token);
        Assert.NotNull(failed.Failure);
        Assert.Equal(backup.BackupPath, failed.BackupPath);
        Assert.Equal(ToolBackupKind.Sibling, failed.Kind);
        var complete = await fixture.Service.CompleteAsync(new(backup, FileExpectation.Hash(Original), Verified: true), Token);
        Assert.NotNull(complete.Failure);
        Assert.Equal(Second, await File.ReadAllBytesAsync(Assert.IsType<string>(backup.BackupPath), Token));
    }

    [Fact(DisplayName = "A newly created then deleted target completes from prior absence without a sibling backup")]
    public async Task PriorAbsence()
    {
        using var fixture = new BackupFixture();
        var backup = await fixture.Service.PrepareAsync(new(fixture.TargetPath), Token);
        Assert.Null(backup.Failure);
        Assert.Equal(ToolBackupKind.PriorAbsence, backup.Kind);
        await File.WriteAllBytesAsync(fixture.TargetPath, First, Token);
        Assert.Same(backup, await fixture.Service.PrepareAsync(new(fixture.TargetPath), Token));
        File.Delete(fixture.TargetPath);
        var completed = await fixture.Service.CompleteAsync(new(backup, ExpectedAppliedSha256: null, Verified: true), Token);
        Assert.Null(completed.Failure);
        Assert.Null(completed.KeptPath);
        Assert.False(File.Exists(ToolBackupPaths.Beside(fixture.TargetPath)));
    }

    [Theory(DisplayName = "Git covers only tracked raw bytes matching HEAD while dirty and untracked bytes get siblings")]
    [InlineData(true, false), InlineData(true, true), InlineData(false, false)]
    public async Task GitCoverage(bool tracked, bool dirty)
    {
        using var fixture = new BackupFixture();
        await File.WriteAllBytesAsync(fixture.TargetPath, Original, Token);
        await fixture.GitAsync("init", "--quiet", "--template=");
        if (tracked)
        {
            await fixture.GitAsync("add", "--", Target);
            var tree = await fixture.GitAsync("write-tree");
            // A synthetic HEAD tree supplies Git's immutable blob without creating a commit.
            await File.WriteAllTextAsync(Path.Combine(fixture.Repository, WorkspaceLayoutNames.GitMetadataDirectoryName, "HEAD"), tree.Trim() + "\n", Token);
        }
        if (dirty) await File.WriteAllBytesAsync(fixture.TargetPath, First, Token);
        var expected = dirty ? First : Original;
        var backup = await fixture.Service.PrepareAsync(new(fixture.TargetPath), Token);
        Assert.Null(backup.Failure);
        Assert.Equal(tracked && !dirty ? ToolBackupKind.Git : ToolBackupKind.Sibling, backup.Kind);
        if (backup.Kind == ToolBackupKind.Git)
        {
            Assert.NotNull(backup.GitObjectId);
            Assert.Null(backup.BackupPath);
        }
        else Assert.Equal(expected, await File.ReadAllBytesAsync(Assert.IsType<string>(backup.BackupPath), Token));
        await File.WriteAllBytesAsync(fixture.TargetPath, Second, Token);
        var completed = await fixture.Service.CompleteAsync(new(backup, FileExpectation.Hash(Second), Verified: true), Token);
        Assert.Null(completed.Failure);
        Assert.False(File.Exists(ToolBackupPaths.Beside(fixture.TargetPath)));
    }

    [Fact(DisplayName = "Backup handles from another operation cannot delete a sibling file")]
    public async Task ForeignHandle()
    {
        using var fixture = new BackupFixture();
        await File.WriteAllBytesAsync(fixture.TargetPath, Original, Token);
        var backup = await fixture.Service.PrepareAsync(new(fixture.TargetPath), Token);
        var other = new ToolBackupService(fixture.Host);
        var result = await other.CompleteAsync(new(backup, FileExpectation.Hash(Original), Verified: true), Token);
        Assert.NotNull(result.Failure);
        Assert.True(File.Exists(backup.BackupPath));
    }

    private sealed class BackupFixture : IDisposable
    {
        private readonly TemporaryWorkspace _temporary = TemporaryWorkspace.Create("tool-backup");
        internal string Repository { get; }
        internal string TargetPath => Path.Combine(Repository, Target);
        internal ToolGitTestHost Host { get; }
        internal ToolBackupService Service { get; }

        internal BackupFixture()
        {
            Repository = _temporary.CreateDirectory("repository");
            Host = new(_temporary.CreateDirectory("home"));
            Service = new(Host);
        }

        internal async Task<string> GitAsync(params string[] arguments)
        {
            var result = await Host.RunAsync(new()
            {
                ExecutablePath = ToolGitEnvironment.Executable,
                WorkingDirectory = Repository,
                Arguments = arguments,
                Environment = new Dictionary<string, string?>(),
                Timeout = TimeSpan.FromSeconds(10),
                MaximumOutputBytes = 16 * 1024
            }, Token);
            Assert.Equal(0, result.ExitCode);
            Assert.Null(result.Failure);
            return result.StandardOutput;
        }

        public void Dispose()
        {
            Assert.Equal(_temporary.Combine("repository"), Repository);
            Assert.Null(new DirectoryInfo(Repository).LinkTarget);
            foreach (var file in Directory.EnumerateFiles(Repository, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal);
            Directory.Delete(Repository, recursive: true);
            _temporary.Dispose();
        }
    }

}
