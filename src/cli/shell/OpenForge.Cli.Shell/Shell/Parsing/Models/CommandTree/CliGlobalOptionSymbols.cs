using System.CommandLine;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

internal sealed record CliGlobalOptionSymbols(
    Option<string?> Workspace, Option<CliFormat> Format, Option<CliDetail> Detail,
    Option<CliSeverityFilter[]> DetailFilter, Option<bool> Help, Option<bool> Version);
