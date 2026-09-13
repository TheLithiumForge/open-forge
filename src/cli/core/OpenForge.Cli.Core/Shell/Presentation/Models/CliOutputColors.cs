namespace OpenForge.Cli.Core.Shell.Presentation.Models;

internal sealed record CliOutputColors(bool StandardOutput, bool StandardError)
{
    internal static CliOutputColors Plain { get; } = new(StandardOutput: false, StandardError: false);
}
