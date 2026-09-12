using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.Input;

internal sealed record CliGlobalInputResolution(
    CliGlobalInput? Input,
    CliTerminalMode TerminalMode,
    CliInvalidInput? InvalidInput);
