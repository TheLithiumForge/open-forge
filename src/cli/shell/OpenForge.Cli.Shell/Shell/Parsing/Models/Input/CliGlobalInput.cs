using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.Input;

internal sealed record CliGlobalInput(
    string? WorkspaceValue, int WorkspaceOccurrences,
    CliFormat OutputFormat, int FormatOccurrences,
    CliDetail Detail, int DetailOccurrences,
    IReadOnlySet<CliSeverity>? Filter, int FilterOccurrences,
    bool Help, int HelpOccurrences, bool Version, int VersionOccurrences)
{
    internal CliPresentation Presentation => new(OutputFormat, Detail, Filter);
}
