using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Parsing.Models;

internal sealed record CliGlobalInputResolution(
    CliGlobalInput? Input,
    CliTerminalMode TerminalMode,
    CliInvalidInput? InvalidInput);
