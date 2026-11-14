using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Execution;

internal sealed record ToolProcessRequest
{
    public required string Action { get; init; }
    public required string? Host { get; init; }
    public required string ExecutableRoot { get; init; }
    public required string Executable { get; init; }
    public required string EntrySha256 { get; init; }
    public required string WorkingDirectory { get; init; }
    public required string[] Arguments { get; init; }
    public required Dictionary<string, string> Environment { get; init; }
    public required int TimeoutSeconds { get; init; }
    internal Func<CancellationToken, ValueTask>? Started { get; init; }
}

internal sealed record ToolProcessOutcome(ToolCommandState State, int? ExitCode, string? Cause)
{
    internal bool Succeeded => State == ToolCommandState.Exited && ExitCode == 0;
}

internal sealed record ToolDownloadRequest(Uri Source, long Bytes, string Integrity, string Destination);
internal delegate ValueTask ToolDownload(ToolDownloadRequest request, CancellationToken token);
internal delegate ValueTask<ToolProcessOutcome> ToolRunProcess(ToolProcessRequest request, CancellationToken token);
internal sealed record ToolExecutionHost(ToolDownload Download, ToolRunProcess Run);
