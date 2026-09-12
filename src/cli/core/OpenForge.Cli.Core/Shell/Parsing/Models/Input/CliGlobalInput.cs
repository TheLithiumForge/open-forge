using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.Input;

internal sealed record CliGlobalInput(
    string? WorkspaceValue,
    int WorkspaceOccurrences,
    CliOutputFormat OutputFormat,
    int JsonOccurrences,
    CliView View,
    int ViewOccurrences,
    CliVerbosity Verbosity,
    int VerboseOccurrences,
    bool Help,
    int HelpOccurrences,
    bool Version,
    int VersionOccurrences)
{
    internal CliPresentation Presentation => new(OutputFormat, View, Verbosity);
}
