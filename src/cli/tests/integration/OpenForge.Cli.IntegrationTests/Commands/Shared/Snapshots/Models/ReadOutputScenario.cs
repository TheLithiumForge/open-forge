namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;

internal sealed record ReadOutputScenario
{
    public required string Situation { get; init; }
    public required string[] Arguments { get; init; }
    public required int ExitCode { get; init; }
    public string? DiagnosticId { get; init; }
    public bool ShellDiagnostic { get; init; }
}
