using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Shared;

/// <summary>
/// The command-line half of a workspace permission grant, shared by every command
/// that can be asked to write outside <c>.agents</c>.
///
/// A terminal can be prompted; an agent, a script, and CI cannot, and the prompt
/// requires a non-redirected stdin. Without this option the only non-interactive
/// way to permit a destination is to hand-edit a file, which is the defect this
/// closes. It matters most beside <c>--automatic</c>, where no prompt will fire.
/// </summary>
internal static class PermissionOptions
{
    internal const string HelpBody = "  Destinations outside .agents require allowInstallPaths in .agents/open-forge.json. "
        + "--allow-path <path> records a grant for all Extensions and Libraries. Repeat it for several paths; --dry-run writes nothing.";

    internal static readonly CliOptionDefinition<string> AllowPath = new(
        "--allow-path",
        "Permit writing to this workspace-relative path, recording it in .agents/open-forge.json.",
        CliOptionArity.ExactlyOne,
        string.Empty,
        "path");
}
