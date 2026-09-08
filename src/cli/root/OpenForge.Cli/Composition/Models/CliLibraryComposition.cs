using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Composition.Models;

internal sealed record CliLibraryComposition
{
    public required CliRootBranch Branch { get; init; }

    public required ICliCommandBinding ListBinding { get; init; }

    public required ICliCommandBinding InspectBinding { get; init; }

    public required ICliCommandBinding AttachBinding { get; init; }

    public required ICliCommandBinding SyncBinding { get; init; }

    public required ICliCommandBinding DetachBinding { get; init; }
}
