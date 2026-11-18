using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Uninstall;

// The selection flags live in StrippingDefinitions, because uninstall and export share them.
internal static class UninstallDefinitions
{
    internal const string CommandIdentity = StrippingDefinitions.UninstallCommandIdentity;

    internal static readonly CliSyntaxDefinition Command = new(CommandIdentity, "Remove Open Forge from this workspace and keep the files you wrote.");
    internal static readonly CliOptionDefinition<bool> Automatic = new(StrippingDefinitions.AutomaticOption, "Apply the plan without a confirmation prompt.", CliOptionArity.None, false);
}
