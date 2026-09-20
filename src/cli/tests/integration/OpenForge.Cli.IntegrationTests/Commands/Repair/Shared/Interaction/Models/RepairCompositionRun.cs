using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair.Shared.Interaction.Models;

internal sealed record RepairCompositionRun
{
    public required int ExitCode { get; init; }

    public required CliSemanticStatus Status { get; init; }

    public required CliOutputTarget PrimaryOutputTarget { get; init; }

    public required string StandardOutput { get; init; }

    public required string StandardError { get; init; }

    public required string? RemainingInput { get; init; }
}
