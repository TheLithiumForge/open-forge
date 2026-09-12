using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Library;

internal static class LibraryDefinitions
{
    internal const int SchemaVersion = 1;
    internal static readonly CliSyntaxDefinition Group = new("library", "Manage Libraries and their file links.");
    internal static readonly CliSyntaxDefinition LibraryId = new("library-id", "Select one exact Library management ID.");
    internal static readonly CliSyntaxDefinition SourceRoot = new("source-root", "Select a source directory by its workspace-relative path.");
    internal static readonly CliOptionDefinition<string> DestinationRoot = new(
        "--to", "Create file links below this workspace-relative directory.",
        CliOptionArity.ExactlyOne, Framework.Libraries.Models.Identity.LibraryDestinationRoot.WorkspaceRootValue,
        "directory");
    internal static readonly CliOptionDefinition<bool> DryRun = new("--dry-run", "Preview the complete plan without writing.", CliOptionArity.None, false);
}
