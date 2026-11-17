using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

namespace OpenForge.Cli.Composition.Models;

internal sealed record CliToolComposition
{
    public required CliRootBranch Branch { get; init; }

    public required ICliCommandBinding ListBinding { get; init; }

    public required ICliCommandBinding InstallBinding { get; init; }

    public required ICliCommandBinding RemoveBinding { get; init; }
}
