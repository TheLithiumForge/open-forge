using System.CommandLine;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal sealed record CliRootLeaf(
    Command Command,
    IReadOnlyList<CliDelimiterPolicy> DelimiterPolicies);
