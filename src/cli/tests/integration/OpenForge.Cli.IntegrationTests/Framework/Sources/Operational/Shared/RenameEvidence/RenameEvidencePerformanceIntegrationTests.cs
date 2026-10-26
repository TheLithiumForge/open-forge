using System.Text;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RenameEvidencePerformanceIntegrationTests
{
    [Fact(DisplayName = "Many unchanged tracked files never enter successor content comparison")]
    public async Task UnchangedTrackedFilesAreNotCompared()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        const string oldPath = ".agents/guidance/cli-design.md";
        const string newPath = ".agents/guidance/new-cli-design.md";
        repository.Move(oldPath, newPath);
        foreach (var path in repository.Request().CandidatePaths)
            repository.Write(path, Encoding.UTF8.GetString(File.ReadAllBytes(repository.PathFor(path))).ReplaceLineEndings("\r\n"));
        var text = File.ReadAllText(repository.PathFor(newPath));
        repository.Write(newPath, text.ReplaceLineEndings("\r\n") + "\r\nOne extra authored line.\r\n");
        var request = repository.Request(oldPath);
        Assert.True(request.CandidatePaths.Length > 1000);
        var comparisons = 0;
        var reader = new RenameEvidenceReader(new PhysicalPathResolver(), new GitRenameEvidenceReader(configureProcess: repository.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync,
            timeProvider: new RenameEvidenceClock(), contentComparisonStarted: (old, current) =>
            {
                Assert.Equal(oldPath, old);
                Assert.Equal(newPath, current);
                comparisons++;
            });
        var result = await reader.ReadAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(RenameGitAvailability.Available, result.Facts.Git.Availability);
        Assert.Equal(1, comparisons);
        Assert.Equal(newPath, result.Find(oldPath)?.Proven?.NewPath);
    }

    [Theory(DisplayName = "Content evidence deadline reports timeout while caller cancellation still propagates")]
    [InlineData(false), InlineData(true)]
    public async Task ContentDeadline(bool cancel)
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        const string oldPath = ".agents/guidance/cli-design.md";
        const string newPath = ".agents/guidance/new-cli-design.md";
        var original = File.ReadAllBytes(repository.PathFor(oldPath));
        repository.Move(oldPath, newPath);
        var request = repository.Request(oldPath);
        var git = await new GitRenameEvidenceReader(configureProcess: repository.Configure, timeProvider: new RenameEvidenceClock())
            .ReadAsync(new(repository.Workspace, request.Policy), TestContext.Current.CancellationToken);
        Assert.Equal(RenameGitAvailability.Available, git.Availability);
        Assert.Contains(git.TrackedPaths, tracked => tracked.Path == oldPath);
        Assert.DoesNotContain(git.TrackedPaths, tracked => tracked.Path == newPath);
        Assert.Equal(original, Assert.Single(git.Blobs, blob => blob.Path == oldPath).Bytes.ToArray());
        Assert.False(File.Exists(repository.PathFor(oldPath)));
        Assert.Equal(original, File.ReadAllBytes(repository.PathFor(newPath)));
        var clock = new RenameEvidenceClock();
        var comparisons = 0;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var reader = new RenameEvidenceReader(new PhysicalPathResolver(), (_, _) => ValueTask.FromResult(git),
            timeProvider: clock, contentComparisonStarted: (old, current) =>
            {
                Assert.Equal(oldPath, old);
                Assert.Equal(newPath, current);
                comparisons++;
                if (cancel) cancellation.Cancel();
                else clock.Advance(RenameEvidenceReader.ContentDeadline);
            });
        if (cancel)
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await reader.ReadAsync(request, cancellation.Token));
            Assert.Equal(1, comparisons);
            Assert.True(cancellation.IsCancellationRequested);
            return;
        }

        var result = await reader.ReadAsync(request, cancellation.Token);
        Assert.Equal(1, comparisons);
        Assert.False(cancellation.IsCancellationRequested);
        Assert.Equal(RenameGitAvailability.TimedOut, result.Facts.Git.Availability);
        Assert.False(result.Facts.CandidateUniverseComplete);
        Assert.DoesNotContain(result.Resolutions, resolution => resolution.Proven is not null);
    }
}
