using System.CommandLine;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

internal sealed record CliGlobalOptionSymbols(
    Option<string?> Workspace,
    Option<bool> Json,
    Option<CliView> View,
    Option<bool> Verbose,
    Option<bool> Help,
    Option<bool> Version);
