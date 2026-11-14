namespace OpenForge.Cli.Core.Framework.Tools.Models.Launchers;

internal enum ToolPathReadiness { Ready, Missing, Unavailable }
internal enum ToolPathChoice { AddForMe, Manual }
internal enum ToolShellKind { Posix, Fish }

internal sealed record ToolShellFile(string AbsolutePath, ToolShellKind Shell);

internal sealed record ToolPathFacts
{
    public required string BinPath { get; init; }
    public required ToolPathReadiness Readiness { get; init; }
    public required IReadOnlyList<ToolShellFile> DetectedShellFiles { get; init; }
}
