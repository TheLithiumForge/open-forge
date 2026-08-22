using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Parsing.Models;

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
