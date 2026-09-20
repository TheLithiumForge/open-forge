using OpenForge.Cli.Hosting.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Hosting;

public sealed class CliHostTerminalIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Host capability policy is fail-closed for redirected, dumb, and unknown terminals"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Integration")]
    [InlineData(true, false, "xterm", false, true, false, false, false)]
    [InlineData(false, true, "dumb", false, true, false, false, false)]
    [InlineData(false, false, "xterm", false, true, true, true, true)]
    [InlineData(false, false, "unknown", false, true, true, true, false)]
    [InlineData(false, false, "xterm", true, true, true, true, false)]
    public void ResolveCapabilities(
        bool inputRedirected,
        bool outputRedirected,
        string terminalName,
        bool isWindows,
        bool supportsKeyReads,
        bool expectedCanPrompt,
        bool expectedCanReadKeys,
        bool expectedCanRedraw)
    {
        var capabilities = CliHostTerminalFactory.ResolveCapabilities(
            inputRedirected,
            outputRedirected,
            terminalName,
            isWindows,
            supportsKeyReads);

        Assert.Equal(expectedCanPrompt, capabilities.CanPrompt);
        Assert.Equal(expectedCanReadKeys, capabilities.CanReadKeys);
        Assert.Equal(expectedCanRedraw, capabilities.CanRedraw);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Cancelled line wait reuses one retained underlying read"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Integration")]
    public async Task CancelledLineWaitResumesTheSameRead()
    {
        var reader = new BlockingTextReader();
        var adapter = new CliHostTerminalAdapter(
            reader,
            TextWriter.Null,
            () => true,
            () => new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false));
        using var cancellation = new CancellationTokenSource();

        var cancelledRead = adapter.ReadLineAsync(cancellation.Token).AsTask();
        await reader.Started;
        Assert.Equal(1, reader.ReadCount);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await cancelledRead);

        var resumedRead = adapter.ReadLineAsync(CancellationToken.None).AsTask();
        reader.Complete("preserved answer");

        Assert.Equal("preserved answer", await resumedRead);
        Assert.Equal(1, reader.ReadCount);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Host adapter keeps a synchronously blocking line reader off the command thread"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Integration")]
    public async Task SynchronouslyBlockingReaderRemainsCancellable()
    {
        var reader = new SynchronousBlockingTextReader();
        var adapter = new CliHostTerminalAdapter(
            reader,
            TextWriter.Null,
            () => false,
            () => new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false));
        using var cancellation = new CancellationTokenSource();

        try
        {
            var cancelledRead = adapter.ReadLineAsync(cancellation.Token).AsTask();
            await reader.Started;
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await cancelledRead);

            var resumedRead = adapter.ReadLineAsync(CancellationToken.None).AsTask();
            reader.Complete("preserved synchronous answer");

            Assert.Equal("preserved synchronous answer", await resumedRead);
            Assert.Equal(1, reader.ReadCount);
        }
        finally
        {
            reader.Complete("test cleanup");
        }
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Host adapter rejects key reads while a line read is pending"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Integration")]
    public async Task PendingLineReadBlocksCompetingKeyRead()
    {
        var reader = new BlockingTextReader();
        var keyReads = 0;
        var adapter = new CliHostTerminalAdapter(
            reader,
            TextWriter.Null,
            () =>
            {
                keyReads++;
                return true;
            },
            () => new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false));

        using var cancellation = new CancellationTokenSource();
        var cancelledRead = adapter.ReadLineAsync(cancellation.Token).AsTask();
        await reader.Started;
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await cancelledRead);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await adapter.ReadKeyAsync(CancellationToken.None));
        Assert.Equal(0, keyReads);

        reader.Complete("retained answer");
        await reader.Completed;
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await adapter.ReadKeyAsync(CancellationToken.None));

        Assert.Equal("retained answer", await adapter.ReadLineAsync(CancellationToken.None));
    }

    private sealed class BlockingTextReader : TextReader
    {
        private readonly TaskCompletionSource<bool> _started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<string?> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal int ReadCount { get; private set; }
        internal Task Started => _started.Task;
        internal Task Completed => _completion.Task;

        public override Task<string?> ReadLineAsync()
        {
            ReadCount++;
            _started.TrySetResult(true);
            return _completion.Task;
        }

        internal void Complete(string value) => _completion.TrySetResult(value);
    }

    private sealed class SynchronousBlockingTextReader : TextReader
    {
        private readonly TaskCompletionSource<bool> _started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private string? _answer;

        internal int ReadCount { get; private set; }
        internal Task Started => _started.Task;

        public override Task<string?> ReadLineAsync()
        {
            ReadCount++;
            _started.TrySetResult(true);
            _release.Task.GetAwaiter().GetResult();
            return Task.FromResult(_answer);
        }

        internal void Complete(string value)
        {
            _answer = value;
            _release.TrySetResult(true);
        }
    }
}
