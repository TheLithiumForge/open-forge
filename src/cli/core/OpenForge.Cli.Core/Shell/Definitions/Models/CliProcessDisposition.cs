namespace OpenForge.Cli.Core.Shell.Definitions.Models;

internal sealed record CliProcessDisposition(
    int ExitCode,
    CliOutputTarget HumanOutputTarget);
