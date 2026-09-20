using System.CommandLine;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.Results;

internal sealed record CliParseOutcome(
    ParseResult Result,
    CliCommandTree Tree,
    CliGlobalOptionSymbols Options,
    IReadOnlyList<string> OriginalArguments);
