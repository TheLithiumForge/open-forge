namespace OpenForge.Cli.Core.Framework.Tools.Models;

/// <summary>Runs one catalogue command. The root host owns the real implementation and tests pass a fake.</summary>
internal interface IToolProcessHost
{
    Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken);
}

internal sealed record ToolProcessRequest(ToolCommand Command, string WorkingDirectory, TimeSpan Timeout);

internal sealed record ToolProcessResult
{
    /// <summary>False when the shell or program couldn't start at all.</summary>
    public required bool Started { get; init; }
    public int? ExitCode { get; init; }
    public bool TimedOut { get; init; }

    /// <summary>The last part of standard output and standard error together.</summary>
    public string OutputTail { get; init; } = string.Empty;

    internal bool Succeeded => Started && !TimedOut && ExitCode == 0;
}
