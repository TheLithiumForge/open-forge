using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Composition.Models;

internal sealed record CliExtensionComposition
{
    public required CliRootBranch Branch { get; init; }

    public required ICliCommandBinding ListBinding { get; init; }

    public required ICliCommandBinding InspectBinding { get; init; }

    public required ICliCommandBinding CreateBinding { get; init; }

    public required ICliCommandBinding InstallBinding { get; init; }
}
