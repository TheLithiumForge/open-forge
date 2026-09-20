using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update.Models;

internal sealed record ExtensionUpdateApplicationInteractionRun
{
    public required int ExitCode { get; init; }

    public required CliSemanticStatus Status { get; init; }

    public required string StandardOutput { get; init; }

    public required string StandardError { get; init; }

    public required string TerminalOutput { get; init; }

    public required string? RemainingInput { get; init; }
}
