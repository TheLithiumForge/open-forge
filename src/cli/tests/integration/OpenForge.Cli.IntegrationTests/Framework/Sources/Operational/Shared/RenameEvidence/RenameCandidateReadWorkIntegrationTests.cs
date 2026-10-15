using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Input;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RenameCandidateReadWorkIntegrationTests
{
    [Fact(DisplayName = "Missing-path evidence batches fresh observations in order and notices a created leaf on the next read")]
    public async Task MissingPathReadsAreFreshAndBounded()
    {
        using var temporary = TemporaryWorkspace.Create("rename-missing-read-work");
        temporary.CreateDirectory(".agents/old");
        var missing = Enumerable.Range(0, 32).Select(index => $".agents/old/note-{index}.md").Order(StringComparer.Ordinal).ToArray();
        using var release = new ManualResetEventSlim();
        var reads = 0;
        var active = 0;
        var peak = 0;
        var token = TestContext.Current.CancellationToken;
        var paths = new PhysicalPathResolver(path =>
        {
            if (!PhysicalIdentityTracker.PathComparer.Equals(path, temporary.Combine(".agents/old")))
                return LinkTargetReader.Read(path);
            Interlocked.Increment(ref reads);
            var count = Interlocked.Increment(ref active);
            InterlockedMax(ref peak, count);
            if (count > 1) release.Set();
            try
            {
                Assert.True(release.Wait(TimeSpan.FromSeconds(10), token));
                return LinkTargetReader.Read(path);
            }
            finally { Interlocked.Decrement(ref active); }
        });
        var reader = new RenameEvidenceReader(paths);
        var request = new RenameEvidenceRequest
        {
            Workspace = new(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace),
            CandidatePaths = [],
            MissingPaths = [.. missing],
            Policy = new(),
            CandidateUniverseComplete = true,
        };
        var first = await reader.ReadAsync(request, token);
        Assert.Equal(missing, first.Facts.MissingPaths.Select(item => item.Path));
        Assert.Equal(32, reads);
        Assert.InRange(peak, 2, RenameEvidenceReader.MaximumConcurrentReads);
        Assert.Equal(0, active);
        temporary.WriteText(missing[0], "# Created\n");
        var second = await reader.ReadAsync(request, token);
        Assert.Equal(missing.Skip(1), second.Facts.MissingPaths.Select(item => item.Path));
        Assert.Equal(64, reads);
        Assert.Equal(0, active);
    }

    [Fact(DisplayName = "Rename evidence reads each candidate freshly once per observation with bounded parallel acquisition")]
    public async Task CandidateReadsAreFreshAndBounded()
    {
        using var temporary = TemporaryWorkspace.Create("rename-candidate-read-work");
        var paths = Enumerable.Range(0, 32).Select(index => $".agents/guidance/note-{index}.md").ToArray();
        foreach (var path in paths) temporary.WriteText(path, "# Same content\n");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var reads = 0;
        var active = 0;
        var peak = 0;
        var reader = new RenameEvidenceReader(new PhysicalPathResolver(), readCandidate: async (path, token) =>
        {
            Interlocked.Increment(ref reads);
            var count = Interlocked.Increment(ref active);
            InterlockedMax(ref peak, count);
            Assert.InRange(count, 1, RenameEvidenceReader.MaximumConcurrentReads);
            try { await Task.Yield(); return await File.ReadAllBytesAsync(path, token); }
            finally { Interlocked.Decrement(ref active); }
        });
        var request = new RenameEvidenceRequest
        {
            Workspace = workspace,
            CandidatePaths = [.. paths],
            MissingPaths = [],
            CandidateUniverseComplete = true,
            Policy = new(),
        };
        var first = await reader.ReadAsync(request, TestContext.Current.CancellationToken);
        temporary.ReplaceText(paths[0], "# Changed\n");
        var second = await reader.ReadAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(64, reads);
        Assert.InRange(peak, 2, RenameEvidenceReader.MaximumConcurrentReads);
        Assert.Equal(0, active);
        Assert.Equal(32, first.Facts.Candidates.Length);
        Assert.Equal(first.Facts.Candidates.Select(candidate => candidate.Path), second.Facts.Candidates.Select(candidate => candidate.Path));
        Assert.NotEqual(first.Facts.Candidates[0].Content.Hash, second.Facts.Candidates[0].Content.Hash);
    }

    private static void InterlockedMax(ref int target, int value)
    {
        var current = Volatile.Read(ref target);
        while (current < value)
        {
            var previous = Interlocked.CompareExchange(ref target, value, current);
            if (previous == current) return;
            current = previous;
        }
    }
}
