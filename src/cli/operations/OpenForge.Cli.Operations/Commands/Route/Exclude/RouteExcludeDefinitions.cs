using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Route.Exclude;

internal static class RouteExcludeDefinitions
{
    internal const string RecoveryHistoryCode = "route-exclude.recovery-history";
    internal const string RecoveryCoverageUnavailableCode = "route-exclude.recovery-coverage-unavailable";

    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "route exclude";
    internal static readonly CliSyntaxDefinition Command = new("exclude", "Remove a workspace file from the loader without deleting it.");
    internal static readonly CliSyntaxDefinition File = new("workspace-file", "Select one workspace-relative text file.");
    internal static readonly CliOptionDefinition<bool> DryRun = new("--dry-run", "Preview loader and settings changes without writing.", CliOptionArity.None, false);
}
