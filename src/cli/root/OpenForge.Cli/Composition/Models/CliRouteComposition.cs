using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Composition.Models;

internal sealed record CliRouteComposition
{
    public required CliRootBranch Branch { get; init; }

    public required ICliCommandBinding ListBinding { get; init; }

    public required ICliCommandBinding InspectBinding { get; init; }

    public required ICliCommandBinding InitBinding { get; init; }

    public required ICliCommandBinding CreateBinding { get; init; }

    public required ICliCommandBinding UpdateBinding { get; init; }

    public required ICliCommandBinding MoveBinding { get; init; }

    public required ICliCommandBinding RemoveBinding { get; init; }
}
