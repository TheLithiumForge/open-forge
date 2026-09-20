using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Filesystem;

public sealed class TypedReadTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Strict UTF-8 reader distinguishes all foundational outcomes")]
    [Trait("Feature", "cli-filesystem"), Trait("Evidence", "Integration")]
    public async Task StrictReaderDistinguishesCompleteMissingInvalidAndCancelled()
    {
        using var temporary = TemporaryWorkspace.Create("typed-read");
        var valid = temporary.CreateFile("valid.txt", "content");
        var invalid = temporary.CreateFile("invalid.txt", [0xC3, 0x28]);
        var missing = temporary.Combine("missing.txt");
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var complete = await StrictUtf8FileReader.ReadAsync(valid, "valid.txt", TestContext.Current.CancellationToken);
        var absent = await StrictUtf8FileReader.ReadAsync(missing, "missing.txt", TestContext.Current.CancellationToken);
        var malformed = await StrictUtf8FileReader.ReadAsync(invalid, "invalid.txt", TestContext.Current.CancellationToken);
        var cancelled = await StrictUtf8FileReader.ReadAsync(valid, "valid.txt", cancellation.Token);

        Assert.Equal(FileReadState.Complete, complete.State);
        Assert.Equal("content", complete.Value);
        Assert.Equal(FileReadState.Missing, absent.State);
        Assert.Equal(FileReadState.InvalidEncoding, malformed.State);
        Assert.NotNull(malformed.Failure?.DirectCause);
        Assert.Equal(FileReadState.Cancelled, cancelled.State);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Directory enumerator materializes entries and typed outcomes")]
    [Trait("Feature", "cli-filesystem"), Trait("Evidence", "Integration")]
    public void DirectoryEnumeratorMaterializesEntriesAndReportsMissingAndCancellation()
    {
        using var temporary = TemporaryWorkspace.Create("typed-enumeration");
        var directory = temporary.CreateDirectory("entries");
        var file = temporary.CreateFile("entries/value.txt", "value");
        var complete = DirectoryEntryEnumerator.Enumerate(
            directory,
            "entries",
            TestContext.Current.CancellationToken);
        var missing = DirectoryEntryEnumerator.Enumerate(
            temporary.Combine("missing"),
            "missing",
            TestContext.Current.CancellationToken);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = DirectoryEntryEnumerator.Enumerate(directory, "entries", cancellation.Token);

        Assert.Equal(DirectoryEnumerationState.Complete, complete.State);
        Assert.Equal([file], complete.Entries);
        Assert.Equal(DirectoryEnumerationState.Missing, missing.State);
        Assert.Equal(DirectoryEnumerationState.Cancelled, cancelled.State);
    }
}
