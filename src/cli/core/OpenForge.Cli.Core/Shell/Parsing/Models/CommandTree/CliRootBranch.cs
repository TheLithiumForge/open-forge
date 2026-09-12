using System.CommandLine;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

internal sealed record CliRootBranch(
    Command Command,
    CliHelpContent Help,
    IReadOnlyList<CliDelimiterPolicy> DelimiterPolicies);
