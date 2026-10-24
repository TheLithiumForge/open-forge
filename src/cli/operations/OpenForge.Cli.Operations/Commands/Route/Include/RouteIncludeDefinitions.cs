using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Route.Include;

internal static class RouteIncludeDefinitions
{
    internal const string RecoveryHistoryCode = "route-include.recovery-history";
    internal const string RecoveryCoverageUnavailableCode = "route-include.recovery-coverage-unavailable";

    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "route include";
    internal static readonly CliSyntaxDefinition Command = new("include", "List an existing workspace file in the loader where it lives.");
    internal static readonly CliSyntaxDefinition File = new("workspace-file", "Select one workspace-relative text file.");
    internal static readonly CliOptionDefinition<bool> DryRun = new("--dry-run", "Preview loader and settings changes without writing.", CliOptionArity.None, false);
    internal static readonly CliOptionDefinition<string?> Description = new("--description", "Set the loader row description. Required for a new row.", CliOptionArity.ExactlyOne, null, "text");
    internal static readonly CliOptionDefinition<string[]> Tag = new("--tag", "Replace the ordered tags. Repeat for more tags, with or without #.", CliOptionArity.ExactlyOne, [], "tag");
    internal static readonly CliOptionDefinition<string[]> ApplyTo = new("--apply-to", "Replace the conditions. Repeat for more workspace-relative globs.", CliOptionArity.ExactlyOne, [], "glob");
}
