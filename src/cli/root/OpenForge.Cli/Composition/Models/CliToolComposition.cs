using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

namespace OpenForge.Cli.Composition.Models;

internal sealed record CliToolComposition
{
    public required CliRootBranch Branch { get; init; }
    public required ICliCommandBinding ListBinding { get; init; }
    public required ICliCommandBinding InspectBinding { get; init; }
    public required ICliCommandBinding StatusBinding { get; init; }
    public required ICliCommandBinding InstallBinding { get; init; }
    public required ICliCommandBinding UpdateBinding { get; init; }
    public required ICliCommandBinding RemoveBinding { get; init; }
    public required Func<ToolWorkspaceObservationReader> CreateWorkspaceReader { get; init; }
}
