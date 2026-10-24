using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using System.Diagnostics;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RenameGitFailureIntegrationTests
{
    [Fact(DisplayName = "Git admission shares ordinary ancestors within each observation and reacquires leaves on every pass")]
    public async Task AdmissionReadsOnlyOrdinaryAncestorsOnce()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        var reads = new Dictionary<string, int>(PhysicalIdentityTracker.PathComparer);
        var paths = new PhysicalPathResolver(path =>
        {
            reads[path] = reads.GetValueOrDefault(path) + 1;
            return LinkTargetReader.Read(path);
        });
        var reader = new GitRenameEvidenceReader(configureProcess: repository.Configure, physicalPaths: paths);
        for (var pass = 0; pass < 2; pass++)
        {
            reads.Clear();
            var result = await reader.ReadAsync(new(repository.Workspace, new() { Layout = WorkspaceLayout.Create(repository.Workspace, WorkspaceLayout.DefaultMainRoot), ManagedPaths = [".agents/skills"] }), TestContext.Current.CancellationToken);
            Assert.Equal(RenameGitAvailability.Available, result.Availability);
            Assert.Equal(1, reads[repository.PathFor(".agents")]);
            Assert.Equal(1, reads[repository.PathFor(".agents/guidance/_guidance.md")]);
        }
    }

    [Fact(DisplayName = "Each Git invocation gets its own deadline across a longer evidence read")]
    public async Task DeadlineIsPerInvocation()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        var clock = new RenameEvidenceClock();
        var invocations = 0;
        var adapter = new GitRenameEvidenceReader(configureProcess: repository.Configure, timeProvider: clock,
            processStarted: _ =>
            {
                invocations++;
                clock.Advance(GitRenameEvidenceReader.Deadline / 2);
            });
        var result = await adapter.ReadAsync(new(repository.Workspace, new() { Layout = WorkspaceLayout.Create(repository.Workspace, WorkspaceLayout.DefaultMainRoot) }), TestContext.Current.CancellationToken);
        Assert.Equal(RenameGitAvailability.Available, result.Availability);
        Assert.Equal(4, invocations);
        Assert.NotEmpty(result.Blobs);
    }

    [Theory(DisplayName = "No repository and a missing executable are optional internal observations")]
    [InlineData(false), InlineData(true)]
    public async Task MissingGitOrRepository(bool missingExecutable)
    {
        using var repository = new RenameEvidenceGitWorkspace();
        var executable = missingExecutable ? Path.Combine(repository.Root, "missing-git") : "git";
        var result = await new GitRenameEvidenceReader(executable, repository.Configure, timeProvider: new RenameEvidenceClock())
            .ReadAsync(new(repository.Workspace, new() { Layout = WorkspaceLayout.Create(repository.Workspace, WorkspaceLayout.DefaultMainRoot) }), TestContext.Current.CancellationToken);
        Assert.Equal(missingExecutable ? RenameGitAvailability.Unavailable : RenameGitAvailability.NoRepository, result.Availability);
        Assert.Empty(result.Blobs);
        Assert.Null(result.Head);
    }

    [Theory(DisplayName = "Timeout and caller cancellation reap a real blocked Git child before returning")]
    [InlineData(false), InlineData(true)]
    public async Task TimeoutAndCancellation(bool cancel)
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        var clock = new RenameEvidenceClock();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        Process? child = null;
        var adapter = new GitRenameEvidenceReader(configureProcess: start =>
        {
            repository.Configure(start);
            if (!start.ArgumentList.Contains("ls-tree")) return;
            start.ArgumentList.Clear();
            start.ArgumentList.Add("hash-object");
            start.ArgumentList.Add("--stdin");
            start.RedirectStandardInput = true;
        }, timeProvider: clock, processStarted: process =>
        {
            if (!process.StartInfo.RedirectStandardInput) return;
            child = Process.GetProcessById(process.Id);
            Assert.False(child.HasExited);
            if (cancel) cancellation.Cancel();
            else clock.Advance(GitRenameEvidenceReader.Deadline);
        });
        try
        {
            if (cancel)
            {
                await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                    await adapter.ReadAsync(new(repository.Workspace, new() { Layout = WorkspaceLayout.Create(repository.Workspace, WorkspaceLayout.DefaultMainRoot) }), cancellation.Token));
            }
            else
            {
                var result = await adapter.ReadAsync(new(repository.Workspace, new() { Layout = WorkspaceLayout.Create(repository.Workspace, WorkspaceLayout.DefaultMainRoot) }), cancellation.Token);
                Assert.Equal(RenameGitAvailability.TimedOut, result.Availability);
                Assert.Empty(result.Blobs);
            }

            Assert.NotNull(child);
            Assert.True(child.HasExited);
        }
        finally
        {
            if (child is not null)
            {
                if (!child.HasExited)
                {
                    child.Kill(entireProcessTree: true);
                    await child.WaitForExitAsync(CancellationToken.None);
                }
                child.Dispose();
            }
        }
    }
}
