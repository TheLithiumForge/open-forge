using System.CommandLine;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

internal sealed record CliRootDefinition(
    RootCommand Root,
    CliGlobalOptionSymbols Options,
    IReadOnlyList<CliDelimiterPolicy> DelimiterPolicies);
