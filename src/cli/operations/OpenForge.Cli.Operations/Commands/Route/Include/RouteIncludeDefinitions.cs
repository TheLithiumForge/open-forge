using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Route.Include;

internal static class RouteIncludeDefinitions
{
    internal const string RecoveryHistoryCode = "route-include.recovery-history";
    internal const string RecoveryCoverageUnavailableCode = "route-include.recovery-coverage-unavailable";

    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "route include";
    internal static readonly CliSyntaxDefinition Command = new("include", "List an existing file or register an outside folder as a maintained route.");
    internal static readonly CliSyntaxDefinition File = new("workspace-path", "Select one workspace-relative text file or outside folder.");
    internal static readonly CliOptionDefinition<bool> DryRun = new("--dry-run", "Preview catalogue, loader and settings changes without writing.", CliOptionArity.None, false);
    internal static readonly CliOptionDefinition<string?> Description = new("--description", "Set the loader row description. Required for a new file row.", CliOptionArity.ExactlyOne, null, "text");
    internal static readonly CliOptionDefinition<string[]> Tag = new("--tag", "Replace the ordered tags. Repeat for more tags, with or without #.", CliOptionArity.ExactlyOne, [], "tag");
    internal static readonly CliOptionDefinition<string[]> ApplyTo = new("--apply-to", "Replace the conditions. Repeat for more workspace-relative globs.", CliOptionArity.ExactlyOne, [], "glob");
}
