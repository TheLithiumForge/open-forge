using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library;

internal static class LibraryDefinitions
{
    internal const int SchemaVersion = 1;
    internal static readonly CliSyntaxDefinition Group = new("library", "Manage contained Workspace Libraries.");
    internal static readonly CliSyntaxDefinition LibraryId = new("library-id", "Select one exact Library management ID.");
    internal static readonly CliSyntaxDefinition SourceRoot = new("source-root", "Select one contained workspace-relative source root.");
    internal static readonly CliOptionDefinition<bool> DryRun = new("--dry-run", "Preview the complete plan without writing.", CliOptionArity.None, false);
}
