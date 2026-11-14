namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal sealed record ToolProcessRequest
{
    public required string ExecutablePath { get; init; }
    public required IReadOnlyList<string> Arguments { get; init; }
    public required string WorkingDirectory { get; init; }
    public required IReadOnlyDictionary<string, string?> Environment { get; init; }
    public required TimeSpan Timeout { get; init; }
    public required int MaximumOutputBytes { get; init; }
}

internal sealed record ToolProcessResult
{
    public int? ExitCode { get; init; }
    public required bool TimedOut { get; init; }
    public required bool Cancelled { get; init; }
    public required string StandardOutput { get; init; }
    public required string StandardError { get; init; }
    public string? Failure { get; init; }
}
