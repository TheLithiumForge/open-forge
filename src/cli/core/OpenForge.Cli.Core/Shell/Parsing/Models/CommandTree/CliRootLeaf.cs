using System.CommandLine;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

internal sealed record CliRootLeaf(
    Command Command,
    IReadOnlyList<CliDelimiterPolicy> DelimiterPolicies);
