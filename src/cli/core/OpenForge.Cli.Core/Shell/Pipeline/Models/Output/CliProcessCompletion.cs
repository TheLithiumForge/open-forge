using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Pipeline.Models.Output;

internal sealed record CliProcessCompletion(
    CliSemanticStatus Status,
    int ExitCode,
    CliOutputTarget PrimaryOutputTarget);
