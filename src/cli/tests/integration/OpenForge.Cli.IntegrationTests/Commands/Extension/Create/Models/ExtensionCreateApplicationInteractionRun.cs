using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Create.Models;

internal sealed record ExtensionCreateApplicationInteractionRun
{
    public required int ExitCode { get; init; }

    public required CliSemanticStatus Status { get; init; }

    public required string StandardOutput { get; init; }

    public required string StandardError { get; init; }

    public required string? RemainingInput { get; init; }
}
