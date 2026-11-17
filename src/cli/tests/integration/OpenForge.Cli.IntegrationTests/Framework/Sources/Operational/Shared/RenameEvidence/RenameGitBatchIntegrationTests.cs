using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RenameGitBatchIntegrationTests
{
    [Fact(DisplayName = "Batched Git blobs preserve BOM CRLF binary bytes and shared object identities")]
    public async Task BatchBytesAndDuplicateObjects()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync(unborn: true);
        const string path = ".agents/guidance/note.md";
        repository.Write(path, "\uFEFF# Authored\r\n\r\nBody without a final newline");
        var runner = new GitProcessRunner("git", repository.Configure);
        var hash = await runner.RunAsync(repository.Root, ["hash-object", "-w", path], TestContext.Current.CancellationToken);
        Assert.NotNull(hash);
        Assert.Equal(0, hash.Value.ExitCode);
        var id = hash.Value.Output.Trim();
        var output = await runner.RunBytesWithInputAsync(repository.Root, ["cat-file", "--batch"], Encoding.ASCII.GetBytes($"{id}\n"), TestContext.Current.CancellationToken);
        Assert.NotNull(output);
        Assert.Equal(0, output.Value.ExitCode);
        var paths = ImmutableArray.Create(new RenameGitTrackedPath(path, id), new RenameGitTrackedPath(".agents/guidance/alias.md", id));
        var blobs = GitRenameBlobBatchReader.Read(output.Value.Output, paths);
        Assert.Equal(2, blobs.Length);
        Assert.All(blobs, blob => Assert.Equal(File.ReadAllBytes(repository.PathFor(path)), blob.Bytes));
    }

    // The runner writes input without the caller's token, because a cancelled Windows pipe write can complete while its
    // native IO is pending. A child that exits without reading breaks the pipe, which must not fail the invocation.
    [Fact(DisplayName = "Git input a child exits without reading leaves its result intact")]
    public async Task UnreadInputKeepsTheResult()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        var runner = new GitProcessRunner("git", repository.Configure);
        var input = new byte[8 * 1024 * 1024];

        var output = await runner.RunBytesWithInputAsync(repository.Root, ["rev-parse", "--is-inside-work-tree"], input, TestContext.Current.CancellationToken);

        Assert.NotNull(output);
        Assert.Equal(0, output.Value.ExitCode);
        Assert.NotEmpty(output.Value.Output);
    }

    [Theory(DisplayName = "Malformed or truncated batch records cannot supply optional Git evidence")]
    [InlineData(""), InlineData("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb blob 2\nx\n")]
    [InlineData("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb tree 1\nx\n")]
    [InlineData("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb blob 1\nx\nextra")]
    public void InvalidBatchRecords(string text)
    {
        ImmutableArray<RenameGitTrackedPath> paths = [new(".agents/guidance/note.md", "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb")];
        Assert.Throws<ArgumentException>(() => GitRenameBlobBatchReader.Read(Encoding.ASCII.GetBytes(text), paths));
    }
}
